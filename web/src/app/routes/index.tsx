import { createFileRoute } from "@tanstack/react-router";
import { HealthCard } from "../../features/health/components/health-card";

export const Route = createFileRoute("/")({
  component: HomePage,
});

function HomePage() {
  return (
    <main>
      <h1>Musialize</h1>
      <HealthCard />
    </main>
  );
}
