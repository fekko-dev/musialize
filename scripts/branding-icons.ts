// `pnpm branding:icons <folder>/logo.svg` (or logo.png): writes the icons next to the logo, with the
// fixed filenames that Branding__AssetsPath expects (see docs/deployment.md#branding). A PNG logo is
// also wrapped in a logo.svg, because the app always serves logo.svg.
import { spawnSync } from "node:child_process";
import { readFileSync, writeFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const source = process.argv[2];
if (!source || !/\.(svg|png)$/i.test(source)) {
  throw new Error("Usage: pnpm branding:icons <folder>/logo.svg (or logo.png)");
}

if (source.toLowerCase().endsWith(".png")) {
  writeFileSync(path.join(path.dirname(source), "logo.svg"), wrapPng(source));
}

// The generator's own CLI, which reads pwa-assets.config.ts for the sizes and names.
const generatorPackage = fileURLToPath(
  import.meta.resolve("@vite-pwa/assets-generator/package.json"),
);
const { bin } = JSON.parse(readFileSync(generatorPackage, "utf8")) as {
  bin: Record<string, string>;
};
const generator = path.join(
  path.dirname(generatorPackage),
  bin["pwa-assets-generator"]!,
);
const result = spawnSync(
  process.execPath,
  [generator, "--manifest=false", source],
  {
    stdio: "inherit",
  },
);
process.exitCode = result.status ?? 1;

// An SVG that only shows the PNG, at the PNG's own size (read from its IHDR header).
function wrapPng(file: string): string {
  const png = readFileSync(file);
  const width = png.readUInt32BE(16);
  const height = png.readUInt32BE(20);
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${width} ${height}"><image width="${width}" height="${height}" href="data:image/png;base64,${png.toString("base64")}"/></svg>\n`;
}
