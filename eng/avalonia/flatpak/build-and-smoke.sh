#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: validates development confinement before release packaging replaces it.

evidence_dir=${1:-}
if [[ -z "$evidence_dir" ]]; then
    echo "usage: build-and-smoke.sh <evidence-directory>" >&2
    exit 2
fi

for tool in dotnet flatpak flatpak-builder git weston weston-screenshooter; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "error: required command '$tool' is not installed" >&2
        exit 1
    fi
done

repo_root="$(git rev-parse --show-toplevel)"
flatpak_root="$repo_root/eng/avalonia/flatpak"
manifest="$flatpak_root/com.github.gitextensions.GitExtensions.Avalonia.json"
publish_root="$flatpak_root/publish"
flatpak_cache_root="$HOME/.cache/gitextensions-avalonia/p83"
build_root="$flatpak_cache_root/build"
state_root="$flatpak_cache_root/state"
smoke_manifest="$flatpak_cache_root/smoke-manifest.json"
packaging_root="$flatpak_cache_root/packaging"
package_app_id="com.github.gitextensions.GitExtensions.Avalonia"
app_id="com.github.gitextensions.GitExtensions.Avalonia.P83Smoke"
flatpak_home="$HOME/.var/app/$app_id"
smoke_parent="$HOME/.local/share/gitextensions-parity-smoke"
smoke_repo="$smoke_parent/repository"
data_root="$flatpak_home/data"
user_plugins_directory="$data_root/GitExtensions/UserPlugins.Avalonia"
unreachable_user_plugins_path="$user_plugins_directory"

case "$publish_root" in
    "$flatpak_root"/publish) ;;
    *)
        echo "error: refusing unsafe publish path '$publish_root'" >&2
        exit 2
        ;;
esac
case "$smoke_parent" in
    "$HOME"/.local/share/gitextensions-parity-smoke) ;;
    *)
        echo "error: refusing unsafe smoke path '$smoke_parent'" >&2
        exit 2
        ;;
esac
case "$flatpak_cache_root" in
    "$HOME"/.cache/gitextensions-avalonia/p83) ;;
    *)
        echo "error: refusing unsafe Flatpak cache path '$flatpak_cache_root'" >&2
        exit 2
        ;;
esac
case "$flatpak_home" in
    "$HOME"/.var/app/com.github.gitextensions.GitExtensions.Avalonia.P83Smoke) ;;
    *)
        echo "error: refusing unsafe Flatpak home path '$flatpak_home'" >&2
        exit 2
        ;;
esac

settings_directory="$flatpak_home/config/GitExtensions/GitExtensions"
rm -rf -- "$flatpak_home" "$smoke_parent"
mkdir -p \
    "$evidence_dir" \
    "$publish_root" \
    "$build_root" \
    "$state_root" \
    "$packaging_root" \
    "$smoke_repo" \
    "$settings_directory" \
    "$data_root/GitExtensions"
find "$publish_root" -mindepth 1 -maxdepth 1 ! -name .gitignore -exec rm -rf -- {} +

cat > "$settings_directory/GitExtensions.settings" <<'EOF'
<?xml version="1.0" encoding="utf-8"?>
<dictionary>
  <item>
    <key><string>CheckSettings</string></key>
    <value><string>false</string></value>
  </item>
  <item>
    <key><string>translation</string></key>
    <value><string>English</string></value>
  </item>
  <item>
    <key><string>uitheme_v2</string></key>
    <value><string>P83Confined</string></value>
  </item>
  <item>
    <key><string>uithemeisbuiltin_v2</string></key>
    <value><string>false</string></value>
  </item>
</dictionary>
EOF

case "$user_plugins_directory" in
    "$flatpak_home"/data/GitExtensions/UserPlugins.Avalonia) ;;
    *)
        echo "error: refusing unsafe user-plugin fixture path '$user_plugins_directory'" >&2
        exit 2
        ;;
esac
case "$unreachable_user_plugins_path" in
    "$flatpak_home"/data/GitExtensions/UserPlugins.Avalonia) ;;
    *)
        echo "error: refusing unsafe inaccessible-plugin fixture path '$unreachable_user_plugins_path'" >&2
        exit 2
        ;;
esac
rm -rf -- "$unreachable_user_plugins_path"
printf 'inaccessible user-plugin fixture\n' > "$unreachable_user_plugins_path"

