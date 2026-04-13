// Internal callers still use the legacy Get/Set methods for properties that can't yet be converted to ISetting<T>.
#pragma warning disable CS0618 // Type or member is obsolete

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.Win32;

namespace GitCommands;

public static partial class AppSettings
{
    // semi-constants
    public static Version AppVersion => Assembly.GetCallingAssembly().GetName().Version!;
    public static string ProductVersion => Application.ProductVersion;
    public static readonly string ApplicationName = "Git Extensions";
    public static readonly string ApplicationId = ApplicationName.Replace(" ", "");
    public static readonly string SettingsFileName = ApplicationId + ".settings";
    public static readonly string UserPluginsDirectoryName = "UserPlugins";
    private static string _applicationExecutablePath = Application.ExecutablePath;
    private static string? _documentationBaseUrl;

    public static Lazy<string?> ApplicationDataPath { get; private set; }
    public static readonly Lazy<string?> LocalApplicationDataPath;
    public static string SettingsFilePath => Path.Join(ApplicationDataPath.Value!, SettingsFileName);
    public static string UserPluginsPath => Path.Join(LocalApplicationDataPath.Value!, UserPluginsDirectoryName);

    public static DistributedSettings SettingsContainer { get; private set; }

    private static readonly SettingsPath AppearanceSettingsPath = new AppSettingsPath("Appearance");
    private static readonly SettingsPath ConfirmationsSettingsPath = new AppSettingsPath("Confirmations");
    private static readonly SettingsPath DetailedSettingsPath = new AppSettingsPath("Detailed");
    private static readonly SettingsPath DialogSettingsPath = new AppSettingsPath("Dialogs");
    private static readonly SettingsPath ExperimentalSettingsPath = new AppSettingsPath(DetailedSettingsPath, "Experimental");
    private static readonly SettingsPath FileStatusSettingsPath = new AppSettingsPath(AppearanceSettingsPath, "FileStatus");
    private static readonly SettingsPath RevisionGraphSettingsPath = new AppSettingsPath(AppearanceSettingsPath, "RevisionGraph");
    private static readonly SettingsPath RecentRepositories = new AppSettingsPath("RecentRepositories");
    private static readonly SettingsPath RootSettingsPath = new AppSettingsPath(pathName: "");
    private static readonly SettingsPath HiddenSettingsPath = new AppSettingsPath("Hidden");
    private static readonly SettingsPath MigrationSettingsPath = new AppSettingsPath(HiddenSettingsPath, "Migration");

    #region Setting declarations
    private static readonly ISetting<bool?> _telemetryEnabled = Setting.CreateNullableBool(RootSettingsPath, "TelemetryEnabled");

    private static readonly ISetting<bool> _autoNormaliseBranchName = Setting.CreateBool(RootSettingsPath, "AutoNormaliseBranchName", defaultValue: true);

    private static readonly ISetting<bool> _rememberAmendCommitState = Setting.CreateBool(RootSettingsPath, "RememberAmendCommitState", defaultValue: true);

    private static readonly ISetting<int> _fileStatusFindInFilesGitGrepTypeIndex = Setting.CreateInt(FileStatusSettingsPath, nameof(FileStatusFindInFilesGitGrepTypeIndex), defaultValue: 1);

    private static readonly ISetting<bool> _fileStatusMergeSingleItemWithFolder = Setting.CreateBool(FileStatusSettingsPath, nameof(FileStatusMergeSingleItemWithFolder), defaultValue: false);

    private static readonly ISetting<bool> _fileStatusShowGroupNodesInFlatList = Setting.CreateBool(FileStatusSettingsPath, nameof(FileStatusShowGroupNodesInFlatList), defaultValue: false);

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<bool> _wslGitEnabled = Setting.CreateBool(RootSettingsPath, "WslGitEnabled", defaultValue: true);

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<string> _wslCommand = Setting.Create(RootSettingsPath, nameof(WslCommand), defaultValue: "wsl");

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<string> _wslGitCommand = Setting.Create(RootSettingsPath, nameof(WslGitCommand), defaultValue: "git");

    private static readonly ISetting<bool> _stashKeepIndex = Setting.CreateBool(RootSettingsPath, "stashkeepindex", defaultValue: false);

    // History Compatibility: The setting was originally called 'StashConfirmDropShow', and then it was inverted.
    // To maintain compat with existing user settings, negate the stored value.
    private static readonly ISetting<bool> _dontConfirmStashDrop = Setting.Create(
        RootSettingsPath,
        "stashconfirmdropshow",
        defaultValue: false,
        read: static s => (true, s != "True"),
        store: static v => v ? "False" : "True");

    private static readonly ISetting<bool> _applyPatchIgnoreWhitespace = Setting.CreateBool(RootSettingsPath, "applypatchignorewhitespace", defaultValue: false);

    private static readonly ISetting<bool> _applyPatchSignOff = Setting.CreateBool(RootSettingsPath, "applypatchsignoff", defaultValue: true);

    // History Compatibility: The settings key has patience in the name for historical reasons
    private static readonly ISetting<bool> _useHistogramDiffAlgorithm = Setting.CreateBool(RootSettingsPath, "usepatiencediffalgorithm", defaultValue: false);

    /// <summary>
    /// Use Git coloring for selected commands
    /// </summary>
    private static readonly ISetting<bool> _useGitColoring = Setting.CreateBool(AppearanceSettingsPath, nameof(UseGitColoring), defaultValue: true);

    /// <summary>
    /// Color the background at changes (invert colors).
    /// </summary>
    private static readonly ISetting<bool> _reverseGitColoring = Setting.CreateBool(AppearanceSettingsPath, nameof(ReverseGitColoring), defaultValue: true);

    private static readonly ISetting<bool> _showErrorsWhenStagingFiles = Setting.CreateBool(RootSettingsPath, "showerrorswhenstagingfiles", defaultValue: true);

    private static readonly ISetting<bool> _ensureCommitMessageSecondLineEmpty = Setting.CreateBool(RootSettingsPath, "addnewlinetocommitmessagewhenmissing", defaultValue: true);

    private static readonly ISetting<string> _lastCommitMessage = Setting.Create(RootSettingsPath, "lastCommitMessage", defaultValue: "");

    private static readonly ISetting<int> _commitDialogNumberOfPreviousMessages = Setting.CreateInt(RootSettingsPath, "commitDialogNumberOfPreviousMessages", defaultValue: 6);

    private static readonly ISetting<bool> _commitDialogSelectStagedOnEnterMessage = Setting.CreateBool(DialogSettingsPath, nameof(CommitDialogSelectStagedOnEnterMessage), defaultValue: true);

    private static readonly ISetting<bool> _commitDialogShowOnlyMyMessages = Setting.CreateBool(RootSettingsPath, "commitDialogShowOnlyMyMessages", defaultValue: false);

    private static readonly ISetting<bool> _showCommitAndPush = Setting.CreateBool(RootSettingsPath, "showcommitandpush", defaultValue: true);

    private static readonly ISetting<bool> _showResetWorkTreeChanges = Setting.CreateBool(RootSettingsPath, "showresetunstagedchanges", defaultValue: true);

    private static readonly ISetting<bool> _showResetAllChanges = Setting.CreateBool(RootSettingsPath, "showresetallchanges", defaultValue: true);

    private static readonly ISetting<bool> _showConEmuTab = Setting.CreateBool(DetailedSettingsPath, nameof(ShowConEmuTab), defaultValue: true);

    private static readonly ISetting<string> _conEmuStyle = Setting.Create(DetailedSettingsPath, nameof(ConEmuStyle), defaultValue: ConEmuStyleDefault);

    private static readonly ISetting<string> _conEmuTerminal = Setting.Create(DetailedSettingsPath, nameof(ConEmuTerminal), defaultValue: "bash");

    private static readonly ISetting<int> _outputHistoryDepth = Setting.CreateInt(DetailedSettingsPath, nameof(OutputHistoryDepth), defaultValue: 20);

    private static readonly ISetting<bool> _outputHistoryPanelVisible = Setting.CreateBool(DetailedSettingsPath, nameof(OutputHistoryPanelVisible), defaultValue: false);

    private static readonly ISetting<bool> _showOutputHistoryAsTab = Setting.CreateBool(DetailedSettingsPath, nameof(ShowOutputHistoryAsTab), defaultValue: true);

    private static readonly ISetting<bool> _useBrowseForFileHistory = Setting.CreateBool(DetailedSettingsPath, nameof(UseBrowseForFileHistory), defaultValue: true);

    private static readonly ISetting<bool> _useDiffViewerForBlame = Setting.CreateBool(DetailedSettingsPath, nameof(UseDiffViewerForBlame), defaultValue: false);

    private static readonly ISetting<bool> _showGpgInformation = Setting.CreateBool(DetailedSettingsPath, nameof(ShowGpgInformation), defaultValue: true);

    private static readonly ISetting<bool> _messageEditorWordWrap = Setting.CreateBool(DetailedSettingsPath, nameof(MessageEditorWordWrap), defaultValue: false);

    private static readonly ISetting<bool> _showSplitViewLayout = Setting.CreateBool(DetailedSettingsPath, "ShowSplitViewLayout", defaultValue: true);

    private static readonly ISetting<bool> _provideAutocompletion = Setting.CreateBool(RootSettingsPath, "provideautocompletion", defaultValue: true);

    private static readonly ISetting<TruncatePathMethod> _truncatePathMethod = Setting.CreateEnum(RootSettingsPath, "truncatepathmethod", defaultValue: TruncatePathMethod.None);

    private static readonly ISetting<bool> _showGitStatusInBrowseToolbar = Setting.CreateBool(RootSettingsPath, "showgitstatusinbrowsetoolbar", defaultValue: true);

    private static readonly ISetting<bool> _showGitStatusForArtificialCommits = Setting.CreateBool(RootSettingsPath, "showgitstatusforartificialcommits", defaultValue: true);

    private static readonly ISetting<bool> _commitInfoShowContainedInBranchesLocal = Setting.CreateBool(RootSettingsPath, "commitinfoshowcontainedinbrancheslocal", defaultValue: true);

    private static readonly ISetting<bool> _checkForUncommittedChangesInCheckoutBranch = Setting.CreateBool(RootSettingsPath, "checkforuncommittedchangesincheckoutbranch", defaultValue: true);

    private static readonly ISetting<bool> _alwaysShowCheckoutBranchDlg = Setting.CreateBool(RootSettingsPath, "AlwaysShowCheckoutBranchDlg", defaultValue: false);

    private static readonly ISetting<bool> _commitAndPushForcedWhenAmend = Setting.CreateBool(RootSettingsPath, "CommitAndPushForcedWhenAmend", defaultValue: false);

    private static readonly ISetting<bool> _commitInfoShowContainedInBranchesRemote = Setting.CreateBool(RootSettingsPath, "commitinfoshowcontainedinbranchesremote", defaultValue: false);

    private static readonly ISetting<bool> _commitInfoShowContainedInBranchesRemoteIfNoLocal = Setting.CreateBool(RootSettingsPath, "commitinfoshowcontainedinbranchesremoteifnolocal", defaultValue: false);

    private static readonly ISetting<bool> _commitInfoShowContainedInTags = Setting.CreateBool(RootSettingsPath, "commitinfoshowcontainedintags", defaultValue: true);

    private static readonly ISetting<bool> _commitInfoShowTagThisCommitDerivesFrom = Setting.CreateBool(RootSettingsPath, "commitinfoshowtagthiscommitderivesfrom", defaultValue: true);

    private static readonly ISetting<string> _customAvatarTemplate = Setting.Create(RootSettingsPath, "CustomAvatarTemplate", defaultValue: string.Empty);

    private static readonly ISetting<int> _avatarImageCacheDays = Setting.CreateInt(RootSettingsPath, "authorimagecachedays", defaultValue: 13);

    private static readonly ISetting<bool> _showAuthorAvatarInCommitInfo = Setting.CreateBool(RootSettingsPath, "showauthorgravatar", defaultValue: true);

    private static readonly ISetting<int> _avatarCacheSize = Setting.CreateInt(RootSettingsPath, "Appearance.AvatarCacheSize", defaultValue: 200);

    // Currently not configurable in UI (Set manually in settings file)
    // Names from here: https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.brushes?view=windowsdesktop-7.0
    // or #AARRGGBB code
    private static readonly ISetting<string> _avatarAuthorInitialsPalette = Setting.Create(RootSettingsPath, "Appearance.AvatarAuthorInitialsPalette", defaultValue: "SlateGray,RoyalBlue,Purple,OrangeRed,Teal,OliveDrab");

    private static readonly ISetting<string> _translation = Setting.Create(RootSettingsPath, "translation", defaultValue: "");

    private static readonly ISetting<bool> _userProfileHomeDir = Setting.CreateBool(RootSettingsPath, "userprofilehomedir", defaultValue: false);

    private static readonly ISetting<string> _customHomeDir = Setting.Create(RootSettingsPath, "customhomedir", defaultValue: "");

    private static readonly ISetting<bool> _enableAutoScale = Setting.CreateBool(RootSettingsPath, "enableautoscale", defaultValue: true);

    private static readonly ISetting<bool> _closeCommitDialogAfterCommit = Setting.CreateBool(RootSettingsPath, "closecommitdialogaftercommit", defaultValue: true);

    private static readonly ISetting<bool> _closeCommitDialogAfterLastCommit = Setting.CreateBool(RootSettingsPath, "closecommitdialogafterlastcommit", defaultValue: true);

    private static readonly ISetting<bool> _refreshArtificialCommitOnApplicationActivated = Setting.CreateBool(RootSettingsPath, "refreshcommitdialogonformfocus", defaultValue: false);

    private static readonly ISetting<bool> _stageInSuperprojectAfterCommit = Setting.CreateBool(RootSettingsPath, "stageinsuperprojectaftercommit", defaultValue: true);

    private static readonly ISetting<bool> _followRenamesInFileHistory = Setting.CreateBool(RootSettingsPath, "followrenamesinfilehistory", defaultValue: true);

    private static readonly ISetting<bool> _followRenamesInFileHistoryExactOnly = Setting.CreateBool(RootSettingsPath, "followrenamesinfilehistoryexactonly", defaultValue: false);

    private static readonly ISetting<bool> _fullHistoryInFileHistory = Setting.CreateBool(RootSettingsPath, "fullhistoryinfilehistory", defaultValue: false);

    private static readonly ISetting<bool> _simplifyMergesInFileHistory = Setting.CreateBool(RootSettingsPath, "simplifymergesinfileHistory", defaultValue: false);

    private static readonly ISetting<bool> _loadFileHistoryOnShow = Setting.CreateBool(RootSettingsPath, "LoadFileHistoryOnShow", defaultValue: true);

    private static readonly ISetting<bool> _loadBlameOnShow = Setting.CreateBool(RootSettingsPath, "LoadBlameOnShow", defaultValue: true);

