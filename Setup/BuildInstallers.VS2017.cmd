@echo off

cd /d "%~p0"

IF NOT EXIST "%~p0\tools\vswhere1.0.62.exe" (
    "%~p0\tools\curl.exe" -L -k -o %~p0\tools\vswhere1.0.62.exe https://github.com/Microsoft/vswhere/releases/download/1.0.62/vswhere.exe -L https://github.com > NUL
    IF ERRORLEVEL 1 EXIT /B 1
)

for /f "usebackq tokens=1* delims=: " %%i in (`.\tools\vswhere1.0.62.exe -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" set msbuild="%%j\MSBuild\15.0\Bin\MSBuild.exe"
)

set project=..\GitExtensions.VS2015.sln
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.vcxproj
set projectSshAskPass=..\GitExtSshAskPass\SshAskPass.vcxproj
set nuget=..\.nuget\nuget.exe
set SkipShellExtRegistration=1
set EnableNuGetPackageRestore=true

set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%nuget% install ..\GitUI\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/
%nuget% install ..\GitExtensionsVSIX\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/
%nuget% install ..\Plugins\BackgroundFetch\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/
%nuget% install ..\Plugins\BuildServerIntegration\TeamCityIntegration\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/
%nuget% install packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/
%nuget% install ..\Externals\conemu-inside\ConEmuWinForms\packages.config -OutputDirectory ..\packages -Source https://nuget.org/api/v2/

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
