using Identity.Infrastructure.DI;
namespace Identity.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {

            app.UseSwagger();
            app.UseSwaggerUI();
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
