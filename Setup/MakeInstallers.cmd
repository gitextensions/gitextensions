@echo off
echo "MakeInstallers current path"
echo %cd%
rem
rem Update this version number with every release
rem
setlocal
set Version=3.1.0
if not "%APPVEYOR_BUILD_VERSION%"=="" (
    set Version=%APPVEYOR_BUILD_VERSION%
)

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

SET BuildType=%2
IF "%BuildType%"=="" SET BuildType=Rebuild

SET AI=%3
IF "%AI%"=="" SET AI="C:\Program Files (x86)\Caphyon\Advanced Installer 16.6.1\bin\x86\AdvancedInstaller.com"

set msbuild=hMSBuild

REM HACK: for some reason when we build the full solution the VSIX contains too many files, clean and rebuild the VSIX
rmdir ..\GitExtensionsVSIX\bin\Release /s /q
pushd ..\GitExtensionsVSIX
set msbuild32=..\Setup\hMSBuild -notamd64
call %msbuild32% /t:%BuildType% /p:Configuration=%Configuration% /nologo /v:m
popd

echo Creating installers for Git Extensions %Version%
echo.

call GenerateInstallerOutput.cmd %Configuration%
IF ERRORLEVEL 1 EXIT /B 1
call GenerateInstallerBuild.cmd %AI% %Configuration% %Version%
IF ERRORLEVEL 1 EXIT /B 1
