import { QueryClient } from "@tanstack/react-query";
import { createMemoryHistory } from "@tanstack/react-router";
import { render } from "@testing-library/react";
import { App } from "../app/app";
import { createAppRouter } from "../app/router";

export function renderApp(path: string) {
  // A fresh client per test, without retries, as TanStack Query's testing guide recommends.
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  const router = createAppRouter(
    createMemoryHistory({ initialEntries: [path] }),
  );
  return render(<App router={router} queryClient={queryClient} />);
}
