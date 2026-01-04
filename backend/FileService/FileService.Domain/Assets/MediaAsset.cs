using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }

    public MediaData MediaData { get; protected set; } = null!;

    public AssetType AssetType { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    public StorageKey Key { get; protected set; } = null!;

    public MediaStatus Status { get; protected set; }

    public string? UploadId { get; protected set; }

    protected MediaAsset()
    {
    }

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType assetType,
        StorageKey key)
    {
        Id = id;
        MediaData = mediaData;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        AssetType = assetType;
        Key = key;
    }

    public static Result<MediaAsset, Error> CreateForUpload(MediaData mediaData, AssetType assetType)
    {
        var assetId = Guid.NewGuid();

        switch (assetType)
        {
            case AssetType.VIDEO:
                Result<VideoAsset, Error> videoResult = VideoAsset.CreateForUpload(assetId, mediaData);
                return videoResult.IsFailure ? videoResult.Error : videoResult.Value;
            case AssetType.PREVIEW:
                Result<PreviewAsset, Error> previewResult = PreviewAsset.CreateForUpload(assetId, mediaData);
                return previewResult.IsFailure ? previewResult.Error : previewResult.Value;

            case AssetType.AVATAR:
            default:
                throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
        }
    }

    public void SetUploadId(string uploadId)
    {
        if (Status != MediaStatus.UPLOADING)
            return;

        UploadId = uploadId;
        UpdatedAt = DateTime.UtcNow;
    }

    public UnitResult<Error> MarkUploaded()
    {
        if (Status != MediaStatus.UPLOADING)
            return UnitResult.Success<Error>();

        Status = MediaStatus.UPLOADED;
        UploadId = null;
        UpdatedAt = DateTime.UtcNow;
        return UnitResult.Success<Error>();
    }

    public void MarkFailed()
    {
        Status = MediaStatus.FAILED;
        UpdatedAt = DateTime.UtcNow;
    }
}
