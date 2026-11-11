import "i18next";
import type { defaultNS, resources } from "@/lib/i18n";

// https://www.i18next.com/overview/typescript
declare module "i18next" {
  interface CustomTypeOptions {
    defaultNS: typeof defaultNS;
    resources: (typeof resources)["en"];
  }
}
