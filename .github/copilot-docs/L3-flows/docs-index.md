# L3 — Flows · Docs-Index

**TL;DR:** End-to-end user flows that compose the [L2](../L2-core-platform/docs-index.md) subsystems.
Each flow doc traces a user action from the UI down to `git` and back, with code pointers at every hop.
Read the [L0 primer](../L0-foundations/gitextensions-primer.md) first.

> All docs below are written. `Status` tracks any future additions.

## Repository & history

| Doc | One-line description | Status |
| --- | --- | --- |
| [browse-flow.md](browse-flow.md) | App → `FormBrowse` startup, loading a repo, populating panels. | done |
| [revision-grid-flow.md](revision-grid-flow.md) | How history is read (`RevisionReader`) and rendered in `RevisionGrid`. | done |
| [diff-blame-flow.md](diff-blame-flow.md) | Viewing diffs and blame for files/commits. | done |
| [file-history-flow.md](file-history-flow.md) | `FormFileHistory` — following a single file's history. | done |

## Working with changes

| Doc | One-line description | Status |
| --- | --- | --- |
| [commit-flow.md](commit-flow.md) | `FormCommit` — staging, commit message, amend, hooks. | done |
| [stash-flow.md](stash-flow.md) | `FormStash` — save/apply/pop/drop stashes. | done |
| [conflict-resolution-flow.md](conflict-resolution-flow.md) | `FormResolveConflicts` + `MergeConflictHandler`. | done |

## Branches & remotes

| Doc | One-line description | Status |
| --- | --- | --- |
| [checkout-flow.md](checkout-flow.md) | `FormCheckoutBranch` / `FormCheckoutRevision`. | done |
| [branch-management-flow.md](branch-management-flow.md) | Create / rename / delete branches. | done |
| [clone-flow.md](clone-flow.md) | `FormClone` — clone a remote repository. | done |
| [push-flow.md](push-flow.md) | `FormPush` — push, force options, tracking. | done |
| [pull-flow.md](pull-flow.md) | `FormPull` — fetch + merge/rebase. | done |
| [rebase-flow.md](rebase-flow.md) | `FormRebase` — interactive and non-interactive rebase. | done |
| [merge-flow.md](merge-flow.md) | `FormMergeBranch` — merge branches. | done |

## Submodules

| Doc | One-line description | Status |
| --- | --- | --- |
| [submodule-flow.md](submodule-flow.md) | `FormSubmodules` + `SubmoduleHelpers` — init/update/sync. | done |

Back to [master docs-index](../docs-index.md).
