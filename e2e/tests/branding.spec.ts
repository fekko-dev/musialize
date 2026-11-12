import { readFileSync } from "node:fs";
import { expect, test, type Page } from "@playwright/test";

// app serves Musialize's own branding; app-branded a Group's, set like Dokploy sets it
// (compose.e2e.yaml).
const brandings = [
  {
    title: "without branding settings, the app is Musialize",
    baseURL: "http://localhost:8080",
    appName: "Musialize",
    shortName: "Musialize",
    themeColor: "#ffffff",
    logo: new URL(
      "../../backend/src/Musialize.Api/Branding/Defaults/logo.svg",
      import.meta.url,
    ),
  },
  {
    title: "with branding settings, the app is the Group's",
    baseURL: "http://localhost:8081",
    appName: "The Test Group",
    shortName: "Test Group",
    themeColor: "#123456",
    logo: new URL("../branding/logo.svg", import.meta.url),
  },
];

for (const branding of brandings) {
  test.describe(branding.title, () => {
    test.use({ baseURL: branding.baseURL });

    test("the header, tab and installed app use the branding", async ({
      page,
    }) => {
      await test.step("open the home page", async () => {
        await page.goto("/");
      });

      await test.step("the header shows the logo and name", async () => {
        const header = page.getByRole("banner");
        await expect(
          header.getByRole("heading", { name: branding.appName }),
        ).toBeVisible();
        await expect(header.getByRole("presentation")).toHaveJSProperty(
          "complete",
          true,
        );
      });

      await test.step("the tab shows the name", async () => {
        await expect(page).toHaveTitle(branding.appName);
      });

      await test.step("the logo is the configured one", async () => {
        const logo = await fetchLinked(
          page,
          'link[rel="icon"][type="image/svg+xml"]',
        );
        expect(logo).toEqual(readFileSync(branding.logo));
      });

      await test.step("the manifest installs it with its name and icons", async () => {
        const manifest = JSON.parse(
          (await fetchLinked(page, 'link[rel="manifest"]')).toString(),
        );
        expect(manifest).toMatchObject({
          name: branding.appName,
          short_name: branding.shortName,
          theme_color: branding.themeColor,
          display: "standalone",
        });
        for (const icon of manifest.icons) {
          const response = await page.request.get(icon.src);
          expect(response.ok(), icon.src).toBe(true);
        }
      });

      await test.step("the favicon and Apple touch icon are served", async () => {
        await fetchLinked(page, 'link[rel="icon"][sizes="any"]');
        await fetchLinked(page, 'link[rel="apple-touch-icon"]');
      });
    });
  });
}

// Follows a <link> in the page's <head>, as a browser or phone does.
async function fetchLinked(page: Page, selector: string): Promise<Buffer> {
  const href = await page.locator(selector).getAttribute("href");
  expect(href, selector).toBeTruthy();
  const response = await page.request.get(href!);
  expect(response.ok(), href!).toBe(true);
  return response.body();
}
