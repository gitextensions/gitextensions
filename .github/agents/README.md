# Custom Agents (planned)

Custom agents are domain-scoped agent definitions (`*.agent.md`) that preload only the
docs-indexes relevant to one area. They exist to **split context** when loading every
docs-index at once becomes too expensive for a single agent.

> **Status: SKELETON / OPTIONAL.** Git Extensions is a single cohesive repository, so the
> default agent with the master [docs-index](../copilot-docs/docs-index.md) is sufficient
> today. Add custom agents only when the indexes grow large enough to strain the token budget.

## When to add a custom agent

- The combined docs-indexes no longer fit comfortably in context alongside real work.
- A subsystem has deep, self-contained docs that most sessions don't need.
- You want to restrict which tools/MCP servers an agent can use to reduce preloaded context.

Split along **logical ownership boundaries**, not arbitrarily.

## Candidate agents (when needed)

| Agent | Scope | Preloads |
| --- | --- | --- |
| `app.agent.md` | Core app (GitCommands + GitUI) | L1 + L2 + L3 indexes |
| `plugins.agent.md` | `src/plugins/*` and plugin interfaces | L2 `plugin-system` + plugin docs |
| `native.agent.md` | `src/native/*` (C++ shell ext, askpass) | L2 `shell-integration` |

Each agent file MUST act as a blocking gate: *"Do not answer, search, or use any tool until
all specified index files have been read."*
