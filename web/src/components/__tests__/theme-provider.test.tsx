import { fireEvent, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { renderApp } from "@/testing/render-app";

// jsdom's device setting is light (see testing/setup.ts).
describe("the theme", () => {
  afterEach(() => {
    vi.restoreAllMocks();
    localStorage.clear();
    document.documentElement.classList.remove("light", "dark");
  });

  it("follows the device when the browser blocks storage", async () => {
    const blocked = () => {
      throw new DOMException("Storage is blocked", "SecurityError");
    };
    vi.spyOn(Storage.prototype, "getItem").mockImplementation(blocked);
    vi.spyOn(Storage.prototype, "setItem").mockImplementation(blocked);

    renderApp("/");

    expect(await screen.findByRole("button", { name: "Theme" })).toBeVisible();
    expect(document.documentElement).toHaveClass("light");

    // Picking a theme still works for this visit, it just isn't remembered.
    fireEvent.click(screen.getByRole("button", { name: "Theme" }));
    fireEvent.click(await screen.findByRole("menuitem", { name: "Dark" }));
    expect(document.documentElement).toHaveClass("dark");
  });

  it("treats an unknown stored value as System, like index.html does", async () => {
    localStorage.setItem("vite-ui-theme", "blue");

    renderApp("/");

    await screen.findByRole("button", { name: "Theme" });
    expect(document.documentElement).toHaveClass("light");
    expect(document.documentElement).not.toHaveClass("blue");
  });
});
