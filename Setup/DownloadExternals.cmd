@echo off

REM Also update in Product.wxs and UI\RequiredSoftwareDlg.wxs

IF NOT EXIST "%~p0\..\bin\Git-2.5.3-64-bit.exe" (
    "%~p0\tools\curl.exe" -L -k -o ..\bin\Git-2.5.3-64-bit.exe https://github.com/git-for-windows/git/releases/download/v2.5.3.windows.1/Git-2.5.3-64-bit.exe
    IF ERRORLEVEL 1 EXIT /B 1
)

IF NOT EXIST "%~p0\..\bin\KDiff3-64bit-Setup_0.9.98-2.exe" (
    "%~p0\tools\curl.exe" -L -k -o ..\bin\KDiff3-64bit-Setup_0.9.98-2.exe https://downloads.sourceforge.net/project/kdiff3/kdiff3/0.9.98/KDiff3-64bit-Setup_0.9.98-2.exe# -L http://sourceforge.net/ > NUL
    IF ERRORLEVEL 1 EXIT /B 1
)
