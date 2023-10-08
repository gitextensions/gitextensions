<#
.SYNOPSIS
	Installs Windows Sandbox feature if needed and then sets up a clean machine test enviroment
.DESCRIPTION
	Starts Windows Sandbox instance and runs a powershell script to setup dependencies and a repo to test gitextensions on a clean machine.
.NOTES
	See https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-overview
	to learn about windows sandbox. 
	Caveat: You must have run a build for this to fully work.  If artifacts folder is clean, there will be errors.
#>
#required to turn on windows feature and Get-WindowsOptionalFeature 
#requires -RunAsAdministrator
Push-Location $PSScriptRoot\wsb
$sandbox = "\Sandbox.wsb"
$sandbox = "$(Join-Path -Path $pwd -ChildPath $sandbox)"
function GenerateSandboxConfig() {
	$templatePath = ".\Sandbox.template"
	
	[xml]$template = [xml]( Get-Content -Path $templatePath)
	$Mapped = $template.Configuration.MappedFolders
	$Mapped.MappedFolder | ForEach-Object {     
		$_.HostFolder = "$(Join-Path -Path $pwd -ChildPath $_.HostFolder -Resolve)"       
	}
	$template.Save($sandbox)
}


function StartWSB() {
	GenerateSandboxConfig
	#Include PSR (problem steps recorder)
	Copy-Item  "$env:SystemRoot\system32\psr.exe" .\
	Start-Process $sandbox
	
}

$test = (Get-WindowsOptionalFeature -Featurename "Containers-DisposableClientVM" -Online)
if (!$test) {
	Write-Warning "Unsupported Operating System: Windows 10 Pro or Enterprise 1903 or greater required."
	throw
}
if ($test.state -eq 'Enabled') {
	Write-Host "  ** Sandbox already installed. Starting it detached." -Foreground Magenta
	StartWSB
}
else {
	$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
	if ($currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
		Write-Warning "A windows restart may be required. If it fails to start reboot and attempt again."
		Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -NoRestart
		StartWSB
	}
	else {
		Write-Error "Must run script as Administrator to install Containers-DisposableClientVM windows optional feature.   Once installed, it is not required."	
	}
}
Pop-Location