prebuilt_publish=${GITEXTENSIONS_FLATPAK_PREBUILT:-}
if [[ -n "$prebuilt_publish" ]]; then
    if [[ ! -x "$prebuilt_publish/GitExtensions.Avalonia" ]]; then
        echo "error: prebuilt Flatpak input does not contain an executable Linux publish" >&2
        exit 1
    fi
    cp -a "$prebuilt_publish/." "$publish_root/"
else
    dotnet publish "$repo_root/src/app/GitExtensions.Avalonia/GitExtensions.Avalonia.csproj" \
        -c Release \
        -p:BuildAvalonia=true \
        -r linux-x64 \
        --self-contained true \
        -m:1 \
        --output "$publish_root"
fi

mkdir -p "$settings_directory/Themes"
cp "$publish_root/Themes/dark.css" "$settings_directory/Themes/P83Confined.css"

desktop_source="$flatpak_root/$package_app_id.desktop"
metainfo_source="$flatpak_root/$package_app_id.metainfo.xml"
desktop="$packaging_root/$app_id.desktop"
metainfo="$packaging_root/$app_id.metainfo.xml"
logo="$repo_root/setup/assets/Logo/git-extensions-logo-512px.png"
sed "s/$package_app_id/$app_id/g" "$desktop_source" > "$desktop"
sed "s/$package_app_id/$app_id/g" "$metainfo_source" > "$metainfo"

sed \
    -e "s/$package_app_id/$app_id/g" \
    -e "s#\"path\": \"publish\"#\"path\": \"$publish_root\"#" \
    -e "s#\"path\": \"$app_id.desktop\"#\"path\": \"$desktop\"#" \
    -e "s#\"path\": \"$app_id.metainfo.xml\"#\"path\": \"$metainfo\"#" \
    -e "s#\"path\": \"../../../setup/assets/Logo/git-extensions-logo-512px.png\"#\"path\": \"$logo\"#" \
    "$manifest" > "$smoke_manifest"

git -C "$smoke_repo" init --quiet --initial-branch=main
git -C "$smoke_repo" config user.name "P0.6 Flatpak Smoke"
git -C "$smoke_repo" config user.email "p06-flatpak@example.invalid"
printf 'flatpak smoke\n' > "$smoke_repo/smoke.txt"
git -C "$smoke_repo" add smoke.txt
if ! git -C "$smoke_repo" rev-parse --verify HEAD >/dev/null 2>&1; then
    git -C "$smoke_repo" -c commit.gpgSign=false commit --quiet -m initial
fi

flatpak remote-add --user --if-not-exists flathub \
    https://dl.flathub.org/repo/flathub.flatpakrepo
flatpak-builder \
    --user \
    --disable-rofiles-fuse \
    --state-dir="$state_root" \
    --install-deps-from=flathub \
    --force-clean \
    --install \
    "$build_root" \
    "$smoke_manifest"

stdout_log="$evidence_dir/stdout.log"
stderr_log="$evidence_dir/stderr.log"
capture_log="$evidence_dir/capture.log"
screenshot="$evidence_dir/window.png"
manifest_output="$evidence_dir/smoke.json"
weston_log="$evidence_dir/weston.log"
backend_log="$evidence_dir/backend-evidence.txt"
permissions_log="$evidence_dir/flatpak-permissions.txt"
plugin_discovery_log="$evidence_dir/plugin-discovery.txt"
unreachable_stdout_log="$evidence_dir/plugin-unreachable-stdout.log"
unreachable_stderr_log="$evidence_dir/plugin-unreachable-stderr.log"
rm -f -- \
    "$stdout_log" \
    "$stderr_log" \
    "$capture_log" \
    "$screenshot" \
    "$manifest_output" \
    "$weston_log" \
    "$backend_log" \
    "$permissions_log" \
    "$plugin_discovery_log" \
    "$unreachable_stdout_log" \
    "$unreachable_stderr_log"

flatpak info --user --show-permissions "$app_id" >"$permissions_log"
filesystem_permissions="$(sed -n 's/^filesystems=//p' "$permissions_log")"
case ";$filesystem_permissions" in
    *";host;"*) ;;
    *)
        echo "error: installed Flatpak does not grant --filesystem=host" >&2
        cat "$permissions_log" >&2
        exit 1
        ;;
