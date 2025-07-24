using FinTrackWebApi.Extensions;
using Prometheus;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "logs/fintrack-log-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    QuestPDF.Settings.License = LicenseType.Community;

    builder.Services
        .AddApplicationServices(builder.Configuration)
        .AddPersistenceServices(builder.Configuration)
        .AddAuthenticationServices(builder.Configuration, builder.Environment)
        .AddSwaggerServices()
        .AddMetricServer(options => { });

    var app = builder.Build();

    await app.SeedDatabaseAsync();

    app.ConfigurePipeline();

    app.UseMetricServer();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}