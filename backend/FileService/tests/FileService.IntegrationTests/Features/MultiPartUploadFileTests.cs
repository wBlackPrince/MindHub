using System.Net.Http.Json;
using FileService.Core.Features;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Features;

public class MultiPartUploadFileTests: IClassFixture<IntegrationTestsWebFactory>
{
    private readonly IntegrationTestsWebFactory _factory;

    public MultiPartUploadFileTests(IntegrationTestsWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void MultiPartUpload_FullCycle_PersistsMediaFile()
    {
        HttpClient httpClient = _factory.CreateClient();

        httpClient.PostAsJsonAsync();
    }
}