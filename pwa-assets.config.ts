// The sizes and filenames for `pnpm branding:icons` (scripts/branding-icons.ts), which writes the
// icons next to the logo with the names Branding__AssetsPath expects (docs/deployment.md#branding).
import { defineConfig } from "@vite-pwa/assets-generator/config";

// iOS fills a transparent Apple touch icon with black, and a maskable icon fills its whole shape,
// so both get a solid background.
const background = { resizeOptions: { background: "white" } };

export default defineConfig({
  preset: {
    transparent: { sizes: [192, 512], favicons: [[48, "favicon.ico"]] },
    // Android may crop it to a circle, so the logo stays inside the middle 80% (the safe zone).
    maskable: { sizes: [512], padding: 0.3, ...background },
    // iOS only rounds the corners, so the logo can fill more of the tile.
    apple: { sizes: [180], padding: 0.12, ...background },
    assetName: (type, size) =>
      ({
        transparent: `icon-${size.width}.png`,
        maskable: `icon-maskable-${size.width}.png`,
        apple: "apple-touch-icon.png",
      })[type],
  },
});
