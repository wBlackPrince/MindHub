using EducationContentService.Core.EndpointsSettings;
using Serilog;

namespace EducationContentService.Core.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder ConfigureApp(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseSwagger();
        app.UseSwaggerUI();

        RouteGroupBuilder apiGroup = app.MapGroup("api/lessons");

        app.MapEndpoints(apiGroup);

        return app;
    }
}