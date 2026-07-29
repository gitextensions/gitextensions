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

The report schema version is `1`. It contains:

- deterministic original and twin inventories: source partials, members and lexical order,
  event handlers/wiring, menu trees, hotkey command IDs, settings reads/writes,
  `TranslationString` fields, and inferred translation keys with English-catalog presence;
- categorized, stable-code findings for missing/extra facts and member-order differences;
- explicit `partial.missing` findings that name both the original partial and expected twin
  path.

The analyzer is deliberately syntax based. Facts that cannot be represented safely must become
findings in a future schema revision rather than guessed semantic matches.
