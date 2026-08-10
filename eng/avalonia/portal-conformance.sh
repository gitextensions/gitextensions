#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: drives real XDG portal checks until the platform gate closes.

mode=
evidence_dir=
flatpak_app_id=${GITEXTENSIONS_PORTAL_CONFORMANCE_FLATPAK_APP_ID:-}
flatpak_host_home=$HOME
flatpak_host_data_home=${XDG_DATA_HOME:-$HOME/.local/share}
if [[ "${1:-}" == "--mode" ]]; then
    mode=${2:-}
    evidence_dir=${3:-}
else
    evidence_dir=${1:-}
fi

if [[ -z "$evidence_dir" ]] || { [[ -n "$mode" ]] && [[ "$mode" != "present" && "$mode" != "absent" ]]; }; then
    echo "usage: portal-conformance.sh <evidence-directory>" >&2
    exit 2
fi

repo_root="$(git rev-parse --show-toplevel)"
case "$evidence_dir" in
    /*) ;;
    *) evidence_dir="$repo_root/$evidence_dir" ;;
esac

if [[ -z "$mode" ]]; then
    mkdir -p "$evidence_dir"
    if [[ -n "$flatpak_app_id" ]]; then
        dbus-run-session -- bash "$0" --mode present "$evidence_dir/present"
        python3 - "$evidence_dir/present/report.json" "$evidence_dir/manifest.json" "$flatpak_app_id" <<'PY'
import json
import sys

report_path, manifest_path, app_id = sys.argv[1:]
report = json.load(open(report_path, encoding="utf-8"))
assert report["stage"] == "completed" and report["error"] is None, report
assert report["interfaces"] == {"fileChooser": True, "openUri": True}, report
assert all(value is True for value in report["pickers"].values()), report
assert all(value is True for value in report["shellActions"].values()), report
manifest = {
    "schemaVersion": 1,
    "execution": "flatpak",
    "appId": app_id,
    "backendPresent": "passed",
    "pickerTransport": "org.freedesktop.portal.FileChooser",
    "shellTransport": "org.freedesktop.portal.OpenURI",
    "defaultHandlerLaunchesObserved": 3,
    "openFileApplicationChooserAccepted": True,
    "pickerCancellationCompleted": True,
    "backend": "wayland",
    "settingsIsolation": "temporaryXdgHomeInsideDedicatedFlatpakId",
    "repositoryLocation": "outsideWorkingTree",
    "reports": ["present/report.json"],
}
with open(manifest_path, "w", encoding="utf-8", newline="\n") as output:
    json.dump(manifest, output, indent=2)
    output.write("\n")
PY
        printf 'confined XDG portal conformance passed; backend=present pickers=3 cancellation=1 shellActions=4\n'
        exit 0
    fi

    dbus-run-session -- bash "$0" --mode present "$evidence_dir/present"
    bash "$0" --mode absent "$evidence_dir/absent"
    python3 - "$evidence_dir/present/report.json" "$evidence_dir/absent/report.json" "$evidence_dir/manifest.json" <<'PY'
import json
import sys

present_path, absent_path, manifest_path = sys.argv[1:]
present = json.load(open(present_path, encoding="utf-8"))
absent = json.load(open(absent_path, encoding="utf-8"))
assert present["stage"] == "completed" and present["error"] is None, present
assert present["interfaces"] == {"fileChooser": True, "openUri": True}, present
assert all(value is True for value in present["pickers"].values()), present
assert all(value is True for value in present["shellActions"].values()), present
assert absent["stage"] == "completed" and absent["error"] is None, absent
assert absent["interfaces"] == {"fileChooser": False, "openUri": False}, absent
assert all(value is None for value in absent["pickers"].values()), absent
assert all(value is False for value in absent["shellActions"].values()), absent
manifest = {
    "schemaVersion": 1,
    "backendPresent": "passed",
    "backendAbsent": "passed",
    "pickerTransport": "org.freedesktop.portal.FileChooser",
    "shellTransport": "org.freedesktop.portal.OpenURI",
    "defaultHandlerLaunchesObserved": 3,
    "openFileApplicationChooserAccepted": True,
    "backend": "x11",
    "settingsIsolation": "temporaryXdgHome",
    "repositoryLocation": "outsideWorkingTree",
    "reports": ["present/report.json", "absent/report.json"],
}
with open(manifest_path, "w", encoding="utf-8", newline="\n") as output:
    json.dump(manifest, output, indent=2)
    output.write("\n")
PY
    printf 'XDG portal conformance passed; backend=present+absent pickers=3 shellActions=4\n'
    exit 0
fi

for command_name in python3 git; do
    command -v "$command_name" >/dev/null 2>&1 || { echo "error: '$command_name' is required" >&2; exit 2; }
done
app="$repo_root/artifacts/Debug/bin/GitExtensions.Avalonia/net10.0/GitExtensions.Avalonia"
if [[ -z "$flatpak_app_id" ]]; then
    [[ -x "$app" ]] || { echo "error: build the Avalonia solution before running the portal sweep" >&2; exit 2; }
else
    command -v flatpak >/dev/null 2>&1 || { echo "error: 'flatpak' is required" >&2; exit 2; }
    flatpak info --user "$flatpak_app_id" >/dev/null
fi
[[ -n "${DISPLAY:-}" ]] || { echo "error: DISPLAY is required" >&2; exit 2; }

mkdir -p "$evidence_dir"
if [[ -n "$flatpak_app_id" ]]; then
    probe_root="$(mktemp -d "$HOME/.local/share/gitextensions-p83-flatpak-portal.XXXXXX")"
else
    probe_root="$(mktemp -d "${TMPDIR:-/tmp}/gitextensions-p82-portal.XXXXXX")"
fi
settings_root="$probe_root/settings"
fixture_repo="$probe_root/repository"
fixture_file="$fixture_repo/portal.txt"
mkdir -p "$settings_root/config/GitExtensions/GitExtensions" "$settings_root/data/applications" \
    "$settings_root/cache" "$settings_root/home" "$fixture_repo"

app_pid=
portal_pid=
backend_pid=
monitor_pid=
cleanup()
{
    if [[ -n "$flatpak_app_id" ]]; then
        env HOME="$flatpak_host_home" XDG_DATA_HOME="$flatpak_host_data_home" \
            flatpak kill "$flatpak_app_id" 2>/dev/null || true
    fi
    for pid_name in app_pid monitor_pid portal_pid backend_pid; do
        pid=${!pid_name:-}
        if [[ -n "$pid" ]] && kill -0 "$pid" 2>/dev/null; then
            kill -TERM "$pid" 2>/dev/null || true
            wait "$pid" 2>/dev/null || true
        fi
    done
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
git -C "$fixture_repo" config user.name "P8.2 Portal Conformance"
git -C "$fixture_repo" config user.email "p82-portal@example.invalid"
printf 'P8.2 portal conformance\n' > "$fixture_file"
git -C "$fixture_repo" add portal.txt
git -C "$fixture_repo" -c commit.gpgSign=false commit --quiet -m initial

report="$evidence_dir/report.json"
report_output="$report"
report_for_app="$report"
if [[ -n "$flatpak_app_id" ]]; then
    flatpak_report_root="$flatpak_host_home/.var/app/$flatpak_app_id/data/p83-portal"
    rm -rf -- "$flatpak_report_root"
    mkdir -p "$flatpak_report_root"
    report="$flatpak_report_root/report.json"
    report_for_app=/var/data/p83-portal/report.json
fi
stdout_log="$evidence_dir/stdout.log"
stderr_log="$evidence_dir/stderr.log"
portal_log="$evidence_dir/portal.log"
backend_log="$evidence_dir/backend.log"
dbus_log="$evidence_dir/dbus-monitor.log"
launch_log="$evidence_dir/launches.log"
rm -f -- "$report" "$stdout_log" "$stderr_log" "$portal_log" "$backend_log" "$dbus_log" "$launch_log"

export HOME="$settings_root/home"
export XDG_CONFIG_HOME="$settings_root/config"
export XDG_DATA_HOME="$settings_root/data"
export XDG_CACHE_HOME="$settings_root/cache"
export GIT_CONFIG_GLOBAL=/dev/null
export GITEXTENSIONS_DEBUG_FAIL_FAST=1
export GITEXTENSIONS_PORTAL_CONFORMANCE_REPORT="$report"
export GITEXTENSIONS_PORTAL_CONFORMANCE_EXPECTED="$mode"
export GITEXTENSIONS_PORTAL_CONFORMANCE_FIXTURE="$fixture_file"

if [[ "$mode" == "present" ]]; then
    for command_name in dbus-monitor dbus-update-activation-environment gdbus xdotool; do
        command -v "$command_name" >/dev/null 2>&1 || { echo "error: '$command_name' is required" >&2; exit 2; }
    done
    [[ -x /usr/libexec/xdg-desktop-portal ]] || { echo "error: xdg-desktop-portal is required" >&2; exit 2; }
    [[ -x /usr/libexec/xdg-desktop-portal-gtk ]] || { echo "error: xdg-desktop-portal-gtk is required" >&2; exit 2; }
    [[ -n "${DISPLAY:-}" ]] || { echo "error: DISPLAY is required for the isolated GTK portal backend" >&2; exit 2; }
    host_display=$DISPLAY
    export GDK_BACKEND=x11
    dbus-update-activation-environment HOME XDG_CONFIG_HOME XDG_DATA_HOME XDG_CACHE_HOME DISPLAY GDK_BACKEND

    recorder="$probe_root/portal-recorder.sh"
    cat > "$recorder" <<EOF
#!/usr/bin/env bash
printf '%s\n' "\$*" >> "$launch_log"
EOF
    chmod +x "$recorder"
    cat > "$settings_root/data/applications/gitextensions-portal-recorder.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=Git Extensions portal recorder
Exec=$recorder %U
StartupNotify=false
MimeType=x-scheme-handler/https;text/plain;inode/directory;
EOF
    cat > "$settings_root/config/mimeapps.list" <<'EOF'
[Default Applications]
x-scheme-handler/https=gitextensions-portal-recorder.desktop;
text/plain=gitextensions-portal-recorder.desktop;
inode/directory=gitextensions-portal-recorder.desktop;
EOF
    update-desktop-database "$settings_root/data/applications"
    for content_type in text/plain inode/directory x-scheme-handler/https; do
        gio mime "$content_type" gitextensions-portal-recorder.desktop
    done
    {
        gio mime text/plain
        gio mime inode/directory
        gio mime x-scheme-handler/https
    } > "$evidence_dir/mime-associations.txt"

    env DISPLAY="$host_display" /usr/libexec/xdg-desktop-portal-gtk >"$backend_log" 2>&1 &
    backend_pid=$!
    /usr/libexec/xdg-desktop-portal >"$portal_log" 2>&1 &
    portal_pid=$!
    for _ in {1..80}; do
        if gdbus introspect --session --dest org.freedesktop.portal.Desktop \
            --object-path /org/freedesktop/portal/desktop 2>/dev/null | grep -q 'org.freedesktop.portal.FileChooser'; then
            break
        fi
        sleep 0.25
    done
    gdbus introspect --session --dest org.freedesktop.portal.Desktop \
        --object-path /org/freedesktop/portal/desktop > "$evidence_dir/introspection.txt"
    grep -F 'org.freedesktop.portal.FileChooser' "$evidence_dir/introspection.txt" >/dev/null
    grep -F 'org.freedesktop.portal.OpenURI' "$evidence_dir/introspection.txt" >/dev/null
    dbus-monitor --session "destination='org.freedesktop.portal.Desktop'" >"$dbus_log" 2>&1 &
    monitor_pid=$!
else
    export DBUS_SESSION_BUS_ADDRESS="unix:path=$probe_root/missing-session-bus"
fi

if [[ -n "$flatpak_app_id" ]]; then
    env HOME="$flatpak_host_home" XDG_DATA_HOME="$flatpak_host_data_home" \
    flatpak run --user \
        --nosocket=x11 \
        --nosocket=fallback-x11 \
        --env=XDG_RUNTIME_DIR="$XDG_RUNTIME_DIR" \
        --env=WAYLAND_DISPLAY="$WAYLAND_DISPLAY" \
        --env=XDG_CONFIG_HOME="$XDG_CONFIG_HOME" \
        --env=XDG_DATA_HOME="$XDG_DATA_HOME" \
        --env=XDG_CACHE_HOME="$XDG_CACHE_HOME" \
        --env=GIT_CONFIG_GLOBAL=/dev/null \
        --env=GITEXTENSIONS_DEBUG_FAIL_FAST=1 \
        --env=GITEXTENSIONS_PORTAL_CONFORMANCE_REPORT="$report_for_app" \
        --env=GITEXTENSIONS_PORTAL_CONFORMANCE_EXPECTED="$mode" \
        --env=GITEXTENSIONS_PORTAL_CONFORMANCE_FIXTURE="$fixture_file" \
        --env=LIBGL_ALWAYS_SOFTWARE=1 \
        --env=MESA_LOADER_DRIVER_OVERRIDE=llvmpipe \
        --env=GALLIUM_DRIVER=llvmpipe \
        "$flatpak_app_id" \
        browse \
        "$fixture_repo" >"$stdout_log" 2>"$stderr_log" &
else
    env -u WAYLAND_DISPLAY "$app" browse "$fixture_repo" >"$stdout_log" 2>"$stderr_log" &
fi
app_pid=$!

wait_for_stage()
{
    local expected=$1
    for _ in {1..160}; do
        if [[ -s "$report" ]] && python3 - "$report" "$expected" <<'PY'
import json
import sys
try:
    report = json.load(open(sys.argv[1], encoding="utf-8"))
except (FileNotFoundError, json.JSONDecodeError):
    raise SystemExit(1)
raise SystemExit(0 if report["stage"] == sys.argv[2] else 1)
PY
        then
            return 0
        fi
        kill -0 "$app_pid" 2>/dev/null || { cat "$stderr_log" >&2; return 1; }
        sleep 0.25
    done
    echo "error: timed out waiting for portal probe stage '$expected'" >&2
    [[ -s "$report" ]] && cat "$report" >&2
    return 1
}

complete_picker()
{
    local stage=$1
    local title=$2
    local selection=$3
    wait_for_stage "$stage"
    local window=
    for _ in {1..80}; do
        window=$(DISPLAY="$host_display" xdotool search --name "$title" 2>/dev/null | tail -n 1 || true)
        [[ -n "$window" ]] && break
        sleep 0.25
    done
    [[ -n "$window" ]] || { echo "error: portal dialog '$title' was not found" >&2; return 1; }
    DISPLAY="$host_display" xdotool getwindowname "$window" >> "$evidence_dir/picker-windows.txt"
    DISPLAY="$host_display" xdotool windowactivate --sync "$window" 2>/dev/null || true
    DISPLAY="$host_display" xdotool key ctrl+l
    sleep 0.25
    DISPLAY="$host_display" xdotool type --clearmodifiers "$selection"
    DISPLAY="$host_display" xdotool key Return
    sleep 0.5
    if [[ "$stage" == "openFolderPicker" ]]; then
        DISPLAY="$host_display" xdotool key Down
    fi
    DISPLAY="$host_display" xdotool key alt+s 2>/dev/null || true
    for _ in {1..40}; do
        if [[ -s "$report" ]] && ! python3 - "$report" "$stage" <<'PY'
import json
import sys
try:
    report = json.load(open(sys.argv[1], encoding="utf-8"))
except (FileNotFoundError, json.JSONDecodeError):
    raise SystemExit(0)
raise SystemExit(0 if report["stage"] == sys.argv[2] else 1)
PY
        then
            return 0
        fi
        sleep 0.25
    done
    echo "error: portal dialog '$title' did not accept '$selection'" >&2
    return 1
}

cancel_picker()
{
    local stage=$1
    local title=$2
    wait_for_stage "$stage"
    local window=
    for _ in {1..80}; do
        window=$(DISPLAY="$host_display" xdotool search --name "$title" 2>/dev/null | tail -n 1 || true)
        [[ -n "$window" ]] && break
        sleep 0.25
    done
    [[ -n "$window" ]] || { echo "error: portal cancel dialog '$title' was not found" >&2; return 1; }
    DISPLAY="$host_display" xdotool getwindowname "$window" >> "$evidence_dir/picker-windows.txt"
    DISPLAY="$host_display" xdotool windowactivate --sync "$window" 2>/dev/null || true
    DISPLAY="$host_display" xdotool key Escape
    for _ in {1..40}; do
        if [[ -s "$report" ]] && ! python3 - "$report" "$stage" <<'PY'
import json
import sys
try:
    report = json.load(open(sys.argv[1], encoding="utf-8"))
except (FileNotFoundError, json.JSONDecodeError):
    raise SystemExit(0)
raise SystemExit(0 if report["stage"] == sys.argv[2] else 1)
PY
        then
            return 0
        fi
        sleep 0.25
    done
    echo "error: portal dialog '$title' did not complete cancellation" >&2
    return 1
}

complete_app_chooser()
{
    wait_for_stage openFileAction
    local window=
    for _ in {1..80}; do
        window=$(DISPLAY="$host_display" xdotool search --name 'Open With' 2>/dev/null | tail -n 1 || true)
        [[ -n "$window" ]] && break
        sleep 0.25
    done
    [[ -n "$window" ]] || { echo "error: portal application chooser was not found" >&2; return 1; }
    DISPLAY="$host_display" xdotool getwindowname "$window" >> "$evidence_dir/picker-windows.txt"
    DISPLAY="$host_display" xdotool windowactivate --sync "$window" 2>/dev/null || true
    DISPLAY="$host_display" xdotool key Down
    DISPLAY="$host_display" xdotool key Return
    for _ in {1..40}; do
        if [[ -s "$report" ]] && ! python3 - "$report" <<'PY'
import json
import sys
try:
    report = json.load(open(sys.argv[1], encoding="utf-8"))
except (FileNotFoundError, json.JSONDecodeError):
    raise SystemExit(0)
raise SystemExit(0 if report["stage"] == "openFileAction" else 1)
PY
        then
            return 0
        fi
        sleep 0.25
    done
    echo "error: portal application chooser did not complete" >&2
    return 1
}

if [[ "$mode" == "present" ]]; then
    complete_picker openFilePicker 'P8.2 Open file' "$fixture_file"
    complete_picker openFolderPicker 'P8.2 Open folder' "$fixture_repo"
    complete_picker saveFilePicker 'P8.2 Save file' "$probe_root/p82-saved.txt"
    cancel_picker cancelFilePicker 'P8.3 Cancel file'
    complete_app_chooser
fi
wait_for_stage completed

if [[ -n "$flatpak_app_id" ]]; then
    cp "$report" "$report_output"
fi

if grep -Eiq 'Unhandled exception|fatal error|JIT debugger' "$stdout_log" "$stderr_log"; then
    echo "error: runtime logs contain a failure signature" >&2
    sed -n '1,200p' "$stderr_log" >&2
    exit 1
fi

if [[ "$mode" == "present" ]]; then
    for _ in {1..40}; do
        [[ -s "$launch_log" ]] && [[ $(wc -l < "$launch_log") -ge 3 ]] && break
        sleep 0.25
    done
    [[ -s "$launch_log" ]] && [[ $(wc -l < "$launch_log") -ge 3 ]]
    grep -E 'OpenFile|SaveFile|OpenURI|OpenDirectory' "$dbus_log" > "$evidence_dir/portal-methods.txt"
    for member in OpenFile SaveFile OpenURI OpenDirectory; do
        grep -F "member=$member" "$evidence_dir/portal-methods.txt" >/dev/null
    done
fi

printf 'portal mode %s passed\n' "$mode"
