using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffController
    {
        bool ShouldShowMenuBlame(SelectionInfo selectionInfo);
        bool ShouldShowMenuCherryPick(SelectionInfo selectionInfo);
        bool ShouldShowMenuEditFile(SelectionInfo selectionInfo);
        bool ShouldShowMenuResetFile(SelectionInfo selectionInfo);
        bool ShouldShowMenuFileHistory(SelectionInfo selectionInfo);
        bool ShouldShowMenuSaveAs(SelectionInfo selectionInfo);
        bool ShouldShowMenuShowInFileTree(SelectionInfo selectionInfo);
        bool ShouldShowMenuStage(IList<GitRevision> selectedRevisions);
        bool ShouldShowMenuUnstage(IList<GitRevision> selectedRevisions);
        bool ShouldShowSubmoduleMenus(SelectionInfo selectionInfo);
    }

    public sealed class SelectionInfo
    {
        public SelectionInfo(IList<GitRevision> selectedRevisions, GitItemStatus selectedDiff, bool isSingleGitItemSelected, bool isCombinedDiff)
        {
            SelectedRevisions = selectedRevisions;
            SelectedDiff = selectedDiff;
            IsSingleGitItemSelected = isSingleGitItemSelected;
            IsCombinedDiff = isCombinedDiff;
        }
        public IList<GitRevision> SelectedRevisions { get; }
        public GitItemStatus SelectedDiff { get; }
        public bool IsSingleGitItemSelected { get; }
        public bool IsCombinedDiff { get; }
    }

    public sealed class RevisionDiffController : IRevisionDiffController
    {
        private readonly IGitModule _module;

        public RevisionDiffController(IGitModule module)
        {
            _module = module;
        }


        public bool ShouldShowMenuBlame(SelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !(selectionInfo.SelectedDiff.IsSubmodule || selectionInfo.SelectedRevisions[0].IsArtificial());
        }

        public bool ShouldShowMenuCherryPick(SelectionInfo selectionInfo)
        {
            return !selectionInfo.IsCombinedDiff && selectionInfo.IsSingleGitItemSelected &&
                   !(selectionInfo.SelectedDiff.IsSubmodule || selectionInfo.SelectedRevisions[0].Guid == GitRevision.UnstagedGuid ||
                     (selectionInfo.SelectedDiff.IsNew || selectionInfo.SelectedDiff.IsDeleted) && selectionInfo.SelectedRevisions[0].Guid == GitRevision.IndexGuid);
        }

        public bool ShouldShowMenuEditFile(SelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedDiff.IsSubmodule && selectionInfo.SelectedRevisions[0].IsArtificial();
        }

        public bool ShouldShowMenuResetFile(SelectionInfo selectionInfo)
        {
            return !selectionInfo.IsCombinedDiff &&
                !(selectionInfo.IsSingleGitItemSelected && (selectionInfo.SelectedDiff.IsSubmodule || selectionInfo.SelectedDiff.IsNew) && selectionInfo.SelectedRevisions[0].Guid == GitRevision.UnstagedGuid);
        }

        public bool ShouldShowMenuFileHistory(SelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !(selectionInfo.SelectedDiff.IsNew && selectionInfo.SelectedRevisions[0].IsArtificial());
        }

        public bool ShouldShowMenuSaveAs(SelectionInfo selectionInfo)
        {
            return !selectionInfo.IsCombinedDiff && selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedDiff.IsSubmodule;
        }

        public bool ShouldShowMenuShowInFileTree(SelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedRevisions[0].IsArtificial();
        }

        public bool ShouldShowMenuStage(IList<GitRevision> selectedRevisions)
        {
            return selectedRevisions.Count >= 1 && selectedRevisions[0].Guid == GitRevision.UnstagedGuid ||
                   selectedRevisions.Count >= 2 && selectedRevisions[1].Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuUnstage(IList<GitRevision> selectedRevisions)
        {
            return selectedRevisions.Count >= 1 && selectedRevisions[0].Guid == GitRevision.IndexGuid ||
                   selectedRevisions.Count >= 2 && selectedRevisions[1].Guid == GitRevision.IndexGuid;
        }

        public bool ShouldShowSubmoduleMenus(SelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && selectionInfo.SelectedDiff.IsSubmodule && selectionInfo.SelectedRevisions[0].Guid == GitRevision.UnstagedGuid;
        }
    }
}