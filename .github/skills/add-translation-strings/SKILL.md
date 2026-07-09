---
name: add-translation-strings
description: '**EXECUTION (runbook) SKILL** — Add or change a user-facing UI string in Git Extensions and regenerate the translation catalog. USE FOR: adding a TranslationString, changing a control/menu/toolbar caption, any UI text change that must update English.xlf. DO NOT USE FOR: non-UI/log/exception-internal strings, or general feature work. Triggers: "add a UI string", "change this label", "regenerate English.xlf", "update-loc".'
---

# Add Translation Strings (runbook)

Any user-facing string must be a `TranslationString` and reflected in `English.xlf`. Follow in order.

## Steps

1. **Add/edit the string** in the right place:
   - A form/control literal → declare a `private readonly TranslationString _x = new("...");` on the
     `Translate`-derived class, or edit the control's designer text.
   - A shared/common literal → add to
     [TranslatedStrings.cs](../../../src/app/ResourceManager/TranslatedStrings.cs).
   - Use `SmartFormat` placeholders for plurals (e.g. `"{0:item|items}"`).
2. **Build first:** `dotnet build /v:q` must succeed (the generator reflects over the built assemblies).
3. **Regenerate the catalog:** run from the repo root:
   ```
   .\update-loc.cmd
   ```
   This runs `TranslationApp`, regenerates [English.xlf](../../../src/app/GitUI/Translation/English.xlf),
   and stages it.
4. **Commit together:** include the regenerated `English.xlf` in the **same** commit as the code change.

## Verify

- `git status` shows `English.xlf` staged/updated.
- The new string appears in `English.xlf`.

## STOP conditions / hard rules

- **NEVER** hand-edit any `.xlf` file — they are generated.
- Do **not** wrap non-UI strings (logs, internal exceptions) as `TranslationString`.
- If you skip `update-loc.cmd`, **CI will fail** on a stale `English.xlf`.

Background: [translation-system](../../copilot-docs/L2-core-platform/translation-system.md).
