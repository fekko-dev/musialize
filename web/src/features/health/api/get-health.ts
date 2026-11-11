import { api } from "@/lib/api-client";

export function useHealth() {
  // An unhealthy server answers with a 503 that already says what's wrong, so retrying it
  // only delays showing that answer (by about 20 seconds with the default retries).
  return api.useQuery("get", "/health", {}, { retry: false });
}
