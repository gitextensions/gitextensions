﻿# Example:
#    .\Prepare-Release.ps1 -oldVersion 2.51.01 -newVersion 2.51.02 -milestones 36,37
#

[CmdletBinding()]
Param(
    #[Parameter(Mandatory=$True, Position=3)]
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

    $issues = @();
    $issueLinks = @();

    $totalIssues | Sort-Object number -Descending | ForEach-Object {
        $issue = $_;
        $issues += $issue;
        $issueLinks += "[$($issue.number)]:$($issue.html_url)"
    }

    "### Version $milestoneTitle ($milestoneDue)" | Out-File $changelogFile -Encoding utf8
    "`r`n#### Changes:" | Out-File $changelogFile -Append -Encoding utf8
    $issues | ForEach-Object {
        $issue = $_;
        "* [#$($issue.number)] $($issue.title)" | Out-File $changelogFile -Append -Encoding utf8
    }
    "`r`n" | Out-File $changelogFile -Append -Encoding utf8
    $issueLinks | Out-File $changelogFile -Append -Encoding utf8
}

function Update-Contributors {
    Param(
        [string] $originalVersion
    )

    # if running in PS ISE, accessing Console object's properties will likely fail
    # for more details see https://social.technet.microsoft.com/Forums/en-US/b92b15c8-6854-4d3e-8a35-51b4b56276ba/powershell-ise-vs-consoleoutputencoding?forum=ITCG
    ping | Out-Null
    [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8

    $file = '../GitUI/Properties/Resources.resx';
    [xml]$content = Get-Content $file -Encoding UTF8;
    $rawTeam = ($content.root.data | Where name -eq "Team").Value;
    $rawContributors = ($content.root.data | Where name -eq "Coders").Value;
    $allContributors = "$rawTeam, $rawContributors";

    $cmd = "git shortlog 700a3000a8672205a6f523e6d4dc70f0f66314d9..HEAD -s --no-merges";
    Write-Host "Getting contributors by running: $cmd";
    $result = iex $cmd

    $count = 0;
    $result | ForEach-Object {
        # extract each contributor and add it to the global list, if absent
        if ($_ -match "\s*\d{1,}\s*(?<name>.*)" -and
            -not $allContributors.Contains($Matches['name'])) {
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
    Write-Host Update contributors
    Write-Host ----------------------------------------------------------------------
    Update-Contributors -originalVersion $oldVersion

    Write-Host ----------------------------------------------------------------------
    Write-Host Generate the changelog
    Write-Host ----------------------------------------------------------------------
    Generate-Changelog -milestones $milestoneNumbers

}
catch {
    Write-Error $_;
    Exit -1;
}
