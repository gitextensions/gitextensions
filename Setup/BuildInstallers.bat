@echo off

cd /d "%~p0"

set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set project=..\GitExtensions.VS2010.sln

set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %project% /p:Platform=x86 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %project% /p:Platform=x64 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

call MakeInstallers.bat
