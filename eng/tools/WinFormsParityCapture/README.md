# WinForms parity capture

`WinFormsParityCapture` is the Windows-only reference half of the Avalonia parity toolchain.
It loads the real `GitUI` forms, captures their resolved control trees and pixels, and records
the state, theme, DPI, DPI acquisition mode, and image API used for every artifact.

The public command does not load `AppSettings`. It copies the built runtime to a disposable
directory and starts a worker there in portable mode. Consequently, `GitExtensions.settings`,
the custom capture theme, and all other worker state stay under that disposable directory.
The custom theme is staged as local/preinstalled for that isolated process, so it is never
resolved from or written to the user themes directory. The supplied repository must also be
a throwaway repository outside the current working tree and contain at least one commit.
Repository-host fixtures use `HEAD` as both sides of their deterministic comparison when that
commit has no parent, so a valid object id is always supplied without requiring extra history.

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
exact native monitor and otherwise drives the complete Per-Monitor-v2 transition that Windows
sends to the real WinForms window: `WM_DPICHANGED_BEFOREPARENT` traverses the child HWND tree
bottom-up, the top-level window receives `WM_DPICHANGED`, then
`WM_DPICHANGED_AFTERPARENT` traverses the child tree top-down. Because the fallback cannot
change the physical monitor context inherited by HWNDs that WinForms creates or recreates during
that transition, it gives those late child subtrees the same before/after callbacks and checks
again after applying the requested capture state. The fallback refuses to emit a capture if any
managed child window still does not report the requested DPI. It never
stretches a bitmap and never calls `Control.Scale`. Every successful tree names either
`nativeMonitor` or `dpiChangeMessage`; unsupported states are manifest-only entries with a
reason and `captureMethod` set to `unsupported`. A pre-change per-control font baseline lets
the reader normalize only runtime fonts actually scaled by WinForms' DPI handler; explicit
fonts that remain unchanged keep their authored size. This matches the existing pixel-to-DIP
normalization for control geometry. For the message fallback, the suggested window rectangle
scales the measured client area with WinForms' integer DPI rounding and retains the actual
non-client inset. Sending the DPI-change message does not move native chrome onto a differently
scaled monitor, so scaling that unchanged inset would overstate the product client. The retained full
window image and `dpiChangeMessage` provenance keep this fallback distinguishable from native
monitor evidence. Isolation cleanup retries transient executable locks from a just-exited worker
or antivirus scanner, and a cleanup error never hides the original capture failure.

The shared `eng/tools/ParityCaptureSchema` project targets plain `net10.0` and has no UI or
Windows dependency. P0.2 will deliberately reference that one schema project from the
Avalonia test build graph so both capture implementations serialize the same contract. The
Windows-only capture tool itself remains outside both solutions.
