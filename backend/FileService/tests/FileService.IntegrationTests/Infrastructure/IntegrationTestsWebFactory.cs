using Amazon.S3;
using FileService.Core;
using FileService.Core.FilesStorage;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using FileService.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("file_service_db_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MinioContainer _minioContainer = new MinioBuilder()
        .WithImage("minio/minio")
        .WithUsername("minioadmin")
        .WithPassword("minioadmin")
        .Build();


    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _minioContainer.StartAsync();

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        FilesServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FilesServiceDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        await _minioContainer.StopAsync();
        await _minioContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appSettings.Tests.json"), optional: true);

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString(),
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // postgres
            services.RemoveAll<FilesServiceDbContext>();
            services.RemoveAll<IReadDbContext>();

            services.AddDbContextPool<FilesServiceDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.AddDbContextPool<IReadDbContext, FilesServiceDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });


            // s3
            services.RemoveAll<IAmazonS3>();

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;

                var port = _minioContainer.GetMappedPublicPort(9000);

                var config = new AmazonS3Config()
                {
                    ServiceURL = $"http://{_minioContainer.Hostname}:{port}",
                    UseHttp = true,
                    ForcePathStyle = true
                };

                return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
            });
        });
    }
}