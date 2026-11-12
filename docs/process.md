# Development process

Musialize follows an iterative, incremental software development lifecycle. Each feature runs through a small cycle of its own, from idea to feedback. How far it goes through each phase depends on the size and risk of the feature: a small change can go straight to a single issue.

| Phase                     | Step                    | How                                                                                                                      | Where the result lives                                                   |
| ------------------------- | ----------------------- | ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| Plan and analyze          | Discovery               | Interview-style sessions that challenge assumptions                                                                      | [vision](product/vision.md), [GLOSSARY.md](../GLOSSARY.md), [ADRs](adr/) |
|                           | Requirements            | A spec with problem, solution, user stories, decisions and what's out of scope                                           | **One spec issue per feature** on GitHub                                 |
| Design                    | Journeys and screen map | Before a release's first feature: Mermaid flowcharts per journey, a screen map and a table mapping must-haves to screens | [journeys.md](product/journeys.md)                                       |
|                           | Feature flow            | The feature's part of the journey, zoomed in, as Mermaid                                                                 | In the spec issue                                                        |
|                           | Wireframes              | Figma, _Wireframes_ page                                                                                                 | Figma, linked from the spec issue                                        |
|                           | Domain model (ERD, UML) | Mermaid `erDiagram` / `classDiagram`                                                                                     | In the spec issue; a lasting domain model in `docs/` once it's stable    |
|                           | Visual design           | Figma: _Design system_ page (colours, typography, components), _Designs_ page (screens)                                  | Figma, linked from the spec issue                                        |
| Implement                 | Tickets                 | The spec split into vertical slices (database to screen, demonstrable on their own) with "blocked by" links              | Sub-issues of the spec, on the GitHub Project board                      |
|                           | Build                   | Test-driven, one ticket per branch and PR                                                                                | See [CONTRIBUTING.md](../CONTRIBUTING.md)                                |
| Test                      | Verify                  | Required CI checks, code review against the conventions and against the spec, a check on staging                         | PR and staging                                                           |
| Deploy                    | Release                 | Merge the release PR                                                                                                     | See [ADR-0004](adr/0004-promote-images-from-main-by-release-tag.md)      |
| Maintain and get feedback | Learn                   | Use at Review Meetings, triage incoming issues, a short retrospective after each feature                                 | New issues, updated docs                                                 |

### Order within a feature

The table lists phases, not a strict sequence. For each feature, requirements and wireframes are worked out together in one session: grill the feature (`/grill-with-docs`) and draw the wireframes as questions come up, because a drawing settles questions talk can't. The spec issue is written at the end of that session with `/to-spec`: it records the decisions and links the frames and the feature's part of the journey map. Tickets (`/to-tickets`) follow from the spec. Small changes can skip the spec and go straight to implementation.

## Figma

One Figma project with three pages, which stays within the free plan's limits:

1. **Wireframes**: low-fidelity layouts and flows.
2. **Design system**: colours, typography, spacing and components.
3. **Designs**: the final screens, built from the design system.

**Wireframe conventions.** Greyscale only, no brand styling, Inter. Icons only where they do a job (close, back, add, link), from Phosphor, the icon library in `web/components.json`; no decorative icons. Frames use auto-layout and are named `<Screen> / Desktop` (1440 wide) and `<Screen> / Phone` (390 wide); extra states as `<Screen> (<state>) / Phone`. Text uses the Tailwind type scale (12/14/16/18/20). Use glossary terms exactly, never the _Avoid_ words. Draw only must-haves. Open questions are asked in the session, not left on the canvas.

The designs in Figma are where visual decisions get made, and code follows them. Agents read Figma through the open-source Figma CLI, not through a Figma MCP connector. Mermaid diagrams live next to the text they explain, in issues or in `docs/`.
