@echo off

REM Also update in Product.wxs and UI\RequiredSoftwareDlg.wxs
set GIT_VERSION=2.16.1
set GIT_VERSION_MINOR=.2
set KDIFF3_VERSION=0.9.97

md %~p0\cache\ 2> NUL

IF NOT EXIST "%~p0\cache\Git-%GIT_VERSION%%GIT_VERSION_MINOR%-32-bit.exe" (
    "%~p0\tools\curl.exe" -L -k -o %~p0\cache\Git-%GIT_VERSION%%GIT_VERSION_MINOR%-32-bit.exe https://github.com/git-for-windows/git/releases/download/v%GIT_VERSION%.windows%GIT_VERSION_MINOR%/Git-%GIT_VERSION%%GIT_VERSION_MINOR%-32-bit.exe
    IF ERRORLEVEL 1 EXIT /B 1
)

IF NOT EXIST "%~p0\cache\KDiff3-32bit-Setup_%KDIFF3_VERSION%.exe" (
    "%~p0\tools\curl.exe" -L -k -o %~p0\cache\KDiff3-32bit-Setup_%KDIFF3_VERSION%.exe http://sourceforge.net/projects/kdiff3/files/kdiff3/%KDIFF3_VERSION%/KDiff3-32bit-Setup_%KDIFF3_VERSION%.exe/download -L http://sourceforge.net/ > NUL
    IF ERRORLEVEL 1 EXIT /B 1
)
