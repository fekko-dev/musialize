// `pnpm check:commits`: checks every commit since the branch left main on its own, oldest first,
// as CONTRIBUTING asks. CI only checks a pull request's last commit. Lint runs for each commit,
// the web and backend tests only for commits that touch web/ or backend/. It checks the commits
// out one by one in this working tree and returns to where it started.
import { spawnSync } from "node:child_process";

const git = (...args: string[]) => {
  const result = spawnSync("git", args, { encoding: "utf8" });
  if (result.status !== 0) {
    throw new Error(`git ${args.join(" ")} failed: ${result.stderr.trim()}`);
  }
  return result.stdout.trim();
};

// pnpm is pnpm.cmd on Windows, which only starts through a shell.
const pnpm = (...args: string[]) =>
  spawnSync("pnpm", args, {
    stdio: "inherit",
    shell: process.platform === "win32",
  }).status === 0;

if (git("status", "--porcelain")) {
  throw new Error(
    "Commit or set aside your changes first: this checks out each commit.",
  );
}

const start =
  git("symbolic-ref", "--short", "-q", "HEAD") || git("rev-parse", "HEAD");
const base = git("merge-base", "origin/main", "HEAD");
const commits = git("rev-list", "--reverse", `${base}..HEAD`)
  .split("\n")
  .filter(Boolean);
let dependenciesChanged = false;
let failed: string | undefined;

try {
  for (const commit of commits) {
    const subject = git("log", "-1", "--format=%h %s", commit);
    const files = git(
      "diff-tree",
      "--no-commit-id",
      "--name-only",
      "-r",
      commit,
    ).split("\n");
    console.log(`\n=== ${subject}`);
    git("checkout", "--quiet", "--detach", commit);

    if (files.includes("pnpm-lock.yaml")) {
      dependenciesChanged = true;
    }
    const checks = [
      ...(dependenciesChanged ? [["install", "--frozen-lockfile"]] : []),
      ["lint"],
      ...(files.some((file) => file.startsWith("web/")) ? [["test:web"]] : []),
      ...(files.some((file) => file.startsWith("backend/"))
        ? [["test:backend"]]
        : []),
    ];
    const failedCheck = checks.find((check) => !pnpm(...check));
    if (failedCheck) {
      failed = `${subject}: pnpm ${failedCheck.join(" ")} failed`;
      break;
    }
  }
} finally {
  git("checkout", "--quiet", start);
  if (dependenciesChanged) {
    pnpm("install", "--frozen-lockfile");
  }
}

if (failed) {
  console.error(`\n✗ ${failed}`);
  process.exitCode = 1;
} else {
  const result =
    commits.length === 1
      ? "commit passes on its own"
      : "commits pass on their own";
  console.log(`\n✓ ${commits.length} ${result}`);
}
