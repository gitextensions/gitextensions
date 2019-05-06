@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

SET BuildType=%2
IF "%BuildType%"=="" SET BuildType=Rebuild

set msbuild32=hMSBuild -notamd64
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.sln
set projectSshAskPass=..\GitExtSshAskPass\GitExtSshAskPass.sln
set SkipShellExtRegistration=1
set msbuildparams=/p:Configuration=%Configuration% /t:%BuildType% /nologo /v:m

%msbuild32% %projectShellEx% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild32% %projectShellEx% /p:Platform=x64 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild32% %projectSshAskPass% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1


