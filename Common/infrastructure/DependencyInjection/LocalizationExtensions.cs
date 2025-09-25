using Common.Application.Contracts.interfaces;
using Common.infrastructure.Services.Translation;
using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;

namespace Identity.Infrastructure.DI
{

    public static class LocalizationExtensions
        {
            public static IServiceCollection AddJsonLocalizationServices(
                this IServiceCollection services,
                string resourcesPath = "Resources",
                string baseName = "messages")
            {
                services.AddSingleton<JsonTranslationService>();
                services.AddSingleton<ISharedMessageLocalizer>(provider =>
                    provider.GetRequiredService<JsonTranslationService>());
                services.AddSingleton<IStringLocalizer>(provider =>
                    provider.GetRequiredService<JsonTranslationService>());

                services.AddLocalization(options =>
                {
                    options.ResourcesPath = resourcesPath;
                });

                return services;
            }

            public static IApplicationBuilder UseJsonLocalization(
                this IApplicationBuilder app,
                string defaultCulture = "en",
                IEnumerable<string>? supportedCultures = null)
            {
                // تحويل IEnumerable<string> إلى CultureInfo[]
                var cultures = (supportedCultures ?? new[] { "en", "ar" })
                    .Select(c => new CultureInfo(c))
                    .ToArray();

                var options = new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(defaultCulture),
                    SupportedCultures = cultures,
                    SupportedUICultures = cultures,
                    RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                }
                };

                return app.UseRequestLocalization(options);
            }
        
    }
}
