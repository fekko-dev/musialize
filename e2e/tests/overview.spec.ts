import { expect, test } from "@playwright/test";

// The e2e app seeds the demo Group (DemoGroup__Seed in compose.e2e.yaml).
test("the Overview lists the demo Group's Suggestions, oldest first", async ({
  page,
}) => {
  const suggestions = page.getByRole("region", { name: "Suggestions" });

  await test.step("open the app", async () => {
    await page.goto("/");
  });

  await test.step("the Suggestions are in the order they came in", async () => {
    await expect(suggestions.getByRole("listitem")).toHaveText([
      /^Wonderwall\s*Oasis · Suggested on/,
      /^Mr\. Brightside\s*The Killers · Suggested on/,
      /^Africa\s*Toto · Suggested on/,
    ]);
  });
});
