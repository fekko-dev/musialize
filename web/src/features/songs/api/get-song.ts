import { api } from "@/lib/api-client";

export function useSong(songId: string) {
  return api.useQuery("get", "/api/songs/{id}", {
    params: { path: { id: songId } },
  });
}
