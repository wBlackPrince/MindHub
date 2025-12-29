namespace FileService.Domain.Assets;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }

    public MediaData MediaData { get; protected set; } = null!;

    public AssetType AssetType { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    public StorageKey Key { get; protected set; } = null!;

    public MediaOwner Owner { get; protected set; } = null!;

    public MediaStatus Status { get; protected set; }

    // Ef Core
    protected MediaAsset()
    {
    }


    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType assetType,
        MediaOwner owner,
        StorageKey key)
    {
        Id = id;
        MediaData = mediaData;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AssetType = assetType;
        Owner = owner;
        Key = key;
    }
}