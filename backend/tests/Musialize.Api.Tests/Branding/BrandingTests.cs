using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Musialize.Api.Tests.Branding;

public sealed class BrandingTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _defaults = new();

    [Fact]
    public async Task BrandingDefaultsToMusialize()
    {
        using var client = _defaults.CreateClient();

        var branding = await GetJsonAsync(client, "/api/branding");

        branding.GetProperty("appName").GetString().ShouldBe("Musialize");
        branding.GetProperty("shortName").GetString().ShouldBe("Musialize");
    }

    [Fact]
    public async Task BrandingComesFromTheDeploymentSettings()
    {
        await using var configured = Configured();
        using var client = configured.CreateClient();

        var branding = await GetJsonAsync(client, "/api/branding");

        branding.GetProperty("appName").GetString().ShouldBe("The Test Group");
        branding.GetProperty("shortName").GetString().ShouldBe("Test Group");
    }

    private WebApplicationFactory<Program> Configured() =>
        _defaults.WithWebHostBuilder(builder => builder
            .UseSetting("Branding:AppName", "The Test Group")
            .UseSetting("Branding:ShortName", "Test Group"));

    private static async Task<JsonElement> GetJsonAsync(HttpClient client, string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    }

    public void Dispose() => _defaults.Dispose();
}
