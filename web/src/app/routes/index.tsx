import { createFileRoute } from "@tanstack/react-router";
import { SuggestionList } from "@/features/songs/components/suggestion-list";

export const Route = createFileRoute("/")({
  component: OverviewPage,
});

function OverviewPage() {
  return (
    <main className="flex w-full max-w-3xl flex-col gap-6 px-4 py-8 md:px-10">
      <SuggestionList />
    </main>
  );
}
