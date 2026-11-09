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
export HOME="$root/home" GIT_CONFIG_GLOBAL="$root/global" GIT_CONFIG_SYSTEM="$root/system" GIT_CONFIG_NOSYSTEM=1
mkdir -p "$HOME" "$root/package/libexec/git-core/mergetools" "$root/proxies" "$root/repository"
: > "$GIT_CONFIG_GLOBAL"
: > "$GIT_CONFIG_SYSTEM"
core="$root/package/libexec/git-core"
native_core=$("$native_git" --exec-path)
if command -v cygpath >/dev/null 2>&1; then native_core=$(cygpath -u "$native_core"); fi
printf '#!/bin/bash\nexec %q "$@"\n' "$native_git" > "$core/git"
chmod +x "$core/git"
cp "$native_core/git-mergetool--lib" "$native_core/git-difftool--helper" "$core/"
cp "$native_core/mergetools/meld" "$core/mergetools/"
bash "$sources/gitextensions-install-tools.sh" "$root/package"
export GITEXTENSIONS_FLATPAK_PREFIX="$root/package" GITEXTENSIONS_HOST_PROXY_DIR="${GITEXTENSIONS_TEST_PROXY_DIR:-$root/proxies}"
export GITEXTENSIONS_HOST_TOOL="$root/package/bin/gitextensions-host-tool"
export GITEXTENSIONS_FLATPAK_SPAWN="$root/spawn" GITEXTENSIONS_SCRIPT_LOG="$root/host.log"
cat > "$root/spawn" <<'EOF'
#!/bin/bash
if [[ ${GITEXTENSIONS_TEST_DENIED:-} = 1 ]]; then exit 1; fi
printf '%s\0' "$@" >> "$GITEXTENSIONS_SCRIPT_LOG.spawn"
if [[ ${GITEXTENSIONS_TEST_MANAGED_PROXY:-} = 1 && " $* " = *' gitextensions-tool-lookup meld '* ]]; then exit 44; fi
while [[ $# -gt 0 ]]; do
    case "$1" in
        --directory=*) cd -- "${1#*=}"; shift;;
        --env=*) export "${1#*=}"; shift;;
        --host|--watch-bus) shift;;
        *) break;;
    esac
done
arguments=()
for argument do
    if [[ "$argument" = /host/flatpak ]]; then arguments+=("$GITEXTENSIONS_TEST_FLATPAK"); else arguments+=("$argument"); fi
done
exec "${arguments[@]}"
EOF
cat > "$root/tool" <<'EOF'
#!/bin/bash
printf '%s\0' "$PWD" "$@" > "$GITEXTENSIONS_SCRIPT_LOG"
if [[ ${GITEXTENSIONS_TEST_EDIT:-} = 1 ]]; then printf 'Edited by host tool\n' > "$1"; fi
EOF
chmod +x "$root/spawn" "$root/tool"
git="$root/package/bin/git"
cd "$root/repository"
"$git" init -q
"$git" config user.name 'Owned regression'
"$git" config user.email 'owned@example.invalid'
"$git" config commit.gpgsign false
printf 'before\n' > file.txt
"$git" add file.txt
"$git" commit -qm base
printf 'after\n' > file.txt
case "$scenario" in
diff-command)
    command="'${root}/tool' \"\$LOCAL\" \"\$REMOTE\""
    "$git" config difftool.owned.cmd "$command"
    before=$(cat .git/config)
    "$git" difftool --no-prompt --tool=owned
    test "$(cat .git/config)" = "$before"
    ;;
diff-environment)
    "$git" config difftool.difftastic.cmd "'${root}/tool' \"\$LOCAL\" \"\$REMOTE\""
    read -r -a diff_arguments <<< "$GITEXTENSIONS_TEST_DIFF_ARGUMENTS"
    before=$(cat .git/config)
    "$git" difftool --no-prompt "${diff_arguments[@]}"
    test "$(cat .git/config)" = "$before"
    ;;
diff-path)
    "$git" config difftool.meld.path "$root/tool"
    before=$(cat .git/config)
    "$git" difftool --no-prompt --tool=meld
    test "$(cat .git/config)" = "$before"
    ;;
diff-managed-proxy)
    export GITEXTENSIONS_TEST_MANAGED_PROXY=1 GITEXTENSIONS_TEST_FLATPAK="$root/flatpak"
    printf '#!/bin/bash\nprintf "%%s\\0" "$@" > "$GITEXTENSIONS_SCRIPT_LOG"\n' > "$root/flatpak"
    chmod +x "$root/flatpak"
    before=$(cat .git/config)
    "$git" difftool --no-prompt --tool=meld
    test "$(cat .git/config)" = "$before"
    ;;
editor)
    "$git" config core.editor "'${root}/tool'"
    export GITEXTENSIONS_TEST_EDIT=1
    before=$(cat .git/config)
    "$git" commit --allow-empty -q
    test "$("$git" log -1 --format=%s)" = 'Edited by host tool'
    test "$(cat .git/config)" = "$before"
    ;;
sequence-editor)
    export GIT_SEQUENCE_EDITOR="'${root}/tool'"
    "$git" rebase --interactive --root --autostash >/dev/null
    ;;
denied)
    export GITEXTENSIONS_TEST_DENIED=1
    if "$root/package/bin/gitextensions-host-tool" /bin/true 2> "$root/error"; then exit 1; fi
    grep -F 'not available in this Flatpak installation' "$root/error"
    exit 0
    ;;
esac
test -s "$GITEXTENSIONS_SCRIPT_LOG"
test -s "$GITEXTENSIONS_SCRIPT_LOG.spawn"
tr '\0' '\n' < "$GITEXTENSIONS_SCRIPT_LOG"
