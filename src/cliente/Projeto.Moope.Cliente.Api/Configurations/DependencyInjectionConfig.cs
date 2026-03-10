using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Cliente.Api.Configurations
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
            //service.Configure<JwtSettings>(configuration.GetSection("Jwt"));
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
            //service.AddScoped<IPlanoRepository, PlanoRepository>();
            //service.AddScoped<IClienteRepository, ClienteRepository>();
            //service.AddScoped<IVendedorRepository, VendedorRepository>();
            //service.AddScoped<IEnderecoRepository, EnderecoRepository>();
            //service.AddScoped<IUsuarioRepository, UsuarioRepository>();
            //service.AddScoped<IPessoaFisicaRepository, PessoaFisicaRepository>();
            //service.AddScoped<IPessoaJuridicaRepository, PessoaJuridicaRepository>();
            //service.AddScoped<IPapelRepository, PapelRepository>();
            //service.AddScoped<IPedidoRepository, PedidoRepository>();
            //service.AddScoped<ITransacaoRepository, TransacaoRepository>();
            //service.AddScoped<IDescontoRepository, DescontoRepository>();
            //service.AddScoped<IEmailRepository, EmailRepository>();
            //service.AddScoped<IComodatoConviteRepository, ComodatoConviteRepository>();
            //service.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void RegisterServices(IServiceCollection service)
        {
            //service.AddScoped<IPlanoService, PlanoService>();
            //service.AddScoped<IPapelService, PapelService>();
            //service.AddScoped<IClienteService, ClienteService>();
            //service.AddScoped<IVendedorService, VendedorService>();
            //service.AddScoped<IEnderecoService, EnderecoService>();
            //service.AddScoped<IUsuarioService, UsuarioService>();
            //service.AddScoped<IIdentityUserService, IdentityUserService>();
            //service.AddScoped<IVendaService, VendaService>();
            //service.AddScoped<IComodatoConviteService, ComodatoConviteService>();
            ////service.AddScoped<IComodatoService, ComodatoService>();
            //service.AddScoped<IDescontoService, DescontoService>();
            //service.AddScoped<IEmailService, EmailService>();
            //service.AddScoped<IWhatsAppService, WhatsAppService>();
            //service.AddHttpClient<IEmailGateway, EmailGateway>();
            //service.AddHttpClient<IGoogleRecaptchaService, GoogleRecaptchaService>();
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
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarVendaCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SalvarEmailCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AtualizarClienteCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AlterarSenhaClienteCommand).Assembly));
            //service.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AlterarSenhaAdminCommand).Assembly));
        }
    }
}
