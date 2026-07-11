![Git Extensions logo](setup/assets/Logo/git-extensions-logo.svg)

# Git Extensions Cross Platform

**Git Extensions Cross Platform** is an independent fork of
[Git Extensions](https://github.com/gitextensions/gitextensions), the standalone Windows UI
tool for managing git repositories.

## The challenge

Git Extensions is a mature, feature-rich git client — but it is built on Windows Forms, which
ties it to Windows forever. The goal of this fork is to **migrate the UI to
[Avalonia](https://avaloniaui.net/)** so the application runs natively on **Windows, Linux and
macOS**.

This is not a form-by-form port: the presentation layer (~142K lines of WinForms code) will be
rewritten as a new Avalonia (MVVM) shell that reuses the existing core (`GitCommands`,
`GitExtUtils`, the plugin infrastructure and the revision graph model). The original WinForms
application keeps building and running throughout the migration and serves as the functional
reference until the new shell reaches parity.

The journey started on **July 11, 2026**, from the upstream stable tag `v7.2.0`.

## Status

🚧 **Early days.** The migration is being developed incrementally, phase by phase:

1. **Phase 0 — Foundations**: decouple the core from WinForms and prove it builds and passes
   its tests on Linux.
2. **Phase 1 — Walking skeleton**: a minimal Avalonia app that opens a repository.
3. **Phase 2 — Vertical slice**: commit graph and diff viewer (read-only browsing).
4. **Phase 3 — Write operations**: commit, push/pull, branches…
5. **Phase 4 — Platform & parity**: localization, settings, packaging per OS.

## Building

Requirements: [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and git.

```powershell
git clone --recurse-submodules https://github.com/angazo/gitextensionscp.git
cd gitextensionscp
dotnet build
```

The resulting (still WinForms, Windows-only) application is at
`artifacts/Debug/bin/GitExtensions/net10.0-windows/GitExtensions.exe`.

## Credits

This project stands on the shoulders of the
[Git Extensions](https://github.com/gitextensions/gitextensions) team and its
[contributors](https://github.com/gitextensions/gitextensions/graphs/contributors) — thank you
for two decades of work on an outstanding git client. Upstream resources:

* Original repository: [github.com/gitextensions/gitextensions](https://github.com/gitextensions/gitextensions)
* Online manual: [git-extensions-documentation.readthedocs.org](https://git-extensions-documentation.readthedocs.org/)

Icons by [Yusuke Kamiyamane](http://p.yusukekamiyamane.com/)
([CCA/3.0](http://creativecommons.org/licenses/by/3.0/)).

## License

Same license as the original project — see [LICENSE.md](LICENSE.md).
