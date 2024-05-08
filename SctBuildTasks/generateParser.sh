#!/usr/bin/env bash
set -e
# Evil one-liner to find the directory of the script
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

set -x

# Find the antlr command
ANTLR_COMMAND=$(command -v antlr4 || command -v antlr)

pushd "$SCRIPT_DIR"

"$ANTLR_COMMAND" -Dlanguage=CSharp "Sct.g4" -o "out" -visitor -no-listener

popd
