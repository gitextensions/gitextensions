#!/usr/bin/env bash
set -euo pipefail

publish_directory=${1:-}
output_bundle=${2:-}
flatpak_scope=${GITEXTENSIONS_FLATPAK_SCOPE:-system}

if [[ -z "$publish_directory" || -z "$output_bundle" ]]; then
    echo "usage: build-bundle.sh <publish-directory> <output-bundle>" >&2
    exit 2
fi

for tool in appstreamcli desktop-file-validate flatpak flatpak-builder git mktemp ostree; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "error: required command '$tool' is not installed" >&2
        exit 1
    fi
done

if [[ ! -x "$publish_directory/GitExtensions.Avalonia" ]]; then
    echo "error: publish directory does not contain the GitExtensions.Avalonia executable" >&2
    exit 1
fi

case "$flatpak_scope" in
    system)
        scope_arguments=()
        ;;
    user)
        scope_arguments=(--user)
        ;;
    *)
        echo "error: GITEXTENSIONS_FLATPAK_SCOPE must be 'system' or 'user'" >&2
        exit 2
        ;;
esac

repo_root=$(git rev-parse --show-toplevel)
flatpak_root="$repo_root/eng/avalonia/flatpak"
app_id="com.github.gitextensions.GitExtensions.Avalonia"
work_directory=$(mktemp -d)
cleanup()
{
    rm -rf -- "$work_directory"
}
trap cleanup EXIT

stage_directory="$work_directory/stage"
build_directory="$work_directory/build"
repository_directory="$work_directory/repository"
state_directory="$work_directory/state"
import_directory="$work_directory/import-check"
mkdir -p "$stage_directory/publish"
cp -a "$publish_directory/." "$stage_directory/publish/"
sed 's/\r$//' "$flatpak_root/$app_id.desktop" > "$stage_directory/$app_id.desktop"
sed 's/\r$//' "$flatpak_root/$app_id.metainfo.xml" > "$stage_directory/$app_id.metainfo.xml"
cp "$repo_root/setup/assets/Logo/git-extensions-logo-512px.png" "$stage_directory/"

sed \
    -e 's/\r$//' \
    -e 's#"path": "../../../setup/assets/Logo/git-extensions-logo-512px.png"#"path": "git-extensions-logo-512px.png"#' \
    "$flatpak_root/$app_id.json" > "$stage_directory/$app_id.json"

desktop-file-validate "$stage_directory/$app_id.desktop"
appstreamcli validate --no-net "$stage_directory/$app_id.metainfo.xml"

flatpak-builder \
    "${scope_arguments[@]}" \
    --disable-rofiles-fuse \
    --force-clean \
    --install-deps-from=flathub \
    --default-branch=stable \
    --repo="$repository_directory" \
    --state-dir="$state_directory" \
    "$build_directory" \
    "$stage_directory/$app_id.json"

mkdir -p "$(dirname "$output_bundle")"
rm -f -- "$output_bundle"
flatpak build-bundle "$repository_directory" "$output_bundle" "$app_id" stable

ostree init --repo="$import_directory" --mode=archive-z2
flatpak build-import-bundle --update-appstream "$import_directory" "$output_bundle" >/dev/null
if ! ostree --repo="$import_directory" refs | grep -Fx "app/$app_id/$(uname -m)/stable" >/dev/null; then
    echo "error: Flatpak bundle does not contain the expected stable application ref" >&2
    exit 1
fi
