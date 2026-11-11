# Musialize

[![CI](https://github.com/fekko-dev/musialize/actions/workflows/ci.yml/badge.svg)](https://github.com/fekko-dev/musialize/actions/workflows/ci.yml)

Musialize is a web app for Groups of musicians who choose Songs together and form a different Line-up for each Song. The [product vision](docs/product/vision.md) explains the problem it solves and for whom.

## Prerequisites

- **Node.js**, at the version in [`.nvmrc`](.nvmrc)
- **pnpm**, at the version in `packageManager` in [`package.json`](package.json)
- **.NET SDK**, at the version in [`global.json`](global.json)

## Run, test and lint

```bash
pnpm install   # dependencies and Git hooks
pnpm dev       # API and web app together, with hot reload
pnpm test      # backend and web tests
pnpm lint      # the lint checks CI runs
```

The other scripts are in [`package.json`](package.json).

## Further reading

- [Development process](docs/process.md): how work goes from idea to release
- [CONTRIBUTING.md](CONTRIBUTING.md): branches, commits, pull requests and migrations
- [GLOSSARY.md](GLOSSARY.md): the domain terms used in code, issues and commits
- [Architecture decisions](docs/adr/)
- [Deployment](docs/deployment.md): staging and production
