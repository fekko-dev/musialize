import { QueryClientProvider, type QueryClient } from "@tanstack/react-query";
import { RouterProvider } from "@tanstack/react-router";
import { I18nextProvider } from "react-i18next";
import { i18n } from "@/lib/i18n";
import type { AppRouter } from "./router";

type AppProps = {
  router: AppRouter;
  queryClient: QueryClient;
};

export function App({ router, queryClient }: AppProps) {
  return (
    <I18nextProvider i18n={i18n}>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </I18nextProvider>
  );
}
