using System.Globalization;
using FileService.Web.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web app...");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    string environment = builder.Environment.EnvironmentName;

    builder.Configuration.AddJsonFile($"appsettings.{environment}.json", true, true);

    builder.Configuration.AddEnvironmentVariables();
    builder.Services.AddConfiguration(builder.Configuration);

    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    WebApplication app = builder.Build();

    app.ConfigureApp();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception");
}
finally
{
    await Log.CloseAndFlushAsync(); // ensure all logs written before app exits
}

namespace FileService.Web
{
    public partial class Program;
}