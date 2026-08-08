#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: records real Linux window evidence until the parity gate closes.

backend=${1:-}
evidence_dir=${2:-}
if [[ "$backend" != "wayland" && "$backend" != "x11" ]] || [[ -z "$evidence_dir" ]]; then
    echo "usage: linux-runtime-smoke.sh <wayland|x11> <evidence-directory>" >&2
    exit 2
fi

repo_root="$(git rev-parse --show-toplevel)"
app="$repo_root/artifacts/Debug/bin/GitExtensions.Avalonia/net10.0/GitExtensions.Avalonia"
if [[ ! -x "$app" ]]; then
    echo "error: build the Avalonia solution before running the runtime smoke" >&2
    exit 2
fi

case "$evidence_dir" in
    /*) ;;
    *) evidence_dir="$repo_root/$evidence_dir" ;;
esac
mkdir -p "$evidence_dir"

smoke_root="$(mktemp -d "${TMPDIR:-/tmp}/gitextensions-p06-runtime.XXXXXX")"
app_pid=
compositor_pid=
cleanup()
{
    if [[ -n "$app_pid" ]] && kill -0 "$app_pid" 2>/dev/null; then
        kill -TERM "$app_pid" 2>/dev/null || true
        for _ in {1..20}; do
            kill -0 "$app_pid" 2>/dev/null || break
            sleep 0.25
        done
        kill -KILL "$app_pid" 2>/dev/null || true
        wait "$app_pid" 2>/dev/null || true
    fi
    if [[ -n "$compositor_pid" ]] && kill -0 "$compositor_pid" 2>/dev/null; then
        kill -TERM "$compositor_pid" 2>/dev/null || true
        wait "$compositor_pid" 2>/dev/null || true
    fi
    rm -rf -- "$smoke_root"
}
trap cleanup EXIT

fixture_repo="$smoke_root/repository"
settings_root="$smoke_root/settings"
settings_directory="$settings_root/config/GitExtensions/GitExtensions"
mkdir -p "$fixture_repo" "$settings_root/home" "$settings_directory"
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
</dictionary>
EOF
git -C "$fixture_repo" init --quiet --initial-branch=main
git -C "$fixture_repo" config user.name "P0.6 Runtime Smoke"
git -C "$fixture_repo" config user.email "p06-smoke@example.invalid"
printf 'runtime smoke\n' > "$fixture_repo/smoke.txt"
git -C "$fixture_repo" add smoke.txt
git -C "$fixture_repo" -c commit.gpgSign=false commit --quiet -m initial

stdout_log="$evidence_dir/stdout.log"
stderr_log="$evidence_dir/stderr.log"
screenshot="$evidence_dir/window.png"
capture_log="$evidence_dir/capture.log"
backend_log="$evidence_dir/backend-assemblies.txt"
connection_log="$evidence_dir/backend-connections.txt"
manifest="$evidence_dir/smoke.json"
rm -f -- \
    "$stdout_log" \
    "$stderr_log" \
    "$screenshot" \
    "$capture_log" \
    "$backend_log" \
    "$connection_log" \
    "$manifest"

wayland_display=${WAYLAND_DISPLAY:-}
x11_display=${DISPLAY:-}
session_host=
capture_title="Git Extensions"
compositor_json=null
backend_evidence=
if [[ "$backend" == "wayland" ]]; then
    if [[ -z "$wayland_display" ]]; then
        echo "error: WAYLAND_DISPLAY is not set" >&2
        exit 1
    fi

    wayland_globals="$(timeout 5 wayland-info 2>/dev/null || true)"
    if grep -Eq "interface: 'xdg_wm_base'.*version:[[:space:]]+([3-9]|[1-9][0-9]+)" \
        <<<"$wayland_globals"; then
        export WAYLAND_DISPLAY="$wayland_display"
        session_host="nativeSession"
        backend_evidence="waylandSessionScreenshotWithDisplayUnset"
    else
        if [[ -z "$x11_display" ]] || ! command -v weston >/dev/null 2>&1; then
            echo "error: the session lacks xdg_wm_base >= 3 and nested Weston is unavailable" >&2
            exit 1
        fi

        weston_runtime="$smoke_root/weston-runtime"
        weston_log="$evidence_dir/weston.log"
        weston_socket="wayland-p06"
        mkdir -p "$weston_runtime"
        chmod 700 "$weston_runtime"
        rm -f -- "$weston_log"
        env \
            -u WAYLAND_DISPLAY \
            XDG_RUNTIME_DIR="$weston_runtime" \
            DISPLAY="$x11_display" \
            weston \
                --backend=x11-backend.so \
                --socket="$weston_socket" \
                --width=1280 \
                --height=900 \
                --renderer=pixman \
                --idle-time=0 \
                --debug \
                --log="$weston_log" >>"$weston_log" 2>&1 &
        compositor_pid=$!
        for _ in {1..30}; do
            [[ -S "$weston_runtime/$weston_socket" ]] && break
            if ! kill -0 "$compositor_pid" 2>/dev/null; then
                wait "$compositor_pid" || true
                echo "error: nested Weston exited before its Wayland socket appeared" >&2
                sed -n '1,160p' "$weston_log" >&2
                exit 1
            fi
            sleep 1
        done
        if [[ ! -S "$weston_runtime/$weston_socket" ]]; then
            echo "error: nested Weston did not create its Wayland socket" >&2
            exit 1
        fi

        export XDG_RUNTIME_DIR="$weston_runtime"
        export WAYLAND_DISPLAY="$weston_socket"
        export LIBGL_ALWAYS_SOFTWARE=1
        export MESA_LOADER_DRIVER_OVERRIDE=llvmpipe
        export GALLIUM_DRIVER=llvmpipe
        session_host="nestedWestonOnWslgX11"
        capture_title="Weston"
        compositor_json='"weston.log"'
        backend_evidence="westonOutputScreenshotWithDisplayUnset"
    fi
    unset DISPLAY
    expected_assembly="Avalonia.Wayland.dll"
else
    if [[ -z "$x11_display" ]]; then
        echo "error: DISPLAY is not set" >&2
        exit 1
    fi
    if ! command -v Xephyr >/dev/null 2>&1; then
        echo "error: Xephyr is required for the reproducible nested X11 session" >&2
        exit 1
    fi

    display_number=
    for candidate in {100..110}; do
        candidate_port=$((6000 + candidate))
        if ! ss -H -ltn "sport = :$candidate_port" | grep -q .; then
            display_number=$candidate
            break
        fi
    done
    if [[ -z "$display_number" ]]; then
        echo "error: no free nested X11 display is available" >&2
        exit 1
    fi

    xephyr_log="$evidence_dir/xephyr.log"
    rm -f -- "$xephyr_log"
    env DISPLAY="$x11_display" \
        Xephyr ":$display_number" \
            -screen 1280x900 \
            -listen tcp \
            -nolisten unix \
            -ac \
            -noreset >"$xephyr_log" 2>&1 &
    compositor_pid=$!
    display_port=$((6000 + display_number))
    for _ in {1..30}; do
        ss -H -ltn "sport = :$display_port" | grep -q . && break
        if ! kill -0 "$compositor_pid" 2>/dev/null; then
            wait "$compositor_pid" || true
            echo "error: nested Xephyr exited before its X11 socket appeared" >&2
            sed -n '1,160p' "$xephyr_log" >&2
            exit 1
        fi
        sleep 1
    done
    if ! ss -H -ltn "sport = :$display_port" | grep -q .; then
        echo "error: nested Xephyr did not create its X11 listener" >&2
        exit 1
    fi

    unset WAYLAND_DISPLAY
    export DISPLAY="127.0.0.1:$display_number"
    export LIBGL_ALWAYS_SOFTWARE=1
    export MESA_LOADER_DRIVER_OVERRIDE=llvmpipe
    export GALLIUM_DRIVER=llvmpipe
    session_host="nestedXephyrOnWslgX11"
    compositor_json='"xephyr.log"'
    backend_evidence="xwdWindowCaptureWithWaylandUnset"
    expected_assembly="Avalonia.X11.dll"
fi

unset AVALONIA_SCREEN_SCALE_FACTORS
export HOME="$settings_root/home"
export XDG_CONFIG_HOME="$settings_root/config"
export XDG_DATA_HOME="$settings_root/data"
export XDG_CACHE_HOME="$settings_root/cache"
export GIT_CONFIG_GLOBAL=/dev/null
export GITEXTENSIONS_DEBUG_FAIL_FAST=1

"$app" browse "$fixture_repo" >"$stdout_log" 2>"$stderr_log" &
app_pid=$!

for _ in {1..30}; do
    if ! kill -0 "$app_pid" 2>/dev/null; then
        wait "$app_pid" || true
        echo "error: Avalonia exited before the $backend window could be captured" >&2
        sed -n '1,160p' "$stderr_log" >&2
        exit 1
    fi
    if grep -F "$expected_assembly" "/proc/$app_pid/maps" >/dev/null 2>&1; then
        break
    fi
    sleep 1
done

grep -Eo '/[^ ]*/Avalonia\.(Wayland|X11)\.dll' "/proc/$app_pid/maps" \
    | sort -u > "$backend_log" || true
