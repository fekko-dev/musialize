using System.Reflection;
using NetArchTest.Rules;
using Shouldly;

namespace Musialize.Api.Tests.Architecture;

// Covers the parts of the dependency rule (ADR-0008) that project references can't enforce.
public sealed class DependencyRuleTests
{
    [Fact]
    public void DomainDoesNotDependOnEfCore()
    {
        var result = Types.InAssembly(Assembly.Load("Musialize.Domain"))
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        ShouldPass(result);
    }

    // Handlers may use EF Core LINQ, but PostgreSQL details stay in Infrastructure (ADR-0010).
    [Fact]
    public void ApplicationDoesNotDependOnNpgsql()
    {
        var result = Types.InAssembly(Assembly.Load("Musialize.Application"))
            .ShouldNot()
            .HaveDependencyOn("Npgsql")
            .GetResult();

        ShouldPass(result);
    }

    // The startup code is Program.cs, whose top-level statements compile into the global namespace.
    // Everything else in Api lives under Musialize.Api and must not use Infrastructure types.
    [Fact]
    public void ApiUsesInfrastructureOnlyInStartupCode()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespaceStartingWith("Musialize.Api")
            .ShouldNot()
            .HaveDependencyOn("Musialize.Infrastructure")
            .GetResult();

        ShouldPass(result);
    }

    private static void ShouldPass(NetArchTest.Rules.TestResult result) =>
        result.IsSuccessful.ShouldBeTrue($"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
