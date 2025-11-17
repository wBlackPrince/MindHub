using EducationContentService.Core.Features.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextPool<EducationDbContext>((sp, options) =>
        {
            string? connectionString = configuration.GetConnectionString(Constants.DATABASE);
            IHostEnvironment? hostEnvironment = sp.GetService<IHostEnvironment>();
            ILoggerFactory? loggerFactory = sp.GetService<ILoggerFactory>();

            options.UseNpgsql(connectionString);

            if (hostEnvironment!.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // serilog
            options.UseLoggerFactory(loggerFactory);
        });

        services.AddScoped<ILessonsRepository, LessonsRepository>();

        return services;
    }
}