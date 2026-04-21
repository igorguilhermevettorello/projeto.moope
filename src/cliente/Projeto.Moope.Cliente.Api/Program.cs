using Projeto.Moope.Cliente.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiConfig();
builder.Services.AddAuthConfig(builder.Configuration, builder.Environment);
builder.Services.AddConectionConfig(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<AnonymousEndpointKeysSettings>(
    builder.Configuration.GetSection(AnonymousEndpointKeysSettings.SectionPath));
DependencyInjectionConfig.RegisterServices(builder.Services, builder.Configuration);

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
