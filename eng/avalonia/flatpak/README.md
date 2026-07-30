# Development Flatpak

This directory contains the P0.6 development-only Flatpak manifest. It packages a
self-contained build against the Freedesktop 25.08 SDK so Git is available inside the
development sandbox. The release manifest, runtime selection, metadata, and distribution
policy belong to P10.

The manifest grants the application only network, native Wayland, GPU, and the dedicated
`~/.local/share/gitextensions-parity-smoke` fixture used by the confined smoke. It exposes no
X11 socket. The smoke launches inside a nested Weston session, explicitly revokes both X11
socket types, and captures the rendered window through that compositor. Build and run it
through `build-and-smoke.sh`; generated publish and builder output remain ignored.

parity-scaffolding: the development manifest and smoke are replaced by release packaging in P10.
