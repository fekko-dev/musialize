import { defineConfig, devices } from "@playwright/test";

// Runs against compose.e2e.yaml, which `pnpm test:e2e` starts (see run.ts).
export default defineConfig({
  testDir: "./tests",
  forbidOnly: !!process.env.CI,
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
