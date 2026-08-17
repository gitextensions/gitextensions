<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Merge Flow (L3)

**TL;DR:** `FormMergeBranch` merges another branch into the current one. It builds
`Commands.MergeBranch(...)` and runs it via `FormProcess`. Options include no-fast-forward,
squash, no-commit, and adding log messages. Conflicts route to the conflict-resolution flow.

**Related:** [rebase-flow](rebase-flow.md) · [pull-flow](pull-flow.md) · [conflict-resolution-flow](conflict-resolution-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormMergeBranch.cs](../../../src/app/GitUI/CommandsDialogs/FormMergeBranch.cs) ·
`Commands.MergeBranch` in [Commands.Arguments.cs](../../../src/app/GitCommands/Git/Commands.Arguments.cs)

## Why

Merging integrates branches while preserving history. Different situations want different merge
shapes (fast-forward vs an explicit merge commit vs squash), so the dialog surfaces those options
and handles the conflict case cleanly.

## What / How

1. User selects the branch to merge and options (no-ff, squash, no-commit, log messages).
2. `FormMergeBranch` builds `Commands.MergeBranch(branchToMerge, noFastForward, squash, noCommit,
   addLogMessages, getPathForGitExecution, logMessagesCount)`.
3. It runs via `FormProcess.ShowDialog(...)`; on conflict, route to
   [conflict resolution](conflict-resolution-flow.md), then commit.

## Quirks

- **--no-ff** forces a merge commit even when a fast-forward is possible (keeps topic-branch shape).
- **squash** stages the result without recording a merge commit (you commit separately).
- Optional shortlog of merged commits can be added to the merge message.

## Hard rules

- Keep the merge-option semantics accurate (no-ff/squash/no-commit) — they change history shape.
- On conflicts, use the conflict-resolution flow; don't force-complete a conflicted merge.
- Refresh via `RepoChangedNotifier` after merge/commit.

**Next:** [conflict-resolution-flow](conflict-resolution-flow.md).
