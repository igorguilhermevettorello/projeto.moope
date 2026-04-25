using Microsoft.Extensions.Options;
using Projeto.Moope.RabbitMQ.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace Projeto.Moope.RabbitMQ.Worker
{
    public class Worker : BackgroundService
    {
        private const string QueueName = "vendas";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqOptions rabbitMqOptions;
        private readonly IEfetuarPagamentoService efetuarPagamentoService;
        private readonly IAuthClientTokenService authClientTokenService;
        private readonly IPedidoValoresPagamentoQueryService pedidoValoresPagamentoQueryService;

        public Worker(
            ILogger<Worker> logger,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IEfetuarPagamentoService efetuarPagamentoService,
            IAuthClientTokenService authClientTokenService,
            IPedidoValoresPagamentoQueryService pedidoValoresPagamentoQueryService)
        {
            _logger = logger;
            this.rabbitMqOptions = rabbitMqOptions.Value;
            this.efetuarPagamentoService = efetuarPagamentoService;
            this.authClientTokenService = authClientTokenService;
            this.pedidoValoresPagamentoQueryService = pedidoValoresPagamentoQueryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunConsumerLoopAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha no loop do consumidor RabbitMQ. Tentando novamente em {Seconds}s.", rabbitMqOptions.RetryIntervalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, rabbitMqOptions.RetryIntervalSeconds)), stoppingToken);
                }
            }
        }

        private async Task RunConsumerLoopAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqOptions.HostName,
                Port = rabbitMqOptions.Port,
                UserName = rabbitMqOptions.UserName,
                Password = rabbitMqOptions.Password,
                VirtualHost = rabbitMqOptions.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(rabbitMqOptions.ConnectionTimeout),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await EnsureVendaQueueExistsAsync(channel, stoppingToken);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                await HandleMessageAsync(channel, ea, stoppingToken);
            };

            await channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            _logger.LogInformation("Consumidor RabbitMQ conectado. Queue: {QueueName}. Host: {Host}:{Port}", QueueName, rabbitMqOptions.HostName, rabbitMqOptions.Port);

            // Mantém a conexão/canal vivos até cancelamento.
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken stoppingToken)
        {
            var body = ea.Body.ToArray();
            var raw = Encoding.UTF8.GetString(body);

            try
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    _logger.LogWarning("Mensagem vazia recebida. DeliveryTag: {DeliveryTag}", ea.DeliveryTag);
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                var dto = JsonSerializer.Deserialize<PagamentoDto>(raw, JsonOptions);
                if (dto is null)
                {
                    _logger.LogWarning("Falha ao desserializar mensagem. DeliveryTag: {DeliveryTag}. Body: {Body}", ea.DeliveryTag, raw);
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    return;
                }

                var attempts = Math.Max(1, rabbitMqOptions.MaxRetryAttempts);
                for (var attempt = 1; attempt <= attempts; attempt++)
                {
                    var tokenRs = await authClientTokenService.GetClientAccessTokenAsync(stoppingToken);
                    if (!tokenRs.Status || tokenRs.Dados == null || string.IsNullOrWhiteSpace(tokenRs.Dados.AccessToken))
                    {
                        _logger.LogWarning(
                            "Falha ao obter access token do Auth (tentativa {Attempt}/{Attempts}). PedidoId: {PedidoId}. StatusCode: {StatusCode}. Msg: {Mensagem}",
                            attempt, attempts, dto.PedidoId, tokenRs.StatusCode, tokenRs.Mensagem);

                        if (attempt < attempts)
                            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, rabbitMqOptions.RetryIntervalSeconds)), stoppingToken);

                        continue;
                    }

                    var authorizationHeader = $"Bearer {tokenRs.Dados.AccessToken}";

                    var valoresRs = await pedidoValoresPagamentoQueryService.ObterValoresPagamentoAsync(dto.PedidoId, authorizationHeader, stoppingToken);
                    if (!valoresRs.Status || valoresRs.Dados == null)
                    {
                        _logger.LogWarning(
                            "Falha ao obter valores do Pedido (tentativa {Attempt}/{Attempts}). PedidoId: {PedidoId}. StatusCode: {StatusCode}. Msg: {Mensagem}",
                            attempt, attempts, dto.PedidoId, valoresRs.StatusCode, valoresRs.Mensagem);

                        if (attempt < attempts)
                            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, rabbitMqOptions.RetryIntervalSeconds)), stoppingToken);

                        continue;
                    }

                    dto = new PagamentoDto
                    {
                        Name = dto.Name,
                        Email = dto.Email,
                        PedidoId = dto.PedidoId,
                        Valor = valoresRs.Dados.ValorTotalMensalidade,
                        TaxaAdesao = valoresRs.Dados.ValorTotalTaxaAdesao,
                        Periodicidade = dto.Periodicidade,
                        MetodoPagamento = dto.MetodoPagamento,
                        GalaxPayCustomerId = dto.GalaxPayCustomerId,
                        GalaxPayCardId = dto.GalaxPayCardId,
                        Observacao = dto.Observacao
                    };

                    var rs = await efetuarPagamentoService.EfetuarPagamento(dto, authorizationHeader: authorizationHeader, cancellationToken: stoppingToken);
                    if (rs.Status)
                    {
                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        _logger.LogInformation("Pagamento processado com sucesso. PedidoId: {PedidoId}. DeliveryTag: {DeliveryTag}", dto.PedidoId, ea.DeliveryTag);
                        return;
                    }

                    _logger.LogWarning(
                        "Falha ao processar pagamento (tentativa {Attempt}/{Attempts}). PedidoId: {PedidoId}. StatusCode: {StatusCode}. Msg: {Mensagem}",
                        attempt, attempts, dto.PedidoId, rs.StatusCode, rs.Mensagem);

                    if (attempt < attempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, rabbitMqOptions.RetryIntervalSeconds)), stoppingToken);
                    }
                }

                await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Evita perder a mensagem em shutdown: requeue.
                try
                {
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: CancellationToken.None);
                }
                catch
                {
                    // Ignora para não mascarar o cancelamento.
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem. DeliveryTag: {DeliveryTag}. Body: {Body}", ea.DeliveryTag, raw);
                await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        }

        private static async Task EnsureVendaQueueExistsAsync(IChannel channel, CancellationToken cancellationToken)
        {
            try
            {
                await channel.QueueDeclarePassiveAsync(queue: QueueName, cancellationToken: cancellationToken);
            }
            catch (OperationInterruptedException)
            {
                var arguments = new Dictionary<string, object?>
                {
                    ["x-queue-type"] = "stream"
                };

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: arguments,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
