import { screen } from "@testing-library/react";
import { HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { http } from "@/testing/http";
import { renderApp } from "@/testing/render-app";
import { server } from "@/testing/server";

const songId = "0197a000-0000-7000-8000-000000000001";

describe("Song page", () => {
  it("shows the title, artist, link and date", async () => {
    server.use(
      http.get("/api/songs/{id}", ({ response }) =>
        response(200).json({
          id: songId,
          title: "Wonderwall",
          artist: "Oasis",
          link: "https://www.youtube.com/watch?v=bx1Bh8ZvH84",
          suggestedAt: "2026-10-22T12:00:00Z",
        }),
      ),
    );

    renderApp(`/songs/${songId}`);

    expect(
      await screen.findByRole("heading", { name: "Wonderwall" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Oasis")).toBeInTheDocument();
    const link = screen.getByRole("link", {
      name: "www.youtube.com/watch?v=bx1Bh8ZvH84",
    });
    expect(link).toHaveAttribute(
      "href",
      "https://www.youtube.com/watch?v=bx1Bh8ZvH84",
    );
    expect(link).toHaveAttribute("target", "_blank");
    expect(screen.getByText("Suggested on Oct 22, 2026")).toBeInTheDocument();
  });

  it("replaces the shell's top bar with a back button on a phone", async () => {
    renderApp(`/songs/${songId}`);

    expect(await screen.findByRole("link", { name: "Back" })).toHaveAttribute(
      "href",
      "/",
    );
    expect(
      screen.queryByRole("button", { name: "Open menu" }),
    ).not.toBeInTheDocument();
  });

  it("says the Song can't be loaded when the Group has no such Song", async () => {
    server.use(
      // With a ProblemDetails body, as UseStatusCodePages gives every bodyless error.
      http.get("/api/songs/{id}", () =>
        HttpResponse.json({ title: "Not Found", status: 404 }, { status: 404 }),
      ),
    );

    renderApp(`/songs/${songId}`);

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "This Song can't be shown.",
    );
  });
});
