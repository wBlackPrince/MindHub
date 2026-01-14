using System.Net.Http.Json;
using Core.Validation;
using CSharpFunctionalExtensions;
using EducationContentService.Contracts;
using EducationContentService.Core.Database;
using EducationContentService.Domain.Lessons;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using FluentValidation;
using FluentValidation.Results;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace EducationContentService.Core.Features.Lessons;

public class GetLessonRequestValidator : AbstractValidator<GetLessonRequest>
{
    public GetLessonRequestValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(1000).WithError(GeneralErrors.ValueIsInvalid("search"));

        RuleFor(x => x.Page)
            .NotEmpty().WithError(GeneralErrors.ValueIsInvalid("page"))
            .GreaterThan(0).WithError(GeneralErrors.ValueIsInvalid("page"));

        RuleFor(x => x.PageSize)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("page_size"))
            .GreaterThan(0).WithError(GeneralErrors.ValueIsInvalid("page_size"));
    }
}

public sealed class GetEndpoint: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("lessons", async Task<EndpointResult<PaginationLessonResponse>>(
            [AsParameters] GetLessonRequest request,
            [FromServices] GetHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(request, cancellationToken));
    }
}

public sealed class GetHandler(
    IEducationReadDbContext dbContext,
    IFileCommunicationService fileCommunicationService,
    IValidator<GetLessonRequest> requestValidator)
{
    public async Task<Result<PaginationLessonResponse, Error>> Handle(
        GetLessonRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await requestValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        IQueryable<Lesson> query = dbContext.LessonsQuery;

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(l => EF.Functions.Like(l.Title.Value, $"%{request.Search}%"));
        }

        int lessonsCount = await query.CountAsync(cancellationToken);

        List<LessonDto> lessons = await query
            .Select(l => new LessonDto()
            {
                Id = l.Id,
                Title = l.Title.Value,
                Description = l.Description.Value,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Video = new MediaDto
                {
                    Id = l.Id
                }
            })
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        List<Guid> mediaAssetIds = lessons
            .Where(l => l.Video.Id != null)
            .Select(l => l.Video!.Id!.Value)
            .ToList();

        Result<GetMediaAssetsResponse, Error> fileMediaAssets = await fileCommunicationService.GetMediaAssets(
            new GetMediaAssetsRequest(mediaAssetIds),
            cancellationToken);

        if (fileMediaAssets.IsFailure)
            return fileMediaAssets.Error;

        Dictionary<Guid, GetMediaAssetsDto> mediaAssetsDict = fileMediaAssets.Value.MediaAssets
            .ToDictionary(ma => ma.Id, ma => ma);

        foreach (LessonDto lesson in lessons)
        {
            if (lesson.Video != null && mediaAssetsDict.TryGetValue(lesson.Video.Id.Value, out GetMediaAssetsDto? mediaAsset))
            {
                lesson.Video = new MediaDto()
                {
                    Id = mediaAsset.Id,
                    Url = mediaAsset.Url,
                    Status = mediaAsset.Status,
                };
            }
        }

        return new PaginationLessonResponse(lessons, lessonsCount);
    }
}