# Skills

Skills are cross-cutting workflows that apply across all documentation areas. Each skill lives
in its own folder as `.github/skills/<name>/SKILL.md` and is auto-matched by the agent from its
description/keyword triggers.

> **Status:** core skills written. Add more as recurring workflows emerge.

## Two classes of skills

- **SDLC (SME-first):** judgment-heavy work. MUST force the agent to become a domain expert
  first (read the relevant docs-index reading chain + anchor docs) before acting.
- **Execution (runbook):** deterministic "do X" procedures with prechecks, steps, and STOP conditions.

## Available skills

| Skill | Class | Purpose | Status |
| --- | --- | --- | --- |
| [code-search](code-search/SKILL.md) | SDLC | How to search this codebase effectively (symbols, forms, command flows) before editing. | done |
| [deep-explanation](deep-explanation/SKILL.md) | SDLC | Explain a subsystem/flow with Why→What→How, Mermaid diagram, and code pointers. | done |
| [doc-management](doc-management/SKILL.md) | SDLC | Create/maintain DAG docs: enforce agentic-doc rules, update the right docs-index, keep chains valid. | done |
| [pr-creation](pr-creation/SKILL.md) | Execution | Branch naming, conventional-commit messages, and the pre-PR checklist. | done |
| [add-translation-strings](add-translation-strings/SKILL.md) | Execution | Runbook for adding/changing UI strings and regenerating `English.xlf` via `update-loc.cmd`. | done |
| [add-winforms-dialog](add-winforms-dialog/SKILL.md) | Execution | Runbook for adding a new Form/control following naming + designer conventions. | done |
| [run-tests](run-tests/SKILL.md) | Execution | Build and run the NUnit suite; interpret failures. | done |

## Authoring rules

Every SDLC skill MUST begin with a blocking **"Build SME context"** phase: read the relevant
[docs-index](../copilot-docs/docs-index.md) reading chain and 1–N anchor docs, extract the rules/
invariants and code pointers, then explore surrounding code — *before* making changes.
