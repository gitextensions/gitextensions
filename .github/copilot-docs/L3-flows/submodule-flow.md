<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Submodule Flow (L3)

**TL;DR:** `FormSubmodules` manages nested repositories: init, update, and sync. It builds
`Commands.SubmoduleSync(...)` / `SubmoduleUpdate(...)` (`ArgumentString`) and runs them via
`FormProcess`. Selecting a submodule can open it as its own repository in a new `FormBrowse`.
`SubmoduleHelpers` provides the supporting logic.

**Related:** [browse-flow](browse-flow.md) · [git-module](../L2-core-platform/git-module.md) · [structured-commands](../L2-core-platform/structured-commands.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormSubmodules.cs](../../../src/app/GitUI/CommandsDialogs/FormSubmodules.cs) ·
[SubmoduleHelpers.cs](../../../src/app/GitCommands/Git/SubmoduleHelpers.cs) ·
[GitCommands/Submodules/](../../../src/app/GitCommands/Submodules/)

## Why

A repository can contain other git repositories (submodules), each with its own state. Managing
them (init/update/sync) and being able to open one as a first-class repo is essential for
multi-repo projects.

## What / How

1. `FormSubmodules` lists the current repo's submodules and their status.
2. **Init/Update** — `Commands.SubmoduleUpdate(path)` brings a submodule to its recorded commit.
3. **Sync** — `Commands.SubmoduleSync(path)` updates submodule remote URLs from `.gitmodules`.
4. Commands run via `FormProcess.ShowDialog(...)`; opening a submodule launches a new `FormBrowse`
   bound to a new `GitModule` for that nested repo.

## Quirks

- Each submodule is a **separate** `GitModule` (one module per repo — see [git-module](../L2-core-platform/git-module.md)).
- Submodule **status** (ahead/behind/dirty) is computed by a background status provider.
- `GetSubmoduleFullPath()` resolves a submodule's absolute path from the parent.

## Hard rules

- Treat each submodule as its own repo/`GitModule`; never reuse the parent's module for it.
- Use `Commands.Submodule*` for init/update/sync rather than raw command strings.
- Refresh parent and submodule views via `RepoChangedNotifier` after changes.

**Next:** back to the [L3 index](docs-index.md) or the [master docs-index](../docs-index.md).
