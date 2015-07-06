@echo off

call DownloadExternals.cmd

cd /d "%~p0"

rem
rem Update this version number with every release
rem
setlocal
set version=2.49a1
set normal=GitExtensions-%version%-Mono.zip
set szip="..\packages\7-Zip.CommandLine.9.20.0\tools\7za"

rd /q /s GitExtensions\
rd /q %normal%
xcopy /y ..\GitExtensions\bin\Release\Git.hub.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\GitCommands.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\GitExtUtils.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\GitExtensions.exe GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\GitExtensions.exe.config GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\GitUI.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\GitUIPluginInterfaces.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\Gravatar.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\Release\TeamCityIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\Release\System.Net.Http.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\Release\System.Net.Http.Primitives.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\ICSharpCode.SharpZipLib.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\ICSharpCode.TextEditor.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Microsoft.WindowsAPICodePack.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Microsoft.WindowsAPICodePack.Shell.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\NBug.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\SmartFormat.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\NetSpell.SpellChecker.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\PSTaskDialog.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\ResourceManager.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Github3\bin\Release\RestSharp.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.Core.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.Interfaces.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.Linq.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.PlatformServices.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\TranslationApp.exe GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\Release\TranslationApp.exe.config GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\AutoCompileSubmodules\bin\Release\AutoCompileSubmodules.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\BackgroundFetch.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\CreateLocalBranches\bin\Release\CreateLocalBranches.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\DeleteUnusedBranches\bin\Release\DeleteUnusedBranches.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\FindLargeFiles\bin\Release\FindLargeFiles.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gerrit\bin\Release\Gerrit.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gerrit\bin\Release\Newtonsoft.Json.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Github3\bin\Release\Github3.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Statistics\GitImpact\bin\Release\GitImpact.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Statistics\GitStatistics\bin\Release\GitStatistics.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gource\bin\Release\Gource.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\ProxySwitcher\bin\Release\ProxySwitcher.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Stash\bin\Release\Stash.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\ReleaseNotesGenerator\bin\Release\ReleaseNotesGenerator.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\English.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Czech.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Czech.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Czech.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Dutch.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Dutch.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Dutch.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\French.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\French.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\French.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\German.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\German.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\German.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Italian.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Italian.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Italian.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Japanese.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Japanese.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Japanese.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Korean.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Korean.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Korean.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Polish.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Polish.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Polish.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Russian.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Russian.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Russian.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y "..\GitUI\Translation\Simplified Chinese.gif" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y "..\GitUI\Translation\Simplified Chinese.xlf" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y "..\GitUI\Translation\Simplified Chinese.Plugins.xlf" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Spanish.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Spanish.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\Spanish.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y "..\GitUI\Translation\Traditional Chinese.gif" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y "..\GitUI\Translation\Traditional Chinese.xlf" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y "..\GitUI\Translation\Traditional Chinese.Plugins.xlf" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Dictionaries GitExtensions\Dictionaries\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Diff-Scripts\merge-* GitExtensions\Diff-Scripts\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Diff-Scripts\*.txt GitExtensions\Diff-Scripts\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\pageant.exe GitExtensions\PuTTY\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\plink.exe GitExtensions\PuTTY\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\puttygen.exe GitExtensions\PuTTY\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\git-credential-winstore.exe GitExtensions\GitCredentialWinStore\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Logo\git-extensions-logo-final-256.ico GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\gitext.sh GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1

set nuget=..\.nuget\nuget.exe
%nuget% install ..\.nuget\packages.config -OutputDirectory ..\packages
%szip% a -tzip %normal% GitExtensions
IF ERRORLEVEL 1 EXIT /B 1