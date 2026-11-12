import { createFileRoute } from "@tanstack/react-router";
import { HealthCard } from "@/features/health/components/health-card";

export const Route = createFileRoute("/")({
  component: HomePage,
});

function HomePage() {
  return (
    <main className="mx-auto flex w-full max-w-md flex-col gap-6 px-4 py-8">
      <HealthCard />
    </main>
  );
}
