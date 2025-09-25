using Common.Application.Contracts.interfaces;
using Common.Application.Contracts.Interfaces;
using Common.infrastructure.Services;
using Common.infrastructure.Services.EventPublisher;
using Common.infrastructure.Services.Logger;
using Common.Infrastructure.Services.Validation;
using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace Common.infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration config)
        {
           

            // FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            
            //services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

            //services.AddSingleton<IGlobalLogger, GlobalLogger>();
            //services.AddSingleton<IExternalLogService, AppInsightsLogService>();

            services.AddTransient(typeof(IAppLogger<>), typeof(AppLogger<>));

            // التسجيل لمزود التسجيل الفعلي (مثل Serilog أو NLog)

            //services.AddScoped<IEventPublisher, EventPublisher>();

            //// Register repositories
            //services.AddScoped<ICourseRepository, CourseRepository>();
            //services.AddScoped<ICategoryRepository, CategoryRepository>();
            //services.AddScoped<IInstructorRepository, InstructorRepository>();

            return services;
        }
       
    }
}
