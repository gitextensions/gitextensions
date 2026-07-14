<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# File History Flow (L3)

**TL;DR:** `FormFileHistory` shows the commit history of a single file (or submodule) by reusing
the revision-grid engine filtered to that path. `FormFileHistoryController` builds it; the view
follows renames and can diff/blame any revision of the file.

**Related:** [revision-grid-flow](revision-grid-flow.md) · [diff-blame-flow](diff-blame-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormFileHistory.cs](../../../src/app/GitUI/CommandsDialogs/FormFileHistory.cs) ·
[FormFileHistoryController.cs](../../../src/app/GitUI/CommandsDialogs/FormFileHistoryController.cs)

## Why

Developers often ask "how did this file evolve?" Rather than build a separate history engine, file
history reuses the same `RevisionGrid`/`RevisionReader` machinery with a path filter, so graph,
diff, and blame behave identically to the main window.

## What / How

1. **Open** — `FormFileHistoryController` constructs `FormFileHistory` for a selected path.
2. **Load** — the embedded revision grid runs the [revision-grid flow](revision-grid-flow.md)
   scoped to the file's path (git log for that path), following renames.
3. **Inspect** — selecting a revision shows that version's diff/blame (see
   [diff-blame-flow](diff-blame-flow.md)); comparisons can launch the difftool via `UICommands`.

## Quirks

- **Follows renames** so history isn't truncated when a file was moved.
- A **submodule** can be shown as a "file" whose history is its recorded commits.
- Uses the same async load + cancellation as the main grid.

## Hard rules

- Reuse the revision-grid engine with a path filter; don't fork a parallel history reader.
- Off-thread load, UI-thread render ([threading-model](../L1-conceptual/threading-model.md)).

**Next:** back to the [L3 index](docs-index.md).
