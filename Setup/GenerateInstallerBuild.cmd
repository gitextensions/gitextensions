@ECHO OFF
REM This script generates the MSI using AdvancedInstaller

cd /d "%~p0"
SET AI=%1
SET Configuration=%2
SET Version=%3
SET Output="%CD%\InstallerBuild"
SET ARCH=x64
SET MSI="GitExtensions-%ARCH%-%Configuration%-%Version%.msi"
rd /q /s %Output% 2>nul

ECHO "Configure Advanced Installer Project Version: %Version%"
%AI% /Edit "..\Setup.AI.64\Setup.AI.64.aip" /SetVersion %Version%
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Configure Advanced Installer Project Output Path: %Output%"
%AI% /Edit "..\Setup.AI.64\Setup.AI.64.aip" /SetOutputLocation -BuildName DefaultBuild -Path %Output%
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Configure Advanced Installer Project File Name: %MSI%"
%AI% /Edit "..\Setup.AI.64\Setup.AI.64.aip" /SetPackageName %MSI% -buildname DefaultBuild
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Build Advanced Installer Project: Git Extensions"
%AI% /Build "..\Setup.AI.64\Setup.AI.64.aip"
IF ERRORLEVEL 1 EXIT /B 1