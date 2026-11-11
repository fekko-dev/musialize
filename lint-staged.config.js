import { relative } from "node:path";

// Globs don't overlap, so no two tasks write to the same file at once.
// Type checking and tests run in CI, not on commit.
//
// Known issue (lint-staged 17.6, tinyexec 1.3.1): with a large unstaged diff
// (over ~64 KB) in files with multi-byte characters (emoji, "…"), lint-staged
// can stage and commit an unstaged file it didn't touch. tinyexec decodes
// git's output chunk by chunk, which sometimes garbles a character on a chunk
// boundary. Check `git show --stat` after big commits.
export default {
  "*.{js,ts,tsx}": [
    "eslint --max-warnings 0 --no-warn-ignored --fix",
    "prettier --write",
  ],
  "*.{json,md,yaml,yml,css,html}": "prettier --write",
  // dotnet format ignores absolute --include paths; relative ones work.
  "*.cs": (files) =>
    `dotnet format backend/Musialize.slnx --include ${files.map((file) => `"${relative(process.cwd(), file)}"`).join(" ")}`,
};
