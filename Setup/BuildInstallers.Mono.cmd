@echo off

cd /d "%~p0"

if "%MONOPATH%"=="" SET MONOPATH=C:\Program Files (x86)\Mono-3.2.3\bin\

set msbuild="%MONOPATH%\xbuild"
set project=..\GitExtensionsMono.sln
set nuget=..\.nuget\nuget.exe
set EnableNuGetPackageRestore=true

%nuget% install ..\Plugins\BackgroundFetch\packages.config -OutputDirectory ..\packages
%nuget% install ..\Plugins\BuildServerIntegration\TeamCityIntegration\packages.config -OutputDirectory ..\packages

call %msbuild% %project% /t:clean
call %msbuild% %project% /p:TargetFrameworkProfile="" /p:Platform="Any CPU" /p:Configuration=Release /nologo /v:m
IF ERRORLEVEL 1 EXIT /B 1

call MakeMonoArchive.cmd
IF ERRORLEVEL 1 EXIT /B 1

echo.
pause
