<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Push Flow (L3)

**TL;DR:** `FormPush` pushes branches/tags to a remote. It builds a push command via
`Commands.Push` / `PushAll` / `PushTag` / `PushMultiple` and runs it through `FormRemoteProcess`
(remote-aware). It manages tracking branches, multiple-branch pushes, and suggests
force-with-lease instead of a plain force.

**Related:** [pull-flow](pull-flow.md) · [clone-flow](clone-flow.md) · [structured-commands](../L2-core-platform/structured-commands.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormPush.cs](../../../src/app/GitUI/CommandsDialogs/FormPush.cs) ·
`Commands.Push*` in [Commands.Arguments.cs](../../../src/app/GitCommands/Git/Commands.Arguments.cs)

## Why

Push writes to a shared remote, so it needs credential/progress handling and safety around forced
updates. Supporting single, all, tag, and multi-branch pushes from one dialog keeps common
workflows fast while steering users away from destructive plain `--force`.

## What / How

1. User selects remote, branch(es)/tags, and options (force, track, recurse submodules).
2. `FormPush` builds the appropriate command: `Commands.Push` (one), `PushAll`, `PushTag`, or
   `PushMultiple` (a branch matrix from the grid).
3. It runs via `FormRemoteProcess.ShowDialog(...)` (progress, auth, SSH).
4. Post-push, tracking may be set and the repo refreshed.

## Quirks

- Prefers **force-with-lease** over `--force` to avoid clobbering others' work.
- The multi-branch grid pushes several refs in one operation (`PushMultiple`).
- Remote-listing (to populate the UI) may run a lighter process dialog first.

## Hard rules

- Push accesses a remote — always use the remote process dialog.
- Prefer **force-with-lease**; never silently do a bare `--force`.
- Quote ref/remote names; rely on `RepoChangedNotifier` afterward.

**Next:** [pull-flow](pull-flow.md).
