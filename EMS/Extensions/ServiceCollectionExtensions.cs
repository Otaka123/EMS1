using Identity.Application.Contracts.Seeds;
using Identity.Domain;
using Identity.Infrastructure.Extensions;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services.PolicyProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Common.infrastructure.DependencyInjection;
using Identity.Infrastructure.DI;
namespace Identity.API.Extensions
  
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EMS API",
                    Version = "v1",
                    Description = "Enterprise Management System API"
                });

                // إضافة فلتر مخصص لو محتاج
                //c.OperationFilter<SwaggerAddLanguageHeader>();
            });

            return services;
        }

        public static IServiceCollection AddIdentityAndAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddIdentityServices(connectionString, options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            });

            services.AddCustomAuthentication(configuration);
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddLocalizationSupport(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCommonServices(configuration);
            services.AddJsonLocalizationServices(
                resourcesPath: null, // مش محتاجين مسار
                baseName: "messages");

            return services;
        }

        //public static async Task SeedIdentityDataAsync(this IServiceProvider services)
        //{
        //    using var scope = services.CreateScope();
        //    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        //    var dbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        //    await DataSeeder.SeedRolesAndPermissionsAsync(roleManager, dbContext);
        //}
    }
}
