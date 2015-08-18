@echo off
setlocal
pushd "%~dp0"
call :main %*
popd
goto :EOF

:main
set MSBUILDEXE=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
if not exist "%MSBUILDEXE%" (
    echo The .NET Framework 4.0 does not appear to be installed on this 
    echo machine, which is required to build the solution.
    exit /b 1
)
set EnableNuGetPackageRestore=true
for %%s in (src\*.sln) do for %%c in (debug release) do "%MSBUILDEXE%" %%s /p:Configuration=%%c /v:m %*
