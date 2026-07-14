<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Service Container (DI) (L2)

**TL;DR:** Git Extensions uses a lightweight DI container (`System.ComponentModel.Design.ServiceContainer`)
wired up at startup in `Program.cs`. Each layer contributes a `ServiceContainerRegistry.RegisterServices`
that registers its services; code resolves them with `GetRequiredService<T>()`. `GitUICommands` acts
as the primary service hub passed to forms.

**Related:** [architecture-overview](../L1-conceptual/architecture-overview.md) · [git-command-execution](git-command-execution.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

The app is layered and needs testable seams: forms shouldn't `new` up executors, parsers, or
managers directly. A container lets each layer register its implementations once and lets
consumers ask for interfaces, so tests can substitute fakes and layers stay decoupled.

## What

Startup composition in [Program.cs](../../../src/app/GitExtensions/Program.cs):

```
ServiceContainer container = new();
ServiceContainerRegistry.RegisterServices(container);   // called per layer
```

Each layer owns a `ServiceContainerRegistry`:

| Registry | File | Registers |
| --- | --- | --- |
| Utils | [GitExtUtils/ServiceContainerRegistry.cs](../../../src/app/GitExtUtils/ServiceContainerRegistry.cs) | Core services (e.g. `ISubscribableTraceListener`). |
| Commands | [GitCommands/ServiceContainerRegistry.cs](../../../src/app/GitCommands/ServiceContainerRegistry.cs) | Git services: `IGitExecutorProvider`, submodule status, etc. |
| UI | [GitUI/ServiceContainerRegistry.cs](../../../src/app/GitUI/ServiceContainerRegistry.cs) | UI services: `IScriptsManager`, `IHotkeySettingsManager`, repository-history UI, … |

- **Resolution** — `serviceContainer.GetRequiredService<T>()` (throws if missing). Forms get the
  container via `GitModuleForm`/`GitUICommands`.
- **`GitUICommands`** implements the service-provider role for the UI and carries the current
  `IGitModule`. [GitUICommands.cs](../../../src/app/GitUI/GitUICommands.cs).

## How (add a service)

1. Define the interface in the layer that owns the contract (often `Extensibility`).
2. Register the implementation in that layer's `ServiceContainerRegistry.RegisterServices`.
3. Resolve it via `GetRequiredService<T>()`; in tests, register a substitute instead.

## Hard rules

- Register services in the **owning layer's** `ServiceContainerRegistry`, not ad hoc.
- Prefer container-resolved interfaces over `new`-ing concrete services in forms.
- Use `GetRequiredService<T>()` for mandatory services so missing registrations fail fast.

**Next:** [shell-integration](shell-integration.md) or back to the [L2 index](docs-index.md).
