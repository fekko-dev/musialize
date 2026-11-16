import { api } from "@/lib/api-client";

export function useSuggestSong() {
  return api.useMutation("post", "/api/songs");
}