esac

outer_display=${DISPLAY:-}
if [[ -z "$outer_display" ]]; then
    echo "error: DISPLAY is not set for the nested Flatpak Wayland session" >&2
    exit 1
fi

weston_runtime="$smoke_parent/weston-runtime"
weston_socket="wayland-flatpak-p06"
mkdir -p "$weston_runtime"
chmod 700 "$weston_runtime"
rm -f -- "$weston_runtime/$weston_socket" "$weston_runtime/$weston_socket.lock"
env -u WAYLAND_DISPLAY \
    DISPLAY="$outer_display" \
    XDG_RUNTIME_DIR="$weston_runtime" \
    weston \
        --backend=x11-backend.so \
        --socket="$weston_socket" \
        --width=1280 \
        --height=900 \
        --renderer=pixman \
        --idle-time=0 \
        --debug \
        --log="$weston_log" >>"$weston_log" 2>&1 &
weston_pid=$!
for _ in {1..30}; do
    [[ -S "$weston_runtime/$weston_socket" ]] && break
    if ! kill -0 "$weston_pid" 2>/dev/null; then
        wait "$weston_pid" || true
        echo "error: nested Flatpak Weston exited before its Wayland socket appeared" >&2
        sed -n '1,160p' "$weston_log" >&2
        exit 1
    fi
    sleep 1
done
if [[ ! -S "$weston_runtime/$weston_socket" ]]; then
    echo "error: nested Flatpak Weston did not create its Wayland socket" >&2
    exit 1
fi

flatpak_pid=
cleanup()
{
    flatpak kill "$app_id" 2>/dev/null || true
    if [[ -n "$flatpak_pid" ]]; then
        wait "$flatpak_pid" 2>/dev/null || true
    fi
    kill -TERM "$weston_pid" 2>/dev/null || true
    wait "$weston_pid" 2>/dev/null || true
}
trap cleanup EXIT

wait_for_flatpak_start()
{
    local process_id=$1
    for _ in {1..30}; do
        if ! kill -0 "$process_id" 2>/dev/null; then
            wait "$process_id" || true
            return 1
        fi
        if flatpak ps --columns=application | grep -Fx "$app_id" >/dev/null 2>&1; then
            return 0
        fi
        sleep 1
    done

    return 1
}

# First prove that an inaccessible UserPlugins.Avalonia entry does not abort confined startup.
flatpak run --user \
    --nosocket=x11 \
    --nosocket=fallback-x11 \
    --env=XDG_RUNTIME_DIR="$weston_runtime" \
    --env=WAYLAND_DISPLAY="$weston_socket" \
    --env=LIBGL_ALWAYS_SOFTWARE=1 \
    --env=MESA_LOADER_DRIVER_OVERRIDE=llvmpipe \
    --env=GALLIUM_DRIVER=llvmpipe \
    "$app_id" \
    browse \
    "$smoke_repo" >"$unreachable_stdout_log" 2>"$unreachable_stderr_log" &
flatpak_pid=$!
if ! wait_for_flatpak_start "$flatpak_pid"; then
    echo "error: confined application did not start with an inaccessible user-plugin directory" >&2
    sed -n '1,160p' "$unreachable_stderr_log" >&2
    exit 1
fi
sleep 3
if ! kill -0 "$flatpak_pid" 2>/dev/null; then
    echo "error: confined application exited during the inaccessible user-plugin probe" >&2
    sed -n '1,160p' "$unreachable_stderr_log" >&2
    exit 1
fi
if ! grep -Fq "User plugin discovery is disabled because '$unreachable_user_plugins_path' is inaccessible:" \
    "$unreachable_stderr_log"; then
    echo "error: confined application did not report the inaccessible user-plugin directory" >&2
    sed -n '1,160p' "$unreachable_stderr_log" >&2
    exit 1
fi
flatpak kill "$app_id" 2>/dev/null || true
wait "$flatpak_pid" 2>/dev/null || true
flatpak_pid=
for _ in {1..20}; do
    if ! flatpak ps --columns=application | grep -Fx "$app_id" >/dev/null 2>&1; then
        break
    fi
    sleep 0.25
done

rm -f -- "$unreachable_user_plugins_path"

