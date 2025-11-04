using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Core.Features.Lessons;

public sealed class CreateEndpoint: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("lessons", async (CreateHandler handler) =>
        {
            await handler.Handle();
        });
    }
}

public sealed class CreateHandler(ILogger<CreateHandler> logger)
{
    public async Task Handle()
    {
        logger.LogInformation("Creating a new lesson");
        await Task.Delay(1000);
        logger.LogInformation("Created a new lesson");
    }
}