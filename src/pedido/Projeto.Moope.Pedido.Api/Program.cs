using Projeto.Moope.Pedido.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiConfig();
builder.Services.AddAuthConfig(builder.Configuration, builder.Environment);
builder.Services.AddConectionConfig(builder.Configuration);
builder.Services.AddHttpContextAccessor();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
