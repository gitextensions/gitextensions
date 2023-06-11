<#
.SYNOPSIS
    Script that runs inside Windows Sandbox to install dependencies and kick off the setup-repo script.
.DESCRIPTION
    Sets up initial environment for testing Git Extensions in a clean sandbox.
.NOTES
    Alter to fit your desired initial environment as needed.
#>

$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'

function ShowFileExtensions() {
    # http://superuser.com/questions/666891/script-to-set-hide-file-extensions
    Push-Location
    Set-Location HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced
    Set-ItemProperty . HideFileExt "0"
    Pop-Location
    Stop-Process -processName: Explorer -force # This will restart the Explorer service to make this work.
}
 
ShowFileExtensions

Set-Variable -Name 'testID'-Scope 'script' -Value $([guid]::NewGuid().ToString("N")) 
Set-Variable -Name 'TestResults' -Scope 'script' -Value "$env:USERPROFILE\Desktop\TestResults\$testID"
mkdir $TestResults 
Write-Output "Test Result Location: $TestResults"

#Install chocolatey
& { Set-ExecutionPolicy Bypass -Scope Process -Force;
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; 
    Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1')); 
    Import-Module "$env:ProgramData\chocolatey\helpers\chocolateyInstaller.psm1";
    Update-SessionEnvironment;    
}
#install dependencies
choco upgrade git p4merge dotnet-6.0-desktopruntime vscode -y

#adjust path environment variable
$paths = $env:Path -split ';'
$paths += 'C:\Program Files\Git\usr\bin\'
$paths += 'C:\Users\WDAGUtilityAccount\Desktop\artifacts\Debug\bin\GitExtensions\net6.0-windows'
[Environment]::SetEnvironmentVariable('Path', $paths -join ';', [EnvironmentVariableTarget]::User)
Update-SessionEnvironment

#setup a new gpg key
gpg --batch --generate-key .\wsb\gen-gpg.dat 2>&1
#just generate same key info but another key so we can see default vs non default key
gpg --batch --generate-key .\wsb\gen-gpg.dat 2>&1

#Setup soon to expire key.  Show how the key will disapear when seleccting keys an dyou will get an expired key message in gpg tab if you use the key
$dtExpire = ([System.DateTimeOffset]::UTCNow.Add([System.Timespan]::FromMinutes(2))).ToString('yyyyMMddTHHmmss')
$expired = Get-Content .\wsb\gen-gpg.dat 
$expired = $expired -replace 'Expire-Date.*', $('Expire-Date: {0}' -f $dtExpire)
$expired = $expired -replace 'Name-Real.*', 'Name-Real: GitExtensions Tester Should Expire Soon'
$expired | Set-Content -Path $env:USERPROFILE\Desktop\Expired.dat
gpg --batch --generate-key $env:USERPROFILE\Desktop\Expired.dat 2>&1

$keyLines = gpg -K --with-colons | awk -F: '/^sec:/ { print $5 }'

Set-Variable -Name 'keys' -Scope 'script' -Value @{
    DefaultKey = $keyLines[0]
    OtherKey   = $keyLines[1]
    ExpiredKey = $keyLines[2]
}

$keys



gpg --armor --export --export-options export-backup > "$TestResults\Keys.pgp"
#gpg --armor --export-secret-keys --export-options export-backup >> "$TestResults\Keys.pgp"
gpg --export-ownertrust >"$TestResults\gpgTrust.txt"

#configure git
git config --global user.name 'Tester'
git config --global user.email 'Test@test.com'
git config --global user.signingKey "$($keys.DefaultKey)"
git config --global rebase.autosquash 'true'
git config --global rebase.autostash 'true'
git config --global rebase.updaterefs 'true'
git config --global i18n.filesencoding 'utf-8'
git config --global core.autocrlf 'true'
git config --global difftool.p4merge.path 'C:/Program Files/Perforce/p4merge.exe'
git config --global difftool.p4merge.cmd '\"C:/Program Files/Perforce/p4merge.exe\" \"$LOCAL\" \"$REMOTE\"'
git config --global mergetool.p4merge.path 'C:/Program Files/Perforce/p4merge.exe'
git config --global mergetool.p4merge.cmd '\"C:/Program Files/Perforce/p4merge.exe\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"'
git config --global merge.guitool 'p4merge'
git config --global diff.guitool 'p4merge'


#create shortcut to Git Extensions
$TargetFile = "C:\Users\WDAGUtilityAccount\Desktop\artifacts\Debug\bin\GitExtensions\net6.0-windows\GitExtensions.exe"
$ShortcutFile = "$env:Public\Desktop\Git Extensions.lnk"
$WScriptShell = New-Object -ComObject WScript.Shell
$Shortcut = $WScriptShell.CreateShortcut($ShortcutFile)
$Shortcut.TargetPath = $TargetFile
$Shortcut.Save()



Copy-Item "$env:USERPROFILE\Desktop\wsb\psr.exe" "$env:SystemRoot\system32\"
Set-Alias psr "$env:SystemRoot\system32\psr.exe"


#setup repo in sandbox
& $PSScriptRoot\Setup-Repo.ps1

Copy-Item .\setup.log $TestResults


$output = "$env:USERPROFILE\Desktop\TestResults\$testID.zip"
Compress-Archive -Path $TestResults -DestinationPath $output -Verbose 4>&1

Write-Output ''
Write-Output "Test results created at $output." 


