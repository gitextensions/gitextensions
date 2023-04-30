<#
.SYNOPSIS
    Runs build output and asks user if app behaved as expected.
.DESCRIPTION
    This script is the helper script that runs in the bisect run call from the Run-BisectTest script.
    You can run this script with a redirect to a file to validate functionality of current commit and note the problem if a problem is found. Then use the file in an issue report.
.PARAMETER Force
    Skip asking the user if they want to perform the git clean on their repo. 
.PARAMETER Run
    Run the test and if build output does not exist build and then test.
.PARAMETER Build
    Force a build even if build output exists.  Uses the right build functionality based on if the build script exists or not.
.OUTPUTS
    Only used when this script is being used from another script or program like Run-BisectTest
    125 - Build failure or user answered Unsure.  Causes git bisect run to run git bisect skip on current commit.
      1 - User answered No. Causes git bisect run to run git bisect bad on current commit.
      0 - User answered Yes. Causes git bisect run to run git bisect good on current commit.
     -1 - User aborted by saying no to the clean question.  To prevent testing weird state that is not reproducable cleans are required.  Make sure your code is commited.

.EXAMPLE
    Bisect-Test.ps1 -Build
    Just builds.
    Asks user if it should run the cleans before building.
.EXAMPLE 
    Bisect-Test.ps1 -Build -Run
    Builds output and then runs build output.  After user closes program, script will ask if it behaved.  If Unsure or No responses, then user is asked to describe the failure.  
    Asks user if it should run the cleans before building.
.EXAMPLE
    Bisect-Test.ps1 -Build -Run -Force
    Builds output and then runs build output.  After user closes program, script will ask if it behaved.  If Unsure or No responses, then user is asked to describe the failure.  
    Skips asking user if it should run the cleans before building.
.EXAMPLE
    Bisect-Test.ps1 -Run
    Runs output and if build output does not exist then runs build first.  After user closes program, script will ask if it behaved.  If Unsure or No responses, then user is asked to describe the failure.  
    Asks user if it should run the cleans before building if a build is run.
.EXAMPLE
    Bisect-Test.ps1 -Run -Force
    Runs output and if build output does not exist then runs build first.  After user closes program, script will ask if it behaved.  If Unsure or No responses, then user is asked to describe the failure.  
    Skips asking user if it should run the cleans before building if a build is run.

#>


Param([switch] $Force,
    [switch] $Run,
    [switch] $Build)


function Build() {
    Write-Verbose "Build ran?  $buildRan"
    if ($buildRan -eq $false) {
        Set-Variable -Scope Script -name "buildRan" -Value $true
        $buildScript = Join-Path -Path $scriptsPath -ChildPath build.ps1
        $result = 0

        if (-not $Force) {
            $result = $Host.UI.PromptForChoice('Running git clean -xdf', 'Are you sure you want to proceed? This will delete all files that are new and not committed or staged.', @('&Yes'; '&No'), 1)
        }
        if ($result -eq 0) {

            git submodule update --init --recursive
            git submodule foreach --recursive git clean -x -d -f
            git clean -x -d -f --exclude="scripts/*Bisect*.ps1" --exclude="Bisect.log"

            if (Test-Path -Path $buildScript) {
                & $buildScript -rebuild -bn -b -restore
            }
            else {
                $native = Join-Path -Path $scriptsPath -ChildPath native.proj
                $native = Resolve-Path -Relative -Path $native
                Write-Verbose $native
                if (Test-Path -Path $native) {
                    dotnet build $native
                }
                dotnet build --no-incremental
            }
        }
        else {
            Write-Output "User aborted"
            exit -1
        }
    }
}

function Test() {
    try {
        $artifacts = Join-Path -Path $scriptsPath -ChildPath ..\artifacts
        Write-Verbose $artifacts
        $ge = Get-ChildItem -Path $artifacts -Name gitextensions.exe -Recurse | Select-Object -First 1
        $geFull = Join-Path -Path $artifacts -ChildPath $ge
        $geFull = Resolve-Path -Path $geFull
        if ( (Get-Item $geFull) -is [System.IO.DirectoryInfo]) {

            if ($buildRan) {
                Write-Output "Untestable Commit.  Cannot find build output"
                exit 125; # See https://git-scm.com/docs/git-bisect/#_bisect_run
            }
            else {
                Build
                Run
            }
        }
        else {
            Write-Output "Starting $geFull"
            [console]::beep(2000, 500)
            Start-Process -Wait -FilePath $geFull -ArgumentList browse
        }
    }
   
    catch {
        if ($buildRan) {
            Write-Output "Untestable Commit"
            Write-Output $_
            exit 125; # See https://git-scm.com/docs/git-bisect/#_bisect_run
        }
        else {
            Build
            Run
        }
    }
              
    $testResult = $Host.UI.PromptForChoice('Did it perform as expected?', 'Did Git Extensions behave as expected?', @('&Yes'; '&No', '&Unsure'), 0)
    Write-Output "Current Commit: $(git rev-parse HEAD)"
    if ($testResult -eq 0) {
        Write-Output "Good Commit"
        exit 0
    }
    elseif ($testResult -eq 1) {
        Write-Output "Bad Commit"
        $describe = Read-Host -Prompt "Describe failure"
        Write-Output $describe
        exit 1 
    }
    elseif ($testResult -eq 2) {
        Write-Output "Untestable Commit"
        $describe = Read-Host -Prompt "Describe why you can't test"
        Write-Output $describe
        exit 125; # See https://git-scm.com/docs/git-bisect/#_bisect_run
    }
}

function Run {
  
    $scriptsPath = Get-Location
    $scriptsPath = Join-Path $scriptsPath -ChildPath scripts
    Write-Verbose "scripts: $scriptsPath"
    if ($Build) {
           
        try {
            Build  
        }
        catch {
            Write-Output "Error  $LASTEXITCODE"
            Write-Output $_
            exit 125; # See https://git-scm.com/docs/git-bisect/#_bisect_run
        }       
    }  

    if ($Run) {
        try {
            Test
        }
        catch {
            Write-Output "Error  $LASTEXITCODE"
            Write-Output $_
            exit 125; # See https://git-scm.com/docs/git-bisect/#_bisect_run
        }
    }
     
}
$buildRan = $false
Run