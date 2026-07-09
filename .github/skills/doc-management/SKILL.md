---
name: doc-management
description: '**SDLC (SME-first) SKILL** — Create, update, or review Git Extensions DAG agent-docs under .github/copilot-docs. USE FOR: writing a new L1/L2/L3 doc; updating a docs-index; adding/fixing a reading chain; keeping the L0 primer within its size cap; reviewing a doc for the agentic-doc rules. DO NOT USE FOR: writing product/user documentation, README changes for end users, or code changes. Triggers: "add a DAG doc", "document this subsystem", "update the docs-index", "add a reading chain", "is this doc agentic".'
---

# Doc Management (DAG)

Keep the [DAG documentation](../../copilot-docs/README.md) consistent, small, and navigable.

## Phase 1 — Build SME context (BLOCKING; do this first)

STOP — do not write a doc until you have:
1. Read the [L0 primer](../../copilot-docs/L0-foundations/gitextensions-primer.md) and the
   [master docs-index](../../copilot-docs/docs-index.md).
2. Read the target layer's `docs-index.md` and 1–2 sibling docs to match tone/shape.
3. Verified the real code you'll point to (open the actual `src/` files — pointers MUST be correct).

## Phase 2 — Write / update the doc

Follow the **agentic-doc rules** (hard rules):
- **TL;DR + related links in the first ~15 lines.** Agents often read only the first 80–100 lines.
- **Why → What → How.** Lead with motivation, then concepts, then code pointers.
- **Code pointers, not code.** Link `ClassName` / file paths; do NOT paste code that will rot.
- **~100 lines max** (L0 primer: **80 lines max**, no index). Split if larger.
- **Hard rules use MUST/ALWAYS/NEVER/STOP**, not "consider".
- Use workspace-relative markdown links; verify every link resolves.

## Phase 3 — Wire it in (MUST)

1. Add a one-line row to the correct `docs-index.md` with a status (`planned`/`done`) and a link.
2. If it answers a common question, add/extend a **reading chain** in the [master docs-index](../../copilot-docs/docs-index.md).
3. Keep the ownership table current if the doc introduces a new area.

## Phase 4 — Validate (MUST)

- Confirm line count is within the cap (L0 ≤ 80; others ≤ ~100).
- Confirm every relative link resolves (docs and `src/` files).
- Re-ask the driving question mentally: does the reading chain now reach the right code faster?

## STOP conditions

- A code pointer you can't verify → open the file first; never guess class/file names.
- The doc exceeds the size cap → split it and link from the index.
- L0 primer needs an index → it's too big; move detail to L1.
