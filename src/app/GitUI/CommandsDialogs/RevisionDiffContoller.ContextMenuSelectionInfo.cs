using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

internal sealed record ContextMenuSelectionInfo(
    GitRevision? SelectedRevision,
    RelativePath? SelectedFolder,
    bool IsDisplayOnlyDiff,
    bool IsStatusOnly,
    int SelectedGitItemCount,
    bool IsAnyItemIndex,
    bool IsAnyItemWorkTree,
    bool IsBareRepository,
    bool AllFilesExist,
    bool AllDirectoriesExist,
    bool AllFilesOrUntrackedDirectoriesExist,
    bool IsAnyTracked,
    bool SupportPatches,
    bool IsDeleted,
    bool IsAnySubmodule);
