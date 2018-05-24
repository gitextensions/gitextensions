@echo off

cd /d "%~p0"

set msbuild="%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
set project=..\GitExtensions.VS2015.sln
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.VS2015.sln
set projectSshAskPass=..\GitExtSshAskPass\GitExtSshAskPass.VS2015.sln
set SkipShellExtRegistration=1
set EnableNuGetPackageRestore=true
..\.nuget\nuget.exe restore %project%
set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

Echo ----------------------------------------------------------------------
Echo Building the release
Echo ----------------------------------------------------------------------
call BuildGitExtNative.cmd Release Rebuild

Echo ----------------------------------------------------------------------
Echo Generating installers
Echo ----------------------------------------------------------------------
call MakeInstallers.cmd
IF ERRORLEVEL 1 EXIT /B 1

%msbuild% %project% /p:Platform="Any CPU" /p:DefineConstants=__MonoCS__ %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

Echo ----------------------------------------------------------------------
Echo Building the portable archive
Echo ----------------------------------------------------------------------
call MakeMonoArchive.cmd
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
