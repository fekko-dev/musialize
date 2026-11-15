import { screen, waitFor, within } from "@testing-library/react";
import { delay, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { http } from "@/testing/http";
import { renderApp } from "@/testing/render-app";
import { server } from "@/testing/server";

describe("every page", () => {
  it("shows the App's logo and name in the sidebar, the phone's top bar and the browser tab", async () => {
    server.use(
      http.get("/api/branding", ({ response }) =>
        response(200).json({
          appName: "The Test Group",
          shortName: "Test Group",
        }),
      ),
    );

    renderApp("/");

    // Once in the sidebar, once in the phone's top bar.
    expect(await screen.findAllByText("The Test Group")).toHaveLength(2);
    const topBar = within(screen.getByRole("banner"));
    expect(topBar.getByText("The Test Group")).toBeInTheDocument();
    for (const logo of screen.getAllByRole("presentation")) {
      expect(logo).toHaveAttribute("src", "/branding/logo.svg");
    }
    expect(topBar.getByRole("presentation")).toBeInTheDocument();
    await waitFor(() => expect(document.title).toBe("The Test Group"));
    // The label under the icon when an iPhone adds the app to its home screen.
    expect(
      document.querySelector('meta[name="apple-mobile-web-app-title"]'),
    ).toHaveAttribute("content", "Test Group");
  });

  it("shows only the logo while the branding is loading", async () => {
    server.use(
      http.get("/api/branding", async () => {
        await delay("infinite");
      }),
    );
    const title = document.title;

    renderApp("/");

    expect(await screen.findAllByRole("presentation")).toHaveLength(2);
    expect(screen.queryByText("The Test Group")).not.toBeInTheDocument();
    expect(document.title).toBe(title);
  });

  // The logo is a file of its own, so it still shows; the health card says the server is down.
  it("shows only the logo when the server can't be reached", async () => {
    server.use(
      http.get("/api/branding", () => HttpResponse.error()),
      http.get("/health", () => HttpResponse.error()),
    );
    const title = document.title;

    renderApp("/");

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "The server can't be reached.",
    );
    expect(screen.getAllByRole("presentation")).toHaveLength(2);
    expect(screen.queryByText("The Test Group")).not.toBeInTheDocument();
    expect(document.title).toBe(title);
  });
});
