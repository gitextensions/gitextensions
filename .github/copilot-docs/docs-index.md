# Git Extensions Docs-Index (Master)

**TL;DR — read this first, then load only what your task needs.**
This is the master navigation map for Git Extensions agent documentation.
Use the **Ownership Table** to find which area owns a topic, then follow the matching
**Reading Chain** to load docs in the right order. Read the cheap indexes first; open
individual docs only when the task requires them.

> **Status: SKELETON.** Most linked docs are `planned` (not written yet). This index is the
> roadmap. See [README.md](README.md) for how DAG works and how to add docs.

## Always loaded

- [L0 Foundations Primer](L0-foundations/gitextensions-primer.md) — architecture anchor, glossary, ownership map. **Read this every session.** (< 80 lines)

## Layer indexes (load on demand)

- [L1 — Conceptual](L1-conceptual/docs-index.md) — what Git Extensions is: architecture, terminology, project map.
- [L2 — Core Platform](L2-core-platform/docs-index.md) — git execution, settings, plugins, translation, theming, build/test.
- [L3 — Flows](L3-flows/docs-index.md) — end-to-end user flows (commit, checkout, clone, push/pull, rebase, diff).

## Ownership Table (topic → area → where to look)

Use this to route a question to the right layer/doc and the right part of the codebase.

| Topic | Layer | Where in code |
| --- | --- | --- |
| App startup / entry point | L1 | `src/app/GitExtensions/` (`Program`, `GitExtensionsForm`) |
| Running git commands / process execution | L2 | `src/app/GitCommands/Git/` (`GitModule`, `Executable`, `Commands`) |
| Command arguments (structured) | L2 | `src/app/GitExtensions.Extensibility/` (`ArgumentBuilder`), `GitCommands.Git.Commands` |
| UI dialogs & forms | L1/L3 | `src/app/GitUI/CommandsDialogs/` (`FormBrowse`, `FormCommit`, …) |
| Commit graph / revision grid | L3 | `src/app/GitUI/UserControls/RevisionGrid/` |
| Settings & configuration | L2 | `src/app/GitCommands/Settings/`, `GitCommands/Config/` |
| Plugins & extensibility | L2 | `src/app/GitExtensions.Extensibility/Plugins/`, `src/plugins/`, `GitUIPluginInterfaces/` |
| Translation / localization | L2 | `src/app/ResourceManager/`, `GitUI/Translation/English.xlf` |
| Theming / colors | L2 | `src/app/GitUI/Theming/`, `GitUI/Themes/` |
| Hotkeys | L2 | `src/app/GitUI/Hotkey/` |
| User scripts engine | L2 | `src/app/GitUI/ScriptsEngine/` |
| Windows Explorer shell integration | L2 | `src/native/GitExtensionsShellEx/` |
| Build / installer / CI | L2 | `Setup/`, `.github/workflows/`, `eng/` |
| Tests | L2 | `tests/` (NUnit, NSubstitute, AwesomeAssertions) |
| Submodules | L2/L3 | `src/app/GitCommands/Submodules/`, `GitCommands/Git/SubmoduleHelpers.cs` |

## Reading Chains (ordered — read top to bottom, then verify against code)

Ordered doc sequences for the most common questions. `→` means "read next". End every chain by
verifying against the actual source files referenced in the docs.

> Chains reference `planned` docs. As docs are written, these become live.

- **"How does Git Extensions run a git command?"**
  L0 primer → [git-command-execution](L2-core-platform/git-command-execution.md) → [structured-commands](L2-core-platform/structured-commands.md) → verify in `GitModule` / `Executable`.
- **"How does the commit flow work?"**
  L0 primer → [architecture-overview](L1-conceptual/architecture-overview.md) → [commit-flow](L3-flows/commit-flow.md) → verify in `FormCommit`.
- **"How does checkout / branch switching work?"**
  L0 primer → [checkout-flow](L3-flows/docs-index.md) → verify in `FormCheckoutBranch`.
- **"How do plugins work / how do I write one?"**
  L0 primer → [plugin-system](L2-core-platform/docs-index.md) → verify in `GitExtensions.Extensibility/Plugins/` + a sample under `src/plugins/`.
- **"How does translation / adding UI strings work?"**
  L0 primer → [translation-system](L2-core-platform/docs-index.md) → verify with `update-loc.cmd` + `English.xlf`.
- **"How is the commit graph rendered?"**
  L0 primer → [architecture-overview](L1-conceptual/architecture-overview.md) → [revision-grid-flow](L3-flows/docs-index.md) → verify in `RevisionGrid`.

## Cross-cutting

- [Custom agents](../agents/README.md) — domain-scoped agents (planned).
- [Skills](../skills/README.md) — reusable workflows: code search, deep explanation, PR creation, doc management (planned).
