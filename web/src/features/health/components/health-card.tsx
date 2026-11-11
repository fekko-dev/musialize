import { useTranslation } from "react-i18next";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { components } from "@/lib/api-schema.gen";
import { useHealth } from "../api/get-health";

type HealthResponse = components["schemas"]["HealthResponse"];

export function HealthCard() {
  const { t } = useTranslation();

  return (
    <Card role="region" aria-labelledby="health-card-title">
      <CardHeader>
        <CardTitle>
          <h2 id="health-card-title">{t("health.title")}</h2>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <HealthCardContent />
      </CardContent>
    </Card>
  );
}

function HealthCardContent() {
  const { t } = useTranslation();
  const health = useHealth();

  if (health.isPending) {
    return <p className="text-muted-foreground">{t("health.checking")}</p>;
  }

  if (health.isError) {
    // An unhealthy server answers with a 503 and its health in the body. Anything else,
    // such as no answer at all (fetch throws an Error despite the generated type), means
    // the server can't be reached.
    if (isUnhealthyResponse(health.error)) {
      return <HealthDetails health={health.error} />;
    }
    return (
      <p role="alert" className="text-destructive">
        {t("health.unreachable")}
      </p>
    );
  }

  return <HealthDetails health={health.data} />;
}

function isUnhealthyResponse(error: unknown): error is HealthResponse {
  return (
    typeof error === "object" &&
    error !== null &&
    "status" in error &&
    error.status === "unhealthy"
  );
}

function HealthDetails({ health }: { health: HealthResponse }) {
  const { t } = useTranslation();

  return (
    <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1">
      <dt className="text-muted-foreground">{t("health.statusLabel")}</dt>
      <dd className={health.status === "unhealthy" ? "text-destructive" : ""}>
        {t(`health.status.${health.status}`)}
      </dd>
      <dt className="text-muted-foreground">{t("health.versionLabel")}</dt>
      <dd className="font-mono break-all">{health.version}</dd>
      <dt className="text-muted-foreground">{t("health.migrationLabel")}</dt>
      {health.databaseMigration ? (
        <dd className="font-mono break-all">{health.databaseMigration}</dd>
      ) : (
        <dd>{t("health.migrationUnknown")}</dd>
      )}
    </dl>
  );
}
