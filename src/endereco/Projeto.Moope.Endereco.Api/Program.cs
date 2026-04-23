using Projeto.Moope.Endereco.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiConfig(builder.Configuration);
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

app.UseAuthorization();

app.MapControllers();

app.Run();
