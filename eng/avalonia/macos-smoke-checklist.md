# macOS runtime smoke checklist

This checklist is the reproducible macOS leg of the cross-platform parity harness. Run it on
Apple Silicon, the supported macOS architecture, keeping evidence below
`eng/avalonia/parity-evidence/P0.6/macos-arm64/`.

1. Install the .NET 10 SDK, Git, and the Xcode Command Line Tools. Confirm `dotnet --info`,
   `git --version`, `cc --version`, `uname -m`, and `sw_vers`.
2. Clone the reviewed commit into a case-sensitive APFS volume, initialize submodules, and
   build `GitExtensions.Avalonia.slnx` with zero warnings and errors.
3. Run the focused P0.6 tests:

   ```sh
   dotnet test tests/app/UnitTests/GitUI.Avalonia.Tests/GitUI.Avalonia.Tests.csproj \
     -p:BuildAvalonia=true \
     --filter TestCategory=P0_6 \
     -m:1 \
     --no-restore \
     -v:minimal
   ```

4. Create a throwaway Git repository outside the checkout and isolated XDG-style settings
   directories under `mktemp -d`. Do not use the operator's real Git Extensions settings.
5. Launch the Debug app with `GITEXTENSIONS_DEBUG_FAIL_FAST=1` and the original command shape:

   ```sh
   artifacts/Debug/bin/GitExtensions.Avalonia/net10.0/GitExtensions.Avalonia \
     browse /path/to/throwaway/repository
   ```

6. Confirm the process remains alive with no exception/error dialog or failure signature in
   captured stdout/stderr. Exercise modal ownership, centring, resize, native menus, clipboard,
   drag-and-drop, file and folder pickers, default-browser opening, and terminal launch.
7. Move the window between Retina and non-Retina monitors when available. Confirm scale changes,
   fonts, icons, popups, and tooltips remain correct.
8. Capture the window with `screencapture`, record the exact commit, architecture, macOS
   version, scale, and evidence paths, then terminate only the process tree started by the
   smoke.

# parity-scaffolding: this checklist is retained only until automated macOS parity gates replace it.
