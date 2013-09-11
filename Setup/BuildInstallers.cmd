@echo off

cd /d "%~p0"

set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set project=..\GitExtensions.VS2010.sln
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.VS2010.sln
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

call MakeInstallers.bat
