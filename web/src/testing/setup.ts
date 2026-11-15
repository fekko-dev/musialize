import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterAll, afterEach } from "vitest";
import { server } from "./server";

// jsdom doesn't implement scrolling; the router calls it on every navigation.
window.scrollTo = () => {};

// jsdom has no matchMedia. useIsMobile only listens to it and reads window.innerWidth,
// which a test can lower to act as a phone.
window.matchMedia = (query) =>
  ({
    matches: false,
    media: query,
    onchange: null,
    addEventListener: () => {},
    removeEventListener: () => {},
    addListener: () => {},
    removeListener: () => {},
    dispatchEvent: () => false,
  }) satisfies MediaQueryList;

// openapi-fetch captures `fetch` when the client is created, so MSW must patch it
// before any test module imports the client (https://openapi-ts.dev/openapi-fetch/testing).
server.listen({ onUnhandledFrame: "error" });

afterEach(() => {
  cleanup();
  server.resetHandlers();
});

afterAll(() => {
  server.close();
});
