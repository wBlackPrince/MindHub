using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public sealed record ContentType
{
    public string Value { get; }

    public MediaType Category { get; }

    private ContentType(string value, MediaType mediaType)
    {
        Value = value;
        Category = mediaType;
    }

    public static Result<ContentType, Error> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return GeneralErrors.ValueIsInvalid(nameof(contentType));

        MediaType category = contentType switch
        {
            var ct when ct.Contains("video", StringComparison.InvariantCultureIgnoreCase) => MediaType.VIDEO,
            var ct when ct.Contains("image", StringComparison.InvariantCultureIgnoreCase) => MediaType.IMAGE,
            var ct when ct.Contains("audio", StringComparison.InvariantCultureIgnoreCase) => MediaType.AUDIO,
            var ct when ct.Contains("document", StringComparison.InvariantCultureIgnoreCase) => MediaType.DOCUMENT,
            _ => MediaType.UNKNOWN
        };

        return new ContentType(contentType, category);
    }
}