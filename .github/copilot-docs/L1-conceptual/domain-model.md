<!-- L1 CONCEPTUAL. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Domain Model (L1)

**TL;DR:** The in-memory types that represent git objects. The most important are `ObjectId`
(a SHA), `GitRevision` (a commit), `GitRef` (a branch/tag), and `GitItemStatus` (a changed file).
Tree contents use `IGitItem`/`GitItem`; the rendered commit graph uses `RevisionGraphRow`. Core
types live in the versioned `Extensibility` assembly; concrete helpers live in `GitCommands`.

**Related:** [glossary](glossary.md) · [git-command-execution](../L2-core-platform/git-command-execution.md) · [git-output-parsing](../L2-core-platform/git-output-parsing.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Git speaks in text (SHAs, ref names, status lines). To reason about a repository safely, Git
Extensions parses that text **once** into strongly-typed, mostly-immutable objects, so the UI and
logic don't re-parse strings everywhere. Putting the core types in `Extensibility` lets plugins
share the same model.

## What (the core types)

| Type | File | Role |
| --- | --- | --- |
| `ObjectId` | [Extensibility/Git/ObjectId.cs](../../../src/app/GitExtensions.Extensibility/Git/ObjectId.cs) | Immutable git SHA-1 (readonly struct, 20 bytes). Equality/parse helpers. |
| `GitRevision` | [Extensibility/Git/GitRevision.cs](../../../src/app/GitExtensions.Extensibility/Git/GitRevision.cs) | A commit: `ObjectId`, parents, author/committer, subject/body, refs. |
| `GitRef` | [GitCommands/Git/GitRef.cs](../../../src/app/GitCommands/Git/GitRef.cs) | A branch/tag/remote ref (`IGitRef`): name, target `ObjectId`, kind. |
| `GitItemStatus` | [Extensibility/Git/GitItemStatus.cs](../../../src/app/GitExtensions.Extensibility/Git/GitItemStatus.cs) | A changed file's status flags (tracked/new/deleted/changed/renamed/…). |
| `IGitItem` / `IObjectGitItem` | [Extensibility/Git/](../../../src/app/GitExtensions.Extensibility/Git/) | Tree-item contracts (object id + name + git object type). |
| `GitItem` | [GitCommands/Git/GitItem.cs](../../../src/app/GitCommands/Git/GitItem.cs) | Concrete tree item (blob/tree/commit entry). |
| `RevisionGraphRow` | [GitUI/UserControls/RevisionGrid/Graph/RevisionGraphRow.cs](../../../src/app/GitUI/UserControls/RevisionGrid/Graph/RevisionGraphRow.cs) | One graph row with lane segments to parents. |

## How they're produced

- Commits: `RevisionReader` parses `git log` output → `GitRevision`.
- Files: `GetAllChangedFilesOutputParser` parses `git status --porcelain=2 -z` → `GitItemStatus`.
- Trees: `GitTreeParser` parses `git ls-tree` → `GitItem`.
- Refs: parsed by `GitModule` (see the `RefRegex` generated regex).
- See [git-output-parsing](../L2-core-platform/git-output-parsing.md) for the parsing layer.

## Notes

- Prefer `ObjectId` over raw `string` SHAs; it's cheaper to compare and impossible to malform.
- The grid shows two **artificial** revisions (WorkTree, Index) that are not real commits.
- Model types are largely immutable — construct new instances rather than mutating.

## Hard rules

- **NEVER** pass SHAs around as `string` when an `ObjectId` is available.
- New model types that plugins need MUST live in `Extensibility` (and bump the plugin interface version).
- Keep parsing in the parser classes — model types should not parse git output themselves.

**Next:** [threading-model](threading-model.md) for how these are loaded off the UI thread.
