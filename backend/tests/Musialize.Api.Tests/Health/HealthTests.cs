using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Musialize.Api.Tests.Health;

public sealed class HealthTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task HealthReportsHealthyWithTheVersionTheApiWasBuiltWith()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        body.GetProperty("status").GetString().ShouldBe("healthy");
        body.GetProperty("version").GetString().ShouldBe(BuiltVersion());
    }

    // The SHA passed in with -p:AppVersion=<sha>, or "dev" when nothing is passed in.
    private static string BuiltVersion()
    {
        var version = typeof(Program).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AppVersion")
            .Value;
        version.ShouldNotBeNullOrWhiteSpace();
        return version;
    }
}
