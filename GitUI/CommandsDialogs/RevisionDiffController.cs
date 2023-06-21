using System.Diagnostics;
using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffController
    {
        void SaveFiles(List<FileStatusItem> files, Func<string, string?> userSelection);
        bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuEditWorkingDirectoryFile(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuOpenRevision(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuDeleteFile(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuShowInFolder(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo);
    }

    public sealed class ContextMenuSelectionInfo
    {
        // Defaults are set to simplify test cases, the defaults enables most
        public ContextMenuSelectionInfo(
            GitRevision? selectedRevision,
            bool isDisplayOnlyDiff,
            bool isStatusOnly,
            int selectedGitItemCount,
            bool isAnyItemIndex,
            bool isAnyItemWorkTree,
            bool isBareRepository,
            bool allFilesExist,
            bool allDirectoriesExist,
            bool allFilesOrUntrackedDirectoriesExist,
            bool isAnyTracked,
            bool supportPatches,
            bool isDeleted,
            bool isAnySubmodule)
        {
            SelectedRevision = selectedRevision;
            IsDisplayOnlyDiff = isDisplayOnlyDiff;
            IsStatusOnly = isStatusOnly;
            SelectedGitItemCount = selectedGitItemCount;
            IsAnyItemIndex = isAnyItemIndex;
            IsAnyItemWorkTree = isAnyItemWorkTree;
            IsBareRepository = isBareRepository;
            AllFilesExist = allFilesExist;
            AllDirectoriesExist = allDirectoriesExist;
            AllFilesOrUntrackedDirectoriesExist = allFilesOrUntrackedDirectoriesExist;
            IsAnyTracked = isAnyTracked;
            SupportPatches = supportPatches;
            IsDeleted = isDeleted;
            IsAnySubmodule = isAnySubmodule;
        }

        public GitRevision? SelectedRevision { get; }
        public bool IsDisplayOnlyDiff { get; }
        public bool IsStatusOnly { get; }
        public int SelectedGitItemCount { get; }
        public bool IsAnyItemIndex { get; }
        public bool IsAnyItemWorkTree { get; }
        public bool IsBareRepository { get; }
        public bool AllFilesExist { get; }
        public bool AllDirectoriesExist { get; }
        public bool AllFilesOrUntrackedDirectoriesExist { get; }
        public bool IsAnyTracked { get; }
        public bool SupportPatches { get; }
        public bool IsDeleted { get; }
        public bool IsAnySubmodule { get; }
    }

    public sealed class RevisionDiffController : IRevisionDiffController
    {
        private readonly Func<IGitModule> _getModule;
        private readonly IFullPathResolver _fullPathResolver;

        public RevisionDiffController(Func<IGitModule> getModule, IFullPathResolver fullPathResolver)
        {
            _getModule = getModule;
            _fullPathResolver = fullPathResolver;
        }

        public void SaveFiles(List<FileStatusItem> files, Func<string, string?> userSelection)
        {
            if (files is null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            if (files.Count == 0)
            {
                return;
            }

            if (userSelection is null)
            {
                throw new ArgumentNullException(nameof(userSelection));
            }

            if (files.Count > 1)
            {
                SaveMultipleFiles(files);
                return;
            }

            SaveSingleFile(files[0]);
            return;

            void SaveMultipleFiles(List<FileStatusItem> selectedFiles)
            {
                // Derive the folder from the first selected file.
                string firstItemFullName = _fullPathResolver.Resolve(selectedFiles.First().Item.Name);
                string baseSourceDirectory = Path.GetDirectoryName(firstItemFullName).EnsureTrailingPathSeparator();

                string selectedPath = userSelection(baseSourceDirectory);
                if (selectedPath is null)
                {
                    // User has cancelled the selection
                    return;
                }

                Uri baseSourceDirectoryUri = new(baseSourceDirectory);

                foreach (FileStatusItem item in selectedFiles)
                {
                    string selectedItemFullName = _fullPathResolver.Resolve(item.Item.Name);
                    string selectedItemSourceDirectory = Path.GetDirectoryName(selectedItemFullName).EnsureTrailingPathSeparator();

                    string targetDirectory;
                    if (selectedItemSourceDirectory == baseSourceDirectory)
                    {
                        targetDirectory = selectedPath;
                    }
                    else
                    {
                        Uri selectedItemUri = new(selectedItemSourceDirectory);
                        targetDirectory = Path.Combine(selectedPath, baseSourceDirectoryUri.MakeRelativeUri(selectedItemUri).OriginalString);
                    }

                    // TODO: check target file exists.
                    // TODO: allow cancel the whole sequence

                    string targetFileName = Path.Combine(targetDirectory, Path.GetFileName(selectedItemFullName)).ToNativePath();
                    Debug.WriteLine($"Saving {selectedItemFullName} --> {targetFileName}");

                    GetModule().SaveBlobAs(targetFileName, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
                }
            }

            void SaveSingleFile(FileStatusItem item)
            {
                string fullName = _fullPathResolver.Resolve(item.Item.Name);
                string selectedFileName = userSelection(fullName);
                if (selectedFileName is null)
                {
                    // User has cancelled the selection
                    return;
                }

                GetModule().SaveBlobAs(selectedFileName, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
            }
        }

        // The enabling of menu items is related to how the actions have been implemented

        public bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsDisplayOnlyDiff;
        }

        public bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount > 0
                && !selectionInfo.IsBareRepository
                && selectionInfo.IsAnyTracked
                && !selectionInfo.IsDisplayOnlyDiff;
        }

        #region Main menu items
        public bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsAnySubmodule
                && !(selectionInfo.SelectedRevision?.IsArtificial ?? false)
                && !selectionInfo.IsDisplayOnlyDiff;
        }

        public bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SupportPatches;
        }

        // Stage/unstage must limit the selected items, IsStaged is not reflecting Staged status
        public bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemWorkTree;
        }

        public bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemIndex;
        }

        public bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnySubmodule && selectionInfo.SelectedRevision?.ObjectId == ObjectId.WorkTreeId && selectionInfo.AllDirectoriesExist;
        }

        public bool ShouldShowMenuEditWorkingDirectoryFile(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount == 1 && selectionInfo.AllFilesExist;
        }

        public bool ShouldShowMenuDeleteFile(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.AllFilesOrUntrackedDirectoriesExist && (selectionInfo.SelectedRevision?.IsArtificial ?? false);
        }

        public bool ShouldShowMenuOpenRevision(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount == 1
                && !selectionInfo.IsAnySubmodule
                && !selectionInfo.IsDisplayOnlyDiff
                && !(selectionInfo.SelectedRevision?.IsArtificial ?? false);
        }

        public bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsStatusOnly;
        }

        public bool ShouldShowMenuShowInFolder(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsStatusOnly;
        }

        public bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount == 1
                && !(selectionInfo.SelectedRevision?.IsArtificial ?? false)
                && !selectionInfo.IsDeleted
                && !selectionInfo.IsStatusOnly;
        }

        public bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount == 1 && selectionInfo.IsAnyTracked;
        }

        public bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo)
        {
            return ShouldShowMenuFileHistory(selectionInfo) && !selectionInfo.IsAnySubmodule;
        }
        #endregion

        private IGitModule GetModule()
        {
            return _getModule() ?? throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
        }
    }
}
