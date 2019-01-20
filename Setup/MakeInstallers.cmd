@echo off
echo "MakeInstallers current path"
echo %cd%
rem
rem Update this version number with every release
rem
setlocal
set version=3.1.0
set numericVersion=3.1.0
if not "%APPVEYOR_BUILD_VERSION%"=="" (
    set version=%APPVEYOR_BUILD_VERSION%
    set numericVersion=%APPVEYOR_BUILD_VERSION%
)

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

SET BuildType=%2
IF "%BuildType%"=="" SET BuildType=Rebuild

set normal=GitExtensions-%Version%.msi
for /f "tokens=*" %%i in ('hMSBuild.bat -only-path -notamd64') do set msbuild="%%i"
set output=bin\%Configuration%\GitExtensions.msi

REM HACK: for some reason when we build the full solution the VSIX contains too many files, clean and rebuild the VSIX
rmdir ..\GitExtensionsVSIX\bin\Release /s /q
pushd ..\GitExtensionsVSIX
%msbuild% /t:%BuildType% /p:Configuration=%Configuration% /nologo /v:m
popd

echo Creating installers for Git Extensions %version%
echo.

echo Removing %normal%
del %normal% 2> nul

echo.

%msbuild% Setup.wixproj /t:%BuildType% /p:Version=%version% /p:NumericVersion=%numericVersion% /p:Configuration=%Configuration% /nologo /v:m
IF ERRORLEVEL 1 EXIT /B 1
copy %output% %normal%
IF ERRORLEVEL 1 EXIT /B 1