    private static readonly ISetting<bool> _detectCopyInFileOnBlame = Setting.CreateBool(RootSettingsPath, "DetectCopyInFileOnBlame", defaultValue: false);

    private static readonly ISetting<bool> _detectCopyInAllOnBlame = Setting.CreateBool(RootSettingsPath, "DetectCopyInAllOnBlame", defaultValue: false);

    private static readonly ISetting<bool> _ignoreWhitespaceOnBlame = Setting.CreateBool(RootSettingsPath, "IgnoreWhitespaceOnBlame", defaultValue: true);

    private static readonly ISetting<bool> _openSubmoduleDiffInSeparateWindow = Setting.CreateBool(RootSettingsPath, "opensubmodulediffinseparatewindow", defaultValue: false);

    /// <summary>
    /// Gets or sets whether to show artificial commits in the revision graph.
    /// </summary>
    private static readonly ISetting<bool> _revisionGraphShowArtificialCommits = Setting.CreateBool(RootSettingsPath, "revisiongraphshowworkingdirchanges", defaultValue: true);

    private static readonly ISetting<bool> _revisionGraphDrawAlternateBackColor = Setting.CreateBool(RootSettingsPath, "RevisionGraphDrawAlternateBackColor", defaultValue: true);

    private static readonly ISetting<bool> _revisionGraphDrawNonRelativesGray = Setting.CreateBool(RootSettingsPath, "revisiongraphdrawnonrelativesgray", defaultValue: true);

    private static readonly ISetting<bool> _revisionGraphDrawNonRelativesTextGray = Setting.CreateBool(RootSettingsPath, "revisiongraphdrawnonrelativestextgray", defaultValue: false);

    /// <summary>
    /// Gets or sets the default pull action that is performed by the toolbar icon when it is clicked on.
    /// </summary>
    private static readonly ISetting<GitPullAction> _defaultPullAction = Setting.CreateEnum(RootSettingsPath, "DefaultPullAction", defaultValue: GitPullAction.Merge);

    /// <summary>
    /// Gets or sets the default pull action as configured in the FormPull dialog.
    /// </summary>
    private static readonly ISetting<GitPullAction> _formPullAction = Setting.CreateEnum(RootSettingsPath, "FormPullAction", defaultValue: GitPullAction.Merge);

    private static readonly ISetting<bool> _autoStash = Setting.CreateBool(RootSettingsPath, "autostash", defaultValue: false);

    private static readonly ISetting<bool> _rebaseAutoStash = Setting.CreateBool(RootSettingsPath, "RebaseAutostash", defaultValue: false);

    private static readonly ISetting<LocalChangesAction> _checkoutBranchAction = Setting.CreateEnum(RootSettingsPath, "checkoutbranchaction", defaultValue: LocalChangesAction.DontChange);

    private static readonly ISetting<bool> _checkoutOtherBranchAfterReset = Setting.CreateBool(DialogSettingsPath, nameof(CheckoutOtherBranchAfterReset), defaultValue: true);

    private static readonly ISetting<bool> _useDefaultCheckoutBranchAction = Setting.CreateBool(RootSettingsPath, "UseDefaultCheckoutBranchAction", defaultValue: false);

    private static readonly ISetting<bool> _dontShowHelpImages = Setting.CreateBool(RootSettingsPath, "DontShowHelpImages", defaultValue: false);

