using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;

namespace Identity.API.Filters
{
    public class SwaggerAddLanguageHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // إزالة أي معلمة لغة موجودة مسبقاً
            operation.Parameters = operation.Parameters
                .Where(p => p.Name != "Accept-Language")
                .ToList();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "حدد اللغة: ar للعربية, en للإنجليزية",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("en"),
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("en"),
                        new OpenApiString("ar")
                    }
                },
                Style = ParameterStyle.Simple
            });
        }
    }
}
