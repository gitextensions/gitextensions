@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0Build\build.ps1""" -c Release -bl -ci  %*"
exit /b %ErrorLevel%
