import { readdirSync } from "node:fs";
import { expect, test } from "@playwright/test";

test("the home page shows a healthy server with the image's version and latest migration", async ({
  page,
}) => {
  const health = page.getByRole("region", { name: "Server health" });

  await test.step("open the home page", async () => {
    await page.goto("/");
  });

  await test.step("the server is healthy", async () => {
    await expect(health.getByText("Healthy", { exact: true })).toBeVisible();
  });

  await test.step("it runs the image's version", async () => {
    await expect(
      health.getByText(expectedVersion(), { exact: true }),
    ).toBeVisible();
  });

  await test.step("the database has the latest migration", async () => {
    await expect(
      health.getByText(latestMigration(), { exact: true }),
    ).toBeVisible();
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
  return migrations.at(-1)!.replace(/\.cs$/, "");
}
