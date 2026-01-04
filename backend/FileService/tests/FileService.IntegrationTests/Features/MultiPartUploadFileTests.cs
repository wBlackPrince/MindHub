using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.HttpCommunication;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;
using CompleteMultipartUploadRequest = FileService.Contracts.CompleteMultipartUploadRequest;

namespace FileService.IntegrationTests.Features;

public class MultiPartUploadFileTests: FileServiceTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;

    public MultiPartUploadFileTests(IntegrationTestsWebFactory factory): base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MultiPartUpload_FullCycle_PersistsMediaFile()
    {
        // arange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // act
        StartMultipartUploadResponse startMultipartUploadResponse = await StartMultiPartUpload(
            fileInfo,
            cancellationToken);

        IReadOnlyList<PartETagDto> partETagDtos = await UploadChunks(
            fileInfo,
            startMultipartUploadResponse,
            cancellationToken);

        UnitResult<Error> completeMultipartUploadResult = await CompleteMultiPartUpload(
            startMultipartUploadResponse,
            partETagDtos,
            cancellationToken);

        // assert
        Assert.True(completeMultipartUploadResult.IsSuccess);
        await ExecuteInDb(async dbContext =>
        {
            MediaAsset? mediaAsset = await dbContext.MediaAssets.FirstOrDefaultAsync(
                m => m.Id == startMultipartUploadResponse.MediaAssetId,
                cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.UPLOADED, mediaAsset.Status);


            IAmazonS3 amazonS3Client = Services.GetRequiredService<IAmazonS3>();

            GetObjectResponse objectResponse = await amazonS3Client.GetObjectAsync(
                mediaAsset.Key.Location,
                mediaAsset.Key.Value,
                cancellationToken);

            Console.WriteLine(objectResponse.ContentLength);
        });
    }

    private async Task<StartMultipartUploadResponse> StartMultiPartUpload(
        FileInfo fileInfo,
        CancellationToken cancellationToken)
    {
        // Arrange
        StartMultipartUploadRequest request = new StartMultipartUploadRequest(
            fileInfo.Name,
            "video",
            "video/mp4",
            fileInfo.Length);

        // Act
        HttpResponseMessage startMultipartUploadResponse = await AppHttpClient.PostAsJsonAsync(
            "/api/files/start-multipart-upload",
            request,
            cancellationToken);

        Result<StartMultipartUploadResponse, Error> startMultipartUploadResult = await startMultipartUploadResponse
            .HandleResponseAsync<StartMultipartUploadResponse>(cancellationToken);

        // Assert
        Assert.True(startMultipartUploadResult.IsSuccess);
        Assert.NotNull(startMultipartUploadResult.Value.UploadId);


        await ExecuteInDb(async dbContext =>
        {
            MediaAsset? mediaAsset = await dbContext.MediaAssets.FirstOrDefaultAsync(
                m => m.Id == startMultipartUploadResult.Value.MediaAssetId,
                cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.UPLOADING, mediaAsset.Status);
        });

        return startMultipartUploadResult.Value;
    }


    private async Task<IReadOnlyList<PartETagDto>> UploadChunks(
        FileInfo fileInfo,
        StartMultipartUploadResponse startMultiPartUploadResponse,
        CancellationToken cancellationToken)
    {
        await using var stream = fileInfo.OpenRead();

        List<PartETagDto> parts = new List<PartETagDto>();

        foreach (ChunkUploadUrl chunkUploadUrl in startMultiPartUploadResponse.ChunkUploadUrls.OrderBy(c => c.PartNumber))
        {
            byte[] chunk = new byte[startMultiPartUploadResponse.ChunkSize];

            // файл читается по кусочкам начиная каждый раз с того места где остановилось предыдущее чтение
            int bytesRead = await stream.ReadAsync(
                chunk.AsMemory(0, startMultiPartUploadResponse.ChunkSize),
                cancellationToken);

            if (bytesRead == 0)
                break;

            ByteArrayContent content = new ByteArrayContent(chunk);

            HttpResponseMessage response = await HttpClient.PutAsync(
                chunkUploadUrl.UploadUrl,
                content,
                cancellationToken);

            string? partETag = response.Headers.ETag?.Tag.Trim('"');
            PartETagDto eTagDto = new PartETagDto(chunkUploadUrl.PartNumber, partETag!);
            parts.Add(eTagDto);
        }

        return parts;
    }

    private async Task<UnitResult<Error>> CompleteMultiPartUpload(
        StartMultipartUploadResponse startMultiPartUploadResponse,
        IEnumerable<PartETagDto> partETagDtos,
        CancellationToken cancellationToken)
    {
        CompleteMultipartUploadRequest completeMultiPartUploadRequest = new CompleteMultipartUploadRequest(
            startMultiPartUploadResponse.MediaAssetId,
            startMultiPartUploadResponse.UploadId,
            partETagDtos.ToList());

        HttpResponseMessage completeMultipartResponse = await AppHttpClient.PostAsJsonAsync(
            "/api/files/complete-upload",
            completeMultiPartUploadRequest,
            cancellationToken);

        UnitResult<Error> completeMultipartUploadResult =
            await completeMultipartResponse.HandleResponseAsync(cancellationToken);

        return completeMultipartUploadResult;
    }
}