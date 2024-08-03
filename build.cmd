@echo off
pushd "%~dp0"
call :main %*
popd
goto :EOF

:main
    dotnet restore ^
 && dotnet build --no-restore -c Debug ^
 && dotnet build --no-restore -c Release
goto :EOF
