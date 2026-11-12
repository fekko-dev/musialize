using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Shouldly;

namespace Musialize.Api.Tests.Logging;

// Redirecting the console is global, so these tests run on their own, after the parallel ones.
[CollectionDefinition(nameof(ConsoleOutput), DisableParallelization = true)]
public sealed class ConsoleOutput;

[Collection(nameof(ConsoleOutput))]
public sealed class ConsoleFormatTests
{
    [Fact]
    public async Task ProductionLogsAreJsonLinesWithTheTraceId()
    {
        var lines = await ConsoleLinesForAnUnknownPathAsync("Production");

        lines.ShouldNotBeEmpty();
        foreach (var line in lines)
        {
            Should.NotThrow(() => JsonDocument.Parse(line).Dispose(), $"Not JSON: {line}");
        }
        var request = lines
            .Where(line => line.Contains($"\"RequestPath\":\"{TestEndpoints.Unknown}\"", StringComparison.Ordinal))
            .ShouldHaveSingleItem();
        request.ShouldContain("\"@tr\":");
    }

    [Fact]
    public async Task DevelopmentLogsAreReadableLines()
    {
        var lines = await ConsoleLinesForAnUnknownPathAsync("Development");

        lines.ShouldContain(line => line.Contains($"INF] HTTP GET {TestEndpoints.Unknown} responded 404 in ", StringComparison.Ordinal));
    }

    private static async Task<string[]> ConsoleLinesForAnUnknownPathAsync(string environment)
    {
        var original = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);
        try
        {
            await using var factory = new ApiWithoutDatabaseFactory().WithWebHostBuilder(builder => builder.UseEnvironment(environment));
            using var client = factory.CreateClient();
            using var response = await client.GetAsync(new Uri(TestEndpoints.Unknown, UriKind.Relative), TestContext.Current.CancellationToken);
        }
        finally
        {
            Console.SetOut(original);
        }

        return output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }
}
