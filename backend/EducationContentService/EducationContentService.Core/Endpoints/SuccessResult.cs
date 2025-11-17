using System.Net;
using EducationContentService.Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace EducationContentService.Core.Endpoints;

public class SuccessResult: IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        Envelope envelope = Envelope.Ok();

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}

public class SuccessResult<TValue>(TValue value): IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        Envelope envelope = Envelope.Ok(value);

        httpContext.Response.StatusCode = StatusCodes.Status200OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}