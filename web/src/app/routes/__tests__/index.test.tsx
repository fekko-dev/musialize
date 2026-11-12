import { screen, within } from "@testing-library/react";
import { delay, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { http } from "@/testing/http";
import { renderApp } from "@/testing/render-app";
import { server } from "@/testing/server";

describe("Overview", () => {
  it("lists the Suggestions in the order the API returns them, with artist and date", async () => {
    server.use(
      http.get("/api/songs", ({ response }) =>
        response(200).json([
          {
            id: "0197a000-0000-7000-8000-000000000001",
            title: "Wonderwall",
            artist: "Oasis",
            link: "https://example.com/wonderwall",
            suggestedAt: "2026-10-22T12:00:00Z",
          },
          {
            id: "0197a000-0000-7000-8000-000000000002",
            title: "Africa",
            artist: "Toto",
            link: "https://example.com/africa",
            suggestedAt: "2026-11-01T12:00:00Z",
          },
        ]),
      ),
    );

    renderApp("/");

    const suggestions = await screen.findByRole("region", {
      name: "Suggestions",
    });
    const rows = await within(suggestions).findAllByRole("listitem");
    expect(rows.map((row) => row.firstChild?.textContent)).toEqual([
      "Wonderwall",
      "Africa",
    ]);
    expect(rows[0]).toHaveTextContent("Oasis · Suggested on Oct 22, 2026");
  });

  it("says so when there are no Suggestions yet", async () => {
    server.use(
      http.get("/api/songs", ({ response }) => response(200).json([])),
    );

    renderApp("/");

    expect(await screen.findByText("No Suggestions yet.")).toBeInTheDocument();
  });

  it("shows that it's loading while the request is pending", async () => {
    server.use(
      http.get("/api/songs", async () => {
        await delay("infinite");
      }),
    );

    renderApp("/");

    expect(await screen.findByText("Loading…")).toBeInTheDocument();
  });

  it("says the Suggestions can't be loaded when the request fails", async () => {
    server.use(http.get("/api/songs", () => HttpResponse.error()));

    renderApp("/");

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "The Suggestions can't be loaded.",
    );
  });
});
