param(
    [Parameter(Mandatory = $true)]
    [string] $PublishDirectory,

    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$publishRoot = [System.IO.Path]::GetFullPath($PublishDirectory)
$pluginSource = Join-Path $repoRoot "artifacts/$Configuration/bin/GitExtensions.Avalonia/net10.0/Plugins"
$pluginDestination = Join-Path $publishRoot "Plugins"
$expectedPlugins = @(
    "AppVeyorIntegration",
    "AutoCompileSubmodules",
    "AzureDevOpsIntegration",
    "BackgroundFetch",
    "CreateLocalBranches",
    "DeleteUnusedBranches",
    "FindLargeFiles",
    "GitHub3",
    "GitHubActionsIntegration",
    "GitImpact",
    "GitlabIntegration",
    "GitStatistics",
    "Gource",
    "JenkinsIntegration",
    "ProxySwitcher",
    "ReleaseNotesGenerator",
    "TeamCityIntegration"
)

if (-not (Test-Path -LiteralPath $publishRoot -PathType Container))
{
    throw "Publish directory does not exist: $publishRoot"
}

if (-not (Test-Path -LiteralPath $pluginSource -PathType Container))
{
    throw "Portable plugin output does not exist: $pluginSource"
}

New-Item -ItemType Directory -Path $pluginDestination -Force | Out-Null

foreach ($plugin in $expectedPlugins)
{
    $sourceDirectory = Join-Path $pluginSource $plugin
    $assembly = Join-Path $sourceDirectory "GitExtensions.Plugins.$plugin.dll"
    if (-not (Test-Path -LiteralPath $assembly -PathType Leaf))
    {
        throw "Portable plugin output is incomplete: $assembly"
    }

    $destinationDirectory = Join-Path $pluginDestination $plugin
    New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null

    foreach ($file in Get-ChildItem -LiteralPath $sourceDirectory -File)
    {
        if ($file.Name.StartsWith("GitExtensions.Plugins.$plugin.", [System.StringComparison]::Ordinal))
        {
            Copy-Item -LiteralPath $file.FullName -Destination $destinationDirectory -Force
            continue
        }

        $sharedDestination = Join-Path $publishRoot $file.Name
        if (-not (Test-Path -LiteralPath $sharedDestination -PathType Leaf))
        {
            Copy-Item -LiteralPath $file.FullName -Destination $sharedDestination
        }
    }
}

$stagedPluginCount = @(
    Get-ChildItem -LiteralPath $pluginDestination -Recurse -Filter "GitExtensions.Plugins.*.dll" |
        Where-Object { $_.Directory.Parent.FullName -eq $pluginDestination }
).Count

if ($stagedPluginCount -ne $expectedPlugins.Count)
{
    throw "Expected $($expectedPlugins.Count) staged plugins, found $stagedPluginCount."
}

Write-Host "Staged $stagedPluginCount portable plugins in $pluginDestination"
