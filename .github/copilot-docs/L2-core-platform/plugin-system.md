<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Plugin System (L2)

**TL;DR:** Plugins implement `IGitPlugin` (or a specialised sub-interface) and are exported via
**MEF** (VS-MEF / `Microsoft.VisualStudio.Composition`). `PluginRegistry` discovers them through
`ManagedExtensibility` and hosts them in the UI. A plugin `Register()`s hooks, exposes settings,
and `Execute()`s on demand. Built-in plugins live in `src/plugins/`; users can drop extra ones in
the `UserPlugins` folder.

**Related:** [architecture-overview](../L1-conceptual/architecture-overview.md) · [settings-system](settings-system.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [IGitPlugin.cs](../../../src/app/GitExtensions.Extensibility/Plugins/IGitPlugin.cs) ·
[GitPluginBase.cs](../../../src/app/GitExtensions.Extensibility/Plugins/GitPluginBase.cs) ·
[PluginRegistry.cs](../../../src/app/GitUI/Plugin/PluginRegistry.cs) ·
[ManagedExtensibility.cs](../../../src/plugins/GitUIPluginInterfaces/ManagedExtensibility.cs)

## Why

Optional features (background fetch, build-server integration, GitHub/Bitbucket hosting, stats)
should be decoupled from the core so they can ship, fail, and evolve independently. MEF gives a
discovery mechanism where a plugin just declares an `[Export]` and the host finds it — no central
registration list to edit.

## What (the pieces)

### Contracts (`GitExtensions.Extensibility/Plugins/`)

- **`IGitPlugin`** — the core contract: `Id`, `Name`, `Description`, `Icon`, `SettingsContainer`,
  `HasSettings`, `GetSettings()`, `Register(IGitUICommands)`, `Unregister(...)`,
  `Execute(GitUIEventArgs)` (returns whether to refresh `RevisionGrid`).
- **`GitPluginBase`** — convenience base class most plugins derive from.
- **`IRepositoryHostPlugin`** — repo hosts (GitHub, Bitbucket) with PR/remote features.
- **`IGitPluginSettingsContainer`** — how a plugin reads/writes its own settings.

### Legacy interfaces (`src/plugins/GitUIPluginInterfaces/`)

`ManagedExtensibility` (MEF composition), `IGitPluginForCommit`, `PluginsPathScanner`,
`RepositoryHosts/`. This project predates `Extensibility`; both are still referenced.

### Host (`GitUI/Plugin/`)

- **`PluginRegistry`** — static host. `Plugins` and `GitHosters` lists; `InitializeAll()`,
  `InitializeForCommitForm()`, `InitializeGitHostersOnly()` call `LoadPlugins<T>()`, which pulls
  exports via `ManagedExtensibility.GetExports<T>()`.
- **`FailedPluginWrapper`** — wraps a plugin that threw during load so one bad plugin can't crash
  the app.
- **`GitPluginSettingsContainer`** — concrete settings bridge.

### Built-in plugins (`src/plugins/*`)

`BackgroundFetch`, `AutoCompileSubmodules`, `Bitbucket`, `GitHub3`, `GitFlow`,
`BuildServerIntegration`, `Statistics`, `Gource`, `FindLargeFiles`, `ProxySwitcher`,
`CreateLocalBranches`, `DeleteUnusedBranches`, `ReleaseNotesGenerator`.

## How (add / load a plugin)

```
[Export(typeof(IGitPlugin))]           // MEF discovery — no registry edit needed
public sealed class MyPlugin : GitPluginBase { ... }
```

1. Create a project under `src/plugins/`, derive from `GitPluginBase`, `[Export]` the interface.
2. Implement `Register()` to subscribe to `IGitUICommands` events; `Execute()` for the action.
3. Expose settings via `GetSettings()` + `IGitPluginSettingsContainer` (see [settings-system](settings-system.md)).
4. Load timing: general plugins load on background init (`InitializeAll`); commit-form plugins
   load via `InitializeForCommitForm`; repo hosts via `InitializeGitHostersOnly`.

## Hard rules

- Plugins depend **only** on the interface assemblies (`Extensibility` / `GitUIPluginInterfaces`) — never on `GitUI` internals.
- Changes to `GitExtensions.Extensibility/Plugins/` alter the **public plugin interface version** — note it in the commit message.
- **NEVER** let a plugin throw during load uncaught — the host wraps failures in `FailedPluginWrapper`; keep it that way.
- `Execute()` MUST return whether the `RevisionGrid` should refresh.

**Next:** [translation-system](translation-system.md) — plugin UI strings are localised too.
