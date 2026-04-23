using Projeto.Moope.Auth.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiConfig(builder.Configuration);
builder.Services.AddConectionConfig(builder.Configuration);
builder.Services.AddIdentityConfig(builder.Configuration);
builder.Services.AddAuthConfig(builder.Configuration, builder.Environment);
builder.Services.AddSwaggerConfig();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDependencyInjectionConfig(builder.Configuration);

var app = builder.Build();
app.UseSwaggerConfig();
await app.UseSeedDataAsync();
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
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
