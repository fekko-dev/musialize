import { expect, test } from "@playwright/test";

test.describe("on a desktop", () => {
  test.skip(({ isMobile }) => isMobile, "a phone has no sidebar on the page");

  test("the sidebar links Suggestions and folds to a rail and back", async ({
    page,
  }) => {
    const navigation = page.getByRole("navigation", { name: "Main" });
    const suggestions = navigation.getByRole("link", { name: "Suggestions" });

    await test.step("open the home page", async () => {
      await page.goto("/");
    });

    await test.step("Suggestions is the current page", async () => {
      await expect(suggestions).toBeVisible();
      await expect(suggestions).toHaveAttribute("aria-current", "page");
    });

    await test.step("collapse to the rail: the icon stays, its name hides", async () => {
      await page.getByRole("button", { name: "Collapse" }).click();
      await expect(suggestions).toBeVisible();
      await expect(navigation.getByText("Suggestions")).toBeHidden();
    });

    await test.step("open the sidebar again", async () => {
      await page.getByRole("button", { name: "Expand" }).click();
      await expect(navigation.getByText("Suggestions")).toBeVisible();
    });
  });
});

test.describe("on a phone", () => {
  test.skip(({ isMobile }) => !isMobile, "a desktop shows the sidebar itself");

  test("the top bar opens the sidebar as a drawer, which closes after a tap", async ({
    page,
  }) => {
    const drawer = page.getByRole("dialog");

    await test.step("open the home page", async () => {
      await page.goto("/");
    });

    await test.step("the top bar shows the logo and App name", async () => {
      const topBar = page.getByRole("banner");
      await expect(topBar.getByText("Musialize")).toBeVisible();
      await expect(topBar.getByRole("presentation")).toHaveJSProperty(
        "complete",
        true,
      );
    });

    await test.step("the menu button opens the drawer", async () => {
      await page.getByRole("button", { name: "Open menu" }).click();
      await expect(
        drawer.getByRole("link", { name: "Suggestions" }),
      ).toBeVisible();
    });

    await test.step("tapping Suggestions closes the drawer", async () => {
      await drawer.getByRole("link", { name: "Suggestions" }).click();
      await expect(drawer).toBeHidden();
    });
  });
});
