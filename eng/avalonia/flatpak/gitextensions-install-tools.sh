#!/bin/bash
set -euo pipefail
prefix=${1:?usage: gitextensions-install-tools.sh prefix}
source_directory=$(cd -- "$(dirname -- "$0")" && pwd)
core="$prefix/libexec/git-core"
overlay="$prefix/libexec/gitextensions"
test -x "$core/git"
mkdir -p "$prefix/bin" "$overlay"
for entry in "$core"/*; do
    ln -s "$entry" "$overlay/$(basename -- "$entry")"
done
rm -f -- "$overlay/git" "$overlay/git-mergetool--lib" "$prefix/bin/git"
install -m755 "$source_directory/gitextensions-git.sh" "$prefix/bin/git"
install -m755 "$source_directory/gitextensions-host-tool.sh" "$prefix/bin/gitextensions-host-tool"
install -m755 "$source_directory/gitextensions-host-git.sh" "$prefix/bin/gitextensions-host-git"
install -m755 "$source_directory/gitextensions-editor.sh" "$prefix/bin/gitextensions-editor"
install -m755 "$source_directory/gitextensions-editor.sh" "$prefix/bin/gitextensions-sequence-editor"
install -m644 "$source_directory/gitextensions-mergetool-lib.sh" "$overlay/git-mergetool--lib"
ln -s "$prefix/bin/git" "$overlay/git"
"$core/git" --version
