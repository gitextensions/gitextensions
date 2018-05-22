#!/bin/bash
DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
mono "$DIR/GitExtensions.exe" "$@" &

