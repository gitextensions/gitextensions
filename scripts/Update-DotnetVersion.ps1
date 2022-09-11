param ( [string]$version = "6.0")

$encoding = [System.Text.UTF8Encoding]::new($false)

$path = Split-Path $MyInvocation.MyCommand.Path -Parent
$uri = "https://raw.githubusercontent.com/dotnet/core/main/release-notes/$version/releases.json"

# Get .NET version info
$releaseJson = Invoke-WebRequest -Uri $uri -ContentType "application/json" -Method Get -UseBasicParsing
$versionResult = $releaseJson.Content | Out-String | ConvertFrom-Json 
Write-Output "Dotnet version info"
$versionResult | Format-List -Property channel-version, latest-release, latest-release-date, latest-runtime, latest-sdk, support-phase, eol-date

# Update global.json file
$globalJson = [System.IO.Path]::Combine($path, "..\global.json")
$globalJson = Resolve-Path $globalJson
$json = Get-Content $globalJson | ConvertFrom-Json
Write-Output "Updating SDK version from $($json.sdk.version) to $($versionResult.'latest-sdk')"
$json.sdk.version = $versionResult.'latest-sdk'
$json | ConvertTo-Json  | Out-File -Encoding utf8 $globalJson

# Update RepoLayout.props file
$propsPath = [System.IO.Path]::Combine($path, "RepoLayout.props")
[xml]$props = Get-Content -Path $propsPath
$nd = $props.SelectSingleNode("/Project/PropertyGroup/RuntimeFrameworkVersion")
Write-Output "Updating Runtime version from $($nd.'#text') to $($versionResult.'latest-runtime')"
$nd.'#text' = $versionResult.'latest-runtime'
$settings = [System.Xml.XmlWriterSettings]@{
  Encoding = $encoding
  Indent   = $true    
}
$writer = [System.Xml.XmlWriter]::Create($propsPath, $settings)
$nd.OwnerDocument.Save($writer)
$writer.Dispose()

