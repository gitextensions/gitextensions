using GitCommands;
using ResourceManager;

namespace GitUI
{
    internal sealed class TranslatedStrings : Translate
    {
        private readonly TranslationString _error = new("Error");
        private readonly TranslationString _warning = new("Warning");
        private readonly TranslationString _yes = new("Yes");
        private readonly TranslationString _no = new("No");
        private readonly TranslationString _okText = new("OK");
        private readonly TranslationString _cancelText = new("Cancel");
        private readonly TranslationString _closeText = new("Close");
        private readonly TranslationString _dontShowAgain = new("&Don't show me this message again");

        private readonly TranslationString _buttonCheckoutBranch = new("Checkout branch");
        private readonly TranslationString _buttonContinue = new("Continue");
        private readonly TranslationString _buttonCloseApp = new("Close application");
        private readonly TranslationString _buttonCreateBranch = new("Create branch");
        private readonly TranslationString _buttonIgnore = new("Ignore");
        private readonly TranslationString _buttonPush = new("&Push");
        private readonly TranslationString _buttonReportBug = new("Report bug!");
        private readonly TranslationString _buttonViewDetails = new("View details");

        private readonly TranslationString _containedInCurrentCommitText = new("'{0}' is contained in the currently selected commit");
        private readonly TranslationString _containedInBranchesText = new("Contained in branches:");
        private readonly TranslationString _containedInNoBranchText = new("Contained in no branch");
        private readonly TranslationString _containedInTagsText = new("Contained in tags:");
        private readonly TranslationString _containedInNoTagText = new("Contained in no tag");
        private readonly TranslationString _invisibleCommitText = new("'{0}' is not currently visible");
        private readonly TranslationString _viewPullRequest = new("View pull requests");
        private readonly TranslationString _createPullRequest = new("Create pull request");
        private readonly TranslationString _forkCloneRepo = new("Fork or clone a repository");
        private readonly TranslationString _branchText = new("Branch");
        private readonly TranslationString _branchesText = new("Branches");
        private readonly TranslationString _remotesText = new("Remotes");
        private readonly TranslationString _tagsText = new("Tags");
        private readonly TranslationString _stashesText = new("Stashes");
        private readonly TranslationString _submodulesText = new("Submodules");
        private readonly TranslationString _bodyNotLoaded = new("\n\nFull message text is not present in older commits.\nSelect this commit to populate the full message.");
        private readonly TranslationString _searchingFor = new("Searching for: ");
        private readonly TranslationString _loadingDataText = new("Loading data...");
        private readonly TranslationString _uninterestingDiffOmitted = new("Uninteresting diff hunks are omitted.");
        private readonly TranslationString _openReport = new("Open report");
        private readonly TranslationString _noResultsFound = new("<No results found>");
        private readonly TranslationString _local = new("Local");
        private readonly TranslationString _tag = new("Tag");
        private readonly TranslationString _remote = new("Remote");
        private readonly TranslationString _openWithGitExtensions = new("&Open with Git Extensions");
        private readonly TranslationString _openInVisualStudio = new("Open in &Visual Studio");
        private readonly TranslationString _contScrollToNextFileOnlyWithAlt = new("Enable automatic continuous scroll (without ALT button)");
        private readonly TranslationString _noRevision = new("No revision");

        private readonly TranslationString _authored = new("authored");
        private readonly TranslationString _committed = new("committed");
        private readonly TranslationString _authoredAndCommitted = new("authored and committed");
        private readonly TranslationString _markBisectAsGood = new("Marked as good in bisect");
        private readonly TranslationString _markBisectAsBad = new("Marked as bad in bisect");

        private readonly TranslationString _errorCaptionFailedDeleteFile = new("Failed to delete file");
        private readonly TranslationString _errorCaptionFailedDeleteFolder = new("Failed to delete directory");
        private readonly TranslationString _errorCaptionNotOnBranch = new("Not on a branch");

