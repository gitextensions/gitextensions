<!-- L1 CONCEPTUAL. Agentic doc: TL;DR at top, code pointers not code, ~100 lines. -->
# Project Map (L1)

**TL;DR:** Where everything lives. `src/app/` is the desktop app, `src/plugins/` are optional
features, `src/native/` is C++ Windows integration, `tests/` mirrors `src/`. Use this to route
a task to the right project before reading code.

**Related:** [architecture-overview](architecture-overview.md) · [L0 primer](../L0-foundations/gitextensions-primer.md) · master [docs-index](../docs-index.md)

## `src/app/` — the application

| Project | Owns | Depends on |
| --- | --- | --- |
| [GitExtensions](../../../src/app/GitExtensions/) | Executable entry point, bootstrap, DI wiring, app manifest. | GitUI |
| [GitUI](../../../src/app/GitUI/) | All WinForms UI: `FormBrowse`, dialogs (`CommandsDialogs/`), `RevisionGrid`, panels, theming, hotkeys, scripts engine, `GitUICommands`. | GitCommands |
| [GitCommands](../../../src/app/GitCommands/) | Git execution engine (`GitModule`, `Executable`), `Commands`, settings/config, remotes, submodules, output parsing. | Extensibility |
| [GitExtensions.Extensibility](../../../src/app/GitExtensions.Extensibility/) | **Public/versioned** interfaces & primitives: `IGitModule`, `IExecutable`, `IGitCommand`, `ArgumentBuilder`, settings/plugin contracts. | (base) |
| [GitExtUtils](../../../src/app/GitExtUtils/) | Cross-cutting helpers (string/collection utils, `GitUI` helpers). | (base) |
| [ResourceManager](../../../src/app/ResourceManager/) | Translation infrastructure & shared resources. | Extensibility |
| [GitExtensions.Analyzers.CSharp](../../../src/app/GitExtensions.Analyzers.CSharp/) | Roslyn analyzers enforcing repo conventions. | (analyzer) |
| [BugReporter](../../../src/app/BugReporter/) | Crash/bug reporting UI (NBug integration). | GitUI |

## `src/plugins/` — optional features

Each plugin implements [GitUIPluginInterfaces](../../../src/plugins/GitUIPluginInterfaces/) and is
discovered/hosted by `GitUI`. Examples:
`BackgroundFetch`, `AutoCompileSubmodules`, `Bitbucket`, `GitHub3`, `GitFlow`,
`BuildServerIntegration`, `Statistics`, `Gource`, `FindLargeFiles`, `ProxySwitcher`,
`CreateLocalBranches`, `DeleteUnusedBranches`, `ReleaseNotesGenerator`.
See [plugin-system](../L2-core-platform/docs-index.md) for how they load.

## `src/native/` — Windows integration (C++)

| Project | Owns |
| --- | --- |
| [GitExtensionsShellEx](../../../src/native/GitExtensionsShellEx/) | Windows Explorer shell extension (context-menu / overlay integration). |
| [GitExtSshAskPass](../../../src/native/GitExtSshAskPass/) | SSH `askpass` helper executable for credential prompts. |

Built via [src/native/build.proj](../../../src/native/build.proj) (needs VC++/ATL).

## `tests/`

| Folder | Covers |
| --- | --- |
| [tests/app/](../../../tests/app/) | Unit/integration tests for `src/app/` projects. |
| [tests/plugins/](../../../tests/plugins/) | Plugin tests. |
| [tests/CommonTestUtils/](../../../tests/CommonTestUtils/) | Shared test helpers/fixtures. |

Stack: NUnit + `NSubstitute` + `AwesomeAssertions`. See [testing-guide](../L2-core-platform/docs-index.md).

## Top-level support folders

- [Setup/](../../../Setup/) — installer (WiX), assets, `TranslationApp`, dictionaries.
- [eng/](../../../eng/) — build/engineering scripts, rulesets, StyleCop config.
- [Externals/](../../../Externals/) — git submodules (e.g. `ICSharpCode.TextEditor`, `NetSpell`).
- [.github/](../../../.github/) — CI workflows, these DAG docs, contributor instructions.

## Hard rules

- Put new UI in `GitUI`, new git logic in `GitCommands`, new contracts in `Extensibility`.
- Any change under `GitExtensions.Extensibility/` bumps the **plugin interface version** — note it in the commit message.
- Solution file: [GitExtensions.slnx](../../../GitExtensions.slnx); build with `dotnet build`.
