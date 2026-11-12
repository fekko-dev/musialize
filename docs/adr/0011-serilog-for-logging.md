# Serilog for logging

Application code logs through the built-in `ILogger` abstraction, and Serilog writes those entries. Serilog gives readable lines in development, compact JSON with the TraceId (`@tr`) on stdout in production, a one-line request log (`HTTP GET /api/songs responded 200 in 12.3 ms`), and sinks for tools such as Seq or Elastic if we add one later. Program.cs also logs a crash while the app starts or runs, before the host's own logger exists, and exits with code 1.

## Considered options

- **The built-in console logger with `AddHttpLogging`**: no extra packages and enough for stdout in Dokploy, but the request entry is a multi-line block, scopes have to be switched on to get the TraceId, there are no sinks for other tools, and nothing logs a crash before the host is built.
- **OpenTelemetry logging straight to a collector**: the long-term standard, but it needs a collector we don't run yet. Serilog can export to OpenTelemetry later without code changes.

## Consequences

- Code depends only on `ILogger<T>`. Swapping Serilog out again only touches Program.cs and configuration.
- Log levels live in the `Serilog` configuration section (`Serilog__MinimumLevel__Override__<category>`). The standard `Logging` section has no effect, so it's removed from the appsettings files.
- Each host gets its own logger (`preserveStaticLogger: true`), and the request-logging middleware is handed that logger. Otherwise hosts that run side by side, as the tests' `WebApplicationFactory` instances do, write into each other's sinks. Only the crash logging in Program.cs uses the global `Log.Logger`.
- Tests capture log events with an `ILogEventSink` registered in DI, which `ReadFrom.Services` adds to the app's logger.
