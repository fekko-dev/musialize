import { defineConfig, devices } from "@playwright/test";

// Runs against compose.e2e.yaml, which `pnpm test:e2e` starts (see run.ts).
export default defineConfig({
  testDir: "./tests",
  forbidOnly: !!process.env.CI,
  // Playwright's recommended CI settings: one worker for stable, reproducible runs, and
  // retries so a one-off hiccup on a shared runner doesn't fail the pull request.
  workers: process.env.CI ? 1 : undefined,
  retries: process.env.CI ? 2 : 0,
  reporter: "list",
  use: {
    baseURL: "http://localhost:8080",
    trace: "retain-on-failure",
  },
  // Every test runs at both widths, so no test sets a viewport itself.
  projects: [
    { name: "desktop", use: { ...devices["Desktop Chrome"] } },
    { name: "phone", use: { ...devices["Pixel 7"] } },
  ],
});
