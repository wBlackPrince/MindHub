using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Core.HttpCommunication;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, Error>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken) where TResponse: class
    {
        try
        {
            Envelope<TResponse>? startMultipartUploadResponse = await response.Content
                .ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return startMultipartUploadResponse?.Error ?? GeneralErrors.Failure("Error while reading response");
            }

            if (startMultipartUploadResponse is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (startMultipartUploadResponse.Error is not null)
            {
                return startMultipartUploadResponse.Error;
            }

            if (startMultipartUploadResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            return startMultipartUploadResponse.Result;
        }
        catch (Exception e)
        {
            return GeneralErrors.Failure("Error while reading response");
        }
    }

    public static async Task<UnitResult<Error>> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            Envelope? jsonResponse = await response.Content
                .ReadFromJsonAsync<Envelope>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return jsonResponse?.Error ?? GeneralErrors.Failure("Error while reading response");
            }

            if (jsonResponse is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (jsonResponse.Error is not null)
            {
                return jsonResponse.Error;
            }

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            return GeneralErrors.Failure("Error while reading response");
        }
    }
}