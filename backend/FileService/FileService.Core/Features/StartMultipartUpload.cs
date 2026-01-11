using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.Assets;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public sealed class StartMultipartUpload: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/start-multipart-upload", async Task<EndpointResult<StartMultipartUploadResponse>> (
            [FromBody] StartMultipartUploadRequest request,
            [FromServices] StartMultiPartUploadHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken));
    }
}


public sealed class StartMultiPartUploadHandler(
    ILogger<StartMultiPartUploadHandler> logger,
    IChunkSizeCalculator chunkSizeCalculator,
    IMediaAssetsRepository mediaAssetsRepository,
    IS3Provider s3Provider)
{
    public async Task<Result<StartMultipartUploadResponse, Error>> Handle(
        StartMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        // валидация

        Result<FileName, Error> fileNameResult = FileName.Create(request.FileName);

        if (fileNameResult.IsFailure)
            return fileNameResult.Error;


        Result<ContentType, Error> contentTypeResult = ContentType.Create(request.ContentType);

        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error;


        // посчитать количество чанков для загрузки файла

        Result<(int ChunkSize, int TotalChunks), Error> chunkCalculationResult = chunkSizeCalculator
            .CalculateChunkSize(request.Size);

        if(chunkCalculationResult.IsFailure)
            return chunkCalculationResult.Error;

        Result<MediaData, Error> mediaDataResult = MediaData.Create(
            fileNameResult.Value,
            contentTypeResult.Value,
            request.Size,
            chunkCalculationResult.Value.TotalChunks);

        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error;

        Result<MediaAsset, Error> mediaAssetResult = MediaAsset.CreateForUpload(
            mediaDataResult.Value,
            request.AssetType.ToAssetType());

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;



        await mediaAssetsRepository.AddAsync(mediaAssetResult.Value, cancellationToken);


        // начать multipart-загрузку

        Result<string, Error> startUploadResult = await s3Provider.StartMultipartUploadAsync(
            mediaAssetResult.Value.Key,
            mediaAssetResult.Value.MediaData,
            cancellationToken);

        if (startUploadResult.IsFailure)
            return startUploadResult.Error;


        // сгенерировать коллекцию upload-url для чанков

        Result<IReadOnlyList<ChunkUploadUrl>, Error> chunkUploadUrlsResult = await s3Provider.GenerateAllChunksUploadUrlsAsync(
            mediaAssetResult.Value.Key,
            startUploadResult.Value,
            chunkCalculationResult.Value.TotalChunks,
            cancellationToken);

        if (chunkUploadUrlsResult.IsFailure)
            return chunkUploadUrlsResult.Error;

        logger.LogInformation(
            "Media asset started uploading: {MediaAssetId} with key {StorageKey}",
            mediaAssetResult.Value.Id,
            mediaAssetResult.Value.Key);

        // вернуть данные mediaasset (id), uploadId, коллекцию ссылок для загрузки чанков, размер чанка
        return new StartMultipartUploadResponse(
            mediaAssetResult.Value.Id,
            startUploadResult.Value,
            chunkUploadUrlsResult.Value,
            chunkCalculationResult.Value.ChunkSize);
    }
}