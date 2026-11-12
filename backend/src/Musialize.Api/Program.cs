using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Musialize.Api.Health;
using Musialize.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

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
app.UseExceptionHandler();
// Gives bodyless error responses, such as a 404, a ProblemDetails body.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthEndpoints();

app.Run();

static LoggerConfiguration WriteToConsole(LoggerConfiguration logger, bool development) =>
    development
        ? logger.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        : logger.WriteTo.Console(new RenderedCompactJsonFormatter());
