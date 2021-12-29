using Microsoft.Extensions.DependencyInjection;
using NSwag;
using System.Linq;

namespace MarketingBox.AuthApi
{
    public static class StartupUtils
    {
        public static void SetupSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerDocument(o =>
            {
                o.Title = "Auth API";
                o.GenerateEnumMappingDescription = true;

                //o.AddSecurity("Bearer", Enumerable.Empty<string>(),
                //    new OpenApiSecurityScheme
                //    {
                //        Type = OpenApiSecuritySchemeType.ApiKey,
                //        Description = "Bearer Token",
                //        In = OpenApiSecurityApiKeyLocation.Header,
                //        Name = "Authorization"
                //    });
            });
        }
    }
}
