namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestsBase: IClassFixture<IntegrationTestsWebFactory>
{
    protected FileServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services = factory.Services;
    }

    protected HttpClient AppHttpClient { get; init; }

    protected HttpClient HttpClient { get; init; }

    protected IServiceProvider Services { get; init; }
}