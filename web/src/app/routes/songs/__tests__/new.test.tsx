import { fireEvent, screen } from "@testing-library/react";
import { HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { http } from "@/testing/http";
import { renderApp } from "@/testing/render-app";
import { server } from "@/testing/server";

const song = {
  id: "0197a000-0000-7000-8000-000000000001",
  title: "Wonderwall",
  artist: "Oasis",
  link: "https://example.com/wonderwall",
  suggestedAt: "2026-11-16T12:00:00Z",
};

async function fillIn(label: string, value: string) {
  const input = await screen.findByLabelText(label);
  fireEvent.change(input, { target: { value } });
  fireEvent.blur(input);
}

async function suggest(title: string, artist: string, link: string) {
  await fillIn("Title", title);
  await fillIn("Artist", artist);
  await fillIn("Link to the song", link);
  fireEvent.click(screen.getByRole("button", { name: "Suggest" }));
}

describe("Suggest a Song", () => {
  it("lands on the new Song's page", async () => {
    let sent: unknown;
    server.use(
      http.post("/api/songs", async ({ request, response }) => {
        sent = await request.json();
        return response(201).json(song);
      }),
      http.get("/api/songs/{id}", ({ response }) => response(200).json(song)),
    );
    renderApp("/songs/new");

    await suggest("Wonderwall", "Oasis", "https://example.com/wonderwall");

    expect(
      await screen.findByRole("heading", { name: "Wonderwall" }),
    ).toBeInTheDocument();
    expect(sent).toEqual({
      title: "Wonderwall",
      artist: "Oasis",
      link: "https://example.com/wonderwall",
    });
  });

  it("checks each field when it loses focus", async () => {
    renderApp("/songs/new");

    await fillIn("Title", " ");
    await fillIn("Artist", "a".repeat(201));
    await fillIn("Link to the song", "example.com/song");

    expect(await screen.findByText("Fill this in.")).toBeInTheDocument();
    expect(screen.getByText("Use at most 200 characters.")).toBeInTheDocument();
    expect(
      screen.getByText("Enter a full link, starting with https://."),
    ).toBeInTheDocument();
  });

  it("shows the fields the API rejects", async () => {
    server.use(
      http.post("/api/songs", ({ response }) =>
        response(400).json({ errors: { link: ["Not a song."] } }),
      ),
    );
    renderApp("/songs/new");

    await suggest("Wonderwall", "Oasis", "https://example.com/wonderwall");

    expect(await screen.findByText("Check this field.")).toBeInTheDocument();
    expect(screen.getByLabelText("Link to the song")).toHaveAttribute(
      "aria-invalid",
      "true",
    );
  });

  it("says the Song can't be suggested when a 400 names no field", async () => {
    server.use(
      http.post("/api/songs", ({ response }) =>
        response(400).json({ errors: { "": ["Unreadable."] } }),
      ),
    );
    renderApp("/songs/new");

    await suggest("Wonderwall", "Oasis", "https://example.com/wonderwall");

    expect(
      await screen.findByText("The Song can't be suggested. Try again later."),
    ).toBeInTheDocument();
  });

  it("says the Song can't be suggested when the request fails", async () => {
    server.use(http.post("/api/songs", () => HttpResponse.error()));
    renderApp("/songs/new");

    await suggest("Wonderwall", "Oasis", "https://example.com/wonderwall");

    expect(
      await screen.findByText("The Song can't be suggested. Try again later."),
    ).toBeInTheDocument();
  });

  it("closes to the Overview", async () => {
    renderApp("/songs/new");

    const close = await screen.findAllByRole("link", { name: "Close" });
    expect(close[0]).toHaveAttribute("href", "/");
  });
});