    private static readonly ISetting<bool> _alwaysShowAdvOpt = Setting.CreateBool(RootSettingsPath, "AlwaysShowAdvOpt", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmAmend = Setting.CreateBool(RootSettingsPath, "DontConfirmAmend", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmDeleteUnmergedBranch = Setting.CreateBool(RootSettingsPath, "DontConfirmDeleteUnmergedBranch", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmCommitIfNoBranch = Setting.CreateBool(RootSettingsPath, "DontConfirmCommitIfNoBranch", defaultValue: false);

    private static readonly ISetting<bool> _confirmBranchCheckout = Setting.CreateBool(ConfirmationsSettingsPath, nameof(ConfirmBranchCheckout), defaultValue: false);

    private static readonly ISetting<bool?> _autoPopStashAfterPull = Setting.CreateNullableBool(RootSettingsPath, "AutoPopStashAfterPull");

    private static readonly ISetting<bool?> _autoPopStashAfterCheckoutBranch = Setting.CreateNullableBool(RootSettingsPath, "AutoPopStashAfterCheckoutBranch");

    private static readonly ISetting<bool> _dontConfirmPushNewBranch = Setting.CreateBool(RootSettingsPath, "DontConfirmPushNewBranch", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmAddTrackingRef = Setting.CreateBool(RootSettingsPath, "DontConfirmAddTrackingRef", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmCommitAfterConflictsResolved = Setting.CreateBool(RootSettingsPath, "DontConfirmCommitAfterConflictsResolved", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmSecondAbortConfirmation = Setting.CreateBool(RootSettingsPath, "DontConfirmSecondAbortConfirmation", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmRebase = Setting.CreateBool(RootSettingsPath, "DontConfirmRebase", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmResolveConflicts = Setting.CreateBool(RootSettingsPath, "DontConfirmResolveConflicts", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmUndoLastCommit = Setting.CreateBool(RootSettingsPath, "DontConfirmUndoLastCommit", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmFetchAndPruneAll = Setting.CreateBool(RootSettingsPath, "DontConfirmFetchAndPruneAll", defaultValue: false);

    private static readonly ISetting<bool> _dontConfirmSwitchWorktree = Setting.CreateBool(RootSettingsPath, "DontConfirmSwitchWorktree", defaultValue: false);

    private static readonly ISetting<bool> _includeUntrackedFilesInAutoStash = Setting.CreateBool(RootSettingsPath, "includeUntrackedFilesInAutoStash", defaultValue: false);

    private static readonly ISetting<bool> _includeUntrackedFilesInManualStash = Setting.CreateBool(RootSettingsPath, "includeUntrackedFilesInManualStash", defaultValue: false);

    private static readonly ISetting<bool> _showRemoteBranches = Setting.CreateBool(RootSettingsPath, "showRemoteBranches", defaultValue: true);

    private static readonly ISetting<bool> _showStashes = Setting.CreateBool(RootSettingsPath, "showStashes", defaultValue: true);

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<int> _maxStashesWithUntrackedFiles = Setting.CreateInt(RootSettingsPath, "maxStashesWithUntrackedFiles", defaultValue: 10);

    private static readonly ISetting<bool> _showSuperprojectTags = Setting.CreateBool(RootSettingsPath, "showSuperprojectTags", defaultValue: false);

    private static readonly ISetting<bool> _showSuperprojectBranches = Setting.CreateBool(RootSettingsPath, "showSuperprojectBranches", defaultValue: true);

    private static readonly ISetting<bool> _showSuperprojectRemoteBranches = Setting.CreateBool(RootSettingsPath, "showSuperprojectRemoteBranches", defaultValue: false);

    private static readonly ISetting<bool?> _updateSubmodulesOnCheckout = Setting.CreateNullableBool(RootSettingsPath, "updateSubmodulesOnCheckout");

    private static readonly ISetting<bool?> _dontConfirmUpdateSubmodulesOnCheckout = Setting.CreateNullableBool(RootSettingsPath, "dontConfirmUpdateSubmodulesOnCheckout");

    private static readonly ISetting<bool> _showGitCommandLine = Setting.CreateBool(RootSettingsPath, "showgitcommandline", defaultValue: false);

    private static readonly ISetting<bool> _showStashCount = Setting.CreateBool(RootSettingsPath, "showstashcount", defaultValue: false);

    private static readonly ISetting<bool> _showAheadBehindData = Setting.CreateBool(RootSettingsPath, "showaheadbehinddata", defaultValue: true);

    private static readonly ISetting<bool> _showSubmoduleStatus = Setting.CreateBool(RootSettingsPath, "showsubmodulestatus", defaultValue: false);

    private static readonly ISetting<bool> _relativeDate = Setting.CreateBool(RootSettingsPath, "relativedate", defaultValue: true);

    private static readonly ISetting<bool> _showGitNotes = Setting.CreateBool(RootSettingsPath, "showgitnotes", defaultValue: false);

    private static readonly ISetting<bool> _showSessionRefs = Setting.CreateBool(RootSettingsPath, "showSessionRefs", defaultValue: false);

    private static readonly ISetting<bool> _showGitNotesColumn = Setting.CreateBool(AppearanceSettingsPath, nameof(ShowGitNotesColumn), defaultValue: false);

    private static readonly ISetting<bool> _showAnnotatedTagsMessages = Setting.CreateBool(RootSettingsPath, "showannotatedtagsmessages", defaultValue: true);

    // History Compatibility: The meaning of this value is changed in the GUI, setting name is kept for compatibility
    private static readonly ISetting<bool> _hideMergeCommits = Setting.Create(
        RootSettingsPath,
        "showmergecommits",
        defaultValue: false,
        read: static s => (true, s != "True"),
        store: static v => v ? "False" : "True");

    private static readonly ISetting<bool> _showTags = Setting.CreateBool(RootSettingsPath, "showtags", defaultValue: true);

    private static readonly ISetting<bool> _showRevisionGridGraphColumn = Setting.CreateBool(RootSettingsPath, "showrevisiongridgraphcolumn", defaultValue: true);

    private static readonly ISetting<bool> _showAuthorAvatarColumn = Setting.CreateBool(RootSettingsPath, "showrevisiongridauthoravatarcolumn", defaultValue: true);

    private static readonly ISetting<bool> _showAuthorNameColumn = Setting.CreateBool(RootSettingsPath, "showrevisiongridauthornamecolumn", defaultValue: true);

    private static readonly ISetting<bool> _showDateColumn = Setting.CreateBool(RootSettingsPath, "showrevisiongriddatecolumn", defaultValue: true);

    private static readonly ISetting<bool> _showObjectIdColumn = Setting.CreateBool(RootSettingsPath, "showids", defaultValue: true);

    private static readonly ISetting<bool> _showBuildStatusIconColumn = Setting.CreateBool(RootSettingsPath, "showbuildstatusiconcolumn", defaultValue: true);

    private static readonly ISetting<bool> _showBuildStatusTextColumn = Setting.CreateBool(RootSettingsPath, "showbuildstatustextcolumn", defaultValue: false);

    private static readonly ISetting<bool> _showAuthorDate = Setting.CreateBool(RootSettingsPath, "showauthordate", defaultValue: true);

    private static readonly ISetting<bool> _closeProcessDialog = Setting.CreateBool(RootSettingsPath, "closeprocessdialog", defaultValue: false);

    private static readonly ISetting<bool> _showProcessDialogPasswordInput = Setting.CreateBool(DetailedSettingsPath, nameof(ShowProcessDialogPasswordInput), defaultValue: false);

    private static readonly ISetting<bool> _showSimplifyByDecoration = Setting.CreateBool(RootSettingsPath, "showsimplifybydecoration", defaultValue: false);

    private static readonly ISetting<bool> _showOnlyFirstParent = Setting.CreateBool(RootSettingsPath, "showfirstparent", defaultValue: false);

    private static readonly ISetting<bool> _commitDialogSelectionFilter = Setting.CreateBool(RootSettingsPath, "commitdialogselectionfilter", defaultValue: false);

    private static readonly ISetting<string> _defaultCloneDestinationPath = Setting.Create(RootSettingsPath, "defaultclonedestinationpath", defaultValue: string.Empty);

    private static readonly ISetting<int> _revisionGridQuickSearchTimeout = Setting.CreateInt(RootSettingsPath, "revisiongridquicksearchtimeout", defaultValue: 4000);

    private static readonly ISetting<bool> _showCommitBodyInRevisionGrid = Setting.CreateBool(RootSettingsPath, "ShowCommitBodyInRevisionGrid", defaultValue: true);

    private static readonly ISetting<int> _maxRevisionGraphCommits = Setting.CreateInt(RootSettingsPath, "maxrevisiongraphcommits", defaultValue: 100000);

    private static readonly ISetting<bool> _showDiffForAllParents = Setting.CreateBool(RootSettingsPath, "showdiffforallparents", defaultValue: true);

    private static readonly ISetting<bool> _showFindInCommitFilesGitGrep = Setting.CreateBool(AppearanceSettingsPath, nameof(ShowFindInCommitFilesGitGrep), defaultValue: false);

    private static readonly ISetting<bool> _showRevisionGridTooltips = Setting.CreateBool(AppearanceSettingsPath, nameof(ShowRevisionGridTooltips), defaultValue: true);

    private static readonly ISetting<bool> _showAvailableDiffTools = Setting.CreateBool(RootSettingsPath, "difftools.showavailable", defaultValue: true);

    private static readonly ISetting<int> _diffVerticalRulerPosition = Setting.CreateInt(RootSettingsPath, "diffverticalrulerposition", defaultValue: 0);

    private static readonly ISetting<string> _gitGrepUserArguments = Setting.Create(DialogSettingsPath, nameof(GitGrepUserArguments), defaultValue: "");

    private static readonly ISetting<bool> _gitGrepIgnoreCase = Setting.CreateBool(DialogSettingsPath, nameof(GitGrepIgnoreCase), defaultValue: false);

    private static readonly ISetting<bool> _gitGrepMatchWholeWord = Setting.CreateBool(DialogSettingsPath, nameof(GitGrepMatchWholeWord), defaultValue: false);

    private static readonly ISetting<bool> _startWithRecentWorkingDir = Setting.CreateBool(RootSettingsPath, "StartWithRecentWorkingDir", defaultValue: false);

    private static readonly ISetting<bool> _autoStartPageant = Setting.CreateBool(RootSettingsPath, "autostartpageant", defaultValue: true);

    private static readonly ISetting<bool> _markIllFormedLinesInCommitMsg = Setting.CreateBool(RootSettingsPath, "markillformedlinesincommitmsg", defaultValue: true);

    private static readonly ISetting<bool> _useSystemVisualStyle = Setting.CreateBool(RootSettingsPath, "systemvisualstyle", defaultValue: true);

    private static readonly ISetting<bool> _showEolMarkerAsGlyph = Setting.CreateBool(RootSettingsPath, "ShowEolMarkerAsGlyph", defaultValue: false);

    private static readonly ISetting<bool> _multicolorBranches = Setting.CreateBool(RootSettingsPath, "multicolorbranches", defaultValue: true);

    private static readonly ISetting<bool> _highlightAuthoredRevisions = Setting.CreateBool(RootSettingsPath, "highlightauthoredrevisions", defaultValue: true);

    private static readonly ISetting<bool> _fillRefLabels = Setting.CreateBool(RootSettingsPath, "FillRefLabels", defaultValue: false);

    private static readonly ISetting<bool> _mergeGraphLanesHavingCommonParent = Setting.CreateBool(RevisionGraphSettingsPath, nameof(MergeGraphLanesHavingCommonParent), defaultValue: true);

    private static readonly ISetting<bool> _renderGraphWithDiagonals = Setting.CreateBool(RevisionGraphSettingsPath, nameof(RenderGraphWithDiagonals), defaultValue: true);

    private static readonly ISetting<bool> _straightenGraphDiagonals = Setting.CreateBool(RevisionGraphSettingsPath, nameof(StraightenGraphDiagonals), defaultValue: true);

    /// <summary>
    ///  The limit when to skip the straightening of revision graph segments.
    /// </summary>
    /// <remarks>
    ///  Straightening needs to call the expensive RevisionGraphRow.BuildSegmentLanes function.<br></br>
    ///  Straightening inserts gaps making the graph wider. If it already has to display many segments, i.e. parallel branches, there would be a low benefit of straightening.<br></br>
    ///  So rather skip the - in this case particularly expensive - RevisionGraphRow.BuildSegmentLanes function and call it only if the row is visible.
    /// </remarks>
    private static readonly ISetting<int> _straightenGraphSegmentsLimit = Setting.CreateInt(RevisionGraphSettingsPath, nameof(StraightenGraphSegmentsLimit), defaultValue: 80);

    private static readonly ISetting<string> _lastFormatPatchDir = Setting.Create(RootSettingsPath, "lastformatpatchdir", defaultValue: "");

    private static readonly ISetting<bool> _rememberIgnoreWhiteSpacePreference = Setting.CreateBool(RootSettingsPath, "rememberIgnoreWhiteSpacePreference", defaultValue: true);

    private static readonly ISetting<bool> _rememberShowNonPrintingCharsPreference = Setting.CreateBool(RootSettingsPath, "RememberShowNonPrintableCharsPreference", defaultValue: false);

    private static readonly ISetting<bool> _rememberShowEntireFilePreference = Setting.CreateBool(RootSettingsPath, "RememberShowEntireFilePreference", defaultValue: false);

    /// <summary>
    /// Gets or sets whether to remember the preference for diff appearance.
    /// </summary>
    private static readonly ISetting<bool> _rememberDiffDisplayAppearance = Setting.CreateBool(AppearanceSettingsPath, nameof(RememberDiffDisplayAppearance), defaultValue: false);

    private static readonly ISetting<bool> _rememberNumberOfContextLines = Setting.CreateBool(RootSettingsPath, "RememberNumberOfContextLines", defaultValue: false);

    private static readonly ISetting<bool> _rememberShowSyntaxHighlightingInDiff = Setting.CreateBool(RootSettingsPath, "RememberShowSyntaxHighlightingInDiff", defaultValue: true);

    private static readonly ISetting<bool> _showRepoCurrentBranch = Setting.CreateBool(RootSettingsPath, "dashboardshowcurrentbranch", defaultValue: true);

    private static readonly ISetting<int> _recursiveSubmodules = Setting.CreateInt(RootSettingsPath, "RecursiveSubmodules", defaultValue: 1);

    private static readonly ISetting<ShorteningRecentRepoPathStrategy> _shorteningRecentRepoPathStrategy = Setting.CreateEnum(RootSettingsPath, "ShorteningRecentRepoPathStrategy", defaultValue: ShorteningRecentRepoPathStrategy.None);

    // History Compatibility: Keep original key to maintain the compatibility with the existing user settings
    private static readonly ISetting<int> _maxTopRepositories = Setting.CreateInt(RootSettingsPath, "MaxMostRecentRepositories", defaultValue: 0);

    private static readonly ISetting<int> _recentRepositoriesHistorySize = Setting.CreateInt(RootSettingsPath, "history size", defaultValue: 30);

    private static readonly ISetting<bool> _hideTopRepositoriesFromRecentList = Setting.CreateBool(RecentRepositories, nameof(HideTopRepositoriesFromRecentList), defaultValue: false);

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<int> _remotesCacheLength = Setting.CreateInt(RootSettingsPath, "RemotesCacheLength", defaultValue: 30);

    private static readonly ISetting<int> _recentReposComboMinWidth = Setting.CreateInt(RootSettingsPath, "RecentReposComboMinWidth", defaultValue: 0);

    private static readonly ISetting<bool> _sortTopRepos = Setting.CreateBool(RootSettingsPath, "SortMostRecentRepos", defaultValue: false);

    private static readonly ISetting<bool> _sortRecentRepos = Setting.CreateBool(RootSettingsPath, "SortLessRecentRepos", defaultValue: false);

    private static readonly ISetting<bool> _dontCommitMerge = Setting.CreateBool(RootSettingsPath, "DontCommitMerge", defaultValue: false);

    private static readonly ISetting<int> _commitValidationMaxCntCharsFirstLine = Setting.CreateInt(RootSettingsPath, "CommitValidationMaxCntCharsFirstLine", defaultValue: 0);

    private static readonly ISetting<int> _commitValidationMaxCntCharsPerLine = Setting.CreateInt(RootSettingsPath, "CommitValidationMaxCntCharsPerLine", defaultValue: 0);

    private static readonly ISetting<bool> _commitValidationSecondLineMustBeEmpty = Setting.CreateBool(RootSettingsPath, "CommitValidationSecondLineMustBeEmpty", defaultValue: false);

    private static readonly ISetting<bool> _commitValidationIndentAfterFirstLine = Setting.CreateBool(RootSettingsPath, "CommitValidationIndentAfterFirstLine", defaultValue: true);

    private static readonly ISetting<bool> _commitValidationAutoWrap = Setting.CreateBool(RootSettingsPath, "CommitValidationAutoWrap", defaultValue: true);

    private static readonly ISetting<string> _commitValidationRegEx = Setting.Create(RootSettingsPath, "CommitValidationRegEx", defaultValue: string.Empty);

    private static readonly ISetting<string> _commitTemplates = Setting.Create(RootSettingsPath, "CommitTemplates", defaultValue: string.Empty);

    private static readonly ISetting<bool> _createLocalBranchForRemote = Setting.CreateBool(RootSettingsPath, "CreateLocalBranchForRemote", defaultValue: false);

    private static readonly ISetting<bool> _useFormCommitMessage = Setting.CreateBool(RootSettingsPath, "UseFormCommitMessage", defaultValue: true);

    private static readonly ISetting<bool> _commitAutomaticallyAfterCherryPick = Setting.CreateBool(RootSettingsPath, "CommitAutomaticallyAfterCherryPick", defaultValue: false);

    private static readonly ISetting<bool> _addCommitReferenceToCherryPick = Setting.CreateBool(RootSettingsPath, "AddCommitReferenceToCherryPick", defaultValue: false);

    private static readonly ISetting<bool> _checkForUpdates = Setting.CreateBool(RootSettingsPath, "CheckForUpdates", defaultValue: true);

    private static readonly ISetting<bool> _checkForReleaseCandidates = Setting.CreateBool(RootSettingsPath, "CheckForReleaseCandidates", defaultValue: false);

    private static readonly ISetting<bool> _omitUninterestingDiff = Setting.CreateBool(RootSettingsPath, "OmitUninterestingDiff", defaultValue: false);

    private static readonly ISetting<bool> _useConsoleEmulatorForCommands = Setting.CreateBool(RootSettingsPath, "UseConsoleEmulatorForCommands", defaultValue: true);

    private static readonly ISetting<GitRefsSortBy> _refsSortBy = Setting.CreateEnum(RootSettingsPath, "RefsSortBy", defaultValue: GitRefsSortBy.Default);

    private static readonly ISetting<GitRefsSortOrder> _refsSortOrder = Setting.CreateEnum(RootSettingsPath, "RefsSortOrder", defaultValue: GitRefsSortOrder.Descending);

    private static readonly ISetting<DiffListSortType> _diffListSorting = Setting.CreateEnum(RootSettingsPath, "DiffListSortType", defaultValue: DiffListSortType.FilePath);

    private static readonly ISetting<bool> _repoObjectsTreeShowBranches = Setting.CreateBool(RootSettingsPath, "RepoObjectsTree.ShowBranches", defaultValue: true);

    private static readonly ISetting<bool> _repoObjectsTreeShowRemotes = Setting.CreateBool(RootSettingsPath, "RepoObjectsTree.ShowRemotes", defaultValue: true);

    private static readonly ISetting<bool> _repoObjectsTreeShowTags = Setting.CreateBool(RootSettingsPath, "RepoObjectsTree.ShowTags", defaultValue: true);

    private static readonly ISetting<bool> _repoObjectsTreeShowStashes = Setting.CreateBool(RootSettingsPath, "RepoObjectsTree.ShowStashes", defaultValue: true);

    private static readonly ISetting<bool> _repoObjectsTreeShowSubmodules = Setting.CreateBool(RootSettingsPath, "RepoObjectsTree.ShowSubmodules", defaultValue: true);

    private static readonly ISetting<bool> _repoObjectsTreeShowWorktrees = Setting.CreateBool(RootSettingsPath, "RepoObjectsTree.ShowWorktrees", defaultValue: true);

    private static readonly ISetting<int> _repoObjectsTreeBranchesIndex = Setting.CreateInt(RootSettingsPath, "RepoObjectsTree.BranchesIndex", defaultValue: 0);

    private static readonly ISetting<int> _repoObjectsTreeRemotesIndex = Setting.CreateInt(RootSettingsPath, "RepoObjectsTree.RemotesIndex", defaultValue: 1);

    private static readonly ISetting<int> _repoObjectsTreeWorktreesIndex = Setting.CreateInt(RootSettingsPath, "RepoObjectsTree.WorktreesIndex", defaultValue: 2);

    private static readonly ISetting<int> _repoObjectsTreeTagsIndex = Setting.CreateInt(RootSettingsPath, "RepoObjectsTree.TagsIndex", defaultValue: 3);

    private static readonly ISetting<int> _repoObjectsTreeSubmodulesIndex = Setting.CreateInt(RootSettingsPath, "RepoObjectsTree.SubmodulesIndex", defaultValue: 4);

    private static readonly ISetting<int> _repoObjectsTreeStashesIndex = Setting.CreateInt(RootSettingsPath, "RepoObjectsTree.StashesIndex", defaultValue: 5);

    private static readonly ISetting<string> _prioritizedBranchNames = Setting.Create(RootSettingsPath, "PrioritizedBranchNames", defaultValue: "main[^/]*|master[^/]*|release/.*");

    private static readonly ISetting<string> _prioritizedRemoteNames = Setting.Create(RootSettingsPath, "PrioritizedRemoteNames", defaultValue: "origin|upstream");

    /// <summary>
    ///  Remote names to prefer when auto-detecting build server integration, separated by <c>|</c>.
    ///  Defaults to <c>upstream|origin</c> so that forks resolve to the upstream project's CI.
    /// </summary>
    private static readonly ISetting<string> _prioritizedBuildServerRemoteNames = Setting.Create(RootSettingsPath, "PrioritizedBuildServerRemoteNames", defaultValue: "upstream|origin|remote");

    private static readonly ISetting<bool> _blameDisplayAuthorFirst = Setting.CreateBool(RootSettingsPath, "Blame.DisplayAuthorFirst", defaultValue: false);

    private static readonly ISetting<bool> _blameShowAuthor = Setting.CreateBool(RootSettingsPath, "Blame.ShowAuthor", defaultValue: true);

    private static readonly ISetting<bool> _blameShowAuthorDate = Setting.CreateBool(RootSettingsPath, "Blame.ShowAuthorDate", defaultValue: true);

    private static readonly ISetting<bool> _blameShowAuthorTime = Setting.CreateBool(RootSettingsPath, "Blame.ShowAuthorTime", defaultValue: true);

    private static readonly ISetting<bool> _blameShowLineNumbers = Setting.CreateBool(RootSettingsPath, "Blame.ShowLineNumbers", defaultValue: false);

    private static readonly ISetting<bool> _blameShowOriginalFilePath = Setting.CreateBool(RootSettingsPath, "Blame.ShowOriginalFilePath", defaultValue: true);

    private static readonly ISetting<bool> _blameShowAuthorAvatar = Setting.CreateBool(RootSettingsPath, "Blame.ShowAuthorAvatar", defaultValue: true);

    private static readonly ISetting<bool> _automaticContinuousScroll = Setting.CreateBool(RootSettingsPath, "DiffViewer.AutomaticContinuousScroll", defaultValue: false);

    private static readonly ISetting<int> _automaticContinuousScrollDelay = Setting.CreateInt(RootSettingsPath, "DiffViewer.AutomaticContinuousScrollDelay", defaultValue: 600);

    private static readonly ISetting<bool> _logCaptureCallStacks = Setting.CreateBool(RootSettingsPath, "Log.CaptureCallStacks", defaultValue: false);

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<bool> _writeErrorLog = Setting.CreateBool(RootSettingsPath, "WriteErrorLog", defaultValue: false);

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<bool> _workaroundActivateFromMinimize = Setting.CreateBool(RootSettingsPath, "WorkaroundActivateFromMinimize", defaultValue: false);

    private static readonly ISetting<bool> _gitAsyncWhenMinimized = Setting.CreateBool(RootSettingsPath, "GitAsyncWhenMinimized", defaultValue: false);

    private static readonly ISetting<bool> _isEditorSettingsMigrated = Setting.CreateBool(MigrationSettingsPath, nameof(IsEditorSettingsMigrated), defaultValue: false);

    private static readonly ISetting<string> _uninformativeRepoNameRegex = Setting.Create(DetailedSettingsPath, nameof(UninformativeRepoNameRegex), defaultValue: "app|(repo(sitory)?)");

    private static readonly ISetting<string> _autoNormaliseSymbol = Setting.Create(
        RootSettingsPath,
        "AutoNormaliseSymbol",
        defaultValue: "_",
        read: static s => (true, s == "+" ? "" : s),
        store: static v => string.IsNullOrWhiteSpace(v) ? "+" : v);

    private static readonly ISetting<AvatarFallbackType> _avatarFallbackType = Setting.Create(
        RootSettingsPath,
        "GravatarDefaultImageType",
        defaultValue: GitCommands.AvatarFallbackType.AuthorInitials,
        read: static s => (true, s switch { "None" => GitCommands.AvatarFallbackType.AuthorInitials, _ => Enum.TryParse(s, out AvatarFallbackType e) ? e : GitCommands.AvatarFallbackType.AuthorInitials }),
        store: static v => v.ToString());

    private static readonly ISetting<AvatarProvider> _avatarProvider = Setting.Create(
        RootSettingsPath,
        "Appearance.AvatarProvider",
        defaultValue: GitCommands.AvatarProvider.None,
        read: static s => (true, s switch { "AuthorInitials" => GitCommands.AvatarProvider.None, "Gravatar" => GitCommands.AvatarProvider.Default, _ => Enum.TryParse(s, out AvatarProvider e) ? e : GitCommands.AvatarProvider.None }),
        store: static v => v.ToString());

    private static readonly ISetting<string[]> _revisionFilterDropdowns = Setting.Create(
        RootSettingsPath,
        "RevisionFilterDropdowns",
        defaultValue: Array.Empty<string>(),
        read: static s => (true, s.Split('\n', StringSplitOptions.RemoveEmptyEntries)),
        store: static v => string.Join("\n", v ?? []));

    private static readonly ISetting<string[]> _themeVariations = Setting.Create(
        RootSettingsPath,
        "uithemevariations",
        defaultValue: Array.Empty<string>(),
        read: static s => (true, s.Split(Delimiters.Comma, StringSplitOptions.RemoveEmptyEntries)),
        store: static v => string.Join(",", v ?? []));

    private static readonly ISetting<string[]> _customGenericRemoteNames = Setting.Create(
        RootSettingsPath,
        "CustomGenericRemoteNames",
        defaultValue: Array.Empty<string>(),
        read: static s => (true, s.Split(',', StringSplitOptions.RemoveEmptyEntries)),
        store: static v => string.Join(",", v ?? []));

    #endregion

    private static Mutex? _globalMutex;

    [GeneratedRegex(@"^(?<major>\d+)\.(?<minor>\d+)", RegexOptions.ExplicitCapture)]
    private static partial Regex VersionRegex { get; }

    public static event Action? Saved;

    static AppSettings()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ApplicationDataPath = new Lazy<string?>(() =>
        {
            if (IsPortable())
            {
                return GetGitExtensionsDirectory();
            }

            // Make ApplicationDataPath version independent
            return Application.UserAppDataPath.Replace(Application.ProductVersion, string.Empty)
                                              .Replace(ApplicationName, ApplicationId); // 'GitExtensions' has been changed to 'Git Extensions' in v3.0
        });

        LocalApplicationDataPath = new Lazy<string?>(() =>
        {
            if (IsPortable())
            {
                return GetGitExtensionsDirectory();
            }

            string path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationId);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        });

        bool newFile = CreateEmptySettingsFileIfMissing();

        SettingsContainer = new DistributedSettings(lowerPriority: null, GitExtSettingsCache.FromCache(SettingsFilePath), SettingLevel.Unknown);

        if (newFile || !File.Exists(SettingsFilePath))
        {
            ImportFromRegistry();
        }

        MigrateSshSettings();

        return;

        static bool CreateEmptySettingsFileIfMissing()
        {
            try
            {
                string? dir = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(dir) || File.Exists(SettingsFilePath))
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                // Illegal characters in the filename
                return false;
            }

            File.WriteAllText(SettingsFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />", Encoding.UTF8);
            return true;
        }
    }

    /// <summary>
    /// Gets the base part of the documentation link for the current application version,
    /// which looks something like "https://git-extensions-documentation.readthedocs.org/en/main/"
    /// for the master branch, and "https://git-extensions-documentation.readthedocs.org/en/release-X.Y/"
    /// for a release/X.Y branch.
    ///
    /// TODO: We currently use only EN language, but should maybe consider using the user's preferred language.
    /// </summary>
    public static string DocumentationBaseUrl
    {
        get => _documentationBaseUrl ?? throw new InvalidOperationException($"Call {nameof(SetDocumentationBaseUrl)} first to set the documentation base URL.");
    }

    internal static void SetDocumentationBaseUrl(string version)
    {
        if (_documentationBaseUrl is not null)
        {
            throw new InvalidOperationException("Documentation base URL can only be set once");
        }

        string? docVersion = "en/main/";
        const string defaultDevelopmentVersion = "33.33";
        if (!string.IsNullOrWhiteSpace(version) && !version.StartsWith(defaultDevelopmentVersion))
        {
            // We expect version to be something starting with "X.Y" (ignore patch versions)
            Match match = VersionRegex.Match(version);
            if (match.Success)
            {
                docVersion = $"en/release-{match.Groups["major"]}.{match.Groups["minor"]}/";
            }
        }

        _documentationBaseUrl = $"https://git-extensions-documentation.readthedocs.org/{docVersion}";
    }

    public static bool? TelemetryEnabled
    {
        get => Setting.GetNullableValue(_telemetryEnabled);
        set => Setting.SetValue(_telemetryEnabled, value);
    }

    public static bool AutoNormaliseBranchName
    {
        get => Setting.GetValue(_autoNormaliseBranchName);
        set => Setting.SetValue(_autoNormaliseBranchName, value);
    }

    public static string AutoNormaliseSymbol
    {
        get => Setting.GetValue(_autoNormaliseSymbol);
        set => Setting.SetValue(_autoNormaliseSymbol, value);
    }

    public static string FileEditorCommand => @$"""{AppSettings.GetGitExtensionsFullPath()}"" fileeditor";
    public static bool RememberAmendCommitState
    {
        get => Setting.GetValue(_rememberAmendCommitState);
        set => Setting.SetValue(_rememberAmendCommitState, value);
    }

    public static void UsingContainer(DistributedSettings settingsContainer, Action action)
    {
        SettingsContainer.LockedAction(() =>
            {
                DistributedSettings oldSC = SettingsContainer;
                try
                {
                    SettingsContainer = settingsContainer;
                    action();
                }
                finally
                {
                    SettingsContainer = oldSC;

                    // refresh settings if needed
                    SettingsContainer.GetString(string.Empty, null);
                }
            });
    }

    public static string? GetInstallDir()
    {
        if (IsPortable())
        {
            return GetGitExtensionsDirectory();
        }

        string dir = ReadStringRegValue("InstallDir", string.Empty);
        if (string.IsNullOrEmpty(dir))
        {
            return GetGitExtensionsDirectory();
        }

        return dir;
    }

    public static string? GetResourceDir()
    {
#if DEBUG
        string gitExtDir = GetGitExtensionsDirectory()!.TrimEnd('\\').TrimEnd('/');
        const string debugPath = @"GitExtensions\bin\Debug";
        int len = debugPath.Length;
        if (gitExtDir.Length > len)
        {
            string path = gitExtDir[^len..];

            if (debugPath.ToPosixPath() == path.ToPosixPath())
            {
                string projectPath = gitExtDir[..^len];
                return Path.Join(projectPath, "Bin");
            }
        }
#endif
        return GetInstallDir();
    }

    // for repair only
    public static void SetInstallDir(string dir)
    {
        WriteStringRegValue("InstallDir", dir);
    }

    #region Registry helpers

    private static bool ReadBoolRegKey(string key, bool defaultValue)
    {
        object? obj = VersionIndependentRegKey.GetValue(key);
        if (obj is not string)
        {
            obj = null;
        }

        if (obj is null)
        {
            return defaultValue;
        }

        return ((string)obj).Equals("true", StringComparison.CurrentCultureIgnoreCase);
    }

    private static void WriteBoolRegKey(string key, bool value)
    {
        VersionIndependentRegKey.SetValue(key, value ? "true" : "false");
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    private static string? ReadStringRegValue(string key, string? defaultValue)
    {
        return (string?)VersionIndependentRegKey.GetValue(key, defaultValue);
    }

    private static void WriteStringRegValue(string key, string value)
    {
        VersionIndependentRegKey.SetValue(key, value);
    }

    #endregion

    public static bool CheckSettings
    {
        get => ReadBoolRegKey("CheckSettings", true);
        set => WriteBoolRegKey("CheckSettings", value);
    }

    public static string CascadeShellMenuItems
    {
        get => ReadStringRegValue("CascadeShellMenuItems", "110111000111111111");
        set => WriteStringRegValue("CascadeShellMenuItems", value);
    }

    public static int FileStatusFindInFilesGitGrepTypeIndex
    {
        get => Setting.GetValue(_fileStatusFindInFilesGitGrepTypeIndex);
        set => Setting.SetValue(_fileStatusFindInFilesGitGrepTypeIndex, value);
    }

    public static bool FileStatusMergeSingleItemWithFolder
    {
        get => Setting.GetValue(_fileStatusMergeSingleItemWithFolder);
        set => Setting.SetValue(_fileStatusMergeSingleItemWithFolder, value);
    }

    public static bool FileStatusShowGroupNodesInFlatList
    {
        get => Setting.GetValue(_fileStatusShowGroupNodesInFlatList);
        set => Setting.SetValue(_fileStatusShowGroupNodesInFlatList, value);
    }

    public static string SshPath
    {
        get => ReadStringRegValue("gitssh", "");
        set => WriteStringRegValue("gitssh", value);
    }

    public static bool AlwaysShowAllCommands
    {
        get => ReadBoolRegKey("AlwaysShowAllCommands", false);
        set => WriteBoolRegKey("AlwaysShowAllCommands", value);
    }

    public static bool ShowCurrentBranchInVisualStudio
    {
        // This setting MUST be set to false by default, otherwise it will not work in Visual Studio without
        // other changes in the Visual Studio plugin itself.
        get => ReadBoolRegKey("ShowCurrentBranchInVS", true);
        set => WriteBoolRegKey("ShowCurrentBranchInVS", value);
    }

    public static string GitCommandValue
    {
        get
        {
            if (IsPortable())
            {
                return GetString("gitcommand", "");
            }
            else
            {
                return ReadStringRegValue("gitcommand", "");
            }
        }
        set
        {
            if (GitCommandValue == value)
            {
                return;
            }

            if (IsPortable())
            {
                SetString("gitcommand", value);
            }
            else
            {
                WriteStringRegValue("gitcommand", value);
            }

            GitVersion.ResetVersion();
        }
    }

    public static string GitCommand
    {
        get
        {
            if (string.IsNullOrEmpty(GitCommandValue))
            {
                return "git";
            }

            return GitCommandValue;
        }
    }

    public static bool WslGitEnabled
    {
        get => Setting.GetValue(_wslGitEnabled);
    }

    public static string WslCommand
    {
        get => Setting.GetValue(_wslCommand);
    }

    public static string WslGitCommand
    {
        get => Setting.GetValue(_wslGitCommand);
    }

    public static bool StashKeepIndex
    {
        get => Setting.GetValue(_stashKeepIndex);
        set => Setting.SetValue(_stashKeepIndex, value);
    }

    public static bool DontConfirmStashDrop
    {
        get => Setting.GetValue(_dontConfirmStashDrop);
        set => Setting.SetValue(_dontConfirmStashDrop, value);
    }

    public static bool ApplyPatchIgnoreWhitespace
    {
        get => Setting.GetValue(_applyPatchIgnoreWhitespace);
        set => Setting.SetValue(_applyPatchIgnoreWhitespace, value);
    }

    public static bool ApplyPatchSignOff
    {
        get => Setting.GetValue(_applyPatchSignOff);
        set => Setting.SetValue(_applyPatchSignOff, value);
    }

    public static bool UseHistogramDiffAlgorithm
    {
        get => Setting.GetValue(_useHistogramDiffAlgorithm);
        set => Setting.SetValue(_useHistogramDiffAlgorithm, value);
    }

    public static bool UseGitColoring
    {
        get => Setting.GetValue(_useGitColoring);
        set => Setting.SetValue(_useGitColoring, value);
    }

    public static bool ReverseGitColoring
    {
        get => Setting.GetValue(_reverseGitColoring);
        set => Setting.SetValue(_reverseGitColoring, value);
    }

    public static bool ShowErrorsWhenStagingFiles
    {
        get => Setting.GetValue(_showErrorsWhenStagingFiles);
        set => Setting.SetValue(_showErrorsWhenStagingFiles, value);
    }

    public static bool EnsureCommitMessageSecondLineEmpty
    {
        get => Setting.GetValue(_ensureCommitMessageSecondLineEmpty);
        set => Setting.SetValue(_ensureCommitMessageSecondLineEmpty, value);
    }

    public static string LastCommitMessage
    {
        get => Setting.GetValue(_lastCommitMessage);
        set => Setting.SetValue(_lastCommitMessage, value);
    }

    public static int CommitDialogNumberOfPreviousMessages
    {
        get => Setting.GetValue(_commitDialogNumberOfPreviousMessages);
        set => Setting.SetValue(_commitDialogNumberOfPreviousMessages, value);
    }

    public static bool CommitDialogSelectStagedOnEnterMessage
    {
        get => Setting.GetValue(_commitDialogSelectStagedOnEnterMessage);
        set => Setting.SetValue(_commitDialogSelectStagedOnEnterMessage, value);
    }

    public static bool CommitDialogShowOnlyMyMessages
    {
        get => Setting.GetValue(_commitDialogShowOnlyMyMessages);
        set => Setting.SetValue(_commitDialogShowOnlyMyMessages, value);
    }

    public static bool ShowCommitAndPush
    {
        get => Setting.GetValue(_showCommitAndPush);
        set => Setting.SetValue(_showCommitAndPush, value);
    }

    public static bool ShowResetWorkTreeChanges
    {
        get => Setting.GetValue(_showResetWorkTreeChanges);
        set => Setting.SetValue(_showResetWorkTreeChanges, value);
    }

    public static bool ShowResetAllChanges
    {
        get => Setting.GetValue(_showResetAllChanges);
        set => Setting.SetValue(_showResetAllChanges, value);
    }

    public static bool ShowConEmuTab
    {
        get => Setting.GetValue(_showConEmuTab);
        set => Setting.SetValue(_showConEmuTab, value);
    }

    private const string ConEmuStyleDefault = "Default";
    private const string ConEmuStyleDark = "<Tomorrow Night>";
    private const string ConEmuStyleLight = "<Tomorrow>";

    /// <summary>
    ///  Gets the <see cref="ISetting{T}"/> for the ConEmu style, used for settings page bindings.
    /// </summary>
    public static ISetting<string> ConEmuStyleSetting => _conEmuStyle;

    public static string ConEmuStyle
    {
        get => Setting.GetValue(_conEmuStyle);
        set => Setting.SetValue(_conEmuStyle, value);
    }

    /// <summary>
    ///  Returns the ConEmu style to use. When the configured value is <see cref="ConEmuStyleDefault"/>,
    ///  automatically selects a style that matches the current application theme.
    /// </summary>
    public static string GetEffectiveConEmuStyle()
    {
        string style = ConEmuStyle;
        return style == ConEmuStyleDefault
            ? Application.IsDarkModeEnabled ? ConEmuStyleDark : ConEmuStyleLight
            : style;
    }

    public static string ConEmuTerminal
    {
        get => Setting.GetValue(_conEmuTerminal);
        set => Setting.SetValue(_conEmuTerminal, value);
    }

    public static int OutputHistoryDepth
    {
        get => Setting.GetValue(_outputHistoryDepth);
        set => Setting.SetValue(_outputHistoryDepth, value);
    }

    public static bool OutputHistoryPanelVisible
    {
        get => Setting.GetValue(_outputHistoryPanelVisible);
        set => Setting.SetValue(_outputHistoryPanelVisible, value);
    }

    public static bool ShowOutputHistoryAsTab
    {
        get => Setting.GetValue(_showOutputHistoryAsTab);
        set => Setting.SetValue(_showOutputHistoryAsTab, value);
    }

    public static bool UseBrowseForFileHistory
    {
        get => Setting.GetValue(_useBrowseForFileHistory);
        set => Setting.SetValue(_useBrowseForFileHistory, value);
    }

    public static bool UseDiffViewerForBlame
    {
        get => Setting.GetValue(_useDiffViewerForBlame);
        set => Setting.SetValue(_useDiffViewerForBlame, value);
    }

    public static bool ShowGpgInformation
    {
        get => Setting.GetValue(_showGpgInformation);
        set => Setting.SetValue(_showGpgInformation, value);
    }

    public static CommitInfoPosition CommitInfoPosition
    {
        get => ((ISettingsValueGetter)DetailedSettingsPath).GetValue<CommitInfoPosition>("CommitInfoPosition") ?? (
            DetailedSettingsPath.GetBool("ShowRevisionInfoNextToRevisionGrid") == true // legacy setting
                ? CommitInfoPosition.RightwardFromList
                : CommitInfoPosition.BelowList);
        set => DetailedSettingsPath.SetEnum("CommitInfoPosition", value);
    }

    public static bool MessageEditorWordWrap
    {
        get => Setting.GetValue(_messageEditorWordWrap);
        set => Setting.SetValue(_messageEditorWordWrap, value);
    }

    public static bool ShowSplitViewLayout
    {
        get => Setting.GetValue(_showSplitViewLayout);
        set => Setting.SetValue(_showSplitViewLayout, value);
    }

    public static bool ProvideAutocompletion
    {
        get => Setting.GetValue(_provideAutocompletion);
        set => Setting.SetValue(_provideAutocompletion, value);
    }

    public static TruncatePathMethod TruncatePathMethod
    {
        get => Setting.GetValue(_truncatePathMethod);
        set => Setting.SetValue(_truncatePathMethod, value);
    }

    public static bool ShowGitStatusInBrowseToolbar
    {
        get => Setting.GetValue(_showGitStatusInBrowseToolbar);
        set => Setting.SetValue(_showGitStatusInBrowseToolbar, value);
    }

    public static bool ShowGitStatusForArtificialCommits
    {
        get => Setting.GetValue(_showGitStatusForArtificialCommits);
        set => Setting.SetValue(_showGitStatusForArtificialCommits, value);
    }

    public static EnumRuntimeSetting<RevisionSortOrder> RevisionSortOrder { get; } = new(RootSettingsPath, nameof(RevisionSortOrder), GitCommands.RevisionSortOrder.GitDefault);

    public static bool CommitInfoShowContainedInBranches => CommitInfoShowContainedInBranchesLocal ||
                                                            CommitInfoShowContainedInBranchesRemote ||
                                                            CommitInfoShowContainedInBranchesRemoteIfNoLocal;
    public static bool CommitInfoShowContainedInBranchesLocal
    {
        get => Setting.GetValue(_commitInfoShowContainedInBranchesLocal);
        set => Setting.SetValue(_commitInfoShowContainedInBranchesLocal, value);
    }

    public static bool CheckForUncommittedChangesInCheckoutBranch
    {
        get => Setting.GetValue(_checkForUncommittedChangesInCheckoutBranch);
        set => Setting.SetValue(_checkForUncommittedChangesInCheckoutBranch, value);
    }

    public static bool AlwaysShowCheckoutBranchDlg
    {
        get => Setting.GetValue(_alwaysShowCheckoutBranchDlg);
        set => Setting.SetValue(_alwaysShowCheckoutBranchDlg, value);
    }

    public static bool CommitAndPushForcedWhenAmend
    {
        get => Setting.GetValue(_commitAndPushForcedWhenAmend);
        set => Setting.SetValue(_commitAndPushForcedWhenAmend, value);
    }

    public static bool CommitInfoShowContainedInBranchesRemote
    {
        get => Setting.GetValue(_commitInfoShowContainedInBranchesRemote);
        set => Setting.SetValue(_commitInfoShowContainedInBranchesRemote, value);
    }

    public static bool CommitInfoShowContainedInBranchesRemoteIfNoLocal
    {
        get => Setting.GetValue(_commitInfoShowContainedInBranchesRemoteIfNoLocal);
        set => Setting.SetValue(_commitInfoShowContainedInBranchesRemoteIfNoLocal, value);
    }

    public static bool CommitInfoShowContainedInTags
    {
        get => Setting.GetValue(_commitInfoShowContainedInTags);
        set => Setting.SetValue(_commitInfoShowContainedInTags, value);
    }

    public static bool CommitInfoShowTagThisCommitDerivesFrom
    {
        get => Setting.GetValue(_commitInfoShowTagThisCommitDerivesFrom);
        set => Setting.SetValue(_commitInfoShowTagThisCommitDerivesFrom, value);
    }

    #region Avatars

    public static string AvatarImageCachePath => Path.Join(LocalApplicationDataPath.Value!, "Images\\");

    public static AvatarFallbackType AvatarFallbackType
    {
        get => Setting.GetValue(_avatarFallbackType);
        set => Setting.SetValue(_avatarFallbackType, value);
    }

    public static string CustomAvatarTemplate
    {
        get => Setting.GetValue(_customAvatarTemplate);
        set => Setting.SetValue(_customAvatarTemplate, value);
    }

    /// <summary>
    /// Gets the size of the commit author avatar. Set to 80px.
    /// </summary>
    /// <remarks>The value should be scaled with DPI.</remarks>
    public static int AuthorImageSizeInCommitInfo => 80;
    public static int AvatarImageCacheDays
    {
        get => Setting.GetValue(_avatarImageCacheDays);
        set => Setting.SetValue(_avatarImageCacheDays, value);
    }

    public static bool ShowAuthorAvatarInCommitInfo
    {
        get => Setting.GetValue(_showAuthorAvatarInCommitInfo);
        set => Setting.SetValue(_showAuthorAvatarInCommitInfo, value);
    }

    public static AvatarProvider AvatarProvider
    {
        get => Setting.GetValue(_avatarProvider);
        set => Setting.SetValue(_avatarProvider, value);
    }

    public static int AvatarCacheSize
    {
        get => Setting.GetValue(_avatarCacheSize);
        set => Setting.SetValue(_avatarCacheSize, value);
    }

    public static string AvatarAuthorInitialsPalette
    {
        get => Setting.GetValue(_avatarAuthorInitialsPalette);
        set => Setting.SetValue(_avatarAuthorInitialsPalette, value);
    }

    // Currently not configurable in UI (Set manually in settings file)
    public static float AvatarAuthorInitialsLuminanceThreshold => GetFloat("AvatarAuthorInitialsLuminanceThreshold", 0.5f);

    /// <summary>
    /// Loads a setting with GetString and parses it to an enum
    /// </summary>
    /// <remarks>
    /// It's currently a limitation by <see cref="SettingsCache"/> that a given setting can
    /// only ever use GetString/SetString or GetEnum/SetEnum but not both. This is the case
    /// because <see cref="SettingsCache"/> caches a typed/parsed value of the setting and
    /// crashes at <see cref="SettingsCache.TryGetValue(string, out string?)"/>
    /// if the type that is requested doesn't match the cached type.
    /// </remarks>
    private static void MigrateSshSettings()
    {
        string ssh = AppSettings.SshPath;
        if (!string.IsNullOrEmpty(ssh))
        {
            // OpenSSH uses empty path, compatibility with path set in 3.4
            string? path = new SshPathLocator().GetSshFromGitDir(LinuxToolsDir);
            if (path == ssh)
            {
                AppSettings.SshPath = "";
            }
        }
    }

    /// <summary>
    /// Migrate editor settings if GE was installed in 'Program Files (x86)' before 4.3.
    /// Only check and update global Git settings in Windows, the only set by GE.
    /// Guess previous, migrate to current path (i.e. the first usage).
    /// Only needed in x64, should be meaningless on other platforms.
    /// </summary>
    private static void MigrateEditorSettings()
    {
        // Only run this once
        bool isMigrated = IsEditorSettingsMigrated;
        IsEditorSettingsMigrated = true;
        if (isMigrated)
        {
            return;
        }

        EnvironmentConfiguration.SetEnvironmentVariables();
        IPersistentConfigValueStore globalSettings = new GitConfigSettings(new Executable(AppSettings.GitCommand), GitSettingLevel.Global);
        string? path = globalSettings.GetValue("core.editor");
        if (path?.Contains("Program Files (x86)/GitExtensions", StringComparison.CurrentCultureIgnoreCase) is not true)
        {
            return;
        }

#if DEBUG
        // avoid setting, this may be in the debugger
        throw new Exception($"Update the core.editor path {path} in global git settings");
#else
        globalSettings.SetValue("core.editor", FileEditorCommand.ConvertPathToGitSetting());
        globalSettings.Save();
#endif
    }

    #endregion
    public static string Translation
    {
        get => Setting.GetValue(_translation);
        set => Setting.SetValue(_translation, value);
    }

    private static string? _currentTranslation;

    public static string? CurrentTranslation
    {
        get => _currentTranslation ?? Translation;
        set => _currentTranslation = value;
    }

    private static readonly Dictionary<string, string> _languageCodes =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Czech", "cs" },
            { "Dutch", "nl" },
            { "English", "en" },
            { "French", "fr" },
            { "German", "de" },
            { "Indonesian", "id" },
            { "Italian", "it" },
            { "Japanese", "ja" },
            { "Korean", "ko" },
            { "Polish", "pl" },
            { "Portuguese (Brazil)", "pt-BR" },
            { "Portuguese (Portugal)", "pt-PT" },
            { "Romanian", "ro" },
            { "Russian", "ru" },
            { "Simplified Chinese", "zh-CN" },
            { "Spanish", "es" },
            { "Traditional Chinese", "zh-TW" }
        };

    public static string CurrentLanguageCode
    {
        get
        {
            if (CurrentTranslation is not null && _languageCodes.TryGetValue(CurrentTranslation, out string? code))
            {
                return code;
            }

            return "en";
        }
    }

    public static CultureInfo CurrentCultureInfo
    {
        get
        {
            try
            {
                return CultureInfo.GetCultureInfo(CurrentLanguageCode);
            }
            catch (CultureNotFoundException)
            {
                Debug.WriteLine("Culture {0} not found", [CurrentLanguageCode]);
                return CultureInfo.GetCultureInfo("en");
            }
        }
    }

    public static bool UserProfileHomeDir
    {
        get => Setting.GetValue(_userProfileHomeDir);
        set => Setting.SetValue(_userProfileHomeDir, value);
    }

    public static string CustomHomeDir
    {
        get => Setting.GetValue(_customHomeDir);
        set => Setting.SetValue(_customHomeDir, value);
    }

    public static bool EnableAutoScale
    {
        get => Setting.GetValue(_enableAutoScale);
        set => Setting.SetValue(_enableAutoScale, value);
    }

    public static bool CloseCommitDialogAfterCommit
    {
        get => Setting.GetValue(_closeCommitDialogAfterCommit);
        set => Setting.SetValue(_closeCommitDialogAfterCommit, value);
    }

    public static bool CloseCommitDialogAfterLastCommit
    {
        get => Setting.GetValue(_closeCommitDialogAfterLastCommit);
        set => Setting.SetValue(_closeCommitDialogAfterLastCommit, value);
    }

    public static bool RefreshArtificialCommitOnApplicationActivated
    {
        get => Setting.GetValue(_refreshArtificialCommitOnApplicationActivated);
        set => Setting.SetValue(_refreshArtificialCommitOnApplicationActivated, value);
    }

    public static bool StageInSuperprojectAfterCommit
    {
        get => Setting.GetValue(_stageInSuperprojectAfterCommit);
        set => Setting.SetValue(_stageInSuperprojectAfterCommit, value);
    }

    public static bool FollowRenamesInFileHistory
    {
        get => Setting.GetValue(_followRenamesInFileHistory);
        set => Setting.SetValue(_followRenamesInFileHistory, value);
    }

    public static bool FollowRenamesInFileHistoryExactOnly
    {
        get => Setting.GetValue(_followRenamesInFileHistoryExactOnly);
        set => Setting.SetValue(_followRenamesInFileHistoryExactOnly, value);
    }

    public static bool FullHistoryInFileHistory
    {
        get => Setting.GetValue(_fullHistoryInFileHistory);
        set => Setting.SetValue(_fullHistoryInFileHistory, value);
    }

    public static bool SimplifyMergesInFileHistory
    {
        get => Setting.GetValue(_simplifyMergesInFileHistory);
        set => Setting.SetValue(_simplifyMergesInFileHistory, value);
    }

    public static bool LoadFileHistoryOnShow
    {
        get => Setting.GetValue(_loadFileHistoryOnShow);
        set => Setting.SetValue(_loadFileHistoryOnShow, value);
    }

    public static bool LoadBlameOnShow
    {
        get => Setting.GetValue(_loadBlameOnShow);
        set => Setting.SetValue(_loadBlameOnShow, value);
    }

    public static bool DetectCopyInFileOnBlame
    {
        get => Setting.GetValue(_detectCopyInFileOnBlame);
        set => Setting.SetValue(_detectCopyInFileOnBlame, value);
    }

    public static bool DetectCopyInAllOnBlame
    {
        get => Setting.GetValue(_detectCopyInAllOnBlame);
        set => Setting.SetValue(_detectCopyInAllOnBlame, value);
    }

    public static bool IgnoreWhitespaceOnBlame
    {
        get => Setting.GetValue(_ignoreWhitespaceOnBlame);
        set => Setting.SetValue(_ignoreWhitespaceOnBlame, value);
    }

    public static bool OpenSubmoduleDiffInSeparateWindow
    {
        get => Setting.GetValue(_openSubmoduleDiffInSeparateWindow);
        set => Setting.SetValue(_openSubmoduleDiffInSeparateWindow, value);
    }

    public static bool RevisionGraphShowArtificialCommits
    {
        get => Setting.GetValue(_revisionGraphShowArtificialCommits);
        set => Setting.SetValue(_revisionGraphShowArtificialCommits, value);
    }

    public static bool RevisionGraphDrawAlternateBackColor
    {
        get => Setting.GetValue(_revisionGraphDrawAlternateBackColor);
        set => Setting.SetValue(_revisionGraphDrawAlternateBackColor, value);
    }

    public static bool RevisionGraphDrawNonRelativesGray
    {
        get => Setting.GetValue(_revisionGraphDrawNonRelativesGray);
        set => Setting.SetValue(_revisionGraphDrawNonRelativesGray, value);
    }

    public static bool RevisionGraphDrawNonRelativesTextGray
    {
        get => Setting.GetValue(_revisionGraphDrawNonRelativesTextGray);
        set => Setting.SetValue(_revisionGraphDrawNonRelativesTextGray, value);
    }

    public static readonly Dictionary<string, Encoding> AvailableEncodings = [];
    public static GitPullAction DefaultPullAction
    {
        get => Setting.GetValue(_defaultPullAction);
        set => Setting.SetValue(_defaultPullAction, value);
    }

    public static GitPullAction FormPullAction
    {
        get => Setting.GetValue(_formPullAction);
        set => Setting.SetValue(_formPullAction, value);
    }

    public static bool AutoStash
    {
        get => Setting.GetValue(_autoStash);
        set => Setting.SetValue(_autoStash, value);
    }

    public static bool RebaseAutoStash
    {
        get => Setting.GetValue(_rebaseAutoStash);
        set => Setting.SetValue(_rebaseAutoStash, value);
    }

    public static LocalChangesAction CheckoutBranchAction
    {
        get => Setting.GetValue(_checkoutBranchAction);
        set => Setting.SetValue(_checkoutBranchAction, value);
    }

    public static bool CheckoutOtherBranchAfterReset
    {
        get => Setting.GetValue(_checkoutOtherBranchAfterReset);
        set => Setting.SetValue(_checkoutOtherBranchAfterReset, value);
    }

    public static bool UseDefaultCheckoutBranchAction
    {
        get => Setting.GetValue(_useDefaultCheckoutBranchAction);
        set => Setting.SetValue(_useDefaultCheckoutBranchAction, value);
    }

    public static bool DontShowHelpImages
    {
        get => Setting.GetValue(_dontShowHelpImages);
        set => Setting.SetValue(_dontShowHelpImages, value);
    }

    public static bool AlwaysShowAdvOpt
    {
        get => Setting.GetValue(_alwaysShowAdvOpt);
        set => Setting.SetValue(_alwaysShowAdvOpt, value);
    }

    public static bool DontConfirmAmend
    {
        get => Setting.GetValue(_dontConfirmAmend);
        set => Setting.SetValue(_dontConfirmAmend, value);
    }

    public static bool DontConfirmDeleteUnmergedBranch
    {
        get => Setting.GetValue(_dontConfirmDeleteUnmergedBranch);
        set => Setting.SetValue(_dontConfirmDeleteUnmergedBranch, value);
    }

    public static bool DontConfirmCommitIfNoBranch
    {
        get => Setting.GetValue(_dontConfirmCommitIfNoBranch);
        set => Setting.SetValue(_dontConfirmCommitIfNoBranch, value);
    }

    public static bool ConfirmBranchCheckout
    {
        get => Setting.GetValue(_confirmBranchCheckout);
        set => Setting.SetValue(_confirmBranchCheckout, value);
    }

    public static bool? AutoPopStashAfterPull
    {
        get => Setting.GetNullableValue(_autoPopStashAfterPull);
        set => Setting.SetValue(_autoPopStashAfterPull, value);
    }

    public static bool? AutoPopStashAfterCheckoutBranch
    {
        get => Setting.GetNullableValue(_autoPopStashAfterCheckoutBranch);
        set => Setting.SetValue(_autoPopStashAfterCheckoutBranch, value);
    }

    public static GitPullAction? AutoPullOnPushRejectedAction
    {
        get => GetNullableEnum<GitPullAction>("AutoPullOnPushRejectedAction");
        set => SetNullableEnum("AutoPullOnPushRejectedAction", value);
    }

    public static bool DontConfirmPushNewBranch
    {
        get => Setting.GetValue(_dontConfirmPushNewBranch);
        set => Setting.SetValue(_dontConfirmPushNewBranch, value);
    }

    public static bool DontConfirmAddTrackingRef
    {
        get => Setting.GetValue(_dontConfirmAddTrackingRef);
        set => Setting.SetValue(_dontConfirmAddTrackingRef, value);
    }

    public static bool DontConfirmCommitAfterConflictsResolved
    {
        get => Setting.GetValue(_dontConfirmCommitAfterConflictsResolved);
        set => Setting.SetValue(_dontConfirmCommitAfterConflictsResolved, value);
    }

    public static bool DontConfirmSecondAbortConfirmation
    {
        get => Setting.GetValue(_dontConfirmSecondAbortConfirmation);
        set => Setting.SetValue(_dontConfirmSecondAbortConfirmation, value);
    }

    public static bool DontConfirmRebase
    {
        get => Setting.GetValue(_dontConfirmRebase);
        set => Setting.SetValue(_dontConfirmRebase, value);
    }

    public static bool DontConfirmResolveConflicts
    {
        get => Setting.GetValue(_dontConfirmResolveConflicts);
        set => Setting.SetValue(_dontConfirmResolveConflicts, value);
    }

    public static bool DontConfirmUndoLastCommit
    {
        get => Setting.GetValue(_dontConfirmUndoLastCommit);
        set => Setting.SetValue(_dontConfirmUndoLastCommit, value);
    }

    public static bool DontConfirmFetchAndPruneAll
    {
        get => Setting.GetValue(_dontConfirmFetchAndPruneAll);
        set => Setting.SetValue(_dontConfirmFetchAndPruneAll, value);
    }

    public static bool DontConfirmSwitchWorktree
    {
        get => Setting.GetValue(_dontConfirmSwitchWorktree);
        set => Setting.SetValue(_dontConfirmSwitchWorktree, value);
    }

    public static bool IncludeUntrackedFilesInAutoStash
    {
        get => Setting.GetValue(_includeUntrackedFilesInAutoStash);
        set => Setting.SetValue(_includeUntrackedFilesInAutoStash, value);
    }

    public static bool IncludeUntrackedFilesInManualStash
    {
        get => Setting.GetValue(_includeUntrackedFilesInManualStash);
        set => Setting.SetValue(_includeUntrackedFilesInManualStash, value);
    }

    public static bool ShowRemoteBranches
    {
        get => Setting.GetValue(_showRemoteBranches);
        set => Setting.SetValue(_showRemoteBranches, value);
    }

    public static BoolRuntimeSetting ShowReflogReferences { get; } = new(RootSettingsPath, nameof(ShowReflogReferences), false);
    public static bool ShowStashes
    {
        get => Setting.GetValue(_showStashes);
        set => Setting.SetValue(_showStashes, value);
    }

    public static int MaxStashesWithUntrackedFiles
    {
        get => Setting.GetValue(_maxStashesWithUntrackedFiles);
    }

    public static bool ShowSuperprojectTags
    {
        get => Setting.GetValue(_showSuperprojectTags);
        set => Setting.SetValue(_showSuperprojectTags, value);
    }

    public static bool ShowSuperprojectBranches
    {
        get => Setting.GetValue(_showSuperprojectBranches);
        set => Setting.SetValue(_showSuperprojectBranches, value);
    }

    public static bool ShowSuperprojectRemoteBranches
    {
        get => Setting.GetValue(_showSuperprojectRemoteBranches);
        set => Setting.SetValue(_showSuperprojectRemoteBranches, value);
    }

    public static bool? UpdateSubmodulesOnCheckout
    {
        get => Setting.GetNullableValue(_updateSubmodulesOnCheckout);
        set => Setting.SetValue(_updateSubmodulesOnCheckout, value);
    }

    public static bool? DontConfirmUpdateSubmodulesOnCheckout
    {
        get => Setting.GetNullableValue(_dontConfirmUpdateSubmodulesOnCheckout);
        set => Setting.SetValue(_dontConfirmUpdateSubmodulesOnCheckout, value);
    }

    public static string Dictionary
    {
        get => SettingsContainer.Detached().Dictionary;
        set => SettingsContainer.Detached().Dictionary = value;
    }

    public static bool ShowGitCommandLine
    {
        get => Setting.GetValue(_showGitCommandLine);
        set => Setting.SetValue(_showGitCommandLine, value);
    }

    public static bool ShowStashCount
    {
        get => Setting.GetValue(_showStashCount);
        set => Setting.SetValue(_showStashCount, value);
    }

    public static bool ShowAheadBehindData
    {
        get => Setting.GetValue(_showAheadBehindData);
        set => Setting.SetValue(_showAheadBehindData, value);
    }

    public static bool ShowSubmoduleStatus
    {
        get => Setting.GetValue(_showSubmoduleStatus);
        set => Setting.SetValue(_showSubmoduleStatus, value);
    }

    public static bool RelativeDate
    {
        get => Setting.GetValue(_relativeDate);
        set => Setting.SetValue(_relativeDate, value);
    }

    public static bool ShowGitNotes
    {
        get => Setting.GetValue(_showGitNotes);
        set => Setting.SetValue(_showGitNotes, value);
    }

    public static bool ShowSessionRefs
    {
        get => Setting.GetValue(_showSessionRefs);
        set => Setting.SetValue(_showSessionRefs, value);
    }

    public static bool ShowGitNotesColumn
    {
        get => Setting.GetValue(_showGitNotesColumn);
        set => Setting.SetValue(_showGitNotesColumn, value);
    }

    public static bool ShowAnnotatedTagsMessages
    {
        get => Setting.GetValue(_showAnnotatedTagsMessages);
        set => Setting.SetValue(_showAnnotatedTagsMessages, value);
    }

    public static bool HideMergeCommits
    {
        get => Setting.GetValue(_hideMergeCommits);
        set => Setting.SetValue(_hideMergeCommits, value);
    }

    public static bool ShowTags
    {
        get => Setting.GetValue(_showTags);
        set => Setting.SetValue(_showTags, value);
    }

    #region Revision grid column visibilities
    public static bool ShowRevisionGridGraphColumn
    {
        get => Setting.GetValue(_showRevisionGridGraphColumn);
        set => Setting.SetValue(_showRevisionGridGraphColumn, value);
    }

    public static bool ShowAuthorAvatarColumn
    {
        get => Setting.GetValue(_showAuthorAvatarColumn);
        set => Setting.SetValue(_showAuthorAvatarColumn, value);
    }

    public static bool ShowAuthorNameColumn
    {
        get => Setting.GetValue(_showAuthorNameColumn);
        set => Setting.SetValue(_showAuthorNameColumn, value);
    }

    public static bool ShowDateColumn
    {
        get => Setting.GetValue(_showDateColumn);
        set => Setting.SetValue(_showDateColumn, value);
    }

    public static bool ShowObjectIdColumn
    {
        get => Setting.GetValue(_showObjectIdColumn);
        set => Setting.SetValue(_showObjectIdColumn, value);
    }

    public static bool ShowBuildStatusIconColumn
    {
        get => Setting.GetValue(_showBuildStatusIconColumn);
        set => Setting.SetValue(_showBuildStatusIconColumn, value);
    }

    public static bool ShowBuildStatusTextColumn
    {
        get => Setting.GetValue(_showBuildStatusTextColumn);
        set => Setting.SetValue(_showBuildStatusTextColumn, value);
    }

    #endregion
    public static bool ShowAuthorDate
    {
        get => Setting.GetValue(_showAuthorDate);
        set => Setting.SetValue(_showAuthorDate, value);
    }

    public static bool CloseProcessDialog
    {
        get => Setting.GetValue(_closeProcessDialog);
        set => Setting.SetValue(_closeProcessDialog, value);
    }

    public static bool ShowProcessDialogPasswordInput
    {
        get => Setting.GetValue(_showProcessDialogPasswordInput);
        set => Setting.SetValue(_showProcessDialogPasswordInput, value);
    }

    public static BoolRuntimeSetting ShowCurrentBranchOnly { get; } = new(RootSettingsPath, nameof(ShowCurrentBranchOnly), false);
    public static bool ShowSimplifyByDecoration
    {
        get => Setting.GetValue(_showSimplifyByDecoration);
        set => Setting.SetValue(_showSimplifyByDecoration, value);
    }

    public static BoolRuntimeSetting BranchFilterEnabled { get; } = new(RootSettingsPath, nameof(BranchFilterEnabled), false);
    public static bool ShowOnlyFirstParent
    {
        get => Setting.GetValue(_showOnlyFirstParent);
        set => Setting.SetValue(_showOnlyFirstParent, value);
    }

    public static string[] RevisionFilterDropdowns
    {
        get => Setting.GetRawValue(_revisionFilterDropdowns);
        set => Setting.SetValue(_revisionFilterDropdowns, value);
    }

    public static bool CommitDialogSelectionFilter
    {
        get => Setting.GetValue(_commitDialogSelectionFilter);
        set => Setting.SetValue(_commitDialogSelectionFilter, value);
    }

    public static string DefaultCloneDestinationPath
    {
        get => Setting.GetValue(_defaultCloneDestinationPath);
        set => Setting.SetValue(_defaultCloneDestinationPath, value);
    }

    public static int RevisionGridQuickSearchTimeout
    {
        get => Setting.GetValue(_revisionGridQuickSearchTimeout);
        set => Setting.SetValue(_revisionGridQuickSearchTimeout, value);
    }

    public static bool ShowCommitBodyInRevisionGrid
    {
        get => Setting.GetValue(_showCommitBodyInRevisionGrid);
        set => Setting.SetValue(_showCommitBodyInRevisionGrid, value);
    }

    /// <summary>Gets or sets the path to the GNU/Linux tools (bash, ps, sh, ssh, etc.), e.g. "C:\Program Files\Git\usr\bin\"</summary>
    public static string LinuxToolsDir
    {
        // History Compatibility: Migrate the setting value from the from the former "gitbindir"
        get => GetString(nameof(LinuxToolsDir), defaultValue: null) ?? GetString("gitbindir", "");
        set
        {
            string linuxToolsDir = value.EnsureTrailingPathSeparator();
            SetString(nameof(LinuxToolsDir), linuxToolsDir);
        }
    }

    public static int MaxRevisionGraphCommits
    {
        get => Setting.GetValue(_maxRevisionGraphCommits);
        set => Setting.SetValue(_maxRevisionGraphCommits, value);
    }

    public static bool ShowDiffForAllParents
    {
        get => Setting.GetValue(_showDiffForAllParents);
        set => Setting.SetValue(_showDiffForAllParents, value);
    }

    public static bool ShowFindInCommitFilesGitGrep
    {
        get => Setting.GetValue(_showFindInCommitFilesGitGrep);
        set => Setting.SetValue(_showFindInCommitFilesGitGrep, value);
    }

    public static bool ShowRevisionGridTooltips
    {
        get => Setting.GetValue(_showRevisionGridTooltips);
        set => Setting.SetValue(_showRevisionGridTooltips, value);
    }

    public static bool ShowAvailableDiffTools
    {
        get => Setting.GetValue(_showAvailableDiffTools);
        set => Setting.SetValue(_showAvailableDiffTools, value);
    }

    public static int DiffVerticalRulerPosition
    {
        get => Setting.GetValue(_diffVerticalRulerPosition);
        set => Setting.SetValue(_diffVerticalRulerPosition, value);
    }

    public static string GitGrepUserArguments
    {
        get => Setting.GetValue(_gitGrepUserArguments);
        set => Setting.SetValue(_gitGrepUserArguments, value);
    }

    public static bool GitGrepIgnoreCase
    {
        get => Setting.GetValue(_gitGrepIgnoreCase);
        set => Setting.SetValue(_gitGrepIgnoreCase, value);
    }

    public static bool GitGrepMatchWholeWord
    {
        get => Setting.GetValue(_gitGrepMatchWholeWord);
        set => Setting.SetValue(_gitGrepMatchWholeWord, value);
    }

    [MaybeNull]
    public static string RecentWorkingDir
    {
        get => GetString("RecentWorkingDir", null);
        set => SetString("RecentWorkingDir", value);
    }

    public static bool StartWithRecentWorkingDir
    {
        get => Setting.GetValue(_startWithRecentWorkingDir);
        set => Setting.SetValue(_startWithRecentWorkingDir, value);
    }

    public static string Plink
    {
        get => GetString("plink", Environment.GetEnvironmentVariable("GITEXT_PLINK") ?? ReadStringRegValue("plink", ""));
        set
        {
            if (value != Environment.GetEnvironmentVariable("GITEXT_PLINK"))
            {
                SetString("plink", value);
            }
        }
    }

    public static string Puttygen
    {
        get => GetString("puttygen", Environment.GetEnvironmentVariable("GITEXT_PUTTYGEN") ?? ReadStringRegValue("puttygen", ""));
        set
        {
            if (value != Environment.GetEnvironmentVariable("GITEXT_PUTTYGEN"))
            {
                SetString("puttygen", value);
            }
        }
    }

    /// <summary>Gets the path to Pageant (SSH auth agent).</summary>
    public static string Pageant
    {
        get => GetString("pageant", Environment.GetEnvironmentVariable("GITEXT_PAGEANT") ?? ReadStringRegValue("pageant", ""));
        set
        {
            if (value != Environment.GetEnvironmentVariable("GITEXT_PAGEANT"))
            {
                SetString("pageant", value);
            }
        }
    }

    public static bool AutoStartPageant
    {
        get => Setting.GetValue(_autoStartPageant);
        set => Setting.SetValue(_autoStartPageant, value);
    }

    public static bool MarkIllFormedLinesInCommitMsg
    {
        get => Setting.GetValue(_markIllFormedLinesInCommitMsg);
        set => Setting.SetValue(_markIllFormedLinesInCommitMsg, value);
    }

    public static bool UseSystemVisualStyle
    {
        get => Setting.GetValue(_useSystemVisualStyle);
        set => Setting.SetValue(_useSystemVisualStyle, value);
    }

    public static ThemeId ThemeId
    {
        get
        {
            return new ThemeId(
                GetString("uitheme_v2", ThemeId.DefaultLight.Name),
                GetBool("uithemeisbuiltin_v2", ThemeId.DefaultLight.IsBuiltin));
        }
        set
        {
            SetString("uitheme_v2", value.Name ?? string.Empty);
            SetBool("uithemeisbuiltin_v2", value.IsBuiltin);
        }
    }

    public static string[] ThemeVariations
    {
        get => Setting.GetRawValue(_themeVariations);
        set => Setting.SetValue(_themeVariations, value);
    }

    #region Fonts

    public static Font FixedWidthFont
    {
        get => GetFont("difffont", new Font("Consolas", 10));
        set => SetFont("difffont", value);
    }

    public static Font CommitFont
    {
        get => GetFont("commitfont", SystemFonts.MessageBoxFont!);
        set => SetFont("commitfont", value);
    }

    public static Font MonospaceFont
    {
        get => GetFont("monospacefont", new Font("Consolas", 9));
        set => SetFont("monospacefont", value);
    }

    public static Font Font
    {
        get => GetFont("font", SystemFonts.MessageBoxFont!);
        set => SetFont("font", value);
    }

    public static Font ConEmuConsoleFont
    {
        get => GetFont("conemuconsolefont", new Font("Consolas", 12));
        set => SetFont("conemuconsolefont", value);
    }

    public static bool ShowEolMarkerAsGlyph
    {
        get => Setting.GetValue(_showEolMarkerAsGlyph);
        set => Setting.SetValue(_showEolMarkerAsGlyph, value);
    }

    #endregion
    public static bool MulticolorBranches
    {
        get => Setting.GetValue(_multicolorBranches);
        set => Setting.SetValue(_multicolorBranches, value);
    }

    public static bool HighlightAuthoredRevisions
    {
        get => Setting.GetValue(_highlightAuthoredRevisions);
        set => Setting.SetValue(_highlightAuthoredRevisions, value);
    }

    public static bool FillRefLabels
    {
        get => Setting.GetValue(_fillRefLabels);
        set => Setting.SetValue(_fillRefLabels, value);
    }

    public static bool MergeGraphLanesHavingCommonParent
    {
        get => Setting.GetValue(_mergeGraphLanesHavingCommonParent);
        set => Setting.SetValue(_mergeGraphLanesHavingCommonParent, value);
    }

    public static bool RenderGraphWithDiagonals
    {
        get => Setting.GetValue(_renderGraphWithDiagonals);
        set => Setting.SetValue(_renderGraphWithDiagonals, value);
    }

    public static bool StraightenGraphDiagonals
    {
        get => Setting.GetValue(_straightenGraphDiagonals);
        set => Setting.SetValue(_straightenGraphDiagonals, value);
    }

    public static int StraightenGraphSegmentsLimit
    {
        get => Setting.GetValue(_straightenGraphSegmentsLimit);
        set => Setting.SetValue(_straightenGraphSegmentsLimit, value);
    }

    public static string LastFormatPatchDir
    {
        get => Setting.GetValue(_lastFormatPatchDir);
        set => Setting.SetValue(_lastFormatPatchDir, value);
    }

    public static EnumRuntimeSetting<IgnoreWhitespaceKind> IgnoreWhitespaceKind { get; } = new(RootSettingsPath, nameof(IgnoreWhitespaceKind), Settings.IgnoreWhitespaceKind.None);
    public static bool RememberIgnoreWhiteSpacePreference
    {
        get => Setting.GetValue(_rememberIgnoreWhiteSpacePreference);
        set => Setting.SetValue(_rememberIgnoreWhiteSpacePreference, value);
    }

    public static BoolRuntimeSetting ShowNonPrintingChars { get; } = new(RootSettingsPath, nameof(ShowNonPrintingChars), false);
    public static bool RememberShowNonPrintingCharsPreference
    {
        get => Setting.GetValue(_rememberShowNonPrintingCharsPreference);
        set => Setting.SetValue(_rememberShowNonPrintingCharsPreference, value);
    }

    public static BoolRuntimeSetting ShowEntireFile { get; } = new(RootSettingsPath, nameof(ShowEntireFile), false);
    public static bool RememberShowEntireFilePreference
    {
        get => Setting.GetValue(_rememberShowEntireFilePreference);
        set => Setting.SetValue(_rememberShowEntireFilePreference, value);
    }

    /// <summary>
    /// Diff appearance, alternatives to "patch" viewer.
    /// </summary>
    public static EnumRuntimeSetting<DiffDisplayAppearance> DiffDisplayAppearance { get; } = new(RootSettingsPath, nameof(DiffDisplayAppearance), Settings.DiffDisplayAppearance.Patch);
    public static bool RememberDiffDisplayAppearance
    {
        get => Setting.GetValue(_rememberDiffDisplayAppearance);
        set => Setting.SetValue(_rememberDiffDisplayAppearance, value);
    }

    public static int NumberOfContextLines
    {
        get
        {
            const int defaultValue = 3;
            return RememberNumberOfContextLines ? GetInt("NumberOfContextLines", defaultValue) : defaultValue;
        }
        set
        {
            if (RememberNumberOfContextLines)
            {
                SetInt("NumberOfContextLines", value);
            }
        }
    }

    public static bool RememberNumberOfContextLines
    {
        get => Setting.GetValue(_rememberNumberOfContextLines);
        set => Setting.SetValue(_rememberNumberOfContextLines, value);
    }

    public static BoolRuntimeSetting ShowSyntaxHighlightingInDiff { get; } = new(RootSettingsPath, nameof(ShowSyntaxHighlightingInDiff), true);
    public static bool RememberShowSyntaxHighlightingInDiff
    {
        get => Setting.GetValue(_rememberShowSyntaxHighlightingInDiff);
        set => Setting.SetValue(_rememberShowSyntaxHighlightingInDiff, value);
    }

    public static string GetDictionaryDir()
    {
        return Path.Join(GetResourceDir()!, "Dictionaries");
    }

    public static void SaveSettings()
    {
        SaveEncodings();
        try
        {
            SettingsContainer.LockedAction(() =>
            {
                // prepend "Global\" in order to be safe in preparation for non-Windows OS, too
                _globalMutex ??= new Mutex(initiallyOwned: false, name: @$"Global\Mutex{SettingsFilePath.ToPosixPath()}");

                try
                {
                    _globalMutex.WaitOne();
                    SettingsContainer.Save();
                }
                finally
                {
                    _globalMutex.ReleaseMutex();
                }
            });

            Saved?.Invoke();
        }
        catch
        {
        }
    }

    public static void LoadSettings()
    {
        LoadEncodings();

        try
        {
            // Set environment variable
            GitSshHelpers.SetGitSshEnvironmentVariable(SshPath);
            MigrateEditorSettings();
        }
        catch
        {
        }
    }

    public static bool ShowRepoCurrentBranch
    {
        get => Setting.GetValue(_showRepoCurrentBranch);
        set => Setting.SetValue(_showRepoCurrentBranch, value);
    }

    public static string? OwnScripts
    {
        get => GetString("ownScripts", "");
        set => SetString("ownScripts", value ?? "");
    }

    public static int RecursiveSubmodules
    {
        get => Setting.GetValue(_recursiveSubmodules);
        set => Setting.SetValue(_recursiveSubmodules, value);
    }

    public static ShorteningRecentRepoPathStrategy ShorteningRecentRepoPathStrategy
    {
        get => Setting.GetValue(_shorteningRecentRepoPathStrategy);
        set => Setting.SetValue(_shorteningRecentRepoPathStrategy, value);
    }

    public static int MaxTopRepositories
    {
        get => Setting.GetValue(_maxTopRepositories);
        set => Setting.SetValue(_maxTopRepositories, value);
    }

    public static int RecentRepositoriesHistorySize
    {
        get => Setting.GetValue(_recentRepositoriesHistorySize);
        set => Setting.SetValue(_recentRepositoriesHistorySize, value);
    }

    public static bool HideTopRepositoriesFromRecentList
    {
        get => Setting.GetValue(_hideTopRepositoriesFromRecentList);
        set => Setting.SetValue(_hideTopRepositoriesFromRecentList, value);
    }

    public static int RemotesCacheLength
    {
        get => Setting.GetValue(_remotesCacheLength);
    }

    public static int RecentReposComboMinWidth
    {
        get => Setting.GetValue(_recentReposComboMinWidth);
        set => Setting.SetValue(_recentReposComboMinWidth, value);
    }

    [MaybeNull]
    public static string SerializedHotkeys
    {
        get => GetString("SerializedHotkeys", null);
        set => SetString("SerializedHotkeys", value);
    }

    public static bool SortTopRepos
    {
        get => Setting.GetValue(_sortTopRepos);
        set => Setting.SetValue(_sortTopRepos, value);
    }

    public static bool SortRecentRepos
    {
        get => Setting.GetValue(_sortRecentRepos);
        set => Setting.SetValue(_sortRecentRepos, value);
    }

    public static bool DontCommitMerge
    {
        get => Setting.GetValue(_dontCommitMerge);
        set => Setting.SetValue(_dontCommitMerge, value);
    }

    public static int CommitValidationMaxCntCharsFirstLine
    {
        get => Setting.GetValue(_commitValidationMaxCntCharsFirstLine);
        set => Setting.SetValue(_commitValidationMaxCntCharsFirstLine, value);
    }

    public static int CommitValidationMaxCntCharsPerLine
    {
        get => Setting.GetValue(_commitValidationMaxCntCharsPerLine);
        set => Setting.SetValue(_commitValidationMaxCntCharsPerLine, value);
    }

    public static bool CommitValidationSecondLineMustBeEmpty
    {
        get => Setting.GetValue(_commitValidationSecondLineMustBeEmpty);
        set => Setting.SetValue(_commitValidationSecondLineMustBeEmpty, value);
    }

    public static bool CommitValidationIndentAfterFirstLine
    {
        get => Setting.GetValue(_commitValidationIndentAfterFirstLine);
        set => Setting.SetValue(_commitValidationIndentAfterFirstLine, value);
    }

    public static bool CommitValidationAutoWrap
    {
        get => Setting.GetValue(_commitValidationAutoWrap);
        set => Setting.SetValue(_commitValidationAutoWrap, value);
    }

    public static string CommitValidationRegEx
    {
        get => Setting.GetValue(_commitValidationRegEx);
        set => Setting.SetValue(_commitValidationRegEx, value);
    }

    public static string CommitTemplates
    {
        get => Setting.GetValue(_commitTemplates);
        set => Setting.SetValue(_commitTemplates, value);
    }

    public static bool CreateLocalBranchForRemote
    {
        get => Setting.GetValue(_createLocalBranchForRemote);
        set => Setting.SetValue(_createLocalBranchForRemote, value);
    }

    public static bool UseFormCommitMessage
    {
        get => Setting.GetValue(_useFormCommitMessage);
        set => Setting.SetValue(_useFormCommitMessage, value);
    }

    public static bool CommitAutomaticallyAfterCherryPick
    {
        get => Setting.GetValue(_commitAutomaticallyAfterCherryPick);
        set => Setting.SetValue(_commitAutomaticallyAfterCherryPick, value);
    }

    public static bool AddCommitReferenceToCherryPick
    {
        get => Setting.GetValue(_addCommitReferenceToCherryPick);
        set => Setting.SetValue(_addCommitReferenceToCherryPick, value);
    }

    public static DateTime LastUpdateCheck
    {
        get => GetDate("LastUpdateCheck", default);
        set => SetDate("LastUpdateCheck", value);
    }

    public static bool CheckForUpdates
    {
        get => Setting.GetValue(_checkForUpdates);
        set => Setting.SetValue(_checkForUpdates, value);
    }

    public static bool CheckForReleaseCandidates
    {
        get => Setting.GetValue(_checkForReleaseCandidates);
        set => Setting.SetValue(_checkForReleaseCandidates, value);
    }

    public static bool OmitUninterestingDiff
    {
        get => Setting.GetValue(_omitUninterestingDiff);
        set => Setting.SetValue(_omitUninterestingDiff, value);
    }

    public static bool UseConsoleEmulatorForCommands
    {
        get => Setting.GetValue(_useConsoleEmulatorForCommands);
        set => Setting.SetValue(_useConsoleEmulatorForCommands, value);
    }

    public static GitRefsSortBy RefsSortBy
    {
        get => Setting.GetValue(_refsSortBy);
        set => Setting.SetValue(_refsSortBy, value);
    }

    public static GitRefsSortOrder RefsSortOrder
    {
        get => Setting.GetValue(_refsSortOrder);
        set => Setting.SetValue(_refsSortOrder, value);
    }

    public static DiffListSortType DiffListSorting
    {
        get => Setting.GetValue(_diffListSorting);
        set => Setting.SetValue(_diffListSorting, value);
    }

    public static string GetGitExtensionsFullPath()
    {
#if DEBUG
        if (!IsDesignMode)
        {
            bool isExpectedExe =

                // The app's entry point is GitExtensions.exe
                _applicationExecutablePath.EndsWith("GitExtensions.exe", StringComparison.InvariantCultureIgnoreCase) ||

                // Tests are run by testhost.exe
                _applicationExecutablePath.EndsWith("testhost.exe", StringComparison.InvariantCultureIgnoreCase) ||
                _applicationExecutablePath.EndsWith("testhost.x86.exe", StringComparison.InvariantCultureIgnoreCase) ||

                _applicationExecutablePath.EndsWith("ReSharperTestRunner.exe", StringComparison.InvariantCultureIgnoreCase) ||
                _applicationExecutablePath.EndsWith("dotnet.exe", StringComparison.InvariantCultureIgnoreCase) ||

                // Translations
                _applicationExecutablePath.EndsWith("TranslationApp.exe", StringComparison.InvariantCultureIgnoreCase);

            DebugHelpers.Assert(isExpectedExe, $"{_applicationExecutablePath} must point to GitExtensions.exe");
        }
#endif

        return _applicationExecutablePath;
    }

    public static string? GetGitExtensionsDirectory()
    {
        return Path.GetDirectoryName(GetGitExtensionsFullPath());
    }

    private static RegistryKey? _versionIndependentRegKey;

    private static RegistryKey VersionIndependentRegKey
    {
        get
        {
            if (_versionIndependentRegKey is null)
            {
                _versionIndependentRegKey = Registry.CurrentUser.CreateSubKey("Software\\GitExtensions", RegistryKeyPermissionCheck.ReadWriteSubTree);
                Validates.NotNull(_versionIndependentRegKey);
            }

            return _versionIndependentRegKey;
        }
    }

    public static bool RepoObjectsTreeShowBranches
    {
        get => Setting.GetValue(_repoObjectsTreeShowBranches);
        set => Setting.SetValue(_repoObjectsTreeShowBranches, value);
    }

    public static bool RepoObjectsTreeShowRemotes
    {
        get => Setting.GetValue(_repoObjectsTreeShowRemotes);
        set => Setting.SetValue(_repoObjectsTreeShowRemotes, value);
    }

    public static bool RepoObjectsTreeShowTags
    {
        get => Setting.GetValue(_repoObjectsTreeShowTags);
        set => Setting.SetValue(_repoObjectsTreeShowTags, value);
    }

    public static bool RepoObjectsTreeShowStashes
    {
        get => Setting.GetValue(_repoObjectsTreeShowStashes);
        set => Setting.SetValue(_repoObjectsTreeShowStashes, value);
    }

    public static bool RepoObjectsTreeShowSubmodules
    {
        get => Setting.GetValue(_repoObjectsTreeShowSubmodules);
        set => Setting.SetValue(_repoObjectsTreeShowSubmodules, value);
    }

    public static bool RepoObjectsTreeShowWorktrees
    {
        get => Setting.GetValue(_repoObjectsTreeShowWorktrees);
        set => Setting.SetValue(_repoObjectsTreeShowWorktrees, value);
    }

    public static int RepoObjectsTreeBranchesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeBranchesIndex);
        set => Setting.SetValue(_repoObjectsTreeBranchesIndex, value);
    }

    public static int RepoObjectsTreeRemotesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeRemotesIndex);
        set => Setting.SetValue(_repoObjectsTreeRemotesIndex, value);
    }

    public static int RepoObjectsTreeWorktreesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeWorktreesIndex);
        set => Setting.SetValue(_repoObjectsTreeWorktreesIndex, value);
    }

    public static int RepoObjectsTreeTagsIndex
    {
        get => Setting.GetValue(_repoObjectsTreeTagsIndex);
        set => Setting.SetValue(_repoObjectsTreeTagsIndex, value);
    }

    public static int RepoObjectsTreeSubmodulesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeSubmodulesIndex);
        set => Setting.SetValue(_repoObjectsTreeSubmodulesIndex, value);
    }

    public static int RepoObjectsTreeStashesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeStashesIndex);
        set => Setting.SetValue(_repoObjectsTreeStashesIndex, value);
    }

    public static string PrioritizedBranchNames
    {
        get => Setting.GetValue(_prioritizedBranchNames);
        set => Setting.SetValue(_prioritizedBranchNames, value);
    }

    public static string PrioritizedRemoteNames
    {
        get => Setting.GetValue(_prioritizedRemoteNames);
        set => Setting.SetValue(_prioritizedRemoteNames, value);
    }

    public static string PrioritizedBuildServerRemoteNames
    {
        get => Setting.GetValue(_prioritizedBuildServerRemoteNames);
        set => Setting.SetValue(_prioritizedBuildServerRemoteNames, value);
    }

    public static bool BlameDisplayAuthorFirst
    {
        get => Setting.GetValue(_blameDisplayAuthorFirst);
        set => Setting.SetValue(_blameDisplayAuthorFirst, value);
    }

    public static bool BlameShowAuthor
    {
        get => Setting.GetValue(_blameShowAuthor);
        set => Setting.SetValue(_blameShowAuthor, value);
    }

    public static bool BlameShowAuthorDate
    {
        get => Setting.GetValue(_blameShowAuthorDate);
        set => Setting.SetValue(_blameShowAuthorDate, value);
    }

    public static bool BlameShowAuthorTime
    {
        get => Setting.GetValue(_blameShowAuthorTime);
        set => Setting.SetValue(_blameShowAuthorTime, value);
    }

    public static bool BlameShowLineNumbers
    {
        get => Setting.GetValue(_blameShowLineNumbers);
        set => Setting.SetValue(_blameShowLineNumbers, value);
    }

    public static bool BlameShowOriginalFilePath
    {
        get => Setting.GetValue(_blameShowOriginalFilePath);
        set => Setting.SetValue(_blameShowOriginalFilePath, value);
    }

    public static bool BlameShowAuthorAvatar
    {
        get => Setting.GetValue(_blameShowAuthorAvatar);
        set => Setting.SetValue(_blameShowAuthorAvatar, value);
    }

    public static bool AutomaticContinuousScroll
    {
        get => Setting.GetValue(_automaticContinuousScroll);
        set => Setting.SetValue(_automaticContinuousScroll, value);
    }

    public static int AutomaticContinuousScrollDelay
    {
        get => Setting.GetValue(_automaticContinuousScrollDelay);
        set => Setting.SetValue(_automaticContinuousScrollDelay, value);
    }

    public static IEnumerable<string> CustomGenericRemoteNames
    {
        get => Setting.GetRawValue(_customGenericRemoteNames);
    }

    public static bool LogCaptureCallStacks
    {
        get => Setting.GetValue(_logCaptureCallStacks);
        set => Setting.SetValue(_logCaptureCallStacks, value);
    }

    // There is a bug in .NET/.NET Designer that fails to execute Properties.Settings.Default call.
    // Return false whilst we're in the designer.
    public static bool IsPortable() => !IsDesignMode && Properties.Settings.Default.IsPortable;
    public static bool WriteErrorLog
    {
        get => Setting.GetValue(_writeErrorLog);
    }

    public static bool WorkaroundActivateFromMinimize
    {
        get => Setting.GetValue(_workaroundActivateFromMinimize);
    }

    public static bool GitAsyncWhenMinimized
    {
        get => Setting.GetValue(_gitAsyncWhenMinimized);
    }

    public static bool IsEditorSettingsMigrated
    {
        get => Setting.GetValue(_isEditorSettingsMigrated);
        set => Setting.SetValue(_isEditorSettingsMigrated, value);
    }

    public static string UninformativeRepoNameRegex
    {
        get => Setting.GetValue(_uninformativeRepoNameRegex);
        set => Setting.SetValue(_uninformativeRepoNameRegex, value);
    }

    private static IEnumerable<(string name, string? value)> GetSettingsFromRegistry()
    {
        RegistryKey? oldSettings = VersionIndependentRegKey.OpenSubKey("GitExtensions");

        if (oldSettings is null)
        {
            yield break;
        }

        foreach (string name in oldSettings.GetValueNames())
        {
            object? value = oldSettings.GetValue(name, null);

            if (value is not null)
            {
                yield return (name, value.ToString());
            }
        }
    }

    private static void ImportFromRegistry()
    {
        SettingsContainer.SettingsCache.Import(GetSettingsFromRegistry());
    }

    #region Save in settings file

    private const string ObsoleteGetSetMessage =
        "Use ISetting<T> via Setting.Create() instead. See existing ISetting properties in this class for examples.";

    // String
    [Obsolete(ObsoleteGetSetMessage)]
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static string? GetString(string name, string? defaultValue) => SettingsContainer.GetString(name, defaultValue);
    [Obsolete(ObsoleteGetSetMessage)]
    public static void SetString(string name, string value) => SettingsContainer.SetString(name, value);

    // Bool
    [Obsolete(ObsoleteGetSetMessage)]
    public static bool? GetBool(string name) => SettingsContainer.GetBool(name);
    [Obsolete(ObsoleteGetSetMessage)]
    public static bool GetBool(string name, bool defaultValue) => SettingsContainer.GetBool(name, defaultValue);
    [Obsolete(ObsoleteGetSetMessage)]
    public static void SetBool(string name, bool? value) => SettingsContainer.SetBool(name, value);

    // Int
    [Obsolete(ObsoleteGetSetMessage)]
    public static int? GetInt(string name) => SettingsContainer.GetInt(name);
    [Obsolete(ObsoleteGetSetMessage)]
    public static int GetInt(string name, int defaultValue) => SettingsContainer.GetInt(name, defaultValue);
    [Obsolete(ObsoleteGetSetMessage)]
    public static void SetInt(string name, int? value) => SettingsContainer.SetInt(name, value);

    // Float
    [Obsolete(ObsoleteGetSetMessage)]
    public static float GetFloat(string name, float defaultValue) => SettingsContainer.GetFloat(name, defaultValue);

    // Date
    public static DateTime? GetDate(string name) => SettingsContainer.GetDate(name);
    public static DateTime GetDate(string name, DateTime defaultValue) => SettingsContainer.GetDate(name, defaultValue);
    public static void SetDate(string name, DateTime? value) => SettingsContainer.SetDate(name, value);

    // Font
    public static Font GetFont(string name, Font defaultValue) => SettingsContainer.GetFont(name, defaultValue);
    public static void SetFont(string name, Font value) => SettingsContainer.SetFont(name, value);

    [Obsolete("AppSettings is no longer responsible for colors, ThemeModule is. Only used by ThemeMigration.")]
    public static Color GetColor(AppColor name)
    {
        return SettingsContainer.GetColor(name.ToString().ToLowerInvariant() + "color", AppColorDefaults.GetBy(name));
    }

    // Enum
    [Obsolete(ObsoleteGetSetMessage)]
    public static T GetEnum<T>(string name, T defaultValue) where T : struct, Enum => SettingsContainer.GetEnum(name, defaultValue);
    [Obsolete(ObsoleteGetSetMessage)]
    public static void SetEnum<T>(string name, T value) where T : Enum => SettingsContainer.SetEnum(name, value);

    [Obsolete(ObsoleteGetSetMessage)]
    public static T? GetNullableEnum<T>(string name) where T : struct, Enum => ((ISettingsValueGetter)SettingsContainer).GetValue<T>(name);
    [Obsolete(ObsoleteGetSetMessage)]
    public static void SetNullableEnum<T>(string name, T? value) where T : struct, Enum => SettingsContainer.SetValue(name, value?.ToString());
    #endregion

    private static void LoadEncodings()
    {
        void AddEncoding(Encoding e)
        {
            AvailableEncodings[e.WebName] = e;
        }

        void AddEncodingByName(string s)
        {
            try
            {
                AddEncoding(Encoding.GetEncoding(s));
            }
            catch
            {
            }
        }

        string availableEncodings = GetString("AvailableEncodings", "");
        if (string.IsNullOrWhiteSpace(availableEncodings))
        {
            // Default encoding set
            AddEncoding(Encoding.Default);
            AddEncoding(new ASCIIEncoding());
            AddEncoding(new UnicodeEncoding());

            // UTF-7 is no longer supported, see: https://github.com/dotnet/docs/issues/19274
            // AddEncoding(new UTF7Encoding());

            AddEncoding(new UTF8Encoding(false));

            try
            {
                AddEncoding(Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage));
            }
            catch
            {
                // there are CultureInfo values without a code page
            }

            try
            {
                AddEncoding(Encoding.GetEncoding(0));
            }
            catch
            {
                // catch if error retrieving operating system's active code page
            }
        }
        else
        {
            UTF8Encoding utf8 = new(false);
            foreach (string encodingName in availableEncodings.LazySplit(';'))
            {
#pragma warning disable SYSLIB0001 // Type or member is obsolete
                if (encodingName == Encoding.UTF7.WebName)
#pragma warning restore SYSLIB0001 // Type or member is obsolete
                {
                    // UTF-7 is no longer supported, see: https://github.com/dotnet/docs/issues/19274
                    continue;
                }

                // create utf-8 without BOM
                if (encodingName == utf8.WebName)
                {
                    AddEncoding(utf8);
                }

                // default encoding
                else if (encodingName == "Default")
                {
                    AddEncoding(Encoding.Default);
                }

                // add encoding by name
                else
                {
                    AddEncodingByName(encodingName);
                }
            }
        }
    }

    private static void SaveEncodings()
    {
        string availableEncodings = AvailableEncodings.Values.Select(e => e.WebName).Join(";");
        availableEncodings = availableEncodings.Replace(Encoding.Default.WebName, "Default");
        SetString("AvailableEncodings", availableEncodings);
    }

    private static bool? _isDesignMode;

    private static bool IsDesignMode
    {
        get
        {
            if (_isDesignMode is null)
            {
                string processName = Process.GetCurrentProcess().ProcessName.ToLowerInvariant();
                _isDesignMode = processName.Contains("devenv") || processName.Contains("designtoolsserver");
            }

            return _isDesignMode.Value;
        }
    }

    internal static TestAccessor GetTestAccessor() => new();

    internal struct TestAccessor
    {
        public readonly string ApplicationExecutablePath
        {
            get => _applicationExecutablePath;
            set => _applicationExecutablePath = value;
        }

        public readonly Lazy<string?> ApplicationDataPath
        {
            get => AppSettings.ApplicationDataPath;
            set => AppSettings.ApplicationDataPath = value;
        }

        public readonly void ResetDocumentationBaseUrl() => AppSettings._documentationBaseUrl = null;
    }
}

public class AppSettingsPath : SettingsPath
{
    public AppSettingsPath(string pathName) : base(parent: null, pathName)
    {
    }

    public AppSettingsPath(SettingsPath parent, string pathName) : base(parent: null, parent.PathFor(pathName))
    {
    }

    public override string? GetValue(string name)
    {
        return AppSettings.SettingsContainer.GetValue(PathFor(name));
    }

    public override void SetValue(string name, string? value)
    {
        AppSettings.SettingsContainer.SetValue(PathFor(name), value);
    }
}
