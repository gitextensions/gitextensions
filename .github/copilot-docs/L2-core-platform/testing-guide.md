<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Testing Guide (L2)

**TL;DR:** Tests use **NUnit** with **NSubstitute** (mocking) and **AwesomeAssertions**
(assertions). Unit tests live under `tests/app/UnitTests/` (mirroring the source projects); UI
integration tests under `tests/app/IntegrationTests/`. Test names use a **snake_case suffix**
while keeping the method-under-test name intact. Run with `dotnet test`.

**Related:** [build-and-installer](build-and-installer.md) · [ci-workflows](ci-workflows.md) · [git-command-execution](git-command-execution.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

A git GUI has lots of parsing, argument-building, and stateful UI. Fast, isolated tests (with the
`git` process and services substituted) catch regressions cheaply, and the shared conventions keep
the large suite consistent and readable.

## What

- **Unit tests** — [tests/app/UnitTests/](../../../tests/app/UnitTests/): `GitCommands.Tests`,
  `GitUI.Tests`, `ResourceManager.Tests`, `BugReporter.Tests`, etc.
- **Integration tests** — [tests/app/IntegrationTests/](../../../tests/app/IntegrationTests/)
  (`UI.IntegrationTests`): drive real forms/controls (`RevisionGridControl`, script engine, …).
- **Shared helpers** — [tests/CommonTestUtils/](../../../tests/CommonTestUtils/).
- **Test config** — [eng/Tests.targets](../../../eng/Tests.targets) / `eng/Tests.props` wire in
  NUnit, AwesomeAssertions, NSubstitute, and `Verify.NUnit` (snapshot testing).

## How (conventions)

- **Framework:** NUnit. **Mocks:** `NSubstitute`. **Assertions:** `AwesomeAssertions` (never `ClassicAssert`).
- **Naming:** keep the method name, add a snake_case suffix — e.g. a test for `MyMethod` →
  `MyMethod_should_return_expected`. Don't repeat the name in a comment.
- **No `Arrange`/`Act`/`Assert` comments.**
- **Isolation:** substitute `IExecutable`/`IProcess` (see [git-command-execution](git-command-execution.md))
  so tests don't spawn real `git`; substitute services registered in the
  [service container](service-container.md).
- **`TestAccessor`:** classes expose an internal `TestAccessor` to reach private members from
  tests — do **not** add XML docs to `TestAccessor` types/members.
- **Snapshots:** use `Verify.NUnit` (`.verified.*` files) for large/structured output.

```
dotnet test        # runs the suite (default VS Code "test" task)
```

## Hard rules

- Use NUnit + `NSubstitute` + `AwesomeAssertions`. **NEVER** `ClassicAssert`, **never** Moq.
- Test names: snake_case suffix, method-under-test name unchanged; no redundant name comment.
- No `Arrange`/`Act`/`Assert` comments.
- Fix flaky tests at the root cause — don't dismiss them as pre-existing.
- New git operations SHOULD have unit tests asserting the built arguments.

**Next:** back to the [L2 index](docs-index.md) or explore an [L3 flow](../L3-flows/docs-index.md).
