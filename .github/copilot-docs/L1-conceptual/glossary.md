<!-- L1 CONCEPTUAL. Agentic doc: TL;DR at top, code pointers not code, ~100 lines. -->
# Glossary (L1)

**TL;DR:** Terminology used across Git Extensions — both standard git concepts as they appear in
this codebase and Git-Extensions-specific types/UI names. The [L0 primer](../L0-foundations/gitextensions-primer.md)
has the short list; this is the extended one.

**Related:** [domain-model](domain-model.md) (the types behind these terms) · [architecture-overview](architecture-overview.md)

## Git concepts (as modelled here)

| Term | Meaning in this codebase |
| --- | --- |
| **Revision / commit** | A commit, modelled by `GitRevision` (id, author, committer, subject, body). |
| **ObjectId** | A git SHA-1 hash; the immutable `ObjectId` struct. |
| **Ref** | A branch/tag/remote pointer; `GitRef` (implements `IGitRef`). |
| **HEAD / detached HEAD** | Current checkout; "detached" = not on a branch (see `DetachedHeadParser`). |
| **Index / staging area** | Staged changes; shown as the artificial **Index** row in the grid. |
| **Working directory / worktree** | The checked-out files; the artificial **WorkTree** row represents uncommitted changes. |
| **Item status** | A changed file's state; `GitItemStatus` (tracked/new/deleted/changed/…). |
| **Remote / upstream / tracking** | A remote repo and the branch a local branch tracks. |
| **Submodule** | A nested git repo inside the current one (see `SubmoduleHelpers`). |
| **Stash / rebase / merge / cherry-pick / squash / fast-forward** | Standard git operations, each exposed by a dialog under `CommandsDialogs/`. |

## Git Extensions specific

| Term | Meaning |
| --- | --- |
| **GitModule** | The per-repository engine object (`IGitModule`); runs git and reads state. |
| **GitUICommands** | Bridge that runs operations with UI feedback and raises repo-changed events. |
| **IGitCommand** | A structured git command carrying `AccessesRemote` / `ChangesRepoState`. |
| **Executable** | Abstraction over launching the `git` process (`IExecutable`/`IProcess`). |
| **FormBrowse** | The main repository window. |
| **RevisionGrid / RevisionGridControl** | The commit-graph control. |
| **RevisionGraphRow** | One row/node in the rendered commit graph. |
| **Artificial commits** | The synthetic **WorkTree** and **Index** rows shown atop real history. |
| **RepoObjectsTree** | The left panel tree of branches/tags/stashes/submodules; nodes derive from `Node`. |
| **Plugin (IGitPlugin)** | An optional feature discovered via MEF (see [plugin-system](../L2-core-platform/plugin-system.md)). |
| **TranslationString** | A localizable UI string extracted into `English.xlf`. |
| **AppSettings** | The app's own preferences facade (distinct from git config). |
| **RepoChangedNotifier** | Fires after state-changing operations so views refresh. |
| **AsyncLoader** | Helper that loads data on a background thread then calls back on the UI thread. |

**Next:** [domain-model](domain-model.md) for the concrete types.
