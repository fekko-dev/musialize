# React component libraries for a React + TypeScript + Vite SPA

Research date: 2026-11-06. Sources are official docs, GitHub repos/releases (via `gh api`), and npm (`npm view`). Release dates are from GitHub releases or npm `time`. Anything not confirmed in a primary source is marked **unverified**.

Context: a small-group web app that must work on phones and give a dense overview on desktop ([vision](../product/vision.md)), with a pilot deadline of 2026-11-12. Stack assumption: TanStack Router + TanStack Query.

## 1. Summary and recommendation

**Pick: shadcn/ui, Base UI variant (the default since July 2026).** It is the only candidate where every hard requirement is covered by first-party docs without a paid tier or a third-party table package:

- Theming is plain CSS variables (`--primary`, `--primary-foreground`, ...) overridden under `.dark`, so a brand palette and dark mode come later without touching components.
- The data table is an official guide built on **TanStack Table v9**, so server sorting and pagination driven by TanStack Query are straightforward.
- The Base UI Toast supports an action button (`actionProps`). Drawer and Sheet support swipe gestures, and the docs show a responsive Dialog-on-desktop / Drawer-on-mobile pattern.

Be clear about the cost. You own the copied code, the "data table" is a recipe and not a component (column sizing and compact density are yours to build with TanStack Table), it requires Tailwind v4, and there is no official Figma kit (only community kits, one free with Figma variables).

**Runner-up: Chakra UI v3.** It covers the same ground as a maintained package instead of copied code: tokens and semantic tokens as CSS variables, Toast with `action`, async Combobox, Table plus an official TanStack Table example, and Drawer. It also has the best design story here: an **official, free Figma kit with light/dark variable modes**. Downsides: a runtime CSS-in-JS engine (Emotion is a peer dependency), and its Table has no built-in sorting or resizing.

Not recommended for this project:

- **MUI.** The MIT Data Grid limits sorting to one column and pages to 100 rows. Its Material look takes more work to rebrand, and the full Figma kit is paid.
- **Mantine 9.** Very complete, but it has no data table (the most popular community table, Mantine React Table, does not support Mantine 9), no toast action prop, and no official Figma kit.
- **HeroUI v3.** A credible option (React Aria underneath, a Table with sorting and resizing, an official Figma kit), but it is a young major (3.0.0 on 2026-03-21), and its docs are Next.js-centric.
- **React Aria Components and Ark UI.** Excellent primitives, but unstyled. Too much assembly before 2026-11-12.
- **Radix Themes, Park UI and Ant Design** are excluded; see §4.

## 2. Comparison table

Legend: ✅ first-party and documented · 🟡 partial / build it yourself / caveat · ❌ missing · ❓ unverified

