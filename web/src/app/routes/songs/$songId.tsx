import { ArrowLeftIcon } from "@phosphor-icons/react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { PhoneSubpageBar } from "@/components/layouts/phone-subpage-bar";
import { buttonVariants } from "@/components/ui/button";
import { SongDetails } from "@/features/songs/components/song-details";

export const Route = createFileRoute("/songs/$songId")({
  component: SongPage,
  staticData: { subpage: true },
});

function SongPage() {
  const { t } = useTranslation();
  const { songId } = Route.useParams();

  return (
    <>
      <PhoneSubpageBar>
        <Link
          to="/"
          aria-label={t("song.back")}
          className={buttonVariants({ variant: "ghost", size: "icon-lg" })}
        >
          <ArrowLeftIcon className="size-5" />
        </Link>
      </PhoneSubpageBar>
      <main className="flex w-full max-w-180 flex-col gap-6 px-4 py-6 md:px-10 md:py-8">
        <Link
          to="/"
          className="hidden items-center gap-1.5 text-sm font-semibold text-muted-foreground md:flex"
        >
          <ArrowLeftIcon />
          {t("shell.suggestions")}
        </Link>
        <SongDetails songId={songId} />
      </main>
    </>
  );
}
