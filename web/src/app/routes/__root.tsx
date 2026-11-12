import { createRootRoute, Outlet } from "@tanstack/react-router";
import { AppHeader } from "@/features/branding/components/app-header";

export const Route = createRootRoute({
  component: RootLayout,
});

function RootLayout() {
  return (
    <>
      <AppHeader />
      <Outlet />
    </>
  );
}
