<#
.SYNOPSIS
    Script that runs inside the Windows Sandbox to setup the initial repo.
.DESCRIPTION
    Sets up the repo in the sandbox.  Alter the code to setup initial state of repo to do your tests.
#>

#Define how to generate initial commit history before opening Git Extensions
function CreateCommits() {
    [Guid]::NewGuid() | Out-File -FilePath .\initial.txt
    git add .
    git commit -m "Initial commit"
    

}
git init .\Repo

#start GE from Repo
Copy-Item "$env:USERPROFILE\Desktop\Bin\gitex.cmd" .\Repo

Push-Location .\Repo

#Dump initial config to file
git config --list --show-origin >GitConfig.txt

#Create initial git history
CreateCommits



psr /start /output "$TestResults\Recording.zip" /gui 1 /sc 1 /maxsc 999 /slides 1 /sketch 1
#Start Git Extensions in repo.  Waits for GE to close
.\gitex.cmd
psr /stop
#put anything here like validation.  
#Anything sent to output will be put in setup.log include any test gathering
Copy-Item "$env:USERPROFILE\AppData\Roaming\GitExtensions\GitExtensions\GitExtensions.settings" $TestResults
#Dump log
git log --all --show-signature
#Dump current git config
git config --list --show-origin >GitConfig.txt

#show what was different in config from initial setup to when user closed Git Extensions
$initialSha = $(git rev-list --max-parents=0 HEAD | tail -1)
git --no-pager diff $initialSha -- GitConfig.txt > "$TestResults\gitconfig.patch"

#create git bundle of repo to be able to inspect repo later
#https://git-scm.com/docs/git-bundle

$bundlePath = '{0}\{1}.bundle' -f $TestResults, $testID
git bundle create $bundlePath --all

Pop-Location