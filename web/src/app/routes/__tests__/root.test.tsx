import { screen, waitFor, within } from "@testing-library/react";
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
});
