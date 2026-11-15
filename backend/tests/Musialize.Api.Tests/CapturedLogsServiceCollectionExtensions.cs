using Musialize.Api.Tests;
using Serilog.Core;

// In this namespace, as ASP.NET Core's DI docs recommend, so callers need no extra using.
namespace Microsoft.Extensions.DependencyInjection;

public static class CapturedLogsServiceCollectionExtensions
{
    /// <summary>Captures the app's log events in a <see cref="CapturedLogs"/> singleton.</summary>
    public static IServiceCollection AddCapturedLogs(this IServiceCollection services) =>
        services
            .AddSingleton<CapturedLogs>()
            .AddSingleton<ILogEventSink>(provider => provider.GetRequiredService<CapturedLogs>());
}
