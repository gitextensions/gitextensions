# Git Extensions Avalonia

A cross-platform user interface for Git Extensions built with [Avalonia](https://avaloniaui.net/).
It runs on Windows, Linux, and macOS from a single code base and coexists with the classic
Windows Forms application in this repository: both share the same underlying engine
(`GitCommands`, `GitExtensions.Extensibility`, and related projects), and the Windows Forms
application remains fully intact and buildable.

## Status

Early development. The application currently provides a read-only repository browser:
commit history with the commit graph and branch/tag labels, the file list of a selected
revision, and a colored diff viewer. Write operations (commit, checkout, push, pull) and
the remaining dialogs are being ported incrementally.

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

The tests run headlessly (no display required) on all three operating systems.

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
- Follow the repository coding guidelines (`.github/copilot-instructions.md`) and the
  Windows Forms control naming conventions (`.github/ui_design_guidelines.md`).
- Keep this document up to date when build or runtime requirements change.
