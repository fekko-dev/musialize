import { createRootRoute, Outlet } from "@tanstack/react-router";
import { AppLayout } from "@/components/layouts/app-layout";
import { useBranding } from "@/features/branding/api/get-branding";
import { BrandingHead } from "@/features/branding/components/branding-head";

export const Route = createRootRoute({
  component: RootLayout,
});

function RootLayout() {
  const branding = useBranding();

  return (
    <>
      <BrandingHead />
      <AppLayout appName={branding.data?.appName}>
        <Outlet />
      </AppLayout>
    </>
  );
}
