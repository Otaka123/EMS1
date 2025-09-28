using Identity.Infrastructure.DI;
using Microsoft.OpenApi.Models;
namespace Identity.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EMS API V1");
                c.RoutePrefix = string.Empty; // الرابط الرئيسي
                c.ConfigObject.AdditionalItems.Remove("auth");

            });
            return app;
        }
        public static IApplicationBuilder UseLocalizationSupport(this IApplicationBuilder app)
        {
            app.UseJsonLocalization(
                defaultCulture: "en",
                supportedCultures: new[] { "en", "ar" });
            return app;
        }
    
}
}
