# L2 — Core Platform · Docs-Index

**TL;DR:** The infrastructure subsystems that flows (L3) build on. Load only the doc that
matches your task. Each doc leads with *why*, then *what*, then code pointers.
Read the [L0 primer](../L0-foundations/gitextensions-primer.md) first.

> **Status: SKELETON** — docs below are the roadmap. `planned` = not written yet.

## Git execution

| Doc | One-line description | Status |
| --- | --- | --- |
| [git-command-execution.md](git-command-execution.md) | How `git` is launched: `Executable`, `GitCommandRunner`, `ExecutionResult`, encoding. | done |
| [structured-commands.md](structured-commands.md) | `Commands`/`IGitCommand` records, `ArgumentBuilder`, remote vs state-changing flags. | done |
| [git-module.md](git-module.md) | `GitModule` responsibilities: repo state, refs, submodules, command entry point. | done |
| `output-parsing.md` | Parsing git output into models (`RevisionReader`, status/tree/diff parsers). | planned |

## Application services

| Doc | One-line description | Status |
| --- | --- | --- |
| [settings-system.md](settings-system.md) | Layered settings/config: `GitExtensions.settings`, `SettingsSource`, git config. | done |
| [plugin-system.md](plugin-system.md) | Plugin loading & interfaces (`GitUIPluginInterfaces`, `Extensibility.Plugins`). | done |
| [translation-system.md](translation-system.md) | Localization: `TranslatedStrings`, `English.xlf`, `update-loc.cmd`, `TranslationApp`. | done |
| `theming-system.md` | Themes & colors: `Theming/`, `Themes/`, high-contrast handling. | planned |
| `hotkey-system.md` | Hotkey registration and dispatch (`GitUI/Hotkey/`). | planned |
| `scripts-engine.md` | User-defined scripts and command menus (`GitUI/ScriptsEngine/`). | planned |
| `service-container.md` | DI / `ServiceContainerRegistry`, how services are registered and resolved. | planned |

## Platform & tooling

| Doc | One-line description | Status |
| --- | --- | --- |
| `shell-integration.md` | Native Windows Explorer shell extension (`src/native/GitExtensionsShellEx`). | planned |
| `build-and-installer.md` | Build (`dotnet build`), native build, installer (`Setup/`), publish. | planned |
| `ci-workflows.md` | GitHub Actions pipelines in `.github/workflows/` and what gates a PR. | planned |
| `testing-guide.md` | Test layout, NUnit + NSubstitute + AwesomeAssertions conventions, `TestAccessor`. | planned |

Back to [master docs-index](../docs-index.md).
