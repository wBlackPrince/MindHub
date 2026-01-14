namespace EducationContentService.Contracts;

public record CreateLessonRequest(string Title, string Description, Guid VideoId);