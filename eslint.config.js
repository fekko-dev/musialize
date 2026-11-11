import { readdirSync } from "node:fs";
import js from "@eslint/js";
import { defineConfig, globalIgnores } from "eslint/config";
import { createTypeScriptImportResolver } from "eslint-import-resolver-typescript";
import { importX } from "eslint-plugin-import-x";
import reactHooks from "eslint-plugin-react-hooks";
import { reactRefresh } from "eslint-plugin-react-refresh";
import globals from "globals";
import tseslint from "typescript-eslint";

const webSrc = `${import.meta.dirname}/web/src`;
const features = readdirSync(`${webSrc}/features`);

export default defineConfig([
  globalIgnores(["**/dist", "**/*.gen.ts"]),
  {
    files: ["**/*.{js,ts,tsx}"],
    extends: [js.configs.recommended, tseslint.configs.recommended],
    languageOptions: { globals: globals.node },
  },
  {
    files: ["web/src/**/*.{ts,tsx}"],
    extends: [reactHooks.configs.flat.recommended, reactRefresh.configs.vite()],
    languageOptions: { globals: globals.browser },
  },
  {
    // Imports flow one way, shared → features → app, and features don't import each other
    // (as in Bulletproof React). `testing` sits outside this: it renders the whole app.
    files: ["web/src/**/*.{ts,tsx}"],
    plugins: { "import-x": importX },
    settings: {
      "import-x/resolver-next": [
        createTypeScriptImportResolver({ project: "web/tsconfig.app.json" }),
      ],
    },
    rules: {
      "import-x/no-restricted-paths": [
        "error",
        {
          basePath: webSrc,
          zones: [
            ...features.map((feature) => ({
              target: `./features/${feature}`,
              from: "./features",
              except: [`./${feature}`],
              message: "Features don't import each other.",
            })),
            {
              target: "./features",
              from: "./app",
              message: "Features don't import from the app layer.",
            },
            {
              target: [
                "./components",
                "./hooks",
                "./lib",
                "./locales",
                "./types",
                "./utils",
              ],
              from: ["./features", "./app"],
              message: "Shared code doesn't import features or the app layer.",
            },
          ],
        },
      ],
    },
  },
  {
    // Route files export `Route` and keep their component local. The router
    // plugin's autoCodeSplitting moves the component into its own module.
    files: ["web/src/app/routes/**/*.tsx"],
    rules: { "react-refresh/only-export-components": "off" },
  },
]);
