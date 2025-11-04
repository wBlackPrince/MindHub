using CSharpFunctionalExtensions;
using EducationContentService.Domain.Lessons;
using EducationContentService.Domain.Shared;
using EducationContentService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Core.Features.Lessons;

public record CreateLessonRequest(string Title, string Description);

public sealed class CreateEndpoint: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("lessons", async (
            [FromBody] CreateLessonRequest request,
            CreateHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.Handle(request, cancellationToken);
        });
    }
}

public sealed class CreateHandler(
    ILogger<CreateHandler> logger,
    ILessonsRepository lessonsRepository)
{
    public async Task<Result<Guid, Error>> Handle(
        CreateLessonRequest request,
        CancellationToken cancellationToken)
    {
        Result<Title, Error> titleResult = Title.Create(request.Title);

        if (titleResult.IsFailure)
            return titleResult.Error;

        Result<Description, Error> descriptionResult = Description.Create(request.Description);

        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        Lesson lesson = new Lesson(
            Guid.NewGuid(),
            titleResult.Value,
            descriptionResult.Value);

        Result<Guid, Error> result = await lessonsRepository.Add(lesson, cancellationToken);

        if (result.IsFailure)
            return result.Error;

        logger.LogInformation("Created lesson {@Lesson}", lesson);

        return result;
    }
}