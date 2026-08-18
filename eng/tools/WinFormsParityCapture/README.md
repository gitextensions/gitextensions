# WinForms parity capture

`WinFormsParityCapture` is the Windows-only reference half of the Avalonia parity toolchain.
It loads the real `GitUI` forms, captures their resolved control trees and pixels, and records
the state, theme, DPI, DPI acquisition mode, and image API used for every artifact.

The public command does not load `AppSettings`. It copies the built runtime to a disposable
directory and starts a worker there in portable mode. Consequently, `GitExtensions.settings`,
the custom capture theme, and all other worker state stay under that disposable directory.
The custom theme is staged as local/preinstalled for that isolated process, so it is never
resolved from or written to the user themes directory. The supplied repository must also be
a throwaway repository outside the current working tree.

```powershell
dotnet run --project eng/tools/WinFormsParityCapture -c Release -- capture `
  --plan eng/tools/WinFormsParityCapture/capture-plan.json `
  --repository C:\path\to\throwaway-repository `
  --output eng/avalonia/parity-evidence/P0.1/winforms

dotnet run --project eng/tools/WinFormsParityCapture -c Release -- validate `
  --manifest eng/avalonia/parity-evidence/P0.1/winforms/manifest.json `
  --round-trip `
  --require-resolved-argb
```

At 100%, a genuine 96-DPI monitor is mandatory. At 125%, 150%, and 200%, the tool prefers an
exact native monitor and otherwise sends `WM_DPICHANGED` to the real WinForms window. It never
stretches a bitmap and never calls `Control.Scale`. Every successful tree names either
`nativeMonitor` or `dpiChangeMessage`; unsupported states are manifest-only entries with a
reason and `captureMethod` set to `unsupported`.

The shared `eng/tools/ParityCaptureSchema` project targets plain `net10.0` and has no UI or
Windows dependency. P0.2 will deliberately reference that one schema project from the
Avalonia test build graph so both capture implementations serialize the same contract. The
Windows-only capture tool itself remains outside both solutions.
