<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Settings System (L2)

**TL;DR:** Git Extensions has **two** settings worlds. (1) **App settings** — the app's own
preferences, stored in the `GitExtensions.settings` XML file and accessed through the static
`AppSettings` facade. (2) **Git config** — real `git config` values (system/global/local),
accessed through the `*Settings` classes and `IGitModule.GetSetting/GetEffectiveSetting`. Know
which world a value belongs to before reading or writing it.

**Related:** [git-module](git-module.md) · [L0 primer](../L0-foundations/gitextensions-primer.md) · master [docs-index](../docs-index.md)

**Key files:** [AppSettings.cs](../../../src/app/GitCommands/Settings/AppSettings.cs) ·
[SettingsPath.cs](../../../src/app/GitCommands/Settings/SettingsPath.cs) ·
[DistributedSettings.cs](../../../src/app/GitCommands/Settings/DistributedSettings.cs) ·
[GitExtSettingsCache.cs](../../../src/app/GitCommands/Settings/GitExtSettingsCache.cs)

## Why

The app must persist user preferences independently of any repository, **and** read/write git's
own configuration (which is layered: system → global → local). Separating these keeps app prefs
portable across repos while still honoring git's precedence rules for config.

## What

### 1. App settings — `AppSettings`

- Static facade with strongly-typed properties (e.g. sort orders, confirmations, appearance).
  [AppSettings.cs](../../../src/app/GitCommands/Settings/AppSettings.cs) (partial across several files).
- Stored in **`GitExtensions.settings`** (XML). Note the code split: `ApplicationName = "Git
  Extensions"` but `ApplicationId = "GitExtensions"` (used for the file name and data folder).
- Portable vs installed: `ApplicationDataPath` / `IsPortable()` decide where the file lives.
- Keys are organised hierarchically via `SettingsPath` / `AppSettingsPath` (e.g. `Appearance`,
  `Confirmations`, `Detailed`, `Dialogs`, `RecentRepositories`).
- Backing store: `GitExtSettingsCache` / `FileSettingsCache` read & cache the XML; `SettingsContainer`
  (a `DistributedSettings`) is the root source.

### 2. Git config settings

- `DistributedSettings` — settings that follow git's distributed layering (local/global).
  `DetachedSettings` / `DetachedServerSettings` — non-repo-bound.
- `EffectiveGitConfigSettings` / `GitConfigSettings` / `ConfigFileSettingsCache` — read the
  actual `git config`. `IGitModule.GetSetting()` and `GetEffectiveSetting()` bridge to these.
- Encoding-related config uses `GitEncodingSettingsGetter/Setter`.

### Setting abstractions

`ISetting` / `Setting` / `RuntimeSetting` model a single setting; `SettingsSource<T>` and
`SettingsSourceExtension` provide typed get/set over a source.

## How (typical usage)

```
bool confirm = AppSettings.DontConfirmCommitIfNoBranch;     // app setting
AppSettings.LastCommitMessage = message;                    // app setting (persisted)
string editor = module.GetEffectiveSetting("core.editor");  // git config (layered)
```

- The **Settings dialog** UI lives in `GitUI/CommandsDialogs/SettingsDialog/`; controls bind via
  `GitUI/SettingControlBindings/`.
- `AppSettings.Saved` fires when settings are persisted; `AppSettings.SaveSettings()` writes the file.

## Hard rules

- Decide the **world** first: app preference → `AppSettings`; git behavior → `GetEffectiveSetting`/config classes.
- **NEVER** hand-edit the `GitExtensions.settings` path or key strings — go through `AppSettings` / `SettingsPath`.
- Respect git config precedence; use `GetEffectiveSetting` (layered) rather than reading one file.
- New user-facing settings SHOULD get a typed `AppSettings` member plus a Settings-dialog binding.

**Next:** [plugin-system](plugin-system.md) — plugins get their own settings via `IGitPluginSettingsContainer`.
