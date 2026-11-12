// All interface text in English. Keys are typed from this object (see src/types/i18next.d.ts),
// so a missing or misspelled key is a type error.
export const en = {
  suggestions: {
    title: "Suggestions",
    loading: "Loading…",
    unavailable: "The Suggestions can't be loaded. Try again later.",
    empty: "No Suggestions yet.",
    byline: "{{artist}} · Suggested on {{date, datetime(dateStyle: medium)}}",
  },
  health: {
    title: "Server health",
    checking: "Checking…",
    unreachable: "The server can't be reached. Try again later.",
    statusLabel: "Status",
    versionLabel: "Version",
    migrationLabel: "Database migration",
    migrationUnknown: "Unknown",
    status: {
      healthy: "Healthy",
      unhealthy: "Unhealthy",
    },
  },
} as const;
