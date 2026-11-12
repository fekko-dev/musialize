using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;
using Shouldly;

namespace Musialize.Api.Tests.Errors;

public sealed class ErrorTests(ApiWithoutDatabaseFactory factory) : IClassFixture<ApiWithoutDatabaseFactory>
{
    [Fact]
    public async Task AnUnknownApiPathReturnsAProblemDetails404()
    {
        using var client = factory.CreateClient();

        var (statusCode, problem) = await SendForProblemAsync(client, HttpMethod.Get, TestEndpoints.Unknown);

        statusCode.ShouldBe(HttpStatusCode.NotFound);
        problem.Status.ShouldBe(404);
    }

    [Fact]
    public async Task AWrongMethodReturnsAProblemDetails405()
    {
        using var client = factory.CreateClient();

        // The throw endpoint only accepts GET.
        var (statusCode, problem) = await SendForProblemAsync(client, HttpMethod.Post, TestEndpoints.Throw);

        statusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        problem.Status.ShouldBe(405);
    }

    [Fact]
    public async Task AnUnexpectedExceptionReturnsAProblemDetails500AndIsLoggedOnce()
    {
        using var client = factory.CreateClient();
        factory.Logs.Clear();

        var (statusCode, problem) = await SendForProblemAsync(client, HttpMethod.Get, TestEndpoints.Throw);

        statusCode.ShouldBe(HttpStatusCode.InternalServerError);
        problem.Status.ShouldBe(500);
        var error = factory.Logs.Snapshot().Where(log => log.Level >= LogEventLevel.Error).ShouldHaveSingleItem();
        error.Exception.ShouldNotBeNull().Message.ShouldBe(TestEndpoints.ExceptionMessage);
    }

    [Fact]
    public async Task InDevelopmentA500IncludesTheExceptionDetails()
    {
        using var client = factory.CreateClient();

        var (_, problem) = await SendForProblemAsync(client, HttpMethod.Get, TestEndpoints.Throw);

        problem.Detail.ShouldNotBeNull().ShouldContain(TestEndpoints.ExceptionMessage);
    }

    [Fact]
    public async Task OutsideDevelopmentA500DoesNotIncludeTheExceptionDetails()
    {
        await using var production = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = production.CreateClient();

        using var response = await client.GetAsync(new Uri(TestEndpoints.Throw, UriKind.Relative), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        body.ShouldNotContain(TestEndpoints.ExceptionMessage);
        body.ShouldNotContain(nameof(InvalidOperationException));
    }

    private static async Task<(HttpStatusCode StatusCode, ProblemDetails Problem)> SendForProblemAsync(HttpClient client, HttpMethod method, string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(method, new Uri(path, UriKind.Relative));
        using var response = await client.SendAsync(request, cancellationToken);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        return (response.StatusCode, problem.ShouldNotBeNull());
    }
}
