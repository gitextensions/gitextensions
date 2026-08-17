<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Translation System (L2)

**TL;DR:** User-facing strings are `TranslationString` fields (often grouped on a `Translate`
subclass such as `TranslatedStrings`). The English source of truth is the generated file
**`English.xlf`**. When you add or change any UI string/control, you **MUST** regenerate it by
running **`update-loc.cmd`** — CI fails if `English.xlf` is stale. Other languages come from
Transifex; never hand-edit `.xlf`.

**Related:** [L0 primer](../L0-foundations/gitextensions-primer.md) · [architecture-overview](../L1-conceptual/architecture-overview.md) · master [docs-index](../docs-index.md)

**Key files:** [TranslationString.cs](../../../src/app/ResourceManager/TranslationString.cs) ·
[Translate.cs](../../../src/app/ResourceManager/Translate.cs) ·
[TranslatedStrings.cs](../../../src/app/ResourceManager/TranslatedStrings.cs) ·
[English.xlf](../../../src/app/GitUI/Translation/English.xlf) · [update-loc.cmd](../../../update-loc.cmd)

## Why

Git Extensions is localised into many languages. To keep translators productive and the app
consistent, the English strings are **extracted by reflection** into a single XLIFF catalog
(`English.xlf`) rather than scattered resource files. Translators work from that catalog on
Transifex; the app loads the matching language file at runtime.

## What

- **`TranslationString`** — wraps a single translatable string; supports `SmartFormat`
  placeholders like `{0:second|seconds}` for pluralisation.
  [TranslationString.cs](../../../src/app/ResourceManager/TranslationString.cs).
- **`Translate`** — base class; a control/form/service derives from it and declares
  `TranslationString` fields, which the tooling discovers.
  [Translate.cs](../../../src/app/ResourceManager/Translate.cs).
- **`TranslatedStrings`** — shared, app-wide common literals (dates, "Author", etc.).
  [TranslatedStrings.cs](../../../src/app/ResourceManager/TranslatedStrings.cs).
- **`TranslatedControl`** / **`LocalizationHelpers`** — apply translations to WinForms controls.
- **`English.xlf`** — the generated English catalog and CI's reference; translated `.xlf` files
  live alongside it in `GitUI/Translation/`.
- **`TranslationApp`** (`Setup/TranslationApp`) — the generator run by `update-loc.cmd`. It
  reflects over all translatable types, regenerates `English.xlf`, and stages the result.

## How (add or change a UI string)

1. Add a `TranslationString` field (or edit a control's designer text / a `TranslatedStrings` entry).
2. Build succeeds first (`dotnet build /v:q`).
3. Run **`update-loc.cmd`** from the repo root. This runs `TranslationApp`, regenerates
   `English.xlf`, and stages the change.
4. Commit the updated `English.xlf` **together** with your code change.

```
> .\update-loc.cmd      # regenerates & stages English.xlf
```

## Hard rules

- Adding/modifying forms, controls, toolbar/menu items, or `TranslationString` fields **MUST**
  update `English.xlf` via `update-loc.cmd`. **CI fails** otherwise.
- **NEVER** hand-edit `.xlf` files — they are generated.
- Do not translate non-user-facing/log/exception-internal strings; only wrap real UI text.
- Commit the regenerated `English.xlf` in the **same** change as the code that introduced the string.

**Next:** back to the [L2 index](docs-index.md) for other subsystems (theming, hotkeys, testing).
