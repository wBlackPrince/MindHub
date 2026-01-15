using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using Shared.SharedKernel;

namespace EducationContentService.IntegrationTests.Mocks;

public class FileServiceCommunicationMock: IFileCommunicationService
{
    public Task<Result<GetMediaAssetsResponse, Error>> GetMediaAssets(
        GetMediaAssetsRequest request,
        CancellationToken cancellationToken)
    {
        GetMediaAssetsResponse result = new GetMediaAssetsResponse([
            new GetMediaAssetsDto(Guid.NewGuid(), "ready", "video", "url"),
            new GetMediaAssetsDto(Guid.NewGuid(), "ready", "video", "url"),
            new GetMediaAssetsDto(Guid.NewGuid(), "ready", "video", "url")
        ]);

        return Task.FromResult(Result.Success<GetMediaAssetsResponse, Error>(result));
    }
}