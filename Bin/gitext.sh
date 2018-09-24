#!/bin/bash
DIR=$( cd "$( dirname "$( readlink -f "${BASH_SOURCE[0]}" )" )" && pwd )
mono "$DIR/GitExtensions.exe" "$@" &

