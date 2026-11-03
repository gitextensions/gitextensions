#Requires -Version 7.0
# parity-scaffolding: shows parity progress by area and finding class until the parity gate closes.

<#
.SYNOPSIS
    Summarizes a reviewed inventory and the proof ledger as Markdown, and records the trend.

.DESCRIPTION
    Mechanical findings are comments and member order. Missing-behavior findings are original
    members, handlers, event wiring, settings, translation keys, hotkeys or menu items absent from
    the twin; each still needs review because some are intentional. Structural findings are the
    rest. Findings a missing method already explains are counted separately as dependent.

.EXAMPLE
    pwsh eng/avalonia/parity-dashboard.ps1
#>
[CmdletBinding()]
param(
    [string]$Inventory,
    [string]$Ownership,
    [string]$Output,

    [string]$History,

    [string]$LedgerPath,

    # Do not append this measurement to the trend history.
    [switch]$NoHistory
)

$ErrorActionPreference = 'Stop'

$repo = (git -C $PSScriptRoot rev-parse --show-toplevel).Trim().Replace('/', '\')
$evidence = Join-Path $repo 'eng\avalonia\parity-evidence'
if (-not $Inventory) { $Inventory = Join-Path $evidence 'gates\latest-inventory\current-inventory.json' }
if (-not $Ownership) { $Ownership = Join-Path $evidence 'P8.6i\application-shell-126\closure-ownership.json' }
if (-not $Output) { $Output = Join-Path $evidence 'dashboard.md' }
$historyPath = if ($History) { $History } else { Join-Path $evidence 'dashboard-history.jsonl' }
if (-not $LedgerPath) { $LedgerPath = Join-Path $repo 'eng/avalonia/parity-ledger.json' }

$mechanicalCodes = 'comment.missing', 'comment.changed', 'comment.drifted', 'member.order'
$behaviorCodes = 'member.missing', 'event.wiring.missing', 'event.handler.missing', 'setting.missing', 'setting.extra',
    'translation.key.missing', 'translation.key.extra', 'translation.not-in-english', 'translation.string.missing',
    'hotkey.command.missing', 'hotkey.command.extra', 'menu.item.missing', 'menu.item.extra', 'partial.missing'
$areaNames = [ordered]@{
    progress126 = '126 Browse and shell'
    progress127 = '127 Commit, status, editor'
    progress128 = '128 Core workflows'
    progress129 = '129 Settings and themes'
    progress130 = '130 Hosting and platform'
    progress131 = '131 Plugins'
    progress132 = '132 Shared core and compat'
    unowned = 'Unowned'
}

$owners = @{}
if (Test-Path -LiteralPath $Ownership) {
    foreach ($type in (Get-Content -LiteralPath $Ownership -Raw | ConvertFrom-Json).types) { $owners[$type.typeName] = $type.owner }
}

$current = Get-Content -LiteralPath $Inventory -Raw | ConvertFrom-Json
$counts = @{}
$dependent = 0
$acceptedComments = @{ codeAbsent = 0; codeMoved = 0; adaptedMinimal = 0 }
foreach ($file in Get-ChildItem -LiteralPath (Join-Path (Split-Path -Parent $Inventory) 'types') -Filter '*.json') {
    $report = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
    $owner = if ($owners.ContainsKey($report.typeName)) { $owners[$report.typeName] } else { 'unowned' }
    if (-not $areaNames.Contains($owner)) { $owner = 'unowned' }
    if (-not $counts.ContainsKey($owner)) { $counts[$owner] = @{ Mechanical = 0; Behavior = 0; Structural = 0 } }
    $dependent += @($report.dependentFindings).Count
    foreach ($deviation in $report.acceptedFrameworkDeviations) {
        if ($deviation.category -eq 'comments' -and $deviation.reason -in 'codeAbsent', 'codeMoved', 'adaptedMinimal') {
            $acceptedComments[$deviation.reason]++
        }
    }
    foreach ($finding in $report.findings) {
        $class = if ($finding.code -in $mechanicalCodes) { 'Mechanical' } elseif ($finding.code -in $behaviorCodes) { 'Behavior' } else { 'Structural' }
        $counts[$owner][$class]++
    }
}

$totals = @{ Mechanical = 0; Behavior = 0; Structural = 0 }
$areaLines = foreach ($owner in $areaNames.Keys) {
    if (-not $counts.ContainsKey($owner)) { continue }
    $row = $counts[$owner]
    foreach ($key in 'Mechanical', 'Behavior', 'Structural') { $totals[$key] += $row[$key] }
    '| {0} | {1} | {2} | {3} | {4} |' -f $areaNames[$owner], $row.Mechanical, $row.Behavior, $row.Structural, ($row.Mechanical + $row.Behavior + $row.Structural)
}
$actionable = $totals.Mechanical + $totals.Behavior + $totals.Structural
if ($actionable -ne $current.summary.findingCount) { throw "type reports hold $actionable findings but the summary says $($current.summary.findingCount)." }

$ledger = Get-Content -LiteralPath $LedgerPath -Raw | ConvertFrom-Json
$statusNames = 'verified', 'partial', 'gap', 'unverified', 'notApplicable'
$axisCounts = [ordered]@{}
foreach ($axis in $ledger.axisOrder) { $axisCounts[$axis] = @{} }
$platforms = @{}
$allButMacOs = 0
$requiredPlatforms = 'windows', 'linux-wayland', 'linux-x11', 'flatpak'
foreach ($component in $ledger.components) {
    foreach ($axis in $ledger.axisOrder) {
        $state = $component.axes.$axis.status
        $axisCounts[$axis][$state] = 1 + [int]$axisCounts[$axis][$state]
    }
    foreach ($platform in $component.axes.platform.platforms) { $platforms[$platform] = 1 + [int]$platforms[$platform] }
    # Everything except the macOS evidence no local machine can produce.
    $otherAxes = @($ledger.axisOrder | Where-Object { $_ -ne 'platform' })
    $axesProven = @($otherAxes | Where-Object { $component.axes.$_.status -in 'verified', 'notApplicable' }).Count -eq $otherAxes.Count
    $platformsProven = @($requiredPlatforms | Where-Object { $_ -in $component.axes.platform.platforms }).Count -eq $requiredPlatforms.Count
    if ($axesProven -and $platformsProven) { $allButMacOs++ }
}
$complete = @($ledger.components | Where-Object complete).Count

$measurement = [ordered]@{
    measuredUtc = (Get-Date).ToUniversalTime().ToString('o')
    commit = $current.analyzedCommit
    actionable = $actionable
    mechanical = $totals.Mechanical
    behavior = $totals.Behavior
    structural = $totals.Structural
    dependent = $dependent
    acceptedComments = $acceptedComments.codeAbsent + $acceptedComments.codeMoved + $acceptedComments.adaptedMinimal
    acceptedAdaptedMinimal = $acceptedComments.adaptedMinimal
    complete = $complete
    allButMacOs = $allButMacOs
}

$previous = $null
$lastMeasurement = $null
if (Test-Path -LiteralPath $historyPath) {
    $historyRows = @(Get-Content -LiteralPath $historyPath | Where-Object { $_.Trim() } | ForEach-Object { $_ | ConvertFrom-Json })
    if ($historyRows.Count) { $lastMeasurement = $historyRows[-1] }
    for ($index = $historyRows.Count - 1; $index -ge 0; $index--) {
        if ($historyRows[$index].commit -ne $measurement.commit) {
            $previous = $historyRows[$index]
            break
        }
    }
}
$change = if ($previous) { ' (change since {0}: {1:+0;-0;0})' -f $previous.commit.Substring(0, 9), ($actionable - $previous.actionable) } else { '' }

$lines = [Collections.Generic.List[string]]::new()
$lines.Add('# Parity dashboard')
$lines.Add('')
$lines.Add("Measured $($measurement.measuredUtc) at $($current.analyzedCommit), $($current.summary.mappingCount) mappings and $($current.summary.analyzedTypeCount) types.")
$lines.Add('')
$lines.Add("**Actionable source findings: $actionable**$change, plus $dependent dependent.")
$lines.Add("**Accepted handwritten comment findings: $($measurement.acceptedComments)** (codeAbsent $($acceptedComments.codeAbsent), codeMoved $($acceptedComments.codeMoved), adaptedMinimal $($acceptedComments.adaptedMinimal)); accepted deviations, not closed.")
$lines.Add('Source findings are one measure only; capture and state coverage are tracked separately.')
$lines.Add('')
$lines.Add('| Area | Mechanical | Missing behavior | Structural | Actionable |')
$lines.Add('| --- | ---: | ---: | ---: | ---: |')
$areaLines | ForEach-Object { $lines.Add($_) }
$lines.Add("| **Total** | **$($totals.Mechanical)** | **$($totals.Behavior)** | **$($totals.Structural)** | **$actionable** |")
$lines.Add('')
$lines.Add("**Components complete: $complete of $(@($ledger.components).Count).** Complete except macOS: $allButMacOs.")
$lines.Add('')
$lines.Add('| Axis | ' + ($statusNames -join ' | ') + ' |')
$lines.Add('| --- |' + (' ---: |' * $statusNames.Count))
foreach ($axis in $axisCounts.Keys) {
    $lines.Add("| $axis | " + (($statusNames | ForEach-Object { [int]$axisCounts[$axis][$_] }) -join ' | ') + ' |')
}
$lines.Add('')
$lines.Add('Components with platform evidence: ' + (($platforms.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { "$($_.Key) $($_.Value)" }) -join ', ') + ', macos 0.')

Set-Content -LiteralPath $Output -Value $lines -Encoding utf8
$measurementChanged = -not $lastMeasurement
if ($lastMeasurement) {
    foreach ($property in 'commit', 'actionable', 'mechanical', 'behavior', 'structural', 'dependent', 'acceptedComments', 'acceptedAdaptedMinimal', 'complete', 'allButMacOs') {
        if ($lastMeasurement.$property -ne $measurement[$property]) {
            $measurementChanged = $true
            break
        }
    }
}
if (-not $NoHistory -and $measurementChanged) {
    Add-Content -LiteralPath $historyPath -Value ([pscustomobject]$measurement | ConvertTo-Json -Compress) -Encoding utf8
}
$lines
