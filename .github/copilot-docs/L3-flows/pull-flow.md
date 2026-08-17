<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Pull Flow (L3)

**TL;DR:** `FormPull` fetches from a remote and then merges or rebases (user's choice). It composes
`Commands.Fetch` with `Commands.Merge` or `Commands.Rebase` and runs via `FormProcess`. It can
auto-stash local changes, run before/after pull scripts, detect conflicts, and pop the stash after.

**Related:** [push-flow](push-flow.md) · [rebase-flow](rebase-flow.md) · [merge-flow](merge-flow.md) · [conflict-resolution-flow](conflict-resolution-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormPull.cs](../../../src/app/GitUI/CommandsDialogs/FormPull.cs) ·
`Commands.Fetch` / `Merge` / `Rebase` in [Commands.Arguments.cs](../../../src/app/GitCommands/Git/Commands.Arguments.cs)

## Why

"Pull" is really fetch + integrate, and the safe way to integrate depends on the user's workflow
(merge vs rebase). Wrapping both in one dialog — with auto-stash and conflict handling — makes a
multi-step, error-prone operation reliable.

## What / How

1. User picks remote/branch and integration mode (merge, rebase, or fetch-only).
2. `FormPull` (`CreateFormProcess()`) builds `Commands.Fetch(...)` and, per the choice,
   `Commands.Merge(...)` or `Commands.Rebase(...)`.
3. It runs via `FormProcess.ShowDialog(...)`.
4. If local changes exist it may **auto-stash** first and **pop** afterward; conflicts route to
   [conflict resolution](conflict-resolution-flow.md).
5. `ScriptEvent.BeforePull` / `AfterPull` scripts run around the operation (see [scripts-engine](../L2-core-platform/scripts-engine.md)).

## Quirks

- Merge vs rebase changes history shape — the dialog makes the choice explicit.
- Auto-stash/pop protects uncommitted work; a conflict during pop is surfaced.
- Fetch accesses the remote (remote-aware progress/auth).

## Hard rules

- Honor the user's merge/rebase choice; don't silently pick one.
- Preserve auto-stash/pop and conflict detection — dropping them risks losing local work.
- Run before/after pull scripts and refresh via `RepoChangedNotifier`.

**Next:** [rebase-flow](rebase-flow.md).
