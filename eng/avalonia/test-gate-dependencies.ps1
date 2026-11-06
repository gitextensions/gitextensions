#Requires -Version 7.0
# parity-scaffolding: Checks the product dependency boundary without launching any build or desktop process.
[CmdletBinding()]
param([string]$Gate = (Join-Path $PSScriptRoot 'gate.ps1'))

$ErrorActionPreference = 'Stop'
$ast = [Management.Automation.Language.Parser]::ParseFile($Gate, [ref]$null, [ref]$null)
$assignment = $ast.Find({ param($node)
    $node -is [Management.Automation.Language.AssignmentStatementAst] -and
        $node.Left -is [Management.Automation.Language.VariableExpressionAst] -and
        $node.Left.VariablePath.UserPath -eq 'cannotAffectProduct'
}, $true)
if (-not $assignment) { throw 'The production dependency boundary was not found.' }
$pattern = Invoke-Expression $assignment.Right.Extent.Text
foreach ($path in @(
    'eng/avalonia/framework-adaptations.json', 'eng/avalonia/portmap.json',
    'eng/avalonia/parity-ledger.json', 'eng/avalonia/gate.ps1',
    'eng/tools/ParityInventory/MemberMatcher.cs', 'eng/tools/ParityInventory.Tests/OverloadInventoryTests.cs',
    'eng/tools/ParityFix/MechanicalFixer.cs', 'eng/tools/ParityFix.Tests/MechanicalFixerTests.cs')) {
    if ($path -notmatch $pattern) { throw "Inventory/tool-only path invalidates product legs: $path" }
}
foreach ($path in @(
    'src/app/GitUI.Avalonia/CommandsDialogs/FormBrowse.axaml.cs',
    'src/app/ResourceManager/TranslatedControl.cs',
    'tests/app/UnitTests/GitUI.Avalonia.Tests/TerminalLauncherTests.cs',
    'eng/avalonia/TranslationParityKnownConflicts.json', 'eng/avalonia/unknown.json',
    'eng/tools/UnknownTool/Program.cs')) {
    if ($path -match $pattern) { throw "Product or unknown path incorrectly reuses product legs: $path" }
}
'PASS: inventory-only adaptations and standalone tooling reuse product legs; product/test sources and unknown inputs invalidate them.'
