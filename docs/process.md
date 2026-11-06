# Development process

Musialize follows an iterative, incremental software development lifecycle. Each feature runs through a small cycle of its own, from idea to feedback. How far it goes through each phase depends on the size and risk of the feature: a small change can go straight to a single issue.

| Phase | Step | How | Where the result lives |
|---|---|---|---|
| Plan and analyze | Discovery | Interview-style sessions that challenge assumptions | [vision](product/vision.md), [GLOSSARY.md](../GLOSSARY.md), [ADRs](adr/) |
| | Requirements | A spec with problem, solution, user stories, decisions and what's out of scope | **One spec issue per feature** on GitHub |
| Design | User journeys and flows | Mermaid diagrams | In the spec issue |
| | Wireframes | Figma, *Wireframes* page | Figma, linked from the spec issue |
| | Domain model (ERD, UML) | Mermaid `erDiagram` / `classDiagram` | In the spec issue; a lasting domain model in `docs/` once it's stable |
| | Visual design | Figma: *Design system* page (colours, typography, components), *Designs* page (screens) | Figma, linked from the spec issue |
| Implement | Tickets | The spec split into vertical slices (database to screen, demonstrable on their own) with "blocked by" links | Sub-issues of the spec, on the GitHub Project board |
| | Build | Test-driven, one ticket per branch and PR | See [CONTRIBUTING.md](../CONTRIBUTING.md) |
| Test | Verify | Required CI checks, code review against the conventions and against the spec, a check on staging | PR and staging |
| Deploy | Release | Merge the release PR | See [ADR-0004](adr/0004-promote-images-from-main-by-release-tag.md) |
| Maintain and get feedback | Learn | Use at Review Meetings, triage incoming issues, a short retrospective after each feature | New issues, updated docs |

## Figma

One Figma project with three pages, which stays within the free plan's limits:

1. **Wireframes**: low-fidelity layouts and flows.
2. **Design system**: colours, typography, spacing and components.
3. **Designs**: the final screens, built from the design system.

The designs in Figma are where visual decisions get made, and code follows them. Agents read Figma through the open-source Figma CLI, not through a Figma MCP connector. Mermaid diagrams live next to the text they explain, in issues or in `docs/`.
