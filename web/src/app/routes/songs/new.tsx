import { XIcon } from "@phosphor-icons/react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { PhoneSubpageBar } from "@/components/layouts/phone-subpage-bar";
import { buttonVariants } from "@/components/ui/button";
import { SuggestSongForm } from "@/features/songs/components/suggest-song-form";

export const Route = createFileRoute("/songs/new")({
  component: SuggestSongPage,
  staticData: { subpage: true },
});

function SuggestSongPage() {
  const { t } = useTranslation();

  const close = (
    <Link
      to="/"
      aria-label={t("suggest.close")}
      className={buttonVariants({ variant: "ghost", size: "icon-lg" })}
    >
      <XIcon className="size-5" />
    </Link>
  );

  // Phone and desktop each have their own heading with Close; CSS shows one of them.
  return (
    <>
      <PhoneSubpageBar>
        <h2 className="flex-1 pl-3 font-heading text-lg font-semibold">
          {t("suggest.title")}
        </h2>
        {close}
      </PhoneSubpageBar>
      <main className="flex w-full max-w-160 flex-col gap-6 px-4 py-6 md:px-10 md:py-8">
        <div className="hidden items-center gap-1 md:flex">
          <h2 className="flex-1 text-xl font-semibold">{t("suggest.title")}</h2>
          {close}
        </div>
        <SuggestSongForm />
      </main>
    </>
  );
}
