$testAssemblies = (Get-ChildItem -Path UnitTests -Filter '*Tests.dll' -Recurse -Exclude 'ApprovalTests.dll').FullName | Where-Object { $_.Contains('\bin\Release') }
$packageConfig = [xml](Get-Content .nuget\packages.config)
$opencover_version = $packageConfig.SelectSingleNode('/packages/package[@id="OpenCover"]').version
$opencover_console = "packages\OpenCover.$opencover_version\tools\OpenCover.Console.exe"

&$opencover_console `
    -register:user `
    -returntargetcode `
    -hideskipped:All `
    -filter:"+[*]* -[FluentAssertions*]* -[SmartFormat*]* -[nunit*]*" `
    -excludebyattribute:*.ExcludeFromCodeCoverage* `
    -excludebyfile:*\*Designer.cs `
    -output:"OpenCover.GitExtensions.xml" `
    -target:"nunit3-console.exe" `
    -targetargs:"$testAssemblies"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

$codecov_version = $packageConfig.SelectSingleNode('/packages/package[@id="Codecov"]').version
$codecov = "packages\Codecov.$codecov_version\tools\codecov.exe"
&$codecov -f ".\OpenCover.GitExtensions.xml" --flag production
&$codecov -f ".\OpenCover.GitExtensions.xml" --flag tests
