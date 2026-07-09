# L1 — Conceptual · Docs-Index

**TL;DR:** Foundational understanding of *what* Git Extensions is and how it is put together.
Start here to orient before diving into a specific subsystem (L2) or flow (L3).
Read the [L0 primer](../L0-foundations/gitextensions-primer.md) first.

> **Status: SKELETON** — docs below are the roadmap. `planned` = not written yet.

| Doc | One-line description | Status |
| --- | --- | --- |
| [gitextensions-overview.md](gitextensions-overview.md) | What Git Extensions is, who uses it, core capabilities, and non-goals. | done |
| [architecture-overview.md](architecture-overview.md) | The tiers (Extensibility → GitCommands → GitUI → exe), dependency direction, plugins & native. | done |
| [project-map.md](project-map.md) | Every project under `src/` and `tests/` and what it owns. | done |
| [domain-model.md](domain-model.md) | Core model types: `ObjectId`, `GitRevision`, `GitRef`, `GitItemStatus`, `GitItem`. | done |
| [glossary.md](glossary.md) | Extended terminology beyond the L0 anchor. | done |
| [ui-composition.md](ui-composition.md) | How `FormBrowse` composes panels, toolbars, `RevisionGrid`, and dockable controls. | done |
| [threading-model.md](threading-model.md) | UI thread rules, `AsyncLoader`, `JoinableTaskFactory`, background work conventions. | done |

## Reading order for onboarding

`gitextensions-overview` → `architecture-overview` → `project-map` → (then jump to the relevant [L2](../L2-core-platform/docs-index.md) subsystem).

Back to [master docs-index](../docs-index.md).
