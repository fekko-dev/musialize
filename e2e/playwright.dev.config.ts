import { defineConfig } from "@playwright/test";
import config from "./playwright.config.ts";

// `pnpm test:e2e:dev`: the same tests against `pnpm dev`, which Playwright reuses when it's
// running and starts otherwise (https://playwright.dev/docs/test-webserver). Tests tagged
// @image need the production image and run only with `pnpm test:e2e`.
export default defineConfig({
  ...config,
  grepInvert: /@image/,
  use: { ...config.use, baseURL: "http://localhost:5173" },
  webServer: {
    command: "pnpm dev",
    cwd: "..",
    // Through Vite's proxy, so the API is up too.
    url: "http://localhost:5173/health",
    reuseExistingServer: true,
    // `pnpm dev` also starts PostgreSQL in Docker and migrates first.
    timeout: 180_000,
  },
});
