#!/usr/bin/env bash
set -euo pipefail

# parity-scaffolding: exercises host-facing product boundaries in the release-shaped Flatpak.

evidence_dir=${1:-}
if [[ -z "$evidence_dir" ]]; then
    echo "usage: confined-action-sweep.sh <evidence-directory>" >&2
    exit 2
fi

for tool in appstreamcli desktop-file-validate flatpak git ssh-add ssh-agent ssh-keygen weston; do
    command -v "$tool" >/dev/null 2>&1 || { echo "error: required command '$tool' is not installed" >&2; exit 1; }
done

repo_root="$(git rev-parse --show-toplevel)"
app_id=${GITEXTENSIONS_FLATPAK_APP_ID:-com.github.gitextensions.GitExtensions.Avalonia.P83Smoke}
flatpak_home="$HOME/.var/app/$app_id"
action_root="$flatpak_home/data/p83-action-sweep"
action_root_in_sandbox=/var/data/p83-action-sweep
fixture_root="$HOME/.local/share/gitextensions-p83-confined"
fixture_repo="$fixture_root/repository"
remote_repo="$action_root/remote.git"
remote_repo_in_sandbox="$action_root_in_sandbox/remote.git"
ssh_port=45983
outer_display=${DISPLAY:-}

case "$action_root" in
    "$HOME"/.var/app/*/data/p83-action-sweep) ;;
    *) echo "error: refusing unsafe action path '$action_root'" >&2; exit 2 ;;
esac
case "$fixture_root" in
    "$HOME"/.local/share/gitextensions-p83-confined) ;;
    *) echo "error: refusing unsafe fixture path '$fixture_root'" >&2; exit 2 ;;
esac
[[ -n "$outer_display" ]] || { echo "error: DISPLAY is required for confined portal dialogs" >&2; exit 1; }
flatpak info --user "$app_id" >/dev/null

rm -rf -- "$action_root" "$fixture_root"
mkdir -p "$evidence_dir" "$action_root" "$fixture_repo"

permissions_log="$evidence_dir/flatpak-permissions.txt"
runtime_log="$evidence_dir/runtime-assets.txt"
ssh_log="$evidence_dir/ssh-push.txt"
credential_log="$evidence_dir/credential-helper.txt"
mergetool_log="$evidence_dir/mergetool.txt"
process_report="$evidence_dir/process-cancellation.json"
process_report_host="$action_root/process-cancellation.json"
process_report_in_sandbox="$action_root_in_sandbox/process-cancellation.json"
manifest_output="$evidence_dir/manifest.json"

flatpak info --user --show-permissions "$app_id" > "$permissions_log"
grep -F 'sockets=wayland;ssh-auth;' "$permissions_log" >/dev/null
grep -F 'filesystems=host;' "$permissions_log" >/dev/null

desktop-file-validate "$repo_root/eng/avalonia/flatpak/com.github.gitextensions.GitExtensions.Avalonia.desktop"
appstreamcli validate --no-net "$repo_root/eng/avalonia/flatpak/com.github.gitextensions.GitExtensions.Avalonia.metainfo.xml" \
    > "$evidence_dir/appstream-validation.txt"

flatpak run --user --command=sh "$app_id" -c '
set -e
/app/bin/git --version
/app/bin/ssh -V 2>&1
printf "sans=%s\n" "$(fc-match -f "%{family}" "DejaVu Sans")"
printf "mono=%s\n" "$(fc-match -f "%{family}" "DejaVu Sans Mono")"
test -r /app/bin/Themes/dark.css
printf "bundledTheme=/app/bin/Themes/dark.css\n"
test -r /var/config/GitExtensions/GitExtensions/Themes/P83Confined.css
printf "userTheme=/var/config/GitExtensions/GitExtensions/Themes/P83Confined.css\n"
' > "$runtime_log" 2>&1
grep -F 'git version 2.51.0' "$runtime_log" >/dev/null
grep -F 'OpenSSH_10.0' "$runtime_log" >/dev/null
grep -F 'sans=DejaVu Sans' "$runtime_log" >/dev/null
grep -F 'mono=DejaVu Sans Mono' "$runtime_log" >/dev/null

ssh-keygen -q -t ed25519 -N '' -f "$action_root/client"
ssh-keygen -q -t ed25519 -N '' -f "$action_root/host"
cp "$action_root/client.pub" "$action_root/authorized_keys"
chmod 600 "$action_root/authorized_keys"
cat > "$action_root/sshd_config" <<EOF
Port $ssh_port
ListenAddress 127.0.0.1
HostKey $action_root_in_sandbox/host
AuthorizedKeysFile $action_root_in_sandbox/authorized_keys
PidFile $action_root_in_sandbox/sshd.pid
StrictModes no
PasswordAuthentication no
KbdInteractiveAuthentication no
UsePAM no
PrintMotd no
LogLevel VERBOSE
AllowUsers $(id -un)
Subsystem sftp internal-sftp
EOF

flatpak run --user --command=/app/sbin/sshd "$app_id" \
    -D -e -f "$action_root_in_sandbox/sshd_config" \
    > "$action_root/sshd.stdout" 2> "$action_root/sshd.stderr" &
sshd_pid=$!
agent_socket="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}/gitextensions-p83-agent.sock"
rm -f -- "$agent_socket"
SSH_AUTH_SOCK="$agent_socket" ssh-agent -D -a "$agent_socket" > "$action_root/ssh-agent.log" 2>&1 &
agent_pid=$!
weston_pid=
cleanup()
{
    flatpak kill "$app_id" 2>/dev/null || true
    for pid in "${weston_pid:-}" "$sshd_pid" "$agent_pid"; do
        if [[ -n "$pid" ]]; then
            kill -TERM "$pid" 2>/dev/null || true
            wait "$pid" 2>/dev/null || true
        fi
    done
    rm -f -- "$agent_socket"
}
trap cleanup EXIT

sleep 1
kill -0 "$sshd_pid"
kill -0 "$agent_pid"
SSH_AUTH_SOCK="$agent_socket" ssh-add "$action_root/client" >/dev/null
SSH_AUTH_SOCK="$agent_socket" flatpak run --user --command=sh "$app_id" -c \
    'printf "agentSocket=%s\n" "$SSH_AUTH_SOCK"; ssh-add -L' > "$evidence_dir/ssh-agent.txt"
grep -F 'agentSocket=/run/flatpak/ssh-auth' "$evidence_dir/ssh-agent.txt" >/dev/null
grep -F 'ssh-ed25519 ' "$evidence_dir/ssh-agent.txt" >/dev/null

flatpak run --user --command=/app/bin/git "$app_id" init --bare "$remote_repo_in_sandbox" >/dev/null
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" init --initial-branch=main >/dev/null
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" config user.name 'P8.3 Confined Sweep'
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" config user.email p83-confined@example.invalid
printf 'confined push\n' > "$fixture_repo/tracked.txt"
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" add tracked.txt
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" -c commit.gpgSign=false commit -m initial >/dev/null
SSH_AUTH_SOCK="$agent_socket" flatpak run --user \
    --env="GIT_SSH_COMMAND=/app/bin/ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
    --command=/app/bin/git \
    "$app_id" -C "$fixture_repo" push \
    "ssh://$(id -un)@127.0.0.1:$ssh_port$remote_repo_in_sandbox" main \
    > "$ssh_log" 2>&1
flatpak run --user --command=/app/bin/git "$app_id" --git-dir="$remote_repo_in_sandbox" rev-parse refs/heads/main \
    >> "$ssh_log"

cat > "$action_root/credential-input" <<'EOF'
protocol=https
host=p83.example.invalid
username=confined-user
password=confined-secret

EOF
cat > "$action_root/credential-query" <<'EOF'
protocol=https
host=p83.example.invalid

EOF
flatpak run --user --command=sh "$app_id" -c \
    "/app/bin/git -c 'credential.helper=store --file=$action_root_in_sandbox/credentials' credential approve < $action_root_in_sandbox/credential-input"
flatpak run --user --command=sh "$app_id" -c \
    "/app/bin/git -c 'credential.helper=store --file=$action_root_in_sandbox/credentials' credential fill < $action_root_in_sandbox/credential-query" \
    > "$credential_log"
grep -F 'username=confined-user' "$credential_log" >/dev/null
grep -F 'password=confined-secret' "$credential_log" >/dev/null
sed -i 's/^password=.*/password=<verified-redacted>/' "$credential_log"

