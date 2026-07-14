---
name: pr-creation
description: '**EXECUTION (runbook) SKILL** — Prepare a Git Extensions change for a pull request so it passes CI. USE FOR: "create a PR", "get this ready to push", pre-commit/pre-push checklist, writing a conventional-commit message. DO NOT USE FOR: designing the change itself or explaining code. Triggers: "prepare a PR", "ready to commit", "pre-PR checklist", "write the commit message".'
---

# PR Creation (runbook)

A deterministic checklist to make a change merge-ready. Do the steps in order; STOP on any failure.

## Prechecks

1. Confirm you're on a feature branch (not `master`) with a descriptive name.
2. Confirm the working tree contains only intended changes (`git status`).

## Build & test (gates)

3. **Build clean:** `dotnet build /v:q` — no errors, no new analyzer/StyleCop warnings.
4. **Run tests:** `dotnet test` (or the affected test project). All green. See
   [testing-guide](../../copilot-docs/L2-core-platform/testing-guide.md).

## Localization gate (MUST if UI changed)

5. If you added/changed any form, control, menu/toolbar item, or `TranslationString`:
   run `.\update-loc.cmd` from the repo root and stage the regenerated
   [English.xlf](../../../src/app/GitUI/Translation/English.xlf). **CI fails if it's stale.**
   See [translation-system](../../copilot-docs/L2-core-platform/translation-system.md).

## Commit

6. Use **Conventional Commits** (e.g. `feat:`, `fix:`, `docs:`, `perf:`, `refactor:`).
7. If any file under `src/app/GitExtensions.Extensibility/` changed, **note the plugin interface
   version impact** in the commit message.
8. Create a **new** commit (do not amend published commits).

## Postchecks (STOP conditions)

- **NEVER** leave `fixup!` / `squash!` commits on the branch — the `git` workflow blocks merge. Autosquash locally.
- Ensure the CLA is signed (the `cla-check` workflow gates it).
- Files MUST be CRLF and satisfy `.editorconfig`/StyleCop.
- Do **not** use `--no-verify` or disable CI checks to force a pass — fix the cause.

## Do NOT push unless explicitly asked

Prepare the commit(s); only run `git push` when the user explicitly requests it.
