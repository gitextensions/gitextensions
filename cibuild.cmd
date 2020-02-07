@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0Build\Build.ps1""" -c Debug -binaryLog -ci  %*"
exit /b %ErrorLevel%
