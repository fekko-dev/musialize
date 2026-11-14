import { readdirSync } from "node:fs";
import { expect, test } from "@playwright/test";

// Dokploy and the deploy job poll /health, so it's checked as JSON, not on a page.
test("/health reports a healthy server with the image's version and latest migration", async ({
  request,
}) => {
  const response = await request.get("/health");

  expect(response.ok()).toBe(true);
  expect(await response.json()).toEqual({
    status: "healthy",
    version: expectedVersion(),
    databaseMigration: latestMigration(),
  });
});

// The short SHA the image was built with, passed in by run.ts.
function expectedVersion(): string {
  const version = process.env.APP_VERSION;
  if (!version) {
    throw new Error("Set APP_VERSION to the version the image was built with.");
  }
  return version;
}

// The newest migration in the repository, such as "20261111185257_InitialCreate".
function latestMigration(): string {
  const migrations = readdirSync(
    new URL(
      "../../backend/src/Musialize.Infrastructure/Persistence/Migrations",
      import.meta.url,
    ),
  )
    .filter((file) => /^\d{14}_\w+\.cs$/.test(file))
    .sort();
  const latest = migrations.at(-1);
  if (!latest) {
    throw new Error("Found no migrations in the Infrastructure project.");
  }
  return latest.replace(/\.cs$/, "");
}
