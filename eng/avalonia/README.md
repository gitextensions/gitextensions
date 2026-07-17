# Avalonia visual-parity captures

The Avalonia test project can render every ported AXAML window and control with headless
Skia. It discovers views from `src/app/GitUI.Avalonia` automatically, creates a temporary
sample repository, and captures each view in light and dark mode.

Regenerate the local review set from the repository root:

```console
GITEXT_CAPTURE_PARITY_SHOTS=1 dotnet test tests/app/UnitTests/GitUI.Avalonia.Tests/GitUI.Avalonia.Tests.csproj --filter 'Category=VisualParityCapture'
```

The command replaces `eng/avalonia/parity-shots/` and writes a PNG for each theme/view plus
`manifest.json`. The folder is intentionally excluded in `.git/info/exclude`: captures are
developer artifacts, not golden files. Normal test runs do not regenerate them, but they do
verify that every AXAML view resolves to a control with its required public parameterless
constructor.

Place owner-supplied WinForms screenshots in the likewise excluded
`eng/avalonia/parity-reference/` folder. Compare those references with the generated browse,
diff, commit, and dialog captures during a visual-parity review. Relevant run-time dialogs
use the seeded repository and real constructors; standalone controls receive representative
refs, revisions, changes, patches, and text without opening external programs.
