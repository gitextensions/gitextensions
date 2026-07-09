# L2 — Core Platform · Docs-Index

**TL;DR:** The infrastructure subsystems that flows (L3) build on. Load only the doc that
matches your task. Each doc leads with *why*, then *what*, then code pointers.
Read the [L0 primer](../L0-foundations/gitextensions-primer.md) first.

> All docs below are written. `Status` tracks any future additions.

## Git execution

| Doc | One-line description | Status |
| --- | --- | --- |
| [git-command-execution.md](git-command-execution.md) | How `git` is launched: `Executable`, `GitCommandRunner`, `ExecutionResult`, encoding. | done |
| [structured-commands.md](structured-commands.md) | `Commands`/`IGitCommand` records, `ArgumentBuilder`, remote vs state-changing flags. | done |
| [git-module.md](git-module.md) | `GitModule` responsibilities: repo state, refs, submodules, command entry point. | done |
| [git-output-parsing.md](git-output-parsing.md) | Parsing git output into models (`RevisionReader`, status/tree/diff parsers). | done |

## Application services

| Doc | One-line description | Status |
| --- | --- | --- |
| [settings-system.md](settings-system.md) | Layered settings/config: `GitExtensions.settings`, `SettingsSource`, git config. | done |
| [plugin-system.md](plugin-system.md) | Plugin loading & interfaces (`GitUIPluginInterfaces`, `Extensibility.Plugins`). | done |
| [translation-system.md](translation-system.md) | Localization: `TranslatedStrings`, `English.xlf`, `update-loc.cmd`, `TranslationApp`. | done |
| [theming-system.md](theming-system.md) | Themes & colors: `Theming/`, `Themes/`, high-contrast handling. | done |
| [hotkey-system.md](hotkey-system.md) | Hotkey registration and dispatch (`GitUI/Hotkey/`). | done |
| [scripts-engine.md](scripts-engine.md) | User-defined scripts and command menus (`GitUI/ScriptsEngine/`). | done |
| [service-container.md](service-container.md) | DI / `ServiceContainerRegistry`, how services are registered and resolved. | done |

## Platform & tooling

| Doc | One-line description | Status |
| --- | --- | --- |
| [shell-integration.md](shell-integration.md) | Native Windows Explorer shell extension (`src/native/GitExtensionsShellEx`). | done |
| [build-and-installer.md](build-and-installer.md) | Build (`dotnet build`), native build, installer (`Setup/`), publish. | done |
| [ci-workflows.md](ci-workflows.md) | GitHub Actions pipelines in `.github/workflows/` and what gates a PR. | done |
| [testing-guide.md](testing-guide.md) | Test layout, NUnit + NSubstitute + AwesomeAssertions conventions, `TestAccessor`. | done |

Back to [master docs-index](../docs-index.md).
