# Handlers reach the database through `IAppDbContext`

Use-case handlers in Application reach PostgreSQL through `IAppDbContext`, a small interface in Application with one `DbSet<T>` per aggregate root and `SaveChangesAsync`. `MusializeDbContext` in Infrastructure implements it, and handlers write plain EF Core LINQ against it. [ADR-0008](0008-four-layer-clean-architecture-with-use-case-handlers.md) left this open until the Songs feature, the first that stores data. We compared the options against Microsoft's EF Core docs and the Jason Taylor and Ardalis templates ([research](../research/persistence-access-pattern.md)).

```csharp
public interface IAppDbContext
{
    DbSet<Member> Members { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public sealed class ClaimMemberHandler(IAppDbContext db)
{
    public async Task<ClaimMemberResult> HandleAsync(Guid groupId, Guid memberId, Guid accountId, CancellationToken ct)
    {
        var member = await db.Members.SingleOrDefaultAsync(m => m.GroupId == groupId && m.Id == memberId, ct);
        if (member is null) return ClaimMemberResult.NotFound;
        if (await db.Members.AnyAsync(m => m.GroupId == groupId && m.AccountId == accountId, ct))
            return ClaimMemberResult.AlreadyInGroup;

        member.Claim(accountId);
        await db.SaveChangesAsync(ct);
        return ClaimMemberResult.Claimed;
    }
}
```

No option makes handlers easier to test under our strategy. We test them over HTTP against a real PostgreSQL, and Microsoft advises against mocking `DbSet` or using the in-memory provider. Jason Taylor's template never mocks its `IApplicationDbContext` either. The interface earns its place because the DbContext, the mapping, the migrations and Npgsql stay in Infrastructure, as ADR-0008 says. That keeps the one place that understands PostgreSQL error codes out of Application.

## Considered options

- **A repository per aggregate root, plus read-side query interfaces**: rejected for now. It is the only option that keeps EF Core out of Application and gives reliable test doubles. But ADR-0008 tests handlers without fakes of our own modules, so we would add a method and an implementation for every query and get nothing back.
- **Handlers take the concrete `MusializeDbContext`, which lives in Application**: the runner-up. It saves one small file, but entity mapping moves into Application, and the Api would have to read Npgsql exception types to produce a 409. A handler that references Infrastructure directly is not an option: it creates a project reference cycle.
- **`EntityFramework.Exceptions`** for typed database errors: not needed yet. A five-line filter covers the one error we turn into a response today (a unique violation). Switch when we handle more kinds, such as foreign-key violations becoming a 404 or 422. It must release a new version for every EF Core major, so it adds a step to each upgrade.

## Consequences

- **Application references `Microsoft.EntityFrameworkCore`, never Npgsql.** The architecture test from ADR-0008 gains that rule. Handlers can use `IQueryable`, `Include` and `ToListAsync`, but not PostgreSQL-specific functions or error types.
- **Aggregates load complete by configuration.** Only aggregate roots get a `DbSet`, and their children are loaded with `Navigation(...).AutoInclude()`. Code review checks this. The compiler doesn't.
- **Reads are plain LINQ projections in the handler** (`AsNoTracking().Where(...).Select(... => new Dto(...))`). A query interface is added only when the same query is needed in several places.
- **Conflicts become 409 in two ways:**
  - **Expected conflicts** come back as a handler result, such as `ClaimMemberResult.AlreadyInGroup`, which the endpoint returns as `TypedResults.Conflict`, so the OpenAPI document lists the 409.
  - **Lost races** surface when `MusializeDbContext` saves. Its `SaveChangesAsync` override catches `DbUpdateConcurrencyException`, and `DbUpdateException` with an inner `PostgresException` of SQLSTATE `23505`. It rethrows them as `ConflictException`, which Application defines. One Api `IExceptionHandler` maps that to 409 problem details.
- **Rows that two people can change at once get an `xmin` concurrency token.** The unique index on `(group_id, account_id)` stops one Account from claiming two Members of a Group. It doesn't stop two Accounts from claiming the same Unclaimed Member, where the last save would silently win. With Npgsql's `xmin` row version on Member, the second save fails and becomes a 409.
- **Switching later:** moving to a concrete DbContext is mechanical. Moving to repositories means rewriting each handler's data access, and that cost grows with every handler. If we ever want Application unit tests with fakes, decide it early.

## Still to verify while building

- Whether `xmin` can be a shadow property (`Property<uint>("xmin").IsRowVersion()`), so the token stays out of the Domain class.
- That overriding `SaveChangesAsync(bool, CancellationToken)` also covers calls to `SaveChangesAsync(CancellationToken)`.
