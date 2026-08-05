<!-- L2 CORE PLATFORM. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# Release Pipeline (L2)

**TL;DR:** Pushing a `v[0-9]*` tag triggers [app-release.yml](../../../.github/workflows/app-release.yml).
It calls the reusable [_app-build-core.yml](../../../.github/workflows/_app-build-core.yml) with
`release=true` on an **x64 + arm64** matrix to build and package MSI + portable artifacts, then
submits them to **SignPath** for code signing. Releases only run from the upstream
`gitextensions` repo, never forks.

**Related:** [ci-workflows](ci-workflows.md) · [build-and-installer](build-and-installer.md) · [translation-system](translation-system.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Release and CI must build **identically** — the only difference is that release *packages and signs*
instead of *testing*. So both share one reusable core workflow toggled by a `release` boolean, and
the release wrapper adds only the signing hand-off. Signing is external (SignPath) so the private
signing key never touches the CI runners.

## What (`app-release.yml`)

Two jobs:

| Job | Purpose |
| --- | --- |
| `build` | Matrix `[windows-latest, windows-11-arm]`, calls `_app-build-core.yml` with `release: true`. Gated by `github.repository_owner == 'gitextensions'`. |
| `trigger-signing` | Downloads the packaged artifacts and submits a SignPath signing request. Gated additionally by `vars.ARTIFACT_SIGNING_ENABLED == 'true'`. |

## How the core packages a release (`release=true`)

In [_app-build-core.yml](../../../.github/workflows/_app-build-core.yml), `release=true` changes the
role of the shared job:

- **Install WiX** + **`Mark-RepoClean.ps1`** run only for release ([Mark-RepoClean.ps1](../../../eng/Mark-RepoClean.ps1)).
- **Resolve build metadata** (`resolve` step) sets the version — see below.
- **Build native → build .NET → verify loc → publish** (same as CI).
- Instead of `dotnet test`, it uploads the **distribution files**: `GE-v<version>-<arch>-dist`
  containing `*.msi` + `*.zip` from `artifacts/Release/publish/`.

## Versioning (`resolve` step)

- Base version is `BUILD_VERSION_BASE` (`7.3.0`), joined with `github.run_number` → e.g. `7.3.0.123`.
- Informational suffix by ref:
  - release build → **no suffix** (stable),
  - `refs/heads/release` or `release/**` → `-preview`,
  - anything else → `-dev`.
- **arm64 legs** append `-arm64` and set `TargetPlatform=arm64`, so portable/MSI file names don't
  collide with the x64 leg (`GitExtensions-Portable-<platform>-...`).
- Outputs `version` / `version-text` are exposed as workflow outputs for the signing job to name
  artifacts consistently. `eng/set_version_to.cs` stamps the resolved version into the build.

## Signing (`trigger-signing`)

1. Downloads all packaged artifacts and re-uploads them as `GE-v<version>-unsigned`.
2. POSTs a signing request to the SignPath GitHub Actions connector, passing the `artifactId`, run
   id, repo, and `GITHUB_TOKEN` (so SignPath pulls the artifact directly from Actions).
3. Uses project slug `GitExtensions_combined` and signing policy `release-2020`. The API token comes
   from `secrets.SIGNPATH_API_TOKEN`.

## Hard rules

- Releases are cut by **pushing a `v…` tag** to the upstream repo — never by editing artifacts by hand.
- Bump `BUILD_VERSION_BASE` in `_app-build-core.yml` (single source of truth) when the major/minor changes.
- The `repository_owner` guard **MUST** stay — forks must never produce signed releases.
- Signing secrets (`SIGNPATH_API_TOKEN`) live in repo secrets; never inline them in a workflow.

**Next:** [ci-workflows](ci-workflows.md) for the PR gates.
