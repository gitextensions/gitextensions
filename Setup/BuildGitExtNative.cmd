@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

SET BuildType=%2
IF "%BuildType%"=="" SET BuildType=Rebuild

for /f "tokens=*" %%i in ('hMSBuild.bat -only-path -notamd64') do set msbuild="%%i"
set projectShellEx=..\GitExtensionsShellEx\GitExtensionsShellEx.sln
set projectSshAskPass=..\GitExtSshAskPass\GitExtSshAskPass.sln
set SkipShellExtRegistration=1
set msbuildparams=/p:Configuration=%Configuration% /t:%BuildType% /nologo /v:m

%msbuild% %projectShellEx% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectShellEx% /p:Platform=x64 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1
%msbuild% %projectSshAskPass% /p:Platform=Win32 %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

echo.
IF "%SKIP_PAUSE%"=="" pause
