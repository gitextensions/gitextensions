#!/bin/bash
set -euo pipefail
root=$1
sources=$2
native_git=$3
scenario=$4
if command -v cygpath >/dev/null 2>&1; then
    root=$(cygpath -u "$root")
    sources=$(cygpath -u "$sources")
    native_git=$(cygpath -u "$native_git")
fi
export HOME="$root/home" XDG_CACHE_HOME="$root/cache" GIT_CONFIG_GLOBAL="$root/global" GIT_CONFIG_SYSTEM="$root/system" GIT_CONFIG_NOSYSTEM=1
mkdir -p "$HOME" "$root/package/libexec/git-core" "$root/host-bin" "$root/repository"
: > "$GIT_CONFIG_GLOBAL"
: > "$GIT_CONFIG_SYSTEM"
export GITEXTENSIONS_HOST_TEST_ROOT="$root" GITEXTENSIONS_HOST_TEST_VERSION=2.53.0 GITEXTENSIONS_HOST_TEST_EXIT=0
printf '#!/bin/bash\nprintf "%%s\\0" "$@" >> %q\nexec %q "$@"\n' "$root/bundled.log" "$native_git" > "$root/package/libexec/git-core/git"
chmod +x "$root/package/libexec/git-core/git"
bash "$sources/gitextensions-install-tools.sh" "$root/package" >/dev/null
rm -f "$root/bundled.log"
export GITEXTENSIONS_FLATPAK_PREFIX="$root/package" GITEXTENSIONS_FLATPAK_SPAWN="$root/spawn"
export GITEXTENSIONS_HOST_GIT_SELECTED=1
export GITEXTENSIONS_HOST_GIT_MISSING_MESSAGE='Translated: missing host Git; using bundled Git.'
export GITEXTENSIONS_HOST_GIT_OLD_MESSAGE='Translated: old host Git; using bundled Git.'
export GITEXTENSIONS_HOST_GIT_UNAVAILABLE_MESSAGE='Translated: host Git unavailable; using bundled Git.'
cat > "$root/spawn" <<'SH'
#!/bin/bash
printf '%s\0' "$@" >> "$GITEXTENSIONS_HOST_TEST_ROOT/spawn.log"
if [[ ${GITEXTENSIONS_HOST_TEST_DENIED:-} = 1 ]]; then exit 77; fi
while [[ $# -gt 0 ]]; do
    case "$1" in
        --directory=*) cd -- "${1#*=}"; shift;;
        --env=*) export "${1#*=}"; shift;;
        --unset-env=*) unset "${1#*=}"; shift;;
        --host|--watch-bus|--forward-fd=9) shift;;
        *) break;;
    esac
done
export PATH="$GITEXTENSIONS_HOST_TEST_ROOT/host-bin"
exec "$@"
SH
cat > "$root/host-bin/git" <<'SH'
#!/bin/bash
if [[ ${1:-} = --version ]]; then printf 'git version %s\n' "$GITEXTENSIONS_HOST_TEST_VERSION"; exit 0; fi
printf '%s\0' "$PWD" "$@" > "$GITEXTENSIONS_HOST_TEST_ROOT/host-arguments.log"
printf '%s\n' "${GIT_AUTHOR_NAME:-}" "${GIT_CONFIG_PARAMETERS:-}" "${GIT_EDITOR:-}" "${GIT_SEQUENCE_EDITOR:-}" "${GIT_SSH:-}" "${GIT_SSH_COMMAND:-}" > "$GITEXTENSIONS_HOST_TEST_ROOT/host-environment.log"
/bin/cat > "$GITEXTENSIONS_HOST_TEST_ROOT/host-stdin.log"
printf 'host-output\n'
printf 'host-error\n' >&2
exit "$GITEXTENSIONS_HOST_TEST_EXIT"
SH
chmod +x "$root/spawn" "$root/host-bin/git"
arguments=(status --porcelain -- 'name with spaces' '$(literal)')
expected=0
case "$scenario" in
missing) rm "$root/host-bin/git"; arguments=(--version);;
old) export GITEXTENSIONS_HOST_TEST_VERSION=2.42.9; arguments=(--version);;
denied) export GITEXTENSIONS_HOST_TEST_DENIED=1; arguments=(--version);;
minimum) export GITEXTENSIONS_HOST_TEST_VERSION=2.43.0;;
failed-command) export GITEXTENSIONS_HOST_TEST_EXIT=23; expected=23;;
git-error) export GITEXTENSIONS_HOST_TEST_EXIT=1; expected=1;;
forward)
    export GIT_AUTHOR_NAME='Owned author with spaces'
    export GIT_CONFIG_PARAMETERS="'owned.key'='value with spaces'"
    export GIT_EDITOR="host editor with 'quotes'"
    export GIT_SEQUENCE_EDITOR='host sequence editor'
    export GIT_SSH="$root/package/bin/ssh" GIT_SSH_COMMAND="$root/package/bin/ssh -o owned=value"
    ;;
esac
cd "$root/repository"
launcher="$root/package/bin/gitextensions-host-git"
if [[ "$scenario" = cached-bundled-path ]]; then launcher="$root/package/bin/git"; fi
set +e
printf 'literal input $(untouched)\n' | "$launcher" "${arguments[@]}" > "$root/output" 2> "$root/error"
status=$?
set -e
test "$status" = "$expected"
case "$scenario" in
missing|old|denied)
    test -s "$root/bundled.log"
    test ! -e "$root/host-arguments.log"
    grep -F 'git version ' "$root/output"
    case "$scenario" in missing) grep -F 'Translated: missing' "$root/error";; old) grep -F 'Translated: old' "$root/error";; denied) grep -F 'Translated: host Git unavailable' "$root/error";; esac
    ;;
*)
    test ! -e "$root/bundled.log"
    grep -Fx 'host-output' "$root/output"
    grep -Fx 'host-error' "$root/error"
    grep -Fx 'literal input $(untouched)' "$root/host-stdin.log"
    tr '\0' '\n' < "$root/host-arguments.log" > "$root/arguments.txt"
    grep -Fx "$root/repository" "$root/arguments.txt"
    grep -Fx 'name with spaces' "$root/arguments.txt"
    grep -Fx '$(literal)' "$root/arguments.txt"
    tr '\0' '\n' < "$root/spawn.log" > "$root/transport.txt"
    grep -Fx -- '--watch-bus' "$root/transport.txt"
    grep -Fx -- '--forward-fd=9' "$root/transport.txt"
    if [[ "$scenario" = forward ]]; then
        grep -Fx 'Owned author with spaces' "$root/host-environment.log"
        grep -Fx "'owned.key'='value with spaces'" "$root/host-environment.log"
        grep -Fx "host editor with 'quotes'" "$root/host-environment.log"
        grep -Fx 'host sequence editor' "$root/host-environment.log"
        grep -Fx 'ssh' "$root/host-environment.log"
        grep -Fx 'ssh -o owned=value' "$root/host-environment.log"
    fi
    ;;
esac
test -z "$(ls -A "$root/cache/GitExtensions/HostGit")"
