# Musialize: Product Vision

Musialize ("music" + "socialize") is a web app, installable on phones, for collectives of musicians who choose songs together and form a different Line-up for each Song. It replaces the shared document where Suggestions, Parts and Sign-ups currently live, so everyone can see at a glance which Songs are Filled and what the Group is working on.

Domain terms in **bold** or with capitals are defined in [GLOSSARY.md](../../GLOSSARY.md). Lasting decisions live in [docs/adr/](../adr/).

## Why

Musialize has two goals, and they divide the work between them:

1. **Be genuinely useful to a real group of musicians.** This decides *what* gets built.
2. **Show professional software engineering.** This decides *how* it gets built: tests, CI/CD, ADRs, a clean architecture. It never justifies infrastructure the product doesn't need.

## The problem

Groups like this often organize themselves with shared documents, a group chat and streaming playlists. That breaks down as the number of Songs grows:

- **Hard to see what's Filled.** With many Suggestions, you can't tell which ones have every Part covered without reading through all of them.
- **Copying by hand.** Suggestions and the Repertoire end up in separate places, and finding a Song during a Rehearsal means scrolling.
- **Manual entry.** Title, artist, key, BPM and every Part are typed in by hand for each Suggestion.
- **Poor on phones.** Shared documents with tables work well on a PC but badly on mobile.
- **No history.** Nobody can see that someone signed up and later withdrew, until a Rehearsal reveals a Part has nobody on it.

What shared documents do well, and Musialize must keep: a dense overview on desktop, anyone being able to edit, free-text notes on Sign-ups ("I need brushes for this"), and Comments under a Song.

## Users and how they work

- **Member**:
  - suggests Songs in their free time
  - signs up for Parts as a regular player or as a Backup
  - comments on Songs
  - checks what they need to prepare
- **Organizer**: does everything a Member can, plus:
  - decides at the Review Meeting which Suggestions are accepted or declined
  - signs Members up on their behalf
  - deletes Comments
  - fixes Claims
  - later, builds Setlists for Events

Main moments of use:

1. **Between meetings, at home, desktop or phone:** suggest a Song, fill in Parts, sign up, comment.
2. **During the Review Meeting, on a projector plus phones:** go through Suggestions one by one, play each link, discuss it, sign people up, accept or decline. Many people change things at once, so changes must appear live.
3. **During a Rehearsal, on a phone:** look up a Song and see who plays what.

The journeys through these moments, and the screens they need, are mapped in [journeys.md](journeys.md).

## Boundaries

Musialize **is** the place for Songs, Parts, Line-ups, Comments and decisions about them.

Musialize **is not**:
- **A chat.** The group chat stays for everything social.
- **File storage.** Sheet music, chord charts and audio stay in Google Drive and similar services; Songs only link to them.
- **A playlist or streaming tool.** Spotify and YouTube Music playlists keep handling listening.
- **A replacement for the people who decide.** The app gathers Sign-ups and opinions; the Organizers decide.

## Pilot: Review Meeting, 12 November 2026

The goal is that the pilot Group can use Musialize in that meeting, or at least that it can be demonstrated with real Songs. Detailed tickets live on the GitHub Project board.

**Must have**
- Sign-in with an email one-time code.
- Claiming through an invite link. Someone whose name isn't listed adds themselves.
- A Members screen for Organizers: add Unclaimed Members, and undo or move a Claim. Members can't switch names themselves.
- Suggest a Song with title, artist, key, BPM, a link and Parts (required or optional), with a duplicate check that offers to open the existing Song or clear the form.
- Sign-ups (regular by default, Backup as the alternative, also on a Part nobody has signed up for) and withdrawing, with an undo toast. A Backup can switch to regular after confirming.
- Organizers signing up other Members.
- Comments: edit your own ("(edited)" shown) and delete with confirmation. Organizers can delete anyone's.
- An overview with fill status ("4/6", Filled) and a separate hint for open optional Parts, filtered by Suggested, Accepted (the Repertoire pool) and Declined. Each filter has its own order, shown on screen: Suggested oldest first, Accepted by title, Declined by most recent decision.
- Organizers accepting or declining a Song, with the decision date shown and an undo toast.
- Live updates of the overview and Songs. Lists change only when you ask (opening the app, changing the filter, or "View new" at the end of the list), so rows never move under someone's finger.
- Edit mode with Save/Cancel for Song details and Parts. Saving over a change you didn't see is rejected, your unsaved input is kept, and a banner shows the change. Line-up changes never conflict with editing.
- Song activity (Sign-ups, withdrawals, status and Part changes) recorded from day one, without a history screen yet.
- A responsive layout and interface text ready for translation, shipping in English only.
- Deployed via CI/CD on a single-level subdomain of `fekko.dev`, with the pilot Group's name and logo set by deployment settings, not in the repository.

