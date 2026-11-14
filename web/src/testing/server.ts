import { setupServer } from "msw/node";
import { http } from "./http";

// Answers every page needs, like the real server's defaults. A test overrides one with
// server.use(); resetHandlers() after each test brings these back.
export const server = setupServer(
  http.get("/api/branding", ({ response }) =>
    response(200).json({ appName: "Musialize", shortName: "Musialize" }),
  ),
  http.get("/api/songs", ({ response }) => response(200).json([])),
);
