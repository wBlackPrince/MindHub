using CSharpFunctionalExtensions;
using FileService.Contracts;
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

public sealed class GetMediaAssets: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/batch", async Task<EndpointResult<GetMediaAssetsResponse>> (
            [FromBody] GetMediaAssetsRequest request,
            [FromServices] GetMediaAssetsHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken));
    }
}

public sealed class GetMediaAssetsHandler(
    IReadDbContext readDbContext,
    IS3Provider s3Provider,
    ILogger<StartMultiPartUploadHandler> logger)
{
    public async Task<Result<GetMediaAssetsResponse, Error>> Handle(
        GetMediaAssetsRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.MediaAssetsIds.Any())
        {
            return new GetMediaAssetsResponse([]);
        }

        List<MediaAsset> mediaAssets = readDbContext.MediaAssetsQuery
            .Where(m => request.MediaAssetsIds.Contains(m.Id) && m.Status != MediaStatus.DELETED)
            .ToList();

        List<MediaAsset> readyMediaAssets = mediaAssets
            .Where(m => m.Status == MediaStatus.READY)
            .ToList();

        List<StorageKey> keys = readyMediaAssets.Select(m => m.Key).Distinct().ToList();

        Result<IReadOnlyList<Models.MediaUrl>, Error> urlsResult = await s3Provider.GenerateDownloadUrlsAsync(
            keys, cancellationToken);

        if (urlsResult.IsFailure)
            return urlsResult.Error;



        IReadOnlyList<Models.MediaUrl> urls = urlsResult.Value;

        var urlsDict = urls.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);

        List<GetMediaAssetsDto> results = [];

        foreach (MediaAsset mediaAsset in mediaAssets)
        {
            string? downloadUrl = null;

            if (urlsDict.TryGetValue(mediaAsset.Key, out string? url))
            {
                downloadUrl = url;
            }

            var mediaAssetDto = new GetMediaAssetsDto(
                mediaAsset.Id,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                mediaAsset.AssetType.ToString().ToLowerInvariant(),
                downloadUrl);

            results.Add(mediaAssetDto);
        }

        return new GetMediaAssetsResponse(results);
    }
}