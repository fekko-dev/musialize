import { createFileRoute } from "@tanstack/react-router";
import { HealthCard } from "@/features/health/components/health-card";

// The server's health, version and latest migration, for checking a deploy.
export const Route = createFileRoute("/status")({
  component: StatusPage,
});

function StatusPage() {
  return (
    <main className="mx-auto flex w-full max-w-md flex-col gap-6 px-4 py-8">
      <HealthCard />
    </main>
  );
}
