namespace FileService.Contracts;

public record CompleteMultipartUploadRequest(Guid MediaAssetId, string UploadId, List<PartETagDto> PartETagDtos);