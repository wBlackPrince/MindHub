using EducationContentService.Core.EndpointsSettings;
using EducationContentService.Core.Features.Lessons;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace EducationContentService.Core.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSerialLogging(configuration)
            .AddOpenApiSpec()
            .AddEndpoints(typeof(Program).Assembly)
            .AddScoped<CreateHandler>();

        return services;
    }

    private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
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

    private static IServiceCollection AddSerialLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "LessonService"));

        return services;
    }
}