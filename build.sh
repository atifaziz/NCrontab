#!/usr/bin/env bash
set -e
which dotnet 2>/dev/null || {
    echo>&2 .NET Core does not appear to be installed on this machine, which is
    echo>&2 required to build the solution. You can install it from the URL below
    echo>&2 and then try building again:
    echo>&2 https://dot.net
    exit 1
}
cd "$(dirname "$0")"
dotnet restore
for p in NCrontab NCrontab.Signed; do {
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
    for c in Debug Release; do {
        dotnet build --no-restore -c $c -f netcoreapp1.0 $p
    }
    done
}
done
