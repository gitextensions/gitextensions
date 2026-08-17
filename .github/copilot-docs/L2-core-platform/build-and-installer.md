<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Build & Installer (L2)

**TL;DR:** The managed app builds with `dotnet build` against `GitExtensions.slnx`; shared build
settings live in `Directory.Build.props` (C# 14, nullable, WinForms, central package management).
The native C++ pieces build via `src/native/build.proj`. The Windows installer is authored in
**WiX** under `Setup/installer/`. Publishing (incl. plugin detection) is driven by `eng/Publish.targets`.

**Related:** [project-map](../L1-conceptual/project-map.md) · [shell-integration](shell-integration.md) · [ci-workflows](ci-workflows.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Git Extensions ships a managed WinForms app **plus** native COM components and a Windows
installer. Centralising build settings and separating the native build keeps the whole thing
reproducible and lets CI gate every PR consistently.

## What

### Managed build

- **`GitExtensions.slnx`** — the solution (modern `.slnx` format). Build: `dotnet build`.
- **`Directory.Build.props`** — repo-wide MSBuild defaults: `LangVersion 14.0`, `Nullable enable`,
  `UseWindowsForms`, code-analysis ruleset, StyleCop. [Directory.Build.props](../../../Directory.Build.props).
- **`Directory.Packages.props`** — central package management (versions pinned in one place).
  [Directory.Packages.props](../../../Directory.Packages.props).
- **`global.json`** — pins the .NET SDK. [global.json](../../../global.json).

### Native build

- **`src/native/build.proj`** — uses VSwhere + MSBuild to build the C++ shell extension and
  `GitExtSshAskPass` for Win32 and x64 (needs VC++/ATL). [build.proj](../../../src/native/build.proj).
  In VS Code this is the `build-vc` task.

### Installer (WiX, `Setup/installer/`)

- **`Config.wxi`** — install paths/features/properties. [Config.wxi](../../../Setup/installer/Config.wxi).
- **`RegisterShellExtension.wxi`** — registers the native shell-extension DLL.
  [RegisterShellExtension.wxi](../../../Setup/installer/RegisterShellExtension.wxi).
- **`UI/WixUI.wxs`** — installer dialogs/flow. [WixUI.wxs](../../../Setup/installer/UI/WixUI.wxs).

### Publish

- **`eng/Publish.targets`** — detects plugin assemblies (Roslyn code task) and publishes to
  `artifacts/`. [Publish.targets](../../../eng/Publish.targets).

## How (common commands)

```
dotnet build                              # managed build (default VS Code "build" task)
dotnet build .\src\native\build.proj      # native build ("build-vc" task)
dotnet test                               # run tests (see testing-guide)
```

## Hard rules

- Files **MUST** use CRLF and satisfy StyleCop / [.editorconfig](../../../.editorconfig) — the build enforces analyzers.
- Package versions go in `Directory.Packages.props` (central management), not per-project.
- Changes under `GitExtensions.Extensibility/` bump the **plugin interface version** — note it in the commit message.
- The native build requires the VC++ toolset (ATL) for x86/x64.

**Next:** [ci-workflows](ci-workflows.md) for what gates a PR.
