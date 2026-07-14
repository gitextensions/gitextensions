<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# GitModule (L2)

**TL;DR:** `GitModule` is the per-repository facade — the single object that represents one open
git working directory and exposes methods to read state and run git. It implements the public
`IGitModule` interface. There is **one instance per repository and per submodule**. Most code
reaches git through a `GitModule` method rather than calling `Executable` directly.

**Related:** [git-command-execution](git-command-execution.md) (the process layer it uses) · [structured-commands](structured-commands.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

**Key files:** [GitModule.cs](../../../src/app/GitCommands/Git/GitModule.cs) ·
[IGitModule.cs](../../../src/app/GitExtensions.Extensibility/Git/IGitModule.cs) (public contract) ·
[GitModule.GitCommandCache](../../../src/app/GitCommands/Git/CommandCache.cs)

## Why

Git operations need a consistent context: the working directory, the git executable, encoding,
caching, and submodule awareness. `GitModule` centralises all of that so callers don't repeat
it. Because it implements `IGitModule` (in the versioned `Extensibility` assembly), plugins and
UI code depend on the **interface**, keeping the concrete engine swappable and testable.

## What (responsibilities)

`GitModule` groups its (large) API into areas:

- **Identity & validation** — `WorkingDir`, `WorkingDirGitDir`, `IsValidGitWorkingDir()`,
  `IsBareRepository()`, `IsDetachedHead()`, `GetCurrentCheckout()`, `GetSelectedBranch()`.
- **Revisions & refs** — `RevParse()`, `TryResolvePartialCommitId()`, ref parsing (see the
  `RefRegex`/`ShaRegex` generated regexes at the top of the class), tags.
- **Remotes** — `AddRemote()`, `RemoveRemote()`, `RenameRemote()`, `GetCurrentRemote()`.
- **Settings passthrough** — `GetSetting()` / `GetEffectiveSetting()` bridge to the git-config
  side of the [settings system](settings-system.md).
- **State changes** — e.g. `ResetAllChanges()`; most mutating operations flow through
  [structured commands](structured-commands.md) run by the UI.
- **Submodules** — `GetSubmoduleFullPath()`, plus helpers in `Submodules/` and `SubmoduleHelpers`.
- **Paths & encoding** — `GetPathForGitExecution()` (to git-style paths), `GetWindowsPath()`,
  `CommitEncoding`; static `GitModule.SystemEncoding` / default UTF-8-no-BOM.

### Execution & caching

- `GitModule` holds an `IGitExecutor` (from `IGitExecutorProvider`, injected) and delegates to
  the [ExecutableExtensions](git-command-execution.md) helpers.
- `GitModule.GitCommandCache` (a static `CommandCache`) caches read-only command output.
- The class is a `sealed partial class`; helpers are split across `Git/` (e.g. parsers,
  `SubmoduleHelpers`, `RevisionReader`).

## How (typical usage)

```
IGitModule module = uiCommands.Module;          // provided by GitUICommands
ObjectId head = module.GetCurrentCheckout();    // read state
string branch = module.GetSelectedBranch();
bool ok = module.ResetAllChanges(clean: false); // mutate
```

- UI code gets the current `IGitModule` from `GitUICommands.Module`.
- New repositories/submodules produce **new** `GitModule` instances (don't reuse across repos).
- To add an operation: prefer a method on `GitModule`/`Commands` returning an `IGitCommand`, and
  cover it with a unit test using a substituted `IExecutable`.

## Hard rules

- **ALWAYS** interact with git through `IGitModule` (interface), not a bare `Executable`.
- One `GitModule` == one repo. **NEVER** share an instance across working directories/submodules.
- Prefer NUL-delimited (`-z`) git output when parsing (see [git-command-execution](git-command-execution.md)).
- Adding members to `IGitModule` changes the **plugin interface version** — note it in the commit message.

**Next:** [settings-system](settings-system.md) for how `GetSetting`/config resolution works.
