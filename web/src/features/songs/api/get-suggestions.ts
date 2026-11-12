import { api } from "@/lib/api-client";

export function useSuggestions() {
  return api.useQuery("get", "/api/songs");
}
