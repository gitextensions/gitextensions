@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0scripts\build.ps1""" -c Release -bl -ci  %*"
exit /b %ErrorLevel%
