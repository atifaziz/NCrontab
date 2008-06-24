@echo off
pushd "%~dp0"
for %%i in (Debug Release) do msbuild /p:Configuration=%%i src\NCrontab.sln
popd
