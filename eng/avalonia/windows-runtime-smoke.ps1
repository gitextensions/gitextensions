param(
    [string]$EvidenceDirectory,

    [switch]$BrowseWindow,

    [switch]$OpenViewMenu,

    [switch]$CheckPlugins
)

$ErrorActionPreference = "Stop"
if (($OpenViewMenu -or $CheckPlugins) -and -not $BrowseWindow)
{
    throw "Menus can only be inspected on a Browse window."
}
if ($OpenViewMenu -and $CheckPlugins)
{
    throw "Inspect one menu per smoke run."
}

# parity-scaffolding: records a real Windows runtime window until the parity gate closes.
$repositoryRoot = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0)
{
    throw "Unable to locate the repository root."
}

if ([string]::IsNullOrWhiteSpace($EvidenceDirectory))
{
    $EvidenceDirectory = Join-Path $repositoryRoot "eng/avalonia/parity-evidence/P0.6/windows"
}

$applicationOutput = Join-Path $repositoryRoot "artifacts/Debug/bin/GitExtensions.Avalonia/net10.0"
$sourceApplication = Join-Path $applicationOutput "GitExtensions.Avalonia.exe"
if (-not [IO.File]::Exists($sourceApplication))
{
    throw "Build the Avalonia solution before running the Windows runtime smoke."
}

$captureScript = Join-Path $repositoryRoot "eng/avalonia/Capture-SmokeWindow.ps1"
$temporaryParent = [IO.Path]::GetTempPath().TrimEnd([IO.Path]::DirectorySeparatorChar)
$smokeRoot = Join-Path $temporaryParent "gitextensions-p06-runtime-$([Guid]::NewGuid().ToString('N'))"
$fixtureRepository = Join-Path $smokeRoot "repository-$([Guid]::NewGuid().ToString('N'))"
$settingsRoot = Join-Path $smokeRoot "settings"
$runtimeRoot = Join-Path $smokeRoot "runtime"
$application = Join-Path $runtimeRoot "GitExtensions.Avalonia.exe"
$process = $null
$stdoutTask = $null
$stderrTask = $null
$outputCaptured = $false
$registryScope = $null
$smokeManifest = $null

function Invoke-Git
{
    & git @args
    if ($LASTEXITCODE -ne 0)
    {
        throw "Git failed with exit code $LASTEXITCODE."
    }
}

function New-HardLinkedRuntime
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Destination
    )

    if ([IO.Path]::GetPathRoot($Source) -ne [IO.Path]::GetPathRoot($Destination))
    {
        throw "The disposable runtime must be on the same volume as the build output."
    }

    $independentFiles = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    $independentFiles.Add("GitCommands.dll.config") | Out-Null
    $independentFiles.Add("GitExtensions.Avalonia.dll.config") | Out-Null
    $independentFiles.Add("GitExtensions.settings") | Out-Null

    foreach ($sourceFile in Get-ChildItem -LiteralPath $Source -Recurse -File)
    {
        $relativePath = [IO.Path]::GetRelativePath($Source, $sourceFile.FullName)
        if ($independentFiles.Contains($relativePath))
        {
            continue
        }

        $destinationFile = Join-Path $Destination $relativePath
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destinationFile)) | Out-Null
        New-Item -ItemType HardLink -Path $destinationFile -Target $sourceFile.FullName | Out-Null
    }
}

