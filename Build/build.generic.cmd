@echo off

rem
rem Parameters
rem   %1: vs_solution_version: VS solution version: VS2010, VS2012
rem   %2: vs_configuration:    VS configuration:    Debug, Release
rem   %3: vs_target:           target:              Build or Rebuild
rem

set vs_solution_version=%1
set vs_configuration=%2
set vs_target=%3

rem %~p0 = dir to location of this cmd file
rem cd /d "%~p0"

set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set project="%~p0\..\GitExtensions.%vs_solution_version%.sln"
set SkipShellExtRegistration=1
set EnableNuGetPackageRestore=true

set msbuildparams=/p:Configuration=%vs_configuration% /t:%vs_target% /nologo /v:m

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

rem %msbuild% %project% /p:Platform=x86 %msbuildparams%
rem IF ERRORLEVEL 1 EXIT /B 1
rem %msbuild% %project% /p:Platform=x64 %msbuildparams%
rem IF ERRORLEVEL 1 EXIT /B 1

