// All interface text in English. Keys are typed from this object (see src/types/i18next.d.ts),
// so a missing or misspelled key is a type error.
export const en = {
  shell: {
    navigation: "Main",
    suggestions: "Suggestions",
    collapse: "Collapse",
    expand: "Expand",
    openMenu: "Open menu",
    theme: {
      label: "Theme",
      system: "System",
      light: "Light",
      dark: "Dark",
    },
  },
  suggestions: {
    title: "Suggestions",
    loading: "Loading…",
    unavailable: "The Suggestions can't be loaded. Try again later.",
    empty: "No Suggestions yet.",
    byline: "{{artist}} · Suggested on {{date, datetime(dateStyle: medium)}}",
  },
} as const;
