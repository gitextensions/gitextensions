@echo off

rem
rem Update this version number with every release
rem
setlocal
set version=2.51
set numericVersion=2.51.00
if not "%APPVEYOR_BUILD_VERSION%"=="" (
    set version=%APPVEYOR_BUILD_VERSION%
    set numericVersion=%APPVEYOR_BUILD_VERSION%
)

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

SET BuildType=%2
IF "%BuildType%"=="" SET BuildType=Rebuild

set normal=GitExtensions-%Version%-Setup.msi
set complete=GitExtensions-%Version%-SetupComplete.msi
for /f "tokens=*" %%i in ('hMSBuild.bat -only-path -notamd64') do set msbuild="%%i"
set output=bin\%Configuration%\GitExtensions.msi
set project=Setup.wixproj

set build=%msbuild% %project% /t:%BuildType% /p:Version=%version% /p:NumericVersion=%numericVersion% /p:Configuration=%Configuration% /nologo /v:m

echo Creating installers for Git Extensions %version%
echo.

echo Removing %normal%
del %normal% 2> nul

echo Removing %complete%
del %complete% 2> nul

echo.

echo Building %normal%
%build% /p:IncludeRequiredSoftware=0
IF ERRORLEVEL 1 EXIT /B 1
copy %output% %normal%
IF ERRORLEVEL 1 EXIT /B 1

echo Building %complete%
%build% /p:IncludeRequiredSoftware=1
IF ERRORLEVEL 1 EXIT /B 1
copy %output% %complete%
IF ERRORLEVEL 1 EXIT /B 1
