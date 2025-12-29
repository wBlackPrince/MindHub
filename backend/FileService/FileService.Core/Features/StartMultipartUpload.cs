using CSharpFunctionalExtensions;
using FileService.Contracts;
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
        app.MapPost("/files/multipart-upload", async Task<EndpointResult<Guid>> (
            [FromBody] StartMultiPartUploadRequest request,
            [FromServices] StartMultiPartUploadHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken));
    }
}


public sealed class StartMultiPartUploadHandler(
    ILogger<StartMultiPartUploadHandler> logger,
    IS3Provider s3Provider)
{
    public async Task<Result<Guid, Error>> Handle(
        StartMultiPartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var response = await s3Provider.StartMultipartUploadAsync(
            request,
            cancellationToken);

        return result;
    }
}