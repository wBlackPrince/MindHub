using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets;

public class PreviewAsset: MediaAsset
{
    public const long MAX_SIZE = 5_368_709;

    public const string ASSET_TYPE = "preview";
    public const string LOCATION = "preview";
    public const string RAW_PREFIX = "raw";
    public const string ALLOWED_CONTENT_TYPE = "image";

    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];


    private PreviewAsset() { }

    private PreviewAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        MediaOwner owner,
        StorageKey storageKey)
        : base(id, mediaData, status, AssetType.PREVIEW, owner, storageKey)
    {
    }

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("preview.invalid.extension", $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }

        if (mediaData.ContentType.Category != MediaType.IMAGE)
        {
            return Error.Validation("preview.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("preview.invalid.size", $"File size must be less than {MAX_SIZE} bytes");
        }

        return UnitResult.Success<Error>();
    }

    public static Result<PreviewAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner mediaOwner)
    {
        UnitResult<Error> validationResult = Validate(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;

        Result<StorageKey, Error> key = StorageKey.Create(LOCATION, null, id.ToString());
        if (key.IsFailure)
            return key.Error;

        Result<StorageKey, Error> storageKeyResult = StorageKey.Create(id.ToString(), RAW_PREFIX, LOCATION);

        if (storageKeyResult.IsFailure)
            return storageKeyResult.Error;

        var preview = new PreviewAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            mediaOwner,
            storageKeyResult.Value);

        return preview;
    }

}