import { useBranding } from "../api/get-branding";

// React hoists both tags into <head>.
export function BrandingHead() {
  const branding = useBranding();

  if (!branding.data) {
    return null;
  }

  return (
    <>
      <title>{branding.data.appName}</title>
      {/* iOS labels a home-screen icon with this, not the manifest's short_name. */}
      <meta
        name="apple-mobile-web-app-title"
        content={branding.data.shortName}
      />
    </>
  );
}
