@echo off
pushd "%~dp0"
call :main %*
popd
goto :EOF

:main
setlocal
set VERSION_SUFFIX=
if not "%~1"=="" set VERSION_SUFFIX=--version-suffix %1
call build && call :pack NCrontab && call :pack NCrontab.Signed
goto :EOF

:pack
setlocal
dotnet pack --no-build -c Release %VERSION_SUFFIX% %1
goto :EOF
