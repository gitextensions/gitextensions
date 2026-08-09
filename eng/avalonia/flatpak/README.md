# Development Flatpak

This directory contains the P0.6 development-only Flatpak manifest. It packages a
self-contained build against the Freedesktop 25.08 SDK so Git is available inside the
development sandbox. The release manifest, runtime selection, metadata, and distribution
policy belong to P10.

The manifest grants the application network, native Wayland, GPU, and host-filesystem access
through `--filesystem=host`. It exposes no X11 socket. The smoke builds a dedicated
`com.github.gitextensions.GitExtensions.Avalonia.P73Smoke` application ID so Flatpak's own
`~/.var/app/<app-id>` settings and data roots remain isolated from the development package. It
uses the `~/.local/share/gitextensions-parity-smoke` repository fixture, launches inside a
nested Weston session, explicitly revokes both X11 socket types, verifies the installed
filesystem permission, and captures the rendered window through that compositor. Build and
run it through `build-and-smoke.sh`; generated publish and builder output remain ignored.

parity-scaffolding: the development manifest and smoke are replaced by release packaging in P10.
