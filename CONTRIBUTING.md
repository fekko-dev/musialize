# Contributing to Musialize

Before you start, read the [product vision](docs/product/vision.md), the [glossary](GLOSSARY.md), the [architecture decisions](docs/adr/) and the [development process](docs/process.md). Use the glossary's terms in code, issues and commits.

To set up and run Musialize locally, see the [README](README.md).

## Workflow

1. **Every change starts from a GitHub issue.**
2. **Create a short-lived branch from `main`**, named `<type>/<issue>-<slug>`, for example `feat/12-sign-up-for-part`.
3. **Open a pull request into `main`** and link the issue (`Closes #12`).
4. **Merge only when all required checks pass:** formatting, linting, type checking, tests and both builds.
5. **Use a merge commit.** Squash and rebase merges are disabled so each commit stays visible in the history and in `git blame`. The branch is deleted after merging.

`main` must always be deployable. There are no other long-lived branches.

`pnpm lint` runs the lint checks that CI will run: Prettier, ESLint, `tsc`, `dotnet format` and a Markdown link check. `pnpm format` fixes formatting. `pnpm install` also installs Git hooks that format and lint the staged files on every commit and check the commit message.

## Commits

- **Follow [Conventional Commits](https://www.conventionalcommits.org/)** (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`, …). This applies to every commit, because they all end up on `main`. It's checked with commitlint.
- **Keep commits atomic:** one logical change per commit, with a message that explains _why_. Each commit passes lint, typecheck and tests on its own, so any one can be reverted or bisected. Clean up your branch history before opening the PR. Don't push "wip" or "fix lint" commits.
- **Keep GitHub's default merge commit message.** Tools ignore it, so changes don't appear twice in the changelog.

## Releases and environments

| Environment | Deploys                       | When                                 |
| ----------- | ----------------------------- | ------------------------------------ |
| Staging     | the newest commit on `main`   | automatically after every merge      |
| Production  | the latest release (`vX.Y.Z`) | when the release-please PR is merged |

Both environments run the same image, built once in CI. Versions and the changelog are derived from the commits. See [ADR-0004](docs/adr/0004-promote-images-from-main-by-release-tag.md).

## Database migrations

A migration runs before the new version starts, and for a few seconds the running version uses the new schema. A rollback also deploys the previous image against it. So **every migration must work with the version that is currently running** ([ADR-0009](docs/adr/0009-migration-bundles-as-a-separate-deploy-step.md)):

- **Additive changes ship in one release:** new tables, and new columns that are nullable or have a default.
- **Renames, drops and type changes take two releases (expand–contract):** first add the new form and stop depending on the old one; remove the old form in a later release. Never change a column's type in place: add a new column and move the data over.
- **Don't rely on `Down()` methods** for deploys or rollbacks. A rollback deploys an earlier image and leaves the schema as it is.

## Errors

Every API error response is a ProblemDetails. Invalid input is rejected by validation (400) before a handler runs, and an expected outcome such as "not found" is a `TypedResults` result of the endpoint. **Throw an exception only for a domain conflict (409) or something unexpected (500).** The central exception handler turns an unexpected exception into a 500 and logs it, so don't catch and log it again.

## Logging

Log with the built-in `ILogger` (Serilog writes the entries, see [ADR-0011](docs/adr/0011-serilog-for-logging.md)) and structured messages (`LogInformation("Song {SongId} accepted", songId)`, not string interpolation), so each value stays a separate field in the JSON logs.

**Never log sign-in codes, cookies or Comment contents**, at any level. The logs leave the app (Dokploy, terminal scrollback, bug reports), so treat them as readable by anyone. Log IDs instead of content. The request log only records the method, path, status code and duration, never headers, cookies, query values or bodies, and a test in `Api.Tests` checks this. Because the path is logged, **never put a secret in a route**: send it in the body.

## Documentation

- **The README is the front door.** A pull request that changes setup, environment variables, run or test commands, folder layout or deployment updates [README.md](README.md) in the same pull request.
- **Domain terms** that are added or changed go into [GLOSSARY.md](GLOSSARY.md).
- **Decisions** that are hard to reverse, surprising, and the result of a real trade-off get an ADR in [docs/adr/](docs/adr/).

## License

Contributions are licensed under the [AGPL-3.0](LICENSE).
