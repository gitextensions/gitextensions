<!-- L1 CONCEPTUAL. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Git Extensions Overview (L1)

**TL;DR:** Git Extensions is a standalone **Windows GUI for Git** plus a Windows Explorer shell
extension and a Visual Studio integration. It doesn't reimplement git — it wraps the real `git`
executable behind an approachable UI for history browsing, committing, branching, merging,
diffing, and more. Written in C# (WinForms, .NET 10).

**Related:** [architecture-overview](architecture-overview.md) · [project-map](project-map.md) · [glossary](glossary.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why it exists

Git's command line is powerful but has a steep learning curve, especially on Windows. Git
Extensions makes common and advanced git workflows **visual and discoverable** — you can see the
commit graph, stage hunks, resolve conflicts, and manage submodules without memorising commands,
while still generating (and showing) the exact git commands it runs.

## What it does

- **Repository browsing** — the commit graph (`RevisionGrid`), branch/tag/stash tree, commit
  details, and file trees. Main window: `FormBrowse`.
- **Working with changes** — stage/commit (`FormCommit`), stash, resolve conflicts, view diffs
  and blame.
- **Branches & remotes** — checkout, create/rename/delete branches, clone, push, pull, fetch,
  rebase, merge, cherry-pick.
- **Submodules & worktrees** — manage nested repositories and linked working trees.
- **Extensibility** — a [plugin system](../L2-core-platform/plugin-system.md) (background fetch,
  GitHub/Bitbucket hosting, build-server integration, statistics, etc.).
- **OS integration** — a native [Explorer shell extension](../L2-core-platform/shell-integration.md)
  (right-click → git actions) and a Visual Studio extension.
- **Personalisation** — [themes](../L2-core-platform/theming-system.md),
  [hotkeys](../L2-core-platform/hotkey-system.md), and
  [user scripts](../L2-core-platform/scripts-engine.md).

## Who it's for

Windows developers who want a full-featured git GUI — from newcomers who prefer visual workflows
to power users who want fast access to advanced operations (interactive rebase, sparse checkout,
bisect) with the underlying commands always visible.

## Non-goals (what it is not)

- **Not a git reimplementation** — it always shells out to a real `git` binary.
- **Not cross-platform first** — it targets Windows (WinForms, Explorer/VS integration).
- **Not a text editor/IDE** — it focuses on git operations, not general code editing (though it
  embeds a diff/blame viewer based on `ICSharpCode.TextEditor`).

## How it's built (one paragraph)

The app is layered: a small executable (`GitExtensions`) bootstraps DI and launches the WinForms
UI (`GitUI`), which drives the git engine (`GitCommands`) through the versioned contract layer
(`GitExtensions.Extensibility`). See [architecture-overview](architecture-overview.md) for the
tiers and [project-map](project-map.md) for where each piece lives.

**Next:** [glossary](glossary.md) for terminology, then [architecture-overview](architecture-overview.md).
