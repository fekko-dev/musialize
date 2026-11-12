/// <reference types="vitest/config" />
import path from "node:path";
import tailwindcss from "@tailwindcss/vite";
import { tanstackRouter } from "@tanstack/router-plugin/vite";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// Must match applicationUrl in backend/src/Musialize.Api/Properties/launchSettings.json.
const backendUrl = "http://localhost:5070";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    // The router plugin must come before the React plugin.
    tanstackRouter({
      target: "react",
      routesDirectory: "./src/app/routes",
      generatedRouteTree: "./src/app/routeTree.gen.ts",
      // Tests live next to their route in __tests__ folders, as in Bulletproof React.
      routeFileIgnorePattern: "__tests__",
      autoCodeSplitting: true,
    }),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: { "@": path.resolve(import.meta.dirname, "./src") },
  },
  server: {
    proxy: {
      "/api": backendUrl,
      "/health": backendUrl,
      "/branding": backendUrl,
      "/manifest.webmanifest": backendUrl,
    },
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./src/testing/setup.ts"],
  },
});
