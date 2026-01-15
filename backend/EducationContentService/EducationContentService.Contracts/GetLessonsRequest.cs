namespace EducationContentService.Contracts;

public record GetLessonsRequest(string? Search, int Page, int PageSize);