cat > "$action_root/mergetool.sh" <<EOF
#!/bin/sh
printf 'invoked\n' > '$action_root_in_sandbox/mergetool-invoked'
cp "\$1" "\$2"
EOF
chmod +x "$action_root/mergetool.sh"
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" switch -c other >/dev/null
printf 'other\n' > "$fixture_repo/tracked.txt"
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" commit -am other >/dev/null
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" switch main >/dev/null
printf 'main\n' > "$fixture_repo/tracked.txt"
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" commit -am main >/dev/null
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" merge other >/dev/null 2>&1 || true
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" config merge.tool p83
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" config mergetool.p83.cmd \
    "$action_root_in_sandbox/mergetool.sh \"\$LOCAL\" \"\$MERGED\""
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" config mergetool.p83.trustExitCode true
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" mergetool --no-prompt \
    > "$mergetool_log" 2>&1
grep -F 'invoked' "$action_root/mergetool-invoked" >/dev/null
flatpak run --user --command=/app/bin/git "$app_id" -C "$fixture_repo" diff --check >> "$mergetool_log"

flatpak run --user \
    --env=GITEXTENSIONS_FLATPAK_CONFORMANCE_REPORT="$process_report_in_sandbox" \
    "$app_id"
cp "$process_report_host" "$process_report"
python3 - "$process_report" <<'PY'
import json
import sys
report = json.load(open(sys.argv[1], encoding="utf-8"))
assert report["confined"] is True, report
assert report["processTreeCancellation"] == {
    "cancellationObserved": True,
    "childProcessTerminated": True,
}, report
assert report["passed"] is True, report
PY

