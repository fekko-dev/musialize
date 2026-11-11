import { useTranslation } from "react-i18next";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useHealth } from "../api/get-health";

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
    return (
      <p role="alert" className="text-destructive">
        {t("health.unreachable")}
      </p>
    );
  }

  return (
    <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1">
      <dt className="text-muted-foreground">{t("health.statusLabel")}</dt>
      <dd>{t(`health.status.${health.data.status}`)}</dd>
      <dt className="text-muted-foreground">{t("health.versionLabel")}</dt>
      <dd className="font-mono break-all">{health.data.version}</dd>
    </dl>
  );
}
