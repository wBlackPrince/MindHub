using CSharpFunctionalExtensions;
using EducationContentService.Core.Database;
using EducationContentService.Domain.Lessons;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace EducationContentService.Core.Features.Lessons;


public sealed class SoftDeleteEndpoint: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("lessons/{lessonId:guid}", async Task<EndpointResult<Guid>>(
            [FromRoute] Guid lessonId,
            [FromServices] SoftDeleteHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(lessonId, cancellationToken));
    }
}

public sealed class SoftDeleteHandler(
    ILogger<SoftDeleteHandler> logger,
    ITransactionManager transactionManager,
    ILessonsRepository repository)
{
    public async Task<Result<Guid, Error>> Handle(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        Result<Lesson, Error> getLessonResult = await repository.GetByAsync(l => l.Id == lessonId, cancellationToken);

        if(getLessonResult.IsFailure)
            return getLessonResult.Error;

        getLessonResult.Value.SoftDelete();

        await transactionManager.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Soft delete lesson {LessonId}", getLessonResult.Value.Id);

        return getLessonResult.Value.Id;
    }
}