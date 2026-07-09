<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Scripts Engine (L2)

**TL;DR:** Users can define their own commands ("scripts") that run at lifecycle events
(before/after commit, pull, push…) or from menus/hotkeys, without writing a plugin. `ScriptInfo`
holds each script's metadata; `ScriptsManager` stores and runs them; `ScriptRunner` performs
token substitution (e.g. `{sHashes}`, `{UserInput}`) and executes.

**Related:** [hotkey-system](hotkey-system.md) · [plugin-system](plugin-system.md) · [commit-flow](../L3-flows/commit-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Teams have bespoke workflows (run a linter before commit, open a tool after pull). Rather than
force everyone to build a plugin, Git Extensions lets users register parameterised shell/tool
commands that hook into well-known events — a lightweight extension point configured entirely in
settings.

## What (`GitUI/ScriptsEngine/`)

- **`ScriptEvent`** (enum) — the hook points: `BeforeCommit`, `AfterCommit`, `BeforePull`,
  `AfterPull`, `ShowInUserMenuBar`, etc. [ScriptEvent.cs](../../../src/app/GitUI/ScriptsEngine/ScriptEvent.cs).
- **`ScriptInfo`** — one script's definition: name, command, arguments, event filter, hotkey, icon.
  [ScriptInfo.cs](../../../src/app/GitUI/ScriptsEngine/ScriptInfo.cs).
- **`ScriptsManager`** (`IScriptsManager`, `IScriptsRunner`) — loads scripts from settings
  (XML), registers their hotkeys, and runs event scripts.
  [ScriptsManager.cs](../../../src/app/GitUI/ScriptsEngine/ScriptsManager.cs).
- **`ScriptRunner`** — `RunScript(...)` executes a single script.
  [ScriptsManager.ScriptRunner.cs](../../../src/app/GitUI/ScriptsEngine/ScriptsManager.ScriptRunner.cs).
- **`ScriptOptionsParser`** — expands tokens like `{UserInput:label=default}`, `{plugin.name}`,
  and revision/branch placeholders. [ScriptOptionsParser.cs](../../../src/app/GitUI/ScriptsEngine/ScriptOptionsParser.cs).

## How (event scripts)

Dialogs invoke scripts through the runner at defined points, e.g. in
[FormCommit](../../../src/app/GitUI/CommandsDialogs/FormCommit.cs):

```
ScriptsRunner.RunEventScripts(ScriptEvent.BeforeCommit, this);   // may cancel the operation
...
ScriptsRunner.RunEventScripts(ScriptEvent.AfterCommit, this);
```

- `Before*` scripts can **cancel** the operation (return value gates the flow).
- Tokens are resolved by `ScriptOptionsParser` against the current revision/selection before run.
- Scripts marked `ShowInUserMenuBar` appear in the UI; others fire only on their event.

## Hard rules

- Run scripts through `IScriptsRunner`/`ScriptsManager`, not ad-hoc process launches.
- Honor the cancel result of `Before*` event scripts — do not proceed if a script cancels.
- Resolve tokens via `ScriptOptionsParser`; never string-concatenate user input into a command line.

**Next:** [service-container](service-container.md) for how `IScriptsManager` is resolved.
