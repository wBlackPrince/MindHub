using System.Net.Http.Json;
using FileService.Contracts;
using FileService.Core.Features;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;

namespace FileService.IntegrationTests.Features;

public class MultiPartUploadFileTests: FileServiceTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;

    public MultiPartUploadFileTests(IntegrationTestsWebFactory factory): base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async void MultiPartUpload_FullCycle_PersistsMediaFile()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        StartMultiPartUploadRequest request = new StartMultiPartUploadRequest(
            "lesson.mp4",
            "video",
            "video/mp4",
            10000);

        HttpResponseMessage response = await AppHttpClient.PostAsJsonAsync(
            "/api/files/start-multipart-upload",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        Envelope<StartMultiPartUploadResponse>? data = await response.Content
            .ReadFromJsonAsync<Envelope<StartMultiPartUploadResponse>>(cancellationToken);

        
    }
}