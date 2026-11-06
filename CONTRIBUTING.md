# Contributing to Musialize

Before you start, read the [product vision](docs/product/vision.md), the [glossary](GLOSSARY.md) and the [architecture decisions](docs/adr/). Use the glossary's terms in code, issues and commits.

Instructions for local setup will be added once there is code to run.

## Workflow

1. **Every change starts from a GitHub issue.**
2. **Create a short-lived branch from `main`**, named `<type>/<issue>-<slug>`, for example `feat/12-sign-up-for-part`.
3. **Open a pull request into `main`** and link the issue (`Closes #12`).
4. **Merge only when all required checks pass:** formatting, linting, type checking, tests and both builds.
5. **Use a merge commit.** Squash and rebase merges are disabled so each commit stays visible in the history and in `git blame`. The branch is deleted after merging.

`main` must always be deployable. There are no other long-lived branches.

## Commits

- **Follow [Conventional Commits](https://www.conventionalcommits.org/)** (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`, …). This applies to every commit, because they all end up on `main`. It's checked with commitlint.
- **Keep commits atomic:** one logical change per commit, with a message that explains *why*. Clean up your branch history before opening the PR. Don't push "wip" or "fix lint" commits.
- **Keep GitHub's default merge commit message.** Tools ignore it, so changes don't appear twice in the changelog.

## Releases and environments

| Environment | Deploys | When |
|---|---|---|
| Staging | the newest commit on `main` | automatically after every merge |
| Production | the latest release (`vX.Y.Z`) | when the release-please PR is merged |

Both environments run the same image, built once in CI. Versions and the changelog are derived from the commits. See [ADR-0004](docs/adr/0004-promote-images-from-main-by-release-tag.md).

## Documentation

- **Domain terms** that are added or changed go into [GLOSSARY.md](GLOSSARY.md).
- **Decisions** that are hard to reverse, surprising, and the result of a real trade-off get an ADR in [docs/adr/](docs/adr/).

## License

Contributions are licensed under the [AGPL-3.0](LICENSE).
