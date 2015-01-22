@echo off

REM Also update in Product.wxs and UI\RequiredSoftwareDlg.wxs

IF NOT EXIST "%~p0\..\bin\Git-1.9.5-preview20141217.exe" (
    "%~p0\tools\curl.exe" -L -k -o ..\bin\Git-1.9.5-preview20141217.exe https://github.com/msysgit/msysgit/releases/download/Git-1.9.5-preview20141217/Git-1.9.5-preview20141217.exe
    IF ERRORLEVEL 1 EXIT /B 1
)

IF NOT EXIST "%~p0\..\bin\KDiff3-64bit-Setup_0.9.98-2.exe" (
    "%~p0\tools\curl.exe" -L -k -o ..\bin\KDiff3-64bit-Setup_0.9.98-2.exe https://downloads.sourceforge.net/project/kdiff3/kdiff3/0.9.98/KDiff3-64bit-Setup_0.9.98-2.exe# -L http://sourceforge.net/ > NUL
    IF ERRORLEVEL 1 EXIT /B 1
)
