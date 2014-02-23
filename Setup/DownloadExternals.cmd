@echo off

IF NOT EXIST "%~p0\..\bin\Git-1.9.0-preview20140217.exe" (
    "%~p0\tools\curl.exe" -o ..\bin\Git-1.9.0-preview20140217.exe http://msysgit.googlecode.com/files/Git-1.9.0-preview20140217.exe
)

IF NOT EXIST "%~p0\..\bin\KDiff3-32bit-Setup_0.9.97.exe" (
    "%~p0\tools\curl.exe" -o ..\bin\KDiff3-32bit-Setup_0.9.97.exe http://sourceforge.net/projects/kdiff3/files/kdiff3/0.9.97/KDiff3-32bit-Setup_0.9.97.exe/download -L http://sourceforge.net/ > NUL
)
