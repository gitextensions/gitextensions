using System;
using GitCommands;
using ResourceManager;

namespace GitUI
{
    internal sealed class Strings : Translate
    {
        private readonly TranslationString _error = new TranslationString("Error");
        private readonly TranslationString _warning = new TranslationString("Warning");
        private readonly TranslationString _yes = new TranslationString("Yes");
        private readonly TranslationString _no = new TranslationString("No");
        private readonly TranslationString _okText = new TranslationString("OK");
        private readonly TranslationString _cancelText = new TranslationString("Cancel");

        private readonly TranslationString _containedInBranchesText = new TranslationString("Contained in branches:");
        private readonly TranslationString _containedInNoBranchText = new TranslationString("Contained in no branch");
        private readonly TranslationString _containedInTagsText = new TranslationString("Contained in tags:");
        private readonly TranslationString _containedInNoTagText = new TranslationString("Contained in no tag");
        private readonly TranslationString _viewPullRequest = new TranslationString("View pull requests");
        private readonly TranslationString _createPullRequest = new TranslationString("Create pull request");
        private readonly TranslationString _forkCloneRepo = new TranslationString("Fork or clone a repository");
        private readonly TranslationString _branchText = new TranslationString("Branch");
        private readonly TranslationString _branchesText = new TranslationString("Branches");
        private readonly TranslationString _remotesText = new TranslationString("Remotes");
        private readonly TranslationString _tagsText = new TranslationString("Tags");
        private readonly TranslationString _submodulesText = new TranslationString("Submodules");
        private readonly TranslationString _bodyNotLoaded = new TranslationString("\n\nFull message text is not present in older commits.\nSelect this commit to populate the full message.");
        private readonly TranslationString _searchingFor = new TranslationString("Searching for: ");
        private readonly TranslationString _loadingDataText = new TranslationString("Loading data...");
        private readonly TranslationString _uninterestingDiffOmitted = new TranslationString("Uninteresting diff hunks are omitted.");
        private readonly TranslationString _openReport = new TranslationString("Open report");
        private readonly TranslationString _noResultsFound = new TranslationString("<No results found>");
        private readonly TranslationString _local = new TranslationString("Local");
        private readonly TranslationString _tag = new TranslationString("Tag");
        private readonly TranslationString _remote = new TranslationString("Remote");
        private readonly TranslationString _openWithGitExtensions = new TranslationString("Open with Git Extensions");
        private readonly TranslationString _contScrollToNextFileOnlyWithAlt = new TranslationString("Enable automatic continuous scroll (without ALT button)");
        private readonly TranslationString _noRevision = new TranslationString("No revision");

        private readonly TranslationString _authored = new TranslationString("authored");
        private readonly TranslationString _committed = new TranslationString("committed");
        private readonly TranslationString _authoredAndCommitted = new TranslationString("authored and committed");
        private readonly TranslationString _markBisectAsGood = new TranslationString("Marked as good in bisect");
        private readonly TranslationString _markBisectAsBad = new TranslationString("Marked as bad in bisect");

        private readonly TranslationString _errorCaptionFailedDeleteFile = new TranslationString("Failed to delete file");
        private readonly TranslationString _errorCaptionFailedDeleteFolder = new TranslationString("Failed to delete directory");

        private readonly TranslationString _noBranch = new TranslationString("no branch");
        private readonly TranslationString _removeSelectedInvalidRepository = new TranslationString("Remove the selected invalid repository");
        private readonly TranslationString _removeAllInvalidRepositories = new TranslationString("Remove all {0} invalid repositories");
        private readonly TranslationString _open = new TranslationString("Open");
        private readonly TranslationString _directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.");

