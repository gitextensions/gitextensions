<#
.SYNOPSIS
    Starts a bisect run session between Good and Bad commits and runs the Bisect-Test.ps1 script on each checkout allowing user to confirm feature is working or broken. Generates a Bisect.log file for use in reporting an issue.
.DESCRIPTION
    This is used to track down some feature breaking.  It will build the project and run it and then ask if it behaved as expected.  If not you can describe why and this is captured in log.  
    See https://git-scm.com/docs/git-bisect/#_bisect_run
.PARAMETER Bad 
    git reference of commit that is known broken
.PARAMETER Good
    git reference of commit that is known working
.PARAMETER Skip
    Array of git references to skip in bisect.  git bisect skip <the value> is called per item in array
    -Skip 4a4674c07,dae1b7949
.PARAMETER Force
    Skips asking user if they want to allow the cleans to run in test script.
.PARAMETER FirstParent
    Passes --First-Parent to git bisect start.  Allows you to skip commits that are not first parents.
.EXAMPLE
    .\eng\Run-BisectTest -Bad 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 -Good ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f
    Starts a bicect between 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 and ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f asking user if the cleans can be run before building.
.EXAMPLE
    .\eng\Run-BisectTest -Bad 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 -Good ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f -FirstParent
    Starts a bicect between 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 and ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f 
    Asks user if the cleans can be run before building. 
    Passes --first-parent to git bisect start to only bisect using first parents, skipping commits inside of merge commits.
.EXAMPLE
    .\eng\Run-BisectTest -Bad 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 -Good ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f -Force
    Starts a bicect between 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 and ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f.
    Skips asking user if they want to allow the git clean runs in the test script.
.EXAMPLE
    .\eng\Run-BisectTest -Bad 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 -Good ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f -Skip 25060c3b0683c710cd5b47cdfc550b7f8a3b4940
    Starts a bicect between 0e69be5ffd82f3b01913fc02d3a8bf85398128c9 and ab9b18bb5c257d8a2aeca19ffc1b6363f63ee81f.
    Calls bisect skip on 25060c3b0683c710cd5b47cdfc550b7f8a3b4940 to avoid bisecting that commit.
    Asks user if the cleans can be run before building. 

#>

[CmdletBinding(DefaultParameterSetName = 'BisectStart')]
param (
    [Parameter(Mandatory,
        ParameterSetName = 'BisectStart')]
    [string]$Bad,
    [Parameter(Mandatory,
        ParameterSetName = 'BisectStart')]
    [string]$Good,
    [Parameter(Mandatory = $false,
        ParameterSetName = 'BisectStart')]
    [string[]]$Skip,
    [switch]$Force,
    [switch]$FirstParent
)

Copy-Item  -Force .\eng\Bisect-Test.ps1 $env:TEMP
$tester = Join-Path -Path $env:TEMP -ChildPath Bisect-Test.ps1
$tester = $tester + ' -Build -Run'

if ($Force) {
    $tester = $tester + ' -Force'
}

$fp = ""
if ($FirstParent) {
    $fp = '--first-parent'
}
git bisect start $Bad $Good $fp 2>&1 | Tee-Object Bisect.log

foreach ($s in $Skip) { 
    git bisect skip $s
}


git bisect run powershell -Command $tester 2>&1 | Tee-Object -Append Bisect.log
git bisect log 2>&1 | Tee-Object -Append Bisect.log
