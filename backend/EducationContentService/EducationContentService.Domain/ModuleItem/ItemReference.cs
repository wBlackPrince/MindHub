namespace EducationContentService.Domain.ModuleItem;

public record ItemReference
{
    public Guid ItemId { get; }

    public ItemType ItemType { get; }

    private ItemReference(Guid itemId, ItemType itemType)
    {
        ItemId = itemId;
        ItemType = itemType;
    }

    public static ItemReference ToLesson(Guid lessonId) => new ItemReference(lessonId, ItemType.LESSON);

    public static ItemReference ToIssue(Guid issueId) => new ItemReference(issueId, ItemType.ISSUE);
}