using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Musialize.Api.Health;
using Musialize.Application;
using Musialize.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

// Created before anything else, so a crash during startup, even while reading appsettings, is logged too.
Log.Logger = WriteToConsole(new LoggerConfiguration(), IsDevelopmentBeforeTheHostExists()).CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    {
        if (context.Exception is not null
            && context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            context.ProblemDetails.Detail = context.Exception.ToString();
        }
    });
    builder.Services.AddSerilog((services, logger) =>
        WriteToConsole(logger, services.GetRequiredService<IHostEnvironment>().IsDevelopment())
            .ReadFrom.Configuration(services.GetRequiredService<IConfiguration>())
            .ReadFrom.Services(services)
            .Enrich.FromLogContext(),
        // Each app keeps its own logger instead of the global Log.Logger, so test apps don't share one (ADR-0011).
        preserveStaticLogger: true);
    builder.Services.AddApplication();
    // For request types declared in Api; Application registers its own.
    builder.Services.AddValidation();
    builder.Services.AddInfrastructure();
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

    var app = builder.Build();

    // First, so it logs the final status code.
    app.UseSerilogRequestLogging(options =>
    {
        // Otherwise it uses the global Log.Logger (see preserveStaticLogger).
        options.Logger = app.Services.GetRequiredService<Serilog.ILogger>();
        options.GetLevel = (httpContext, _, exception) => (exception, httpContext.Response.StatusCode) switch
        {
            (not null, _) => LogEventLevel.Error,
            // A crash is already logged as an Error by the exception handler.
            (_, >= 500) => LogEventLevel.Warning,
            // Below the minimum level, so dropped: Dokploy calls /health every 10 seconds.
            _ when httpContext.Request.Path == HealthEndpoints.Path => LogEventLevel.Verbose,
            _ => LogEventLevel.Information,
        };
    });
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        // An unreadable request body is the client's error: it keeps its 400 and isn't logged.
        StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
            ? badRequest.StatusCode
            : StatusCodes.Status500InternalServerError,
        // Setting this replaces .NET 10's default of skipping exceptions an IExceptionHandler handled, so keep that.
        SuppressDiagnosticsCallback = context => context.Exception is BadHttpRequestException
            || context.ExceptionHandledBy == ExceptionHandledType.ExceptionHandlerService,
    });
    // Gives bodyless error responses, such as a 404, a ProblemDetails body.
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.MapHealthEndpoints();

    // The built SPA, which only the image has (the Dockerfile copies it into wwwroot). In
    // development Vite serves it instead.
    if (app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists)
    {
        MapSpa(app);
    }

    app.Run();
    return 0;
}
// Tests and the build-time OpenAPI generation stop the app on purpose with HostAbortedException.
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "The app stopped because of an unexpected exception");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static LoggerConfiguration WriteToConsole(LoggerConfiguration logger, bool development) =>
    development
        ? logger.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        : logger.WriteTo.Console(new RenderedCompactJsonFormatter());

// Every GET for a path that isn't an endpoint or a file returns index.html, so the SPA's router
// handles it. Under /api a missing endpoint is a ProblemDetails 404 (from UseStatusCodePages)
// instead. Both fallbacks take only GET and HEAD, like a page load: one that took every method
// would turn a wrong method on a real endpoint from a 405 into a 404.
static void MapSpa(WebApplication app)
{
    app.MapStaticAssets();
    app.MapFallback("/api/{**path}", () => TypedResults.NotFound())
        .WithMetadata(new HttpMethodMetadata([HttpMethods.Get, HttpMethods.Head]));
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        // Always revalidated: a cached copy would point at assets a deploy has removed.
        OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache",
    });
}

// Reads what WebApplication.CreateBuilder reads later: ASPNETCORE_ENVIRONMENT wins, and unset means Production.
static bool IsDevelopmentBeforeTheHostExists() =>
    string.Equals(
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
        Environments.Development,
        StringComparison.OrdinalIgnoreCase);
