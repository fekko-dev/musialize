# Promote one image from main to production through release tags

There is one long-lived branch, `main`, and no environment branches. CI builds a Docker image once per commit on `main`, tagged with the commit SHA, and pushes it to GitHub Container Registry. Staging automatically deploys every new image. Production only deploys when a release is created, by merging the release-please PR, and it deploys the _same_ image that ran on staging rather than a rebuild. We chose this over environment branches (`develop` → `staging` → `main`) so that production always runs an image that was tested on staging, without merging between environment branches, and so that nothing goes live by surprise while the app is used during a Review Meeting.

## Consequences

- What runs where is shown by version, not by branch. Staging runs the newest commit on `main`; production runs the latest `vX.Y.Z` tag. The open release PR lists exactly what is on staging but not yet in production.
- Each environment is a Dokploy Application with the Docker image as its source, in its own Dokploy environment, with its own database. GitHub Actions deploys through the Dokploy API: first set the image tag (`application.saveDockerProvider`), then deploy (`application.deploy`). Following Dokploy's own production guidance, nothing is built on the server.
- Only immutable image tags (the SHA or the version) are deployed, never a moving tag such as `latest`. Rolling back means deploying an earlier tag again.
- Release-please reads the Conventional Commits on `main` to choose the next version and write the changelog.
- Deploys avoid most downtime without strict rules. Each Application has a health check on `/health` (the image needs `curl`) and the Swarm Update Config `"Order": "start-first"`, so the new container must be healthy before the old one stops; otherwise the deploy rolls back automatically. Migrations run as a separate step before each deploy and must stay compatible with the running version, so old and new can run side by side without errors ([ADR-0009](0009-migration-bundles-as-a-separate-deploy-step.md)). There is no server-side maintenance page; the app shows a "reconnecting…" banner and an "update available, reload" prompt instead.

## Still to verify while building the pipeline

- Whether Dokploy's built-in registry rollback works for applications that pull an image built in CI.
- That restoring from the database backups actually works (test it once before the pilot).
