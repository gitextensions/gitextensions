@echo off

REM Also update in Product.wxs and UI\RequiredSoftwareDlg.wxs

md %~p0\cache\ 2> NUL

IF NOT EXIST "%~p0\cache\Git-2.12.2-32-bit.exe" (
    "%~p0\tools\curl.exe" -L -k -o %~p0\cache\Git-2.12.2-32-bit.exe https://github.com/git-for-windows/git/releases/download/v2.12.2.windows.1/Git-2.12.2-32-bit.exe
    IF ERRORLEVEL 1 EXIT /B 1
)

IF NOT EXIST "%~p0\cache\KDiff3-32bit-Setup_0.9.98-3.exe" (
    "%~p0\tools\curl.exe" -L -k -o %~p0\cache\KDiff3-32bit-Setup_0.9.98-3.exe https://sourceforge.net/projects/kdiff3/files/kdiff3/0.9.98/KDiff3-32bit-Setup_0.9.98-3.exe/download
    IF ERRORLEVEL 1 EXIT /B 1
)
