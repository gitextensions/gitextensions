[CmdletBinding()]
Param(
    [Parameter(Mandatory=$false, Position=1)]
    [string] $Configuration="Release",
    [switch] $IsPortable = $false
)

Write-Host ----------------------------------------------------------------------
Write-Host "Alter Config IsPortable:$IsPortable"
Write-Host ----------------------------------------------------------------------
#$pth = [System.IO.Path]::Combine($PSScriptRoot,"..\GitExtensions\bin\Release\GitExtensions.exe.config")
$pth = Resolve-Path ([System.IO.Path]::Combine($PSScriptRoot,"..\GitExtensions\bin\$Configuration\GitExtensions.exe.config"))
Write-Host $pth
if($pth){
    $doc= [xml](Get-Content $pth)
    $port=$doc.configuration.applicationSettings.'GitCommands.Properties.Settings'.setting|? {$_.name -eq 'IsPortable'}
    if($port){
        Write-Host "IsPortable was  $($port.value)"
        $port.value=$IsPortable.ToString()
        $doc.Save($pth)
        Write-Host "IsPortable is $($port.value)"
    }
    else {
        Write-Error -Message "Cannot find IsPortable setting in $pth"
    }
}
else {
    Write-Error -Message "Cannot find config file.  Did you run '.\build'?"
}