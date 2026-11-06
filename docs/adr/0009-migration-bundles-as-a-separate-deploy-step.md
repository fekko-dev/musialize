# Run migrations as a separate deploy step, compatible with the running version

Database migrations are applied by an EF Core migration bundle, not by the app at startup, which EF Core's documentation advises against for production. The bundle is built into the same image as the app, so one image is still promoted from staging to production ([ADR-0004](0004-promote-images-from-main-by-release-tag.md)). Each deploy runs: push or re-tag the image, migrate, then deploy through the Dokploy API. Dokploy cannot run the migration step for us: its scheduled jobs can be triggered through the API, but the call reports neither success nor failure, so CI couldn't stop a deploy after a failed migration. Instead, GitHub Actions connects over SSH and starts a one-off container from the new image on Dokploy's internal network, which runs the bundle and exits with its result. PostgreSQL is never exposed to the internet.

Deploys stay `start-first` because zero downtime matters while the app is used live during a Review Meeting. For a few seconds after a migration, the previous version runs against the new schema, and a rollback deploys the previous image against it as well. So every migration must work with the version that is currently running:

- **Additive changes ship in one release:** new tables, and new columns that are nullable or have a default.
- **Destructive changes use expand–contract across two releases:** renames, drops and type changes. First expand (add the new form, and stop depending on the old one), then contract (remove the old form) in a later release, once no running version uses it.

EF Core still generates `Down()` methods. They can undo a mistake locally, but deploys and rollbacks never rely on them: a rollback deploys an earlier image and leaves the schema as it is, and undoing a migration often throws away data written since.

## Consequences

- **SSH access for CI:** a dedicated `deploy` user on the server whose key may run only one script (an SSH forced command). The script accepts only an environment name and an image tag, and starts the bundle with that environment's migrator credentials from a root-only file on the server. Being allowed to run Docker is effectively root access, so this key is kept as narrow as possible.
- **Two database roles per environment:**
  - The **migrator** role owns the schema and runs the bundle. `ALTER DEFAULT PRIVILEGES` makes every table it creates grant `SELECT`, `INSERT`, `UPDATE` and `DELETE` (and sequence usage) to the app role.
  - The **app** role is used by the running app. It can't change the schema, but it can read the migrations history table.
  - The migrator's password lives only in the server file, and the app's password only in Dokploy.
- **`/health` reports unhealthy while the database lacks a migration the app expects.** If the migrate step was skipped or failed, `start-first` keeps the old version running.
- **Where migrations run outside the deployed environments:**
  - `pnpm dev` runs `dotnet ef database update`.
  - Integration tests migrate their test database during setup.
  - The end-to-end Compose setup runs the bundle as its own service before the app starts, with both roles, so the bundle and the permission model are tested before staging. Local development and integration tests may use a single database user.
- **No fallback to migrating at startup.** Setting up the server includes checking that a one-off container reaches each environment's database. If it can't, that is fixed before anything is deployed. A fallback is only considered after its trade-offs are written down and approved.
