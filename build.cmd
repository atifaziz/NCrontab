@echo off
setlocal
for %%i in (NuGet.exe) do set nuget=%%~dpnx$PATH:i
if "%nuget%"=="" (
    echo WARNING! NuGet executable not found in PATH so build may fail!
    echo For more on NuGet, see https://github.com/nuget/home
)
pushd "%~dp0"
nuget restore && call :build Debug && call :build Release
popd
goto :EOF

:build
setlocal
if "%PROCESSOR_ARCHITECTURE%"=="x86" set MSBUILD=%ProgramFiles%
if defined ProgramFiles(x86) set MSBUILD=%ProgramFiles(x86)%
set MSBUILD=%MSBUILD%\MSBuild\14.0\bin\msbuild
if not exist "%MSBUILD%" (
    echo Microsoft Build Tools 2015 does not appear to be installed on this
    echo machine, which is required to build the solution. You can install
    echo it from the URL below and then try building again:
    echo https://www.microsoft.com/en-us/download/details.aspx?id=48159
    exit /b 1
)
"%MSBUILD%" /p:Configuration=%1 /v:m %2 %3 %4 %5 %6 %7 %8 %9
goto :EOF
