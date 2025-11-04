using EducationContentService.Web.EndpointSettings;
using EducationContentService.Web.Middlewares;
using Serilog;

namespace EducationContentService.Web.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder ConfigureApp(this WebApplication app)
    {
        app.UseRequestCorrelationId();
        app.UseSerilogRequestLogging();

        app.UseSwagger();
        app.UseSwaggerUI();

        RouteGroupBuilder apiGroup = app.MapGroup("api/lessons");

        app.MapEndpoints(apiGroup);

        return app;
    }
}