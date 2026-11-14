import { createFileRoute } from "@tanstack/react-router";
import { SuggestionList } from "@/features/songs/components/suggestion-list";

// The Overview: where the app opens.
export const Route = createFileRoute("/")({
  component: OverviewPage,
});

function OverviewPage() {
  return (
    <main className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-4 py-8">
      <SuggestionList />
    </main>
  );
}
