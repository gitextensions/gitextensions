<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# CI Workflows (L2)

**TL;DR:** GitHub Actions gate every PR. The key one is `pr-build.yml` (build + unit/integration
tests). Others enforce commit hygiene (`git.yml` blocks `fixup!`/`squash!`), the CLA, and PR
housekeeping. A PR must build, pass tests, have a current `English.xlf`, contain no fixup commits,
and pass the CLA check.

**Related:** [build-and-installer](build-and-installer.md) · [testing-guide](testing-guide.md) · [translation-system](translation-system.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

A large WinForms app with native components and localisation needs automated gates so regressions,
untranslated strings, and messy history never reach `master`. Encoding these as workflows makes
the rules explicit and enforced for every contributor.

## What (`.github/workflows/`)

| Workflow | Gates |
| --- | --- |
| [pr-build.yml](../../../.github/workflows/pr-build.yml) | Builds and runs unit + integration tests on PR / push to master & release branches. The main quality gate. |
| [git.yml](../../../.github/workflows/git.yml) | Blocks merging while `fixup!` / `squash!` commits are present. |
| [cla-check.yml](../../../.github/workflows/cla-check.yml) | Verifies the Contributor License Agreement. |
| [pr-automation.yml](../../../.github/workflows/pr-automation.yml) | PR triage: labels, milestones, cleanup. |
| [pr-check-stale.yml](../../../.github/workflows/pr-check-stale.yml) | Flags/handles stale PRs and issues. |
| [label-lifecycle.yml](../../../.github/workflows/label-lifecycle.yml) | Label lifecycle automation. |
| `labeler-*.yml` | ML-based PR/issue labeling (train / predict / cache / promote). |

## How (what fails a PR)

- **Compilation errors** or analyzer/StyleCop violations → build fails.
- **Failing tests** (NUnit unit or UI integration) → `pr-build` fails.
- **Stale `English.xlf`** — if UI strings changed without running `update-loc.cmd`, the loc check
  fails. See [translation-system](translation-system.md).
- **`fixup!` / `squash!` commits** present → `git.yml` blocks merge (squash them first).
- **Unsigned CLA** → `cla-check` blocks.

## Hard rules

- Before pushing: build clean, run `dotnet test`, and run `update-loc.cmd` if any UI string changed.
- **NEVER** leave `fixup!`/`squash!` commits on a PR branch destined for merge — rebase/autosquash locally.
- Don't disable or weaken these workflows to make a PR pass; fix the underlying issue.

**Next:** [testing-guide](testing-guide.md).
