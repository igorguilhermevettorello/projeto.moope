using Projeto.Moope.Comodato.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiConfig();
builder.Services.AddConectionConfig(builder.Configuration);
builder.Services.Configure<SwaggerAuthConfig>(builder.Configuration.GetSection("SwaggerAuth"));
builder.Services.RegisterServices(builder.Configuration);

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

app.UseAuthorization();

app.MapControllers();

app.Run();
