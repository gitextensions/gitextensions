<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Hotkey System (L2)

**TL;DR:** Keyboard shortcuts are modelled as `HotkeyCommand`s grouped per form/control and
managed by `HotkeySettingsManager`. Forms expose an `IHotkeySettingsLoader` (via `GitModuleForm`)
to look up their bindings; users edit them in the Settings dialog's hotkey page. Defaults are
generated in code.

**Related:** [ui-composition](../L1-conceptual/ui-composition.md) · [settings-system](settings-system.md) · [scripts-engine](scripts-engine.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Power users expect customisable, discoverable keyboard shortcuts, and different windows need
different bindings without collisions. Centralising hotkeys as serializable commands lets the app
persist user overrides, detect duplicate keys, and show/edit everything in one settings page.

## What

- **`HotkeyCommand`** — an XML-serializable binding: a command code, a name, and the `Keys`.
  [HotkeyCommand.cs](../../../src/app/ResourceManager/Hotkey/HotkeyCommand.cs).
- **`HotkeySettings`** — a named set of `HotkeyCommand`s for one form/control group.
  [HotkeySettings.cs](../../../src/app/GitUI/Hotkey/HotkeySettings.cs).
- **`HotkeySettingsManager`** (`IHotkeySettingsManager`) — `LoadSettings()`,
  `CreateDefaultSettings()`, `IsUniqueKey()`; persists user overrides.
  [HotkeySettingsManager.cs](../../../src/app/GitUI/Hotkey/HotkeySettingsManager.cs).
- **`IHotkeySettingsLoader`** — exposed on `GitModuleForm` so a form can read its own hotkeys.
- **Settings UI** — `ControlHotkeys` page for editing bindings.
  [ControlHotkeys.cs](../../../src/app/GitUI/CommandsDialogs/SettingsDialog/Pages/ControlHotkeys.cs).

## How (add a hotkey to a form)

1. Define a hotkey command code for the form and add it to that form's default `HotkeySettings`
   (via `CreateDefaultSettings()` in `HotkeySettingsManager`).
2. In the form, read the configured `Keys` through the `IHotkeySettingsLoader` and handle the key
   (typically in `ProcessCmdKey`/hotkey dispatch).
3. The Settings → Hotkeys page (`ControlHotkeys`) picks it up automatically for user editing.

Note: user scripts can also be bound to hotkeys — see [scripts-engine](scripts-engine.md).

## Hard rules

- Register/lookup hotkeys through `HotkeySettingsManager` — don't hardcode `Keys` handling that bypasses user settings.
- Keep command **names** stable; they are the serialization key for user overrides.
- Check `IsUniqueKey()` semantics when adding defaults to avoid collisions within a form.

**Next:** [scripts-engine](scripts-engine.md).
