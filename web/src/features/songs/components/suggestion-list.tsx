import { useTranslation } from "react-i18next";
import { useSuggestions } from "../api/get-suggestions";

// The Group's Suggestions, oldest first: the order the API returns them in.
export function SuggestionList() {
  const { t } = useTranslation();

  return (
    <section
      aria-labelledby="suggestions-title"
      className="flex flex-col gap-4"
    >
      <h2 id="suggestions-title" className="text-xl font-semibold">
        {t("suggestions.title")}
      </h2>
      <SuggestionListContent />
    </section>
  );
}

function SuggestionListContent() {
  const { t } = useTranslation();
  const suggestions = useSuggestions();

  if (suggestions.isPending) {
    return <p className="text-muted-foreground">{t("suggestions.loading")}</p>;
  }

  if (suggestions.isError) {
    return (
      <p role="alert" className="text-destructive">
        {t("suggestions.unavailable")}
      </p>
    );
  }

  if (suggestions.data.length === 0) {
    return <p className="text-muted-foreground">{t("suggestions.empty")}</p>;
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
