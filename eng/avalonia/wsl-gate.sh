#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: keeps Linux build and runtime evidence reproducible until the parity gate closes.

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
source_root="$(git -C "$script_dir" rev-parse --show-toplevel)"
mirror_parent="${GITEXTENSIONS_WSL_MIRROR_PARENT:-$HOME/src}"
mirror_root="${GITEXTENSIONS_WSL_MIRROR:-$mirror_parent/gitextensions-wsl}"
marker_name="gitextensions-wsl-mirror-source"
evidence_root="$source_root/eng/avalonia/parity-evidence/P0.6"

usage()
{
    cat <<'EOF'
Usage:
  eng/avalonia/wsl-gate.sh sync
  eng/avalonia/wsl-gate.sh build
  eng/avalonia/wsl-gate.sh test <dotnet-test-filter>
  eng/avalonia/wsl-gate.sh gate <dotnet-test-filter>
  eng/avalonia/wsl-gate.sh runtime
  eng/avalonia/wsl-gate.sh flatpak
  eng/avalonia/wsl-gate.sh all <dotnet-test-filter>
  eng/avalonia/wsl-gate.sh run <command> [arguments...]

Run this copy from the authoritative Windows checkout through WSL. It synchronizes source
into a guarded native-ext4 mirror while retaining Linux bin, obj, artifacts, and Flatpak
build caches. It never edits, commits, or pushes from the mirror.
EOF
}

ensure_source()
{
    case "$source_root" in
        /mnt/*) ;;
        *)
            echo "error: invoke this script from the authoritative Windows checkout under /mnt" >&2
            exit 2
            ;;
    esac
}

ensure_mirror()
{
    case "$mirror_root" in
        "$mirror_parent"/*) ;;
        *)
            echo "error: refusing unsafe mirror target '$mirror_root'" >&2
            exit 2
            ;;
    esac

    if [[ ! -d "$mirror_root/.git" ]]; then
        if [[ -e "$mirror_root" ]]; then
            echo "error: refusing to replace non-mirror path '$mirror_root'" >&2
            exit 2
        fi

        mkdir -p "$mirror_parent"
        git -c core.autocrlf=true -c core.safecrlf=false \
            clone --no-local --origin source "$source_root" "$mirror_root"
        git -C "$mirror_root" config core.autocrlf true
        git -C "$mirror_root" config core.safecrlf false
        git -C "$mirror_root" config core.fileMode false
        printf '%s\n' "$source_root" > "$mirror_root/.git/$marker_name"
    fi

    marker_path="$mirror_root/.git/$marker_name"
    if [[ ! -f "$marker_path" ]] || [[ "$(<"$marker_path")" != "$source_root" ]]; then
        echo "error: refusing to synchronize unrecognized target '$mirror_root'" >&2
        exit 2
    fi
}

sync_source()
{
    ensure_source
    ensure_mirror

    source_branch="$(git -C "$source_root" symbolic-ref --quiet --short HEAD)"
    source_head="$(git -C "$source_root" rev-parse HEAD)"
    mirror_branch="$(git -C "$mirror_root" symbolic-ref --quiet --short HEAD)"
    if [[ "$source_branch" != "$mirror_branch" ]]; then
        echo "error: source branch '$source_branch' does not match mirror branch '$mirror_branch'" >&2
        exit 2
    fi

    git -C "$mirror_root" remote set-url source "$source_root"
    git -C "$mirror_root" fetch --quiet source "$source_branch"
    git -C "$mirror_root" update-ref "refs/heads/$mirror_branch" "$source_head"
    git -C "$mirror_root" read-tree "$source_head"
    rsync -a --delete \
        --exclude=/.git/ \
        --exclude='**/.git' \
        --exclude=/.vs/ \
        --exclude=/.idea/ \
        --exclude=/artifacts/ \
        --exclude=/eng/avalonia/parity-evidence/ \
        --exclude=/eng/avalonia/parity-shots/ \
        --exclude=/eng/avalonia/flatpak/publish/ \
        --exclude='**/bin/' \
        --exclude='**/obj/' \
        "$source_root/" "$mirror_root/"
}

build_solution()
{
    cd "$mirror_root"
    dotnet restore GitExtensions.Avalonia.slnx --force-evaluate -v:minimal
    dotnet build GitExtensions.Avalonia.slnx -m:1 --no-restore -v:minimal
}

run_tests()
{
    local filter=$1

    cd "$mirror_root"
    dotnet test tests/app/UnitTests/GitUI.Avalonia.Tests/GitUI.Avalonia.Tests.csproj \
        -p:BuildAvalonia=true \
        --filter "$filter" \
        -m:1 \
        --no-restore \
        -v:minimal
}

run_runtime_smokes()
{
    mkdir -p "$evidence_root"
    cd "$mirror_root"
    GITEXTENSIONS_CAPTURE_SCRIPT="$source_root/eng/avalonia/Capture-SmokeWindow.ps1" \
        eng/avalonia/linux-runtime-smoke.sh wayland "$evidence_root/wayland"
    GITEXTENSIONS_CAPTURE_SCRIPT="$source_root/eng/avalonia/Capture-SmokeWindow.ps1" \
        eng/avalonia/linux-runtime-smoke.sh x11 "$evidence_root/x11"
}

run_flatpak_smoke()
{
    mkdir -p "$evidence_root"
    cd "$mirror_root"
    GITEXTENSIONS_CAPTURE_SCRIPT="$source_root/eng/avalonia/Capture-SmokeWindow.ps1" \
        eng/avalonia/flatpak/build-and-smoke.sh "$evidence_root/flatpak"
}

command_name=${1:-}
case "$command_name" in
    sync)
        sync_source
        ;;
    build)
        sync_source
        build_solution
        ;;
    test)
        [[ $# -eq 2 ]] || { usage >&2; exit 2; }
        sync_source
        run_tests "$2"
        ;;
    gate)
        [[ $# -eq 2 ]] || { usage >&2; exit 2; }
        sync_source
        build_solution
        run_tests "$2"
        ;;
    runtime)
        [[ $# -eq 1 ]] || { usage >&2; exit 2; }
        sync_source
        build_solution
        run_runtime_smokes
        ;;
    flatpak)
        [[ $# -eq 1 ]] || { usage >&2; exit 2; }
        sync_source
        run_flatpak_smoke
        ;;
    all)
        [[ $# -eq 2 ]] || { usage >&2; exit 2; }
        sync_source
        build_solution
        run_tests "$2"
        run_runtime_smokes
        run_flatpak_smoke
        ;;
    run)
        [[ $# -ge 2 ]] || { usage >&2; exit 2; }
        shift
        sync_source
        cd "$mirror_root"
        exec "$@"
        ;;
    -h|--help|help)
        usage
        ;;
    *)
        usage >&2
        exit 2
        ;;
esac
