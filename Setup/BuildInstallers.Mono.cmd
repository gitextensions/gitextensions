rem @echo off

cd /d "%~p0"

if "%MONOPATH%"=="" SET MONOPATH=C:\Program Files (x86)\Mono-2.10.9\bin\

set version=2.46
set msbuild="%MONOPATH%\xbuild"
set project=..\GitExtensionsMono.sln
set nuget=..\.nuget\nuget.exe
set EnableNuGetPackageRestore=true
set szip="..\packages\7-Zip.CommandLine.9.20.0\tools\7za"

%nuget% install ..\.nuget\packages.config -OutputDirectory ..\packages
%nuget% install ..\Plugins\BackgroundFetch\packages.config -OutputDirectory ..\packages

call %msbuild% %project% /t:clean
call %msbuild% %project% /p:TargetFrameworkProfile="" /p:Platform="Any CPU" /p:Configuration=Release /t:Rebuild /nologo /v:m
IF ERRORLEVEL 1 EXIT /B 1

set zipversion=%version:.=%
set normal=GitExtensions%zipversion%Mono.zip
rd /q /s GitExtensions\
rd /q %normal%
xcopy /y ..\GitExtensions\bin\Release\*.exe GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\*.config GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\*.dll GitExtensions\
for /d %%I in (%~p0\..\Plugins\*, %~p0\..\Plugins\Statistics\*) do xcopy /y %%I\bin\Release\*.dll GitExtensions\Plugins\
xcopy /y ..\GitExtensions\bin\Release\Translation GitExtensions\Translation\
xcopy /y ..\bin\Dictionaries GitExtensions\Dictionaries\
xcopy /y ..\bin\Diff-Scripts GitExtensions\Diff-Scripts\
xcopy /y ..\bin\pageant.exe GitExtensions\PuTTY\
xcopy /y ..\bin\plink.exe GitExtensions\PuTTY\
xcopy /y ..\bin\puttygen.exe GitExtensions\PuTTY\
xcopy /y ..\bin\git-credential-winstore.exe GitExtensions\GitCredentialWinStore\
xcopy /y ..\bin\Logo\git-extensions-logo-final-256.ico GitExtensions\
xcopy /y ..\bin\GitExtensionsUserManual.pdf GitExtensions\
xcopy /y ..\bin\gitex.cmd GitExtensions\
%szip% a -tzip %normal% GitExtensions