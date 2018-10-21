@echo off

cd /d "%~p0"

for /f "tokens=*" %%i in ('hMSBuild.bat -only-path -notamd64') do set msbuild="%%i"
set project=..\GitExtensions.VS2015.sln
set EnableNuGetPackageRestore=true
set SkipShellExtRegistration=1

Echo ----------------------------------------------------------------------
Echo Restore NuGet packages
Echo ----------------------------------------------------------------------
..\.nuget\nuget.exe restore %project%
set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m


Echo ----------------------------------------------------------------------
Echo Building the release
Echo ----------------------------------------------------------------------
%msbuild% ..\GitExtensions.VS2015.sln /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% ..\GitExtensionsShellEx\GitExtensionsShellEx.VS2015.sln /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% ..\GitExtensionsShellEx\GitExtensionsShellEx.VS2015.sln /p:Platform=x64 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% ..\GitExtSshAskPass\GitExtSshAskPass.VS2015.sln /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
