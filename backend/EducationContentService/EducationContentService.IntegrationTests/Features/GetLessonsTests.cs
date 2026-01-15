using CSharpFunctionalExtensions;
using EducationContentService.Contracts;
using EducationContentService.Domain.Lessons;
using EducationContentService.Domain.ValueObjects;
using EducationContentService.IntegrationTests.Infrastructure;
using FileService.Core.HttpCommunication;
using Microsoft.AspNetCore.WebUtilities;
using Shared.SharedKernel;

namespace EducationContentService.IntegrationTests.Features;

public class GetLessonsTests : EducationServiceTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;

    public GetLessonsTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetLessons_Should_Return_Lessons()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        await ExecuteInDb(async db =>
        {
            // Создаем несколько уроков
            var lessons = new List<Lesson>();

            // Попытка создать Title и Description
            var title1Result = Title.Create("Первый урок по C#");
            var desc1Result = Description.Create("Описание первого урока по C#");

            var title2Result = Title.Create("Второй урок по EF Core");
            var desc2Result = Description.Create("Описание второго урока по EF Core");

            var title3Result = Title.Create("Второй урок по DDD");
            var desc3Result = Description.Create("Описание второго урока по EF Core");

            // Проверяем, что создание прошло успешно
            if (title1Result.IsSuccess && desc1Result.IsSuccess)
            {
                lessons.Add(new Lesson(
                    id: null,
                    title: title1Result.Value,
                    description: desc1Result.Value,
                    videoId: Guid.NewGuid()
                ));
            }

            if (title2Result.IsSuccess && desc2Result.IsSuccess)
            {
                lessons.Add(new Lesson(
                    id: null,
                    title: title2Result.Value,
                    description: desc2Result.Value,
                    videoId: Guid.NewGuid()
                ));
            }

            if (title3Result.IsSuccess && desc3Result.IsSuccess)
            {
                lessons.Add(new Lesson(
                    id: null,
                    title: title3Result.Value,
                    description: desc3Result.Value,
                    videoId: Guid.NewGuid()
                ));
            }

            // Добавляем в контекст EF
            db.Lessons.AddRange(lessons);

            await db.SaveChangesAsync(cancellationToken);
        });

        var getLessonsRequest = new GetLessonsRequest(null, 1, 3);

        var queryParams = new Dictionary<string, string?>
        {
            {
                "page", getLessonsRequest.Page.ToString()
            },
            {
                "pageSize", getLessonsRequest.PageSize.ToString()
            },
            {
                "search", null
            }
        };

        string url = QueryHelpers.AddQueryString("api/lessons", queryParams);

        HttpResponseMessage startMultipartResponse = await AppHttpClient.GetAsync(url, cancellationToken);

        // act
        Result<PaginationLessonResponse, Error> lessonsResponse = await startMultipartResponse
            .HandleResponseAsync<PaginationLessonResponse>(cancellationToken);

        // assert
        Assert.True(lessonsResponse.IsSuccess);
        Assert.Equal(3, lessonsResponse.Value.Lessons.Count);
        Assert.Equal(3, lessonsResponse.Value.Lessons.Select(l => l.Video).Count());
    }
}
