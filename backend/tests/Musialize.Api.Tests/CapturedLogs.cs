using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Musialize.Api.Tests;

/// <summary>
/// Collects every log event the app writes. Registered as a Serilog sink in DI,
/// which the app's logger picks up through <c>ReadFrom.Services</c>.
/// </summary>
public sealed class CapturedLogs : ILogEventSink
{
    private readonly ConcurrentQueue<LogEvent> _events = new();

    public void Emit(LogEvent logEvent) => _events.Enqueue(logEvent);

    public IReadOnlyList<LogEvent> Snapshot() => [.. _events];

    public void Clear() => _events.Clear();

    /// <summary>The event as the production console writes it: JSON with its message and every property.</summary>
    public static string AsJson(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        new RenderedCompactJsonFormatter().Format(logEvent, writer);
        return writer.ToString();
    }
}

public static class CapturedLogsRegistration
{
    /// <summary>Captures the app's log events in a <see cref="CapturedLogs"/> singleton.</summary>
    public static IServiceCollection AddCapturedLogs(this IServiceCollection services) =>
        services
            .AddSingleton<CapturedLogs>()
            .AddSingleton<ILogEventSink>(provider => provider.GetRequiredService<CapturedLogs>());
}
