<!-- L3 FLOW. Agentic doc: TL;DR + key files at top, code pointers not code, ~80 lines. -->
# Clone Flow (L3)

**TL;DR:** `FormClone` clones a remote repository. It builds a `git clone` `ArgumentString` via
`Commands.Clone(...)` and runs it through `FormRemoteProcess` (a remote-aware process dialog).
It validates paths, supports shallow/single-branch clones and submodule init, and can open the
cloned repo afterward.

**Related:** [push-flow](push-flow.md) · [pull-flow](pull-flow.md) · [structured-commands](../L2-core-platform/structured-commands.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [FormClone.cs](../../../src/app/GitUI/CommandsDialogs/FormClone.cs) ·
`Commands.Clone` in [Commands.Arguments.cs](../../../src/app/GitCommands/Git/Commands.Arguments.cs)

## Why

Cloning is the entry point to a repository and touches a remote, so it needs a remote-capable
process dialog (auth prompts, progress), path validation, and options for large repos (shallow /
single-branch) and submodules.

## What / How

1. User enters source URL, destination path, and options (branch, depth, single-branch,
   init submodules, central/bare).
2. `FormClone` builds the command with `Commands.Clone(url, destPath, getPathForGitExecution,
   centralRepo, initSubmodules, branch, depth, isSingleBranch)` → an `ArgumentString`.
3. It runs via `FormRemoteProcess.ShowDialog(...)` (remote-aware: shows progress, handles
   credentials/SSH).
4. On success it can store the SSH key and optionally open the cloned repo in a new `FormBrowse`.

## Quirks

- **Shallow** (`depth`) and **single-branch** options speed up large clones.
- Validates source/destination paths before running.
- Because it accesses a remote, the command's `AccessesRemote` is true → remote process dialog.

## Hard rules

- Clone accesses a remote — run it through the remote process dialog, not a plain one.
- Validate/quote user-provided URLs and paths.
- Don't assume submodules are initialised unless the user opted in.

**Next:** [push-flow](push-flow.md).
