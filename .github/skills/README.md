# Skills (planned)

Skills are cross-cutting workflows that apply across all documentation areas. Each skill lives
in its own folder as `.github/skills/<name>/SKILL.md` and is auto-matched by the agent from its
description/keyword triggers.

> **Status: SKELETON.** Skills below are the roadmap. None are written yet.

## Two classes of skills

- **SDLC (SME-first):** judgment-heavy work. MUST force the agent to become a domain expert
  first (read the relevant docs-index reading chain + anchor docs) before acting.
- **Execution (runbook):** deterministic "do X" procedures with prechecks, steps, and STOP conditions.

## Planned skills

| Skill | Class | Purpose | Status |
| --- | --- | --- | --- |
| `code-search` | SDLC | How to search this codebase effectively (symbols, forms, command flows) before editing. | planned |
| `deep-explanation` | SDLC | Explain a subsystem/flow with Why→What→How, Mermaid diagram, and code pointers. | planned |
| `doc-management` | SDLC | Create/maintain DAG docs: enforce agentic-doc rules, update the right docs-index, keep chains valid. | planned |
| `pr-creation` | Execution | Branch naming, conventional-commit messages, and the pre-PR checklist. | planned |
| `add-translation-strings` | Execution | Runbook for adding/changing UI strings and regenerating `English.xlf` via `update-loc.cmd`. | planned |
| `add-winforms-dialog` | Execution | Runbook for adding a new Form/control following naming + designer conventions. | planned |
| `run-tests` | Execution | Build and run the NUnit suite; interpret failures. | planned |

## Authoring rules

Every SDLC skill MUST begin with a blocking **"Build SME context"** phase: read the relevant
[docs-index](../copilot-docs/docs-index.md) reading chain and 1–N anchor docs, extract the rules/
invariants and code pointers, then explore surrounding code — *before* making changes.
