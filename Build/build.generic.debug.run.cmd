@echo off

call "%~p0\build.generic.cmd" %1 Debug Build

IF %ERRORLEVEL% EQU 0 goto success
echo BUILD FAILED
pause
EXIT /B 1

:success
echo BUILD SUCCEEDED. Start GitExtensions.exe
start "" "%~p0\..\GitExtensions\bin\Debug\GitExtensions.exe"
