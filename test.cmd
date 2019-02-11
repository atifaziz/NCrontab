@echo off
pushd "%~dp0"
call build ^
  && call :test Debug ^
  && call :test Release ^
popd
goto :EOF

:test
dotnet test --no-restore --no-build -c %1 NCrontab.Tests
goto :EOF
