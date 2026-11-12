import { api } from "@/lib/api-client";

export function useBranding() {
  // Branding only changes with a redeploy.
  return api.useQuery("get", "/api/branding", {}, { staleTime: Infinity });
}
