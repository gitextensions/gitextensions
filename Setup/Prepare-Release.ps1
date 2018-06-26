# Example:
#    .\Prepare-Release.ps1 -oldVersion 2.51.01 -newVersion 2.51.02 -milestones 36,37
#

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=1)]
    [string] $oldVersion,
    [Parameter(Mandatory=$True, Position=2)]
    [string] $newVersion,
    [Parameter(Mandatory=$True, Position=3)]
    [string] $milestones
)


function Generate-Changelog {
	[CmdletBinding()]
	Param(
		[int[]] $milestones
	)

	$baseUri = "https://api.github.com/repos/gitextensions/gitextensions"

	$changelogFile = "Changelog.md";
	$totalIssues = @();

	$milestones | ForEach-Object {
        $milestoneNumber = $_;

        Write-Host "Preparing changelog for milestone: $milestoneNumber";

		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
		$milestone = Invoke-RestMethod -Method Get -Uri $baseUri/milestones/$milestoneNumber
		$milestoneTitle = $milestone.title;
		$milestoneDue = if ($milestone.due_on -eq $null) { "no due date" } else { Get-Date $milestone.due_on -Format "dd MMM yyyy" };
		$totalIssueCount = $milestone.closed_issues

		$pageIndex = 1;
		while ($totalIssueCount -gt 0) {
			$issues = Invoke-RestMethod -Method Get -Uri "$baseUri/issues?page=$pageIndex&per_page=100&milestone=$($milestone.number)&state=closed" #-Verbose
			$totalIssues += $issues;

			$totalIssueCount -= $issues.Count;
			$pageIndex++;
		}
	}

	$pullrequests = @();
	$issues = @();
	$issueLinks = @();

	$totalIssues | Sort-Object number -Descending | ForEach-Object {
		$issue = $_;

		if ($issue.pull_request -ne $null) {
			$issue | Add-Member customTitle "PR";
		}
		else {
			$issue | Add-Member customTitle "Issue";
		}

		if ($issue.labels.name -eq 'bug') {
			$issues += $issue;
		}
		else {
			$pullrequests += $issue;
		}
		$issueLinks += "[$($issue.number)]:$($issue.html_url)"
	}

	"### Version $milestoneTitle ($milestoneDue)" | Out-File $changelogFile -Encoding utf8
	"`r`n#### Features:" | Out-File $changelogFile -Append -Encoding utf8
	$pullrequests | ForEach-Object {
		$issue = $_;
		"* $($issue.title) - $($issue.customTitle) [$($issue.number)]" | Out-File $changelogFile -Append -Encoding utf8
	}
	"`r`n#### Fixes:" | Out-File $changelogFile -Append -Encoding utf8
	$issues | ForEach-Object {
		$issue = $_;
		"* $($issue.title) - $($issue.customTitle) [$($issue.number)]" | Out-File $changelogFile -Append -Encoding utf8
	}
	"`r`n" | Out-File $changelogFile -Append -Encoding utf8
	$issueLinks | Out-File $changelogFile -Append -Encoding utf8
}

