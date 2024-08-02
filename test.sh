#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"
dotnet tool restore
./build.sh
for f in net8.0 net6.0; do
    for c in Debug Release; do
        dotnet test --no-build -c $c -f $f \
            -s NCrontab.Tests/.runsettings
    done
done
dotnet reportgenerator '-reports:NCrontab.Tests/TestResults/*/coverage.cobertura.xml' \
    -targetdir:etc/coverage \
    '-reporttypes:TextSummary;Html'
cat etc/coverage/Summary.txt