weston_runtime="$fixture_root/weston-runtime"
weston_socket=wayland-flatpak-p83-actions
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
        --log="$evidence_dir/weston.log" > "$evidence_dir/weston.stdout" 2>&1 &
weston_pid=$!
for _ in {1..30}; do
    [[ -S "$weston_runtime/$weston_socket" ]] && break
    kill -0 "$weston_pid" 2>/dev/null || { echo 'error: nested Weston exited' >&2; exit 1; }
    sleep 1
done
[[ -S "$weston_runtime/$weston_socket" ]] || { echo 'error: nested Weston socket was not created' >&2; exit 1; }

GITEXTENSIONS_PORTAL_CONFORMANCE_FLATPAK_APP_ID="$app_id" \
XDG_RUNTIME_DIR="$weston_runtime" \
WAYLAND_DISPLAY="$weston_socket" \
    "$repo_root/eng/avalonia/portal-conformance.sh" "$evidence_dir/portal"

cat > "$manifest_output" <<EOF
{
  "schemaVersion": 1,
  "appId": "$app_id",
  "confined": true,
  "backend": "wayland",
  "filesystemGrant": "--filesystem=host",
  "releaseAssets": "passed",
  "bundledGit": "2.51.0",
  "bundledOpenSsh": "10.0p2",
  "runtimeFonts": "passed",
  "fileDialogsAndCancellation": "passed",
  "externalOpen": "passed",
  "sshAgentPush": "passed",
  "credentialHelper": "passed",
  "mergetool": "passed",
  "themeDirectories": "passed",
  "processGroupCancellation": "passed",
  "externalTerminal": "explicitly-unavailable",
  "gource": "explicitly-unavailable",
  "repositoryLocation": "outsideWorkingTree",
  "evidence": {
    "portal": "portal/manifest.json",
    "sshAgent": "ssh-agent.txt",
    "sshPush": "ssh-push.txt",
    "credentialHelper": "credential-helper.txt",
    "mergetool": "mergetool.txt",
    "processCancellation": "process-cancellation.json",
    "runtimeAssets": "runtime-assets.txt"
  }
}
EOF

printf 'confined action sweep passed; portal=8 ssh=1 credential=1 mergetool=1 theme=2 cancellation=1\n'
