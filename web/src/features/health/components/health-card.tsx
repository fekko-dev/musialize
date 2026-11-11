import type { components } from "../../../lib/api-schema.gen";
import { useHealth } from "../api/get-health";

type HealthStatus = components["schemas"]["HealthStatus"];

const statusLabels: Record<HealthStatus, string> = {
  healthy: "Healthy",
};

export function HealthCard() {
  return (
    <section aria-labelledby="health-card-title">
      <h2 id="health-card-title">Server health</h2>
      <HealthCardContent />
    </section>
  );
}

function HealthCardContent() {
  const health = useHealth();

  if (health.isPending) {
    return <p>Checking…</p>;
  }

  if (health.isError) {
    return <p role="alert">The server can't be reached. Try again later.</p>;
  }

  return (
    <dl>
      <dt>Status</dt>
      <dd>{statusLabels[health.data.status]}</dd>
      <dt>Version</dt>
      <dd>{health.data.version}</dd>
    </dl>
  );
}
