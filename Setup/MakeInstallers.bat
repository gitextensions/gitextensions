@echo off

rem
rem Update this version number with every release
rem
set version=1.92

set msiversion=%version:.=%
set normal=GitExtensions%msiversion%.msi
set complete=GitExtensions%msiversion%Complete.msi

set msbuild="%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe"
set output=bin\Release\GitExtensions.msi
set project=Setup.wixproj

set build=%msbuild% %project% /t:Rebuild /p:Version=%Version% /p:Configuration=Release /nologo /v:m

echo Creating installers for Git Extensions %version%
echo.

del %normal%
del %complete%

echo Building %normal%
%build% /p:IncludeRequiredSoftware=0
copy bin\Release\GitExtensions.msi %normal%

echo Building %complete%
%build% /p:IncludeRequiredSoftware=1
copy bin\Release\GitExtensions.msi %complete%

echo.
pause
