using Microsoft.Extensions.Options;
using Projeto.Moope.RabbitMQ.Worker;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;
using Projeto.Moope.RabbitMQ.Core.Services;
using Projeto.Moope.RabbitMQ.Worker.Workers;

var environmentName =
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environments.Production;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    EnvironmentName = environmentName,
});

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<DownstreamApisOptions>(
    builder.Configuration.GetSection(DownstreamApisOptions.SectionName));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IEfetuarPagamentoService, EfetuarPagamentoService>();
builder.Services.AddSingleton<IAuthClientTokenService, AuthClientTokenService>();
builder.Services.AddSingleton<IPedidoValoresPagamentoQueryService, PedidoValoresPagamentoQueryService>();
builder.Services.AddSingleton<IClientesPendentesQueryService, ClientesPendentesQueryService>();
builder.Services.AddSingleton<IClienteProvisioningService, ClienteProvisioningService>();

builder.Services.AddHostedService<Worker>();
// builder.Services.AddHostedService<ClientesPendentesSyncWorker>();

var host = builder.Build();

var downstreamApis = host.Services.GetRequiredService<IOptions<DownstreamApisOptions>>().Value;
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(
    "Ambiente: {Environment}. Arquivo esperado: appsettings.{Environment}.json. DownstreamApis:Auth configurado: {AuthConfigured} ({AuthUrl})",
    host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName,
    host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName,
    !string.IsNullOrWhiteSpace(downstreamApis.Auth),
    string.IsNullOrWhiteSpace(downstreamApis.Auth) ? "(vazio)" : downstreamApis.Auth);

host.Run();
