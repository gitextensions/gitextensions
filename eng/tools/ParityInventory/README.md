# ParityInventory

`ParityInventory` is a temporary, cross-platform source-analysis tool for the Avalonia parity
work. It compares an original partial class with its twin and writes deterministic
`functional-findings.json` evidence. It does not belong to either solution.

```powershell
dotnet run --project eng/tools/ParityInventory -- compare `
  --original-root src/app/GitUI `
  --twin-root src/app/GitUI.Avalonia `
  --type GitUI.FileStatusList `
  --translations src/app/GitUI/Translation/English.xlf `
  --output eng/avalonia/parity-evidence/P0.4/functional-findings.json
```

The report schema version is `2`. It contains:

- deterministic original and twin inventories: source partials, members and lexical order,
  event handlers/wiring, menu trees, hotkey command IDs, settings reads/writes,
  `TranslationString` fields, inferred translation keys with English-catalog presence, and
  normalized C# comments anchored to the member and placement they describe;
- categorized, stable-code findings for missing/extra facts and member-order differences;
- `comment.missing`, `comment.changed`, and `comment.drifted` findings. Comment sequence
  alignment prevents one deletion from producing a cascade of false changes;
- a separate `adaptedComments` collection for conservative WinForms-to-Avalonia framework-name
  substitutions. Adaptations remain visible evidence but do not count as parity gaps;
- explicit `partial.missing` findings that name both the original partial and expected twin
  path.

XML documentation, single-line comments, block comments, TODO/HACK/NOTE text, and issue links
are all included. Whitespace and comment delimiters are normalized; text is not otherwise
discarded. AXAML comments are not treated as substitutes for C# reasoning because they cannot
be anchored to an original code member.

The analyzer is deliberately syntax based. Facts that cannot be represented safely become
findings rather than guessed semantic matches.
