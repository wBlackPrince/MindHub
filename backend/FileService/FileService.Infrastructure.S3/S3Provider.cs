using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core;
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
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        await _requestsSemaphore.WaitAsync(cancellationToken);

        try
        {
            var request = new InitiateMultipartUploadRequest()
            {
                BucketName = bucketName, Key = key, ContentType = contentType
            };

            InitiateMultipartUploadResponse response = await _s3Client.InitiateMultipartUploadAsync(
                bucketName,
                key,
                cancellationToken);

            return response.UploadId;

        }
        finally
        {
            _requestsSemaphore.Release();
        }
    }


    // разбиение файла на чанки для дальнейшей мульти-парт загрузки
    public async Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<string>> tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);

                    try
                    {
                        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
                        {
                            BucketName = bucketName,
                            Key = key,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                            Protocol = _s3Options.WithSSL ? Protocol.HTTPS : Protocol.HTTP,
                        };

                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            string[] results = await Task.WhenAll(tasks);

            return results;
        }
        catch (Exception e)
        {
            return S3ErrorMapper.ToError(e);
        }
    }


    // метод завершает мульти-парт загрузку файла
    public async Task<Result<string, Error>> CompleteMultiPartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            CompleteMultipartUploadRequest request = new CompleteMultipartUploadRequest()
            {
                BucketName = bucketName,
                Key = key,
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
    public async Task<string?> GenerateDownloadUrlAsync(
        string bucketName,
        string key)
    {
        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
            Protocol = _s3Options.WithSSL ? Protocol.HTTPS : Protocol.HTTP,
        };

        string? response = await _s3Client.GetPreSignedURLAsync(request);

        return response;
    }

    // генерируем url для загрузки файла
    public async Task<Result<string?, Error>> GenerateUploadUrlAsync(
        string bucketName,
        string key)
    {
        try
        {
            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(6),
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

    public void Dispose()
    {
        _requestsSemaphore.Release();
        _requestsSemaphore.Dispose();
    }
}