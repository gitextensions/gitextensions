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

## Cross-platform development gate

P0.6 keeps the Windows checkout authoritative while building and testing from a native-ext4
WSL mirror. Invoke the committed helper through Ubuntu, not from the mirror and not through a
container:

```console
sudo apt-get update
sudo apt-get install libice6 libsm6 libfontconfig1 libwayland-client0 libxkbcommon0 libegl1 \
  rsync lsof weston wayland-utils xserver-xephyr x11-apps xdotool imagemagick \
  flatpak flatpak-builder
```

```powershell
.\eng\avalonia\windows-runtime-smoke.ps1

wsl -d Ubuntu -- bash -lc `
  '/mnt/c/path/to/gitextensions/eng/avalonia/wsl-gate.sh gate TestCategory=P0_6'

wsl -d Ubuntu -- bash -lc `
  '/mnt/c/path/to/gitextensions/eng/avalonia/wsl-gate.sh runtime'
```

The Windows command launches from a disposable portable runtime and captures the prerequisite
checklist without writing the real settings or registry. The WSL `gate` command synchronizes,
restores, builds, and runs the focused Linux tests. The `runtime` command builds once, launches
a real temporary-repository Browse window through native Wayland and through X11, records the
loaded backend packages, and proves the active path through an exclusive compositor-native
capture. It writes isolated evidence below `eng/avalonia/parity-evidence/P0.6/`. The Linux
smokes use temporary XDG settings and never read or write the operator's Git Extensions
configuration.

WSLg currently advertises `xdg_wm_base` version 1 on this machine, while Avalonia requires
version 3 or later. The harness therefore hosts the native Wayland process in nested Weston
when the session compositor is too old. Its X11 leg uses nested Xephyr so the rootless WSLg
surface can be captured reliably. Neither leg bitmap-scales or substitutes a headless render.

The development Flatpak is deliberately separate from release packaging:

```powershell
wsl -d Ubuntu -- bash -lc `
  '/mnt/c/path/to/gitextensions/eng/avalonia/wsl-gate.sh flatpak'
```

It publishes a self-contained Linux build, packages it against the Freedesktop 25.08 SDK,
installs it for the current user, and launches it confined with access only to the dedicated
`~/.local/share/gitextensions-parity-smoke` fixture. The manifest exposes native Wayland but
no X11 socket; the smoke explicitly revokes both X11 socket types and captures the application
through nested Weston. P10 owns the release manifest. The macOS leg is recorded in
`macos-smoke-checklist.md` until a macOS runner is available.

On a WSL installation whose ext4 VHD has remounted read-only, stop and restart WSL before
continuing. If the failure repeats only during RID publish, cross-publish `linux-x64` from
Windows and set `GITEXTENSIONS_FLATPAK_PREBUILT` to its WSL path; Flatpak packaging, launch,
confinement, and capture still run in WSL.
