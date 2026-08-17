<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Revision Grid Loading Flow (L3)

**TL;DR:** `RevisionGridControl` loads history by running `RevisionReader` against the current
`GitModule` (plus `GetStashes()`), on a background thread guarded by a `CancellationTokenSequence`.
It raises `RevisionsLoading`/`RevisionsLoaded` events, builds the commit-graph model
(`RevisionGraphRow`s), and renders lanes/labels. This is the engine behind the browse window.

**Related:** [browse-flow](browse-flow.md) · [git-output-parsing](../L2-core-platform/git-output-parsing.md) · [domain-model](../L1-conceptual/domain-model.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [RevisionGridControl.cs](../../../src/app/GitUI/UserControls/RevisionGrid/RevisionGridControl.cs) ·
[RevisionReader.cs](../../../src/app/GitCommands/RevisionReader.cs) ·
[RevisionGraphRow.cs](../../../src/app/GitUI/UserControls/RevisionGrid/Graph/RevisionGraphRow.cs)

## Why

Commit history can be enormous. The grid must stream revisions in without freezing the UI, cancel
cleanly when the user filters or switches repos, and lay out the branch graph efficiently. Hence a
background reader + cancellation + incremental graph building.

## What / How

1. **Kick off** — a refresh creates a new `RevisionReader(module)` and starts reading on a
   background thread; a `CancellationTokenSequence` cancels any in-flight load first.
2. **Read** — `RevisionReader` parses `git log --format …` into `GitRevision`s (see
   [git-output-parsing](../L2-core-platform/git-output-parsing.md)); stashes are added via
   `GetStashes()`.
3. **Signal** — `RevisionsLoading` fires at start, `RevisionsLoaded` at completion; consumers
   (commit details, toolbars) react.
4. **Build graph** — revisions become `RevisionGraphRow`s with lane segments to parents; the
   control draws rows, refs, and the graph.

## Quirks

- Loading respects **suspend/resume** guards from the browse window to batch updates.
- Filtering (by path, branch, text) restarts the read with a fresh cancellation token.
- Two **artificial** rows (WorkTree, Index) are inserted above real commits.

## Hard rules

- Cancel the previous load before starting a new one (use the `CancellationTokenSequence`); don't leak background reads.
- Marshal to the UI thread before touching the grid ([threading-model](../L1-conceptual/threading-model.md)).
- Keep git-output parsing in `RevisionReader`, not in the control.

**Next:** [diff-blame-flow](diff-blame-flow.md) or [file-history-flow](file-history-flow.md).
