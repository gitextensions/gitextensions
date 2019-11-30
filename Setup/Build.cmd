@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

set msbuild32=hMSBuild -notamd64 -no-cache

set solution=..\GitExtensions.sln
..\.nuget\nuget.exe update -self
..\.nuget\nuget.exe restore -Verbosity Quiet %solution%

%msbuild32% %solution% /p:Platform="Any CPU" /p:Configuration=%Configuration% /t:Rebuild /nologo /v:m /m /bl
IF ERRORLEVEL 1 EXIT /B 1
