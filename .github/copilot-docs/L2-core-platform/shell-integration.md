<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Shell Integration (Native) (L2)

**TL;DR:** A native C++ **Windows Explorer shell extension** adds right-click "git" actions in
Explorer, launching `GitExtensions.exe` with a verb (browse, commit, push…). It's a separate
COM DLL (`CGitExtensionsShellEx`) built by `src/native/build.proj` and registered by the
installer. A companion native tool, `GitExtSshAskPass`, handles SSH passphrase prompts.

**Related:** [project-map](../L1-conceptual/project-map.md) · [build-and-installer](build-and-installer.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Users expect to right-click a folder in Explorer and run git actions without first opening the
app. Windows shell extensions must be **native COM** components (not .NET) for reliability inside
`explorer.exe`, so this piece lives in C++ and simply shells out to the managed app.

## What (`src/native/`)

- **`CGitExtensionsShellEx`** — the shell extension COM class implementing `IShellExtInit` and
  `IContextMenu3`; provides the Explorer context-menu entries and the DLL entry point (`DllMain`).
  [ShellEx.cpp](../../../src/native/GitExtensionsShellEx/ShellEx.cpp).
- **`GitExCommands`** (enum) — the menu commands (browse, clone, commit, push, …).
  [GitExtensionsShellEx.h](../../../src/native/GitExtensionsShellEx/GitExtensionsShellEx.h).
- **`GitExtSshAskPass`** — standalone native tool used as git's `SSH_ASKPASS` to prompt for
  credentials/passphrases. [GitExtSshAskPass/](../../../src/native/GitExtSshAskPass/).
- **Native build** — [build.proj](../../../src/native/build.proj) uses VSwhere + MSBuild to build
  Win32 and x64 configurations (needs VC++/ATL).

## How

- The shell extension launches the managed app with a command-line verb (the same verbs the app's
  command-line handler understands; see `FormCommandlineHelp`). It runs in a **separate process**
  from the managed app.
- Registration/unregistration of the COM DLL is done by the installer
  ([RegisterShellExtension.wxi](../../../Setup/installer/RegisterShellExtension.wxi)); see
  [build-and-installer](build-and-installer.md).

## Hard rules

- Shell-extension code is **native C++/COM** — keep it minimal; do real work in the managed app.
- Don't block Explorer: the extension should launch the app and return quickly.
- Building this requires the VC++ toolset (ATL) for x86/x64 — see the native build target.
- Registration is installer-owned; don't hand-register the DLL in normal dev flows.

**Next:** [build-and-installer](build-and-installer.md).
