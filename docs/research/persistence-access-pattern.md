# How use-case handlers reach the database

Research date: 2026-11-11. Sources are Microsoft Learn (EF Core docs, the EF Core API reference, the ASP.NET Core 10 error-handling docs, and the .NET microservices architecture e-book), the source of `jasontaylordev/CleanArchitecture` (commit [`e1ab4cc`](https://github.com/jasontaylordev/CleanArchitecture/tree/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04)), `ardalis/CleanArchitecture` (commit [`da5491f`](https://github.com/ardalis/CleanArchitecture/tree/da5491f1641f3d388e6cfc2c0ddd29f763746762)) and `ardalis/Specification` (commit [`fc2670a`](https://github.com/ardalis/Specification/tree/fc2670ac94d3ac427b232cc15ee0e1a7d2748a1b)), the Npgsql docs and API reference, the EntityFramework.Exceptions README, and NuGet package metadata. Milan Jovanović's blog is used once, as opinion. Points that a source does not state, or that I inferred, are marked ⚠️.

Context: [ADR-0008](../adr/0008-four-layer-clean-architecture-with-use-case-handlers.md) splits the backend into `Domain` ← `Application` ← `Infrastructure` and `Api`, with one handler class per use case and no mediator. It leaves open how handlers reach the database, and names two candidates: an `IAppDbContext` in Application, or repositories plus read-side query interfaces. Either way, an aggregate always loads complete, and Domain never references EF Core. Handlers are tested over HTTP (`WebApplicationFactory`, PostgreSQL in Testcontainers, Respawn). There is no Application test project. The stack is EF Core 10 with Npgsql and snake_case naming. The [engineering principles](../product/vision.md#engineering-principles) say layers and abstractions are added only when they solve a concrete problem. One developer, pilot on 2026-11-12.

The example use case throughout is **Claim a Member**: load the Unclaimed Member by ID within a Group, check that the signed-in Account has no Member in this Group yet, link it with a domain method on Member, and save. A unique index on `(group_id, account_id)` makes the second of two concurrent Claims fail.

## 1. Summary and recommendation

**Use option (a): a small `IAppDbContext` interface in Application that exposes one `DbSet<T>` per aggregate root and `SaveChangesAsync`. `MusializeDbContext` in Infrastructure implements it. Application references `Microsoft.EntityFrameworkCore`, but not Npgsql. `MusializeDbContext` translates PostgreSQL conflicts into an Application `ConflictException`, which the Api maps to 409.**

1. **It is the least code that keeps ADR-0008's layer rules.** The DbContext, migrations, Npgsql and the naming convention stay in Infrastructure, where ADR-0008 puts "PostgreSQL through EF Core". The interface is a few lines plus one line per aggregate root. This is exactly what Jason Taylor's template does: its `IApplicationDbContext` has two `DbSet`s and `SaveChangesAsync`, and its Application project references `Microsoft.EntityFrameworkCore` ([IApplicationDbContext.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Application/Common/Interfaces/IApplicationDbContext.cs), [Application.csproj](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Application/Application.csproj)).
2. **No abstraction buys testability under our test strategy.** We test handlers over HTTP against real PostgreSQL. Microsoft recommends starting exactly there, and says to avoid mocking `DbSet` for queries and to avoid the in-memory provider ([Choosing a testing strategy: Summary](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#summary)). Only repositories give reliable test doubles, and Microsoft says they have "a significant cost in terms of implementation and maintenance" ([Different types of test doubles](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#different-types-of-test-doubles)). We don't plan those tests, so we would pay that cost for nothing. Jason Taylor's template doesn't mock its interface either: every handler test runs against a real database through `WebApplicationFactory` and Respawn (§3).
3. **The interface does solve one concrete problem: the conflict translation.** Because the DbContext lives in Infrastructure, it can catch `PostgresException` with SQLSTATE `23505` and rethrow an exception type that Application owns. Application never sees Npgsql. In option (c), where the DbContext lives in Application, that catch has to move to the Api (§5).
4. **Read-side queries stay plain LINQ projections in the handler**, with no extra query interface (§7).

**Trade-offs we accept.** Application references EF Core, so `IQueryable`, `Include`, `ToListAsync` and `DbUpdateException` are usable in handlers. Nothing enforces "load an aggregate complete" except configuration (`AutoInclude`) and review (§4). If we ever want Application unit tests with test doubles, (a)→(b) is a rewrite of each handler's data access, not a rename (§6).

**Runner-up: (c2), the DbContext itself in Application, no interface.** It is the simplest code: handlers take `MusializeDbContext` directly. Choose it if the one-file interface starts to feel like ceremony. The price: entity configuration moves into Application, migrations need an explicit `MigrationsAssembly`, and conflict translation moves to an Api exception handler that reads Npgsql types (§5). Moving from (c2) to (a) later is mechanical.

**Not now: (b), repositories plus query interfaces.** It is the only option that keeps EF Core out of Application and allows real handler unit tests. Under ADR-0008 we write no such tests, so it doubles the files per use case with no payoff today.

**Watch out (applies to every option).** The unique index on `(group_id, account_id)` stops one Account from claiming *two* Members of a Group at the same time. It does **not** stop *two* Accounts from claiming the *same* Unclaimed Member at the same time. Both read `account_id IS NULL`, both update the same row, and the last write wins silently ⚠️ (inferred from how the index and an UPDATE work). That race needs a concurrency token on Member. With Npgsql that is the `xmin` system column, so the second save throws `DbUpdateConcurrencyException`, which then maps to 409 like the unique violation (§5).

## 2. Comparison

Legend: ✅ yes / good fit · 🟡 partial / caveat · ❌ no / poor fit

| | (a) `IAppDbContext` in Application | (b) Repository per aggregate + query interfaces | (c1) Application references Infrastructure | (c2) `MusializeDbContext` lives in Application |
|---|---|---|---|---|
| Keeps ADR-0008 project references | ✅ | ✅ | ❌ creates a reference cycle ⚠️ | 🟡 references fine; EF mapping moves into Application |
| Application references EF Core | 🟡 `Microsoft.EntityFrameworkCore` | ✅ no | ❌ yes, plus Npgsql transitively ⚠️ | 🟡 `Microsoft.EntityFrameworkCore` |
| Application free of Npgsql | ✅ | ✅ | ❌ | ✅ |
| Extra code per use case | ✅ none | ❌ repository method(s) + query interface + implementation | ✅ none | ✅ none |
| Buys testability under our HTTP-only strategy | ❌ no | ❌ no (yes only if we add Application unit tests) | ❌ no | ❌ no |
| Microsoft-approved test doubles if needed later | ❌ mocking `DbSet` queries is advised against | ✅ the recommended double | ❌ | ❌ |
| Where 23505 / concurrency → 409 is translated | ✅ `MusializeDbContext.SaveChangesAsync` (Infrastructure) | ✅ same, behind `IUnitOfWork` | 🟡 anywhere, because layers are gone | 🟡 Api exception handler reading Npgsql types |
| "Aggregate loads complete" enforced | 🟡 `AutoInclude` + review | ✅ repository method owns the `Include`s | 🟡 | 🟡 |
| Read-side list query | ✅ LINQ `Select` in handler | 🟡 interface + implementation per query | ✅ | ✅ |
| Switching to another option later | 🟡 (a)→(b) rewrites each handler's data access | 🟡 (b)→(a) mechanical inlining | — | ✅ (c2)→(a) mechanical |
| Used by a reference template | ✅ Jason Taylor | ✅ Ardalis | ❌ | 🟡 Microsoft docs inject the context directly ⚠️ not as a layered template |

## 3. What the sources say

### 3.1 Microsoft: testing and repositories

- Microsoft lists four kinds of test double: SQLite in-memory, the EF in-memory provider, mocking `DbContext`/`DbSet`, and a repository layer. It says "a repository layer is the only approach allowing the comprehensive and reliable stubbing/mocking of the data layer", but that it "has a significant cost in terms of implementation and maintenance" ([Different types of test doubles](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#different-types-of-test-doubles)).
- Mocking `DbSet` queries is "not possible" in a real sense, because LINQ operators are static extension methods over `IQueryable`. What people call "mocking `DbSet`" is an in-memory fake with the same drawbacks as the in-memory provider, and "should generally be avoided for testing any query code" ([Mocking or stubbing DbContext and DbSet](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#mocking-or-stubbing-dbcontext-and-dbset)). So an `IAppDbContext` doesn't make handlers mockable in any useful way.
- The in-memory provider is "highly discouraged" as a database fake ([In-memory as a database fake](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#in-memory-as-a-database-fake)), and the summary says it is "only supported for legacy applications" ([Summary](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#summary)).
- The same summary recommends good coverage "against their actual production database system" first, and test doubles later "if needed". It also names Testcontainers as a way to make that easy ([Testing against the database may be easier than it seems](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#testing-against-the-database-may-be-easier-than-it-seems)). That is our current strategy.
- For a repository to be stubbable, it "cannot expose any IQueryable-returning methods" ([Repository pattern](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#repository-pattern), [Testing without your production database system](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#repository-pattern)).
- The microservices e-book says "EF DbContext implements both the Repository and the Unit of Work patterns" ([Designing the infrastructure persistence layer](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)). It recommends one repository per aggregate root, allows queries "through other channels" as in CQRS, and ends with "Repositories shouldn't be mandatory", quoting Jimmy Bogard: "I still need to have that integration test with the real thing" ([same page, Repositories shouldn't be mandatory](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design#repositories-shouldnt-be-mandatory)).
- The implementation chapter: "In cases where you want the simplest code possible, you might want to directly use the DbContext class, as many developers do." Repositories are recommended for "more complex microservices", mainly for mocking ([Using a custom repository versus using EF DbContext directly](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation-entity-framework-core#using-a-custom-repository-versus-using-ef-dbcontext-directly)). ⚠️ This e-book chapter dates from 2021 and targets microservices, not a single small app.

### 3.2 DbContext lifetime

- "A `DbContext` instance is designed to be used for a *single* unit-of-work." `AddDbContext` registers it as scoped, so in ASP.NET Core each request gets its own instance. It is not thread-safe ([DbContext lifetime](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#the-dbcontext-lifetime), [DbContext in dependency injection](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#dbcontext-in-dependency-injection-for-aspnet-core)).
- Consequence for all options: one handler call is one request scope and one unit of work. Repositories in (b) must also be scoped so they share the request's context ([repository instance lifetime](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation-entity-framework-core#the-repository-instance-lifetime-in-your-ioc-container)).

### 3.3 Jason Taylor's template (option a)

- `IApplicationDbContext` exposes `DbSet<TodoList>`, `DbSet<TodoItem>` and `SaveChangesAsync` ([source](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Application/Common/Interfaces/IApplicationDbContext.cs)). `ApplicationDbContext` in Infrastructure implements it, and DI resolves the interface to the same scoped instance ([ApplicationDbContext.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Infrastructure/Data/ApplicationDbContext.cs), [DependencyInjection.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Infrastructure/DependencyInjection.cs)).
- The Application project references the **full** `Microsoft.EntityFrameworkCore` package and has `global using Microsoft.EntityFrameworkCore;` ([Application.csproj](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Application/Application.csproj), [GlobalUsings.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Application/GlobalUsings.cs)).
- Handlers use EF directly: `_context.TodoItems.Add(entity)` then `SaveChangesAsync` ([CreateTodoItem.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Application/TodoItems/Commands/CreateTodoItem/CreateTodoItem.cs)). Queries use `AsNoTracking`, AutoMapper's `ProjectTo` and `ToListAsync` ([GetTodos.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Application/TodoLists/Queries/GetTodos/GetTodos.cs)).
- **Handler tests never mock the interface.** `Application.UnitTests` only covers a logging behaviour, an exception type and AutoMapper mappings. Every handler test sits in `Application.FunctionalTests`, which boots the web app (`WebApiFactory`) and resets a real database with Respawn ([tests folder](https://github.com/jasontaylordev/CleanArchitecture/tree/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/tests), [CreateTodoItemTests.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/tests/Application.FunctionalTests/TodoItems/Commands/CreateTodoItemTests.cs)). So in the template, the interface serves layering, not mocking.
- Its exception handler maps only Application exceptions (validation, not found, unauthorized, forbidden) to status codes. There is no database-conflict mapping ([ProblemDetailsExceptionHandler.cs](https://github.com/jasontaylordev/CleanArchitecture/blob/e1ab4cc4d5d7b4e25e57bd8b0514b766573b8c04/src/Web/Infrastructure/ProblemDetailsExceptionHandler.cs)).

### 3.4 Ardalis Clean Architecture and Ardalis.Specification (option b)

- `IRepository<T>` and `IReadRepository<T>` come from the `Ardalis.SharedKernel` package and are constrained to `IAggregateRoot`. `IReadRepository` is documented as "primarily to fetch trackable domain entities, not for custom queries" ([IRepository.cs](https://github.com/ardalis/Ardalis.SharedKernel/blob/main/src/Ardalis.SharedKernel/IRepository.cs), [IReadRepository.cs](https://github.com/ardalis/Ardalis.SharedKernel/blob/main/src/Ardalis.SharedKernel/IReadRepository.cs)).
- The Core project references `Ardalis.Specification` but no EF Core package. The UseCases project references only Core and `Ardalis.Result` ([Core.csproj](https://github.com/ardalis/CleanArchitecture/blob/da5491f1641f3d388e6cfc2c0ddd29f763746762/src/Clean.Architecture.Core/Clean.Architecture.Core.csproj), [UseCases.csproj](https://github.com/ardalis/CleanArchitecture/blob/da5491f1641f3d388e6cfc2c0ddd29f763746762/src/Clean.Architecture.UseCases/Clean.Architecture.UseCases.csproj)). Specifications still express `Include` and `OrderBy`, through Ardalis's own API ([use-built-in-abstract-repository.md](https://github.com/ardalis/Specification/blob/fc2670ac94d3ac427b232cc15ee0e1a7d2748a1b/docs/usage/use-built-in-abstract-repository.md)).
- `EfRepository<T>` is a one-line subclass of `RepositoryBase<T>` ([EfRepository.cs](https://github.com/ardalis/CleanArchitecture/blob/da5491f1641f3d388e6cfc2c0ddd29f763746762/src/Clean.Architecture.Infrastructure/Data/EfRepository.cs)). **`RepositoryBase.AddAsync` and `UpdateAsync` each call `SaveChangesAsync` immediately** ([RepositoryBaseOfT.cs](https://github.com/ardalis/Specification/blob/fc2670ac94d3ac427b232cc15ee0e1a7d2748a1b/src/Ardalis.Specification.EntityFrameworkCore/RepositoryBaseOfT.cs)). So each repository call is its own save, unless you write your own repository.
- Read side: a per-query interface in UseCases (`IListContributorsQueryService`) implemented in Infrastructure with EF or raw SQL ([interface](https://github.com/ardalis/CleanArchitecture/blob/da5491f1641f3d388e6cfc2c0ddd29f763746762/src/Clean.Architecture.UseCases/Contributors/List/IListContributorsQueryService.cs), [implementation](https://github.com/ardalis/CleanArchitecture/blob/da5491f1641f3d388e6cfc2c0ddd29f763746762/src/Clean.Architecture.Infrastructure/Data/Queries/ListContributorsQueryService.cs)).
- Unlike Jason Taylor's template, Ardalis does unit test handlers with a mocked repository (`Substitute.For<IRepository<Contributor>>()`) ([CreateContributorHandlerHandle.cs](https://github.com/ardalis/CleanArchitecture/blob/da5491f1641f3d388e6cfc2c0ddd29f763746762/tests/Clean.Architecture.UnitTests/UseCases/Contributors/CreateContributorHandlerHandle.cs)). That is the payoff of (b), and it only matters if you write such tests.
- Both templates are MIT-licensed and were pushed this month (GitHub repository metadata).

### 3.5 Opinion: Milan Jovanović

Secondary source, opinion. His post (2026-08-30) treats repositories as optional. For mostly-CRUD apps: "Just use DbContext directly." He also advises against generic repositories, against returning `IQueryable` from them, and against repositories calling `SaveChanges` ([Repository Pattern in C# With EF Core](https://milanjovanovic.tech/blog/repository-pattern-csharp)). That last point contradicts `Ardalis.Specification`'s `RepositoryBase` (§3.4).

## 4. Option by option

All four options share the same Domain method and endpoint. ⚠️ The code below is my sketch, not taken from a source. ⚠️ It assumes Member is its own aggregate root, not part of a Group aggregate. If Member lives inside Group, the handler loads the Group instead, but the persistence story is the same.

```csharp
// Domain/Members/Member.cs (shared)
public sealed class Member
{
    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid? AccountId { get; private set; }

    public void Claim(Guid accountId)
    {
        if (AccountId is not null) throw new MemberAlreadyClaimedException(Id);
        AccountId = accountId;
    }
}
```

The unique index allows many Unclaimed Members per Group, because PostgreSQL treats `NULL`s as distinct in a unique index by default ([PostgreSQL: Unique Constraints](https://www.postgresql.org/docs/current/ddl-constraints.html#DDL-CONSTRAINTS-UNIQUE-CONSTRAINTS)).

### 4.1 (a) `IAppDbContext` in Application

```csharp
// Application/Common/IAppDbContext.cs
public interface IAppDbContext
{
    DbSet<Member> Members { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// Application/Members/ClaimMemberHandler.cs
public sealed class ClaimMemberHandler(IAppDbContext db)
{
    public async Task<ClaimMemberResult> HandleAsync(
        Guid groupId, Guid memberId, Guid accountId, CancellationToken ct)
    {
        var member = await db.Members
            .SingleOrDefaultAsync(m => m.GroupId == groupId && m.Id == memberId, ct);
        if (member is null) return ClaimMemberResult.NotFound;

        if (await db.Members.AnyAsync(m => m.GroupId == groupId && m.AccountId == accountId, ct))
            return ClaimMemberResult.AlreadyInGroup;

        member.Claim(accountId);
        await db.SaveChangesAsync(ct); // a lost race throws ConflictException (§5)
        return ClaimMemberResult.Claimed;
    }
}

// Infrastructure/Persistence/MusializeDbContext.cs
public sealed class MusializeDbContext(DbContextOptions<MusializeDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<Member> Members => Set<Member>();
    // OnModelCreating + SaveChangesAsync override: see §5
}
```

- **Package:** Application needs `Microsoft.EntityFrameworkCore`. `DbSet<T>` and `DbUpdateException` live in `Microsoft.EntityFrameworkCore.dll` ([DbUpdateException API](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbupdateexception?view=efcore-10.0)). `Microsoft.EntityFrameworkCore.Abstractions` is not enough: it only holds mapping attributes such as `[Index]`, `[Owned]` and `[Keyless]`, and no `DbSet` or `DbContext` ([EFCore.Abstractions source, release/10.0](https://github.com/dotnet/efcore/tree/release/10.0/src/EFCore.Abstractions), [NuGet description](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Abstractions/)).
- **What leaks:** `IQueryable` composition, `Include`, `AsNoTracking`, `ToListAsync`, `ExecuteUpdateAsync`, and any EF Core exception type. Provider-specific things do not leak: `UseNpgsql`, `PostgresException` and `EF.Functions.ILike` are Npgsql extensions, so Application can't call them without referencing Npgsql ⚠️ (inferred from where the types live). That is a useful guard rail.
- **Complete aggregates:** nothing stops a handler from forgetting an `Include`. Configure owned or child navigations with `Navigation(...).AutoInclude()`, so every query for the root loads them ([Model configuration for auto-including navigations](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager#model-configuration-for-auto-including-navigations)). Owned types are auto-included by convention (same page). Expose a `DbSet` only for aggregate roots.
- **Testability:** unchanged. Handlers are tested over HTTP. The interface is not a useful mock seam (§3.1).
- **Architecture test:** ADR-0008's test that Domain has no EF Core reference still holds.

### 4.2 (b) Repository per aggregate root plus query interfaces

```csharp
// Application/Members/IMemberRepository.cs
public interface IMemberRepository
{
    Task<Member?> GetAsync(Guid groupId, Guid memberId, CancellationToken ct);
    Task<bool> AccountHasMemberInGroupAsync(Guid groupId, Guid accountId, CancellationToken ct);
}
public interface IUnitOfWork { Task SaveChangesAsync(CancellationToken ct); }

// Application/Members/ClaimMemberHandler.cs
public sealed class ClaimMemberHandler(IMemberRepository members, IUnitOfWork uow)
{
    public async Task<ClaimMemberResult> HandleAsync(
        Guid groupId, Guid memberId, Guid accountId, CancellationToken ct)
    {
        var member = await members.GetAsync(groupId, memberId, ct);
        if (member is null) return ClaimMemberResult.NotFound;
        if (await members.AccountHasMemberInGroupAsync(groupId, accountId, ct))
            return ClaimMemberResult.AlreadyInGroup;

        member.Claim(accountId);
        await uow.SaveChangesAsync(ct);
        return ClaimMemberResult.Claimed;
    }
}

// Infrastructure/Persistence/MemberRepository.cs
public sealed class MemberRepository(MusializeDbContext db) : IMemberRepository
{
    public Task<Member?> GetAsync(Guid groupId, Guid memberId, CancellationToken ct) =>
        db.Members.SingleOrDefaultAsync(m => m.GroupId == groupId && m.Id == memberId, ct);

    public Task<bool> AccountHasMemberInGroupAsync(Guid groupId, Guid accountId, CancellationToken ct) =>
        db.Members.AnyAsync(m => m.GroupId == groupId && m.AccountId == accountId, ct);
}
// MusializeDbContext implements IUnitOfWork, with the same translation as (a)
```

- **Package:** Application has no EF Core reference. Nothing leaks, as long as repositories return entities or `IEnumerable`/`IAsyncEnumerable`, never `IQueryable` (§3.1).
- **Save semantics:** keep `SaveChanges` in a unit of work, so one use case is one save. Don't adopt `Ardalis.Specification`'s `RepositoryBase` as is, because its `AddAsync`/`UpdateAsync` save immediately (§3.4).
- **Cost:** each new query need adds a repository method, or a specification class. The Claim use case already needs two methods that exist only for this handler.
- **Testability:** this is the only option where `Substitute.For<IMemberRepository>()` unit tests are reliable. But ADR-0008 says handlers are tested through the API "not with fakes of our own modules". We would still need the HTTP tests for the queries themselves, as Microsoft notes ([Repository pattern](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#repository-pattern)).

### 4.3 (c) Handlers depend on the concrete `MusializeDbContext`

There are two ways to do this.

**(c1) Application references Infrastructure.** Infrastructure already references Application (to implement email, SignalR and clock interfaces), so this creates a project reference cycle, which MSBuild doesn't allow ⚠️ (general MSBuild behaviour, not checked against a specific doc). To make it work, Infrastructure would have to stop referencing Application. The email, SignalR and clock interfaces would then move into Infrastructure, and Application would depend on MailKit, SignalR and Npgsql transitively. That reverses ADR-0008, so it isn't a real candidate.

**(c2) The DbContext lives in Application.** Application references only `Microsoft.EntityFrameworkCore`. Infrastructure (or Api) registers it with `UseNpgsql(...)`, `UseSnakeCaseNamingConvention()` and `MigrationsAssembly("Musialize.Infrastructure")`.

```csharp
// Application/Persistence/MusializeDbContext.cs
public sealed class MusializeDbContext(DbContextOptions<MusializeDbContext> options) : DbContext(options)
{
    public DbSet<Member> Members => Set<Member>();

    protected override void OnModelCreating(ModelBuilder b) =>
        b.ApplyConfigurationsFromAssembly(typeof(MusializeDbContext).Assembly);
}

// Application/Members/ClaimMemberHandler.cs: identical to (a), with
// `MusializeDbContext db` instead of `IAppDbContext db`.

// Infrastructure/DependencyInjection.cs
services.AddDbContext<MusializeDbContext>(o => o
    .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly("Musialize.Infrastructure"))
    .UseSnakeCaseNamingConvention());
```

- **Layer rules:** the references stay legal. But Application now holds EF mapping (`IEntityTypeConfiguration<T>` classes), which ADR-0008 assigns to Infrastructure. ⚠️ Mapping that needs Npgsql APIs, such as `xmin` with `IsRowVersion()` on a `uint` property, works with core EF APIs. Anything that calls an Npgsql-only fluent method would force Npgsql into Application.
- **Conflict translation:** `MusializeDbContext` can't catch `PostgresException` without Npgsql, so translation moves to the Api (§5).
- **Testability:** same as (a).

## 5. Unique violations and concurrency as HTTP 409

What EF Core and Npgsql throw:

- A unique-index violation on save arrives as `DbUpdateException`, with the provider exception as `InnerException`. EF Core's docs say a unique violation "results in a provider-specific exception being thrown, and not `DbUpdateConcurrencyException`" ([Handling concurrency conflicts](https://learn.microsoft.com/en-us/ef/core/saving/concurrency#optimistic-concurrency)). For Npgsql, that inner exception is `PostgresException`. Its `SqlState` "is always populated", and its `ConstraintName` names the violated constraint ([PostgresException API](https://www.npgsql.org/doc/api/Npgsql.PostgresException.html)). `PostgresErrorCodes.UniqueViolation` is `"23505"` ([PostgresErrorCodes API](https://www.npgsql.org/doc/api/Npgsql.PostgresErrorCodes.html)). With snake_case naming, the constraint name will look like `ix_members_group_id_account_id` ⚠️ (inferred from EFCore.NamingConventions behaviour, not checked).
- A concurrency-token mismatch throws `DbUpdateConcurrencyException`, which derives from `DbUpdateException` ([DbUpdateException API](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbupdateexception?view=efcore-10.0), [Handling concurrency conflicts](https://learn.microsoft.com/en-us/ef/core/saving/concurrency#optimistic-concurrency)).
- **Optimistic concurrency on PostgreSQL uses `xmin`.** Map a `uint` property with `[Timestamp]` or `.IsRowVersion()`, and Npgsql maps it to the `xmin` system column, which changes on every row update ([Npgsql: Concurrency tokens](https://www.npgsql.org/efcore/modeling/concurrency.html)). This is what catches two Accounts claiming the same Unclaimed Member (§1, "Watch out"). ⚠️ A shadow property (`Property<uint>("xmin").IsRowVersion()`) would keep the token out of the Domain class. Npgsql's docs show only a CLR property, so test this first.
- Npgsql's docs say serialization failures and deadlocks become `DbUpdateConcurrencyException` "starting with version 11.0" ([same page](https://www.npgsql.org/efcore/modeling/concurrency.html)). We use EF Core 10. On NuGet, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 is the latest 10.x and 11.0 is still at rc.3 ([NuGet versions](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL)), so that conversion doesn't apply to us. It only matters if we raise the isolation level anyway.

Where the catch lives, per option:

| | Catch `DbUpdateException` + 23505 / `DbUpdateConcurrencyException` | Map to 409 |
|---|---|---|
| (a) | Override `SaveChangesAsync` in `MusializeDbContext` (Infrastructure). Rethrow `ConflictException`, which Application defines. | Api `IExceptionHandler`: `ConflictException` → 409 problem details |
| (b) | Same override, behind `IUnitOfWork` | Same |
| (c2) | Not possible in the DbContext without Npgsql in Application | Api `IExceptionHandler` matches `DbUpdateConcurrencyException` and `DbUpdateException { InnerException: PostgresException { SqlState: "23505" } }` directly. Api gets Npgsql transitively through Infrastructure ⚠️ (NuGet `PackageReference`s flow transitively by default). |

```csharp
// Infrastructure/Persistence/MusializeDbContext.cs, options (a) and (b)
public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
{
    try
    {
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }
    catch (DbUpdateException e) when (e is DbUpdateConcurrencyException
        || e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
    {
        throw new ConflictException("Someone else changed this at the same time.", e);
    }
}
```

⚠️ I override the `(bool, CancellationToken)` overload because I believe the one-argument `SaveChangesAsync(ct)` calls it. That is from memory of EF Core's `DbContext`, not from a doc.

- **The Api side** is one `IExceptionHandler` registered with `AddExceptionHandler<T>()`, plus `AddProblemDetails()` and `UseExceptionHandler()` ([Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0#problem-details)). The normal case, where the handler's own check finds the Account already in the Group, returns `ClaimMemberResult.AlreadyInGroup`, and the endpoint turns that into `TypedResults.Conflict(...)`. So the OpenAPI document already lists 409 for this endpoint. The exception path is only the backstop for the rare race ⚠️ (my design).
- **Not in the handler.** Catching in the handler would need `DbUpdateException` (fine in (a)/(c2)) *and* the SQLSTATE check (needs Npgsql). It would also repeat in every handler.
- **Optional library: EntityFramework.Exceptions.** `UseExceptionProcessor()` turns provider errors into typed exceptions such as `UniqueConstraintException`, with `ConstraintName` and `ConstraintProperties`. All of them inherit from `DbUpdateException` ([README](https://github.com/Giorgi/EntityFramework.Exceptions)). It is Apache-2.0, `EntityFrameworkCore.Exceptions.PostgreSQL` 10.0.1 targets net10.0, and the repo was pushed on 2026-11-05 (NuGet and GitHub metadata). With it, Application could catch `UniqueConstraintException` by referencing only `EntityFrameworkCore.Exceptions.Common`, which depends on `Microsoft.EntityFrameworkCore.Relational`, not Npgsql (NuGet dependency metadata). The README warns that `ConstraintName` is only populated if the index is in the EF model. For one constraint, a five-line `when` filter is simpler than a dependency (YAGNI).

## 6. Cost of switching later

- **(c2) → (a): mechanical.** Add `IAppDbContext` with the `DbSet`s already in use. Move `MusializeDbContext` and the configurations to Infrastructure. Change the constructor parameter type in each handler. Move the conflict catch from the Api handler into `SaveChangesAsync`. Migrations don't change, because the model doesn't change ⚠️. It's a find-and-replace across handlers plus moving files.
- **(a) → (c2): mechanical,** the reverse.
- **(a) → (b): not mechanical.** Every handler's inline LINQ must become a named repository or query-service method, with an Infrastructure implementation. The cost grows with the number of handlers. Each move is simple, but it touches every handler, and each one needs a design decision (method name and shape).
- **(b) → (a): mechanical.** Inline each repository method's LINQ into its callers and delete the repository.
- **Any → (c1):** not recommended (§4.3).

So (a) and (c2) are cheap to swap between. (b) is the only expensive destination. Postponing (b) until there is a concrete need, such as Application unit tests or a second data store, costs one rewrite of the handlers that exist by then.

## 7. Read-side queries

Example: list the Members of a Group as DTOs.

```csharp
// (a) and (c2): inline in the handler
public sealed class ListMembersHandler(IAppDbContext db) // or MusializeDbContext in (c2)
{
    public Task<List<MemberDto>> HandleAsync(Guid groupId, CancellationToken ct) =>
        db.Members
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Name)
            .Select(m => new MemberDto(m.Id, m.Name, m.AccountId != null))
            .ToListAsync(ct);
}
```

```csharp
// (b): interface in Application, implementation in Infrastructure
public interface IMemberListQuery
{
    Task<IReadOnlyList<MemberDto>> ListAsync(Guid groupId, CancellationToken ct);
}

public sealed class MemberListQuery(MusializeDbContext db) : IMemberListQuery
{
    public async Task<IReadOnlyList<MemberDto>> ListAsync(Guid groupId, CancellationToken ct) =>
        await db.Members
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Name)
            .Select(m => new MemberDto(m.Id, m.Name, m.AccountId != null))
            .ToListAsync(ct);
}
```

- The LINQ is identical. (b) adds an interface, a class and a DI registration per query, as in Ardalis's `IListContributorsQueryService` (§3.4).
- A `Select` into a DTO already produces SQL that reads only the selected columns ⚠️ (general EF Core behaviour). An `AsNoTracking` isn't needed for a projection into a non-entity type ⚠️.
- Both Microsoft's e-book and Ardalis allow queries to bypass the aggregate repositories (CQRS style) ([one repository per aggregate](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design#define-one-repository-per-aggregate)). So even under (b), the read side doesn't go through `IMemberRepository`.

## 8. Recommendation

**Choose (a), `IAppDbContext`.**

- It matches ADR-0008 as written. Infrastructure owns PostgreSQL, EF mapping, migrations and Npgsql. Application owns handlers and the interfaces they need.
- It adds one small interface and no per-use-case files.
- It is the only cheap option where the database-conflict translation lives in Infrastructure and Application still sees no Npgsql type.
- It doesn't pretend to buy testability. Our tests stay over HTTP against PostgreSQL, which is what Microsoft recommends first.
- Concretely:
  - add `Microsoft.EntityFrameworkCore` to Application
  - add `Npgsql.EntityFrameworkCore.PostgreSQL` 10.x and `EFCore.NamingConventions` to Infrastructure
  - expose `DbSet`s only for aggregate roots, and use `AutoInclude` for their parts
  - override `SaveChangesAsync` to throw `ConflictException`
  - map that to 409 in one Api `IExceptionHandler`
  - put an `xmin` concurrency token on Member, so two Claims of the same Member can't both win.

**Runner-up: (c2), `MusializeDbContext` in Application.** It has less ceremony and the same handler code, and moving to (a) later is mechanical. It loses because EF mapping moves into Application, and the conflict translation lands in the Api, reading Npgsql types.

**Revisit (b)** only if handlers gain logic worth unit testing on their own. ADR-0008 already names that as the trigger for an Application test project. At that point, repositories become the test seam Microsoft recommends.
