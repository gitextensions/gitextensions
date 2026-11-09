# Flatpak release candidate

`com.github.gitextensions.GitExtensions.Avalonia.json` is the release-shaped candidate used
for confined development verification. It packages the self-contained application, Git
2.51.0, OpenSSH 10.0p2, the desktop entry, AppStream metainfo, icon, application themes, and
the Freedesktop 25.08 runtime. The application is Wayland-only and receives network, GPU,
SSH-agent, `--filesystem=host` access, and host-command permission
(`--talk-name=org.freedesktop.Flatpak`); it has no X11 socket. P10 performs the final runtime
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
terminals use `flatpak-spawn --host --watch-bus` and the host's PATH. Revoking host-command
permission produces an explicit unavailable error. Configured Gource uses the same host
launcher; its avatar directory comes from the host-visible temporary storage selected at startup.

The package keeps stock Git under `/app/libexec/git-core`. Its `/app/bin/git` launcher selects
a private helper overlay for external editor and diff/merge launches; saved Git configuration
is unchanged and remains usable by host Git. At startup, known host tools are queried through
the host service and temporary storage is accepted only after a host read of the same probe
file succeeds. App-local launch proxies are never stored in the user's Git configuration.

Both harnesses are parity scaffolding and use only disposable application data and
repositories outside the working tree.