if ! grep -F "$expected_assembly" "$backend_log" >/dev/null; then
    echo "error: process did not load expected backend assembly '$expected_assembly'" >&2
    cat "$backend_log" >&2
    exit 1
fi
lsof -a -p "$app_pid" -U >"$connection_log" 2>&1 || true

capture_script=${GITEXTENSIONS_CAPTURE_SCRIPT:-}
if [[ "$session_host" == "nestedWestonOnWslgX11" ]] \
    && command -v weston-screenshooter >/dev/null 2>&1; then
    sleep 8
    rm -f -- "$evidence_dir"/wayland-screenshot-*.png
    (
        cd "$evidence_dir"
        weston-screenshooter
    ) >"$capture_log" 2>&1
    generated_screenshots=("$evidence_dir"/wayland-screenshot-*.png)
    if [[ ${#generated_screenshots[@]} -ne 1 ]] || [[ ! -f "${generated_screenshots[0]}" ]]; then
        echo "error: Weston did not produce exactly one screenshot" >&2
        exit 1
    fi
    mv -- "${generated_screenshots[0]}" "$screenshot"
elif [[ "$session_host" == "nestedXephyrOnWslgX11" ]] \
    && command -v xdotool >/dev/null 2>&1 \
    && command -v xwd >/dev/null 2>&1 \
    && command -v magick >/dev/null 2>&1; then
    window_id=
    for _ in {1..45}; do
        window_id="$(xdotool search --onlyvisible --name "Git Extensions" 2>/dev/null | tail -n 1 || true)"
        [[ -n "$window_id" ]] && break
        sleep 1
    done
    if [[ -z "$window_id" ]]; then
        echo "error: no visible X11 window matching 'Git Extensions' appeared" >&2
        exit 1
    fi
    sleep 8
    xwd_image="$evidence_dir/window.xwd"
    rm -f -- "$xwd_image"
    xwd -silent -id "$window_id" -out "$xwd_image" >"$capture_log" 2>&1
    magick "xwd:$xwd_image" "$screenshot" >>"$capture_log" 2>&1
    rm -f -- "$xwd_image"
    printf 'windowId=%s\ncaptureMethod=Xwd\n' "$window_id" >>"$capture_log"
elif [[ -n "$capture_script" ]] \
    && [[ -x /mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe ]]; then
    capture_script_windows="$(wslpath -w "$capture_script")"
    screenshot_windows="$(wslpath -w "$screenshot")"
    /mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe \
        -NoProfile \
        -NonInteractive \
        -ExecutionPolicy Bypass \
        -File "$capture_script_windows" \
        -TitlePattern "$capture_title" \
        -OutputPath "$screenshot_windows" \
        -TimeoutSeconds 45 \
        -CaptureMethod Screen >"$capture_log" 2>&1
elif command -v grim >/dev/null 2>&1; then
    grim "$screenshot" >"$capture_log" 2>&1
elif command -v gnome-screenshot >/dev/null 2>&1; then
    gnome-screenshot --file="$screenshot" >"$capture_log" 2>&1
elif [[ "$backend" == "x11" ]] && command -v import >/dev/null 2>&1; then
    import -window root "$screenshot" >"$capture_log" 2>&1
else
    echo "error: no screenshot provider is available" >&2
    exit 1
fi

if [[ ! -s "$screenshot" ]]; then
    echo "error: screenshot provider produced no image" >&2
    exit 1
fi
if ! kill -0 "$app_pid" 2>/dev/null; then
    echo "error: Avalonia exited during the $backend capture" >&2
    exit 1
fi
if grep -Eiq 'Unhandled exception|fatal error|JIT debugger|Avalonia.*error' "$stdout_log" "$stderr_log"; then
    echo "error: runtime logs contain a failure signature" >&2
    sed -n '1,160p' "$stderr_log" >&2
    exit 1
fi

screenshot_sha256="$(sha256sum "$screenshot" | cut -d' ' -f1)"
backend_assembly="$expected_assembly"
cat > "$manifest" <<EOF
{
  "schemaVersion": 1,
  "backend": "$backend",
  "backendAssembly": "$backend_assembly",
  "backendEvidence": "$backend_evidence",
  "sessionHost": "$session_host",
  "sessionLog": $compositor_json,
  "command": "browse <temporary-repository>",
  "settingsIsolation": "temporaryXdgHome",
  "repositoryLocation": "outsideWorkingTree",
  "screenshot": "window.png",
  "screenshotSha256": "$screenshot_sha256",
  "stdout": "stdout.log",
  "stderr": "stderr.log"
}
EOF

printf '%s smoke passed; backend=%s screenshot=%s\n' \
    "$backend" "$backend_assembly" "$screenshot_sha256"
