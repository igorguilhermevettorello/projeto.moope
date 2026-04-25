using MediatR;
using Projeto.Moope.Api.Utils;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Gateways.Api.Configurations;
using Projeto.Moope.Gateways.Api.Mappings;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente;
using Projeto.Moope.Gateways.Core.Interfaces.Services.GalaxPay;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Pagemento;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Pedido;
using Projeto.Moope.Gateways.Core.Interfaces.Services.RabbitMQ;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor;
using Projeto.Moope.Gateways.Core.Options;
using Projeto.Moope.Gateways.Core.Services;
using Projeto.Moope.Gateways.Core.Services.Cliente;
using Projeto.Moope.Gateways.Core.Services.GalaxPay;
using Projeto.Moope.Gateways.Core.Services.Pagamento;
using Projeto.Moope.Gateways.Core.Services.Pedido;
using Projeto.Moope.Gateways.Core.Services.RabbitMQ;
using Projeto.Moope.Gateways.Core.Services.Vendedor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiConfig();
builder.Services.AddAuthConfig(builder.Configuration, builder.Environment);
builder.Services.Configure<SwaggerAuthConfig>(builder.Configuration.GetSection("SwaggerAuth"));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<VendedorMappingProfile>();
    cfg.AddProfile<ClienteMappingProfile>();
    cfg.AddProfile<VendaMappingProfile>();
});

builder.Services.Configure<DownstreamApisOptions>(
    builder.Configuration.GetSection(DownstreamApisOptions.SectionName));
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<INotificador, Notificador>();
builder.Services.AddScoped<IClienteCreateService, ClienteCreateService>();
builder.Services.AddScoped<IClienteGetByIdService, ClienteGetByIdService>();
builder.Services.AddScoped<IClienteUpdateService, ClienteUpdateService>();
builder.Services.AddScoped<IClienteDeleteService, ClienteDeleteService>();
builder.Services.AddScoped<IVendedorCreateService, VendedorCreateService>();
builder.Services.AddScoped<IVendedorGetByIdService, VendedorGetByIdService>();
builder.Services.AddScoped<IVendedorUpdateService, VendedorUpdateService>();
builder.Services.AddScoped<IProcessarVendaService, ProcessarVendaService>();
builder.Services.AddScoped<IVendedorGetByCupom, VendedorGetByCupom>();
builder.Services.AddScoped<IVendaSendQueueService, VendaSendQueueService>();
builder.Services.AddScoped<IPlanoGetById, PlanoGetById>();
builder.Services.AddScoped<IAuthClientLoginService, AuthClientLoginService>();
builder.Services.AddScoped<IClienteGalaxPayUpdateService, ClienteGalaxPayUpdateService>();
builder.Services.AddScoped<IClienteGalaxPayCreateService, ClienteGalaxPayCreateService>();
builder.Services.AddScoped<ICartaoGalaxPayCreateService, CartaoGalaxPayCreateService>();
builder.Services.AddScoped<IPedidoCreateService, PedidoCreateService>();
builder.Services.AddScoped<IPagamentoIntencaoGetByIdService, PagamentoIntencaoGetByIdService>();
builder.Services.AddScoped<IUser, AspNetUser>();

var app = builder.Build();

app.UseSwaggerConfig();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors("CorsDevelopmentPolicy");
}
else if (app.Environment.IsStaging())
{
    app.UseCors("CorsStagingPolicy");
}
else if (app.Environment.IsProduction())
{
    app.UseCors("CorsProductionPolicy");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
