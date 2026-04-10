using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Gateways.Api.Configurations;
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

builder.Services.Configure<DownstreamApisOptions>(
    builder.Configuration.GetSection(DownstreamApisOptions.SectionName));

builder.Services.AddHttpClient();
builder.Services.AddScoped<INotificador, Notificador>();
builder.Services.AddScoped<ICadastroRepresentanteOrchestrator, CadastroRepresentanteOrchestrator>();
builder.Services.AddScoped<ICadastroClienteOrchestrator, CadastroClienteOrchestrator>();
builder.Services.AddScoped<IProcessarVendaOrchestrator, ProcessarVendaOrchestrator>();

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
