<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines.
     Follows the pattern documented in commit-flow.md (the template). -->
# Browse / Open Repository Flow (L3)

**TL;DR:** `FormBrowse` is the main window. Opening (or switching) a repository sets the active
`GitModule` and triggers a revision refresh, which loads history on a background thread via
`RevisionReader` and renders it in `RevisionGridControl`. The left `RepoObjectsTree` and commit
details refresh alongside.

**Related:** [revision-grid-flow](revision-grid-flow.md) · [ui-composition](../L1-conceptual/ui-composition.md) · [commit-flow](commit-flow.md) (flow template) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormBrowse.cs](../../../src/app/GitUI/CommandsDialogs/FormBrowse.cs) (+ partials) ·
[RevisionGridControl.cs](../../../src/app/GitUI/UserControls/RevisionGrid/RevisionGridControl.cs) ·
[RepoObjectsTree.cs](../../../src/app/GitUI/LeftPanel/RepoObjectsTree.cs)

## Why

The browse window is the hub every other flow launches from. It must open a repo, show its
history and refs quickly, and stay responsive while git reads potentially huge logs — so loading
is asynchronous and refresh is coordinated centrally.

## What / How

1. **Open a module** — `FormBrowse` is constructed with `UICommands`/`Module`; switching repos
   updates the active `GitModule` and persists recent-repo history.
2. **Refresh revisions** — `FormBrowse` calls into the grid to refresh; `RevisionGridControl`
   runs `RevisionReader` on a background thread (see [revision-grid-flow](revision-grid-flow.md)).
3. **Populate panels** — the left `RepoObjectsTree` loads branches/tags/stashes/submodules; the
   commit-details/diff panels update via the `FormBrowse.InitCommitDetails` partial.
4. **Enable/disable UI** — `FormBrowse.UpdateTargets` adjusts toolbar/menu state for the current repo.

The window is split across partials (`Designer`, `InitRevisionGrid`, `InitMenusAndToolbars`,
`InitCommitDetails`, `UpdateTargets`) — see [ui-composition](../L1-conceptual/ui-composition.md).

## Quirks

- History loads **asynchronously**; the grid may still be populating right after a repo opens.
- Refresh is suspended/resumed around bulk layout changes to avoid flicker and wasted work.
- The grid shows the artificial **WorkTree** and **Index** rows above real commits.

## Hard rules

- Get the repo via `UICommands.Module`; never `new` a `GitModule` in the form.
- Do history/ref loading on the background thread ([threading-model](../L1-conceptual/threading-model.md)); touch controls only after switching to the UI thread.
- After a state change elsewhere, rely on `RepoChangedNotifier` to refresh — don't force ad-hoc reloads.

**Next:** [revision-grid-flow](revision-grid-flow.md).
