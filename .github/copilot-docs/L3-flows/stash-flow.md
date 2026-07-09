<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Stash Flow (L3)

**TL;DR:** `FormStash` saves, applies, pops, and drops stashes. It goes through
`UICommands.StashSave` / `StashApply` / `StashDrop` (which wrap the underlying git commands). It
supports partial (per-file) stashing and a keep-index option, and shows the current worktree as
the first item.

**Related:** [commit-flow](commit-flow.md) · [pull-flow](pull-flow.md) · [git-module](../L2-core-platform/git-module.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormStash.cs](../../../src/app/GitUI/CommandsDialogs/FormStash.cs)

## Why

Stashing lets users park uncommitted work safely (e.g. before checkout/pull). Because stashes are
easy to lose track of, the dialog surfaces existing stashes, their contents, and safe apply/pop/drop
actions in one place.

## What / How

1. The dialog lists existing stashes plus the **current worktree** (first item) with its changes.
2. **Save** — `UICommands.StashSave(...)` (optionally keep index, include untracked, or stash only
   selected files).
3. **Apply / Pop** — `UICommands.StashApply(...)` restores a stash (pop = apply + drop).
4. **Drop** — `UICommands.StashDrop(...)` removes a stash.
5. `RepoChangedNotifier` refreshes views after any change.

## Quirks

- **Partial stash**: only selected files can be stashed.
- **Keep index** stashes working-tree changes while leaving staged changes intact.
- Applying a stash can conflict → route to [conflict resolution](conflict-resolution-flow.md).

## Hard rules

- Use the `UICommands.Stash*` methods; don't hand-roll stash command strings.
- Dropping a stash is destructive — confirm before dropping.
- Refresh via `RepoChangedNotifier` after save/apply/pop/drop.

**Next:** [conflict-resolution-flow](conflict-resolution-flow.md).
