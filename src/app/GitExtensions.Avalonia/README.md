# Git Extensions Avalonia

A cross-platform user interface for Git Extensions built with [Avalonia](https://avaloniaui.net/).
It runs on Windows, Linux, and macOS from a single code base and coexists with the classic
Windows Forms application in this repository: both share the same underlying engine
(`GitCommands`, `GitExtensions.Extensibility`, and related projects), and the Windows Forms
application remains fully intact and buildable.

## Status

Early development. The application currently provides a repository browser with commit
history, graph and branch/tag labels, the files in a selected revision, and a colored diff
viewer. The Repository menu can refresh the view, and Commands > Fetch all runs Git in the
ported process dialog and reloads the repository afterwards. Commands > Checkout branch can
switch between existing local branches, with merge/reset handling for local changes. Remote
checkout and auto-stash are not yet available. Commands > Create branch can create a branch
at the selected revision, optionally check it out, and supports orphan branches in empty
repositories. Right-clicking a revision provides the same checkout-branch and create-branch
actions for that commit. Commands > Commit opens the commit dialog, which displays staged
and unstaged files with a selected-file diff, stages or unstages selected files or all files,
edits the commit message, and creates normal commits through the process dialog. Amend,
fixup/squash, commit templates/history, and signing options are not yet available. Commit
and push opens the reduced push dialog, which pushes the current branch to the same branch
on a selected remote and supports force-with-lease. URL, tag, multiple-branch, and remote
management push options are being ported incrementally. Commands > Pull/Fetch opens the
reduced pull dialog, which supports merge pulls, rebase pulls, and fetch-only from a selected
configured remote. URL pulls, auto-stash, tag/prune/unshallow options, scripts, submodule
follow-up, and conflict recovery remain deferred with the remaining dialogs. Browse hotkeys
use the same persisted `Keys` values and XML settings as the Windows Forms application: F5
refreshes, the currently implemented configurable Browse commands are dispatched through
their upstream command IDs, and Escape closes dialogs without closing the repository browser.
On Linux with `setsid` available, commands shown in the process dialog run in an isolated
process group so cancelling the dialog also terminates descendant processes, including
children that have been re-parented.
On macOS, the build compiles a small native launcher that creates the process group before
executing Git. The launcher preserves the command PID and redirected streams so cancellation
uses the same process-tree behavior as Linux.
Tools > Git bash opens an external terminal in the repository directory. Linux uses the
`TERMINAL` environment variable when set, then `xdg-terminal-exec` to honor the desktop's
configured default terminal, followed by `x-terminal-emulator`, GNOME Terminal, Konsole, or
xterm; Windows prefers Windows Terminal and falls back to Command Prompt; macOS opens
Terminal.app. Linux launch removes inherited Snap-specific GTK/GIO overrides so a system
terminal can start correctly when Git Extensions was launched from a Snap-hosted IDE.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later
- [Git](https://git-scm.com/) available on `PATH`
- On Linux, `setsid` from util-linux for reliable process-tree cleanup (included by standard
  desktop distributions; descendant traversal remains available when it is absent)
- On macOS, the Xcode Command Line Tools (`cc`) to build the process-group launcher
- A desktop environment:
  - Windows 10 or later
  - Linux with an X11 or Wayland session (standard desktop distributions work out of the box)
  - macOS 10.15 or later

## Building

```
git clone https://github.com/gitextensions/gitextensions.git
cd gitextensions
dotnet build src/app/GitExtensions.Avalonia/GitExtensions.Avalonia.csproj
```

On Windows, pass `-p:BuildAvalonia=true` to the build (and run) commands: the shared engine
projects build their cross-platform flavor only when this property is set (or when building
on a non-Windows system), so that the existing Windows Forms solution and its pipelines are
never affected by the Avalonia port. `GitExtensions.Avalonia.slnx` in the repository root is
a separate solution containing only the Avalonia application and the projects it uses.

The Avalonia application builds on all three operating systems with the commands above;
the git submodules are not required for it. Building the complete solution including the
Windows Forms application additionally requires the submodules
(`git submodule update --init --recursive`) and, for running it, Windows.

## Running

```
dotnet run --project src/app/GitExtensions.Avalonia -- browse /path/to/repository
```

The command-line arguments follow the Windows Forms application: the first argument is the
command (for example `browse`), the second is the repository path. Without arguments the
application starts without an opened repository.

## Testing

```
dotnet test tests/app/UnitTests/GitUI.Avalonia.Tests/GitUI.Avalonia.Tests.csproj
```

The tests run headlessly (no display required) on all three operating systems. This
includes golden-image tests that render known commit topologies with the graph renderer
and compare the pixels against the images in the `GoldenImages` folder of the test
project. After an intended rendering change, regenerate the images by running the tests
once with the environment variable `GITEXT_UPDATE_GOLDEN_IMAGES=1`, review the new
images, and commit them.

## Project layout

The UI lives in `src/app/GitUI.Avalonia`, which mirrors the file structure of the Windows
Forms project `src/app/GitUI`: each ported form or control sits at the same relative path
and keeps the same class and member names, with an `.axaml` file taking the role of the
`.Designer.cs`. Code is written in the same code-behind style used throughout Git
Extensions - named control fields and event handlers, no view models. Framework-neutral
sources are compiled directly from `src/app/GitUI` as links. `src/app/GitExtensions.Shims.WinForms`
provides headless stand-ins for the few Windows Forms types referenced by the shared
engine projects.

## Notes for contributors

- Changes must not break the Windows Forms application; both user interfaces are built and
  shipped side by side.
- New dialogs are scaffolded from their Windows Forms layout:
  `dotnet run --project eng/tools/WinFormsToAxaml -- path/to/Form.Designer.cs` prints an
  `.axaml` starting point (`-o <file>` writes it) with the original control names kept and
  `TODO:WinForms` comments for everything that needs a manual decision.
- Follow the repository coding guidelines (`.github/copilot-instructions.md`) and the
  Windows Forms control naming conventions (`.github/ui_design_guidelines.md`).
- Keep this document up to date when build or runtime requirements change.