flatpak run --user \
    --nosocket=x11 \
    --nosocket=fallback-x11 \
    --env=XDG_RUNTIME_DIR="$weston_runtime" \
    --env=WAYLAND_DISPLAY="$weston_socket" \
    --env=LIBGL_ALWAYS_SOFTWARE=1 \
    --env=MESA_LOADER_DRIVER_OVERRIDE=llvmpipe \
    --env=GALLIUM_DRIVER=llvmpipe \
    "$app_id" \
    browse \
    "$smoke_repo" >"$stdout_log" 2>"$stderr_log" &
flatpak_pid=$!
if ! wait_for_flatpak_start "$flatpak_pid"; then
    echo "error: confined application exited before capture" >&2
    sed -n '1,160p' "$stderr_log" >&2
    exit 1
fi
for _ in {1..20}; do
    [[ -d "$user_plugins_directory" ]] && break
    sleep 0.25
done
if [[ ! -d "$user_plugins_directory" ]] || [[ ! -r "$user_plugins_directory" ]]; then
    echo "error: confined application did not create a readable XDG user-plugin directory" >&2
    exit 1
fi

cat > "$plugin_discovery_log" <<EOF
directory=<XDG_DATA_HOME>/GitExtensions/UserPlugins.Avalonia
normal=created-and-readable
unreachable=bundled-plugins-only
unreachableDiagnostic=plugin-unreachable-stderr.log
winFormsUserPluginsFallback=false
EOF

printf '%s\n' \
    'selector=WAYLAND_DISPLAY present' \
    'flatpakSockets=wayland' \
    'runtimeOverrides=--nosocket=x11,--nosocket=fallback-x11' \
    'captureProvider=weston-screenshooter' >"$backend_log"

sleep 8
rm -f -- "$evidence_dir"/wayland-screenshot-*.png
(
    cd "$evidence_dir"
    XDG_RUNTIME_DIR="$weston_runtime" \
        WAYLAND_DISPLAY="$weston_socket" \
        weston-screenshooter
) >"$capture_log" 2>&1
generated_screenshots=("$evidence_dir"/wayland-screenshot-*.png)
if [[ ${#generated_screenshots[@]} -ne 1 ]] || [[ ! -f "${generated_screenshots[0]}" ]]; then
    echo "error: Weston did not produce exactly one confined screenshot" >&2
    exit 1
fi
mv -- "${generated_screenshots[0]}" "$screenshot"

if [[ ! -s "$screenshot" ]]; then
    echo "error: confined screenshot provider produced no image" >&2
    exit 1
fi
if grep -Eiq 'Unhandled exception|fatal error|JIT debugger|Avalonia.*error' "$stdout_log" "$stderr_log"; then
    echo "error: confined runtime logs contain a failure signature" >&2
    sed -n '1,160p' "$stderr_log" >&2
    exit 1
fi

screenshot_sha256="$(sha256sum "$screenshot" | cut -d' ' -f1)"
cat > "$manifest_output" <<EOF
{
  "schemaVersion": 1,
  "appId": "$app_id",
  "packageAppId": "$package_app_id",
  "runtime": "org.freedesktop.Platform//25.08",
  "confined": true,
  "backend": "wayland",
  "backendAssembly": "Avalonia.Wayland.dll",
  "backendEvidence": "Wayland-only sandbox plus compositor-native screenshot",
  "sessionHost": "nestedWestonOnWslgX11",
  "command": "browse <dedicated-smoke-repository>",
  "filesystemGrant": "--filesystem=host",
  "theme": "P83Confined",
  "themeSource": "<XDG_CONFIG_HOME>/GitExtensions/GitExtensions/Themes/P83Confined.css",
  "userPluginsDirectory": "<XDG_DATA_HOME>/GitExtensions/UserPlugins.Avalonia",
  "userPluginsNormal": "created-and-readable",
  "userPluginsUnreachable": "bundled-plugins-only",
  "pluginDiscoveryEvidence": "plugin-discovery.txt",
  "permissionsEvidence": "flatpak-permissions.txt",
  "screenshot": "window.png",
  "screenshotSha256": "$screenshot_sha256",
  "stdout": "stdout.log",
  "stderr": "stderr.log"
}
EOF

printf 'Flatpak smoke passed; screenshot=%s\n' "$screenshot_sha256"
