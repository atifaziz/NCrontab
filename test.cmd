@echo off
pushd "%~dp0"
dotnet tool restore ^
 && call build ^
 && call :test Debug ^
 && call :test Release ^
 && dotnet reportgenerator -reports:NCrontab.Tests\TestResults\*\coverage.cobertura.xml -targetdir:etc\coverage -reporttypes:TextSummary;Html ^
 && type etc\coverage\Summary.txt
popd && exit /b %ERRORLEVEL%

:test
dotnet test --no-build -s NCrontab.Tests\.runsettings -c %*
goto :EOF