function Update-Contributors {
    Param(
        [string] $originalVersion
    )

    $file = '../GitUI/Properties/Resources.resx';
    [xml]$content = Get-Content $file -Encoding UTF8;
    $rawContributors = ($content.root.data | Where name -eq "Coders").Value;

    # \r=13, \n=10, comma=44
    $contributors = $rawContributors.Split(13, 10, 44).Trim() | Where { $_ } 

    [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8
    $cmd = "git shortlog v$originalVersion..HEAD -s --no-merges";
    Write-Host "Getting contributors by running: $cmd";
    $result = iex $cmd

    $count = 0;
    $result | ForEach-Object {
        # extract each contributor and add it to the global list, if absent
        if ($_ -match "\s*\d{1,}\s*(?<name>.*)" -and
            -not $contributors.Contains($Matches['name'])) {
            $rawContributors += ', ' + $Matches['name']
            $count++;
        }
    }

    if ($count -gt 0) {
        Write-Host "`t$count new contributors added";
        # update contributors in resx file and save
        ($content.root.data | Where name -eq "Coders").Value = $rawContributors;
        $loc = Resolve-Path $file;
        $content.Save($loc.Path);
    }
    else {
        Write-Host "`tNo new contributors";
    }
}

function Update-SourceFiles {
    Param(
        [string] $originalVersion,
        [string] $futureVersion
    )

    # Update files replacing old version with the new version
    #-----------------------------------------------------------------------------------------------
    $files = @( 
                @{ File = "MakeInstallers.cmd"; Separator = '.'; Encoding = 'ascii'; Patterns = @( 'set version=#VERSION#', 'set numericVersion=#VERSION#' ) }
               ,@{ File = "MakeMonoArchive.cmd"; Separator = '.'; Encoding = 'ascii'; Patterns = @( 'set version=#VERSION#' ) }
               ,@{ File = "..\CommonAssemblyInfo.cs"; Separator = '.'; Encoding = 'utf8'; Patterns = @( 'assembly: AssemblyVersion("#VERSION#")', '[assembly: AssemblyFileVersion("#VERSION#")]', '[assembly: AssemblyInformationalVersion("#VERSION#")]' ) }
               ,@{ File = "..\GitExtensionsShellEx\GitExtensionsShellEx.rc"; Separator = ','; Encoding = 'ascii'; Patterns = @( 'FILEVERSION #VERSION#', 'PRODUCTVERSION #VERSION#' ) }
               ,@{ File = "..\GitExtensionsShellEx\GitExtensionsShellEx.rc"; Separator = '.'; Encoding = 'ascii'; Patterns = @( 'VALUE "FileVersion", "#VERSION#"', 'VALUE "ProductVersion", "#VERSION#"' ) }
               ,@{ File = "..\GitExtSshAskPass\SshAskPass.rc2"; Separator = ','; Encoding = 'ascii'; Patterns = @( 'FILEVERSION #VERSION#', 'PRODUCTVERSION #VERSION#' ) }
               ,@{ File = "..\GitExtSshAskPass\SshAskPass.rc2"; Separator = '.'; Encoding = 'ascii'; Patterns = @( 'VALUE "FileVersion", "#VERSION#"', 'VALUE "ProductVersion", "#VERSION#"' ) }
               ,@{ File = "..\GitExtensionsVSIX\source.extension.vsixmanifest"; Encoding = 'utf8'; Separator = '.'; Patterns = @( 'Version="#VERSION#"' ) }
               ,@{ File = "..\GitExtensionsDoc\source\conf.py"; Separator = '.'; Encoding = 'utf8'; Patterns = @( "release = '#VERSION#'" ) }
              );
    $files | ForEach-Object {
        $fileDefinition = $_;

        Write-Host "Processing $($fileDefinition.File)";
        $content = Get-Content $fileDefinition.File -Encoding $fileDefinition.Encoding;

        $fileDefinition.Patterns | ForEach-Object {
            $pattern = $_.Replace('#VERSION#', $originalVersion.Replace('.', $fileDefinition.Separator));
            $newValue = $_.Replace('#VERSION#', $futureVersion.Replace('.', $fileDefinition.Separator));
            Write-Host "`tLooking for '$pattern'"
            $content = $content.Replace($pattern, $newValue);
        }

        $content | Out-File $fileDefinition.File -Encoding $fileDefinition.Encoding;
    }

    # update GitExtensionsDoc
    $fileDef = "..\GitExtensionsDoc\source\conf.py";
    Write-Host "Processing $fileDef";
    $content = Get-Content $fileDef -Encoding utf8;

    $shortOldVersion = $originalVersion.Substring(0, $originalVersion.LastIndexOf('.'))
    $shortNewVersion = $futureVersion.Substring(0, $futureVersion.LastIndexOf('.'))

    $pattern = "version = '$shortOldVersion'";
    Write-Host "`tLooking for '$pattern'"
    $content = $content.Replace($pattern, "version = '$shortNewVersion'");
    $content | Out-File $fileDef -Encoding utf8;
}



pushd $PSScriptRoot

try {

    # TODO: verify versions are in X.Y.Z format

    # verify milestones
    $milestoneNumbers = $milestones.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries);
    if ($milestoneNumbers.Count -lt 1) {
        Write-Host "At least one milestone is required" -ForegroundColor Red;
        Exit -1;
    }

    Write-Host ----------------------------------------------------------------------
    Write-Host Update the version number
    Write-Host ----------------------------------------------------------------------
    Update-SourceFiles -originalVersion $oldVersion -futureVersion $newVersion;

    Write-Host ----------------------------------------------------------------------
    Write-Host Update contributors
    Write-Host ----------------------------------------------------------------------
    Update-Contributors -originalVersion $oldVersion

    Write-Host ----------------------------------------------------------------------
    Write-Host Generate the changelog
    Write-Host ----------------------------------------------------------------------
    Generate-Changelog -milestones $milestoneNumbers

    Write-Host ----------------------------------------------------------------------
    Write-Host Compile and package
    Write-Host ----------------------------------------------------------------------
    # preparing the build artifacts
    $env:SKIP_PAUSE=1
    .\BuildInstallers.VS2017.cmd

    Write-Host ----------------------------------------------------------------------
    Write-Host Package PDBs
    Write-Host ----------------------------------------------------------------------
    # collect pdbs
    xcopy ..\GitExtensions\bin\Release\*.pdb pdbs\ /Y
    xcopy ..\GitExtensions\bin\Release\Plugins\*.pdb pdbs\Plugins\ /Y

    pushd pdbs
    ..\..\packages\7-Zip.CommandLine.9.20.0\tools\7za.exe a -tzip ..\GitExtensions-$newVersion-pdb.zip .
    popd

    # cleanup
    Remove-Item -Force -Recurse pdbs
    Remove-Item -Force -Recurse GitExtensions

}
catch {
    Write-Error $_;
    Exit -1;
}
