namespace EducationContentService.Contracts;

public record PaginationLessonResponse(IReadOnlyList<LessonDto> Lessons, int TotalCount);