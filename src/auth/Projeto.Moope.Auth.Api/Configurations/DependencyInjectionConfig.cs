using Projeto.Moope.Auth.Application.Commands.Usuario.Criar;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Auth.Core.Services;
using Projeto.Moope.Auth.Infrastructure.Repositories;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Auth.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterApplicationDependencies(services, configuration);
            RegisterRepositories(services);
            RegisterMediatR(services);
            RegisterServices(services);
            RegisterValidators(services);
            return services;
        }

        private static void RegisterApplicationDependencies(IServiceCollection service, IConfiguration configuration)
        {
            //service.AddScoped<AppDbContext>();
            service.AddScoped<INotificador, Notificador>();
            //service.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.SectionName));
            service.Configure<SwaggerAuthConfig>(configuration.GetSection("SwaggerAuth"));
            //service.Configure<EmailSettings>(configuration.GetSection("Email"));
            //service.Configure<WhatsAppSettings>(configuration.GetSection("WhatsApp"));
            //service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //service.AddSingleton<ITokenHasher, TokenHasher>();
            //service.AddSingleton<IEncryption, Encryption>();
            //service.AddScoped<IComodatoTokenValidationService, ComodatoTokenValidationService>();
            //service.AddScoped<IUser, AspNetUser>();
            //service.AddScoped<IPasswordGenerator, PasswordGenerator>();
        }

        private static void RegisterRepositories(IServiceCollection service)
        {
            service.AddScoped<IUsuarioRepository, UsuarioRepository>();
            service.AddScoped<IPessoaFisicaRepository, PessoaFisicaRepository>();
            service.AddScoped<IPessoaJuridicaRepository, PessoaJuridicaRepository>();
            service.AddScoped<IPapelRepository, PapelRepository>();
            service.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            // other...
            //service.AddScoped<ITransacaoRepository, TransacaoRepository>();
            //service.AddScoped<IDescontoRepository, DescontoRepository>();
            //service.AddScoped<IEmailRepository, EmailRepository>();
            //service.AddScoped<IComodatoConviteRepository, ComodatoConviteRepository>();
            //service.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void RegisterServices(IServiceCollection service)
        {
            service.AddHttpClient<IGoogleRecaptchaService, GoogleRecaptchaService>();
            service.AddScoped<IIdentityUserService, IdentityUserService>();
        }

        private static void RegisterValidators(IServiceCollection service)
        {
            //service.AddScoped<IValidator<CreatePlanoDto>, PlanoDtoValidator>();
            //service.AddScoped<IValidator<CreateClienteDto>, CreateClienteDtoValidator>();
            //service.AddScoped<IValidator<UpdateClienteDto>, UpdateClienteDtoValidator>();
            //service.AddScoped<IValidator<AlterarSenhaClienteDto>, AlterarSenhaClienteDtoValidator>();
            //service.AddScoped<IValidator<AlterarSenhaAdminDto>, AlterarSenhaAdminDtoValidator>();
            //// service.AddScoped<IValidator<CreateVendaDto>, CreateVendaDtoValidator>();
        }

        private static void RegisterMediatR(IServiceCollection service)
        {
            service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CriarUsuarioCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarVendaCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SalvarEmailCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AtualizarClienteCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AlterarSenhaClienteCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AlterarSenhaAdminCommand).Assembly));
        }
    }
}
