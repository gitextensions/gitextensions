# Flatpak release candidate

`com.github.gitextensions.GitExtensions.Avalonia.json` is the release-shaped candidate used
for confined development verification. It packages the self-contained application, Git
2.51.0, OpenSSH 10.0p2, the desktop entry, AppStream metainfo, icon, application themes, and
the Freedesktop 25.08 runtime. The application is Wayland-only and receives network, GPU,
SSH-agent, and `--filesystem=host` access; it has no X11 socket. P10 performs the final runtime
and dependency re-pin, clean-checkout reproducibility check, and distribution decision.

From a native Linux checkout, build and smoke an isolated application ID with:

```console
bash eng/avalonia/flatpak/build-and-smoke.sh /tmp/gitextensions-flatpak-smoke
```

For faster repeated runs, publish once to a directory distinct from the script's staging
directory and pass it as prebuilt input:

```console
dotnet publish src/app/GitExtensions.Avalonia/GitExtensions.Avalonia.csproj \
  -c Release -p:BuildAvalonia=true -r linux-x64 --self-contained true -m:1 \
  --output /tmp/gitextensions-flatpak-publish
GITEXTENSIONS_FLATPAK_PREBUILT=/tmp/gitextensions-flatpak-publish \
  bash eng/avalonia/flatpak/build-and-smoke.sh /tmp/gitextensions-flatpak-smoke
```

The smoke uses `com.github.gitextensions.GitExtensions.Avalonia.P83Smoke`, a throwaway
repository under `~/.local/share`, isolated Flatpak settings, an inaccessible-plugin probe,
a custom theme, and a nested Wayland compositor. It validates the installed permissions and
captures the compositor-rendered window. Generated publish and builder output remain ignored.

After the smoke has installed that isolated ID, run every confined host-facing action:

```console
bash eng/avalonia/flatpak/confined-action-sweep.sh /tmp/gitextensions-flatpak-actions
```

The action sweep drives the application's XDG file and open portals (including cancel), an
SSH push through the forwarded `ssh-agent`, a custom mergetool, sandbox-local Git credential
storage/fill protocol, both theme roots, plugin-directory behavior, and product process-group
cancellation. It does not claim that arbitrary host HTTPS credential-helper executables or
Secret Service providers are available inside the sandbox. External
terminals and host Gource executables are explicitly reported as unavailable because
`--filesystem=host` grants visibility, not permission to execute arbitrary host binaries.

Both harnesses are parity scaffolding and use only disposable application data and
repositories outside the working tree.
