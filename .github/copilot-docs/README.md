<!--
  Meta-doc for MAINTAINERS (humans). This is the only file in copilot-docs/
  written primarily for people rather than agents. It explains how the DAG
  documentation set is organised and how to grow it. Agents should start from
  docs-index.md, not here.
-->
# Git Extensions Agent Documentation (DAG)

This folder contains **Docs-Augmented Generation (DAG)** documentation: repository-native,
agent-optimised docs that let an AI coding agent build accurate context about Git Extensions
*incrementally* — without vector databases, embeddings, or external infrastructure.

> The layer indexes list every doc with a `planned` / `draft` / `done` status. The L1–L3 docs
> and skills are written; use the status column when adding new docs.

## How it works (30-second version)

1. VS Code auto-loads [.github/copilot-instructions.md](../copilot-instructions.md) into every chat.
2. That file links to the always-loaded [L0 primer](L0-foundations/gitextensions-primer.md) and to the master [docs-index.md](docs-index.md).
3. `docs-index.md` links to the per-layer indexes, which link to individual docs.
4. The agent reads the cheap **indexes** first, then loads **only** the specific docs a task needs — preserving the token budget for reading real code.

```
copilot-instructions.md  ──►  L0 primer (always loaded, < 80 lines)
                          └─►  docs-index.md (master)
                                    ├─► L1-conceptual/docs-index.md   ──► concept docs
                                    ├─► L2-core-platform/docs-index.md ──► platform docs
                                    └─► L3-flows/docs-index.md         ──► end-to-end flow docs
```

## The four layers

| Layer | Folder | Contains | Loading |
| --- | --- | --- | --- |
| **L0 – Foundations** | `L0-foundations/` | The irreducible minimum every agent needs: architecture anchor, glossary, ownership map. | **Always loaded.** No index. Hard cap **80 lines**. |
| **L1 – Conceptual** | `L1-conceptual/` | What Git Extensions is: architecture, terminology, project map. | On-demand via `docs-index.md`. |
| **L2 – Core Platform** | `L2-core-platform/` | Infrastructure: git execution, settings, plugins, translation, theming, build/test. | On-demand via `docs-index.md`. |
| **L3 – Flows** | `L3-flows/` | End-to-end user flows that compose L2 (commit, checkout, clone, push/pull, rebase, diff). | On-demand via `docs-index.md`. |

## Rules for writing DAG docs (agentic docs, not user docs)

These are **hard rules**. Agents behave inconsistently when docs are long or bury the point.

- **TL;DR + links at the TOP.** Put the summary and cross-links in the first ~50 lines. Agents often read only the first 80–100 lines of a file.
- **Why → What → How.** Lead with *why* the component exists, then *what* it does, then *how* to find it. Code answers "how"; docs must supply the "why".
- **Code pointers, not code.** Reference `ClassName` / `MethodName` / file paths the agent can open — never paste code snippets that go stale.
- **~100 lines per doc.** If a topic needs more, split it and link from the index.
- **Hard rules beat suggestions.** Write `MUST` / `ALWAYS` / `STOP`, not "you might consider".

## Related folders

- [.github/agents/](../agents/) — custom domain-scoped agents (planned).
- [.github/skills/](../skills/) — cross-cutting workflow skills.

## How to add a doc

1. Write the doc in the correct layer folder following the rules above.
2. Add a one-line entry to that layer's `docs-index.md`.
3. If it participates in a common query, add/extend a **reading chain** in the master [docs-index.md](docs-index.md).
4. (Later) Add a benchmark test so growth doesn't regress other areas.
