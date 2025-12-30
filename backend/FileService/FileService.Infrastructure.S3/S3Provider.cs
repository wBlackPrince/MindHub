using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
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
    public async Task<string?> GenerateDownloadUrlAsync(StorageKey storageKey, CancellationToken cancellationToken)
    {
        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
        {
            BucketName = storageKey.Location,
            Key = storageKey.Value,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
            Protocol = _s3Options.WithSSL ? Protocol.HTTPS : Protocol.HTTP,
        };

        string? response = await _s3Client.GetPreSignedURLAsync(request);

        return response;
    }

    public void Dispose()
    {
        _requestsSemaphore.Release();
        _requestsSemaphore.Dispose();
    }
}