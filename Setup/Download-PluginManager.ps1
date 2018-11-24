[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=1)]
    [string] $ExtractRootPath
)

Set-Location $PSScriptRoot

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$Releases = Invoke-RestMethod -Uri 'https://api.github.com/repos/maraf/GitExtensions.PluginManager/releases'

$Assets = $Releases[0].assets;
foreach ($Asset in $Assets)
{
    if ($Asset.name.StartsWith('GitExtensions.PluginManager'))
    {
        $AssetToDownload = $Asset;
        break;
    }
}

if (!($null -eq $AssetToDownload))
{
    $AssetUrl = $AssetToDownload.browser_download_url;

    $DownloadName = [System.IO.Path]::GetFileName($AssetToDownload.name);
    $DownloadFilePath = [System.IO.Path]::Combine($ExtractRootPath, $DownloadName);
    $ExtractPath = [System.IO.Path]::Combine($ExtractRootPath, 'Output');

    if (!(Test-Path $DownloadFilePath))
    {
        if (!(Test-Path $ExtractRootPath))
        {
            New-Item -ItemType directory -Path $ExtractRootPath | Out-Null;
        }

        if (!(Test-Path $ExtractPath))
        {
            New-Item -ItemType directory -Path $ExtractPath | Out-Null;
        }

        Write-Host ('Downloading "' + $DownloadName + '".');

        Invoke-WebRequest -Uri $AssetUrl -OutFile $DownloadFilePath;
        Expand-Archive $DownloadFilePath -DestinationPath $ExtractPath -Force
    }
    else 
    {
        Write-Host ('Download "' + $DownloadName + '" already exists.');
    }
}

Pop-Location