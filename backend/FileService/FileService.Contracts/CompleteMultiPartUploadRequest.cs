namespace FileService.Contracts;

public record CompleteMultiPartUploadRequest(Guid MediaAssetId, string UploadId, List<PartETagDto> PartETagDtos);