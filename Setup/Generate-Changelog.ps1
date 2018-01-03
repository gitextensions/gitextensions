[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=1)]
    [int] $milestoneNumber
)

pushd $PSScriptRoot

$baseUri = "https://api.github.com/repos/gitextensions/gitextensions"

try{
    $milestone = Invoke-RestMethod -Method Get -Uri $baseUri/milestones/$milestoneNumber
    $milestoneTitle = $milestone.title;
    $milestoneDue = if ($milestone.due_on -eq $null) { "no due date" } else { Get-Date $milestone.due_on -Format "dd MMM yyyy" };
    $changelogFile = "Changelog.v$milestoneTitle.md";
    $totalIssueCount = $milestone.closed_issues

    $totalIssues = @();
    $pageIndex = 1;
    while ($totalIssueCount -gt 0) {
        $issues = Invoke-RestMethod -Method Get -Uri "$baseUri/issues?page=$pageIndex&per_page=100&milestone=$($milestone.number)&state=closed" #-Verbose
        $totalIssues += $issues;

        $totalIssueCount -= $issues.Count;
        $pageIndex++;
    }

    $issueFeatures = @();
    $issueBugs = @();
    $issueLinks = @();

    $totalIssues | ForEach-Object {
        $issue = $_;

        if ($issue.labels.name -eq 'bug') {
            $issueBugs += $issue;
        }
        else {
            $issueFeatures += $issue;
        }
        $issueLinks += "[$($issue.number)]:$($issue.html_url)"
    }


    "### Version $milestoneTitle ($milestoneDue)" | Out-File $changelogFile
    "`r`n#### Features:" | Out-File $changelogFile -Append
    $issueFeatures | ForEach-Object {
        $issue = $_;
        "* $($issue.title) - PR [$($issue.number)]" | Out-File $changelogFile -Append
    }
    "`r`n#### Fixes:" | Out-File $changelogFile -Append
    $issueBugs | ForEach-Object {
        $issue = $_;
        "* $($issue.title) - Issue [$($issue.number)]" | Out-File $changelogFile -Append
    }
    "`r`n" | Out-File $changelogFile -Append
    $issueLinks | Out-File $changelogFile -Append
}
catch {
    Write-Error $_;
    Pause
    Exit -1;
}

