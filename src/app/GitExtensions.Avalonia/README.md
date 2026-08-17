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
In Flatpak, external terminals and host Gource executables are reported as unavailable:
`--filesystem=host` exposes their files but does not make arbitrary host programs executable
inside the sandbox. Git and OpenSSH are bundled for repository and SSH operations.
The manifest forwards the host SSH agent. HTTPS authentication uses Git's configured
credential-helper protocol, but a host helper executable is not assumed to be runnable inside
the sandbox; Secret Service/keyring integration requires a compatible bundled helper and
explicit packaging support.
On Linux, file and folder dialogs use the XDG FileChooser portal. Opening a file or URI and
showing a path in the file manager use the XDG OpenURI portal. If the desktop portal or its
backend is unavailable, Git Extensions reports the failure and does not fall back to an
unconfined toolkit dialog or invoke `xdg-open` directly.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later
- [Git](https://git-scm.com/) available on `PATH`
- On Linux, `setsid` from util-linux for reliable process-tree cleanup (included by standard
  desktop distributions; descendant traversal remains available when it is absent)
- On Linux, `xdg-desktop-portal` and a desktop-appropriate backend such as
  `xdg-desktop-portal-gtk`, `xdg-desktop-portal-gnome`, or `xdg-desktop-portal-kde`
- On minimal Linux installations, the desktop libraries required by Avalonia:
  `libice6`, `libsm6`, `libfontconfig1`, `libwayland-client0`, `libxkbcommon0`, and `libegl1`
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

The shared engine projects build their cross-platform flavor only when the `BuildAvalonia`
MSBuild property is set (or when building on a non-Windows system), so that the existing
Windows Forms solution and its pipelines are never affected by the Avalonia port. The
property is applied automatically: building `GitExtensions.Avalonia.slnx` (the separate
solution in the repository root containing only the Avalonia application and the projects
it uses) sets it through the solution name, and building or running the Avalonia projects
directly picks it up from the `Directory.Build.rsp` next to each of them. Only when building
a shared engine project itself for `net10.0` on Windows (for example
`dotnet build src/app/GitCommands -f net10.0`) does `-p:BuildAvalonia=true` still need to be
passed explicitly. IDE design-time builds do not read `.rsp` files, so on Windows open
`GitExtensions.Avalonia.slnx` rather than an individual Avalonia project.

### AXAML previewer

The VS Code Avalonia previewer currently requires the classic solution format and design-time
plugin references. Launch its dedicated workspace from PowerShell so both properties are
available to the editor process. Exit every running VS Code process first; a new window owned
by an existing process does not inherit these variables.

If the Windows Forms solution was built most recently, first restore the Avalonia project graph.
The two build modes share intermediate restore files, and a stale Windows Forms evaluation can
leave the Avalonia status item at **loading** or make a valid preview time out:

```powershell
dotnet build eng/avalonia/GitExtensions.Avalonia.sln `
  -p:BuildAvalonia=true -p:BuildAvaloniaDesigner=true
```

```powershell
$env:BuildAvalonia = "true"
$env:BuildAvaloniaDesigner = "true"
code --new-window eng/avalonia/GitExtensions.Avalonia.code-workspace
Remove-Item Env:BuildAvalonia
Remove-Item Env:BuildAvaloniaDesigner
```

On Linux or macOS, launch the same preview workspace from a shell:

```console
BuildAvalonia=true BuildAvaloniaDesigner=true \
  code --new-window eng/avalonia/GitExtensions.Avalonia.code-workspace
```

Open an `.axaml` file and choose **Show Preview** from the Avalonia extension. The preview
workspace uses `eng/avalonia/GitExtensions.Avalonia.sln`, because the Avalonia language server
does not currently consume the repository's `.slnx` solution.

The account **Sign In** item is unrelated to project evaluation and is not required for the
previewer. If **Show Preview** is unavailable or the status remains at **loading**, verify that
VS Code was started as a fresh process with both variables above and repeat the explicit
Avalonia build before investigating the view itself.

`BuildAvaloniaDesigner` changes only the design-time dependency graph. Normal application and
solution builds continue to place bundled plugins below `Plugins` for MEF discovery without
referencing them from the entry assembly.

The Avalonia application builds on all three operating systems with the commands above;
the git submodules are not required for it. Building the complete solution including the
Windows Forms application additionally requires the submodules
(`git submodule update --init --recursive`) and, for running it, Windows.

## Running

```
dotnet run --project src/app/GitExtensions.Avalonia -- browse /path/to/repository
```

### Maintainer testing builds

Every successful merge to `master` in the development fork creates an
[Avalonia maintainer prerelease](https://github.com/begota98/gitextensions/releases) with a
Windows x64 portable archive, an offline Linux x64 Flatpak bundle, and macOS application
archives for Intel and Apple Silicon. These are unsigned development snapshots intended for
maintainer testing and issue reports, not production releases. The release title and notes
identify the exact source commit, and `SHA256SUMS.txt` covers every attached package.

Install the Flatpak bundle with:

```console
flatpak install --user ./GitExtensions-Avalonia-*-linux-x64.flatpak
```

The bundle has no automatic update channel; download and install a newer maintainer build to
update it. Windows may display a SmartScreen warning for the unsigned archive. macOS packages
are not Developer ID signed or notarized, so Gatekeeper may require the tester to approve the
downloaded application explicitly.

Some distribution-built .NET SDKs report a distribution-specific runtime identifier. For
example, Arch Linux packages can report `arch-x64`, while native NuGet assets use the portable
Linux identifiers. If restore, build, or launch reports a missing runtime/native asset, select
the matching portable RID explicitly:

```console
dotnet build src/app/GitExtensions.Avalonia/GitExtensions.Avalonia.csproj -r linux-x64
dotnet run --project src/app/GitExtensions.Avalonia -r linux-x64 -- browse /path/to/repository
```

Use `linux-arm64` on ARM64. An ordinary Microsoft-provided SDK already reports a portable RID,
so the shorter commands above remain the default. The Flatpak build selects its release RID
explicitly as part of publishing.

The command-line arguments follow the Windows Forms application: the first argument is the
command (for example `browse`), the second is the repository path. Without arguments the
application starts without an opened repository. The standalone `--` separates `dotnet run`
options from application arguments; `--browse` is not a command. If the explicit MSBuild
property is used on Windows, it must remain before that separator:

```
dotnet run --project src/app/GitExtensions.Avalonia -p:BuildAvalonia=true -- browse .
```

## Debugging

Build and launch the application with debug symbols from the repository root:

```console
dotnet run --project src/app/GitExtensions.Avalonia \
  --configuration Debug -- browse .
```

On Windows the explicit-property form is:

```powershell
dotnet run --project src/app/GitExtensions.Avalonia `
  -p:BuildAvalonia=true --configuration Debug -- browse .
```

For breakpoint debugging, open `GitExtensions.Avalonia.slnx` in Visual Studio or VS Code,
select `GitExtensions.Avalonia` as the startup project, and use `browse .` as the application
arguments. The equivalent executable launched by the debugger is:

```console
dotnet artifacts/Debug/bin/GitExtensions.Avalonia/net10.0/GitExtensions.Avalonia.dll browse .
```

On Linux, a session with `WAYLAND_DISPLAY` set uses Avalonia's native Wayland backend.
An X11-only session, or an explicit launch with `WAYLAND_DISPLAY` unset, uses the X11
backend. Both paths are exercised by the development runtime harness under `eng/avalonia`.
The Flatpak release candidate is Wayland-only; it does not expose an X11 socket to the application.
The portal backend must provide `org.freedesktop.portal.FileChooser` and
`org.freedesktop.portal.OpenURI`. Contributors can exercise both the available and unavailable
paths on Linux after a Debug build:

```console
bash eng/avalonia/portal-conformance.sh eng/avalonia/parity-evidence/portal
```

The harness uses a temporary XDG profile and throwaway repository outside the working tree.
The release-shaped Flatpak manifest and repeatable smoke/action commands are documented in
`eng/avalonia/flatpak/README.md`. Every Flatpak permission example uses
`--filesystem=host`.

The Avalonia application uses the shared MEF plugin infrastructure, but scans the separate
`UserPlugins.Avalonia` directory under the Git Extensions local application-data folder.
It does not load the Windows Forms `UserPlugins` directory because existing plugins can carry
Windows-only UI and `System.Drawing` dependencies. Portable user plugins can use the separate
directory without affecting the Windows Forms installation. In the Flatpak this resolves to
`$XDG_DATA_HOME/GitExtensions/UserPlugins.Avalonia`, normally
`~/.var/app/com.github.gitextensions.GitExtensions.Avalonia/data/GitExtensions/UserPlugins.Avalonia`
for the release candidate. The application creates and checks this directory before MEF
initialization. If it cannot be created or enumerated, startup continues with bundled plugins
only and writes a diagnostic; it never falls back to the Windows Forms or host plugin directory.

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
