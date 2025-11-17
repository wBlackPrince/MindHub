using EducationContentService.Core;
using EducationContentService.Core.Endpoints;
using EducationContentService.Infrastructure.Postgres;
using EducationContentService.Web.EndpointSettings;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace EducationContentService.Web.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCore(configuration)
            .AddInfrastructurePostgres(configuration)
            .AddSerialLogging(configuration)
            .AddOpenApiSpec()
            .AddEndpoints(typeof(IEndpoint).Assembly);

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

    private static IServiceCollection AddSerialLogging(
        this IServiceCollection services,
        IConfiguration configuration)
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