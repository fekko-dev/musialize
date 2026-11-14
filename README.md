# Musialize

[![CI](https://github.com/fekko-dev/musialize/actions/workflows/ci.yml/badge.svg)](https://github.com/fekko-dev/musialize/actions/workflows/ci.yml)

Musialize is a web app for Groups of musicians who choose Songs together and form a different Line-up for each Song. The [product vision](docs/product/vision.md) explains the problem it solves and for whom.

## Prerequisites

- **Node.js**, at the version in [`.nvmrc`](.nvmrc)
- **pnpm**, at the version in `packageManager` in [`package.json`](package.json)
- **.NET SDK**, at the version in [`global.json`](global.json)
- **Docker**, running. `pnpm dev` starts PostgreSQL and Mailpit from [`compose.dev.yaml`](compose.dev.yaml), and the backend tests start their own PostgreSQL with Testcontainers.
- **Playwright's Chromium**, for the end-to-end tests. Install it once, and again after Playwright is updated:

  ```bash
  pnpm --filter @musialize/e2e exec playwright install chromium
  ```

## Run, test and lint

```bash
pnpm install   # dependencies and Git hooks
pnpm dev       # local PostgreSQL and Mailpit, migrations, then API and web app with hot reload
pnpm test      # backend, web and end-to-end tests
pnpm lint      # the lint checks CI runs
```

`pnpm test:e2e` builds the production image from the [`Dockerfile`](Dockerfile), starts it with [`compose.e2e.yaml`](compose.e2e.yaml) on port 8080, runs the [Playwright tests](e2e/tests) on desktop Chromium and a phone viewport, and stops everything again. To test an image you already have, set `MUSIALIZE_IMAGE` to it and `APP_VERSION` to the short SHA it was built with.

`pnpm dev` also seeds a demo Group with three Suggestions (`DemoGroup__Seed` in [`launchSettings.json`](backend/src/Musialize.Api/Properties/launchSettings.json)), because there is no sign-in yet. The app opens on its Overview at <http://localhost:5173>, and the server's health is at `/status`.

The other scripts are in [`package.json`](package.json).

## Web app layout

`web/src` follows a [Bulletproof React](https://github.com/alan2207/bulletproof-react)-style layout. Imports flow one way, shared → features → app, and features don't import each other; `pnpm lint` enforces this.

- `app/`: routes, router and providers
- `features/<name>/`: one folder per feature, with its own `api/`, `components/` and so on
- shared: `components/` (shadcn/ui components in `components/ui/`), `lib/` (the generated API client, i18n), `locales/` (interface text), `types/`
- `testing/`: test helpers

Interface text goes through translation keys in [`web/src/locales/en.ts`](web/src/locales/en.ts); there are no barrel (`index.ts`) files.

## Further reading

- [Development process](docs/process.md): how work goes from idea to release
- [CONTRIBUTING.md](CONTRIBUTING.md): branches, commits, pull requests and migrations
- [GLOSSARY.md](GLOSSARY.md): the domain terms used in code, issues and commits
- [Architecture decisions](docs/adr/)
- [Deployment](docs/deployment.md): staging and production, including [logs and log levels](docs/deployment.md#logs) and [branding](docs/deployment.md#branding)
