import { expect, test } from "@playwright/test";

test("a Suggestion shows on its Song page and at the end of the Overview", async ({
  page,
}, testInfo) => {
  // A Group has each Song once (#45), and the desktop and phone runs share one database.
  const title = `Test Song ${testInfo.project.name} ${Date.now()}`;

  await test.step("open Suggest a Song from the Overview", async () => {
    await page.goto("/");
    // The button by the heading on a desktop, the floating one on a phone.
    await page.getByRole("link", { name: "Suggest a Song" }).click();
  });

  await test.step("suggest a Song", async () => {
    await page.getByLabel("Title").fill(title);
    await page.getByLabel("Artist").fill("The Testers");
    await page
      .getByLabel("Link to the song")
      .fill("https://example.com/test-song");
    await page.getByRole("button", { name: "Suggest" }).click();
  });

  await test.step("its Song page shows it", async () => {
    await expect(page.getByRole("heading", { name: title })).toBeVisible();
    await expect(
      page.getByRole("link", { name: "example.com/test-song" }),
    ).toHaveAttribute("href", "https://example.com/test-song");
  });

  await test.step("the Overview lists it after the seeded Suggestions", async () => {
    await page.goto("/");
    const rows = page
      .getByRole("region", { name: "Suggestions" })
      .getByRole("listitem");
    await expect(rows.filter({ hasText: title })).toBeVisible();
    const titles = await rows.allTextContents();
    const position = titles.findIndex((row) => row.includes(title));
    expect(position).toBeGreaterThan(
      titles.findIndex((row) => row.includes("Africa")),
    );
  });
});
