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
  suggest: {
    title: "Suggest a Song",
    close: "Close",
    songTitle: "Title",
    songTitlePlaceholder: "Song title",
    artist: "Artist",
    link: "Link to the song",
    linkHelp: "The version of the Song you'd like to cover.",
    submit: "Suggest",
    failed: "The Song can't be suggested. Try again later.",
    errors: {
      required: "Fill this in.",
      tooLong: "Use at most {{max}} characters.",
      notALink: "Enter a full link, starting with https://.",
      // When the API rejects a value the checks above let through.
      invalid: "Check this field.",
    },
  },
  song: {
    back: "Back",
    loading: "Loading…",
    unavailable: "This Song can't be shown.",
    suggestedOn: "Suggested on {{date, datetime(dateStyle: medium)}}",
  },
} as const;
