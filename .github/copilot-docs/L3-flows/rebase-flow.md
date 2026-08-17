<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Rebase Flow (L3)

**TL;DR:** `FormRebase` rebases the current branch onto another (or interactively). It builds
`Commands.Rebase(...)` and, during a conflicted rebase, `Commands.ContinueRebase` /
`SkipRebase` / `AbortRebase`, running each via `FormProcess`. It supports interactive rebase and
manages the continue/skip/abort/edit-todo lifecycle.

**Related:** [merge-flow](merge-flow.md) · [pull-flow](pull-flow.md) · [conflict-resolution-flow](conflict-resolution-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormRebase.cs](../../../src/app/GitUI/CommandsDialogs/FormRebase.cs) ·
`Commands.Rebase` / `ContinueRebase` / `SkipRebase` / `AbortRebase` in [Commands.Arguments.cs](../../../src/app/GitCommands/Git/Commands.Arguments.cs)

## Why

Rebase rewrites history and is stateful: it can pause on conflicts and must be continued, skipped,
or aborted. The dialog exposes that lifecycle safely so users don't leave the repo mid-rebase in a
confusing state.

## What / How

1. User chooses the upstream/onto target and options (interactive, autosquash, etc.).
2. `FormRebase` runs `Commands.Rebase(rebaseOptions)` via `FormProcess.ShowDialog(...)`.
3. If it stops on a conflict, the dialog offers **Continue** (`ContinueRebase`), **Skip**
   (`SkipRebase`), **Abort** (`AbortRebase`), or edit-todo — each a separate git command.
4. Conflicts route to [conflict resolution](conflict-resolution-flow.md); state is re-read after each step.

## Quirks

- **Interactive** rebase opens the todo list for reordering/squashing.
- The middle-of-rebase state (`Module.InTheMiddleOfRebase()`) gates other operations (e.g. commit).
- Abort restores the pre-rebase state; encourage it over leaving a half-done rebase.

## Hard rules

- Always provide continue/skip/**abort** paths — never leave the user stuck mid-rebase.
- Re-check repo state after each rebase step before enabling other actions.
- Refresh via `RepoChangedNotifier` after completion/abort.

**Next:** [merge-flow](merge-flow.md).
