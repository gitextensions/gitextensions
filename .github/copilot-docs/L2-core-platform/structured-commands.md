<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Structured Commands (L2)

**TL;DR:** Instead of hand-writing git command strings, Git Extensions builds them with
`ArgumentBuilder`/`GitArgumentBuilder` and exposes them as `IGitCommand` records via the static
`Commands` factory. Each `IGitCommand` declares two facts — `AccessesRemote` and
`ChangesRepoState` — so the UI can pick the right process dialog and fire repo-changed events.
Run them with `GitUICommands.StartCommandLineProcessDialog`.

**Related:** [git-command-execution](git-command-execution.md) (how they run) · [commit-flow](../L3-flows/commit-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Concatenating strings is error-prone (quoting, conditional flags) and loses metadata. The
structured approach gives three wins: **safe argument building** (conditional/one-of options),
**intent metadata** (does it hit a remote? does it change state?), and a **uniform run path**
so that every state-changing command consistently notifies the UI to refresh.

## What

### ArgumentBuilder / GitArgumentBuilder

`ArgumentBuilder` uses collection-initialiser syntax to compose arguments; `null`/whitespace is
skipped, and conditional/either-or forms are supported:

```
new GitArgumentBuilder("checkout")
{
    { localChanges is LocalChangesAction.Merge, "--merge" },  // add only if condition
    branchName.QuoteNE()                                       // unconditional (quoted)
};
```

Contract & docs: [ArgumentBuilder.cs](../../../src/app/GitExtensions.Extensibility/ArgumentBuilder.cs).
`ArgumentString` is the built value passed to the executable.

### IGitCommand + the Commands factory

- **`IGitCommand`** — exposes `Arguments`, `AccessesRemote`, `ChangesRepoState`.
  Contract: [IGitCommand.cs](../../../src/app/GitExtensions.Extensibility/Git/IGitCommand.cs).
- **`Commands`** — a `partial static` factory (`GitCommands.Git`) with one method per operation.
  Each returns a private `GitCommand : IGitCommand` carrying the two flags.
  - Main file: [Commands.cs](../../../src/app/GitCommands/Git/Commands.cs) (e.g. `CheckoutBranch`, `DeleteBranch`).
  - Argument-only variants: [Commands.Arguments.cs](../../../src/app/GitCommands/Git/Commands.Arguments.cs) (e.g. `Commit`).
  - Record definition: [Commands.GitCommand.cs](../../../src/app/GitCommands/Git/Commands.GitCommand.cs).

### Two patterns (know the difference)

| Pattern | Returns | Run via | Use when |
| --- | --- | --- | --- |
| Structured (preferred) | `IGitCommand` | `GitUICommands.StartCommandLineProcessDialog(owner, cmd)` | State-changing UI operations (checkout, delete branch, tag). |
| Argument-only (legacy) | `ArgumentString` | `FormProcess.ShowDialog(...)` or `Executable` extensions | Read commands or older call sites (e.g. `Commands.Commit`). |

`StartCommandLineProcessDialog` inspects `AccessesRemote` to choose a remote-capable process
dialog and fires `RepoChangedNotifier` when `ChangesRepoState` is true.
See [GitUICommands.cs](../../../src/app/GitUI/GitUICommands.cs) (`StartCommandLineProcessDialog`).

## How (add a new structured command)

1. Add a `public static IGitCommand MyThing(...)` to `Commands` (or a `Commands.*.cs` partial).
2. Build args with `GitArgumentBuilder`; set `accessesRemote` / `changesRepoState` correctly.
3. Call it from a dialog via `UICommands.StartCommandLineProcessDialog(this, Commands.MyThing(...))`.
4. Add a unit test (structured commands are easy to assert on `.Arguments`).

## Hard rules

- **ALWAYS** build git arguments with `ArgumentBuilder`/`GitArgumentBuilder` — never string concat.
- Set `AccessesRemote` and `ChangesRepoState` **accurately**; the UI relies on them for dialog
  selection and repo-refresh notifications.
- Quote user-provided values (`.Quote()` / `.QuoteNE()`).
- New interactive git operations SHOULD go through `IGitModule`/`Commands` and have unit tests.

**Next:** [commit-flow](../L3-flows/commit-flow.md) to see the pattern end-to-end.
