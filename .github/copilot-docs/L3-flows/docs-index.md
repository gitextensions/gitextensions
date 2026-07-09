# L3 — Flows · Docs-Index

**TL;DR:** End-to-end user flows that compose the [L2](../L2-core-platform/docs-index.md) subsystems.
Each flow doc traces a user action from the UI down to `git` and back, with code pointers at every hop.
Read the [L0 primer](../L0-foundations/gitextensions-primer.md) first.

> **Status: SKELETON** — docs below are the roadmap. `planned` = not written yet.

## Repository & history

| Doc | One-line description | Status |
| --- | --- | --- |
| `browse-flow.md` | App → `FormBrowse` startup, loading a repo, populating panels. | planned |
| `revision-grid-flow.md` | How history is read (`RevisionReader`) and rendered in `RevisionGrid`. | planned |
| `diff-blame-flow.md` | Viewing diffs and blame for files/commits. | planned |
| `file-history-flow.md` | `FormFileHistory` — following a single file's history. | planned |

## Working with changes

| Doc | One-line description | Status |
| --- | --- | --- |
| [commit-flow.md](commit-flow.md) | `FormCommit` — staging, commit message, amend, hooks. | done |
| `stash-flow.md` | `FormStash` — save/apply/pop/drop stashes. | planned |
| `conflict-resolution-flow.md` | `FormResolveConflicts` + `MergeConflictHandler`. | planned |

## Branches & remotes

| Doc | One-line description | Status |
| --- | --- | --- |
| `checkout-flow.md` | `FormCheckoutBranch` / `FormCheckoutRevision`. | planned |
| `branch-management-flow.md` | Create / rename / delete branches. | planned |
| `clone-flow.md` | `FormClone` — clone a remote repository. | planned |
| `push-flow.md` | `FormPush` — push, force options, tracking. | planned |
| `pull-flow.md` | `FormPull` — fetch + merge/rebase. | planned |
| `rebase-flow.md` | `FormRebase` — interactive and non-interactive rebase. | planned |
| `merge-flow.md` | `FormMergeBranch` — merge branches. | planned |

## Submodules

| Doc | One-line description | Status |
| --- | --- | --- |
| `submodule-flow.md` | `FormSubmodules` + `SubmoduleHelpers` — init/update/sync. | planned |

Back to [master docs-index](../docs-index.md).
