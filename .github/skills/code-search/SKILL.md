---
name: code-search
description: '**SDLC (SME-first) SKILL** — Efficiently locate code in the Git Extensions repository before editing or explaining. USE FOR: finding the form/command/parser behind a feature, tracing a UI action to its git call, locating a symbol''s definition and usages. DO NOT USE FOR: making edits (do the edit after finding), or questions already answered by a DAG doc. Triggers: "where is", "find the code that", "which form handles", "locate the command for".'
---

# Code Search (Git Extensions)

Find the right code fast by using the DAG map first, then targeted symbol search.

## Phase 1 — Orient via the docs (do this first)

1. Check the [master docs-index](../../copilot-docs/docs-index.md) **ownership table** to map the
   topic to a code area, and follow the matching **reading chain**.
2. The [project-map](../../copilot-docs/L1-conceptual/project-map.md) tells you which project owns it.

## Phase 2 — Search with the repo's conventions

Knowing these conventions makes search precise:
- **Dialogs / UI actions** → `src/app/GitUI/CommandsDialogs/Form<Name>.cs` (+ `.Designer.cs`,
  and split partials like `FormBrowse.Init*.cs`).
- **Git commands (structured)** → `Commands.<Verb>(...)` in
  [Commands.cs](../../../src/app/GitCommands/Git/Commands.cs) /
  [Commands.Arguments.cs](../../../src/app/GitCommands/Git/Commands.Arguments.cs).
- **Repo operations / state** → methods on `GitModule` /
  [IGitModule.cs](../../../src/app/GitExtensions.Extensibility/Git/IGitModule.cs).
- **Output parsing** → `*Parser` / `RevisionReader` in `src/app/GitCommands/`.
- **Left-panel tree nodes** → `src/app/GitUI/LeftPanel/`.
- **Contracts/interfaces** → `src/app/GitExtensions.Extensibility/`.

Techniques: search the **symbol** (exact `ClassName`/`MethodName`), then find its usages/definition.
For a UI action, grep the button/menu handler, then follow to `Commands.*` / `Module.*` /
`StartCommandLineProcessDialog` / `FormProcess.ShowDialog`.

## Phase 3 — Confirm before acting

- Open the file and confirm it's the real owner (not a similarly named helper/test).
- Note the tier boundary you're crossing so an edit stays in the right project.

## Hard rules

- Use the docs-index **before** brute-force searching the whole tree.
- Verify the match by opening the file; don't act on a filename guess.
- Respect the dependency direction (see [architecture-overview](../../copilot-docs/L1-conceptual/architecture-overview.md)).
