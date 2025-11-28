namespace EducationContentService.Contracts;

public record GetLessonRequest(string? Search, int Page, int PageSize);