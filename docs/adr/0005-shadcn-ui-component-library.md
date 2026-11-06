# Use shadcn/ui on Base UI with Tailwind v4 for UI components

The frontend (React + TypeScript + Vite, with TanStack Router and Query) uses shadcn/ui, in its Base UI variant, styled with Tailwind v4. The components are copied into the repo, so we own and can change them. Of the libraries we compared ([research](../research/react-component-libraries.md)), it is the only one where every requirement is covered by first-party docs with nothing paid. Theming is plain CSS variables with a `.dark` override, so a brand palette and dark mode can come later without touching components. The data table is a guide built on TanStack Table, which fits TanStack Query. The toast supports an action such as "Undo". Dialog, combobox, tabs and form fields come from accessible Base UI primitives, and a swipeable Drawer serves as the mobile dialog. Chakra UI v3 was the runner-up; it is a maintained package with an official Figma kit, but it needs a runtime CSS-in-JS engine.

## Consequences

- Generated components live in `components/ui/` and stay as close to upstream as possible. Brand work goes in the CSS variables. App-specific behaviour goes in wrappers outside `components/ui/`, so overwriting a generated file loses little or nothing.
- There is no update command. Upstream changes are pulled one component at a time, and only when we need a fix or feature: commit first, run `shadcn add <component> --overwrite`, review `git diff`, and restore the local changes we want to keep. Accessibility and behaviour fixes mostly arrive through normal dependency updates of `@base-ui/react` and `@tanstack/react-table`.
- The data table is a recipe, not a component. Column sizing and compact density are ours to build with TanStack Table.
- A link that looks like a button is a TanStack Router `<Link>` with the button styles applied (`buttonVariants`), not a Button rendered as a link. Base UI's docs advise against rendering links as buttons.
- There is no official Figma kit. Designs use a community kit, such as the free shadcncraft starter kit, which uses Figma variables.

## Still to verify while building

- Touch target sizes and how dialogs and drawers behave with the on-screen keyboard, tested on a real phone.
- Whether the combobox handles async (server-side) search well enough, or needs custom loading.
