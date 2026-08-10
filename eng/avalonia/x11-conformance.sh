#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: drives rendered native X11 protocol checks until the platform gate closes.

evidence_dir=${1:-}
if [[ -z "$evidence_dir" ]]; then
    echo "usage: x11-conformance.sh <evidence-directory>" >&2
    exit 2
fi

for command_name in Xephyr xclip xdotool xwininfo xwd magick python3 ss; do
    if ! command -v "$command_name" >/dev/null 2>&1; then
        echo "error: '$command_name' is required" >&2
        exit 2
    fi
done
if [[ -z "${DISPLAY:-}" ]]; then
    echo "error: DISPLAY is required to host the isolated X11 server" >&2
    exit 2
fi

repo_root="$(git rev-parse --show-toplevel)"
app="$repo_root/artifacts/Debug/bin/GitExtensions.Avalonia/net10.0/GitExtensions.Avalonia"
if [[ ! -x "$app" ]]; then
    echo "error: build the Avalonia solution before running the X11 conformance sweep" >&2
    exit 2
fi
case "$evidence_dir" in
    /*) ;;
    *) evidence_dir="$repo_root/$evidence_dir" ;;
esac
mkdir -p "$evidence_dir"

probe_root="$(mktemp -d "${TMPDIR:-/tmp}/gitextensions-p85-x11.XXXXXX")"
settings_root="$probe_root/settings"
fixture_repo="$probe_root/repository"
mkdir -p "$settings_root/config/GitExtensions/GitExtensions" "$settings_root/home" "$fixture_repo"

app_pid=
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
  <item><key><string>CheckSettings</string></key><value><string>false</string></value></item>
  <item><key><string>translation</string></key><value><string>English</string></value></item>
</dictionary>
EOF
git -C "$fixture_repo" init --quiet --initial-branch=main
git -C "$fixture_repo" config user.name "P8.5 X11 Conformance"
git -C "$fixture_repo" config user.email "p85-x11@example.invalid"
printf 'P8.5 X11 conformance\n' > "$fixture_repo/x11.txt"
git -C "$fixture_repo" add x11.txt
git -C "$fixture_repo" -c commit.gpgSign=false commit --quiet -m initial

stdout_log="$evidence_dir/stdout.log"
stderr_log="$evidence_dir/stderr.log"
xephyr_log="$evidence_dir/xephyr.log"
report="$evidence_dir/report.json"
loaded_modules="$evidence_dir/loaded-modules.txt"
initial_tree="$evidence_dir/initial-window-tree.txt"
tooltip_tree="$evidence_dir/edge-tooltip-window-tree.txt"
context_menu_tree="$evidence_dir/edge-context-menu-window-tree.txt"
clipboard_plain="$evidence_dir/clipboard-plain.txt"
clipboard_html="$evidence_dir/clipboard-html.txt"
clipboard_types="$evidence_dir/clipboard-types.txt"
initial_capture="$evidence_dir/initial.png"
tooltip_capture="$evidence_dir/edge-tooltip.png"
context_menu_capture="$evidence_dir/edge-context-menu.png"
drop_capture="$evidence_dir/drag-drop.png"
manifest="$evidence_dir/manifest.json"
rm -f -- "$stdout_log" "$stderr_log" "$xephyr_log" "$report" "$loaded_modules" \
    "$initial_tree" "$tooltip_tree" "$context_menu_tree" "$clipboard_plain" "$clipboard_html" "$clipboard_types" \
    "$initial_capture" "$tooltip_capture" "$context_menu_capture" "$drop_capture" "$manifest"

outer_display="$DISPLAY"
display_number=
for candidate in {111..121}; do
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
nested_display="127.0.0.1:$display_number"

export HOME="$settings_root/home"
export XDG_CONFIG_HOME="$settings_root/config"
export XDG_DATA_HOME="$settings_root/data"
export XDG_CACHE_HOME="$settings_root/cache"
export GIT_CONFIG_GLOBAL=/dev/null
export GITEXTENSIONS_DEBUG_FAIL_FAST=1
export GITEXTENSIONS_X11_CONFORMANCE_REPORT="$report"
export LIBGL_ALWAYS_SOFTWARE=1
export MESA_LOADER_DRIVER_OVERRIDE=llvmpipe
export GALLIUM_DRIVER=llvmpipe
export DISPLAY="$nested_display"
unset WAYLAND_DISPLAY GITEXTENSIONS_WAYLAND_CONFORMANCE_REPORT AVALONIA_SCREEN_SCALE_FACTORS

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
    python3 - "$report" "$1" <<'PY'
import json
import sys

report = json.load(open(sys.argv[1], encoding="utf-8"))
point = report["modalProbe"][sys.argv[2]]
print(point["x"], point["y"])
PY
}

capture_root()
{
    DISPLAY="$nested_display" xwd -silent -root | magick xwd:- "png:$1"
}

wait_for_report 'report["backend"] == "x11" and report["modalProbe"]["ownerMatchesMainWindow"]'
grep -E '/Avalonia\.(X11|Wayland)\.dll' "/proc/$app_pid/maps" > "$loaded_modules"
grep -F '/Avalonia.X11.dll' "$loaded_modules" >/dev/null

window_id=
for _ in {1..100}; do
    window_id="$(DISPLAY="$nested_display" xdotool search --onlyvisible --name '^Git Extensions X11 conformance$' 2>/dev/null | tail -n 1 || true)"
    [[ -n "$window_id" ]] && break
    sleep 0.1
done
if [[ -z "$window_id" ]]; then
    echo "error: the X11 conformance window was not found" >&2
    exit 1
fi

DISPLAY="$nested_display" xdotool windowmove "$window_id" 700 500
wait_for_report 'report["modalProbe"]["window"]["position"]["X"] >= 650 and report["modalProbe"]["window"]["position"]["Y"] >= 450'
sleep 1
DISPLAY="$nested_display" xwininfo -root -tree > "$initial_tree"
capture_root "$initial_capture"

read -r edge_x edge_y < <(read_control_point edgeButtonCenter)
DISPLAY="$nested_display" xdotool mousemove "$edge_x" "$edge_y"
DISPLAY="$nested_display" xdotool click 1
wait_for_report 'report["clipboard"]["published"]'
DISPLAY="$nested_display" xclip -selection clipboard -out > "$clipboard_plain"
DISPLAY="$nested_display" xclip -selection clipboard -target TARGETS -out > "$clipboard_types"
html_target="$(grep -Eim1 '^text/html($|;)' "$clipboard_types" || true)"
if [[ -z "$html_target" ]]; then
    echo "error: the X11 clipboard did not advertise an HTML target" >&2
    cat "$clipboard_types" >&2
    exit 1
fi
DISPLAY="$nested_display" xclip -selection clipboard -target "$html_target" -out > "$clipboard_html"
python3 - "$report" "$clipboard_plain" "$clipboard_html" <<'PY'
import json
import sys

report = json.load(open(sys.argv[1], encoding="utf-8"))
plain = open(sys.argv[2], encoding="utf-8").read()
html = open(sys.argv[3], encoding="utf-8").read()
if plain != report["clipboard"]["plainText"]:
    raise SystemExit("external X11 plain-text clipboard read did not match")
if html != report["clipboard"]["html"]:
    raise SystemExit("external X11 HTML clipboard read did not match")
PY

DISPLAY="$nested_display" xdotool mousemove 100 100
sleep 0.5
DISPLAY="$nested_display" xdotool mousemove "$edge_x" "$edge_y"
wait_for_report 'report["interactions"]["tooltipOpened"]'
sleep 0.3
DISPLAY="$nested_display" xwininfo -root -tree > "$tooltip_tree"
capture_root "$tooltip_capture"

DISPLAY="$nested_display" xdotool mousemove 100 100
sleep 0.5
DISPLAY="$nested_display" xdotool mousemove "$edge_x" "$edge_y"
DISPLAY="$nested_display" xdotool click 3
wait_for_report 'report["interactions"]["contextMenuOpened"]'
sleep 0.3
DISPLAY="$nested_display" xwininfo -root -tree > "$context_menu_tree"
capture_root "$context_menu_capture"

python3 - "$initial_tree" "$context_menu_tree" <<'PY'
import re
import sys

pattern = re.compile(r"^\s*(0x[0-9a-f]+).*?\s(\d+)x(\d+)\+(-?\d+)\+(-?\d+)\s", re.IGNORECASE)

def windows(path):
    result = {}
    for line in open(path, encoding="utf-8", errors="replace"):
        match = pattern.search(line)
        if match:
            result[match.group(1)] = tuple(map(int, match.groups()[1:]))
    return result

before = windows(sys.argv[1])
after = windows(sys.argv[2])
popups = {window_id: bounds for window_id, bounds in after.items() if window_id not in before}
if not popups:
    raise SystemExit("the X11 context menu did not create a native top-level window")
for window_id, (width, height, x, y) in popups.items():
    if width <= 0 or height <= 0 or x < 0 or y < 0 or x + width > 1400 or y + height > 1000:
        raise SystemExit(f"X11 popup {window_id} was outside the 1400x1000 server bounds: {(width, height, x, y)}")
PY

DISPLAY="$nested_display" xdotool key Escape
sleep 0.5
read -r source_x source_y < <(read_control_point dragSourceCenter)
read -r target_x target_y < <(read_control_point dropTargetCenter)
DISPLAY="$nested_display" xdotool mousemove 100 100
DISPLAY="$nested_display" xdotool mousemove "$source_x" "$source_y"
DISPLAY="$nested_display" xdotool mousedown 1
DISPLAY="$nested_display" xdotool mousemove "$((source_x + 10))" "$source_y"
wait_for_report 'report["interactions"]["dragStarted"]'
DISPLAY="$nested_display" xdotool mousemove "$(((source_x + target_x) / 2))" "$source_y"
sleep 0.2
DISPLAY="$nested_display" xdotool mousemove "$target_x" "$target_y"
sleep 0.5
DISPLAY="$nested_display" xdotool mouseup 1
wait_for_report 'report["interactions"]["dropReceived"]'
sleep 1
capture_root "$drop_capture"

for capture in "$initial_capture" "$tooltip_capture" "$context_menu_capture" "$drop_capture"; do
    unique_colors="$(magick identify -format '%k' "$capture")"
    if [[ "$unique_colors" -lt 100 ]]; then
        echo "error: $(basename "$capture") contains only $unique_colors colors and is not valid rendered evidence" >&2
        exit 1
    fi
done

if grep -Eiq 'Unhandled exception|fatal error|JIT debugger|Avalonia.*error' "$stdout_log" "$stderr_log"; then
    echo "error: runtime logs contain a failure signature" >&2
    sed -n '1,200p' "$stderr_log" >&2
    exit 1
fi

python3 - "$report" "$manifest" "$initial_capture" "$tooltip_capture" "$context_menu_capture" "$drop_capture" <<'PY'
import hashlib
import json
import os
import sys

report_path, manifest_path, *screenshots = sys.argv[1:]
report = json.load(open(report_path, encoding="utf-8"))
manifest = {
    "schemaVersion": 1,
    "backend": "x11",
    "sessionHost": "nestedXephyrOnWslgX11",
    "nativeProtocols": {
        "activeBackend": report["backend"],
        "backendAssemblyLoaded": "Avalonia.X11.dll",
        "waylandDisplayAbsent": report["waylandDisplay"] is None,
        "modalOwnership": report["modalProbe"]["ownerMatchesMainWindow"],
        "globalWindowMoveObserved": report["modalProbe"]["window"]["position"],
        "plainClipboardExternalRoundTrip": True,
        "htmlClipboardExternalRoundTrip": True,
        "tooltipOpenedAtServerEdge": report["interactions"]["tooltipOpened"],
        "contextMenuOpenedAtServerEdge": report["interactions"]["contextMenuOpened"],
        "contextMenuWithinServerBounds": True,
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

printf 'X11 protocol conformance passed; clipboard=plain+html popups=edge dragDrop=passed geometry=moved\n'
