@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0Build\Build.ps1""" -restore -build -bl %*"
exit /b %ErrorLevel%
