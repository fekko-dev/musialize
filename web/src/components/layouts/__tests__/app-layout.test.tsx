import { fireEvent, screen, within } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { renderApp } from "@/testing/render-app";

describe("the sidebar", () => {
  it("links Suggestions to the home page and marks it as the current page there", async () => {
    renderApp("/");

    const navigation = await screen.findByRole("navigation", { name: "Main" });
    const suggestions = within(navigation).getByRole("link", {
      name: "Suggestions",
    });
    expect(suggestions).toHaveAttribute("href", "/");
    expect(suggestions).toHaveAttribute("aria-current", "page");
  });

  it("doesn't mark Suggestions as the current page elsewhere", async () => {
    renderApp("/no-such-page");

    const suggestions = await screen.findByRole("link", {
      name: "Suggestions",
    });
    expect(suggestions).not.toHaveAttribute("aria-current");
  });

  it("folds to a rail and opens again", async () => {
    renderApp("/");

    fireEvent.click(await screen.findByRole("button", { name: "Collapse" }));
    fireEvent.click(screen.getByRole("button", { name: "Expand" }));

    expect(
      screen.getByRole("button", { name: "Collapse" }),
    ).toBeInTheDocument();
  });
});

describe("the sidebar on a phone", () => {
  const desktopWidth = window.innerWidth;
  afterEach(() => {
    window.innerWidth = desktopWidth;
  });

  it("opens as a drawer from the top bar, without a collapse button", async () => {
    window.innerWidth = 390;
    renderApp("/");

    fireEvent.click(await screen.findByRole("button", { name: "Open menu" }));

    const drawer = await screen.findByRole("dialog");
    expect(
      within(drawer).getByRole("link", { name: "Suggestions" }),
    ).toBeInTheDocument();
    expect(
      within(drawer).queryByRole("button", { name: "Collapse" }),
    ).not.toBeInTheDocument();
  });
});
