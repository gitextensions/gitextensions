<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Checkout Flow (L3)

**TL;DR:** `FormCheckoutBranch` switches branches. It builds an `IGitCommand` via
`Commands.CheckoutBranch(...)` and runs it with `UICommands.StartCommandLineProcessDialog`, which
fires the repo-changed refresh. It can auto-stash local changes, create/reset a new branch, and
handle tracking-branch setup. `FormCheckoutRevision` does a detached checkout of a specific commit.

**Related:** [structured-commands](../L2-core-platform/structured-commands.md) · [branch-management-flow](branch-management-flow.md) · [commit-flow](commit-flow.md) (flow template) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormCheckoutBranch.cs](../../../src/app/GitUI/CommandsDialogs/FormCheckoutBranch.cs) ·
[FormCheckoutRevision.cs](../../../src/app/GitUI/CommandsDialogs/FormCheckoutRevision.cs) ·
`Commands.CheckoutBranch` in [Commands.cs](../../../src/app/GitCommands/Git/Commands.cs)

## Why

Checkout must safely reconcile the working tree: it may need to stash/restore local changes, warn
about conflicts, or create a new local branch tracking a remote. Encoding those choices in a
structured command (with `ChangesRepoState = true`) ensures the UI refreshes and the right process
dialog is used.

## What / How

1. User picks a branch (and options: how to handle local changes, create/reset a new branch).
2. `FormCheckoutBranch` calls `Commands.CheckoutBranch(branchName, isRemote, localChanges,
   newBranchMode, newBranchName)` → an `IGitCommand`.
3. It runs via `UICommands.StartCommandLineProcessDialog(this, cmd)` — this is the **preferred**
   structured path (see [structured-commands](../L2-core-platform/structured-commands.md)).
4. On success, `RepoChangedNotifier` refreshes the grid and panels.

## Quirks

- **Local changes**: `LocalChangesAction` decides merge/reset/stash behavior; the form can
  auto-stash then pop.
- **New branch**: `CheckoutNewBranchMode` (Create/Reset) + `--track` for remote branches.
- `FormCheckoutRevision` checks out a commit (detached HEAD) rather than a branch.

## Hard rules

- Use the structured `Commands.CheckoutBranch` + `StartCommandLineProcessDialog` path — don't build a raw checkout string.
- Preserve the local-changes handling; silently discarding user changes is a data-loss bug.
- Rely on `RepoChangedNotifier` for refresh after checkout.

**Next:** [branch-management-flow](branch-management-flow.md).
