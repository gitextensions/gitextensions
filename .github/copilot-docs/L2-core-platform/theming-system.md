<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Theming System (L2)

**TL;DR:** Colors are named by the `AppColor` enum and resolved through a `Theme` object rather
than hardcoded. Themes are **CSS files** loaded by `ThemeLoader` and managed by `ThemeRepository`.
This enables light/dark/high-contrast and user-authored themes. When you need a color in UI code,
ask for the `AppColor`, don't write a literal `Color`.

**Related:** [ui-composition](../L1-conceptual/ui-composition.md) · [settings-system](settings-system.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

A git GUI shows lots of semantic color (graph branches, diff add/remove, ANSI terminal, panel
chrome). Hardcoding colors breaks dark mode, accessibility (high contrast), and user
customisation. A named-color + theme-file approach lets every surface stay consistent and
re-skinnable without touching the drawing code.

## What

### Color model (`GitExtUtils/GitUI/Theming/`)

- **`AppColor`** (enum) — every app-specific semantic color (panel background, editor background,
  graph branch colors, ANSI terminal colors…). [AppColor.cs](../../../src/app/GitExtUtils/GitUI/Theming/AppColor.cs).
- **`AppColorDefaults`** — default value for each `AppColor`.
  [AppColorDefaults.cs](../../../src/app/GitExtUtils/GitUI/Theming/AppColorDefaults.cs).
- **`Theme`** — container holding app colors + system colors + a `ThemeId`.
  [Theme.cs](../../../src/app/GitExtUtils/GitUI/Theming/Theme.cs).
- **`OtherColors`** — system-color mappings/helpers.
  [OtherColors.cs](../../../src/app/GitExtUtils/GitUI/Theming/OtherColors.cs).

### Theme lifecycle (`GitUI/Theming/`)

- **`ThemeLoader`** — parses a CSS theme file into a `Theme`.
  [ThemeLoader.cs](../../../src/app/GitUI/Theming/ThemeLoader.cs).
- **`ThemeRepository`** — `GetTheme()` / `Save()` / `Delete()` theme management.
  [ThemeRepository.cs](../../../src/app/GitUI/Theming/ThemeRepository.cs).
- **`ThemePersistence`** — read/write a `Theme` to CSS.
  [ThemePersistence.cs](../../../src/app/GitUI/Theming/ThemePersistence.cs).
- **`ThemePathProvider`** / **`ThemeCssUrlResolver`** — resolve theme file paths / CSS URLs.

Built-in theme assets live under `GitUI/Themes/`; the active theme is a
[setting](settings-system.md).

## How

- In UI/drawing code, resolve a color via its `AppColor` (through the theming extensions) so it
  honors the active theme and high-contrast mode.
- To add a semantic color: add an `AppColor` value + a default in `AppColorDefaults`, then use it.
- To ship a theme: add a CSS file that `ThemeLoader` can parse.

## Hard rules

- **NEVER** hardcode `Color.FromArgb(...)` for themable UI — add/use an `AppColor`.
- Respect high-contrast / system themes; don't assume a light background.
- Keep color *names* semantic (what it's for), not literal (its RGB).

**Next:** [hotkey-system](hotkey-system.md) or [scripts-engine](scripts-engine.md).
