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
actions for that commit. The first commit-dialog increment can display staged and unstaged
files with a selected-file diff; staging and committing are not enabled yet, and its browse
entry point arrives with the functional commit increment. Push and the remaining dialogs
are being ported incrementally.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later
- [Git](https://git-scm.com/) available on `PATH`
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
