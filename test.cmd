@echo off
pushd "%~dp0"
call build && dotnet test NCrontab.Tests\NCrontab.Tests.csproj
popd
