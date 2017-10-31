using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffController
    {
        bool ShouldShowMenuBlame(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision);
        bool ShouldShowMenuCherryPick(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision, bool isCombinedDiff);
        bool ShouldShowMenuEditFile(bool isExactlyOneItemSelected, GitItemStatus diffFilesSelectedItem, GitRevision selectedRevision);
        bool ShouldShowMenuResetFile(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision, bool isCombinedDiff);
        bool ShouldShowMenuFileHistory(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision);
        bool ShouldShowMenuSaveAs(GitItemStatus itemStatus, bool isSingleGitItemSelected, bool isCombinedDiff);
        bool ShouldShowMenuShowInFileTree(bool isSingleGitItemSelected, GitRevision selectedRevision);
        bool ShouldShowMenuStage(IList<GitRevision> selectedRevisions);
        bool ShouldShowMenuUnstage(IList<GitRevision> selectedRevisions);
        bool ShouldShowSubmoduleMenus(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision);
    }

    public sealed class RevisionDiffController : IRevisionDiffController
    {
        private readonly IGitModule _module;

        public RevisionDiffController(IGitModule module)
        {
            _module = module;
        }


        public bool ShouldShowMenuBlame(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision)
        {
            return isSingleGitItemSelected && !(itemStatus.IsSubmodule || selectedRevision.IsArtificial());
        }

        public bool ShouldShowMenuCherryPick(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision, bool isCombinedDiff)
        {
            return !isCombinedDiff && isSingleGitItemSelected &&
                   !(itemStatus.IsSubmodule || selectedRevision.Guid == GitRevision.UnstagedGuid ||
                     (itemStatus.IsNew || itemStatus.IsDeleted) && selectedRevision.Guid == GitRevision.IndexGuid);
        }

        public bool ShouldShowMenuEditFile(bool isSingleGitItemSelected, GitItemStatus itemStatus, GitRevision selectedRevision)
        {
            return isSingleGitItemSelected && !itemStatus.IsSubmodule && selectedRevision.IsArtificial();
        }

        public bool ShouldShowMenuResetFile(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision, bool isCombinedDiff)
        {
            return !isCombinedDiff && !(isSingleGitItemSelected && (itemStatus.IsSubmodule || itemStatus.IsNew) && selectedRevision.Guid == GitRevision.UnstagedGuid);
        }

        public bool ShouldShowMenuFileHistory(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision)
        {
            return isSingleGitItemSelected && !(itemStatus.IsNew && selectedRevision.IsArtificial());
        }

        public bool ShouldShowMenuSaveAs(GitItemStatus itemStatus, bool isSingleGitItemSelected, bool isCombinedDiff)
        {
            return !isCombinedDiff && isSingleGitItemSelected && !itemStatus.IsSubmodule;
        }

        public bool ShouldShowMenuShowInFileTree(bool isSingleGitItemSelected, GitRevision selectedRevision)
        {
            return isSingleGitItemSelected && !selectedRevision.IsArtificial();
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

        public bool ShouldShowSubmoduleMenus(GitItemStatus itemStatus, bool isSingleGitItemSelected, GitRevision selectedRevision)
        {
            return isSingleGitItemSelected && itemStatus.IsSubmodule && selectedRevision.Guid == GitRevision.UnstagedGuid;
        }
    }
}