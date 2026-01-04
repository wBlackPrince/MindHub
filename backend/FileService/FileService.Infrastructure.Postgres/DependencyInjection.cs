using FileService.Core;
using FileService.Core.FilesStorage;
using FileService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IMediaAssetsRepository, MediaAssetsRepository>();

        services.AddDbContextPool<FilesServiceDbContext>((sp, options) =>
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

        services.AddDbContextPool<IReadDbContext, FilesServiceDbContext>((sp, options) =>
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

        return services;
    }
}