try
{
    [IO.Directory]::CreateDirectory($fixtureRepository) | Out-Null
    [IO.Directory]::CreateDirectory($settingsRoot) | Out-Null
    [IO.Directory]::CreateDirectory($runtimeRoot) | Out-Null
    [IO.Directory]::CreateDirectory($EvidenceDirectory) | Out-Null

    if ([Environment]::Version.Major -lt 10)
    {
        throw "The Windows runtime smoke requires PowerShell hosted on .NET 10 or later."
    }

    $registryTests = Join-Path $repositoryRoot "eng/tools/WindowsRuntimeIsolation.Tests/WindowsRuntimeIsolation.Tests.csproj"
    $registryBuildLog = Join-Path $EvidenceDirectory "registry-contract-build.log"
    $registryTestLog = Join-Path $EvidenceDirectory "registry-contract-tests.log"
    & dotnet build $registryTests -m:1 -v:minimal *> $registryBuildLog
    if ($LASTEXITCODE -ne 0)
    {
        throw "Registry isolation contract build failed; application launch refused. See $registryBuildLog."
    }

    $registryHelper = Join-Path $repositoryRoot "artifacts/Debug/bin/WindowsRuntimeIsolation/net10.0-windows/WindowsRuntimeIsolation.dll"
    [Reflection.Assembly]::LoadFrom($registryHelper) | Out-Null
    $registryScope = [GitExtensions.WindowsRuntimeIsolation.WindowsRegistryIsolationScope]::new()
    & dotnet test $registryTests --no-build --no-restore --logger "trx;LogFileName=registry-contracts.trx" --results-directory $EvidenceDirectory *> $registryTestLog
    if ($LASTEXITCODE -ne 0)
    {
        throw "Registry isolation contracts failed; application launch refused. See $registryTestLog."
    }

    $registryScope.ProveAsync((Join-Path $applicationOutput "GitExtensions.Avalonia.runtimeconfig.json")).GetAwaiter().GetResult()

    New-HardLinkedRuntime -Source $applicationOutput -Destination $runtimeRoot
    if (-not [IO.Directory]::Exists($fixtureRepository))
    {
        throw "The fixture repository directory disappeared while preparing the disposable runtime."
    }

    $portableConfiguration = "<?xml version=`"1.0`" encoding=`"utf-8`"?>`r`n<configuration>`r`n  <configSections>`r`n    <sectionGroup name=`"applicationSettings`" type=`"System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089`">`r`n      <section name=`"GitCommands.Properties.Settings`" type=`"System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089`" requirePermission=`"false`" />`r`n    </sectionGroup>`r`n  </configSections>`r`n  <applicationSettings>`r`n    <GitCommands.Properties.Settings>`r`n      <setting name=`"IsPortable`" serializeAs=`"String`"><value>True</value></setting>`r`n    </GitCommands.Properties.Settings>`r`n  </applicationSettings>`r`n</configuration>`r`n"
    [IO.File]::WriteAllText((Join-Path $runtimeRoot "GitCommands.dll.config"), $portableConfiguration)
    [IO.File]::WriteAllText((Join-Path $runtimeRoot "GitExtensions.Avalonia.dll.config"), $portableConfiguration)
    [IO.File]::WriteAllText(
        (Join-Path $runtimeRoot "GitExtensions.settings"),
        "<?xml version=`"1.0`" encoding=`"utf-8`"?>`r`n<dictionary>`r`n  <item>`r`n    <key><string>CheckSettings</string></key>`r`n    <value><string>false</string></value>`r`n  </item>`r`n  <item>`r`n    <key><string>translation</string></key>`r`n    <value><string>English</string></value>`r`n  </item>`r`n</dictionary>`r`n")

    Invoke-Git -C $fixtureRepository init --quiet --initial-branch=main
    Invoke-Git -C $fixtureRepository config user.name "P0.6 Runtime Smoke"
    Invoke-Git -C $fixtureRepository config user.email "p06-smoke@example.invalid"
    [IO.File]::WriteAllText((Join-Path $fixtureRepository "smoke.txt"), "runtime smoke`r`n")
    Invoke-Git -C $fixtureRepository add smoke.txt
    Invoke-Git -C $fixtureRepository -c commit.gpgSign=false commit --quiet -m initial

    $stdoutLog = Join-Path $EvidenceDirectory "stdout.log"
    $stderrLog = Join-Path $EvidenceDirectory "stderr.log"
    $captureLog = Join-Path $EvidenceDirectory "capture.log"
    $screenshot = Join-Path $EvidenceDirectory "window.png"
    $prerequisiteScreenshot = Join-Path $EvidenceDirectory "prerequisite.png"
    $manifest = Join-Path $EvidenceDirectory "smoke.json"
    foreach ($evidencePath in @($stdoutLog, $stderrLog, $captureLog, $screenshot, $prerequisiteScreenshot, $manifest))
    {
        [IO.File]::Delete($evidencePath)
    }

    $jitDebuggerIds = @(Get-Process -Name vsjitdebugger -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $application
    $startInfo.Arguments = "browse `"$fixtureRepository`""
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.EnvironmentVariables["APPDATA"] = Join-Path $settingsRoot "roaming"
    $startInfo.EnvironmentVariables["LOCALAPPDATA"] = Join-Path $settingsRoot "local"
    $startInfo.EnvironmentVariables["GIT_CONFIG_GLOBAL"] = "NUL"
    $startInfo.EnvironmentVariables["GITEXTENSIONS_DEBUG_FAIL_FAST"] = "1"
    $process = $registryScope.StartApplication($startInfo)
    if ($null -eq $process)
    {
        throw "Unable to start the Avalonia application."
    }

    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if ($BrowseWindow)
    {
        $deadline = [DateTime]::UtcNow.AddSeconds(45)
        do
        {
            if ($process.HasExited)
            {
                throw "The Avalonia application exited before opening its first window."
            }

            $process.Refresh()
            $firstTitle = $process.MainWindowTitle
            if ($firstTitle -like '*Settings - Checklist*' -or
                $firstTitle -like "*$([IO.Path]::GetFileName($fixtureRepository))*")
            {
                break
            }

            Start-Sleep -Milliseconds 500
        }
        while ([DateTime]::UtcNow -lt $deadline)

        if ($firstTitle -like '*Settings - Checklist*')
        {
            & $captureScript -TitlePattern 'Settings - Checklist' -ProcessId $process.Id -OutputPath $prerequisiteScreenshot -TimeoutSeconds 15 | Out-Null
            if (-not $process.CloseMainWindow())
            {
                throw 'The startup checklist could not be closed to inspect the browse window.'
            }
        }
    }

    $titlePattern = if ($BrowseWindow) { [IO.Path]::GetFileName($fixtureRepository) } else { "Settings - Checklist" }
    $captureArguments = @{
        TitlePattern = $titlePattern
        ProcessId = $process.Id
        OutputPath = $screenshot
        TimeoutSeconds = 45
    }
    if ($OpenViewMenu)
    {
        $captureArguments.CaptureMethod = "Screen"
        $captureArguments.OpenViewMenu = $true
    }
    if ($CheckPlugins)
    {
        $captureArguments.CheckPlugins = $true
    }

    $captureOutput = & $captureScript @captureArguments
    $captureOutput | Set-Content -LiteralPath $captureLog
    $captureMethod = ($captureOutput | Where-Object { $_ -like 'captureMethod=*' } |
        Select-Object -First 1) -replace '^captureMethod=', ''
    if ($captureMethod -notin @('PrintWindow', 'Screen'))
    {
        throw "The capture did not report a supported image method."
    }

    if ($process.HasExited)
    {
        throw "The Avalonia application exited during the Windows capture with code $($process.ExitCode)."
    }

    $newJitDebuggers = @(
        Get-Process -Name vsjitdebugger -ErrorAction SilentlyContinue |
            Where-Object { $_.Id -notin $jitDebuggerIds }
    )
    if ($newJitDebuggers.Count -ne 0)
    {
        throw "The runtime smoke opened a JIT debugger process."
    }

    if (-not [IO.File]::Exists($screenshot) -or (Get-Item -LiteralPath $screenshot).Length -eq 0)
    {
        throw "The Windows screenshot provider produced no image."
    }

    & taskkill.exe /PID $process.Id /T /F 2>&1 | Out-Null
    $process.WaitForExit()
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    [IO.File]::WriteAllText($stdoutLog, $stdout)
    [IO.File]::WriteAllText($stderrLog, $stderr)
    $outputCaptured = $true
    if ("$stdout`n$stderr" -match "(?m)Unhandled exception|fatal error|JIT debugger|Avalonia.*error|^([A-Za-z_][A-Za-z0-9_]*\.)+[A-Za-z0-9_]*Exception:")
    {
        throw "The Windows runtime logs contain a failure signature."
    }

    $screenshotHash = (Get-FileHash -LiteralPath $screenshot -Algorithm SHA256).Hash.ToLowerInvariant()
    $smokeManifest = [ordered]@{
        schemaVersion = 1
        platform = "windows"
        command = "browse <temporary-repository>"
        observedSurface = if ($BrowseWindow) { "browse" } else { "prerequisiteChecklist" }
        requestedInteraction = if ($OpenViewMenu) { "openViewMenu" } elseif ($CheckPlugins) { "inspectPluginsMenu" } else { $null }
        pluginEvidence = if ($CheckPlugins) { 'window.plugins.json' } else { $null }
        captureMethod = $captureMethod
        prerequisiteScreenshot = if ([IO.File]::Exists($prerequisiteScreenshot)) { "prerequisite.png" } else { $null }
        settingsFileIsolation = "disposablePortableRuntime"
        registryAccess = "isolatedProcessHive"
        registryEvidence = "registry-isolation.json"
        repositoryLocation = "outsideWorkingTree"
        screenshot = "window.png"
        screenshotSha256 = $screenshotHash
        stdout = "stdout.log"
        stderr = "stderr.log"
    }
}
finally
{
    $processExited = $null -eq $process
    try
    {
        if ($null -ne $process)
        {
            if (-not $process.HasExited)
            {
                & taskkill.exe /PID $process.Id /T /F 2>&1 | Out-Null
            }

            $processExited = $process.WaitForExit(10000)
            if (-not $processExited)
            {
                throw "The isolated application did not terminate; its registry root and runtime are retained."
            }

            if (-not $outputCaptured -and $null -ne $stdoutTask)
            {
                [IO.File]::WriteAllText($stdoutLog, $stdoutTask.GetAwaiter().GetResult())
            }

            if (-not $outputCaptured -and $null -ne $stderrTask)
            {
                [IO.File]::WriteAllText($stderrLog, $stderrTask.GetAwaiter().GetResult())
            }
        }
    }
    finally
    {
        try
        {
            if ($null -ne $registryScope)
            {
                try
                {
                    $registryScope.Dispose()
                }
                finally
                {
                    [IO.File]::WriteAllText((Join-Path $EvidenceDirectory "registry-isolation.json"), $registryScope.GetEvidenceJson())
                }
            }
        }
        finally
        {
            if ($null -ne $process)
            {
                $process.Dispose()
            }

            $resolvedSmokeRoot = [IO.Path]::GetFullPath($smokeRoot)
            $resolvedTemporaryParent = [IO.Path]::GetFullPath($temporaryParent) + [IO.Path]::DirectorySeparatorChar
            if ($processExited -and
                $resolvedSmokeRoot.StartsWith($resolvedTemporaryParent, [StringComparison]::OrdinalIgnoreCase) -and
                [IO.Directory]::Exists($resolvedSmokeRoot))
            {
                # Git creates hidden/read-only metadata in the disposable repository. Clear only
                # that repository's attributes; the runtime uses hard links to build output.
                Get-ChildItem -LiteralPath $fixtureRepository -Recurse -Force -ErrorAction SilentlyContinue |
                    Where-Object {
                        $_.Attributes -band ([IO.FileAttributes]::Hidden -bor [IO.FileAttributes]::ReadOnly)
                    } |
                    ForEach-Object {
                        $_.Attributes = $_.Attributes -band -bnot (
                            [IO.FileAttributes]::Hidden -bor [IO.FileAttributes]::ReadOnly)
                    }
                Remove-Item -LiteralPath $resolvedSmokeRoot -Recurse
            }
        }
    }
}

$smokeManifest | ConvertTo-Json | Set-Content -LiteralPath $manifest
Write-Output "Windows smoke passed; screenshot=$screenshotHash; owner registry unchanged."
