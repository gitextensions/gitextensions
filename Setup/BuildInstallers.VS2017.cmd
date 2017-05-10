@echo off

cd /d "%~p0"

set subPathToVsWhere=Microsoft Visual Studio\Installer\vswhere.exe
if exist "%ProgramFiles(x86)%" (
    set vswhere="%ProgramFiles(x86)%\%subPathToVsWhere%"
) else (
    set vswhere="%ProgramFiles%\%subPathToVsWhere%"
)

if not exist %vswhere% (
    echo "Failed to find vswhere.exe, make sure you have installed Visual Studio 15.2.26418.1 or a later version."
    exit /B 1
)

for /f "usebackq tokens=1* delims=: " %%i in (`%vswhere% -latest -requires Microsoft.Component.MSBuild`) do (
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