        private readonly TranslationString _showDiffForAllParentsText = new TranslationString("Show file differences for all parents in browse dialog");
        private readonly TranslationString _showDiffForAllParentsTooltip = new TranslationString(@"Show all differences between the selected commits, not limiting to only one difference.

- For a single selected commit, show the difference with its parent commit.
- For a single selected merge commit, show the difference with all parents.
- For two selected commits with a common ancestor (BASE):
   - Show the difference between the commits.
   - The difference of unique files from BASE to each of the selected commits.
   - The difference of common files (identical changes) from BASE to the commits.
- For multiple selected commits (up to four), show the difference for all the first selected with the last selected commit.
- For more than four selected commits, show the difference from the first to the last selected commit.");

        // public only because of FormTranslate
        public Strings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static Lazy<Strings> _instance = new Lazy<Strings>();

        public static void Reinitialize()
        {
            if (_instance.IsValueCreated)
            {
                _instance = new Lazy<Strings>();
            }
        }

        public static string Error => _instance.Value._error.Text;
        public static string Warning => _instance.Value._warning.Text;
        public static string Yes => _instance.Value._yes.Text;
        public static string No => _instance.Value._no.Text;
        public static string OK => _instance.Value._okText.Text;
        public static string Cancel => _instance.Value._cancelText.Text;

        public static string ContainedInBranches => _instance.Value._containedInBranchesText.Text;
        public static string ContainedInNoBranch => _instance.Value._containedInNoBranchText.Text;
        public static string ContainedInTags => _instance.Value._containedInTagsText.Text;
        public static string ContainedInNoTag => _instance.Value._containedInNoTagText.Text;

        public static string CreatePullRequest => _instance.Value._createPullRequest.Text;
        public static string ForkCloneRepo => _instance.Value._forkCloneRepo.Text;
        public static string ViewPullRequest => _instance.Value._viewPullRequest.Text;

        public static string Branch => _instance.Value._branchText.Text;
        public static string Branches => _instance.Value._branchesText.Text;
        public static string Remotes => _instance.Value._remotesText.Text;
        public static string Tags => _instance.Value._tagsText.Text;
        public static string Submodules => _instance.Value._submodulesText.Text;

        public static string BodyNotLoaded => _instance.Value._bodyNotLoaded.Text;
        public static string SearchingFor => _instance.Value._searchingFor.Text;

        public static string LoadingData => _instance.Value._loadingDataText.Text;
        public static string UninterestingDiffOmitted => _instance.Value._uninterestingDiffOmitted.Text;
        public static string NoResultsFound => _instance.Value._noResultsFound.Text;
        public static string Local => _instance.Value._local.Text;
        public static string Tag => _instance.Value._tag.Text;
        public static string Remote => _instance.Value._remote.Text;
        public static string OpenWithGitExtensions => _instance.Value._openWithGitExtensions.Text;
        public static string ContScrollToNextFileOnlyWithAlt => _instance.Value._contScrollToNextFileOnlyWithAlt.Text;
        public static string NoRevision => _instance.Value._noRevision.Text;

        public static string OpenReport => _instance.Value._openReport.Text;

        public static string Authored => _instance.Value._authored.Text;
        public static string Committed => _instance.Value._committed.Text;
        public static string AuthoredAndCommitted => _instance.Value._authoredAndCommitted.Text;

        public static string MarkBisectAsGood => _instance.Value._markBisectAsGood.Text;
        public static string MarkBisectAsBad => _instance.Value._markBisectAsBad.Text;

        public static string ErrorCaptionFailedDeleteFile => _instance.Value._errorCaptionFailedDeleteFile.Text;
        public static string ErrorCaptionFailedDeleteFolder => _instance.Value._errorCaptionFailedDeleteFolder.Text;

        public static string NoBranch => _instance.Value._noBranch.Text;
        public static string RemoveSelectedInvalidRepository => _instance.Value._removeSelectedInvalidRepository.Text;
        public static string RemoveAllInvalidRepositories => _instance.Value._removeAllInvalidRepositories.Text;
        public static string Open => _instance.Value._open.Text;
        public static string DirectoryInvalidRepository => _instance.Value._directoryIsNotAValidRepository.Text;

        public static string ShowDiffForAllParentsText => _instance.Value._showDiffForAllParentsText.Text;
        public static string ShowDiffForAllParentsTooltip => _instance.Value._showDiffForAllParentsTooltip.Text;
    }
}
