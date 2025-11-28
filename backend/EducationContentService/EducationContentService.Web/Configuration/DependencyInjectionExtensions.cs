using EducationContentService.Core;
using EducationContentService.Infrastructure.Postgres;
using Framework.Endpoints;
using Framework.Logging;
using Framework.Swagger;

namespace EducationContentService.Web.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCore(configuration)
            .AddInfrastructurePostgres(configuration)
            .AddSerialLogging(configuration, "EducationContentService")
            .AddOpenApiSpec()
            .AddEndpoints(typeof(DependencyInjectionCoreExtensions).Assembly);

        return services;
    }
}