@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

set msbuild32=hMSBuild -notamd64 -no-cache

set solution=..\GitExtensions.sln
..\.nuget\nuget.exe update -self
..\.nuget\nuget.exe restore -Verbosity Quiet %solution%
set msbuildparams=/p:Configuration=%Configuration% /t:Rebuild /nologo /v:m

call BuildGitExtNative.cmd %Configuration% Rebuild
IF ERRORLEVEL 1 EXIT /B 1

%msbuild32% %solution% /p:Platform="Any CPU" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
