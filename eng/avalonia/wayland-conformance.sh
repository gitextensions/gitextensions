#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: drives rendered native Wayland protocol checks until the platform gate closes.

evidence_dir=${1:-}
if [[ -z "$evidence_dir" ]]; then
    echo "usage: wayland-conformance.sh <evidence-directory>" >&2
    exit 2
fi

for command_name in Xephyr weston weston-debug weston-screenshooter wayland-info wl-paste xdotool python3 ss; do
    if ! command -v "$command_name" >/dev/null 2>&1; then
        echo "error: '$command_name' is required" >&2
        exit 2
    fi
done
if [[ -z "${DISPLAY:-}" ]]; then
    echo "error: DISPLAY is required to host the isolated Weston compositor" >&2
    exit 2
fi

repo_root="$(git rev-parse --show-toplevel)"
app="$repo_root/artifacts/Debug/bin/GitExtensions.Avalonia/net10.0/GitExtensions.Avalonia"
if [[ ! -x "$app" ]]; then
    echo "error: build the Avalonia solution before running the Wayland conformance sweep" >&2
    exit 2
fi
case "$evidence_dir" in
    /*) ;;
    *) evidence_dir="$repo_root/$evidence_dir" ;;
esac
mkdir -p "$evidence_dir"

probe_root="$(mktemp -d "${TMPDIR:-/tmp}/gitextensions-p81-wayland.XXXXXX")"
runtime_dir="$probe_root/runtime"
settings_root="$probe_root/settings"
fixture_repo="$probe_root/repository"
mkdir -m 700 "$runtime_dir"
mkdir -p "$settings_root/config/GitExtensions/GitExtensions" "$settings_root/home" "$fixture_repo"

app_pid=
compositor_pid=
xephyr_pid=
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
    if [[ -n "$xephyr_pid" ]] && kill -0 "$xephyr_pid" 2>/dev/null; then
        kill -TERM "$xephyr_pid" 2>/dev/null || true
        wait "$xephyr_pid" 2>/dev/null || true
    fi
    rm -rf -- "$probe_root"
}
trap cleanup EXIT

cat > "$settings_root/config/GitExtensions/GitExtensions/GitExtensions.settings" <<'EOF'
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
git -C "$fixture_repo" config user.name "P8.1 Wayland Conformance"
git -C "$fixture_repo" config user.email "p81-wayland@example.invalid"
printf 'P8.1 Wayland conformance\n' > "$fixture_repo/wayland.txt"
git -C "$fixture_repo" add wayland.txt
git -C "$fixture_repo" -c commit.gpgSign=false commit --quiet -m initial

stdout_log="$evidence_dir/stdout.log"
stderr_log="$evidence_dir/stderr.log"
weston_log="$evidence_dir/weston.log"
xephyr_log="$evidence_dir/xephyr.log"
report="$evidence_dir/report.json"
globals="$evidence_dir/wayland-globals.txt"
scene_graph="$evidence_dir/scene-graph.txt"
tooltip_scene_graph="$evidence_dir/edge-tooltip-scene-graph.txt"
context_menu_scene_graph="$evidence_dir/edge-context-menu-scene-graph.txt"
clipboard_plain="$evidence_dir/clipboard-plain.txt"
clipboard_html="$evidence_dir/clipboard-html.txt"
clipboard_types="$evidence_dir/clipboard-types.txt"
tooltip_capture="$evidence_dir/edge-tooltip.png"
context_menu_capture="$evidence_dir/edge-context-menu.png"
drop_capture="$evidence_dir/drag-drop.png"
initial_capture="$evidence_dir/initial.png"
manifest="$evidence_dir/manifest.json"
rm -f -- "$stdout_log" "$stderr_log" "$weston_log" "$xephyr_log" "$report" "$globals" "$scene_graph" \
    "$tooltip_scene_graph" "$context_menu_scene_graph" \
    "$clipboard_plain" "$clipboard_html" "$clipboard_types" "$tooltip_capture" \
    "$context_menu_capture" "$drop_capture" "$initial_capture" "$manifest"

outer_display="$DISPLAY"
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
env DISPLAY="$outer_display" \
    Xephyr ":$display_number" \
        -screen 1400x1000 \
        -listen tcp \
        -nolisten unix \
        -ac \
        -noreset >"$xephyr_log" 2>&1 &
xephyr_pid=$!
display_port=$((6000 + display_number))
for _ in {1..50}; do
    ss -H -ltn "sport = :$display_port" | grep -q . && break
    sleep 0.2
done
if ! ss -H -ltn "sport = :$display_port" | grep -q .; then
    echo "error: Xephyr did not create its X11 listener" >&2
    exit 1
fi
host_display="127.0.0.1:$display_number"
wayland_socket=wayland-p81
env \
    -u WAYLAND_DISPLAY \
    XDG_RUNTIME_DIR="$runtime_dir" \
    DISPLAY="$host_display" \
    weston \
        --backend=x11-backend.so \
        --socket="$wayland_socket" \
        --width=1280 \
        --height=900 \
        --renderer=gl \
        --idle-time=0 \
        --debug \
        --log="$weston_log" >>"$weston_log" 2>&1 &
compositor_pid=$!
for _ in {1..50}; do
    [[ -S "$runtime_dir/$wayland_socket" ]] && break
    if ! kill -0 "$compositor_pid" 2>/dev/null; then
        wait "$compositor_pid" || true
        echo "error: Weston exited before its Wayland socket appeared" >&2
        sed -n '1,200p' "$weston_log" >&2
        exit 1
    fi
    sleep 0.2
done
if [[ ! -S "$runtime_dir/$wayland_socket" ]]; then
    echo "error: Weston did not create its Wayland socket" >&2
    exit 1
fi

export XDG_RUNTIME_DIR="$runtime_dir"
export WAYLAND_DISPLAY="$wayland_socket"
wayland-info > "$globals"
grep -F "interface: 'xdg_wm_base'" "$globals" >/dev/null

export HOME="$settings_root/home"
export XDG_CONFIG_HOME="$settings_root/config"
export XDG_DATA_HOME="$settings_root/data"
export XDG_CACHE_HOME="$settings_root/cache"
export GIT_CONFIG_GLOBAL=/dev/null
export GITEXTENSIONS_DEBUG_FAIL_FAST=1
export GITEXTENSIONS_WAYLAND_CONFORMANCE_REPORT="$report"
export LIBGL_ALWAYS_SOFTWARE=1
export MESA_LOADER_DRIVER_OVERRIDE=llvmpipe
export GALLIUM_DRIVER=llvmpipe
unset DISPLAY AVALONIA_SCREEN_SCALE_FACTORS

"$app" browse "$fixture_repo" >"$stdout_log" 2>"$stderr_log" &
app_pid=$!

wait_for_report()
{
    local expression=$1
    for _ in {1..150}; do
        if [[ -s "$report" ]] && python3 - "$report" "$expression" <<'PY'
import json
import sys

try:
    report = json.load(open(sys.argv[1], encoding="utf-8"))
except (FileNotFoundError, json.JSONDecodeError):
    raise SystemExit(1)
raise SystemExit(0 if eval(sys.argv[2], {"report": report}) else 1)
PY
        then
            return 0
        fi
        if ! kill -0 "$app_pid" 2>/dev/null; then
            wait "$app_pid" || true
            echo "error: Avalonia exited before satisfying report condition: $expression" >&2
            sed -n '1,200p' "$stderr_log" >&2
            return 1
        fi
        sleep 0.2
    done
    echo "error: timed out waiting for report condition: $expression" >&2
    [[ -s "$report" ]] && cat "$report" >&2
    return 1
}

read_control_point()
{
    weston-debug scene-graph > "$scene_graph"
    python3 - "$report" "$scene_graph" "$1" <<'PY'
import json
import re
import sys

report = json.load(open(sys.argv[1], encoding="utf-8"))
scene = open(sys.argv[2], encoding="utf-8").read().splitlines()
point = report["modalProbe"][sys.argv[3]]
for index, line in enumerate(scene):
    if "top-level window 'Git Extensions Wayland conformance'" not in line:
        continue
    for candidate in scene[index + 1:index + 5]:
        match = re.search(r"position: \((-?\d+), (-?\d+)\)", candidate)
        if match:
            print(int(match.group(1)) + point["x"], int(match.group(2)) + point["y"])
            raise SystemExit(0)
raise SystemExit("the Wayland conformance window was absent from Weston's scene graph")
PY
}

capture_output()
{
    local output=$1
    rm -f -- "$evidence_dir"/wayland-screenshot-*.png
    (
        cd "$evidence_dir"
        weston-screenshooter
    ) >/dev/null
    local generated=("$evidence_dir"/wayland-screenshot-*.png)
    if [[ ${#generated[@]} -ne 1 ]] || [[ ! -f "${generated[0]}" ]]; then
        echo "error: Weston did not produce exactly one screenshot" >&2
        exit 1
    fi
    mv -- "${generated[0]}" "$output"
}

wait_for_report 'report["modalProbe"]["ownerMatchesMainWindow"]'
weston_window=
for _ in {1..100}; do
    while read -r window_id; do
        window_name="$(DISPLAY="$host_display" xdotool getwindowname "$window_id" 2>/dev/null || true)"
        if [[ "$window_name" == Weston* ]]; then
            weston_window=$window_id
        fi
    done < <(DISPLAY="$host_display" xdotool search --all --name '.*' 2>/dev/null || true)
    [[ -n "$weston_window" ]] && break
    sleep 0.1
done
if [[ -z "$weston_window" ]]; then
    echo "error: the nested Weston host window was not found" >&2
    while read -r window_id; do
        printf '%s ' "$window_id" >&2
        DISPLAY="$host_display" xdotool getwindowname "$window_id" >&2 || true
    done < <(DISPLAY="$host_display" xdotool search --all --name '.*' 2>/dev/null || true)
    exit 1
fi

sleep 8
weston-debug scene-graph > "$scene_graph"
capture_output "$initial_capture"
read -r clipboard_x clipboard_y < <(read_control_point edgeButtonCenter)
DISPLAY="$host_display" xdotool mousemove "$clipboard_x" "$clipboard_y"
DISPLAY="$host_display" xdotool click 1
wait_for_report 'report["clipboard"]["published"]'
wl-paste --list-types > "$clipboard_types"
wl-paste --no-newline > "$clipboard_plain"
wl-paste --type text/html --no-newline > "$clipboard_html"
grep -Fx 'Git Extensions Wayland plain-text clipboard' "$clipboard_plain" >/dev/null
grep -Fx '<strong>Git Extensions Wayland rich clipboard</strong>' "$clipboard_html" >/dev/null

read -r edge_x edge_y < <(read_control_point edgeButtonCenter)
DISPLAY="$host_display" xdotool mousemove 100 100
sleep 0.5
DISPLAY="$host_display" xdotool mousemove "$edge_x" "$edge_y"
wait_for_report 'report["interactions"]["tooltipOpened"]'
sleep 0.3
weston-debug scene-graph > "$tooltip_scene_graph"
capture_output "$tooltip_capture"
DISPLAY="$host_display" xdotool mousemove 100 100
sleep 0.5
DISPLAY="$host_display" xdotool mousemove "$edge_x" "$edge_y"
DISPLAY="$host_display" xdotool click 3
wait_for_report 'report["interactions"]["contextMenuOpened"]'
sleep 0.3
weston-debug scene-graph > "$context_menu_scene_graph"
capture_output "$context_menu_capture"
DISPLAY="$host_display" xdotool key Escape
sleep 0.5

read -r source_x source_y < <(read_control_point dragSourceCenter)
read -r target_x target_y < <(read_control_point dropTargetCenter)
DISPLAY="$host_display" xdotool mousemove 100 100
DISPLAY="$host_display" xdotool mousemove "$source_x" "$source_y"
DISPLAY="$host_display" xdotool mousedown 1
DISPLAY="$host_display" xdotool mousemove "$((source_x + 10))" "$source_y"
wait_for_report 'report["interactions"]["dragStarted"]'
DISPLAY="$host_display" xdotool mousemove "$(((source_x + target_x) / 2))" "$source_y"
sleep 0.2
DISPLAY="$host_display" xdotool mousemove "$target_x" "$target_y"
sleep 0.5
DISPLAY="$host_display" xdotool mouseup 1
wait_for_report 'report["interactions"]["dropReceived"]'
capture_output "$drop_capture"

python3 - "$report" "$context_menu_scene_graph" <<'PY'
import json
import re
import sys

report = json.load(open(sys.argv[1], encoding="utf-8"))
bounds = report["modalProbe"]["currentScreen"]["bounds"]
right = bounds["X"] + bounds["Width"]
bottom = bounds["Y"] + bounds["Height"]
for scene_path in sys.argv[2:]:
    lines = open(scene_path, encoding="utf-8").read().splitlines()
    popup_bounds = []
    for index, line in enumerate(lines):
        if "role xdg_popup" not in line:
            continue
        match = re.search(r"position: \((-?\d+), (-?\d+)\) -> \((-?\d+), (-?\d+)\)", lines[index + 1])
        if match:
            popup_bounds.append(tuple(map(int, match.groups())))
    if not popup_bounds:
        raise SystemExit(f"no native xdg_popup appeared in {scene_path}")
    for left, top, popup_right, popup_bottom in popup_bounds:
        if left < bounds["X"] or top < bounds["Y"] or popup_right > right or popup_bottom > bottom:
            raise SystemExit(f"popup outside output bounds in {scene_path}: {(left, top, popup_right, popup_bottom)}")
PY

if grep -Eiq 'Unhandled exception|fatal error|JIT debugger|Avalonia.*error' "$stdout_log" "$stderr_log"; then
    echo "error: runtime logs contain a failure signature" >&2
    sed -n '1,200p' "$stderr_log" >&2
    exit 1
fi

python3 - "$report" "$manifest" "$tooltip_capture" "$context_menu_capture" "$drop_capture" <<'PY'
import hashlib
import json
import os
import sys
import time

report_path, manifest_path, *screenshots = sys.argv[1:]
for attempt in range(20):
    try:
        report = json.load(open(report_path, encoding="utf-8"))
        break
    except (FileNotFoundError, json.JSONDecodeError):
        if attempt == 19:
            raise
        time.sleep(0.1)
manifest = {
    "schemaVersion": 1,
    "backend": "wayland",
    "sessionHost": "nestedWestonOnXephyrOnWslgX11",
    "nativeProtocols": {
        "modalOwnership": report["modalProbe"]["ownerMatchesMainWindow"],
        "plainClipboardExternalRoundTrip": True,
        "htmlClipboardExternalRoundTrip": True,
        "tooltipOpenedAtOutputEdge": report["interactions"]["tooltipOpened"],
        "contextMenuOpenedAtOutputEdge": report["interactions"]["contextMenuOpened"],
        "dragAndDropCompleted": report["interactions"]["dropReceived"],
    },
    "settingsIsolation": "temporaryXdgHome",
    "repositoryLocation": "outsideWorkingTree",
    "screenshots": {
        os.path.basename(path): hashlib.sha256(open(path, "rb").read()).hexdigest()
        for path in screenshots
    },
    "report": os.path.basename(report_path),
    "stdout": "stdout.log",
    "stderr": "stderr.log",
}
with open(manifest_path, "w", encoding="utf-8", newline="\n") as output:
    json.dump(manifest, output, indent=2)
    output.write("\n")
PY

printf 'Wayland protocol conformance passed; clipboard=plain+html popups=edge dragDrop=passed\n'
