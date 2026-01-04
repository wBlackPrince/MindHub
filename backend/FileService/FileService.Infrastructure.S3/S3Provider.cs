using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Core.Models;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel;

namespace FileService.Infrastructure.S3;

public class S3Provider: IDisposable, IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _s3Options;
    private readonly ILogger<S3Provider> _logger;
    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(
        IAmazonS3 s3Client,
        IOptions<S3Options> s3Options,
        ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _s3Options = s3Options.Value;
        _logger = logger;
        _requestsSemaphore = new SemaphoreSlim(_s3Options.MaxConcurrentRequests);
    }

    // метод начинающий мульти-парт загрузку файла
    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        await _requestsSemaphore.WaitAsync(cancellationToken);

        try
        {
            var request = new InitiateMultipartUploadRequest()
            {
                BucketName = storageKey.Location,
                Key = storageKey.Value,
                ContentType = mediaData.ContentType.ToString()
            };

            _logger.LogCritical($"Bucket Name: {storageKey.Location}");

            InitiateMultipartUploadResponse response = await _s3Client.InitiateMultipartUploadAsync(
                storageKey.Location,
                storageKey.Value,
                cancellationToken);

            return response.UploadId;

        }
        finally
        {
            _requestsSemaphore.Release();
        }
    }


    // разбиение файла на чанки для дальнейшей мульти-парт загрузки
    public async Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<ChunkUploadUrl>> tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);

                    try
                    {
                        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
                        {
                            BucketName = storageKey.Location,
                            Key = storageKey.Value,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                            Protocol = _s3Options.WithSSL ? Protocol.HTTPS : Protocol.HTTP,
                        };

                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return new ChunkUploadUrl(partNumber, url);
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            ChunkUploadUrl[] results = await Task.WhenAll(tasks);

            return results;
        }
        catch (Exception e)
        {
            return S3ErrorMapper.ToError(e);
        }
    }


    // метод завершает мульти-парт загрузку файла
    public async Task<Result<string, Error>> CompleteMultiPartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            CompleteMultipartUploadRequest request = new CompleteMultipartUploadRequest()
            {
                BucketName = storageKey.Location,
                Key = storageKey.Value,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList(),
            };

            CompleteMultipartUploadResponse response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception e)
        {
            return S3ErrorMapper.ToError(e);
        }
    }



    // генерируем url для скачивания файла
    public async Task<Result<string?, Error>> GenerateDownloadUrlAsync(StorageKey storageKey, CancellationToken cancellationToken)
    {
        try
        {
            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
            {
                BucketName = storageKey.Location,
                Key = storageKey.Value,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationDays),
                Protocol = _s3Options.WithSSL ? Protocol.HTTPS : Protocol.HTTP,
            };

            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception e)
        {
            return S3ErrorMapper.ToError(e);
        }
    }


    public async Task<Result<IReadOnlyList<MediaUrl>, Error>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<MediaUrl>> tasks = storageKeys.Select(async storageKey =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);


                try
                {
                    GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
                    {
                        BucketName = storageKey.Location,
                        Key = storageKey.Value,
                        Verb = HttpVerb.GET,
                        Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationDays),
                        Protocol = _s3Options.WithSSL ? Protocol.HTTPS : Protocol.HTTP,
                    };

                    string? preSignedUrl = await _s3Client.GetPreSignedURLAsync(request);

                    return new MediaUrl(storageKey, preSignedUrl);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });

            MediaUrl[] result = await Task.WhenAll(tasks);

            return result;
        }
        catch (Exception e)
        {
            return S3ErrorMapper.ToError(e);
        }
    }

    public void Dispose()
    {
        _requestsSemaphore.Release();
        _requestsSemaphore.Dispose();
    }
}