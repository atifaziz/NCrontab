#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"
./build.sh
dotnet test --no-build NCrontab.Tests -c Debug -p:CollectCoverage=true \
                                               -p:CoverletOutputFormat=opencover \
                                               -p:Exclude=[NUnit*]*
dotnet test --no-build NCrontab.Tests -c Release
