namespace FileService.Contracts.Dtos;

public record CompleteMultipartUploadRequest(Guid MediaAssetId, string UploadId, List<PartETagDto> PartETagDtos);