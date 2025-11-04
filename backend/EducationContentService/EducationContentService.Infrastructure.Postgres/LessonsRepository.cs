using CSharpFunctionalExtensions;
using EducationContentService.Core.Features.Lessons;
using EducationContentService.Domain.Lessons;
using EducationContentService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EducationContentService.Infrastructure.Postgres;

public class LessonsRepository(
    ILogger<LessonsRepository> logger,
    EducationDbContext dbContext): ILessonsRepository
{
    public async Task<Result<Guid, Error>> Add(Lesson lesson, CancellationToken cancellationToken = default)
    {
        dbContext.Add(lesson);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return lesson.Id;
        }
        catch (DbUpdateException ex) when(ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                && pgEx.ConstraintName.Contains("title", StringComparison.InvariantCultureIgnoreCase))
            {
                return EducationErrors.TitleConflict(lesson.Title.Value);
            }


            logger.LogError(
                ex,
                "Database update error while creating lesson with title {Title}",
                lesson.Title.Value);

            return EducationErrors.DatabaseError();

        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(
                ex,
                "Operation was cancelled while creating lesson with title {Title}",
                lesson.Title.Value);

            return EducationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while creating lesson with title {Title}",
                lesson.Title.Value);

            return EducationErrors.DatabaseError();
        }

    }
}