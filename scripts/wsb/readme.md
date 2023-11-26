# Windows Sandbox

## Overview

To find out about Windows sandbox itself, see <https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-overview> and related pages in that section.  The goal with this tooling is to allow you to test a clean machine environment and the current build of Git Extensions.

Reasons for this tooling:

- Validate first run of Git Extensions without dev settings getting in the way
- Setup a known git and other config by modifying the right Powershell scripts to fit the testing needs
- Gather information from a run that can be put in an issue showing the issue if tracking down an issue by generating a zip of the information captured
- Potential for installer and portable build installation validation of dependencies
- Validate with latest of desktop runtime and git without breaking dev environment
- Altering initial environment in sandbox just takes altering one of two powershell scripts

What this tooling is **not**:

- Fast.  *Start script and get a cup of coffee and come back to do the testing*  
It installs latest git, desktop runtime, and other items each time.  Instead of speed it is repeatable and consistent with clean machine testing
- Designed for everyday debugging
Debug in dev environment.  This environment is for testing the current build as if you were an end user for the first time.

## What this tooling does

1. Installs needed dependencies
    - [Chocolatey](https://chocolatey.org/how-chocolatey-works) is the tool that installs the other software
    - Git
    - [P4Merge](https://www.perforce.com/products/helix-core-apps/merge-diff-tool-p4merge) diff/merge tool
    - [Vs Code](https://code.visualstudio.com/) Good editor
    - Copies [PSR](https://support.microsoft.com/en-us/windows/record-steps-to-reproduce-a-problem-46582a9b-620f-2e36-00c9-04e25d784e47) from host system to use in Sandbox
    - .net desktop runtime 6
1. Preps environment
    - Sets initial git config
    - Sets up initial repo
1. Starts recording with PSR
1. Starts GE and waits for user to close GE
1. Stops recording
1. Gathers details in Test Results folder
1. Zips Test Results folder contents

## How To Use

1. Perform a build if needed.  This tooling wil **not** run a build for you.  If you have not run a build, expect errors to occur in the sandbox.

    ```cmd
    dotnet build /v:q
    ```

1. Run the following in an admin powershell 
*Because of the use of Get-WindowsOptionalFeature admin is required for now. May split off install and use into different scripts as a a fix later on.  If it installs sandbox feature, a restart may be required.  NoRestart is passed in but it may still require a restart.*

    ```powershell
    .\scripts\Start-CleanMachineTesting.ps1
    ```

1. Wait for Git Extensions to start
*May need to minimize sandbox and restore to be able to select language in language selection dialog.  Something with the sandbox and desktop runtime being just installed or something else causes the items to not be clickable till a restore is done.  Still looking into.*
1. Perform whatever actions you need to perform in Git Extensions
1. Close Git Extensions
1. Powershell script will finish and gather details in test results.

## Test Results

- a bundle file that you can point to as a remote and clone the repo that was generated.
- A recording in a zip file that will open in Microsoft Edge
  - To view the recording you must have Internet Explorer mode enabled in Edge.  <https://support.microsoft.com/en-us/microsoft-edge/internet-explorer-mode-in-microsoft-edge-6604162f-e38a-48b2-acd2-682dbac6f0de>
- Full log from setup script
- A gitconfig.patch file that is a diff of initial git config to what the config is after closing Git Extensions
- A copy of GitExtensions.settings file