**Stretch:** presence ("Anna is editing this song").

**If time runs short, cut in this order:** the Members screen (scripts take over), the duplicate check, then Backup, then live updates.

**Out of scope for the pilot:** Events and Setlists (playlists keep handling them for now), Archive, the history screen, automatic lookup, other admin screens (handled by scripts), notifications and @-mentions, the install prompt, passkeys, and offline use.

**What makes the pilot a success:** the Group stops using the shared documents for Suggestions and Sign-ups.

## Later

Roughly in order of expected value; none of these is committed yet:

- **Events and Setlists:**
  - Rehearsals and Performances, possibly Jam Sessions, each with its own Setlist built from the Repertoire
  - attendance, and Line-ups per Event worked out from attendance and Sign-ups
- **Song history screen**, similar to the change history on a Jira ticket.
- **Archive:**
  - retiring Repertoire Songs
  - re-suggesting Declined Songs
  - deleting old items in bulk, for example everything from last academic year
- **Automatic lookup** of title and artist, later key and BPM, preferring free sources.
- **Notifications and @-mentions** in Comments, including new Suggestions and telling Backups when a Part they back opens up.
- **Install prompt and passkeys.**
- **Per-Group addresses and branding** once Musialize has its own domain, all on one deployment (see [ADR-0002](../adr/0002-multiple-groups-per-deployment.md)).
- **Management screens for Organizers and Admins:** invites, default Parts, Groups. Creating an Unclaimed Member inline while signing someone up, with a check for typos in the new name.
- **Vocals:** a better organization of vocal Parts and harmonies, possibly with lyrics marking who sings what.
- **A user manual.**
- **More languages.**

## Engineering principles

- **Speed first, without debt that blocks later work.** Every slice ends with working, deployed software.
- **Always required:**
  - automated tests on the domain rules (Line-up, status changes, permissions)
  - CI/CD to Dokploy
  - ADRs for lasting decisions
  - a generated, typed API client
- **Consistency is enforced, not discussed:**
  - EditorConfig, Prettier and ESLint, and `dotnet format` with analyzers
  - line endings normalized with `.gitattributes`, so diffs never contain whitespace or line-ending noise
  - checked in CI
- **Pragmatic DDD, TDD and Clean Architecture where they pay off.** Rich domain objects and test-first development where real rules live (Song, Line-up, Claim). Dependencies point inward so the domain has no framework code; the backend's four layers and why are in [ADR-0008](../adr/0008-four-layer-clean-architecture-with-use-case-handlers.md). Any abstraction beyond those is added only when it solves a concrete problem. Follow KISS, YAGNI, DRY and SOLID.
- **The proposed technology stack is provisional:**
  - React + TypeScript + Vite, TanStack Router and TanStack Query
  - ASP.NET Core, PostgreSQL with EF Core, SignalR
  - a monorepo on GitHub, Docker on Hetzner via Dokploy

  Lasting choices get an ADR when they're made.

## Open questions

- **Recording activity:** how Song activity is recorded so a Jira-style history screen can be built later. Settle before the first edit feature.
- **Email:** which email provider sends the one-time codes.
- **Performances:** is a Performance more than a kind of Event? How is working up to a Performance across several Rehearsals represented? Where do open Rehearsals fit?
- **Instruments:** Parts start as free text, with suggestions drawn from Part names already in use in the Group. Revisit a managed Instrument list (one Instrument plus an optional label per Part) when Member profiles or icons arrive, or when typos start showing up in the filter. The Part names in use by then become the starting list.
- **Seats on Parts:** do Parts need a seat count? (Settled: a Part with only Backups counts as open, and the fill status "4/6" counts required Parts only.)
- **Organizers:** how a Member becomes or stops being an Organizer in the app, and what else only Organizers may do. (For the pilot, the role is assigned by script.)
- **Retiring Songs:** what happens to Repertoire Songs the Group stops playing.
