// `pnpm test:e2e`: builds the production image (or takes the one in MUSIALIZE_IMAGE), starts
// compose.e2e.yaml, runs Playwright against it, and always tears everything down.
//
//   MUSIALIZE_IMAGE  an image to test instead of building one (CI passes the image job's)
//   APP_VERSION      the short SHA it was built with; defaults to the current commit's
import { execFileSync, spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("..", import.meta.url));
const playwright = fileURLToPath(import.meta.resolve("@playwright/test/cli"));

const givenImage = process.env.MUSIALIZE_IMAGE;
if (givenImage && !process.env.APP_VERSION) {
  throw new Error(
    "Set APP_VERSION to the version MUSIALIZE_IMAGE was built with.",
  );
}
const image = givenImage ?? "musialize:e2e";
const version =
  process.env.APP_VERSION ??
  execFileSync("git", ["rev-parse", "--short", "HEAD"], {
    encoding: "utf8",
  }).trim();
const env = { ...process.env, MUSIALIZE_IMAGE: image, APP_VERSION: version };

function run(command: string, args: string[], cwd = root): boolean {
  return spawnSync(command, args, { cwd, env, stdio: "inherit" }).status === 0;
}

const compose = ["compose", "--file", "compose.e2e.yaml"];

if (!givenImage) {
  const built = run("docker", [
    "build",
    "--build-arg",
    `APP_VERSION=${version}`,
    "--tag",
    image,
    ".",
  ]);
  if (!built) process.exit(1);
}

try {
  const passed =
    run("docker", [...compose, "up", "--detach", "--wait"]) &&
    run(
      process.execPath,
      [playwright, "test"],
      fileURLToPath(new URL(".", import.meta.url)),
    );
  process.exitCode = passed ? 0 : 1;
} finally {
  run("docker", [...compose, "down", "--volumes"]);
}
