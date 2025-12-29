using CSharpFunctionalExtensions;
using FileService.Contracts;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IS3Provider
{
    Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> CompleteMultiPartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);

    Task<string?> GenerateDownloadUrlAsync(
        string bucketName,
        string key);

    Task<Result<string?, Error>> GenerateUploadUrlAsync(
        string bucketName,
        string key);
}