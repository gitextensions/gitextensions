# git-mergetool--lib is a shell library for common merge tool functions
# Flatpak adaptation: source the unchanged bundled library and translate only the actual tool launch.
gitextensions_prefix=${GITEXTENSIONS_FLATPAK_PREFIX:-/app}
. "$gitextensions_prefix/libexec/git-core/git-mergetool--lib"

gitextensions_quote () {
    gitextensions_text=$1
    printf "'"
    while :
    do
        case "$gitextensions_text" in
        *"'"*)
            printf '%s' "${gitextensions_text%%"'"*}"
            printf '%s' "'\"'\"'"
            gitextensions_text=${gitextensions_text#*"'"}
            ;;
        *) printf "%s'" "$gitextensions_text"; break ;;
        esac
    done
}

get_merge_tool_cmd () {
    merge_tool="$1"
    if diff_mode
    then
        gitextensions_command=$(git config "difftool.$merge_tool.cmd" ||
            git config "mergetool.$merge_tool.cmd")
    else
        gitextensions_command=$(git config "mergetool.$merge_tool.cmd")
    fi
    gitextensions_status=$?
    if test -z "$gitextensions_command"
    then
        return "$gitextensions_status"
    fi
    gitextensions_script='LOCAL=$1; BASE=$2; REMOTE=$3; MERGED=$4; BACKUP=$5; shift 5; '
    gitextensions_script="$gitextensions_script$gitextensions_command"
    printf '%s /bin/sh -c %s gitextensions-tool "$LOCAL" "$BASE" "$REMOTE" "$MERGED" "$BACKUP" "$@"\n' \
        "$(gitextensions_quote "$gitextensions_prefix/bin/gitextensions-host-tool")" \
        "$(gitextensions_quote "$gitextensions_script")"
}

get_merge_tool_path () {
    # A merge tool has been set, so verify that it's valid.
    merge_tool="$1"
    if ! valid_tool "$merge_tool"
    then
        echo >&2 "Unknown $TOOL_MODE tool $merge_tool"
        exit 1
    fi
    if diff_mode
    then
        merge_tool_path=$(git config difftool."$merge_tool".path ||
                  git config mergetool."$merge_tool".path)
    else
        merge_tool_path=$(git config mergetool."$merge_tool".path)
    fi
    if test -z "$merge_tool_path"
    then
        merge_tool_path=$(translate_merge_tool_path "$merge_tool")
    fi
    if test -n "$(get_merge_tool_cmd "$merge_tool")"
    then
        echo "$merge_tool_path"
        return
    fi
    gitextensions_resolved=$("$gitextensions_prefix/bin/gitextensions-host-tool" --resolve "$merge_tool_path")
    gitextensions_status=$?
    if test "$gitextensions_status" != 0
    then
        case "$merge_tool_path" in
        */*) return "$gitextensions_status" ;;
        *)
            if test "$gitextensions_status" = 44 && test -x "$GITEXTENSIONS_HOST_PROXY_DIR/$merge_tool_path"
            then
                echo "$GITEXTENSIONS_HOST_PROXY_DIR/$merge_tool_path"
                return
            fi
            return "$gitextensions_status"
            ;;
        esac
    fi
    gitextensions_digest=$(printf '%s' "$gitextensions_resolved" | sha256sum)
    gitextensions_proxy="$GITEXTENSIONS_HOST_PROXY_DIR/path-${gitextensions_digest%% *}"
    if test ! -f "$gitextensions_proxy"
    then
        gitextensions_temporary=$(mktemp "$GITEXTENSIONS_HOST_PROXY_DIR/path.XXXXXXXX") || return 1
        printf '%s' "$gitextensions_resolved" > "$gitextensions_temporary.command"
        printf '#!/bin/sh\ncommand=$(cat "$0.command")\nexec %s "$command" "$@"\n' \
            "$(gitextensions_quote "$gitextensions_prefix/bin/gitextensions-host-tool")" > "$gitextensions_temporary"
        chmod 700 "$gitextensions_temporary" &&
            mv -f "$gitextensions_temporary.command" "$gitextensions_proxy.command" &&
            mv -f "$gitextensions_temporary" "$gitextensions_proxy" || return 1
    fi
    echo "$gitextensions_proxy"
}
