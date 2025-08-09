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
        .AddHttpContextAccessor()
        .AddPersistenceServices(builder.Configuration)
        .AddAuthenticationServices(builder.Configuration, builder.Environment)
        .AddSwaggerServices()
        .AddMetricServer(options => { });

    var app = builder.Build();

    //Log.Information("Applying migrations...");
    //using (var scope = app.Services.CreateScope())
    //{
    //    var services = scope.ServiceProvider;
    //    try
    //    {
    //        var mainContext = services.GetRequiredService<MyDataContext>();
    //        mainContext.Database.Migrate();
    //        Log.Information("Main database migration completed.");

    //        var logContext = services.GetRequiredService<LogDataContext>();
    //        logContext.Database.Migrate();
    //        Log.Information("Log database migration completed.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Log.Error(ex, "An error occurred during migration.");
    //        throw;
    //    }
    //}

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