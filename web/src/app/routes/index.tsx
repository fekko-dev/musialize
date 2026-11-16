import { PlusIcon } from "@phosphor-icons/react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { cn } from "cn";
import { useTranslation } from "react-i18next";
import { buttonVariants } from "@/components/ui/button";
import { SuggestionList } from "@/features/songs/components/suggestion-list";

export const Route = createFileRoute("/")({
  component: OverviewPage,
});

function OverviewPage() {
  const { t } = useTranslation();

  return (
    // On a phone the bottom padding keeps the floating button off the last row.
    <main className="flex w-full max-w-3xl flex-col gap-6 px-4 pt-8 pb-28 md:px-10 md:pb-8">
      <SuggestionList />
      {/* Phone only: on a desktop the button sits by the heading. */}
      <Link
        to="/songs/new"
        aria-label={t("suggest.title")}
        className={cn(
          buttonVariants(),
          "fixed right-4 bottom-6 size-14 shadow-lg md:hidden",
        )}
      >
        <PlusIcon className="size-6" />
      </Link>
    </main>
  );
}
