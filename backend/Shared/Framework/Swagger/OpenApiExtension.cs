using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Framework.Swagger;

public static class OpenApiExtension
{
    public static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
    {
        services.AddOpenApi();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Title = "Education Content Service",
                Version = "v1",
                Description = "Education Content Service",
                Contact = new OpenApiContact()
                {
                    Name = "Eduard",
                    Email = "masteryoda021@gmail.com",
                }
            });
        });

        return services;
    }
}