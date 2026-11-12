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

    [Fact]
    public async Task TheManifestInstallsTheAppWithTheDefaultBranding()
    {
        using var client = _defaults.CreateClient();

        var manifest = await GetManifestAsync(client);

        manifest.GetProperty("name").GetString().ShouldBe("Musialize");
        manifest.GetProperty("short_name").GetString().ShouldBe("Musialize");
        manifest.GetProperty("theme_color").GetString().ShouldBe("#ffffff");
        manifest.GetProperty("background_color").GetString().ShouldBe("#ffffff");
        manifest.GetProperty("display").GetString().ShouldBe("standalone");
        manifest.GetProperty("start_url").GetString().ShouldBe("/");
        manifest.GetProperty("icons").EnumerateArray()
            .Select(icon => (
                icon.GetProperty("src").GetString(),
                icon.GetProperty("sizes").GetString(),
                icon.GetProperty("type").GetString(),
                icon.GetProperty("purpose").GetString()))
            .ShouldBe([
                ("/branding/icon-192.png", "192x192", "image/png", "any"),
                ("/branding/icon-512.png", "512x512", "image/png", "any"),
                ("/branding/icon-maskable-512.png", "512x512", "image/png", "maskable"),
            ]);
    }

    [Fact]
    public async Task TheManifestUsesTheConfiguredBranding()
    {
        await using var configured = Configured();
        using var client = configured.CreateClient();

        var manifest = await GetManifestAsync(client);

        manifest.GetProperty("name").GetString().ShouldBe("The Test Group");
        manifest.GetProperty("short_name").GetString().ShouldBe("Test Group");
        manifest.GetProperty("theme_color").GetString().ShouldBe("#123456");
    }

    private WebApplicationFactory<Program> Configured() =>
        _defaults.WithWebHostBuilder(builder => builder
            .UseSetting("Branding:AppName", "The Test Group")
            .UseSetting("Branding:ShortName", "Test Group")
            .UseSetting("Branding:ThemeColor", "#123456"));

    private static async Task<JsonElement> GetManifestAsync(HttpClient client)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var response = await client.GetAsync(new Uri("/manifest.webmanifest", UriKind.Relative), cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/manifest+json");
        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    }

    private static async Task<JsonElement> GetJsonAsync(HttpClient client, string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    }

    public void Dispose() => _defaults.Dispose();
}
