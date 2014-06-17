@echo off

IF NOT EXIST "%~p0\..\bin\Git-1.9.2-preview20140411.exe" (
    "%~p0\tools\curl.exe" -k -o ..\bin\Git-1.9.2-preview20140411.exe https://github.com/msysgit/msysgit/releases/download/Git-1.9.2-preview20140411/Git-1.9.2-preview20140411.exe
    IF ERRORLEVEL 1 EXIT /B 1
)

IF NOT EXIST "%~p0\..\bin\KDiff3-32bit-Setup_0.9.97.exe" (
    "%~p0\tools\curl.exe" -k -o ..\bin\KDiff3-32bit-Setup_0.9.97.exe http://sourceforge.net/projects/kdiff3/files/kdiff3/0.9.97/KDiff3-32bit-Setup_0.9.97.exe/download -L http://sourceforge.net/ > NUL
    IF ERRORLEVEL 1 EXIT /B 1
)
