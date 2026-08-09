#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: records real fractional-scale and mixed-output Wayland events until the platform gate closes.

evidence_dir=${1:-}
if [[ -z "$evidence_dir" ]]; then
    echo "usage: wayland-fractional-scale.sh <evidence-directory>" >&2
    exit 2
fi
for command_name in sway swaymsg python3; do
    if ! command -v "$command_name" >/dev/null 2>&1; then
        echo "error: '$command_name' is required (Ubuntu: sudo apt install sway)" >&2
        exit 2
    fi
done

repo_root="$(git rev-parse --show-toplevel)"
app="$repo_root/artifacts/Debug/bin/GitExtensions.Avalonia/net10.0/GitExtensions.Avalonia"
if [[ ! -x "$app" ]]; then
    echo "error: build the Avalonia solution before running the fractional-scale sweep" >&2
    exit 2
fi
case "$evidence_dir" in
    /*) ;;
    *) evidence_dir="$repo_root/$evidence_dir" ;;
esac
mkdir -p "$evidence_dir"

probe_root="$(mktemp -d "${TMPDIR:-/tmp}/gitextensions-p81-scale.XXXXXX")"
runtime_dir="$probe_root/runtime"
settings_root="$probe_root/settings"
fixture_repo="$probe_root/repository"
mkdir -m 700 "$runtime_dir"
mkdir -p "$settings_root/config/GitExtensions/GitExtensions" "$settings_root/home" "$fixture_repo"

app_pid=
compositor_pid=
cleanup()
{
    if [[ -n "$app_pid" ]] && kill -0 "$app_pid" 2>/dev/null; then
        kill -TERM "$app_pid" 2>/dev/null || true
        wait "$app_pid" 2>/dev/null || true
    fi
    if [[ -n "$compositor_pid" ]] && kill -0 "$compositor_pid" 2>/dev/null; then
        kill -TERM "$compositor_pid" 2>/dev/null || true
        wait "$compositor_pid" 2>/dev/null || true
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
git -C "$fixture_repo" config user.name "P8.1 Fractional Scale"
git -C "$fixture_repo" config user.email "p81-scale@example.invalid"
printf 'P8.1 fractional scale\n' > "$fixture_repo/scale.txt"
git -C "$fixture_repo" add scale.txt
git -C "$fixture_repo" -c commit.gpgSign=false commit --quiet -m initial

cat > "$probe_root/sway.conf" <<'EOF'
xwayland disable
default_border pixel 1
seat seat0 fallback true
output HEADLESS-1 mode 1600x1000 position 0 0 scale 1.25
output HEADLESS-2 mode 1800x1200 position 1280 0 scale 1.5
for_window [title="Git Extensions Wayland conformance"] floating enable, resize set width 680 height 460, move position 590 320
EOF

report="$evidence_dir/report.json"
report_125="$evidence_dir/report-125.json"
report_150="$evidence_dir/report-150.json"
outputs="$evidence_dir/outputs.json"
tree_125="$evidence_dir/tree-125.json"
tree_150="$evidence_dir/tree-150.json"
stdout_log="$evidence_dir/stdout.log"
stderr_log="$evidence_dir/stderr.log"
compositor_log="$evidence_dir/sway.log"
manifest="$evidence_dir/manifest.json"
rm -f -- "$report" "$report_125" "$report_150" "$outputs" "$tree_125" "$tree_150" \
    "$stdout_log" "$stderr_log" "$compositor_log" "$manifest"

env -u DISPLAY \
    XDG_RUNTIME_DIR="$runtime_dir" \
    WLR_BACKENDS=headless \
    WLR_HEADLESS_OUTPUTS=2 \
    WLR_RENDERER=pixman \
    WLR_LIBINPUT_NO_DEVICES=1 \
    sway --unsupported-gpu --debug --config "$probe_root/sway.conf" >"$compositor_log" 2>&1 &
compositor_pid=$!

wayland_socket=
sway_socket=
for _ in {1..50}; do
    wayland_socket="$(find "$runtime_dir" -maxdepth 1 -type s -name 'wayland-*' -printf '%f\n' | head -n 1 || true)"
    sway_socket="$(find "$runtime_dir" -maxdepth 1 -type s -name 'sway-ipc.*.sock' -print -quit || true)"
    [[ -n "$wayland_socket" && -n "$sway_socket" ]] && break
    sleep 0.2
done
if [[ -z "$wayland_socket" || -z "$sway_socket" ]]; then
    echo "error: Sway did not create its Wayland and IPC sockets" >&2
    exit 1
fi

export XDG_RUNTIME_DIR="$runtime_dir"
export WAYLAND_DISPLAY="$wayland_socket"
export SWAYSOCK="$sway_socket"
unset DISPLAY AVALONIA_SCREEN_SCALE_FACTORS
swaymsg --socket "$sway_socket" --type get_outputs --raw > "$outputs"
python3 - "$outputs" <<'PY'
import json
import sys

outputs = {item["name"]: item for item in json.load(open(sys.argv[1], encoding="utf-8"))}
expected = {"HEADLESS-1": 1.25, "HEADLESS-2": 1.5}
if set(outputs) != set(expected):
    raise SystemExit(f"unexpected Sway outputs: {sorted(outputs)}")
for name, scale in expected.items():
    if outputs[name]["scale"] != scale:
        raise SystemExit(f"{name} scale was {outputs[name]['scale']}, expected {scale}")
PY
swaymsg --socket "$sway_socket" 'focus output HEADLESS-1' >/dev/null

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
"$app" browse "$fixture_repo" >"$stdout_log" 2>"$stderr_log" &
app_pid=$!

wait_for_scale()
{
    local scale=$1
    for _ in {1..150}; do
        if [[ -s "$report" ]] && python3 - "$report" "$scale" <<'PY'
import json
import sys

try:
    report = json.load(open(sys.argv[1], encoding="utf-8"))
except (FileNotFoundError, json.JSONDecodeError):
    raise SystemExit(1)
expected = float(sys.argv[2])
window = report["modalProbe"]["window"]
raise SystemExit(0 if abs(window["RenderScaling"] - expected) < 0.001 else 1)
PY
        then
            return 0
        fi
        if ! kill -0 "$app_pid" 2>/dev/null; then
            wait "$app_pid" || true
            echo "error: Avalonia exited before observing scale $scale" >&2
            sed -n '1,200p' "$stderr_log" >&2
            return 1
        fi
        sleep 0.2
    done
    echo "error: timed out waiting for Avalonia scale $scale" >&2
    [[ -s "$report" ]] && cat "$report" >&2
    return 1
}

wait_for_scale 1.25
for _ in {1..100}; do
    swaymsg --socket "$sway_socket" --type get_tree --raw > "$tree_125"
    grep -F 'Git Extensions Wayland conformance' "$tree_125" >/dev/null && break
    sleep 0.1
done
grep -F 'Git Extensions Wayland conformance' "$tree_125" >/dev/null
cp -- "$report" "$report_125"

swaymsg --socket "$sway_socket" '[title="Git Extensions Wayland conformance"] move container to output HEADLESS-2' >/dev/null
wait_for_scale 1.5
swaymsg --socket "$sway_socket" --type get_tree --raw > "$tree_150"
cp -- "$report" "$report_150"

if grep -Eiq 'Unhandled exception|fatal error|JIT debugger|Avalonia.*error' "$stdout_log" "$stderr_log"; then
    echo "error: runtime logs contain a failure signature" >&2
    sed -n '1,200p' "$stderr_log" >&2
    exit 1
fi

python3 - "$outputs" "$report_125" "$report_150" "$manifest" <<'PY'
import json
import sys

outputs_path, report_125_path, report_150_path, manifest_path = sys.argv[1:]
outputs = json.load(open(outputs_path, encoding="utf-8"))
report_125 = json.load(open(report_125_path, encoding="utf-8"))
report_150 = json.load(open(report_150_path, encoding="utf-8"))
manifest = {
    "schemaVersion": 1,
    "backend": "wayland",
    "compositor": "sway-headless-pixman",
    "outputs": [
        {"name": item["name"], "scale": item["scale"], "rect": item["rect"]}
        for item in outputs
    ],
    "fractionalScalesObserved": [
        report_125["modalProbe"]["window"]["RenderScaling"],
        report_150["modalProbe"]["window"]["RenderScaling"],
    ],
    "mixedOutputMove": "HEADLESS-1-to-HEADLESS-2",
    "surfaceCapture": "notClaimed-headless-wlroots-has-no-DRM-renderer-on-this-WSL-host",
    "settingsIsolation": "temporaryXdgHome",
    "repositoryLocation": "outsideWorkingTree",
    "reports": ["report-125.json", "report-150.json"],
    "stdout": "stdout.log",
    "stderr": "stderr.log",
}
with open(manifest_path, "w", encoding="utf-8", newline="\n") as output:
    json.dump(manifest, output, indent=2)
    output.write("\n")
PY

printf 'Wayland fractional-scale conformance passed; scales=125/150 mixedOutputMove=passed\n'
