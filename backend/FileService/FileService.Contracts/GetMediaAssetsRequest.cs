namespace FileService.Contracts;

public record GetMediaAssetsRequest(IReadOnlyList<Guid> MediaAssetsIds);