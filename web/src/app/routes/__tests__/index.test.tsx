import { screen } from "@testing-library/react";
import { delay, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { http } from "@/testing/http";
import { renderApp } from "@/testing/render-app";
import { server } from "@/testing/server";

describe("home page", () => {
  it("shows that it's checking while the health request is pending", async () => {
    server.use(
      http.get("/health", async () => {
        await delay("infinite");
      }),
    );

    renderApp("/");

    expect(await screen.findByText("Checking…")).toBeInTheDocument();
  });

  it("shows the status, version and latest migration when the server is healthy", async () => {
    server.use(
      http.get("/health", ({ response }) =>
        response(200).json({
          status: "healthy",
          version: "abc1234",
          databaseMigration: "20261111120000_InitialCreate",
        }),
      ),
    );

    renderApp("/");

    expect(await screen.findByText("Healthy")).toBeInTheDocument();
    expect(screen.getByText("abc1234")).toBeInTheDocument();
    expect(
      screen.getByText("20261111120000_InitialCreate"),
    ).toBeInTheDocument();
  });

  it("shows an unhealthy server as reachable, with its version and migration", async () => {
    server.use(
      http.get("/health", ({ response }) =>
        response(503).json({
          status: "unhealthy",
          version: "abc1234",
          databaseMigration: null,
        }),
      ),
    );

    renderApp("/");

    expect(await screen.findByText("Unhealthy")).toBeInTheDocument();
    expect(screen.getByText("abc1234")).toBeInTheDocument();
    expect(screen.getByText("Unknown")).toBeInTheDocument();
    expect(screen.queryByText(/can't be reached/)).not.toBeInTheDocument();
  });

  it("says the server can't be reached when the request fails", async () => {
    server.use(http.get("/health", () => HttpResponse.error()));

    renderApp("/");

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "The server can't be reached.",
    );
  });
});
