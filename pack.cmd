@echo off
pushd "%~dp0"
call :main %*
popd
goto :EOF

:main
setlocal
set DOTNETEXE=
for %%f in (dotnet.exe) do set DOTNETEXE=%%~dpnx$PATH:f
if not defined DOTNETEXE set DOTNETEXE=%ProgramFiles%\dotnet
if not exist "%DOTNETEXE%" (
    echo .NET Core does not appear to be installed on this machine, which is
    echo required to build the solution. You can install it from the URL below
    echo and then try building again:
    echo https://dot.net
    exit /b 1
)
set VERSION_SUFFIX=
if not "%~1"=="" set VERSION_SUFFIX=--version-suffix %1
call build && call :pack NCrontab && call :pack NCrontab.Signed
goto :EOF

:pack
setlocal
dotnet pack --no-restore --no-build -c Release %VERSION_SUFFIX% %1
goto :EOF
