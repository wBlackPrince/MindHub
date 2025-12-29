using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets;

public class VideoAsset: MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120;

    public const string LOCATION = "videos";
    public const string RAW_PREFIX = "raw";
    public const string ALLOWED_CONTENT_TYPE = "video";


    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];


    private VideoAsset() { }

    private VideoAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        MediaOwner owner,
        StorageKey storageKey)
        : base(id, mediaData, status, AssetType.VIDEO, owner, storageKey)
    {
    }

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("video.invalid.extension", $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }

        if (mediaData.ContentType.Category != MediaType.VIDEO)
        {
            return Error.Validation("video.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("video.invalid.size", $"File size must be less than {MAX_SIZE} bytes");
        }

        return UnitResult.Success<Error>();

    }

    public static Result<VideoAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner mediaOwner)
    {
        UnitResult<Error> validationResult = Validate(mediaData);

        if (validationResult.IsFailure)
            return validationResult.Error;

        Result<StorageKey, Error> storageKeyResult = StorageKey.Create(id.ToString(), RAW_PREFIX, LOCATION);

        if(storageKeyResult.IsFailure)
            return storageKeyResult.Error;

        return new VideoAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            mediaOwner,
            storageKeyResult.Value);
    }
}