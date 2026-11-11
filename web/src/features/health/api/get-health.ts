import { api } from "@/lib/api-client";

export function useHealth() {
  return api.useQuery("get", "/health");
}
