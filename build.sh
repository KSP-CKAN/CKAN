#!/bin/sh
set -e

arg0=""
remainingArgs=""

if [ $# -gt 0 ]; then
    arg0="$1"
fi

if [ $# -gt 1 ]; then
    skippedFirst=false
    for i in "$@"; do
        if $skippedFirst; then
            remainingArgs="$remainingArgs $i"
        else
            skippedFirst=true
        fi
    done
fi

rootDir=$(dirname "$0")
scriptFile="$rootDir/build/Build.csproj"

cakeArgs=""

if [ -n "$arg0" ]; then
    case $arg0 in
        -*)
            cakeArgs="$cakeArgs $arg0"
            ;;
        *)
            cakeArgs="$cakeArgs --target=$arg0"
            ;;
    esac
fi

export PATH="$PATH:$HOME/.dotnet/tools"
# shellcheck disable=SC2086
dotnet run --project "$scriptFile" --  $cakeArgs $remainingArgs
exit $?
