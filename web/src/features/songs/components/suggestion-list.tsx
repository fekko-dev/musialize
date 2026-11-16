import { PlusIcon } from "@phosphor-icons/react";
import { Link } from "@tanstack/react-router";
import { cn } from "cn";
import { useTranslation } from "react-i18next";
import { Alert, AlertTitle } from "@/components/ui/alert";
import { buttonVariants } from "@/components/ui/button";
import { Empty, EmptyHeader, EmptyTitle } from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { useSuggestions } from "../api/get-suggestions";

// The Group's Suggestions, oldest first: the order the API returns them in.
export function SuggestionList() {
  const { t } = useTranslation();

  return (
    <section
      aria-labelledby="suggestions-title"
      className="flex flex-col gap-4"
    >
      <div className="flex items-center justify-between gap-4">
        <h2 id="suggestions-title" className="text-xl font-semibold">
          {t("suggestions.title")}
        </h2>
        {/* Desktop only: phones get the floating button on the Overview. */}
        <Link
          to="/songs/new"
          className={cn(
            buttonVariants({ size: "lg" }),
            "hidden md:inline-flex",
          )}
        >
          <PlusIcon data-icon="inline-start" />
          {t("suggest.title")}
        </Link>
      </div>
      <SuggestionListContent />
    </section>
  );
}

function SuggestionListContent() {
  const { t } = useTranslation();
  const suggestions = useSuggestions();

  if (suggestions.isPending) {
    // Added to shadcn's Skeleton: role="status" and a hidden label, so screen readers hear it's loading.
    return (
      <div role="status" className="flex flex-col gap-4 py-3">
        <span className="sr-only">{t("suggestions.loading")}</span>
        {[0, 1, 2].map((row) => (
          <div key={row} className="flex flex-col gap-2">
            <Skeleton className="h-5 w-48" />
            <Skeleton className="h-4 w-64" />
          </div>
        ))}
      </div>
    );
  }

  if (suggestions.isError) {
    return (
      <Alert variant="destructive">
        <AlertTitle>{t("suggestions.unavailable")}</AlertTitle>
      </Alert>
    );
  }

  if (suggestions.data.length === 0) {
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>{t("suggestions.empty")}</EmptyTitle>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <ul className="divide-y">
      {suggestions.data.map((song) => (
        <li key={song.id} className="flex flex-col gap-1 py-3">
          <span className="font-semibold">{song.title}</span>
          <span className="text-sm text-muted-foreground">
            {t("suggestions.byline", {
              artist: song.artist,
              date: new Date(song.suggestedAt),
            })}
          </span>
        </li>
      ))}
    </ul>
  );
}
