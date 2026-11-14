# Four-layer Clean Architecture with one handler per use case

The backend is split into four projects whose references point inward: `Domain` ← `Application` ← `Infrastructure` and `Api`. Each layer solves a concrete problem:

- **Domain** holds the aggregates and their rules (Song, Line-up, Claim) with no framework code, so those rules get fast unit tests.
- **Application** holds one handler class per use case, such as `AcceptSongHandler`, and the interfaces for real system boundaries: sending email, broadcasting live updates and the clock. In tests, only those boundaries are replaced with fakes (for example, the email sender that captures sign-in codes in [ADR-0007](0007-passwordless-sign-in-with-cookie-sessions.md)).
- **Infrastructure** implements those interfaces with PostgreSQL through EF Core, MailKit and SignalR.
- **Api** holds minimal API endpoints in route groups per feature, returning `TypedResults`, and is where everything is wired together.

Endpoints call their handler directly. There is no mediator library.

## Considered options

- **Application services** (one class per area with many methods, as in ABP): rejected because the classes grow with every feature, and every endpoint pulls in the dependencies of the whole service.
- **A mediator library** (MediatR, or the MIT-licensed `Mediator`): rejected for now. The handlers look the same with or without one. The library adds indirection ("go to definition" lands in the library) for a pipeline that endpoint filters already provide. MediatR itself has been dual-licensed (RPL-1.5 or commercial) since version 13. Moving to `Mediator` later is mechanical, because the handler bodies don't change.
- **Controllers**: still supported, but minimal APIs with `TypedResults` describe each response exactly in the OpenAPI document, which gives the generated TypeScript client better types.
- **Fewer projects** (Domain and Api only, or one project with an architecture test): simpler, but nothing would keep email, SignalR and database details out of the use cases.

## Not decided here

How handlers reach the database is left to the first domain feature, so it is chosen with real code in view. That turned out to be Sign-in and Claim, and [ADR-0010](0010-handlers-reach-the-database-through-iappdbcontext.md) chose `IAppDbContext`. The two candidates are an `IAppDbContext` in Application that exposes EF Core `DbSet`s, as in Jason Taylor's Clean Architecture template, or a repository per aggregate root plus read-side query interfaces, which keep EF Core out of Application. Either way, a Song always loads as a complete aggregate, and Domain never references EF Core.

## Consequences

- The compiler enforces most of the dependency rule through project references. An architecture test covers what it can't: Domain has no EF Core reference, and nothing in Api except the startup code uses Infrastructure types.
- Each handler is registered explicitly at startup.
- Handlers are tested through the API (`WebApplicationFactory` with a real PostgreSQL in Testcontainers), not with fakes of our own modules. Domain rules get fast unit tests. There is no separate Application test project until a handler has logic worth testing on its own.