        private readonly TranslationString _mainInstructionNotOnBranch = new("You are not working on a branch");

        private readonly TranslationString _noBranch = new("no branch");
        private readonly TranslationString _removeSelectedInvalidRepository = new("Remove the selected invalid repository");
        private readonly TranslationString _removeAllInvalidRepositories = new("Remove all {0} invalid repositories");
        private readonly TranslationString _open = new("Open");
        private readonly TranslationString _directoryIsNotAValidRepository = new("The selected item is not a valid git repository.");

        private readonly TranslationString _sortBy = new("&Sort by...");
        private readonly TranslationString _sortOrder = new("&Sort order...");

        private readonly TranslationString _diffSelectedWithRememberedFile = new("&Diff with \"{0}\"");
        private readonly TranslationString _diffWithParent = new("Diff with A ");
        private readonly TranslationString _diffBaseWith = new("Diff BASE with");
        private readonly TranslationString _diffRange = new("Range diff");
        private readonly TranslationString _combinedDiff = new("Combined diff");

        private readonly TranslationString _showDiffForAllParentsText = new("Show file differences for all parents in browse dialog");
        private readonly TranslationString _showDiffForAllParentsTooltip = new(@"Show all differences between the selected commits, not limiting to only one difference.

- For a single selected commit, show the difference with its parent commit.
- For a single selected merge commit, show the difference with all parents.
- For two selected commits with a common ancestor (BASE), show the difference
between the commits as well as the difference from BASE to the selected commits.
See documentation for more details about icons and range diffs.
- For multiple selected commits (up to four), show the difference for
all the first selected with the last selected commit.
- For more than four selected commits, show the difference from the first to
the last selected commit.");

        private readonly TranslationString _stageSelectedLines = new("Stage selected line(s)");
        private readonly TranslationString _unstageSelectedLines = new("Unstage selected line(s)");
        private readonly TranslationString _resetSelectedLines = new("Reset selected line(s)");
        private readonly TranslationString _resetSelectedLinesConfirmation = new("Are you sure you want to reset the changes to the selected lines?");
        private readonly TranslationString _resetChangesCaption = new("Reset changes");

        private readonly TranslationString _rotInactive = new("[ Inactive ]");

        private readonly TranslationString _argumentsText = new("Arguments");
        private readonly TranslationString _commandText = new("Command");
        private readonly TranslationString _exitCodeText = new("Exit code");
        private readonly TranslationString _workingDirectoryText = new("Working directory");
        private readonly TranslationString _reportBugText = new("If you think this was caused by Git Extensions, you can report a bug for the team to investigate.");

        private readonly TranslationString _filterFileInGrid = new("Filter file in &grid");
        private readonly TranslationString _openInVisualStudioFailureText = new("Could not find this file in any open solution. Ensure you have a project containing this file open before trying again.");
        private readonly TranslationString _openInVisualStudioFailureCaption = new("Unable to open file");

        private readonly TranslationString _remoteInError = new("{0}\n\nRemote: {1}");

        private readonly TranslationString _noChanges = new("No changes");

        private readonly TranslationString _settingsTypeToFind = new("Type to find");

        // FormRevisionFilter
        private readonly TranslationString _since = new("Since");
        private readonly TranslationString _until = new("Until");
        private readonly TranslationString _author = new("Author");
        private readonly TranslationString _committer = new("Committer");
        private readonly TranslationString _message = new("Message");
        private readonly TranslationString _diffContent = new("Diff contains");
        private readonly TranslationString _pathFilter = new("Path filter");
        private readonly TranslationString _showOnlyFirstParent = new("Show only first parent");
        private readonly TranslationString _showReflog = new("Show reflog");
        private readonly TranslationString _showReflogTooltip = new("Show all reflog references");
        private readonly TranslationString _showCurrentBranchOnly = new("Show current branch only");
        private readonly TranslationString _simplifyByDecoration = new("Simplify by decoration");

        private readonly TranslationString _stashDropConfirmTitle = new("Drop Stash Confirmation");
        private readonly TranslationString _cannotBeUndone = new("This action cannot be undone.");
        private readonly TranslationString _areYouSure = new("Are you sure you want to drop the stash? This action cannot be undone.");

        private readonly TranslationString _errorPuTTYNotFound = new("SSH agent could not be found");
        private readonly TranslationString _errorSshKeyNotFound = new("SSH key file could not be found");
        private readonly TranslationString _errorSshPuTTYInstalled = new("Is PuTTY installed?");
        private readonly TranslationString _errorSshPuTTYNotConfigured = new("PuTTY is not configured as SSH client");
        private readonly TranslationString _errorSshPuTTYWhereConfigure = new("SSH client can be configured in Settings > SSH.");

        // Dubious ownership popup
        private readonly TranslationString _gitSecurityError = new("Git security error");
        private readonly TranslationString _gitDubiousOwnershipHeader = new("Do you want to trust this repository by adding a security exception?");
        private readonly TranslationString _gitDubiousOwnershipText = new(@"Git detected a security problem that prevents opening the repository.
Git-tracked directories are considered unsafe if they are owned by someone other than the current user.

To be able to open this repository, you need to either:
- add a security exception for the repository to make git trust it, or
- correct the ownership of the repository.");
        private readonly TranslationString _gitDubiousOwnershipTrustRepository = new("Trust this repository");
        private readonly TranslationString _gitDubiousOwnershipTrustAllRepositories = new("Trust all repositories");
        private readonly TranslationString _gitDubiousOwnershipOpenRepositoryFolder = new("Open repository in Explorer");
        private readonly TranslationString _gitDubiousOwnershipSeeGitCommandOutput = new("See git command output...");
        private readonly TranslationString _gitDubiousOwnershipHideGitCommandOutput = new("Hide git command output...");
        private readonly TranslationString _gitDubiousOwnershipTrustAllInstruction = new(@"Git-tracked directories are considered unsafe if they are owned by someone other than the current user.
By default, Git will refuse to even parse a Git config of a repository owned by someone else, let alone
run its hooks, and this config setting allows users to specify exceptions, e.g. for intentionally shared
repositories.

If you wish to trust all git repositories on your system even if they are owned by someone else, run the
following command.

!!! THIS CAN BE DANGEROUS !!!");

        // public only because of FormTranslate
        public TranslatedStrings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static Lazy<TranslatedStrings> _instance = new();

        public static void Reinitialize()
        {
            if (_instance.IsValueCreated)
            {
                _instance = new();
            }
        }

        public static string Error => _instance.Value._error.Text;
        public static string Warning => _instance.Value._warning.Text;
        public static string Yes => _instance.Value._yes.Text;
        public static string No => _instance.Value._no.Text;
        public static string OK => _instance.Value._okText.Text;
        public static string Cancel => _instance.Value._cancelText.Text;
        public static string Close => _instance.Value._closeText.Text;
        public static string DontShowAgain => _instance.Value._dontShowAgain.Text;

        public static string ButtonContinue => _instance.Value._buttonContinue.Text;
        public static string ButtonCheckoutBranch => _instance.Value._buttonCheckoutBranch.Text;
        public static string ButtonCloseApp => _instance.Value._buttonCloseApp.Text;
        public static string ButtonCreateBranch => _instance.Value._buttonCreateBranch.Text;
        public static string ButtonIgnore => _instance.Value._buttonIgnore.Text;
        public static string ButtonPush => _instance.Value._buttonPush.Text;
        public static string ButtonReportBug => _instance.Value._buttonReportBug.Text;
        public static string ButtonViewDetails => _instance.Value._buttonViewDetails.Text;

        public static string ContainedInCurrentCommit => _instance.Value._containedInCurrentCommitText.Text;
        public static string ContainedInBranches => _instance.Value._containedInBranchesText.Text;
        public static string ContainedInNoBranch => _instance.Value._containedInNoBranchText.Text;
        public static string ContainedInTags => _instance.Value._containedInTagsText.Text;
        public static string ContainedInNoTag => _instance.Value._containedInNoTagText.Text;
        public static string InvisibleCommit => _instance.Value._invisibleCommitText.Text;

        public static string CreatePullRequest => _instance.Value._createPullRequest.Text;
        public static string ForkCloneRepo => _instance.Value._forkCloneRepo.Text;
        public static string ViewPullRequest => _instance.Value._viewPullRequest.Text;

        public static string Branch => _instance.Value._branchText.Text;
        public static string Branches => _instance.Value._branchesText.Text;
        public static string Remotes => _instance.Value._remotesText.Text;
        public static string Tags => _instance.Value._tagsText.Text;
        public static string Stashes => _instance.Value._stashesText.Text;
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
        public static string FilterFileInGrid => _instance.Value._filterFileInGrid.Text;
        public static string OpenInVisualStudio => _instance.Value._openInVisualStudio.Text;
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
        public static string ErrorCaptionNotOnBranch => _instance.Value._errorCaptionNotOnBranch.Text;

        public static string ErrorFileNotFound => _instance.Value._errorPuTTYNotFound.Text;
        public static string ErrorSshKeyNotFound => _instance.Value._errorSshKeyNotFound.Text;
        public static string ErrorSshPuTTYInstalled => _instance.Value._errorSshPuTTYInstalled.Text;
        public static string ErrorSshPuTTYNotConfigured => _instance.Value._errorSshPuTTYNotConfigured.Text;
        public static string ErrorSshPuTTYWhereConfigure => _instance.Value._errorSshPuTTYWhereConfigure.Text;

        public static string ErrorInstructionNotOnBranch => _instance.Value._mainInstructionNotOnBranch.Text;

        public static string NoBranch => _instance.Value._noBranch.Text;
        public static string RemoveSelectedInvalidRepository => _instance.Value._removeSelectedInvalidRepository.Text;
        public static string RemoveAllInvalidRepositories => _instance.Value._removeAllInvalidRepositories.Text;
        public static string Open => _instance.Value._open.Text;
        public static string DirectoryInvalidRepository => _instance.Value._directoryIsNotAValidRepository.Text;

        public static string SortBy => _instance.Value._sortBy.Text;
        public static string SortOrder => _instance.Value._sortOrder.Text;

        public static string DiffSelectedWithRememberedFile => _instance.Value._diffSelectedWithRememberedFile.Text;
        public static string DiffWithParent => _instance.Value._diffWithParent.Text;
        public static string DiffBaseWith => _instance.Value._diffBaseWith.Text;
        public static string DiffRange => _instance.Value._diffRange.Text;
        public static string CombinedDiff => _instance.Value._combinedDiff.Text;
        public static string ShowDiffForAllParentsText => _instance.Value._showDiffForAllParentsText.Text;
        public static string ShowDiffForAllParentsTooltip => _instance.Value._showDiffForAllParentsTooltip.Text;

        public static string StageSelectedLines => _instance.Value._stageSelectedLines.Text;
        public static string UnstageSelectedLines => _instance.Value._unstageSelectedLines.Text;
        public static string ResetSelectedLines => _instance.Value._resetSelectedLines.Text;
        public static string ResetSelectedLinesConfirmation => _instance.Value._resetSelectedLinesConfirmation.Text;
        public static string ResetChangesCaption => _instance.Value._resetChangesCaption.Text;

        public static string Inactive => _instance.Value._rotInactive.Text;

        public static string Arguments => _instance.Value._argumentsText.Text;
        public static string Command => _instance.Value._commandText.Text;
        public static string ExitCode => _instance.Value._exitCodeText.Text;
        public static string WorkingDirectory => _instance.Value._workingDirectoryText.Text;
        public static string ReportBug => _instance.Value._reportBugText.Text;

        public static string OpenInVisualStudioFailureText => _instance.Value._openInVisualStudioFailureText.Text;
        public static string OpenInVisualStudioFailureCaption => _instance.Value._openInVisualStudioFailureCaption.Text;
        public static string RemoteInError => _instance.Value._remoteInError.Text;
        public static string NoChanges => _instance.Value._noChanges.Text;

        public static string SettingsTypeToFind => _instance.Value._settingsTypeToFind.Text;

        public static string Since = _instance.Value._since.Text;
        public static string Until = _instance.Value._until.Text;
        public static string Author = _instance.Value._author.Text;
        public static string Committer = _instance.Value._committer.Text;
        public static string Message = _instance.Value._message.Text;
        public static string DiffContent = _instance.Value._diffContent.Text;
        public static string PathFilter = _instance.Value._pathFilter.Text;
        public static string ShowOnlyFirstParent = _instance.Value._showOnlyFirstParent.Text;
        public static string ShowReflog = _instance.Value._showReflog.Text;
        public static string ShowReflogTooltip = _instance.Value._showReflogTooltip.Text;
        public static string ShowCurrentBranchOnly = _instance.Value._showCurrentBranchOnly.Text;
        public static string SimplifyByDecoration = _instance.Value._simplifyByDecoration.Text;

        #region Scripts

        private readonly TranslationString _scriptConfirmExecuteText = new("Do you want to execute script");
        private readonly TranslationString _scriptErrorCantFindText = new("Unable to find script");
        private readonly TranslationString _scriptErrorFailedToExecuteText = new("Failed to execute script");
        private readonly TranslationString _scriptErrorOptionWithoutRevisionGridText = new("option is only supported when invoked from the revision grid");
        private readonly TranslationString _scriptErrorOptionWithoutRevisionText = new("A valid revision is required to substitute the argument options");
        private readonly TranslationString _scriptText = new("Script");

        public static string ScriptConfirmExecute => _instance.Value._scriptConfirmExecuteText.Text;
        public static string ScriptErrorCantFind => _instance.Value._scriptErrorCantFindText.Text;
        public static string ScriptErrorFailedToExecute => _instance.Value._scriptErrorFailedToExecuteText.Text;
        public static string ScriptErrorOptionWithoutRevisionGridText => _instance.Value._scriptErrorOptionWithoutRevisionGridText.Text;
        public static string ScriptErrorOptionWithoutRevisionText => _instance.Value._scriptErrorOptionWithoutRevisionText.Text;
        public static string ScriptText => _instance.Value._scriptText.Text;

        #endregion

        public static string StashDropConfirmTitle => _instance.Value._stashDropConfirmTitle.Text;
        public static string CannotBeUndone => _instance.Value._cannotBeUndone.Text;
        public static string AreYouSure => _instance.Value._areYouSure.Text;

        public static string GitSecurityError => _instance.Value._gitSecurityError.Text;
        public static string GitDubiousOwnershipHeader => _instance.Value._gitDubiousOwnershipHeader.Text;
        public static string GitDubiousOwnershipText => _instance.Value._gitDubiousOwnershipText.Text;
        public static string GitDubiousOwnershipTrustRepository => _instance.Value._gitDubiousOwnershipTrustRepository.Text;
        public static string GitDubiousOwnershipTrustAllRepositories => _instance.Value._gitDubiousOwnershipTrustAllRepositories.Text;
        public static string GitDubiousOwnershipOpenRepositoryFolder => _instance.Value._gitDubiousOwnershipOpenRepositoryFolder.Text;
        public static string GitDubiousOwnershipSeeGitCommandOutput => _instance.Value._gitDubiousOwnershipSeeGitCommandOutput.Text;
        public static string GitDubiousOwnershipHideGitCommandOutput => _instance.Value._gitDubiousOwnershipHideGitCommandOutput.Text;
        public static string GitDubiousOwnershipTrustAllInstruction => _instance.Value._gitDubiousOwnershipTrustAllInstruction.Text;
    }
}
