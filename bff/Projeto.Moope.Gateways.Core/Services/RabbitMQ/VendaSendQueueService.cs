using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Gateways.Core.DTOs.Venda;
using Projeto.Moope.Gateways.Core.Interfaces.Services.RabbitMQ;
using Projeto.Moope.Gateways.Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Projeto.Moope.Gateways.Core.Services.RabbitMQ
{
    public class VendaSendQueueService : IVendaSendQueueService
    {
        private const string QueueName = "vendas";

        private readonly RabbitMqOptions rabbitMqOptions;
        private readonly ILogger<VendaSendQueueService> logger;

        public VendaSendQueueService(
            IOptions<RabbitMqOptions> rabbitMqOptions,
            ILogger<VendaSendQueueService> logger)
        {
            this.rabbitMqOptions = rabbitMqOptions.Value;
            this.logger = logger;
        }

        public async Task<ResultDto> ExecutarAsync(VendaQueueDto request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ResultDto
                {
                    Status = false,
                    StatusCode = 499,
                    Mensagem = "Operação cancelada."
                };
            }

            try
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

                await using var connection = await factory.CreateConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await EnsureVendaQueueExistsAsync(channel, cancellationToken);

                var payload = JsonSerializer.Serialize(
                    request,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                var body = Encoding.UTF8.GetBytes(payload);

                var properties = new BasicProperties
                {
                    ContentType = MediaTypeNames.Application.Json,
                    DeliveryMode = DeliveryModes.Persistent,
                    MessageId = Guid.NewGuid().ToString("N"),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: QueueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);

                return new ResultDto
                {
                    Status = true,
                    StatusCode = StatusCodes.Status200OK,
                    Mensagem = "Venda enviada para processamento."
                };
            }
            catch (OperationInterruptedException ex)
            {
                logger.LogError(ex, "Falha ao declarar/publicar na fila {QueueName}.", QueueName);

                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensagem = $"Erro ao publicar na fila RabbitMQ '{QueueName}'."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado ao enviar venda para fila {QueueName}.", QueueName);

                return new ResultDto
                {
                    Status = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Mensagem = "Erro inesperado ao enviar venda para processamento."
                };
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
