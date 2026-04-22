using Projeto.Moope.RabbitMQ.Worker;
using Projeto.Moope.RabbitMQ.Core.Interfaces.Services;
using Projeto.Moope.RabbitMQ.Core.Options;
using Projeto.Moope.RabbitMQ.Core.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<DownstreamApisOptions>(
    builder.Configuration.GetSection(DownstreamApisOptions.SectionName));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IEfetuarPagamentoService, EfetuarPagamentoService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
