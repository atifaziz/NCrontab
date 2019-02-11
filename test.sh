#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"
VERSION_SUFFIX=
if [ ! -z "$1" ]; then VERSION_SUFFIX="--version-suffix $1"; fi
./build.sh
for c in Debug Release; do
    dotnet test --no-restore --no-build -f netcoreapp1.0 -c $c NCrontab.Tests
done
