# Coding standards

Judgement rules for reviewing a diff. Formatting, import direction and commit messages are checked by `pnpm lint` and the Git hooks. The Errors, Logging and Database migrations sections of [CONTRIBUTING.md](CONTRIBUTING.md) are review rules too.

## Simple first

KISS, YAGNI, DRY: an abstraction, option or layer has a second user today, not someday, and one piece of logic lives in one place. SOLID where a seam earns it.

## Comments say why

A comment is there only where a quick read of the code leaves the reason unclear: a choice, a gotcha, the source of a pattern. One line where possible.

## Documented ways first

Follow a library's or framework's documented way of doing something, and the architecture the [README](README.md#web-app-layout) describes (Bulletproof React), rather than a home-made one. Examples: shadcn's dark mode guide for the theme, ASP.NET Core's convention of putting `Add…` service registrations in the `Microsoft.Extensions.DependencyInjection` namespace (#31). Mark each addition to a documented pattern as one, with the reason.

## Glossary terms

Code, comments and tests use the terms in [GLOSSARY.md](GLOSSARY.md), never its _Avoid_ words: an Organizer is a Group admin.

## Scoped pull requests

A pull request has one reason to exist and stays around 400 lines to review, tests included. Vendor code, such as shadcn CLI output, comes in its own pull request, first. Generated files that belong to the change (EF migrations, `openapi.json`, `*.gen.ts`) stay in it and don't count toward the 400.
