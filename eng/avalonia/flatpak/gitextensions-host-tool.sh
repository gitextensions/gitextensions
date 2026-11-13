#!/bin/bash
set -u

# parity-scaffolding: the process override substitutes the host service in portable tests.
spawn=${GITEXTENSIONS_FLATPAK_SPAWN:-/usr/bin/flatpak-spawn}
if [[ -n ${GITEXTENSIONS_HOST_TOOLS_ERROR:-} ]]; then
    printf '%s\n' "$GITEXTENSIONS_HOST_TOOLS_ERROR" >&2
    exit 69
fi
if [[ ! -x "$spawn" ]]; then
    printf '%s\n' 'Host tools are not available in this Flatpak installation. The host launcher is missing.' >&2
    exit 69
fi
if [[ ${1:-} = --resolve ]]; then
    resolved=$("$spawn" --host "--directory=$PWD" /bin/sh -c 'command -v "$1" || exit 44' gitextensions-tool-lookup "$2")
    status=$?
    if [[ "$status" = 0 ]]; then printf '%s\n' "$resolved"; exit 0; fi
    if [[ "$status" = 44 ]]; then exit 44; fi
    printf '%s\n' 'Host tools are not available in this Flatpak installation. The host-command service or permission is unavailable.' >&2
    exit 69
fi
if ! "$spawn" --host /bin/true >/dev/null 2>&1; then
    printf '%s\n' 'Host tools are not available in this Flatpak installation. The host-command service or permission is unavailable.' >&2
    exit 69
fi

arguments=(--host --watch-bus "--directory=$PWD")
while IFS= read -r name; do
    case "$name" in
        GIT_CONFIG_*|HOME|TMPDIR|LANG|LANGUAGE|LC_*|DFT_*) arguments+=("--env=$name=${!name}");;
    esac
done < <(compgen -e)

# The host service creates this command's own process group. Its bus-loss signal must also stop descendants.
exec "$spawn" "${arguments[@]}" /bin/sh -c '
trap '\''kill -KILL "-$$"'\'' INT HUP TERM
exec 3<&0
"$@" <&3 3<&- &
exec 3<&-
child=$!
wait "$child"
exit "$?"
' gitextensions-host-tool "$@"
