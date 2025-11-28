using Core.Validation;
using CSharpFunctionalExtensions;
using EducationContentService.Contracts;
using EducationContentService.Core.Database;
using EducationContentService.Domain.Lessons;
using EducationContentService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace EducationContentService.Core.Features.Lessons;

public class UpdateLessonRequestValidator : AbstractValidator<UpdateLessonInfoRequest>
{
    public UpdateLessonRequestValidator()
    {
        RuleFor(x => x.Title)
            .MustBeValueObject(Title.Create);

        RuleFor(x => x.Title)
            .MustBeValueObject(Description.Create);
    }
}

public sealed class UpdateEndpoint: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("lessons/{lessonId:guid}", async Task<EndpointResult<Guid>>(
            [FromRoute] Guid lessonId,
            [FromBody] UpdateLessonInfoRequest infoRequest,
            [FromServices] UpdateInfoHandler handler,
            CancellationToken cancellationToken)
                => await handler.Handle(
                    lessonId,
                    infoRequest,
                    cancellationToken));
    }
}

public sealed class UpdateInfoHandler(
    ILogger<UpdateInfoHandler> logger,
    ILessonsRepository lessonsRepository,
    ITransactionManager transactionManager,
    IValidator<UpdateLessonInfoRequest> requestValidator)
{
    public async Task<Result<Guid, Error>> Handle(
        Guid lessonId,
        UpdateLessonInfoRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await requestValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        Title title = Title.Create(request.Title).Value;

        Description description = Description.Create(request.Description).Value;

        Result<Lesson, Error> getLessonResult =
            await lessonsRepository.GetByAsync(l => l.Id == lessonId, cancellationToken);

        if(getLessonResult.IsFailure)
            return getLessonResult.Error;

        getLessonResult.Value.UpdateInfo(title, description);

        await transactionManager.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated lesson {@LessonId}", getLessonResult.Value.Id);

        return getLessonResult.Value.Id;
    }
}