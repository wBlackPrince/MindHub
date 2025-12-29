namespace FileService.Infrastructure.S3;

public record S3Options
{
    public string Endpoint { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public bool WithSSL { get; init; }

    public int DownloadUrlExpirationHours { get; init; } = 24;

    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];

    public int UploadUrlExpirationHours { get; init; } = 1;

    public int MaxConcurrentRequests { get; init; } = 20;
}