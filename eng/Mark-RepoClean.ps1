# Mark all files that we inject version number into as clean

pushd $PSScriptRoot\..

# If the .NET SDK/runtime version was updated - make build clean
& git update-index --skip-worktree global.json
& git update-index --skip-worktree eng/RepoLayout.props

# iterate through each submodule and mark each AssemblyInfo.cs in a submodule as clean
$submodules = git submodule --quiet foreach --recursive 'echo $name'
$submodules | ForEach-Object {
    pushd $_
    Get-ChildItem -Path .\ -Filter AssemblyInfo.cs -Recurse | ForEach-Object {
        & git update-index --skip-worktree $_.FullName
    }
    popd
}

& git update-index --skip-worktree Externals/NetSpell.SpellChecker/Properties/AssemblyInfo.cs
& git update-index --skip-worktree GitUI/CommandsDialogs/FormBrowse.cs
& git update-index --skip-worktree CommonAssemblyInfo.cs
& git update-index --skip-worktree CommonAssemblyInfoExternals.cs
& git update-index --skip-worktree GitExtSshAskPass/SshAskPass.rc2
& git update-index --skip-worktree GitExtensionsShellEx/GitExtensionsShellEx.rc

& git status
& git submodule foreach --recursive git status

popd