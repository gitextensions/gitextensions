#!/bin/bash
set -u

# parity-scaffolding: the prefix override supplies an owned package fixture in portable tests.
prefix=${GITEXTENSIONS_FLATPAK_PREFIX:-/app}
real_git="$prefix/libexec/git-core/git"
if [[ -z ${GITEXTENSIONS_HOST_PROXY_DIR:-} ]]; then exec "$real_git" "$@"; fi

editor="$prefix/bin/gitextensions-editor"
sequence_editor="$prefix/bin/gitextensions-sequence-editor"
if [[ ${GIT_EDITOR-} != "$editor" ]]; then
    if declare -p GIT_EDITOR >/dev/null 2>&1; then
        export GITEXTENSIONS_ORIGINAL_GIT_EDITOR="$GIT_EDITOR" GITEXTENSIONS_ORIGINAL_GIT_EDITOR_SET=1
    else
        unset GITEXTENSIONS_ORIGINAL_GIT_EDITOR
        export GITEXTENSIONS_ORIGINAL_GIT_EDITOR_SET=0
    fi
fi
if [[ ${GIT_SEQUENCE_EDITOR-} != "$sequence_editor" ]]; then
    if declare -p GIT_SEQUENCE_EDITOR >/dev/null 2>&1; then
        export GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR="$GIT_SEQUENCE_EDITOR" GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR_SET=1
    else
        unset GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR
        export GITEXTENSIONS_ORIGINAL_GIT_SEQUENCE_EDITOR_SET=0
    fi
fi
export GIT_EDITOR="$editor" GIT_SEQUENCE_EDITOR="$sequence_editor"
export GIT_EXEC_PATH="$prefix/libexec/gitextensions"
export PATH="$GITEXTENSIONS_HOST_PROXY_DIR:$PATH"
exec "$real_git" "$@"
