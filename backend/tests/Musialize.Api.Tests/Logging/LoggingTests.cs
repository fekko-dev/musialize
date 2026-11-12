using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Shouldly;

namespace Musialize.Api.Tests.Logging;

public sealed class LoggingTests(ApiWithoutDatabaseFactory factory) : IClassFixture<ApiWithoutDatabaseFactory>
{
    [Fact]
    public async Task EachRequestIsLoggedWithItsMethodPathStatusCodeAndDuration()
    {
        using var client = factory.CreateClient();
        factory.Logs.Clear();

        using var response = await client.GetAsync(new Uri(TestEndpoints.Unknown, UriKind.Relative), TestContext.Current.CancellationToken);

        var request = RequestLogFor(factory.Logs, TestEndpoints.Unknown).ShouldHaveSingleItem();
        request.Properties["RequestMethod"].ToString().ShouldBe("\"GET\"");
        request.Properties["StatusCode"].ToString().ShouldBe("404");
        request.Properties.ShouldContainKey("Elapsed");
    }

    [Fact]
    public async Task HeadersCookiesQueryValuesAndBodiesAreNeverLogged()
    {
        await using var production = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = production.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{TestEndpoints.Unknown}?code=secret-query", UriKind.Relative))
        {
            Content = JsonContent.Create(new { name = "secret-body" }),
        };
        request.Headers.Add("Cookie", "session=secret-cookie");
        request.Headers.Add("X-Custom", "secret-header");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var logs = production.Services.GetRequiredService<CapturedLogs>();
        RequestLogFor(logs, TestEndpoints.Unknown).ShouldNotBeEmpty();
        foreach (var log in logs.Snapshot())
        {
            CapturedLogs.AsJson(log).ShouldNotContain("secret");
        }
    }

    [Fact]
    public async Task LogLevelsCanBeSetByConfiguration()
    {
        await using var quiet = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Serilog:MinimumLevel:Override:Serilog.AspNetCore.RequestLoggingMiddleware", "Warning"));
        using var client = quiet.CreateClient();
        var logs = quiet.Services.GetRequiredService<CapturedLogs>();

        using var response = await client.GetAsync(new Uri(TestEndpoints.Unknown, UriKind.Relative), TestContext.Current.CancellationToken);

        RequestLogFor(logs, TestEndpoints.Unknown).ShouldBeEmpty();
    }

    internal static IEnumerable<LogEvent> RequestLogFor(CapturedLogs logs, string path) =>
        logs.Snapshot().Where(log =>
            log.MessageTemplate.Text.StartsWith("HTTP ", StringComparison.Ordinal)
            && log.Properties.TryGetValue("RequestPath", out var requestPath)
            && requestPath.ToString() == $"\"{path}\"");
}
