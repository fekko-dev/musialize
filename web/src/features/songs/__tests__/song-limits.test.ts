import { expect, it } from "vitest";
import openapi from "../../../../../backend/openapi.json";
import { songLimits } from "../song-limits";

it("matches the API's limits for a Suggestion", () => {
  const { properties } = openapi.components.schemas.SuggestSongRequest;

  expect(songLimits).toEqual({
    title: properties.title.maxLength,
    artist: properties.artist.maxLength,
    link: properties.link.maxLength,
  });
});
