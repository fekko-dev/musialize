using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Musialize.Api.Tests.Branding;

public sealed class BrandingTests : IDisposable
{
    private static readonly string[] AssetFiles =
        ["logo.svg", "favicon.ico", "icon-192.png", "icon-512.png", "icon-maskable-512.png", "apple-touch-icon.png"];

    private readonly WebApplicationFactory<Program> _defaults = new();

    // A Group's branding folder, as mounted on the server. Each file holds its own name, so a
    // test can tell which file it got.
    private readonly DirectoryInfo _configuredAssets = Directory.CreateTempSubdirectory("musialize-branding-");

    public BrandingTests()
    {
        foreach (var file in AssetFiles)
        {
            File.WriteAllText(Path.Combine(_configuredAssets.FullName, file), $"configured {file}");
        }
    }

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

    [Theory]
    [InlineData("logo.svg", "image/svg+xml")]
    [InlineData("favicon.ico", "image/x-icon")]
    [InlineData("icon-192.png", "image/png")]
    [InlineData("icon-512.png", "image/png")]
    [InlineData("icon-maskable-512.png", "image/png")]
    [InlineData("apple-touch-icon.png", "image/png")]
    public async Task AnAssetDefaultsToMusializesOwn(string file, string contentType)
    {
        using var client = _defaults.CreateClient();

        using var response = await GetAsync(client, $"/branding/{file}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe(contentType);
        (await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken))
            .ShouldBe(await File.ReadAllBytesAsync(Path.Combine(DefaultAssetsInTheRepository, file), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AnAssetComesFromTheConfiguredFolder()
    {
        await using var configured = Configured();
        using var client = configured.CreateClient();

        using var response = await GetAsync(client, "/branding/icon-192.png");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("image/png");
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBe("configured icon-192.png");
    }

    // Only the fixed filenames are served, never anything else that is in the folder.
    [Fact]
    public async Task AnyOtherFileInTheFolderIsNotFound()
    {
        File.WriteAllText(Path.Combine(_configuredAssets.FullName, "notes.txt"), "not for the browser");
        await using var configured = Configured();
        using var client = configured.CreateClient();

        using var response = await GetAsync(client, "/branding/notes.txt");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // So a deploy with a broken mount never becomes healthy, and Dokploy rolls it back. The
    // app logs why and stops (see Program.cs), so the test only sees that it didn't start; the
    // tests above start the same folder with every file in it.
    [Fact]
    public void TheAppDoesNotStartWhenTheConfiguredFolderMissesAFile()
    {
        File.Delete(Path.Combine(_configuredAssets.FullName, "apple-touch-icon.png"));
        using var configured = Configured();

        Should.Throw<Exception>(configured.CreateClient);
    }

    // The committed defaults, read from the source tree rather than from what the build copied.
    private static string DefaultAssetsInTheRepository => Path.GetFullPath(
        Path.Combine(ThisFilesFolder(), "../../../src/Musialize.Api/Branding/Defaults"));

    // Where this file is in the repository, whatever the build's output folder is.
    private static string ThisFilesFolder([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;

    private WebApplicationFactory<Program> Configured() =>
        _defaults.WithWebHostBuilder(builder => builder
            .UseSetting("Branding:AppName", "The Test Group")
            .UseSetting("Branding:ShortName", "Test Group")
            .UseSetting("Branding:ThemeColor", "#123456")
            .UseSetting("Branding:AssetsPath", _configuredAssets.FullName));

    private static Task<HttpResponseMessage> GetAsync(HttpClient client, string path) =>
        client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);

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

    public void Dispose()
    {
        _defaults.Dispose();
        _configuredAssets.Delete(recursive: true);
    }
}
