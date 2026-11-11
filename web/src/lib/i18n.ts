import { createInstance } from "i18next";
import { en } from "@/locales/en";

export const defaultNS = "translation";
export const resources = { en: { [defaultNS]: en } } as const;

export const i18n = createInstance();

void i18n.init({
  lng: "en",
  defaultNS,
  resources,
  // Resources are bundled, so load them synchronously: text is ready on the first render.
  initAsync: false,
  // React already escapes rendered values.
  interpolation: { escapeValue: false },
});
