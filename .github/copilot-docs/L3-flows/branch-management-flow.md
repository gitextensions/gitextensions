<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Branch Management Flow (L3)

**TL;DR:** Create, rename, and delete branches each have a dialog. Create and rename use the
argument-only command pattern (`ArgumentString` + `FormProcess.ShowDialog`); delete uses the
structured `IGitCommand` + `StartCommandLineProcessDialog` path (it needs worktree-safety checks).

**Related:** [checkout-flow](checkout-flow.md) · [structured-commands](../L2-core-platform/structured-commands.md) · [commit-flow](commit-flow.md) (flow template) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormCreateBranch.cs](../../../src/app/GitUI/CommandsDialogs/FormCreateBranch.cs) ·
[FormRenameBranch.cs](../../../src/app/GitUI/CommandsDialogs/FormRenameBranch.cs) ·
[FormDeleteBranch.cs](../../../src/app/GitUI/CommandsDialogs/FormDeleteBranch.cs)

## Why

Branch operations have different risk profiles: creating/renaming is low-risk, while **deleting**
can lose work or hit worktree constraints. The delete path therefore carries state-change metadata
and extra guards, while create/rename stay simple.

## What / How

| Operation | Form | Builds | Runs via |
| --- | --- | --- | --- |
| Create | `FormCreateBranch` | `Commands.Branch(...)` / `Commands.CreateOrphan(...)` (`ArgumentString`) | `FormProcess.ShowDialog` |
| Rename | `FormRenameBranch` | `Commands.RenameBranch(old, new)` (`ArgumentString`) | `FormProcess.ShowDialog` |
| Delete | `FormDeleteBranch` | `Commands.DeleteBranch(branches, force)` (`IGitCommand`) | `UICommands.StartCommandLineProcessDialog` |

- **Create** supports orphan branches and optional checkout-after-create; reverts on failure.
- **Rename** can auto-normalise the branch name (if `AutoNormaliseBranchName` is enabled).
- **Delete** checks worktrees, prevents deleting the current branch, and distinguishes
  merged vs unmerged (force). It may run `worktree prune` afterward.

## Hard rules

- Deleting a branch is destructive — keep the current-branch and worktree guards intact.
- Quote/normalise branch names (`.Quote()`/normaliser); never concatenate raw user text into args.
- After any of these, refresh via `RepoChangedNotifier`.

**Next:** [clone-flow](clone-flow.md).
