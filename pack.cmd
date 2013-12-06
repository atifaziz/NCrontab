@echo off
call build && call :pack
goto :EOF

:pack
setlocal
pushd "%~dp0\pkg"
if not exist base\lib md base\lib
copy ..\COPYING*.txt base > nul ^
 && copy ..\lic base > nul ^
 && copy ..\bin\Release base\lib > nul ^
 && ..\tools\nuget pack ncrontab.nuspec -b base
