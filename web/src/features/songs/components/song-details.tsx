import { LinkIcon } from "@phosphor-icons/react";
import { useTranslation } from "react-i18next";
import { Alert, AlertTitle } from "@/components/ui/alert";
import { Skeleton } from "@/components/ui/skeleton";
import { useSong } from "../api/get-song";

// Read-only for now: the Song page's title, artist, link and date.
export function SongDetails({ songId }: { songId: string }) {
  const { t } = useTranslation();
  const song = useSong(songId);

  if (song.isPending) {
    return (
      <div role="status" className="flex flex-col gap-2">
        <span className="sr-only">{t("song.loading")}</span>
        <Skeleton className="h-6 w-56" />
        <Skeleton className="h-5 w-32" />
      </div>
    );
  }

  if (song.isError) {
    return (
      <Alert variant="destructive">
        <AlertTitle>{t("song.unavailable")}</AlertTitle>
      </Alert>
    );
  }

  return (
    <article className="flex flex-col gap-5">
      <header className="flex flex-col gap-1">
        <h2 className="text-xl font-semibold">{song.data.title}</h2>
        <p className="text-muted-foreground">{song.data.artist}</p>
      </header>
      <a
        href={song.data.link}
        target="_blank"
        rel="noreferrer"
        className="flex min-h-11 items-center gap-2 font-semibold break-all"
      >
        <LinkIcon className="size-4.5 shrink-0" />
        {/* As in the design: without the scheme, which every link has. */}
        {song.data.link.replace(/^https?:\/\//, "")}
      </a>
      <p className="text-sm text-muted-foreground">
        {t("song.suggestedOn", { date: new Date(song.data.suggestedAt) })}
      </p>
    </article>
  );
}
