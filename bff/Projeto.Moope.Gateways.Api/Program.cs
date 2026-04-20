using Projeto.Moope.Api.Utils;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Gateways.Api.Configurations;
using Projeto.Moope.Gateways.Api.Mappings;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Options;
using Projeto.Moope.Gateways.Core.Services;

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

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<VendedorMappingProfile>();
    cfg.AddProfile<ClienteMappingProfile>();
});

builder.Services.Configure<DownstreamApisOptions>(
    builder.Configuration.GetSection(DownstreamApisOptions.SectionName));

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
builder.Services.AddScoped<IProcessarVendaOrchestrator, ProcessarVendaOrchestrator>();
builder.Services.AddScoped<IUser, AspNetUser>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
