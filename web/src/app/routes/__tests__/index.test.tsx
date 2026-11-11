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

  it("shows the status and version when the server is healthy", async () => {
    server.use(
      http.get("/health", ({ response }) =>
        response(200).json({ status: "healthy", version: "abc1234" }),
      ),
    );

    renderApp("/");

    expect(await screen.findByText("Healthy")).toBeInTheDocument();
    expect(screen.getByText("abc1234")).toBeInTheDocument();
  });

  it("says the server can't be reached when the request fails", async () => {
    server.use(http.get("/health", () => HttpResponse.error()));

    renderApp("/");

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "The server can't be reached.",
    );
  });
});
