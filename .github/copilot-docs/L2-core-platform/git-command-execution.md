<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Git Command Execution (L2)

**TL;DR:** This is the pipeline that actually runs `git`. `GitModule` is the per-repo entry
point; it uses an `IExecutable` (`Executable`) to start a process (`IProcess`), and the
`ExecutableExtensions` helpers (`GetOutput` / `RunCommand` / `ExecuteAsync`) handle
input/output/encoding and return an `ExecutionResult`. Results can be cached via `CommandCache`.

**Related:** [structured-commands](structured-commands.md) (what args to run) · [git-module](git-module.md) · [L0 primer](../L0-foundations/gitextensions-primer.md) · [architecture-overview](../L1-conceptual/architecture-overview.md)

## Why

Git Extensions shells out to the real `git` binary rather than a library, so it needs a robust,
testable process layer: consistent working directory, correct text encoding, cancellation,
stdin/stdout redirection, logging, and optional result caching. The `IExecutable` abstraction
also makes execution **mockable** in tests (substitute a fake `IExecutable`/`IProcess`).

## What (the layers)

1. **`IExecutable`** — a launchable command (usually `git`). Exposes `WorkingDir`, `Command`,
   `PrefixArguments`, and a low-level `Start(...) : IProcess`.
   Contract: [IExecutable.cs](../../../src/app/GitExtensions.Extensibility/IExecutable.cs).
2. **`Executable`** — the concrete implementation. Resolves the git path (deferred via a
   `Func<string>`), sets up `ProcessStartInfo`, logging, and environment.
   [Executable.cs](../../../src/app/GitCommands/Git/Executable.cs).
3. **`IProcess`** — a running process with redirected streams and an exit code.
4. **`ExecutableExtensions`** — the high-level API most code uses instead of `Start`:
   - `GetOutput(args, …)` — run and return concatenated stdout as a string (sync wrapper over async).
   - `RunCommand(args, …)` — run when you only need success/failure (no output text).
   - `ExecuteAsync(args, …)` — run and return a full `ExecutionResult` (stdout + stderr + exit code).
   [ExecutableExtensions.cs](../../../src/app/GitCommands/Git/ExecutableExtensions.cs).
5. **`ExecutionResult`** — value type holding `Command`, `Arguments`, `StandardOutput`,
   `StandardError`, `ExitCode`, plus `ExitedSuccessfully` and `ThrowIfErrorExit()`.
   [ExecutionResult.cs](../../../src/app/GitExtensions.Extensibility/ExecutionResult.cs).

### Supporting pieces

- **`GitModule`** — per-repository facade; most git operations are methods here that call the
  extensions above. One instance per repo/submodule.
  [GitModule.cs](../../../src/app/GitCommands/Git/GitModule.cs).
- **`GitCommandRunner`** — coordinates running commands for a module.
  [GitCommandRunner.cs](../../../src/app/GitCommands/Git/GitCommandRunner.cs).
- **`GitExecutor` / `IGitExecutor` / `IGitExecutorProvider`** — provide the git `IExecutable`
  (path, prefix args) to the rest of the app. Resolved through DI.
- **`CommandCache`** (`GitModule.GitCommandCache`) — caches output of read-only commands to
  avoid re-running identical queries. [CommandCache.cs](../../../src/app/GitCommands/Git/CommandCache.cs).
- **Encoding** — git output is decoded with `GitModule.SystemEncoding` /
  `_defaultEncoding` (UTF-8 without BOM); ANSI escape codes can be stripped.

## How (typical call)

```
GitModule method  ──►  executable.GetOutput("status -z ...")  ──►  Executable.Start
                                                              ──►  IProcess (stdout/stderr)
                                                              ──►  ExecutionResult ──► parse
```

Read-only commands prefer `GetOutput`/`ExecuteAsync`; state-changing commands invoked from the
UI go through the structured path (see [structured-commands](structured-commands.md) and
`GitUICommands.StartCommandLineProcessDialog`).

## Hard rules

- **ALWAYS** run git through `IExecutable`/`GitModule`; never `Process.Start("git")` directly.
- Prefer the **`-z`** (NUL-delimited) form of git commands when parsing paths (avoids newline/space bugs).
- For UI-facing state changes, use the structured `IGitCommand` + `StartCommandLineProcessDialog`
  path so `RepoChangedNotifier` fires correctly.
- Honor `CancellationToken`; use `ExecutionResult.ThrowIfErrorExit()` rather than manual exit-code checks where possible.

**Next:** [structured-commands](structured-commands.md) — how argument sets are built and classified.
