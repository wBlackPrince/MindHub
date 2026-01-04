namespace FileService.Contracts;

public record StartMultiPartUploadRequest(
    string FileName,
    string AssetType,
    string ContentType,
    long Size);

// последние два свойства добавлю когда будет связь между микросервисами
// public record StartMultiPartUploadRequest(
//     string FileName,
//     string AssetType,
//     string ContentType,
//     long Size,
//     string Context,
//     Guid ContextId);