@echo off
pushd "%~dp0"
call build && dotnet test --no-restore --no-build NCrontab.Tests
popd
