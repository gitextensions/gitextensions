---
name: run-tests
description: '**EXECUTION (runbook) SKILL** — Build and run the Git Extensions test suite and interpret results. USE FOR: running tests, running a single test/project, diagnosing a failing or flaky test. DO NOT USE FOR: writing new features (write tests as part of that), or CI config changes. Triggers: "run the tests", "run this test", "why is this test failing", "dotnet test".'
---

# Run Tests (runbook)

## Steps

1. **Build first:** `dotnet build /v:q` (tests won't be meaningful over a broken build).
2. **Run the suite:** `dotnet test` (the default VS Code "test" task). Or scope it:
   ```
   dotnet test .\tests\app\UnitTests\<Project>\<Project>.csproj
   ```
3. **Read failures:** note the test name (its snake_case suffix describes the expected behavior) and
   the AwesomeAssertions message.

## Conventions (see [testing-guide](../../copilot-docs/L2-core-platform/testing-guide.md))

- **NUnit** + **NSubstitute** (mocks) + **AwesomeAssertions** (never `ClassicAssert`, never Moq).
- Test names keep the method name and add a snake_case suffix (`MyMethod_should_do_x`).
- Substitute `IExecutable`/`IProcess` so tests don't spawn real `git`.
- `Verify.NUnit` snapshots use `.verified.*` files — review diffs when they change.
- Reach private members via a class's `TestAccessor`.

## Diagnosing a failure

- Reproduce the single test in isolation before changing code.
- Check whether a `Verify` snapshot legitimately changed (update it) vs a real regression.

## STOP conditions / hard rules

- A **flaky** test → find and fix the **root cause**; do not dismiss it as pre-existing or retry blindly.
- Do not weaken assertions or add `[Ignore]` to make a suite green — fix the underlying issue.
- Never use `[DataTestMethod]` (that's MSTest); this repo uses NUnit `[TestCase]`/`[TestCaseSource]`.
