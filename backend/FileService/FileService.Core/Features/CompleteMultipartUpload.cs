using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Domain.Assets;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public sealed class CompleteMultipartUpload: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/complete-upload", async Task<EndpointResult>(
            [FromBody] CompleteMultipartUploadRequest request,
            [FromServices] CompleteMultiPartUploadHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken));
    }
}


public sealed class CompleteMultiPartUploadHandler(
    ILogger<CompleteMultiPartUploadHandler> logger,
    IMediaAssetsRepository mediaAssetsRepository,
    FileStorageProvider fileStorageProvider)
{
    public async Task<UnitResult<Error>> Handle(
        CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        (_, bool isFailure, MediaAsset? mediaAsset, Error? error) = await mediaAssetsRepository.GetByAsync(
            m => m.Id == request.MediaAssetId,
            cancellationToken);

        if (isFailure)
            return error;

        if (mediaAsset.MediaData.ExpectedChunksCount != request.PartETagDtos.Count)
            return GeneralErrors.Failure("Количество eTags не соответствует количеству чанков");

        Result<string, Error> completedResult = await fileStorageProvider.CompleteMultiPartUploadAsync(
            mediaAsset.Key,
            request.UploadId,
            request.PartETagDtos,
            cancellationToken);

        if (completedResult.IsFailure)
        {
            mediaAsset.MarkFailed();
            await mediaAssetsRepository.SaveChangesAsync(cancellationToken);
            return completedResult.Error;
        }

        mediaAsset.MarkUploaded();

        await mediaAssetsRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("File uploaded successfully - mediaAsset Id: {MediaAssetId}", mediaAsset.Id);

        return UnitResult.Success<Error>();
    }
}