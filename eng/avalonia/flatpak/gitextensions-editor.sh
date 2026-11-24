#!/bin/bash
set -u
prefix=${GITEXTENSIONS_FLATPAK_PREFIX:-/app}
real_git="$prefix/libexec/git-core/git"
get_core_editor()
{
    if [[ ${GITEXTENSIONS_ORIGINAL_GIT_EDITOR_SET:-0} = 1 ]]; then
        printf '%s' "$GITEXTENSIONS_ORIGINAL_GIT_EDITOR"
    else
        unset GIT_EDITOR
        "$real_git" var GIT_EDITOR
    fi
}
if [[ ${0##*/} = gitextensions-sequence-editor ]]; then
    if [[ ${GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR_SET:-0} = 1 ]]; then
        command=$GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR
    elif command=$("$real_git" config --get sequence.editor); then
        :
    else
        command=$(get_core_editor) || exit $?
    fi
else
    command=$(get_core_editor) || exit $?
fi
exec "$prefix/bin/gitextensions-host-tool" /bin/sh -c "$command \"\$@\"" gitextensions-editor "$@"
