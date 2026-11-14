import { screen, waitFor, within } from "@testing-library/react";
import { delay, HttpResponse } from "msw";
import { describe, expect, it } from "vitest";
import { http } from "@/testing/http";
import { renderApp } from "@/testing/render-app";
import { server } from "@/testing/server";

describe("every page", () => {
  it("shows the Group's logo and name in the header and the browser tab", async () => {
    server.use(
      http.get("/api/branding", ({ response }) =>
        response(200).json({
          appName: "The Test Group",
          shortName: "Test Group",
        }),
      ),
    );

    renderApp("/");

    const header = await screen.findByRole("banner");
    expect(
      await within(header).findByText("The Test Group"),
    ).toBeInTheDocument();
    expect(within(header).getByRole("presentation")).toHaveAttribute(
      "src",
      "/branding/logo.svg",
    );
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

    const header = await screen.findByRole("banner");
    expect(within(header).getByRole("presentation")).toBeInTheDocument();
    expect(within(header).queryByRole("heading")).not.toBeInTheDocument();
    expect(document.title).toBe(title);
  });

  // The logo is a file of its own, so it still shows; the status page's health card says the server is down.
  it("shows only the logo when the server can't be reached", async () => {
    server.use(
      http.get("/api/branding", () => HttpResponse.error()),
      http.get("/health", () => HttpResponse.error()),
    );
    const title = document.title;

    renderApp("/status");

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "The server can't be reached.",
    );
    const header = screen.getByRole("banner");
    expect(within(header).getByRole("presentation")).toBeInTheDocument();
    expect(within(header).queryByRole("heading")).not.toBeInTheDocument();
    expect(document.title).toBe(title);
  });
});
