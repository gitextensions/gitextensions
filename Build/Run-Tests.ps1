$testAssemblies = @();
$testAssemblies += (Get-ChildItem -Path UnitTests        -Filter '*Tests.dll' -Recurse -Exclude 'ApprovalTests.dll').FullName | Where-Object { $_.Contains('\bin\Release') }
$testAssemblies += (Get-ChildItem -Path IntegrationTests -Filter '*Tests.dll' -Recurse -Exclude 'ApprovalTests.dll').FullName | Where-Object { $_.Contains('\bin\Release') }
$packageConfig = [xml](Get-Content .nuget\packages.config)
$opencover_version = $packageConfig.SelectSingleNode('/packages/package[@id="OpenCover"]').version
$opencover_console = "packages\OpenCover.$opencover_version\tools\OpenCover.Console.exe"

$testRunCount = 1
for ($i=1; $i -le $testRunCount; $i++)
{
    Write-Host "[INFO]: Test Run ${i}/${testRunCount}"
    &$opencover_console `
        -register:administrator `
        -returntargetcode `
        -hideskipped:All `
        -filter:"+[*]* -[FluentAssertions*]* -[SmartFormat*]* -[nunit*]*" `
        -excludebyattribute:*.ExcludeFromCodeCoverage* `
        -excludebyfile:*\*Designer.cs `
        -output:"OpenCover.GitExtensions.xml" `
        -target:"nunit3-console.exe" `
        -targetargs:"$testAssemblies --workers=1 --timeout=90000"
    $testExitCode += $LastExitCode
    if ($LastExitCode -ne 0) { Write-Host "[ERROR]: Test run ${i} failed!" }
    $artifact_name = "TestResult" + $i + "." + $LastExitCode + "err.xml"
    Copy-Item "TestResult.xml" $artifact_name
    Push-AppveyorArtifact $artifact_name
}
if ($testExitCode -ne 0) { $host.SetShouldExit($testExitCode) }

$codecov_version = $packageConfig.SelectSingleNode('/packages/package[@id="Codecov"]').version
$codecov = "packages\Codecov.$codecov_version\tools\codecov.exe"
&$codecov -f ".\OpenCover.GitExtensions.xml" --flag production
&$codecov -f ".\OpenCover.GitExtensions.xml" --flag tests
