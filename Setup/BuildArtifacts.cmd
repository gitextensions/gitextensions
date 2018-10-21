@echo off

cd /d "%~p0"

for /f "tokens=*" %%i in ('hMSBuild.bat -only-path -notamd64') do set msbuild="%%i"
set project=..\GitExtensions.VS2015.sln
set EnableNuGetPackageRestore=true
set SkipShellExtRegistration=1


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
