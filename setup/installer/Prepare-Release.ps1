# Example:
#    .\Prepare-Release.ps1 -milestones 36,37
#    .\Prepare-Release.ps1 -milestones 36,37 -pat ghp_YourPersonalAccessTokenHere
#

[CmdletBinding()]
Param(
    #[Parameter(Mandatory=$True, Position=3)]
    [string] $milestones,
    [string] $pat
)

$repoRoot = Resolve-Path  "../../"


function Invoke-GitHubApi {
    Param(
        [System.Net.Http.HttpClient] $client,
        [string] $uri
    )

    $response = $client.GetAsync($uri).Result
    if (-not $response.IsSuccessStatusCode) {
        $body = $response.Content.ReadAsStringAsync().Result
        throw "GitHub API request to '$uri' failed with $([int]$response.StatusCode) $($response.ReasonPhrase): $body"
    }

    return $response.Content.ReadAsStringAsync().Result | ConvertFrom-Json
}

function Generate-Changelog {
    [CmdletBinding()]
    Param(
        [int[]] $milestones,
        [string] $pat
    )

    $baseUri = "https://api.github.com/repos/gitextensions/gitextensions"

    # Use HttpClient with a per-request DefaultRequestHeaders so that the
    # Authorization header is set at the protocol level and cannot be dropped.
    # WebClient treats Authorization as a restricted header and silently strips
    # it; Invoke-RestMethod on PS 5.1 has the same limitation via HttpWebRequest.
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Add-Type -AssemblyName System.Net.Http
    $httpClient = New-Object System.Net.Http.HttpClient
    $httpClient.DefaultRequestHeaders.Add('Accept', 'application/vnd.github+json')
    $httpClient.DefaultRequestHeaders.Add('X-GitHub-Api-Version', '2022-11-28')
    $httpClient.DefaultRequestHeaders.Add('User-Agent', 'GitExtensions-PrepareRelease')
    if ($pat) {
        $httpClient.DefaultRequestHeaders.Authorization =
            New-Object System.Net.Http.Headers.AuthenticationHeaderValue('token', $pat)
    }

    $changelogFile = "$repoRoot/src/app/GitUI/Resources/ChangeLog.md";
    $totalIssues = @();

    $milestones | ForEach-Object {
        $milestoneNumber = $_;

        Write-Host "Preparing changelog for milestone: $milestoneNumber";

        $milestone = Invoke-GitHubApi $httpClient "$baseUri/milestones/$milestoneNumber"
        $milestoneTitle = $milestone.title;
        $milestoneDue = if ($milestone.due_on -eq $null) { "no due date" } else { Get-Date $milestone.due_on -Format "dd MMM yyyy" };
        $totalIssueCount = $milestone.closed_issues

        $pageIndex = 1;
        while ($totalIssueCount -gt 0) {
            $issues = Invoke-GitHubApi $httpClient "$baseUri/issues?page=$pageIndex&per_page=100&milestone=$($milestone.number)&state=closed"
            $totalIssues += $issues;

            $totalIssueCount -= $issues.Count;
            $pageIndex++;
        }
    }

    $issues = @();
    $issueLinks = @();

    $totalIssues | Sort-Object number -Descending | ForEach-Object {
        $issue = $_;
        if (!$issue.html_url.Contains('/pull/')) {
            return;
        }

        $issues += $issue;
        $issueLinks += "[#$($issue.number)]:$($issue.html_url)"
    }

    $oldChangelog = Get-Content -Encoding utf8 $changelogFile
    $oldChangelog | Select-Object -First 3 | Out-File $changelogFile -Encoding utf8

    "### Version $milestoneTitle ($milestoneDue)" | Out-File $changelogFile -Append -Encoding utf8
    "`r`n#### Changes:" | Out-File $changelogFile -Append -Encoding utf8
    $issues | ForEach-Object {
        $issue = $_;
        "* [#$($issue.number)] $($issue.title)" | Out-File $changelogFile -Append -Encoding utf8
    }
    "`r`n" | Out-File $changelogFile -Append -Encoding utf8
    $issueLinks | Out-File $changelogFile -Append -Encoding utf8
    "`r`n" | Out-File $changelogFile -Append -Encoding utf8

    $oldChangelog | Select-Object -Skip 3 | Out-File $changelogFile -Append -Encoding utf8
}

function Update-Contributors {
    Param(
        [string] $originalVersion
    )

    # if running in PS ISE, accessing Console object's properties will likely fail
    # for more details see https://social.technet.microsoft.com/Forums/en-US/b92b15c8-6854-4d3e-8a35-51b4b56276ba/powershell-ise-vs-consoleoutputencoding?forum=ITCG
    ping | Out-Null
    [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8

    $file = "$repoRoot/src/app/GitUI/Properties/Resources.resx";
    [xml]$content = Get-Content $file -Encoding UTF8;
    $rawTeam = ($content.root.data | Where name -eq "Team").Value;
    $rawContributors = ($content.root.data | Where name -eq "Coders").Value;
    $rawIgnoreContributors = ($content.root.data | Where name -eq "IgnoreContributors").Value;
    $allContributors = "$rawTeam, $rawContributors";

    $cmd = "git shortlog 0a5ef9ca6ce87754a510aa02c7a96d8b915714ac..HEAD -s --no-merges";
    Write-Host "Getting contributors by running: $cmd";
    $result = iex $cmd

    $count = 0;
    $result | ForEach-Object {
        # extract each contributor and add it to the global list, if absent
        if ($_ -match "\s*\d{1,}\s*(?<name>.*)" -and
            -not $allContributors.Contains($Matches['name']) -and
            -not $rawIgnoreContributors.Contains($Matches['name'])) {
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

Push-Location $PSScriptRoot

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
    Generate-Changelog -milestones $milestoneNumbers -pat $pat

}
catch {
    Write-Error $_;
    Exit -1;
}
finally {
    Pop-Location
}
