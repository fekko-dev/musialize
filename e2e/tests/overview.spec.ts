import { expect, test } from "@playwright/test";

// Migrating seeds the demo Group: DemoGroup__Seed in compose.e2e.yaml, appsettings.Development.json for `pnpm dev`.
test("the Overview lists the demo Group's Suggestions", async ({ page }) => {
  const suggestions = page.getByRole("region", { name: "Suggestions" });

  await test.step("open the app", async () => {
    await page.goto("/");
  });

  await test.step("the seeded Suggestions arrive from the database", async () => {
    await expect(suggestions.getByRole("listitem").first()).toContainText(
      "Suggested on",
    );
  });
});
