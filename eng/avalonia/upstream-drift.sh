#!/usr/bin/env bash
# Reports which upstream changes affect the Avalonia port (see the upstream-sync notes in
# src/app/GitExtensions.Avalonia/README.md).
#
# Compares the recorded last-sync commit with an upstream ref, joins the changed files
# against eng/avalonia/portmap.json, and prints:
#   1. changed files that HAVE a twin        -> action required (port the change)
#   2. build-infrastructure files that changed -> review required
#   3. a count of changed-but-unported files -> no action, ported later at current state
#
# Usage:
#   eng/avalonia/upstream-drift.sh [<upstream-ref>]        report only (default ref: upstream/master)
#   eng/avalonia/upstream-drift.sh [<upstream-ref>] --update-marker   also record <upstream-ref> as the new last-sync
set -euo pipefail

repo_root="$(git rev-parse --show-toplevel)"
portmap="$repo_root/eng/avalonia/portmap.json"
marker_file="$repo_root/eng/avalonia/last-sync"

upstream_ref="upstream/master"
update_marker=0
for arg in "$@"; do
    case "$arg" in
        --update-marker) update_marker=1 ;;
        *) upstream_ref="$arg" ;;
    esac
done

if [[ -f "$marker_file" ]]; then
    # The Windows checkout uses CRLF; strip it so WSL does not pass a trailing CR to git.
    last_sync="$(tr -d '\r\n' < "$marker_file")"
else
    # First run: the merge base of this branch and the upstream ref.
    last_sync="$(git merge-base HEAD "$upstream_ref")"
    echo "No last-sync marker yet; using merge base $last_sync" >&2
fi

if ! git rev-parse --verify --quiet "$upstream_ref" > /dev/null; then
    echo "error: unknown ref '$upstream_ref' (add the upstream remote and fetch first)" >&2
    exit 1
fi

changed="$(git diff --name-status "$last_sync..$upstream_ref" -- \
    'src/app/GitUI' 'src/app/GitCommands' 'src/app/GitExtUtils' 'src/app/GitExtensions.Extensibility' \
    'src/app/GitUIPluginInterfaces' 'src/app/ResourceManager' 'src/app/GitExtensions' \
    'Directory.Build.props' 'Directory.Build.targets' 'Directory.Packages.props')"

CHANGED="$changed" PORTMAP="$portmap" LAST="$last_sync" REF="$upstream_ref" python3 - <<'EOF'
import json
import os

portmap = {
    source: entry
    for source, entry in json.load(open(os.environ["PORTMAP"])).items()
    if isinstance(entry, dict)
}
infrastructure = {"Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props"}

ported, windows_only, infra, unported = [], [], [], []
for line in os.environ["CHANGED"].splitlines():
    if not line.strip():
        continue
    status, _, path = line.partition("\t")
    if path in portmap:
        entry = portmap[path]
        if "twin" in entry:
            ported.append((status, path, entry))
        elif entry["status"] == "windowsOnly":
            windows_only.append((status, path, entry))
        else:
            unported.append(path)
    elif path in infrastructure:
        infra.append((status, path))
    else:
        unported.append(path)

print(f"Upstream drift {os.environ['LAST'][:12]}..{os.environ['REF']}")
print()
if ported:
    print(f"== Ported files with upstream changes ({len(ported)}) — port each change to its twin:")
    for status, path, entry in ported:
        print(f"  [{status}] {path}")
        print(f"        twin: {entry['twin']} (status: {entry['status']}, basedOn: {entry['basedOn'][:12]})")
else:
    print("== No ported file changed upstream.")
print()
if windows_only:
    print(f"== Windows-only classified files changed ({len(windows_only)}) — review the classification and substitute:")
    for status, path, entry in windows_only:
        print(f"  [{status}] {path}")
        print(f"        substitute: {entry['substitute']}")
    print()
if infra:
    print(f"== Build infrastructure changed ({len(infra)}) — check the port's conditional blocks survive:")
    for status, path in infra:
        print(f"  [{status}] {path}")
    print()
print(f"== Changed but not ported yet: {len(unported)} file(s) — no action; they are ported at their then-current state.")
EOF

if [[ $update_marker -eq 1 ]]; then
    git rev-parse "$upstream_ref" > "$marker_file"
    echo
    echo "last-sync marker updated to $(cat "$marker_file") — update basedOn in portmap.json for each twin you sync."
fi
