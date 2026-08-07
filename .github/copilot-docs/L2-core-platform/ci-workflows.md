<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# CI Workflows (L2)

**TL;DR:** GitHub Actions gate every PR. The build/test/package logic lives in **one reusable
workflow**, `_app-build-core.yml`, called two ways: `app-build.yml` (CI — runs tests) and
`app-release.yml` (release — packages + signs). A PR must build, pass tests, have a current
`English.xlf`, contain no `fixup!`/`squash!` commits, and pass the CLA check.

**Related:** [release-pipeline](release-pipeline.md) · [build-and-installer](build-and-installer.md) · [testing-guide](testing-guide.md) · [translation-system](translation-system.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

A large WinForms app with native COM components and localisation needs automated gates so
regressions, untranslated strings, and messy history never reach `master`. The build steps are
identical for CI and release, so they live in a single **reusable** workflow (`workflow_call`) that a
`release` boolean toggles between the two roles — avoiding drift between "what CI tests" and "what
release ships".

## What (`.github/workflows/`)

| Workflow | Role |
| --- | --- |
| [_app-build-core.yml](../../../.github/workflows/_app-build-core.yml) | **Reusable core.** Checkout → set version → native build → .NET build → verify loc → publish. `release=false` runs tests; `release=true` packages MSI/portable artifacts. |
| [app-build.yml](../../../.github/workflows/app-build.yml) | **CI wrapper.** On PR + push to `master`/`release`/`release/**`/`experimental/**`. Resolves an OS matrix, calls the core with `release=false`, then publishes a `test-report`. The main quality gate. |
| [app-release.yml](../../../.github/workflows/app-release.yml) | **Release wrapper.** On `v[0-9]*` tags. Calls the core with `release=true`, then submits artifacts to SignPath. See [release-pipeline](release-pipeline.md). |
| [git.yml](../../../.github/workflows/git.yml) | Blocks merge while `fixup!`/`squash!` commits are present. |
| [cla-check.yml](../../../.github/workflows/cla-check.yml) | Verifies the contributor sign-off in `contributors.txt`; publishes a `CLA` commit status. |
| [pr-automation.yml](../../../.github/workflows/pr-automation.yml) · [pr-check-stale.yml](../../../.github/workflows/pr-check-stale.yml) · [label-lifecycle.yml](../../../.github/workflows/label-lifecycle.yml) | PR triage, stale handling, label lifecycle. |
| `labeler-*.yml` | ML-based PR/issue labeling (train / predict / cache / promote). |

## How CI runs (`app-build.yml`)

1. **setup** resolves the OS matrix: `release`/`release/**` branches build **x64 + arm64**; every
   other ref builds **x64 only** to save CI minutes.
2. **build-and-test** calls `_app-build-core.yml` per matrix leg (`release=false`): builds native +
   .NET, runs the localisation check, publishes, runs `dotnet test`, uploads `test-results-<arch>`.
3. **test-report** downloads the `test-results-*` artifacts and renders them via `dorny/test-reporter`
   (only when `has-test-results == 'true'`).

## How (what fails a PR)

- **Compilation / analyzer / StyleCop** violations → the `Build .NET` step fails.
- **Failing tests** (NUnit unit or UI integration) → the `Run tests` step fails.
- **Stale `English.xlf`** — the `Verify localisation` step regenerates translations and fails if the
  working tree changed. Run `update-loc.cmd` locally. See [translation-system](translation-system.md).
- **`fixup!` / `squash!` commits** present → `git.yml` blocks merge (autosquash them first).
- **Missing CLA sign-off** → `cla-check.yml` blocks.

## Hard rules

- Before pushing: build clean, run `dotnet test`, and run `update-loc.cmd` if any UI string changed.
- **NEVER** leave `fixup!`/`squash!` commits on a PR branch destined for merge — rebase/autosquash locally.
- Don't disable or weaken these workflows to make a PR pass; fix the underlying issue.
- Edit build/test/package steps in **`_app-build-core.yml`**, not the wrappers — the wrappers only
  select the matrix and the `release` role.

**Next:** [release-pipeline](release-pipeline.md) for tagging, versioning, and signing.
