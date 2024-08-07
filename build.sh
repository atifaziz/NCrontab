#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"
for p in NCrontab NCrontab.Signed; do {
    dotnet restore $p
    for c in Debug Release; do {
        for f in netstandard1.0 netstandard2.0; do {
            dotnet build --no-restore -c $c -f $f $p
        }
        done
    }
    done
}
done
for p in NCrontabConsole NCrontab.Tests; do {
    dotnet restore $p
}
done
for c in Debug Release; do {
    dotnet build --no-restore -c $c NCrontabConsole
    for f in net8.0 net6.0; do {
        dotnet build --no-restore -c $c -f $f NCrontab.Tests
    }
    done
}
done
