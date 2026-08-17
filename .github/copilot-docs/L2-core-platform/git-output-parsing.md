<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Git Output Parsing (L2)

**TL;DR:** Git commands emit text; parser classes turn that text into the typed
[domain model](../L1-conceptual/domain-model.md) exactly once. The main parsers are
`RevisionReader` (log → `GitRevision`), `GitTreeParser` (ls-tree → `GitItem`), and
`GetAllChangedFilesOutputParser` (status → `GitItemStatus`). Parsers rely on NUL-delimited
(`-z`, `--porcelain`) output for robustness.

**Related:** [git-command-execution](git-command-execution.md) · [domain-model](../L1-conceptual/domain-model.md) · [git-module](git-module.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Parsing git output is error-prone: paths can contain spaces/newlines, encodings vary, and
formats differ across git versions. Centralising parsing in dedicated classes (fed
machine-readable `-z`/`--porcelain` output) keeps the fragile string handling in one place and
produces immutable, reusable models for the rest of the app.

## What

| Parser | File | Reads → Produces |
| --- | --- | --- |
| `RevisionReader` | [RevisionReader.cs](../../../src/app/GitCommands/RevisionReader.cs) | `git log --format` → `GitRevision` list (range or explicit list). |
| `GitTreeParser` | [GitTreeParser.cs](../../../src/app/GitCommands/Git/GitTreeParser.cs) | `git ls-tree` / `ls-files --stage` → `GitItem`. |
| `GetAllChangedFilesOutputParser` | [GetAllChangedFilesOutputParser.cs](../../../src/app/GitCommands/Git/GetAllChangedFilesOutputParser.cs) | `git status --porcelain=2 -z` → `GitItemStatus`. |
| `DetachedHeadParser` | [DetachedHeadParser.cs](../../../src/app/GitCommands/Git/DetachedHeadParser.cs) | Detects detached HEAD from status/branch text. |
| `ExternalLinkRevisionParser` | [ExternalLinkRevisionParser.cs](../../../src/app/GitCommands/ExternalLinks/ExternalLinkRevisionParser.cs) | Extracts revision/remote patterns for external links. |

`GitModule` also parses refs inline with generated regexes (`RefRegex`, `ShaRegex`,
`RemoteVerboseLineRegex`) at the top of [GitModule.cs](../../../src/app/GitCommands/Git/GitModule.cs).

## How

- Producers ask git for machine-readable output: `--porcelain`, `-z` (NUL delimiters), fixed
  `--format` strings — never human-formatted output.
- `RevisionReader` streams and constructs `GitRevision`s incrementally for responsiveness (it
  feeds the [revision-grid flow](../L3-flows/revision-grid-flow.md)).
- Encoding: decode with `GitModule.SystemEncoding` / the module's `CommitEncoding` as appropriate.

## Hard rules

- **ALWAYS** parse `-z` / `--porcelain` output, not human-readable output (which changes across git versions/locales).
- Keep parsing **inside** these parser classes; model types must not parse text themselves.
- Split on the first delimiter only when values can contain spaces (paths, ref names).
- Cover new parsing with unit tests using recorded git output (`Verify.NUnit` snapshots where useful).

**Next:** [git-module](git-module.md) for who calls these parsers.
