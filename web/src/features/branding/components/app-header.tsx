import { useBranding } from "../api/get-branding";

// The logo and icons come from the deployment's branding folder (docs/deployment.md#branding).
export function AppHeader() {
  const branding = useBranding();

  return (
    <header className="mx-auto flex w-full max-w-md items-center gap-3 px-4 pt-6">
      {/* Decorative: the name next to it says the same. */}
      <img src="/branding/logo.svg" alt="" className="size-10" />
      {branding.data && (
        <>
          {/* React hoists both into <head>. */}
          <title>{branding.data.appName}</title>
          {/* iOS labels a home-screen icon with this, not the manifest's short_name. */}
          <meta
            name="apple-mobile-web-app-title"
            content={branding.data.shortName}
          />
          <h1 className="font-heading text-2xl font-semibold">
            {branding.data.appName}
          </h1>
        </>
      )}
    </header>
  );
}
