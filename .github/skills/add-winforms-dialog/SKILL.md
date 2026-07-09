---
name: add-winforms-dialog
description: '**EXECUTION (runbook) SKILL** — Add a new WinForms dialog/control to Git Extensions following the project''s base classes and naming conventions. USE FOR: creating a new Form or UserControl, wiring it to git via UICommands. DO NOT USE FOR: editing existing dialogs'' logic only, or non-UI code. Triggers: "add a new dialog", "create a form", "new WinForms control", "new settings page".'
---

# Add a WinForms Dialog (runbook)

Create a new dialog that matches the codebase's conventions. Follow in order.

## Steps

1. **Pick the base class** (see [ui-composition](../../copilot-docs/L1-conceptual/ui-composition.md)):
   - Repo-aware dialog → derive from `GitModuleForm` (gives `UICommands` / `Module`).
   - Standard dialog chrome → derive from / use `GitExtensionsDialog`.
   - A panel/control → derive from `GitModuleControl`.
   - **NEVER** derive directly from raw `Form`.
2. **Place the files** under `src/app/GitUI/CommandsDialogs/` (or a suitable subfolder): `Form<Name>.cs`,
   `Form<Name>.Designer.cs`, `Form<Name>.resx`.
3. **Name controls** per [ui_design_guidelines.md](../../ui_design_guidelines.md):
   `btn`, `lbl`, `txt`, `cbx`, `chk`, `rb`, `gbx`, `tlpnl`, `flpnl`, `lnk`, …
4. **Talk to git** only through `UICommands` / `Module`; build commands with `Commands.*` (structured
   `IGitCommand` preferred). **NEVER** `new` a `GitModule` or spawn `git` directly.
5. **Localize:** make every user-facing string a `TranslationString`, then run the
   [add-translation-strings](../add-translation-strings/SKILL.md) runbook (`update-loc.cmd`).
6. **Hotkeys (if any):** register via `HotkeySettingsManager`, not hardcoded key handling
   (see [hotkey-system](../../copilot-docs/L2-core-platform/hotkey-system.md)).
7. **Refresh:** after a state-changing action, call `UICommands.RepoChangedNotifier.Notify()`.

## Verify

- `dotnet build /v:q` succeeds; control names follow the prefixes.
- `English.xlf` updated (localization gate).
- UI touched only on the UI thread ([threading-model](../../copilot-docs/L1-conceptual/threading-model.md)).

## STOP conditions

- Deriving from raw `Form`, hardcoded `Color`s (use `AppColor`), or raw git command strings → fix before proceeding.
- Add a unit test if the dialog builds git arguments via `Commands.*`.
