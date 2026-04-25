using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;

namespace Projeto.Moope.RabbitMQ.Worker.Workers
{
    public sealed class ClientesPendentesSyncWorker : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly ILogger<ClientesPendentesSyncWorker> logger;
        private readonly IAuthClientTokenService authClientTokenService;
        private readonly IClientesPendentesQueryService clientesPendentesQueryService;
        private readonly IClienteProvisioningService clienteProvisioningService;

        private string? accessToken;
        private DateTimeOffset accessTokenExpiresAt;

        public ClientesPendentesSyncWorker(
            ILogger<ClientesPendentesSyncWorker> logger,
            IAuthClientTokenService authClientTokenService,
            IClientesPendentesQueryService clientesPendentesQueryService,
            IClienteProvisioningService clienteProvisioningService)
        {
            this.logger = logger;
            this.authClientTokenService = authClientTokenService;
            this.clientesPendentesQueryService = clientesPendentesQueryService;
            this.clienteProvisioningService = clienteProvisioningService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            logger.LogInformation("ClientesPendentesSyncWorker iniciado. Intervalo: {Interval}.", Interval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunOnceAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown
            }
            finally
            {
                logger.LogInformation("ClientesPendentesSyncWorker finalizado.");
            }
        }

        private async Task RunOnceAsync(CancellationToken stoppingToken)
        {
            var startedAt = DateTimeOffset.UtcNow;

            var tokenResult = await GetOrRefreshTokenAsync(stoppingToken);
            if (!tokenResult.Status)
            {
                logger.LogWarning("Sync clientes pendentes abortado: falha ao obter token. StatusCode: {StatusCode}. Msg: {Msg}",
                    tokenResult.StatusCode, tokenResult.Mensagem);
                return;
            }

            var authorizationHeader = $"Bearer {accessToken}";
            var pendentesResult = await clientesPendentesQueryService.ListarClientesPendentesAsync(authorizationHeader, stoppingToken);
            if (!pendentesResult.Status || pendentesResult.Dados == null)
            {
                logger.LogWarning("Falha ao buscar clientes pendentes. StatusCode: {StatusCode}. Msg: {Msg}",
                    pendentesResult.StatusCode, pendentesResult.Mensagem);
                return;
            }

            var pendentes = pendentesResult.Dados;
            var total = pendentes.Count;
            var created = 0;
            var skipped = 0;
            var failed = 0;

            foreach (var p in pendentes)
            {
                if (p.Id == Guid.Empty)
                {
                    skipped++;
                    continue;
                }

                var rs = await clienteProvisioningService.CriarClienteSeNaoExistirAsync(
                    usuarioId: p.Id,
                    email: p.Email,
                    authorizationHeader: authorizationHeader,
                    cancellationToken: stoppingToken);

                if (rs.Status)
                {
                    if (rs.StatusCode == 409)
                        skipped++;
                    else
                        created++;

                    continue;
                }

                failed++;
                logger.LogWarning("Falha ao criar cliente. UsuarioId: {UsuarioId}. Email: {Email}. StatusCode: {StatusCode}. Msg: {Msg}",
                    p.Id, p.Email, rs.StatusCode, rs.Mensagem);
            }

            var elapsedMs = (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;
            logger.LogInformation(
                "Sync clientes pendentes concluído. Total: {Total}. Criados: {Created}. Pulados: {Skipped}. Falhas: {Failed}. ElapsedMs: {ElapsedMs}",
                total, created, skipped, failed, (int)elapsedMs);
        }

        private async Task<Projeto.Moope.Core.DTOs.ResultDto> GetOrRefreshTokenAsync(CancellationToken stoppingToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken) && accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return new Projeto.Moope.Core.DTOs.ResultDto { Status = true, StatusCode = 200, Mensagem = null };
            }

            var rs = await authClientTokenService.GetClientAccessTokenAsync(stoppingToken);
            if (!rs.Status || rs.Dados == null)
            {
                return new Projeto.Moope.Core.DTOs.ResultDto
                {
                    Status = false,
                    StatusCode = rs.StatusCode,
                    Mensagem = rs.Mensagem
                };
            }

            accessToken = rs.Dados.AccessToken;
            var expiresInSeconds = Math.Max(1, rs.Dados.ExpiresIn);
            accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);

            return new Projeto.Moope.Core.DTOs.ResultDto { Status = true, StatusCode = 200, Mensagem = null };
        }
    }
}

