@echo off
pushd "%~dp0"
for %%i in (Debug Release) do "%SystemRoot%\Microsoft.NET\Framework\v3.5\msbuild" /p:Configuration=%%i src\NCrontab.sln
popd
