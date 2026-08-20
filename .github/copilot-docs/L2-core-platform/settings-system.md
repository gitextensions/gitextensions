<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Settings System (L2)

**TL;DR:** Git Extensions has **three** settings worlds. (1) **Global app settings** — single XML file,
static `AppSettings` facade, no per-repo override. (2) **Layered app settings** — same XML format spread
across Local / Distributed / Global files so settings can be overridden per-repo or committed alongside
source code; accessed via `StringSetting` / `BoolSetting` / `NumberSetting<T>` and `DistributedSettings`.
(3) **Git config** — real `git config` keys (system/global/local), read via `IGitModule`.
Choose the world before adding any setting.

**Related:** [git-module](git-module.md) · [L0 primer](../L0-foundations/gitextensions-primer.md) · master [docs-index](../docs-index.md)

**Key files:**
[AppSettings.cs](../../../src/app/GitCommands/Settings/AppSettings.cs) ·
[DetailedSettings.cs](../../../src/app/GitCommands/Settings/DetailedSettings.cs) ·
[DistributedSettings.cs](../../../src/app/GitCommands/Settings/DistributedSettings.cs) ·
[SettingsPath.cs](../../../src/app/GitCommands/Settings/SettingsPath.cs) ·
[SettingControlBindingsProvider.cs](../../../src/app/GitUI/SettingControlBindings/SettingControlBindingsProvider.cs) ·
[DistributedSettingsPage.cs](../../../src/app/GitUI/CommandsDialogs/SettingsDialog/DistributedSettingsPage.cs)

## Why

Three concerns — global prefs, per-repo overrides, and git config — each need different
storage and precedence rules; keeping them separate prevents accidental coupling.

## What

### 1. Global app settings — `AppSettings`

- One XML file: **`GitExtensions.settings`** (path from `AppSettings.SettingsFilePath`).
  `ApplicationDataPath` / `IsPortable()` decide its location.
- Strongly-typed `ISetting<T>` properties on `AppSettings` (partial class, several files).
  Keys organised via `AppSettingsPath` / `SettingsPath` (prefixes: `Appearance.`, `Confirmations.`,
  `Detailed.`, …). Created with `Setting.Create(path, nameof(Key), defaultValue)`.
- Backing store: `GitExtSettingsCache` / `FileSettingsCache`; root is a single-node `DistributedSettings` at `SettingLevel.Global`.
- Read / write: `AppSettings.Foo.Value` / `AppSettings.Foo.Value = x`.
- `AppSettings.Saved` fires on persist; `AppSettings.SaveSettings()` flushes the file.
- **No per-repo override** — every user shares one global value.

### 2. Layered app settings — `StringSetting` / `BoolSetting` / `NumberSetting<T>`

Same XML key-value format as §1 but spread across **up to three files** so settings can be
overridden per-repo or committed alongside source code.

**Precedence (highest → lowest):**

| Level | File | `SettingLevel` |
|---|---|---|
| Local | `<repo>/.git/GitExtensions.settings` | `Local` |
| Distributed | `<repo>/GitExtensions.settings` | `Distributed` |
| Global | user `GitExtensions.settings` | `Global` |

`DistributedSettings.CreateEffective(module)` builds the three-node chain; `GetValue` walks highest-to-lowest returning the first non-null hit; `SetValue` routes writes to the correct file via the routing logic in `DistributedSettings.cs`.

**Type model:** `StringSetting`, `BoolSetting`, `NumberSetting<T>` in
[GitExtensions.Extensibility/Settings/](../../../src/app/GitExtensions.Extensibility/Settings/).
Each holds a **full key-path string** and a default value; the path is built with
`SettingsPath.PathFor(nameof(Key))`.

**Defining settings** — see [DetailedSettings.cs](../../../src/app/GitCommands/Settings/DetailedSettings.cs)
for the canonical pattern: a `SettingsPath` constant plus static `StringSetting` / `BoolSetting` /
`NumberSetting<T>` properties.

**Reading at runtime (non-UI):** `setting.ValueOrDefault(module.GetEffectiveSettings())`

**Settings-dialog binding** — the page extends `DistributedSettingsPage`, which wires the
Local / Distributed / Global / Effective level tabs automatically.  The constructor populates
`_controlBindings` using `SettingControlBindingsProvider.CreateControlBinding(setting, control)`.
The base class calls `LoadSetting` / `SaveSetting` on each binding; NEVER bypass them with direct
`control.Text =` assignments — doing so breaks the layered load/save semantics.
`StringSettingControlBinding` normalises `\n` ↔ `\r\n` for `TextBox.Multiline = true` automatically.
See [DetailedSettingsPage.cs](../../../src/app/GitUI/CommandsDialogs/SettingsDialog/Pages/DetailedSettingsPage.cs).

### 3. Git config settings

- `GitConfigSettings` / `EffectiveGitConfigSettings` read real `git config` keys (system/global/local).
- `IGitModule.GetEffectiveSetting("core.editor")` — layered read; use this at call sites.
- `CommonLogic` builds `GitConfigSettingsSet`; shown on the Git tab in FormSettings.
- Used for git behaviour (encoding, editor, merge tool, remote URLs…), **not** for app preferences.

## How (choosing the right world)

```
bool confirm = AppSettings.DontConfirmCommitIfNoBranch;           // §1 global app setting
string prompt = DetailedSettings.AiDiffPromptPrefix               // §2 layered, per-repo override
                    .ValueOrDefault(module.GetEffectiveSettings());
string editor = module.GetEffectiveSetting("core.editor");         // §3 git config
```

Decision guide:
- **§1** — UI preference with no repo context (window layout, theme, update-check interval).
- **§2** — value that makes sense to override per-repo or check into the repo (prompts, tool paths).
- **§3** — mirrors an actual `git config` key (editor, merge tool, remote URL).

## Hard rules

- ALWAYS decide the world first; NEVER mix the three access patterns.
- NEVER hand-craft key path strings — ALWAYS use `SettingsPath.PathFor(nameof(Key))`.
- §2 settings pages MUST extend `DistributedSettingsPage` and bind controls via
  `SettingControlBindingsProvider`. NEVER assign `control.Text` directly.
- For git config always use `GetEffectiveSetting`; NEVER read a single config level directly.
- New §1 members need a snapshot-test entry in
  `AppSettingsTests.ISetting_properties_should_have_stable_storage_keys.verified.txt`.

**Next:** [plugin-system](plugin-system.md) — plugins get their own settings via `IGitPluginSettingsContainer`.
