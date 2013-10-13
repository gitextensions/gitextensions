@echo off

cd /d "%~p0"

if "%MONOPATH%"=="" SET MONOPATH=C:\Program Files (x86)\Mono-2.10.9\bin\

set version=2.48
set msbuild="%MONOPATH%\xbuild"
set project=..\GitExtensionsMono.sln
set nuget=..\.nuget\nuget.exe
set EnableNuGetPackageRestore=true
set szip="..\packages\7-Zip.CommandLine.9.20.0\tools\7za"

%nuget% install ..\.nuget\packages.config -OutputDirectory ..\packages
%nuget% install ..\Plugins\BackgroundFetch\packages.config -OutputDirectory ..\packages
%nuget% install ..\Plugins\BuildServerIntegration\TeamCityIntegration\packages.config -OutputDirectory ..\packages

call %msbuild% %project% /t:clean
call %msbuild% %project% /p:TargetFrameworkProfile="" /p:Platform="Any CPU" /p:Configuration=Release /t:Rebuild /nologo /v:m
IF ERRORLEVEL 1 EXIT /B 1

set zipversion=%version:.=%
set normal=GitExtensions%zipversion%Mono.zip
rd /q /s GitExtensions\
rd /q %normal%
xcopy /y ..\GitExtensions\bin\Release\Git.hub.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\GitCommands.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\GitExtensions.exe GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\GitUI.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\GitUIPluginInterfaces.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\Gravatar.dll GitExtensions\
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\Release\System.Runtime.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\Release\System.Threading.Tasks.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\Release\TeamCityIntegration.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\Release\System.Net.Http*.dll GitExtensions\Plugins\
xcopy /y ..\bin\ICSharpCode.SharpZipLib.dll GitExtensions\
xcopy /y ..\bin\ICSharpCode.TextEditor.dll GitExtensions\
xcopy /y ..\bin\Microsoft.WindowsAPICodePack.dll GitExtensions\
xcopy /y ..\bin\Microsoft.WindowsAPICodePack.Shell.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\NBug.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\SmartFormat.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\NetSpell.SpellChecker.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\PSTaskDialog.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\ResourceManager.dll GitExtensions\
xcopy /y ..\Plugins\Github3\bin\Release\RestSharp.dll GitExtensions\
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.Core.dll GitExtensions\
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.Interfaces.dll GitExtensions\
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.Linq.dll GitExtensions\
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\System.Reactive.PlatformServices.dll GitExtensions\
xcopy /y ..\GitExtensions\bin\Release\TranslationApp.exe GitExtensions\
xcopy /y ..\Plugins\AutoCompileSubmodules\bin\Release\AutoCompileSubmodules.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\BackgroundFetch\bin\Release\BackgroundFetch.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\CreateLocalBranches\bin\Release\CreateLocalBranches.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\DeleteUnusedBranches\bin\Release\DeleteUnusedBranches.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\FindLargeFiles\bin\Release\FindLargeFiles.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\Gerrit\bin\Release\Gerrit.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\Gerrit\bin\Release\Newtonsoft.Json.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\Github3\bin\Release\Github3.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\Statistics\GitImpact\bin\Release\GitImpact.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\Statistics\GitStatistics\bin\Release\GitStatistics.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\Gource\bin\Release\Gource.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\ProxySwitcher\bin\Release\ProxySwitcher.dll GitExtensions\Plugins\
xcopy /y ..\Plugins\ReleaseNotesGenerator\bin\Release\ReleaseNotesGenerator.dll GitExtensions\Plugins\
xcopy /y ..\GitUI\Translation GitExtensions\Translation\
xcopy /y ..\bin\Dictionaries GitExtensions\Dictionaries\
xcopy /y ..\bin\Diff-Scripts\merge-* GitExtensions\Diff-Scripts\
xcopy /y ..\bin\Diff-Scripts\*.txt GitExtensions\Diff-Scripts\
xcopy /y ..\bin\pageant.exe GitExtensions\PuTTY\
xcopy /y ..\bin\plink.exe GitExtensions\PuTTY\
xcopy /y ..\bin\puttygen.exe GitExtensions\PuTTY\
xcopy /y ..\bin\git-credential-winstore.exe GitExtensions\GitCredentialWinStore\
xcopy /y ..\bin\Logo\git-extensions-logo-final-256.ico GitExtensions\
xcopy /y ..\bin\GitExtensionsUserManual.pdf GitExtensions\
xcopy /y ..\bin\gitex.cmd GitExtensions\
%szip% a -tzip %normal% GitExtensions