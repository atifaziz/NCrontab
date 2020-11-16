@echo off
pushd "%~dp0"
call :main %*
popd
goto :EOF

:main
    call build ^
 && call :test Debug -p:CollectCoverage=true ^
                     -p:CoverletOutputFormat=opencover ^
                     -p:Exclude=[NUnit*]* ^
 && call :test Release
goto :EOF

:test
dotnet test --no-build -c %1 NCrontab.Tests
goto :EOF
