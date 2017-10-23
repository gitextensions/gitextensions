@echo off

cd /d "%~p0"

if "%MONOPATH%"=="" SET MONOPATH=C:\Program Files (x86)\Mono-3.2.3\bin\

set msbuild="%MONOPATH%\xbuild"
set project=..\GitExtensionsMono.sln
set EnableNuGetPackageRestore=true

call %msbuild% %project% /t:clean
call %msbuild% %project% /p:TargetFrameworkProfile="" /p:Platform="Any CPU" /p:Configuration=Release /nologo /v:m
IF ERRORLEVEL 1 EXIT /B 1

call MakeMonoArchive.cmd
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