|                                               | shadcn/ui (Base UI)                                                                                 | Chakra UI v3                                                 | Mantine 9                                                  | MUI 9 + MUI X                                                              | HeroUI v3                                                  | React Aria Components                                    | Ark UI                |
| --------------------------------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ | ---------------------------------------------------------- | -------------------------------------------------------------------------- | ---------------------------------------------------------- | -------------------------------------------------------- | --------------------- |
| **Theming (tokens / CSS vars, dark mode)**    | ✅ CSS vars + `.dark`                                                                               | ✅ tokens + semantic tokens → CSS vars; next-themes          | ✅ CSS vars from theme; color schemes                      | ✅ CSS theme variables, color schemes                                      | ✅ CSS vars on Tailwind v4, Theme Builder                  | 🟡 unstyled; bring your own                              | 🟡 unstyled           |
| **A11y primitives**                           | ✅ Base UI                                                                                          | ✅ Ark UI / Zag                                              | ✅ own implementation                                      | ✅ own implementation                                                      | ✅ React Aria                                              | ✅ reference-grade                                       | ✅ Zag state machines |
| **Mobile (sheets, swipe, responsive dialog)** | ✅ Drawer swipe + snap points, responsive Dialog/Drawer pattern                                     | 🟡 Drawer exists; responsive pattern ❓                      | 🟡 Drawer, Modal `fullScreen`; swipe-dismiss notifications | 🟡 full-screen responsive Dialog ❓                                        | ✅ Drawer, Sheet                                           | ✅ Sheet with swipe dismiss                              | 🟡 ❓                 |
| **Dense data table**                          | 🟡 TanStack Table v9 guide (sort, pagination, visibility; sizing yours)                             | 🟡 Table `size` sm/md/lg + TanStack example                  | ❌ basic Table only; community packages                    | 🟡 Data Grid: sort (single column), resize, density; page ≤100 rows in MIT | ✅ sort, resize, virtualization, TanStack example          | ✅ sort, resize, load-more (unstyled)                    | ❌ no table           |
| **Toast with action**                         | ✅ `actionProps` (Base UI); Radix variant uses Sonner `action`                                      | ✅ `action` + `actionLabel`                                  | 🟡 no action prop; custom `renderNotification`             | ✅ Snackbar `action`; no stacking (docs point to notistack)                | ✅ `actionProps`                                           | 🟡 `UNSTABLE_Toast` (alpha), compose buttons yourself    | ✅ `action`           |
| **Dialog**                                    | ✅                                                                                                  | ✅                                                           | ✅ Modal                                                   | ✅                                                                         | ✅ Modal                                                   | ✅ Modal                                                 | ✅                    |
| **Combobox**                                  | ✅ Base UI Combobox (filter, multiple)                                                              | ✅ filter, async, multiple                                   | ✅ Combobox, Select `searchable`, Autocomplete             | ✅ Autocomplete                                                            | ✅ ComboBox, Autocomplete                                  | ✅                                                       | ✅                    |
| **Tabs**                                      | ✅                                                                                                  | ✅                                                           | ✅                                                         | ✅                                                                         | ✅                                                         | ✅                                                       | ✅                    |
| **Form inputs**                               | ✅ Field/Input + TanStack Form guide                                                                | ✅ Field, Input                                              | ✅ inputs + `@mantine/form`                                | ✅ TextField etc.                                                          | ✅ TextField, Form, ...                                    | ✅                                                       | ✅                    |
| **TanStack Router fit**                       | 🟡 `render` prop; Base UI says don't render links as buttons → style `<Link>` with `buttonVariants` | ✅ in TanStack's custom-link guide (`createLink`); `asChild` | ✅ in TanStack's guide; polymorphic `component`            | ✅ MUI routing guide covers TanStack `createLink`                          | 🟡 Link `render` function; docs show Next.js only          | ✅ in TanStack's guide; `render` on Link                 | 🟡 `asChild` ❓       |
| **TanStack Query fit**                        | ✅ no data layer; table is TanStack Table                                                           | ✅ no data layer                                             | ✅ no data layer                                           | ✅ server sort/paginate modes                                              | ✅                                                         | ✅ (optional `useAsyncList`)                             | ✅                    |
| **License / paid parts**                      | MIT; no paid parts (paid Figma kits are third-party)                                                | MIT; Chakra Pro (paid) is separate                           | MIT                                                        | MIT core; **MUI X Pro/Premium paid**                                       | Apache-2.0; HeroUI Pro paid ❓                             | Apache-2.0                                               | MIT                   |
| **Maintenance (latest)**                      | `shadcn@4.23.0`, 2026-10-27                                                                         | 3.38.0, 2026-11-03                                           | 9.7.5, 2026-11-04                                          | 9.5.0, 2026-10-09; X 9.17.0, 2026-10-30                                    | 3.2.7, 2026-10-28                                          | 1.22.0, 2026-10-09                                       | 5.40.0, 2026-10-29    |
| **Figma kit**                                 | 🟡 community only; shadcncraft free starter has variables                                           | ✅ official, free, light/dark variable modes                 | 🟡 community only, not maintained by Mantine               | 🟡 official, **$79/user**; free community subset                           | ✅ official v3 kit on Figma Community (price/variables ❓) | ❌ for RAC itself (Spectrum 2 kit is Adobe's own design) | ❌ ❓                 |

## 3. Per-library findings

### shadcn/ui (Base UI, Radix and React Aria variants)

- **What it is:** components copied into your repo by a CLI. Since July 2026, Base UI is the default; "Radix is still fully supported" and "is not being deprecated" ([changelog: Base UI as the default](https://ui.shadcn.com/docs/changelog/2026-07-base-ui-default)). A React Aria variant was added in July 2026 ([changelog](https://ui.shadcn.com/docs/changelog)).
- **Variants mix:** the Radix variant's Combobox imports `@base-ui/react` (`apps/v4/registry/bases/radix/ui/combobox.tsx` in [shadcn-ui/ui](https://github.com/shadcn-ui/ui)). The Radix variant's toast is Sonner, and the Radix Toast page is marked deprecated ([radix/toast](https://ui.shadcn.com/docs/components/radix/toast), [radix/sonner](https://ui.shadcn.com/docs/components/radix/sonner)). So "Radix variant" does not mean Radix-only. For a new app, Base UI is the cleaner choice.
- **Theming:** CSS variables as background/foreground pairs, dark mode by overriding the same tokens under `.dark`, and a `--radius` scale ([theming](https://ui.shadcn.com/docs/theming)). The CSS uses Tailwind v4 syntax (`@theme inline`, `@custom-variant`). Vite dark-mode guide: [dark-mode/vite](https://ui.shadcn.com/docs/dark-mode/vite).
- **Table:** [data-table](https://ui.shadcn.com/docs/components/base/data-table) says "This guide uses TanStack Table v9". It covers sorting, pagination and column visibility. It does **not** cover column sizing or server-side data beyond a link to TanStack's manual pagination. A basic `Table` component also exists.
- **Toast with action:** the Base UI [toast](https://ui.shadcn.com/docs/components/base/toast) takes `actionProps` (the example uses "Undo"). It is built on Base UI Toast ([changelog: Toast](https://ui.shadcn.com/docs/changelog/2026-07-toast); [Base UI toast](https://base-ui.com/react/components/toast) adds swipe dismissal via `swipeDirection`). Sonner (Radix variant) supports `action` ([Sonner docs](https://sonner.emilkowal.ski/toast)).
- **Other components:** [dialog](https://ui.shadcn.com/docs/components/base/dialog), [combobox](https://ui.shadcn.com/docs/components/base/combobox) (filtering, `multiple` with chips; async loading not shown), [tabs](https://ui.shadcn.com/docs/components/base/tabs), [input](https://ui.shadcn.com/docs/components/base/input), [field](https://ui.shadcn.com/docs/components/base/field), and a [TanStack Form guide](https://ui.shadcn.com/docs/forms/tanstack-form).
- **Mobile:** the [Drawer](https://ui.shadcn.com/docs/components/base/drawer) is built on Base UI and no longer uses Vaul. It supports `swipeDirection`, `snapPoints` and a swipe handle. The docs show a responsive example that "renders a `Dialog` component on desktop and a `Drawer` on mobile". [Sheet](https://ui.shadcn.com/docs/components/base/sheet) is also available.
- **TanStack Router:** there is a [Vite install guide](https://ui.shadcn.com/docs/installation/vite) but no TanStack Router-specific guide (only TanStack Start) ([installation](https://ui.shadcn.com/docs/installation)). Base UI composes through a `render` prop ([composition](https://base-ui.com/react/handbook/composition)). However, the [Base UI Button](https://base-ui.com/react/components/button) docs say links "should not be rendered as buttons through the `render` prop", so for button-styled links apply the button styles to TanStack `<Link>` directly.
- **Figma:** "The Figma files are contributed by the community" ([figma](https://ui.shadcn.com/docs/figma)). Free kits include the shadcncraft Free Starter Kit (native Figma variables) and Obra's community edition. Several paid kits exist (prices not listed in the docs).
- **Maintenance:** MIT. Latest release `shadcn@4.23.0` on 2026-10-27 ([releases](https://github.com/shadcn-ui/ui/releases)). The repo was last pushed 2026-11-06. Base UI is at v1.9.0 (2026-10-09, [releases](https://github.com/mui/base-ui/releases)).

### Chakra UI v3

- **Theming:** `createSystem(defaultConfig, config)` with `tokens` and `semanticTokens`. Semantic tokens "always return a css variable" with the `--chakra-` prefix ([theming overview](https://chakra-ui.com/docs/theming/overview)). Dark mode relies on `next-themes`, which the docs say works with Vite ([color mode](https://chakra-ui.com/docs/components/concepts/color-mode)). Components are built on Ark UI (`@ark-ui/react` is a dependency, per npm).
- **Table:** [table](https://chakra-ui.com/docs/components/table) has a `size` prop (sm/md/lg) and `Table.ColumnGroup` widths, but no sorting or resizing. The page says it "works seamlessly with TanStack Table" and includes an example.
- **Toast:** `action` and `actionLabel` props ([toast](https://chakra-ui.com/docs/components/toast)).
- **Other components:** [combobox](https://chakra-ui.com/docs/components/combobox) (filtering, async collection, `multiple`), [dialog](https://chakra-ui.com/docs/components/dialog), [drawer](https://chakra-ui.com/docs/components/drawer), [tabs](https://chakra-ui.com/docs/components/tabs), [field](https://chakra-ui.com/docs/components/field), [input](https://chakra-ui.com/docs/components/input).
- **TanStack Router:** TanStack's [custom link guide](https://tanstack.com/router/latest/docs/framework/react/guide/custom-link) has a Chakra example using `createLink`. Chakra's `asChild` composition: [composition](https://chakra-ui.com/docs/components/concepts/composition). [Vite guide](https://chakra-ui.com/docs/get-started/frameworks/vite).
- **Figma:** an official free v3 kit ([figma](https://chakra-ui.com/docs/get-started/figma), [Figma file](https://www.figma.com/community/file/1506648876941130701)). It includes light and dark semantic-token variable modes. The docs claim it reflects how code works, but do not state a one-to-one token sync.
- **Paid parts:** the combobox page upsells Chakra Pro components (separate and paid). Core is MIT.
- **Maintenance:** `@chakra-ui/react@3.38.0` on 2026-11-03 ([releases](https://github.com/chakra-ui/chakra-ui/releases)). The runtime styling engine requires `@emotion/react` as a peer dependency (npm).

### Mantine 9

- **Theming:** all theme values exposed as CSS variables, with a `cssVariablesResolver` that can set separate light/dark values ([CSS variables](https://mantine.dev/styles/css-variables/), [color schemes](https://mantine.dev/theming/color-schemes/)). Custom colors need 10 shades.
- **9.0** shipped 2026-03-31 and requires React 19.2+ ([changelog 9.0](https://mantine.dev/changelog/9-0-0/)).
- **Table:** the [Table](https://mantine.dev/core/table/) has no sorting or resizing. Mantine's help center says "Mantine does not provide DataGrid component" and lists community packages, or building one with TanStack Table ([help](https://help.mantine.dev/q/data-grid-i-need)). On npm:
  - `mantine-datatable` 9.4.0 (2026-07) peers on `@mantine/core >=9.0` and is not TanStack-based.
  - `mantine-react-table` (TanStack-based) latest is 1.3.4, and its 2.0.0-beta.9 (2025-02-17) peers on Mantine `^7.9`. **It does not support Mantine 9.**
- **Notifications:** no documented action prop. Use `renderNotification` for custom content with buttons. Supports drag/swipe dismiss and tap-to-expand stacks ([notifications](https://mantine.dev/x/notifications/)).
- **Other components:** [combobox](https://mantine.dev/core/combobox/), [select](https://mantine.dev/core/select/), [autocomplete](https://mantine.dev/core/autocomplete/), [modal](https://mantine.dev/core/modal/), [drawer](https://mantine.dev/core/drawer/), [tabs](https://mantine.dev/core/tabs/), [text input](https://mantine.dev/core/text-input/), [use-form](https://mantine.dev/form/use-form/).
- **TanStack Router:** TanStack's custom link guide has a Mantine `Anchor` example. Mantine has [polymorphic components](https://mantine.dev/guides/polymorphic/).
- **Figma:** "there are no official Figma or Sketch design files". Community kits by Ravn and In The Zone are "not maintained by Mantine team" (`apps/mantine.dev/src/pages/getting-started.mdx` in [mantinedev/mantine](https://github.com/mantinedev/mantine)).
- **Maintenance:** MIT. 9.7.5 on 2026-11-04 ([releases](https://github.com/mantinedev/mantine/releases)).

### MUI (Material UI 9 + MUI X Data Grid)

- **Theming:** CSS theme variables with multiple color schemes ([CSS theme variables](https://mui.com/material-ui/customization/css-theme-variables/overview/), [dark mode](https://mui.com/material-ui/customization/dark-mode/)). Uses Emotion (peer dependency).
- **Data Grid licensing:** MUI X is "open-core". `@mui/x-data-grid` is MIT, while Pro and Premium are commercial ([README](https://github.com/mui/mui-x), [licensing](https://mui.com/x/introduction/licensing/)). In the MIT grid:
  - multi-sorting needs Pro; Community sorts by one criterion ([sorting](https://mui.com/x/react-data-grid/sorting/)), though server-side `sortingMode` is shown on the base grid
  - pages are "limited to pages of up to 100 rows" ([pagination](https://mui.com/x/react-data-grid/pagination/))
  - column resizing is shown on the base `DataGrid` with no plan badge ([column dimensions](https://mui.com/x/react-data-grid/column-dimensions/))
  - the [overview](https://mui.com/x/react-data-grid/) lists virtualization among Pro features ❓ (ambiguous; verify before relying on it)
- **Toast:** Snackbar takes an `action` prop. No built-in stacking; the docs point to notistack ([snackbar](https://mui.com/material-ui/react-snackbar/)).
- **Other components:** [Autocomplete](https://mui.com/material-ui/react-autocomplete/), [Dialog](https://mui.com/material-ui/react-dialog/), [Tabs](https://mui.com/material-ui/react-tabs/), [TextField](https://mui.com/material-ui/react-text-field/).
- **TanStack Router:** the [routing guide](https://mui.com/material-ui/integrations/routing/) covers the `component` prop, a theme-wide `LinkComponent`, and TanStack `createLink`.
- **Figma:** official kit at $79 per user, with free community subsets on Figma Community ([store](https://mui.com/store/items/figma-react/)).
- **Maintenance:** MIT core. 9.5.0 on 2026-10-09 and `v10.0.0-alpha.0` on 2026-10-12 ([releases](https://github.com/mui/material-ui/releases)), so a new major is coming. MUI X 9.17.0 on 2026-10-30 ([releases](https://github.com/mui/mui-x/releases)).

### HeroUI v3

- **Stack:** v3 depends on `react-aria-components` and Tailwind v4 (npm peer dependencies of `@heroui/react@3.2.7`). 3.0.0 was released 2026-03-21 (npm).
- **Theming:** "HeroUI uses CSS variables and BEM classes", mapped onto Tailwind v4 `@theme`, with light/dark switching by class ([theming](https://heroui.com/docs/react/getting-started/theming)). The dark-mode example uses next-themes "for Next.js".
- **Table:** sorting, selection, pagination footer, column resizing (React Aria `ColumnResizer`), virtualization, and a TanStack Table example ([table](https://heroui.com/docs/react/components/table); source `apps/docs/.../(data-display)/table.mdx`). Compact density: ❓.
- **Toast:** `Toast.ActionButton` and `actionProps` ([toast](https://heroui.com/docs/react/components/toast)).
- **Other components:** [combo-box](https://heroui.com/docs/react/components/combo-box), [autocomplete](https://heroui.com/docs/react/components/autocomplete), [modal](https://heroui.com/docs/react/components/modal), [drawer](https://heroui.com/docs/react/components/drawer), sheet, [tabs](https://heroui.com/docs/react/components/tabs), [text-field](https://heroui.com/docs/react/components/text-field), [form](https://heroui.com/docs/react/components/form).
- **Router:** the [composition](https://heroui.com/docs/react/getting-started/composition) docs show styling a framework `Link` with `buttonVariants`/`linkVariants` (Next.js examples). HeroUI `Link` has a `render` function prop. No TanStack Router docs.
- **Figma:** official "HeroUI Figma Kit (v3)" on [Figma Community](https://www.figma.com/community/file/1546526812159103429) (linked from the repo README). Price and variables support: ❓.
- **Maintenance:** Apache-2.0. 3.2.7 on 2026-10-28; the previous release was 3.2.6 on 2026-09-17 ([releases](https://github.com/heroui-inc/heroui/releases)).

### React Aria Components (Adobe)

- **What it is:** unstyled; style with CSS or Tailwind, or copy the Vanilla CSS / Tailwind starter kits, Storybook starters, or install via shadcn ([getting started](https://react-aria.adobe.com/getting-started), [styling](https://react-aria.adobe.com/styling)).
- **Table:** sorting, column resizing, load-more/async ([table](https://react-aria.adobe.com/Table)). Not TanStack-based.
- **Toast:** alpha; exported as `UNSTABLE_Toast`. No action API documented; compose buttons yourself ([toast](https://react-aria.adobe.com/Toast)).
- **Mobile:** [Sheet](https://react-aria.adobe.com/Sheet) "slides in from the edge of the screen and can be swiped to dismiss".
- **Other components:** [ComboBox](https://react-aria.adobe.com/ComboBox), [Modal](https://react-aria.adobe.com/Modal), [Tabs](https://react-aria.adobe.com/Tabs), [TextField](https://react-aria.adobe.com/TextField), [Form](https://react-aria.adobe.com/Form).
- **Router:** `Link` accepts a `render` prop to delegate to a router link ([link](https://react-aria.adobe.com/Link)). TanStack's guide wraps RAC `Link` with `createLink`.
- **Figma:** none found for RAC.
- **Maintenance:** Apache-2.0. `react-aria-components@1.22.0` on 2026-10-09 ([releases](https://github.com/adobe/react-spectrum/releases)).
- **Verdict:** the best accessibility foundation, but too much styling work for this deadline. It is easier to use via HeroUI or the shadcn React Aria variant.

### Ark UI (and Park UI)

- **Ark UI:** headless, built on Zag. Toast supports `action` and `Toast.ActionTrigger`. There is **no Table** component ([toast](https://ark-ui.com/docs/components/toast), [combobox](https://ark-ui.com/docs/components/combobox)). MIT; `@ark-ui/react@5.40.0` on 2026-10-29. It is the engine under Chakra v3, so Chakra is the better way to get it styled.
- **Park UI:** see §4.

## 4. Gaps, risks and open questions

**Excluded candidates**

- **Radix Themes:** no Toast and no Combobox in the component source (`packages/radix-ui-themes/src/components` in [radix-ui/themes](https://github.com/radix-ui/themes)). Last release 3.3.0 on 2026-01-31; last commit 2026-04-11. Fails requirement 4 and looks slow-moving.
- **Park UI:** last npm/GitHub release 0.43.1 on 2024-11-22 ([releases](https://github.com/chakra-ui/park-ui/releases)); last commit 2026-02-21. Now under the Chakra org ([park-ui.com](https://park-ui.com/)). Chakra v3 is the maintained path.
- **Ant Design 6:** MIT and active (6.7.0 on 2026-11-04). However, mobile is served by a separate library (`antd-mobile` 5.43.0), the official design resources are Sketch files, and the Figma kits are third-party ([resources](https://ant.design/docs/resources)). That is a poor fit for a phone-first small-group app.

**Risks for the recommendation (shadcn/ui, Base UI)**

- **Owned code:** upstream fixes don't arrive automatically; you re-pull them with the CLI.
- **Data table is a recipe:** column sizing and compact density come from TanStack Table v9 APIs plus your own CSS, which is time against the 2026-11-12 deadline. TanStack Table 9.0.0 released 2026-08-04 (npm), so community examples may still show the v8 API.
- **Tailwind v4 is mandatory** (shared with HeroUI).
- **Button-styled links:** Base UI's advice not to render links through Button `render` means using `buttonVariants` on TanStack `<Link>`. This works, but it is a convention to document.
- **Mixed variants:** the Radix variant still pulls in Base UI (Combobox) and Sonner, so picking "Radix" doesn't avoid Base UI.

**Open questions / unverified**

- Touch-target sizes (≥44px or 24px minimums) are not documented for any candidate's defaults. They need checking in a prototype on a phone.
- Virtual-keyboard behaviour (dialogs/drawers with inputs on iOS Safari) is not documented by any candidate. It needs device testing.
- Whether MUI's MIT Data Grid includes row virtualization: the docs are ambiguous.
- HeroUI: Figma kit price and variables, Table density options, and HeroUI Pro pricing.
- Whether the shadcn community Figma kits track the Base UI variant and the current token names (only shadcncraft explicitly claims Figma variables for all eight styles).
- shadcn Combobox async loading: not shown in the docs.

## 5. Sources

**Versions and maintenance:** GitHub releases API (`gh api repos/<owner>/<repo>/releases`) and `npm view <pkg> version time`, both run 2026-11-06.

- shadcn/ui: https://ui.shadcn.com/docs/components · /docs/theming · /docs/changelog · /docs/changelog/2026-07-base-ui-default · /docs/changelog/2026-07-toast · /docs/components/base/{data-table,toast,dialog,combobox,tabs,input,field,drawer,sheet} · /docs/components/radix/{toast,sonner,combobox} · /docs/figma · /docs/installation · /docs/installation/vite · /docs/dark-mode/vite · /docs/forms/tanstack-form · https://github.com/shadcn-ui/ui
- Sonner: https://sonner.emilkowal.ski/toast
- Base UI: https://base-ui.com/react/handbook/composition · /react/components/button · /react/components/toast · /react/components/combobox · /react/components/drawer · https://github.com/mui/base-ui/releases
- Chakra UI: https://chakra-ui.com/docs/theming/overview · /docs/components/concepts/color-mode · /docs/components/{table,toast,combobox,dialog,drawer,tabs,field,input} · /docs/components/concepts/composition · /docs/get-started/frameworks/vite · /docs/get-started/figma · https://www.figma.com/community/file/1506648876941130701 · https://github.com/chakra-ui/chakra-ui/releases
- Mantine: https://mantine.dev/styles/css-variables/ · /theming/color-schemes/ · /changelog/9-0-0/ · /core/{table,combobox,select,autocomplete,modal,drawer,tabs,text-input}/ · /form/use-form/ · /x/notifications/ · /guides/polymorphic/ · https://help.mantine.dev/q/data-grid-i-need · https://icflorescu.github.io/mantine-datatable/ · https://github.com/mantinedev/mantine
- MUI: https://mui.com/material-ui/customization/css-theme-variables/overview/ · /material-ui/customization/dark-mode/ · /material-ui/react-{snackbar,autocomplete,dialog,tabs,text-field}/ · /material-ui/integrations/routing/ · /x/react-data-grid/ · /x/react-data-grid/{sorting,pagination,column-dimensions}/ · /x/introduction/licensing/ · /store/items/figma-react/ · https://github.com/mui/mui-x · https://github.com/mui/material-ui/releases
- HeroUI: https://heroui.com/docs/react/components/{table,toast,combo-box,autocomplete,modal,drawer,tabs,text-field,form} · /docs/react/getting-started/{theming,composition} · https://github.com/heroui-inc/heroui (branch `v3`, `apps/docs/content/docs/en/react/`) · https://www.figma.com/community/file/1546526812159103429
- React Aria Components: https://react-aria.adobe.com/{getting-started,styling,Table,Toast,Sheet,ComboBox,Modal,Tabs,TextField,Form,Link,frameworks} · https://github.com/adobe/react-spectrum
- Ark UI / Park UI: https://ark-ui.com/docs/components/toast · https://ark-ui.com/docs/components/combobox · https://park-ui.com/ · https://github.com/chakra-ui/park-ui
- Radix Themes: https://github.com/radix-ui/themes
- Ant Design: https://ant.design/docs/resources · https://github.com/ant-design/ant-design
- TanStack Router custom links: https://tanstack.com/router/latest/docs/framework/react/guide/custom-link
