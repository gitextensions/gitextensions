@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0scripts\build.ps1""" -restore -build -bl %*"
exit /b %ErrorLevel%
