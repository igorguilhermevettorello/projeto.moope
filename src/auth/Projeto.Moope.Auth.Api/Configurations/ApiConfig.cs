using Microsoft.AspNetCore.Mvc;

namespace Projeto.Moope.Auth.Api.Configurations
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .WithOrigins("https://compre.moope.com.br")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });

                // Política mais permissiva para desenvolvimento
                options.AddPolicy("DevelopmentCorsPolicy", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });

                options.AddPolicy("StagingCorsPolicy", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            //    // Configurar health checks com verificações de banco de dados
            //    services.AddHealthChecks()
            //        .AddDbContextCheck<AppDbContext>("AppDatabase", tags: new[] { "ready", "live" })
            //        .AddDbContextCheck<AppIdentityDbContext>("IdentityDatabase", tags: new[] { "ready", "live" })
            //        .AddCheck("Memory", () =>
            //        {
            //            var process = System.Diagnostics.Process.GetCurrentProcess();
            //            var workingSet = process.WorkingSet64;
            //            // Considera saudável se usar menos de 1GB de memória
            //            return workingSet < 1024 * 1024 * 1024
            //                ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Memory usage is within acceptable limits")
            //                : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("Memory usage is high");
            //        }, tags: new[] { "live" })
            //        .AddCheck("Disk", () =>
            //        {
            //            try
            //            {
            //                var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:");
            //                var freeSpaceGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
            //                var totalSpaceGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
            //                var usedPercentage = ((totalSpaceGB - freeSpaceGB) / totalSpaceGB) * 100;

            //                if (freeSpaceGB > 1 && usedPercentage < 90)
            //                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Disk space is adequate");
            //                else
            //                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("Disk space is low");
            //            }
            //            catch (Exception ex)
            //            {
            //                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Disk check failed", ex);
            //            }
            //        }, tags: new[] { "live" });

            return services;
        }
    }
}
