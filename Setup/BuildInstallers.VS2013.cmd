@echo off

cd /d "%~p0"

set msbuild="%programfiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
set project=..\GitExtensions.VS2013.sln
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.VS2013.sln
set projectSshAskPass=..\GitExtSshAskPass\GitExtSshAskPass.VS2013.sln
set nuget=..\.nuget\nuget.exe
set SkipShellExtRegistration=1
set EnableNuGetPackageRestore=true

set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%nuget% install ..\GitUI\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/
%nuget% install ..\Plugins\BackgroundFetch\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/
%nuget% install ..\Plugins\BuildServerIntegration\TeamCityIntegration\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectShellEx% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectShellEx% /p:Platform=x64 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectSshAskPass% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

call MakeInstallers.cmd
IF ERRORLEVEL 1 EXIT /B 1

%msbuild% %project% /p:Platform="Any CPU" /p:DefineConstants=__MonoCS__ %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

call MakeMonoArchive.cmd
IF ERRORLEVEL 1 EXIT /B 1

echo.
pause
