using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public sealed record MediaOwner
{
    private static readonly HashSet<string> AllowedContext = [
        "lesson",
        "module",
        "user"
    ];

    public const int MaxLength = 50;

    public string Context { get; }

    public Guid EntityId { get; }

    private MediaOwner(Guid entityId, string context)
    {
        EntityId = entityId;
        Context = context;
    }

    public static Result<MediaOwner, Error> Create(Guid entityId, string context)
    {
        if (string.IsNullOrWhiteSpace(context) || context.Length > MaxLength)
            return GeneralErrors.ValueIsInvalid(nameof(context));

        string normalizedContext = context.Trim().ToLowerInvariant();
        if (!AllowedContext.Contains(normalizedContext))
            return GeneralErrors.ValueIsInvalid(nameof(context));

        if (entityId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(entityId));

        return new MediaOwner(entityId, context);
    }

    public static Result<MediaOwner, Error> ForLesson(Guid lessonId) => new MediaOwner(lessonId, "lesson");

    public static Result<MediaOwner, Error> ForModule(Guid moduleId) => new MediaOwner(moduleId, "module");

    public static Result<MediaOwner, Error> ForUser(Guid userId) => new MediaOwner(userId, "user");
}