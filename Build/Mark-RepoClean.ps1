# Mark all files that we inject version number into as clean

pushd $PSScriptRoot\..

# iterate through each submodule and mark each AssemblyInfo.cs in a submodule as clean
$submodules = git submodule --quiet foreach --recursive 'echo $name'
$submodules | ForEach-Object {
    pushd $_
    Get-ChildItem -Path .\ -Filter AssemblyInfo.cs -Recurse | ForEach-Object {
        & git update-index --skip-worktree $_.FullName
    }
    popd
}

& git update-index --skip-worktree CommonAssemblyInfo.cs
& git update-index --skip-worktree CommonAssemblyInfoExternals.cs
& git update-index --skip-worktree GitExtSshAskPass/SshAskPass.rc2
& git update-index --skip-worktree GitExtensionsShellEx/GitExtensionsShellEx.rc
& git update-index --skip-worktree GitExtensionsVSIX/source.extension.vsixmanifest
& git update-index --skip-worktree Setup/MakeInstallers.cmd

cd GitExtensionsDoc
& git update-index --skip-worktree source/conf.py
cd ..\

& git status
& git submodule foreach --recursive git status

popd