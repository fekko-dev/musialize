using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Shouldly;

namespace Musialize.Api.Tests.Validation;

public sealed class ValidationTests(ApiWithoutDatabaseFactory factory) : IClassFixture<ApiWithoutDatabaseFactory>
{
    [Fact]
    public async Task AnInvalidApplicationRequestIsRejectedWithTheFieldErrors()
    {
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.PostAsJsonAsync(TestEndpoints.Validate, new { name = (string?)null }, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken);
        problem.ShouldNotBeNull().Errors.Keys.ShouldBe(["Name"]);
    }

    // In development, minimal APIs throw on a body they can't read instead of answering 400 themselves.
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task ABodyThatIsNotJsonIsAProblemDetails400(string environment)
    {
        await using var app = factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        using var client = app.CreateClient();
        using var content = new StringContent("{ not json", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(new Uri(TestEndpoints.Validate, UriKind.Relative), content, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        (await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken)).ShouldNotBeNull().Status.ShouldBe(400);
        app.Services.GetRequiredService<CapturedLogs>().Snapshot().ShouldNotContain(log => log.Level >= LogEventLevel.Error);
    }

    [Fact]
    public async Task AValidApplicationRequestReachesTheEndpoint()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(TestEndpoints.Validate, new { name = "Valid" }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
