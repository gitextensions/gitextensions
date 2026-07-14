<!-- L0 FOUNDATIONS PRIMER — ALWAYS LOADED. HARD CAP: 80 lines. No docs-index.
     Only concepts that benefit 90%+ of contributors. Specialised knowledge → L1+. -->
# Git Extensions Primer (L0 — always loaded)

**What it is:** Git Extensions is a standalone **Windows GUI for Git** (WinForms, C# 14 / .NET 10)
that also integrates with Windows Explorer and Visual Studio. It shells out to the real `git`
executable and presents repositories, history, diffs, commits, and branch operations.

## Architecture anchor (how the tiers connect)

```
GitExtensions.Extensibility  → interfaces + primitives (IGitCommand, IExecutable, ArgumentBuilder)
        ▲
GitCommands                  → git execution + models (GitModule, Executable, Commands, Settings)
        ▲
GitUI                        → WinForms UI (FormBrowse main window, dialogs, RevisionGrid)
        ▲
GitExtensions (exe)          → app entry point / bootstrap
```

Plus: **plugins** (`src/plugins/*`, via `GitUIPluginInterfaces`) and **native** code
(`src/native/GitExtensionsShellEx` = Explorer shell extension).

**Typical action flow:** user acts in a `Form` → `GitUICommands` orchestrates → `Commands`
builds structured args (`IGitCommand`) → `GitModule` / `Executable` runs `git` → output is
parsed into models → UI updates and `RepoChangedNotifier` fires.

## Glossary (core terms)

- **GitModule** — represents one git repository; the main entry for running git commands and reading state.
- **Executable / IExecutable** — abstraction over launching a process (usually `git`); returns `ExecutionResult`.
- **Commands** (`GitCommands.Git`) — factory of structured `IGitCommand` records that declare whether a command hits a remote and whether it changes repo state.
- **GitUICommands / IGitUICommands** — runs commands *with UI feedback* (process dialogs) and raises repo-changed events.
- **ArgumentBuilder** — safe builder for git command-line arguments (in `GitExtensions.Extensibility`).
- **FormBrowse** — the main repository window (history, toolbars, panels).
- **RevisionGrid** — the commit graph control that renders history.
- **Revision** — a commit (plus Git Extensions metadata); **ref** = branch/tag/remote pointer.
- **Module vs submodule** — the active repo vs nested git repos it contains.
- **Plugin** — optional feature loaded via `GitUIPluginInterfaces` / `GitExtensions.Extensibility.Plugins`.

## Ownership map (topic → where)

| Topic | Location |
| --- | --- |
| Git execution / models | `src/app/GitCommands/` (esp. `Git/`) |
| Plugin interfaces (versioned) | `src/app/GitExtensions.Extensibility/` |
| UI forms & controls | `src/app/GitUI/` |
| App bootstrap | `src/app/GitExtensions/` |
| Plugins | `src/plugins/` |
| Explorer shell / native | `src/native/` |
| Translations | `src/app/ResourceManager/`, `GitUI/Translation/English.xlf` |
| Build / installer / CI | `Setup/`, `eng/`, `.github/workflows/` |
| Tests | `tests/` |

## Hard rules (project-wide)

- **ALWAYS** run git operations through `IGitModule`; build args via `Commands` (never raw strings).
- Files **MUST** use CRLF endings and follow StyleCop + [.editorconfig](../../../.editorconfig).
- Changing UI strings/controls **MUST** update `English.xlf` via `update-loc.cmd` (CI fails otherwise).
- Changes under `GitExtensions.Extensibility/` affect the **plugin interface version** — note in commit message.
- Tests: NUnit + `NSubstitute` + `AwesomeAssertions`; test names use snake_case suffix.

**Next:** for anything beyond this anchor, load [docs-index.md](../docs-index.md) and follow a reading chain.
