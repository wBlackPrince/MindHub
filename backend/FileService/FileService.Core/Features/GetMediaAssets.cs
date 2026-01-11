using System.Security.Cryptography;
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
using Microsoft.Extensions.Caching.Hybrid;
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
    FileStorageProvider fileStorageProvider,
    HybridCache cache,
    FileStorageOptions fileStorageOptions,
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


        Dictionary<StorageKey, string> urls = await GetPresignedUrlsFromCache(keys, cancellationToken);

        List<GetMediaAssetsDto> results = [];

        foreach (MediaAsset mediaAsset in mediaAssets)
        {
            string? downloadUrl = null;

            if (urls.TryGetValue(mediaAsset.Key, out string? url))
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


    private async Task<Dictionary<StorageKey, string>> GetPresignedUrlsFromCache(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken)
    {
        List<StorageKey> keys = storageKeys.ToList();

        if (!keys.Any())
            return [];

        IEnumerable<Task<(StorageKey keys, string? url)>> cachedUrlsTasks = keys.Select(async key =>
        {
            string? url = await cache.GetOrCreateAsync<string?>(
                key.Value,
                factory: _ => ValueTask.FromResult<string?>(null),
                options: new HybridCacheEntryOptions()
                {
                    Expiration =
                        TimeSpan.FromDays(fileStorageOptions.DownloadUrlExpirationDays)
                            .Subtract(TimeSpan.FromHours(1)),
                    LocalCacheExpiration = TimeSpan.FromHours(1)
                },
                cancellationToken: cancellationToken);

            return (key, url);
        });

        (StorageKey keys, string? url)[] cachedUrls = await Task.WhenAll(cachedUrlsTasks);

        Dictionary<StorageKey, string> result = new Dictionary<StorageKey, string>();
        List<StorageKey> keysToGenerate = new List<StorageKey>();

        foreach ((StorageKey key, string? url) in cachedUrls)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                result[key] = url;
            }
            else
            {
                keysToGenerate.Add(key);
            }
        }

        if (keysToGenerate.Any())
        {
            Result<IReadOnlyList<Models.MediaUrl>, Error> mediaUrls = await fileStorageProvider.GenerateDownloadUrlsAsync(
                keysToGenerate,
                cancellationToken);

            if (mediaUrls.IsFailure)
                return result;


            var setTasks = mediaUrls.Value.Select(async mediaUrl =>
            {
                result[mediaUrl.StorageKey] = mediaUrl.PresignedUrl;

                await cache.SetAsync(
                    key: mediaUrl.StorageKey.Value,
                    mediaUrl.PresignedUrl,
                    options: new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan
                            .FromDays(fileStorageOptions.DownloadUrlExpirationDays)
                            .Subtract(TimeSpan.FromHours(1))
                    });
            });

            await Task.WhenAll(setTasks);

            return result;
        }

        return result;
    }
}