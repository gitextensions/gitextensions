@echo off

cd /d "%~p0"

set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set project=..\GitCommands.VS2010.sln

set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%msbuild% %project% /p:Platform="Any CPU" %msbuildparams%
%msbuild% %project% /p:Platform=x86 %msbuildparams%
%msbuild% %project% /p:Platform=x64 %msbuildparams%

call MakeInstallers.bat
