param(
    [string]$EvidenceDirectory
)

$ErrorActionPreference = "Stop"

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
$fixtureRepository = Join-Path $smokeRoot "repository"
$settingsRoot = Join-Path $smokeRoot "settings"
$runtimeRoot = Join-Path $smokeRoot "runtime"
$application = Join-Path $runtimeRoot "GitExtensions.Avalonia.exe"
$process = $null
$stdoutTask = $null
$stderrTask = $null
$outputCaptured = $false

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
    $manifest = Join-Path $EvidenceDirectory "smoke.json"
    foreach ($evidencePath in @($stdoutLog, $stderrLog, $captureLog, $screenshot, $manifest))
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
    $process = [Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process)
    {
        throw "Unable to start the Avalonia application."
    }

    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    & $captureScript -TitlePattern "Settings - Checklist" -OutputPath $screenshot -TimeoutSeconds 45 |
        Set-Content -LiteralPath $captureLog

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
    if ("$stdout`n$stderr" -match "Unhandled exception|fatal error|JIT debugger|Avalonia.*error")
    {
        throw "The Windows runtime logs contain a failure signature."
    }

    $screenshotHash = (Get-FileHash -LiteralPath $screenshot -Algorithm SHA256).Hash.ToLowerInvariant()
    [ordered]@{
        schemaVersion = 1
        platform = "windows"
        command = "browse <temporary-repository>"
        observedSurface = "prerequisiteChecklist"
        settingsFileIsolation = "disposablePortableRuntime"
        registryAccess = "readOnly"
        repositoryLocation = "outsideWorkingTree"
        screenshot = "window.png"
        screenshotSha256 = $screenshotHash
        stdout = "stdout.log"
        stderr = "stderr.log"
    } | ConvertTo-Json | Set-Content -LiteralPath $manifest

    Write-Output "Windows smoke passed; screenshot=$screenshotHash"
}
finally
{
    if ($null -ne $process)
    {
        if (-not $process.HasExited)
        {
            & taskkill.exe /PID $process.Id /T /F 2>&1 | Out-Null
        }

        if (-not $outputCaptured -and $null -ne $stdoutTask)
        {
            [IO.File]::WriteAllText($stdoutLog, $stdoutTask.GetAwaiter().GetResult())
        }

        if (-not $outputCaptured -and $null -ne $stderrTask)
        {
            [IO.File]::WriteAllText($stderrLog, $stderrTask.GetAwaiter().GetResult())
        }

        $process.Dispose()
    }

    $resolvedSmokeRoot = [IO.Path]::GetFullPath($smokeRoot)
    $resolvedTemporaryParent = [IO.Path]::GetFullPath($temporaryParent) + [IO.Path]::DirectorySeparatorChar
    if ($resolvedSmokeRoot.StartsWith($resolvedTemporaryParent, [StringComparison]::OrdinalIgnoreCase) -and
        [IO.Directory]::Exists($resolvedSmokeRoot))
    {
        Remove-Item -LiteralPath $resolvedSmokeRoot -Recurse -Force
    }
}
