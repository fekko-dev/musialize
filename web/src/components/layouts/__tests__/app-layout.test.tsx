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

describe("the theme menu", () => {
  afterEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove("light", "dark");
  });

  async function pickTheme(name: string) {
    fireEvent.click(await screen.findByRole("button", { name: "Theme" }));
    fireEvent.click(await screen.findByRole("menuitem", { name }));
  }

  it("switches to dark and remembers it in this browser", async () => {
    renderApp("/");

    await pickTheme("Dark");

    expect(document.documentElement).toHaveClass("dark");
    expect(localStorage.getItem("vite-ui-theme")).toBe("dark");
  });

  // jsdom's device setting is light (see testing/setup.ts).
  it("goes back to the device's setting with System", async () => {
    localStorage.setItem("vite-ui-theme", "dark");
    renderApp("/");

    await pickTheme("System");

    expect(document.documentElement).toHaveClass("light");
    expect(localStorage.getItem("vite-ui-theme")).toBe("system");
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
