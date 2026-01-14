using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Core.HttpCommunication;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Contracts.HttpCommunication;

internal sealed class FileHttpClient: IFileCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileHttpClient> _logger;

    public FileHttpClient(
        HttpClient httpClient,
        ILogger<FileHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<GetMediaAssetsResponse, Error>> GetMediaAssets(
        GetMediaAssetsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/files/batch", cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting media assets for {MediaAssetsIds}", request.MediaAssetsIds);

            return Error.Failure("server.internal", "Failed to request media assets info");
        }
    }
}