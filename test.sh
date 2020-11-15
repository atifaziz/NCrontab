#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"
./build.sh
for f in net5 netcoreapp3.1; do {
    dotnet test --no-build NCrontab.Tests -c Debug -f $f \
        -p:CollectCoverage=true \
        -p:CoverletOutputFormat=opencover \
        -p:Exclude=[NUnit*]*
    dotnet test --no-build NCrontab.Tests -c Release -f $f
}
done
