using Framework.Endpoints;
using Framework.Middlewares;
using Serilog;

namespace EducationContentService.Web.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder ConfigureApp(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        app.UseRequestCorrelationId();
        app.UseSerilogRequestLogging();

        app.MapOpenApi();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Education Content Service V1");
        });

        RouteGroupBuilder apiGroup = app.MapGroup("/api").WithOpenApi();

        app.MapEndpoints(apiGroup);

        return app;
    }
}