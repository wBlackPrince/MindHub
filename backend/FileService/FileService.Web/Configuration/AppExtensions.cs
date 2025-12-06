using Framework.Endpoints;
using Framework.Middlewares;
using Serilog;

namespace FileService.Web.Configuration;

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
            options.SwaggerEndpoint("/openapi/v1.json", "File Service V1");
        });

        RouteGroupBuilder apiGroup = app.MapGroup("/api").WithOpenApi();

        app.MapEndpoints(apiGroup);

        return app;
    }
}