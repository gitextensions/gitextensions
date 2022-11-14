[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=1)]
    [string] $Files
)

$Files.Split(';') | `
    ForEach-Object { 
        $fileContent = Get-Content $_ | ConvertFrom-Json;

        # Remove all frameworks except "Microsoft.WindowsDesktop.App"
        $fileContent.runtimeOptions.frameworks = @( $fileContent.runtimeOptions.frameworks | Where-Object { $_.name -eq 'Microsoft.WindowsDesktop.App' } );

        $fileContent | ConvertTo-Json -Depth 5 | Out-File $_ -Encoding ascii;
    }
