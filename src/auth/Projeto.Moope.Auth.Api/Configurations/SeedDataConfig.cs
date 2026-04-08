using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Auth.Infrastructure.Data;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Auth.Api.Configurations
{
    public static class SeedDataConfig
    {
        public static async Task UseSeedDataAsync(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;

            var context = serviceProvider.GetRequiredService<AppAuthContext>();
            var contextIdentity = serviceProvider.GetRequiredService<AppIdentityDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            //await context.Database.MigrateAsync();
            //await contextIdentity.Database.MigrateAsync();
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager, context, configuration);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            foreach (var roleName in Enum.GetNames(typeof(TipoUsuario)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }
        }

        private static async Task SeedUsersAsync(
            UserManager<IdentityUser<Guid>> userManager,
            AppAuthContext context,
            IConfiguration configuration)
        {
            var emailAdmin = "admin@moope.com.br";
            if (await userManager.FindByEmailAsync(emailAdmin) != null)
            {
                return;
            }

            var user = new IdentityUser<Guid>
            {
                UserName = emailAdmin,
                Email = emailAdmin,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var adminPassword = configuration["Admin:Password"] ?? "Admin@123";
            var result = await userManager.CreateAsync(user, adminPassword);

            if (!result.Succeeded)
            {
                return;
            }

            await userManager.AddToRoleAsync(user, TipoUsuario.Administrador.ToString());

            var novoUsuario = new Usuario
            {
                Nome = "Administrador do Sistema",
                TipoUsuario = TipoUsuario.Administrador,
                Id = user.Id,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            await context.Usuarios.AddAsync(novoUsuario);
            await context.SaveChangesAsync();
        }
    }
}
