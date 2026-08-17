<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Conflict Resolution Flow (L3)

**TL;DR:** `FormResolveConflicts` (with `MergeConflictHandler`) walks the user through conflicted
files. It launches the configured mergetool via `Module.RunMergeTool(...)` for text conflicts and
uses `git add` / `git rm` to record resolutions. It handles 2-way/3-way merges, binary files, and
prompts to commit once everything is resolved.

**Related:** [merge-flow](merge-flow.md) · [rebase-flow](rebase-flow.md) · [pull-flow](pull-flow.md) · [commit-flow](commit-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormResolveConflicts.cs](../../../src/app/GitUI/CommandsDialogs/FormResolveConflicts.cs) ·
[MergeConflictHandler.cs](../../../src/app/GitUI/CommandsDialogs/MergeConflictHandler.cs)

## Why

Conflicts are the highest-stakes moment in git workflows — the point where work is most easily
lost. A guided flow (with base/local/remote context and the user's mergetool) makes resolution
safe and repeatable across merge, rebase, pull, and stash-apply.

## What / How

1. After a conflicting merge/rebase/pull, `MergeConflictHandler` detects the conflict and offers to
   open `FormResolveConflicts`.
2. The form lists conflicted files. For each:
   - **Text** — launch the configured mergetool via `Module.RunMergeTool(...)` (base/local/remote).
   - **Binary / choose-side** — pick ours/theirs; recorded with `git add` / `git rm`.
3. Once all files are resolved, it prompts to **commit** the merge (see [commit-flow](commit-flow.md)).

## Quirks

- Supports **2-way and 3-way** merges; shows base/local/remote where available.
- **Binary** files can't be text-merged — the flow offers side selection.
- The repo stays "in the middle of a merge" until resolved+committed; that state gates other actions.

## Hard rules

- Never mark a file resolved without recording it (`git add`/`git rm`); half-resolved state is dangerous.
- Respect the user's configured mergetool; don't hardcode a diff/merge tool.
- Only offer to commit once **all** conflicts are resolved.

**Next:** [submodule-flow](submodule-flow.md).
