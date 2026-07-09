<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Diff & Blame Flow (L3)

**TL;DR:** Diffs are shown either in the embedded file viewer or an external difftool
(`FormDiff` compares two revisions, using merge-base for branch comparisons). Blame is shown by
`FormBlame`, whose blame control loads annotations asynchronously. Both are read-only views over
git output.

**Related:** [file-history-flow](file-history-flow.md) · [revision-grid-flow](revision-grid-flow.md) · [git-command-execution](../L2-core-platform/git-command-execution.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormDiff.cs](../../../src/app/GitUI/CommandsDialogs/FormDiff.cs) ·
[FormBlame.cs](../../../src/app/GitUI/CommandsDialogs/FormBlame.cs) · diff/editor controls under
[GitUI/Editor/](../../../src/app/GitUI/Editor/)

## Why

Understanding changes is core to a git GUI. Diffs and blame must handle large files, binary files,
different encodings, and both internal (embedded editor) and external (configured difftool)
viewing — so these flows delegate to the file viewer or an external tool rather than reimplementing
diffing.

## What / How

- **Diff (`FormDiff`)** — compares two selected revisions. For branch comparison it diffs from the
  **merge-base**. It can render inside the embedded viewer or launch the configured difftool via
  `Module.OpenWithDifftoolDirDiff(...)` (a separate process).
- **Blame (`FormBlame`)** — annotates each line with its last-changing commit. The blame control
  loads asynchronously (cancellable) and lets you navigate to the responsible revision.
- **Embedded viewer** — the diff/file viewer lives under `GitUI/Editor/` (based on
  `ICSharpCode.TextEditor`) with syntax highlighting.

## Quirks

- Branch-vs-branch diff uses the **merge-base**, not a raw two-dot diff, so it shows "what changed
  on this branch".
- Blame loads **async** with cancellation; switching selection cancels the prior load.
- External difftool runs out-of-process; the app doesn't block on it as an internal view.

## Hard rules

- Prefer the existing viewer/difftool integration; don't shell out to `git diff` ad hoc.
- Load blame/diff off the UI thread and marshal back ([threading-model](../L1-conceptual/threading-model.md)).
- Respect the user's configured difftool/mergetool settings.

**Next:** [file-history-flow](file-history-flow.md).
