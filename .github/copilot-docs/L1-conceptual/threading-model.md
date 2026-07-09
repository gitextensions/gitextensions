<!-- L1 CONCEPTUAL. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Threading Model (L1)

**TL;DR:** Git Extensions is a WinForms app, so all UI access **must** happen on the UI thread,
while git commands run in the background. The project uses Microsoft's **vs-threading**
(`JoinableTaskFactory` via `ThreadHelper`) to switch threads safely, and `AsyncLoader` to load
data off-thread then marshal results back. Analyzer rule files under `eng/` enforce main-thread
correctness at build time.

**Related:** [architecture-overview](architecture-overview.md) · [ui-composition](ui-composition.md) · [git-command-execution](../L2-core-platform/git-command-execution.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

WinForms controls are not thread-safe: touching them off the UI thread causes crashes or subtle
corruption. But git operations are slow (spawning processes, reading output), so they must not
block the UI thread. The threading model reconciles these: **do work in the background, touch UI
on the main thread**, using a deadlock-safe primitive (`JoinableTaskFactory`).

## What

- **`ThreadHelper`** — the app's static `JoinableTaskFactory` and assertions.
  [ThreadHelper.cs](../../../src/app/GitExtUtils/GitUI/ThreadHelper.cs). Key members:
  `ThreadHelper.JoinableTaskFactory`, `ThrowIfNotOnUIThread()`, `AssertOnUIThread()`.
- **`SwitchToMainThreadAsync()`** — the awaitable that moves execution back onto the UI thread.
- **`AsyncLoader`** — loads data on a background thread and invokes a callback on the UI thread;
  cancellable, supports a debounce delay. [AsyncLoader.cs](../../../src/app/GitCommands/AsyncLoader.cs).
- **`Control.CheckForIllegalCrossThreadCalls`** — enabled for non-official builds in
  [Program.cs](../../../src/app/GitExtensions/Program.cs) to surface cross-thread bugs early.
- **Analyzer rules** — `eng/vs-threading.*.txt` list main-thread-asserting/switching methods so
  the vs-threading analyzers can flag violations:
  [MainThreadAssertingMethods](../../../eng/vs-threading.MainThreadAssertingMethods.txt) ·
  [MainThreadSwitchingMethods](../../../eng/vs-threading.MainThreadSwitchingMethods.txt) ·
  [TypesRequiringMainThread](../../../eng/vs-threading.TypesRequiringMainThread.txt).

## How (patterns)

```
await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();  // now safe to touch UI
```

- Background → UI: `await …SwitchToMainThreadAsync()` before touching controls.
- Fire-and-forget UI work: run via `JoinableTaskFactory.Run(...)` / `RunAsync(...)`, not raw `Task.Run`.
- Loading grids/panels: prefer `AsyncLoader` so cancellation + UI marshaling are handled for you.
- Synchronously waiting on async: use `ThreadHelper.JoinableTaskFactory.Run(...)` (deadlock-safe),
  never `.Result`/`.Wait()`.

## Hard rules

- **ALWAYS** switch to the main thread (`SwitchToMainThreadAsync`) before touching WinForms controls.
- **NEVER** block on async with `.Result` or `.Wait()` — use `JoinableTaskFactory.Run`.
- Respect the vs-threading analyzer; if it flags a method, fix the threading, don't suppress it.
- Long-running git work belongs in the background (see [git-command-execution](../L2-core-platform/git-command-execution.md)).

**Next:** [ui-composition](ui-composition.md) for how forms/controls are assembled.
