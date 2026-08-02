# Avalonia visual-parity captures

The Avalonia test project can render every ported AXAML window and control with headless
Skia. It discovers views from `src/app/GitUI.Avalonia` automatically, creates a temporary
sample repository, and captures each view in light and dark mode.

Regenerate the local review set from the repository root:

```console
GITEXT_CAPTURE_PARITY_SHOTS=1 dotnet test tests/app/UnitTests/GitUI.Avalonia.Tests/GitUI.Avalonia.Tests.csproj --filter 'Category=VisualParityCapture'
```

The command replaces the generated `Light/` and `Dark/` directories under
`eng/avalonia/parity-shots/` and writes a PNG for each theme/scale/view plus `manifest.json`.
Other local comparison directories are preserved. The folder is intentionally excluded in
`.git/info/exclude`: captures are developer artifacts, not golden files. Normal test runs do
not regenerate them, but they do verify that every AXAML view resolves to a control with its
required public parameterless constructor.

For a targeted repository-tree capture with the recursive submodule node and its context menu,
also set `GITEXT_CAPTURE_PARITY_VIEW=RepoObjectsTree` and
`GITEXT_CAPTURE_REPO_TREE_CONTEXT=submodule`.

Place owner-supplied WinForms screenshots in the likewise excluded
`eng/avalonia/parity-reference/` folder. Compare those references with the generated browse,
diff, commit, and dialog captures during a visual-parity review. Relevant run-time dialogs
use the seeded repository and real constructors; standalone controls receive representative
refs, revisions, changes, patches, and text without opening external programs.

## Proof ledger and baseline

`portmap.json` records which source twins exist. `parity-ledger.json` records what has actually
been proven for each mapping on the structural, functional, visual, theming/color,
behavioral/state, and platform axes. Its versioned contract is
`parity-ledger.schema.json`. A portmap entry may say `parity` only when its ledger entry is
complete and every applicable axis is `verified`.

Regenerate the deterministic functional baseline and ledger from the repository root:

```powershell
dotnet run --project eng/tools/ParityInventory -- sweep `
  --portmap eng/avalonia/portmap.json `
  --original-root src/app/GitUI `
  --twin-root src/app/GitUI.Avalonia `
  --translations src/app/GitUI/Translation/English.xlf `
  --analyzed-commit (git rev-parse HEAD) `
  --output eng/avalonia/parity-evidence/P0.5/functional-findings.json

dotnet run --project eng/tools/ParityInventory -- ledger `
  --portmap eng/avalonia/portmap.json `
  --functional eng/avalonia/parity-evidence/P0.5/functional-findings.json `
  --visual eng/avalonia/parity-evidence/P0.3/findings.json `
  --reference eng/avalonia/parity-evidence/P0.1/winforms/manifest.json `
  --analyzed-commit (git rev-parse HEAD) `
  --verified-on 2026-07-29 `
  --ledger-output eng/avalonia/parity-ledger.json `
  --baseline-output eng/avalonia/parity-evidence/P0.5/baseline-report.json
```

The evidence folder is local and excluded from Git. The ledger is committed so stale or
missing proof cannot be mistaken for parity.
