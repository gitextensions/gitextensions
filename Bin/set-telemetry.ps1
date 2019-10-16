[CmdletBinding()]
Param(
    [string] $Enabled = ''
)

[bool]$telemetryEnabled = -not [string]::IsNullOrEmpty($Enabled);

if ($ExecutionContext.SessionState.LanguageMode -ne "FullLanguage")
{
    exit 0
}

[string]$userAppDataPath = Join-Path -Path $env:APPDATA -ChildPath 'GitExtensions\GitExtensions\GitExtensions.settings'
if (-not (Test-Path -Path $userAppDataPath)) {
    [string]$userAppDataFolder = Split-Path $userAppDataPath -Parent
    if (-not (Test-Path -Path $userAppDataFolder)) {
        New-Item -ItemType Directory -Path $userAppDataFolder | Out-Null
    }

    '<?xml version="1.0" encoding="utf-8"?><dictionary />' | Out-File $userAppDataPath -Encoding utf8
}

[xml]$doc = Get-Content $userAppDataPath
if (!$doc) {
    $doc = '<?xml version="1.0" encoding="utf-8"?>';
}

$node = $doc.SelectSingleNode("/dictionary/item/key/string[text()='TelemetryEnabled']")
if ($node -ne $null) {
    $node.ParentNode.ParentNode.value.string = "$telemetryEnabled";
    $doc.Save($userAppDataPath)
    exit 0
}

$topNode = $doc.SelectSingleNode("/dictionary");
if ($topNode -eq $null) {
    $topNode = $doc.CreateElement('dictionary');
    $_ = $doc.AppendChild($topNode);
}

$node = $doc.CreateElement('item');
$node.InnerXml = "<key><string>TelemetryEnabled</string></key><value><string>$telemetryEnabled</string></value>";
$_ = $topNode.AppendChild($node);
$doc.Save($userAppDataPath)
