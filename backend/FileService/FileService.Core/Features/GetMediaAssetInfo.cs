using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.Assets;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public sealed class GetMediaAsset: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{mediaAssetId:guid}", async Task<EndpointResult<GetMediaAssetDto?>> (
            [FromRoute] Guid mediaAssetId,
            [FromServices] GetMediaAssetHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(mediaAssetId, cancellationToken));
    }
}

public sealed class GetMediaAssetHandler(
    IReadDbContext readDbContext,
    IFileStorageProvider fileStorageProvider,
    ILogger<StartMultiPartUploadHandler> logger)
{
    public async Task<Result<GetMediaAssetDto?, Error>> Handle(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        MediaAsset? mediaAsset = await readDbContext.MediaAssetsQuery
            .FirstOrDefaultAsync(m => m.Id == mediaAssetId, cancellationToken);

        if (mediaAsset is null)
            return Result.Success<GetMediaAssetDto?, Error>(null);


        string? url = null;

        if (mediaAsset.Status == MediaStatus.READY)
        {
            (_, bool isFailure, string presignedUrl, Error? error) = await fileStorageProvider.GenerateDownloadUrlAsync(mediaAsset.Key, cancellationToken);

            if (isFailure)
                return error;

            url = presignedUrl;
        }

        GetMediaAssetDto response = new GetMediaAssetDto(
            mediaAsset.Id,
            mediaAsset.Status.ToString().ToLowerInvariant(),
            mediaAsset.AssetType.ToString().ToLowerInvariant(),
            url,
            mediaAsset.MediaData.Size,
            mediaAsset.MediaData.FileName.Value,
            mediaAsset.MediaData.ContentType.Value);

        return response;
    }
}