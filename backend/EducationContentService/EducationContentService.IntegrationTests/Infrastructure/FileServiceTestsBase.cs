using EducationContentService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace EducationContentService.IntegrationTests.Infrastructure;

public abstract class EducationServiceTestsBase : IClassFixture<IntegrationTestsWebFactory>, IAsyncLifetime
{
    protected EducationServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        Services = factory.Services;
    }

    protected IServiceProvider Services { get; init; }

    protected HttpClient AppHttpClient { get; init; }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task ExecuteInDb(Func<EducationDbContext, Task> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        EducationDbContext dbContext = scope.ServiceProvider.GetRequiredService<EducationDbContext>();

        await action(dbContext);
    }
}
