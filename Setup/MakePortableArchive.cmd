@echo off

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release
SET version=%2
if not "%APPVEYOR_BUILD_VERSION%"=="" set version=%APPVEYOR_BUILD_VERSION%
set normal=GitExtensions-Portable-%version%.zip
set szip="..\packages\7-Zip.CommandLine.9.20.0\tools\7za"

rd /q /s GitExtensions\ 2>nul
del %normal% 2>nul

REM Some plugins are not included, like TeamFoundation/TfsIntegration with related dlls

xcopy /y /i ..\GitExtensions\bin\%Configuration%\ConEmu GitExtensions\ConEmu
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\ConEmu.WinForms.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\Git.hub.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitCommands.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitExtUtils.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitExtensions.exe GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitExtensions.exe.config GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitUI.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\System.IO.Abstractions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitUIPluginInterfaces.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\Gravatar.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\ICSharpCode.SharpZipLib.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\ICSharpCode.TextEditor.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Microsoft.WindowsAPICodePack.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\bin\Microsoft.WindowsAPICodePack.Shell.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\NBug.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\SmartFormat.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\NetSpell.SpellChecker.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\PSTaskDialog.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\ResourceManager.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Threading.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Validation.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1

xcopy /y ..\Plugins\AutoCompileSubmodules\bin\%Configuration%\AutoCompileSubmodules.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\%Configuration%\BackgroundFetch.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.Core.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.Interfaces.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.Linq.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.PlatformServices.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Bitbucket\bin\%Configuration%\Bitbucket.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\AppVeyorIntegration\bin\%Configuration%\AppVeyorIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\JenkinsIntegration\bin\%Configuration%\JenkinsIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\%Configuration%\TeamCityIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\CreateLocalBranches\bin\%Configuration%\CreateLocalBranches.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\DeleteUnusedBranches\bin\%Configuration%\DeleteUnusedBranches.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\FindLargeFiles\bin\%Configuration%\FindLargeFiles.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gerrit\bin\%Configuration%\Gerrit.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gerrit\bin\%Configuration%\Newtonsoft.Json.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\GitFlow\bin\%Configuration%\GitFlow.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Github3\bin\%Configuration%\RestSharp.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Github3\bin\%Configuration%\Github3.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gource\bin\%Configuration%\Gource.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\Atlassian.Jira.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\JiraCommitHintPlugin.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\NString.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\ProxySwitcher\bin\%Configuration%\ProxySwitcher.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\ReleaseNotesGenerator\bin\%Configuration%\ReleaseNotesGenerator.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Statistics\GitImpact\bin\%Configuration%\GitImpact.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Statistics\GitStatistics\bin\%Configuration%\GitStatistics.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1

xcopy /y ..\GitUI\Translation\English.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\English.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitUI\Translation\English.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Czech.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Czech.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y ..\GitUI\Translation\Czech.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
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
REM xcopy /y "..\GitUI\Translation\Traditional Chinese.gif" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y "..\GitUI\Translation\Traditional Chinese.xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y "..\GitUI\Translation\Traditional Chinese.Plugins.xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1

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
xcopy /y ..\bin\Logo\git-extensions-logo-final-256.ico GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1

IF "%ARCHIVE_WITH_PDB%"=="" GOTO create_archive
xcopy /y ..\GitExtensions\bin\%Configuration%\ConEmu.WinForms.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\Git.hub.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitCommands.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitExtUtils.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitExtensions.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitUI.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\GitUIPluginInterfaces.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\Gravatar.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\NBug.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\NetSpell.SpellChecker.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\GitExtensions\bin\%Configuration%\ResourceManager.pdb GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1

xcopy /y ..\Plugins\AutoCompileSubmodules\bin\%Configuration%\AutoCompileSubmodules.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BackgroundFetch\bin\%Configuration%\BackgroundFetch.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Bitbucket\bin\%Configuration%\Bitbucket.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\AppVeyorIntegration\bin\%Configuration%\AppVeyorIntegration.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\JenkinsIntegration\bin\%Configuration%\JenkinsIntegration.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\%Configuration%\TeamCityIntegration.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\CreateLocalBranches\bin\%Configuration%\CreateLocalBranches.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\DeleteUnusedBranches\bin\%Configuration%\DeleteUnusedBranches.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\FindLargeFiles\bin\%Configuration%\FindLargeFiles.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gerrit\bin\%Configuration%\Gerrit.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\GitFlow\bin\%Configuration%\GitFlow.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Github3\bin\%Configuration%\Github3.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Gource\bin\%Configuration%\Gource.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\JiraCommitHintPlugin.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\ProxySwitcher\bin\%Configuration%\ProxySwitcher.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\ReleaseNotesGenerator\bin\%Configuration%\ReleaseNotesGenerator.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Statistics\GitImpact\bin\%Configuration%\GitImpact.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y ..\Plugins\Statistics\GitStatistics\bin\%Configuration%\GitStatistics.pdb GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1

:create_archive
set nuget=..\.nuget\nuget.exe
%nuget% update -self
%nuget% install ..\.nuget\packages.config -OutputDirectory ..\packages -Verbosity Quiet
%szip% a -tzip %normal% GitExtensions
IF ERRORLEVEL 1 EXIT /B 1
