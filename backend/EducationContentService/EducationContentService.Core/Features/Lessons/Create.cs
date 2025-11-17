using CSharpFunctionalExtensions;
using EducationContentService.Core.Endpoints;
using EducationContentService.Core.Validation;
using EducationContentService.Domain.Exceptions;
using EducationContentService.Domain.Lessons;
using EducationContentService.Domain.Shared;
using EducationContentService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace EducationContentService.Core.Features.Lessons;

public record CreateLessonRequest(string Title, string Description);

public class CreateLessonRequestValidator : AbstractValidator<CreateLessonRequest>
{
    public CreateLessonRequestValidator()
    {
        RuleFor(x => x.Title)
            .MustBeValueObject(Title.Create);

        RuleFor(x => x.Title)
            .MustBeValueObject(Description.Create);
    }
}

public sealed class CreateEndpoint: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("lessons", async Task<EndpointResult<Guid>>(
            [FromBody] CreateLessonRequest request,
            [FromServices] CreateHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken));
    }
}

public sealed class CreateHandler(
    ILogger<CreateHandler> logger,
    ILessonsRepository lessonsRepository,
    IValidator<CreateLessonRequest> requestValidator)
{
    public async Task<Result<Guid, Error>> Handle(
        CreateLessonRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await requestValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        Title title = Title.Create(request.Title).Value;

        Description description = Description.Create(request.Description).Value;

        Lesson lesson = new Lesson(
            Guid.NewGuid(),
            title,
            description);

        Result<Guid, Error> result = await lessonsRepository.Add(lesson, cancellationToken);

        if (result.IsFailure)
            return result.Error;

        logger.LogInformation("Created lesson {@Lesson}", lesson);

        return result;
    }
}