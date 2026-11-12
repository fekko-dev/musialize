using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Musialize.Api.Tests.Spa;

// The built SPA isn't part of a test build, so these tests serve a stub index.html. They run
// as Production, like the image: in Development, ASP.NET Core turns off caching for every file.
public sealed class SpaRoutingTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseWebRoot(Path.Combine(AppContext.BaseDirectory, "Spa", "wwwroot")));

    [Fact]
    public async Task AnUnknownPathOutsideTheApiReturnsTheSpa()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/groups/some-group", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("text/html");
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain("Stub SPA");
    }

    // After a deploy, a cached index.html would point at assets that no longer exist.
    [Fact]
    public async Task TheSpaIsRevalidatedOnEveryLoad()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl.ShouldNotBeNull().NoCache.ShouldBeTrue();
    }

    [Fact]
    public async Task AnUnknownApiPathReturnsAProblemDetailsNotFoundInsteadOfTheSpa()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/does-not-exist", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        problem.GetProperty("status").GetInt32().ShouldBe(404);
    }

    // The SPA fallback takes only GET and HEAD, so it doesn't swallow the 405.
    [Fact]
    public async Task AWrongMethodOnAnEndpointIsStillA405()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsync(new Uri("/health", UriKind.Relative), content: null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    public void Dispose() => _factory.Dispose();
}
