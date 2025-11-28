using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using EducationContentService.Core.Features.Lessons;
using EducationContentService.Domain.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.SharedKernel;
using Index = EducationContentService.Infrastructure.Postgres.Configurations.Index;

namespace EducationContentService.Infrastructure.Postgres;

public class LessonsRepository(
    ILogger<LessonsRepository> logger,
    EducationDbContext dbContext): ILessonsRepository
{
    public async Task<Result<Guid, Error>> AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        dbContext.Lessons.Add(lesson);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return lesson.Id;
        }
        catch (DbUpdateException ex) when(ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                && pgEx.ConstraintName.Contains(Index.TITLE, StringComparison.InvariantCultureIgnoreCase))
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

    public async Task<Result<Lesson, Error>> GetByAsync(
        Expression<Func<Lesson, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Lesson? lesson = await dbContext.Lessons.FirstOrDefaultAsync(predicate, cancellationToken);

        if (lesson is null)
            return GeneralErrors.NotFound(null, "lesson");

        return lesson;
    }
}