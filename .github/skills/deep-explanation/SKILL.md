---
name: deep-explanation
description: '**SDLC (SME-first) SKILL** — Explain a Git Extensions subsystem or end-to-end flow accurately, grounded in the DAG docs and verified against source. USE FOR: "how does X work", "explain the commit/checkout/push flow", "trace this from UI to git", onboarding walkthroughs, architecture questions. DO NOT USE FOR: making code changes, writing new docs (use doc-management), or trivial single-file lookups. Triggers: "explain how", "walk me through", "how does X work end to end", "trace the flow".'
---

# Deep Explanation

Produce an accurate, grounded explanation — never hand-wave.

## Phase 1 — Become a mini-SME first (BLOCKING)

STOP — before explaining, ground yourself:
1. Read the [L0 primer](../../copilot-docs/L0-foundations/gitextensions-primer.md).
2. Open the [master docs-index](../../copilot-docs/docs-index.md), pick the matching **reading chain**,
   and read those docs in order (L1 concept → L2 subsystem → L3 flow).
3. **Verify against source** — open the actual `src/` files the docs point to. Docs give the
   "why"; the code confirms the "how".

## Phase 2 — Explain with structure

Use **Why → What → How**:
- **Why** — the motivation/design rationale (from the docs).
- **What** — the components/actors and how they connect.
- **How** — the concrete path, with **code pointers** (`ClassName` / `Method` / file links) the
  reader can open. Prefer pointers over pasted code.

Include a compact **Mermaid** diagram for multi-step flows (sequence or graph). Label the
execution context (UI thread vs background) where it matters — see
[threading-model](../../copilot-docs/L1-conceptual/threading-model.md).

## Phase 3 — Close the loop

- Note cross-tier hops explicitly (UI → `GitUICommands` → `Commands`/`GitModule` → `git`).
- Call out quirks/gotchas the code reveals (e.g. legacy `ArgumentString` vs `IGitCommand`).
- End with pointers to the exact files to read next.

## Hard rules

- **NEVER** state a class/method/flow you haven't verified in source or a DAG doc.
- Ground every claim in a code pointer or a doc reference.
- Keep it scannable: lead with a 2–3 line summary, then the structured explanation.
