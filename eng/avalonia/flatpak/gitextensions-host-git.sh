#!/bin/bash
set -u

# parity-scaffolding: prefix and transport overrides supply owned package/service fixtures in tests.
prefix=${GITEXTENSIONS_FLATPAK_PREFIX:-/app}
spawn=${GITEXTENSIONS_FLATPAK_SPAWN:-/usr/bin/flatpak-spawn}
cache=${XDG_CACHE_HOME:-${TMPDIR:-/tmp}}/GitExtensions/HostGit
fallback()
{
    case "$1" in
        missing) message=${GITEXTENSIONS_HOST_GIT_MISSING_MESSAGE:-Host Git was not found. Using the bundled Git.};;
        old) message=${GITEXTENSIONS_HOST_GIT_OLD_MESSAGE:-Host Git is older than the required version. Using the bundled Git.};;
        *) message=${GITEXTENSIONS_HOST_GIT_UNAVAILABLE_MESSAGE:-Host Git is not available in this Flatpak installation. Using the bundled Git.};;
    esac
    shift
    printf '%s\n' "$message" >&2
    export GITEXTENSIONS_HOST_GIT_FALLBACK=1
    exec "$prefix/bin/git" "$@"
}

if [[ ! -x "$spawn" ]]; then fallback unavailable "$@"; fi
if ! mkdir -p "$cache" || ! chmod 700 "$cache"; then fallback unavailable "$@"; fi
state=$(mktemp "$cache/request.XXXXXXXX") || fallback unavailable "$@"
trap 'rm -f -- "$state"' EXIT
exec 8< "$state"
exec 9> "$state"
rm -f -- "$state"
trap - EXIT
arguments=(--host --watch-bus --forward-fd=9 "--directory=$PWD")
while IFS= read -r name; do
    case "$name" in
        GIT_EXEC_PATH|GIT_TEMPLATE_DIR) continue;;
        GIT_EDITOR)
            if [[ ${!name} = "$prefix/bin/gitextensions-editor" ]]; then
                if [[ ${GITEXTENSIONS_ORIGINAL_GIT_EDITOR_SET:-0} = 1 ]]; then
                    arguments+=("--env=$name=$GITEXTENSIONS_ORIGINAL_GIT_EDITOR")
                else
                    arguments+=("--unset-env=$name")
                fi
                continue
            fi
            ;;
        GIT_SEQUENCE_EDITOR)
            if [[ ${!name} = "$prefix/bin/gitextensions-sequence-editor" ]]; then
                if [[ ${GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR_SET:-0} = 1 ]]; then
                    arguments+=("--env=$name=$GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR")
                else
                    arguments+=("--unset-env=$name")
                fi
                continue
            fi
            ;;
        GIT_SSH)
            if [[ ${!name} = "$prefix/bin/ssh" ]]; then arguments+=(--env=GIT_SSH=ssh); continue; fi
            ;;
        GIT_SSH_COMMAND)
            value=${!name}
            if [[ $value = "$prefix/bin/ssh "* ]]; then
                arguments+=("--env=$name=ssh ${value#"$prefix/bin/ssh "}")
                continue
            fi
            ;;
        SSH_AUTH_SOCK)
            # The host service retains its real agent socket, not Flatpak's mapped socket name.
            if [[ ${!name} = /run/flatpak/ssh-auth ]]; then continue; fi
            ;;
    esac
    case "$name" in
        GIT_*|GCM_*|DFT_*|SSH_ASKPASS|SSH_ASKPASS_REQUIRE|SSH_AUTH_SOCK|HOME|TERM|LANG|LANGUAGE|LC_*|DISPLAY|TMPDIR)
            arguments+=("--env=$name=${!name}")
            ;;
    esac
done < <(compgen -e)

minimum=${GITEXTENSIONS_HOST_GIT_MIN_VERSION:-2.43.0}
"$spawn" "${arguments[@]}" /bin/sh -c '
trap '\''kill -KILL "-$$"'\'' INT HUP TERM
exec 3<&0
/bin/sh -c '\''
minimum=$1
shift
if ! command -v git >/dev/null 2>&1; then printf "missing\n" >&9; exit 69; fi
version=$(git --version) || { printf "missing\n" >&9; exit 69; }
version=${version#git version }
major=${version%%.*}
rest=${version#*.}
minor=${rest%%.*}
patch=${rest#*.}
patch=${patch%%[!0-9]*}
required_major=${minimum%%.*}
rest=${minimum#*.}
required_minor=${rest%%.*}
required_patch=${rest#*.}
case "$major:$minor:${patch:-0}" in *[!0-9:]*|:*|*::*) printf "old\n" >&9; exit 69;; esac
if test "$major" -lt "$required_major" ||
   { test "$major" -eq "$required_major" && test "$minor" -lt "$required_minor"; } ||
   { test "$major" -eq "$required_major" && test "$minor" -eq "$required_minor" && test "${patch:-0}" -lt "$required_patch"; }
then
    printf "old\n" >&9
    exit 69
fi
printf "started\n" >&9
exec 9>&-
# parity-scaffolding: records the actual host Git PID for the confined cancellation proof.
export GITEXTENSIONS_HOST_GIT_PID=$$
exec git "$@"
'\'' gitextensions-host-git-version "$@" <&3 3<&- &
exec 3<&-
child=$!
wait "$child"
exit "$?"
' gitextensions-host-git "$minimum" "$@"
status=$?
exec 9>&-
IFS= read -r disposition <&8 || disposition=
exec 8<&-
if [[ "$disposition" = started ]]; then exit "$status"; fi
fallback "${disposition:-unavailable}" "$@"
