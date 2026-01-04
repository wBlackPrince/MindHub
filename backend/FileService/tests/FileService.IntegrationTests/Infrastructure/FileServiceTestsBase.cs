using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestsBase: IClassFixture<IntegrationTestsWebFactory>
{
    public const string TEST_FILE_NAME = "test-file.mp4";

    protected FileServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services = factory.Services;
    }

    protected HttpClient AppHttpClient { get; init; }

    protected HttpClient HttpClient { get; init; }

    protected IServiceProvider Services { get; init; }

    protected async Task ExecuteInDb(Func<FilesServiceDbContext, Task> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        FilesServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FilesServiceDbContext>();

        await action(dbContext);
    }
}