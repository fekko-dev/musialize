import { describe, expect, it } from "vitest";
import { i18n } from "../i18n";

describe("translations", () => {
  it("translates a known key", () => {
    expect(i18n.t("health.title")).toBe("Server health");
  });

  it("rejects a misspelled key at compile time", () => {
    // @ts-expect-error -- "health.titel" is not a translation key; `pnpm lint:types` fails if it ever becomes one.
    expect(i18n.t("health.titel")).toBe("health.titel");
  });
});
