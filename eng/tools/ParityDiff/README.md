# Parity diff

`ParityDiff` is the cross-platform comparison half of the parity capture toolchain. It joins a
WinForms reference manifest and an Avalonia candidate manifest by component, theme, scale, and
state, then joins controls by their stable field names and aliases. Exact schema metrics and
resolved colors are compared before pixels.

The tool writes deterministic `findings.json` and a human-readable `report.md`. It records
unavailable or unsupported captures and their manifest notes explicitly; it never treats them
as successful comparisons. Image comparison uses global luminance SSIM over the union canvas,
plus declared per-pixel and maximum-channel-delta budgets. Tolerance values live in
`parity-diff.json`, with deliberate per-component overrides. Resolved colors always have zero
tolerance.

```powershell
dotnet run --project eng/tools/ParityDiff -- compare `
  --reference eng/avalonia/parity-shots/winforms/manifest.json `
  --candidate eng/avalonia/parity-shots/avalonia/manifest.json `
  --config eng/tools/ParityDiff/parity-diff.json `
  --output eng/avalonia/parity-evidence/P0.3
```

The tool and its test project belong to neither repository solution.
