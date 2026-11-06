# Four-layer Clean Architecture with one handler per use case

The backend is split into four projects whose references point inward: `Domain` ← `Application` ← `Infrastructure` and `Api`. Domain holds the aggregates and their rules (Song, Line-up, Claim) with no framework code. Application holds one handler class per use case, such as `AcceptSongHandler`, plus the interfaces it needs: a repository per aggregate root for writes, and query interfaces that return view models for reads. Infrastructure implements those interfaces with EF Core and PostgreSQL, and later MailKit and SignalR. Api holds minimal API endpoints in route groups per feature, returning `TypedResults`, and is where everything is wired together. We chose four layers before there is much domain because the pilot's use cases (sign-in, Claim, Sign-up, accepting a Song) arrive within days, and keeping EF Core, email and SignalR out of them lets the rules be tested without a database. A second goal of Musialize is to show professional .NET engineering, and this is the structure a reviewer expects to find.

## Considered options

- **Application services** (one class per area with many methods, as in ABP): rejected because the classes grow with every feature, and every endpoint pulls in the dependencies of the whole service.
- **A mediator library** (MediatR, or the MIT-licensed `Mediator`): rejected for now. The handlers look the same with or without one; the library adds indirection ("go to definition" lands in the library) for a pipeline we can get from endpoint filters. MediatR itself has been dual-licensed (RPL-1.5 or commercial) since version 13. Moving to `Mediator` later is mechanical, because the handler bodies don't change.
- **An `IAppDbContext` in Application** (the popular template): rejected because it puts EF Core into Application.
- **Controllers**: still supported, but minimal APIs with `TypedResults` describe each response exactly in the OpenAPI document, which gives the generated TypeScript client better types.
- **Fewer projects** (Domain and Api only, or one project with an architecture test): simpler, but nothing would stop a use case from reaching for EF Core directly.

## Consequences

- The compiler enforces most of the dependency rule through project references. One gap remains: Api must reference Infrastructure to wire it up, so an architecture test checks that nothing in Api except the startup code uses Infrastructure types.
- Each handler is registered explicitly at startup.
- Handlers are tested through the API (`WebApplicationFactory` with a real PostgreSQL in Testcontainers). Domain rules get fast unit tests. There is no separate Application test project until a handler has logic worth testing on its own.
