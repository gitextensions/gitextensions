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
    [switch]$Force    
)
Copy-Item  -Force .\scripts\Bisect-Test.ps1 $env:TEMP
$tester = Join-Path -Path $env:TEMP -ChildPath Bisect-Test.ps1
$tester = $tester + ' -Build -Run'
if ($Force) {
    $tester = $tester + ' -Force'
}
git bisect start $Bad $Good 2>&1 | Tee-Object Bisect.log

foreach ($s in $Skip) { 
    git bisect skip $s
}


git bisect run powershell -Command $tester 2>&1 | Tee-Object -Append Bisect.log
git bisect log 2>&1 | Tee-Object -Append Bisect.log
