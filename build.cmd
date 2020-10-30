@echo off
pushd "%~dp0"
call :main
popd
goto :EOF

:main
setlocal
for %%c in (Debug Release) do (
    call msbuild /p:Configuration=%%c /v:m NCrontabViewer\NCrontabViewer.csproj || exit /b 1
)
:buildlib
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
"%DOTNETEXE%" restore                    ^
  && call :build NCrontab        Debug   ^
  && call :build NCrontab        Release ^
  && call :build NCrontab.Signed Debug   ^
  && call :build NCrontab.Signed Release ^
  && call :build NCrontab.Tests  Debug   ^
  && call :build NCrontab.Tests  Release ^
  && call :build NCrontabConsole Debug   ^
  && call :build NCrontabConsole Release
goto :EOF

:build
"%DOTNETEXE%" build -c %2 %1
goto :EOF
