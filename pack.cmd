@echo off
setlocal
pushd "%~dp0"
call :main %*
popd
goto :EOF

:main
for %%i in (NuGet.exe) do set nuget=%%~dpnx$PATH:i
if "%nuget%"=="" goto :nonuget
if not exist dist md dist
if not %errorlevel%==0 exit /b %errorlevel%
call build /v:m ^
    && nuget pack ncrontab.nuspec -OutputDirectory dist -Symbols
goto :EOF

:nonuget
echo NuGet executable not found in PATH
echo For more on NuGet, see http://nuget.codeplex.com
exit /b 2
