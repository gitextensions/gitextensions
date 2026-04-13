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

        MigrateAvatarSettings();
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

    private static readonly ISetting<bool?> _telemetryEnabled = Setting.Create<bool>(RootSettingsPath, "TelemetryEnabled");

    public static bool? TelemetryEnabled
    {
        get => Setting.GetNullableValue(_telemetryEnabled);
        set => Setting.SetValue(_telemetryEnabled, value);
    }

    private static readonly ISetting<bool> _autoNormaliseBranchName = Setting.Create(RootSettingsPath, "AutoNormaliseBranchName", defaultValue: true);

    public static bool AutoNormaliseBranchName
    {
        get => Setting.GetValue(_autoNormaliseBranchName);
        set => Setting.SetValue(_autoNormaliseBranchName, value);
    }

    public static string AutoNormaliseSymbol
    {
        // when persisted "" is treated as null, so use "+" instead
        get
        {
            string value = GetString("AutoNormaliseSymbol", "_");
            return value == "+" ? "" : value;
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = "+";
            }

            SetString("AutoNormaliseSymbol", value);
        }
    }

    public static string FileEditorCommand => @$"""{AppSettings.GetGitExtensionsFullPath()}"" fileeditor";

    private static readonly ISetting<bool> _rememberAmendCommitState = Setting.Create(RootSettingsPath, "RememberAmendCommitState", defaultValue: true);

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

    private static readonly ISetting<int> _fileStatusFindInFilesGitGrepTypeIndex = Setting.Create(FileStatusSettingsPath, nameof(FileStatusFindInFilesGitGrepTypeIndex), 1);

    public static int FileStatusFindInFilesGitGrepTypeIndex
    {
        get => Setting.GetValue(_fileStatusFindInFilesGitGrepTypeIndex);
        set => Setting.SetValue(_fileStatusFindInFilesGitGrepTypeIndex, value);
    }

    private static readonly ISetting<bool> _fileStatusMergeSingleItemWithFolder = Setting.Create(FileStatusSettingsPath, nameof(FileStatusMergeSingleItemWithFolder), false);

    public static bool FileStatusMergeSingleItemWithFolder
    {
        get => Setting.GetValue(_fileStatusMergeSingleItemWithFolder);
        set => Setting.SetValue(_fileStatusMergeSingleItemWithFolder, value);
    }

    private static readonly ISetting<bool> _fileStatusShowGroupNodesInFlatList = Setting.Create(FileStatusSettingsPath, nameof(FileStatusShowGroupNodesInFlatList), false);

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

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<bool> _wslGitEnabled = Setting.Create(RootSettingsPath, "WslGitEnabled", defaultValue: true);

    public static bool WslGitEnabled
    {
        get => Setting.GetValue(_wslGitEnabled);
    }

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<string> _wslCommand = Setting.Create(RootSettingsPath, nameof(WslCommand), "wsl");

    public static string WslCommand
    {
        get => Setting.GetValue(_wslCommand);
    }

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<string> _wslGitCommand = Setting.Create(RootSettingsPath, nameof(WslGitCommand), "git");

    public static string WslGitCommand
    {
        get => Setting.GetValue(_wslGitCommand);
    }

    private static readonly ISetting<bool> _stashKeepIndex = Setting.Create(RootSettingsPath, "stashkeepindex", defaultValue: false);

    public static bool StashKeepIndex
    {
        get => Setting.GetValue(_stashKeepIndex);
        set => Setting.SetValue(_stashKeepIndex, value);
    }

    // History Compatibility: The setting was originally called 'StashConfirmDropShow', and then it was inverted.
    // To maintain compat with existing user settings, negate the stored value.
    private static readonly ISetting<bool> _dontConfirmStashDrop = Setting.CreateIntercepted<bool, bool>(
        RootSettingsPath,
        "stashconfirmdropshow",
        defaultValue: false,
        read: v => !v,
        store: v => !v);

    public static bool DontConfirmStashDrop
    {
        get => Setting.GetValue(_dontConfirmStashDrop);
        set => Setting.SetValue(_dontConfirmStashDrop, value);
    }

    private static readonly ISetting<bool> _applyPatchIgnoreWhitespace = Setting.Create(RootSettingsPath, "applypatchignorewhitespace", defaultValue: false);

    public static bool ApplyPatchIgnoreWhitespace
    {
        get => Setting.GetValue(_applyPatchIgnoreWhitespace);
        set => Setting.SetValue(_applyPatchIgnoreWhitespace, value);
    }

    private static readonly ISetting<bool> _applyPatchSignOff = Setting.Create(RootSettingsPath, "applypatchsignoff", defaultValue: true);

    public static bool ApplyPatchSignOff
    {
        get => Setting.GetValue(_applyPatchSignOff);
        set => Setting.SetValue(_applyPatchSignOff, value);
    }

    // History Compatibility: The settings key has patience in the name for historical reasons
    private static readonly ISetting<bool> _useHistogramDiffAlgorithm = Setting.Create(RootSettingsPath, "usepatiencediffalgorithm", defaultValue: false);

    public static bool UseHistogramDiffAlgorithm
    {
        get => Setting.GetValue(_useHistogramDiffAlgorithm);
        set => Setting.SetValue(_useHistogramDiffAlgorithm, value);
    }

    /// <summary>
    /// Use Git coloring for selected commands
    /// </summary>
    private static readonly ISetting<bool> _useGitColoring = Setting.Create(AppearanceSettingsPath, nameof(UseGitColoring), true);

    public static bool UseGitColoring
    {
        get => Setting.GetValue(_useGitColoring);
        set => Setting.SetValue(_useGitColoring, value);
    }

    /// <summary>
    /// Color the background at changes (invert colors).
    /// </summary>
    private static readonly ISetting<bool> _reverseGitColoring = Setting.Create(AppearanceSettingsPath, nameof(ReverseGitColoring), true);

    public static bool ReverseGitColoring
    {
        get => Setting.GetValue(_reverseGitColoring);
        set => Setting.SetValue(_reverseGitColoring, value);
    }

    private static readonly ISetting<bool> _showErrorsWhenStagingFiles = Setting.Create(RootSettingsPath, "showerrorswhenstagingfiles", defaultValue: true);

    public static bool ShowErrorsWhenStagingFiles
    {
        get => Setting.GetValue(_showErrorsWhenStagingFiles);
        set => Setting.SetValue(_showErrorsWhenStagingFiles, value);
    }

    private static readonly ISetting<bool> _ensureCommitMessageSecondLineEmpty = Setting.Create(RootSettingsPath, "addnewlinetocommitmessagewhenmissing", defaultValue: true);

    public static bool EnsureCommitMessageSecondLineEmpty
    {
        get => Setting.GetValue(_ensureCommitMessageSecondLineEmpty);
        set => Setting.SetValue(_ensureCommitMessageSecondLineEmpty, value);
    }

    private static readonly ISetting<string> _lastCommitMessage = Setting.Create(RootSettingsPath, "lastCommitMessage", "");

    public static string LastCommitMessage
    {
        get => Setting.GetValue(_lastCommitMessage);
        set => Setting.SetValue(_lastCommitMessage, value);
    }

    private static readonly ISetting<int> _commitDialogNumberOfPreviousMessages = Setting.Create(RootSettingsPath, "commitDialogNumberOfPreviousMessages", defaultValue: 6);

    public static int CommitDialogNumberOfPreviousMessages
    {
        get => Setting.GetValue(_commitDialogNumberOfPreviousMessages);
        set => Setting.SetValue(_commitDialogNumberOfPreviousMessages, value);
    }

    private static readonly ISetting<bool> _commitDialogSelectStagedOnEnterMessage = Setting.Create(DialogSettingsPath, nameof(CommitDialogSelectStagedOnEnterMessage), true);

    public static bool CommitDialogSelectStagedOnEnterMessage
    {
        get => Setting.GetValue(_commitDialogSelectStagedOnEnterMessage);
        set => Setting.SetValue(_commitDialogSelectStagedOnEnterMessage, value);
    }

    private static readonly ISetting<bool> _commitDialogShowOnlyMyMessages = Setting.Create(RootSettingsPath, "commitDialogShowOnlyMyMessages", defaultValue: false);

    public static bool CommitDialogShowOnlyMyMessages
    {
        get => Setting.GetValue(_commitDialogShowOnlyMyMessages);
        set => Setting.SetValue(_commitDialogShowOnlyMyMessages, value);
    }

    private static readonly ISetting<bool> _showCommitAndPush = Setting.Create(RootSettingsPath, "showcommitandpush", defaultValue: true);

    public static bool ShowCommitAndPush
    {
        get => Setting.GetValue(_showCommitAndPush);
        set => Setting.SetValue(_showCommitAndPush, value);
    }

    private static readonly ISetting<bool> _showResetWorkTreeChanges = Setting.Create(RootSettingsPath, "showresetunstagedchanges", defaultValue: true);

    public static bool ShowResetWorkTreeChanges
    {
        get => Setting.GetValue(_showResetWorkTreeChanges);
        set => Setting.SetValue(_showResetWorkTreeChanges, value);
    }

    private static readonly ISetting<bool> _showResetAllChanges = Setting.Create(RootSettingsPath, "showresetallchanges", defaultValue: true);

    public static bool ShowResetAllChanges
    {
        get => Setting.GetValue(_showResetAllChanges);
        set => Setting.SetValue(_showResetAllChanges, value);
    }

    private static readonly ISetting<bool> _showConEmuTab = Setting.Create(DetailedSettingsPath, nameof(ShowConEmuTab), true);

    public static bool ShowConEmuTab
    {
        get => Setting.GetValue(_showConEmuTab);
        set => Setting.SetValue(_showConEmuTab, value);
    }

    private const string ConEmuStyleDefault = "Default";
    private const string ConEmuStyleDark = "<Tomorrow Night>";
    private const string ConEmuStyleLight = "<Tomorrow>";

    private static readonly ISetting<string> _conEmuStyle = Setting.Create(DetailedSettingsPath, nameof(ConEmuStyle), ConEmuStyleDefault);

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

    private static readonly ISetting<string> _conEmuTerminal = Setting.Create(DetailedSettingsPath, nameof(ConEmuTerminal), "bash");

    public static string ConEmuTerminal
    {
        get => Setting.GetValue(_conEmuTerminal);
        set => Setting.SetValue(_conEmuTerminal, value);
    }

    private static readonly ISetting<int> _outputHistoryDepth = Setting.Create(DetailedSettingsPath, nameof(OutputHistoryDepth), 20);

    public static int OutputHistoryDepth
    {
        get => Setting.GetValue(_outputHistoryDepth);
        set => Setting.SetValue(_outputHistoryDepth, value);
    }

    private static readonly ISetting<bool> _outputHistoryPanelVisible = Setting.Create(DetailedSettingsPath, nameof(OutputHistoryPanelVisible), false);

    public static bool OutputHistoryPanelVisible
    {
        get => Setting.GetValue(_outputHistoryPanelVisible);
        set => Setting.SetValue(_outputHistoryPanelVisible, value);
    }

    private static readonly ISetting<bool> _showOutputHistoryAsTab = Setting.Create(DetailedSettingsPath, nameof(ShowOutputHistoryAsTab), true);

    public static bool ShowOutputHistoryAsTab
    {
        get => Setting.GetValue(_showOutputHistoryAsTab);
        set => Setting.SetValue(_showOutputHistoryAsTab, value);
    }

    private static readonly ISetting<bool> _useBrowseForFileHistory = Setting.Create(DetailedSettingsPath, nameof(UseBrowseForFileHistory), true);

    public static bool UseBrowseForFileHistory
    {
        get => Setting.GetValue(_useBrowseForFileHistory);
        set => Setting.SetValue(_useBrowseForFileHistory, value);
    }

    private static readonly ISetting<bool> _useDiffViewerForBlame = Setting.Create(DetailedSettingsPath, nameof(UseDiffViewerForBlame), false);

    public static bool UseDiffViewerForBlame
    {
        get => Setting.GetValue(_useDiffViewerForBlame);
        set => Setting.SetValue(_useDiffViewerForBlame, value);
    }

    private static readonly ISetting<bool> _showGpgInformation = Setting.Create(DetailedSettingsPath, nameof(ShowGpgInformation), true);

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

    private static readonly ISetting<bool> _messageEditorWordWrap = Setting.Create(DetailedSettingsPath, nameof(MessageEditorWordWrap), false);

    public static bool MessageEditorWordWrap
    {
        get => Setting.GetValue(_messageEditorWordWrap);
        set => Setting.SetValue(_messageEditorWordWrap, value);
    }

    private static readonly ISetting<bool> _showSplitViewLayout = Setting.Create(DetailedSettingsPath, "ShowSplitViewLayout", defaultValue: true);

    public static bool ShowSplitViewLayout
    {
        get => Setting.GetValue(_showSplitViewLayout);
        set => Setting.SetValue(_showSplitViewLayout, value);
    }

    private static readonly ISetting<bool> _provideAutocompletion = Setting.Create(RootSettingsPath, "provideautocompletion", defaultValue: true);

    public static bool ProvideAutocompletion
    {
        get => Setting.GetValue(_provideAutocompletion);
        set => Setting.SetValue(_provideAutocompletion, value);
    }

    private static readonly ISetting<TruncatePathMethod> _truncatePathMethod = Setting.Create(RootSettingsPath, "truncatepathmethod", defaultValue: TruncatePathMethod.None);

    public static TruncatePathMethod TruncatePathMethod
    {
        get => Setting.GetValue(_truncatePathMethod);
        set => Setting.SetValue(_truncatePathMethod, value);
    }

    private static readonly ISetting<bool> _showGitStatusInBrowseToolbar = Setting.Create(RootSettingsPath, "showgitstatusinbrowsetoolbar", defaultValue: true);

    public static bool ShowGitStatusInBrowseToolbar
    {
        get => Setting.GetValue(_showGitStatusInBrowseToolbar);
        set => Setting.SetValue(_showGitStatusInBrowseToolbar, value);
    }

    private static readonly ISetting<bool> _showGitStatusForArtificialCommits = Setting.Create(RootSettingsPath, "showgitstatusforartificialcommits", defaultValue: true);

    public static bool ShowGitStatusForArtificialCommits
    {
        get => Setting.GetValue(_showGitStatusForArtificialCommits);
        set => Setting.SetValue(_showGitStatusForArtificialCommits, value);
    }

    public static EnumRuntimeSetting<RevisionSortOrder> RevisionSortOrder { get; } = new(RootSettingsPath, nameof(RevisionSortOrder), GitCommands.RevisionSortOrder.GitDefault);

    public static bool CommitInfoShowContainedInBranches => CommitInfoShowContainedInBranchesLocal ||
                                                            CommitInfoShowContainedInBranchesRemote ||
                                                            CommitInfoShowContainedInBranchesRemoteIfNoLocal;

    private static readonly ISetting<bool> _commitInfoShowContainedInBranchesLocal = Setting.Create(RootSettingsPath, "commitinfoshowcontainedinbrancheslocal", defaultValue: true);

    public static bool CommitInfoShowContainedInBranchesLocal
    {
        get => Setting.GetValue(_commitInfoShowContainedInBranchesLocal);
        set => Setting.SetValue(_commitInfoShowContainedInBranchesLocal, value);
    }

    private static readonly ISetting<bool> _checkForUncommittedChangesInCheckoutBranch = Setting.Create(RootSettingsPath, "checkforuncommittedchangesincheckoutbranch", defaultValue: true);

    public static bool CheckForUncommittedChangesInCheckoutBranch
    {
        get => Setting.GetValue(_checkForUncommittedChangesInCheckoutBranch);
        set => Setting.SetValue(_checkForUncommittedChangesInCheckoutBranch, value);
    }

    private static readonly ISetting<bool> _alwaysShowCheckoutBranchDlg = Setting.Create(RootSettingsPath, "AlwaysShowCheckoutBranchDlg", defaultValue: false);

    public static bool AlwaysShowCheckoutBranchDlg
    {
        get => Setting.GetValue(_alwaysShowCheckoutBranchDlg);
        set => Setting.SetValue(_alwaysShowCheckoutBranchDlg, value);
    }

    private static readonly ISetting<bool> _commitAndPushForcedWhenAmend = Setting.Create(RootSettingsPath, "CommitAndPushForcedWhenAmend", defaultValue: false);

    public static bool CommitAndPushForcedWhenAmend
    {
        get => Setting.GetValue(_commitAndPushForcedWhenAmend);
        set => Setting.SetValue(_commitAndPushForcedWhenAmend, value);
    }

    private static readonly ISetting<bool> _commitInfoShowContainedInBranchesRemote = Setting.Create(RootSettingsPath, "commitinfoshowcontainedinbranchesremote", defaultValue: false);

    public static bool CommitInfoShowContainedInBranchesRemote
    {
        get => Setting.GetValue(_commitInfoShowContainedInBranchesRemote);
        set => Setting.SetValue(_commitInfoShowContainedInBranchesRemote, value);
    }

    private static readonly ISetting<bool> _commitInfoShowContainedInBranchesRemoteIfNoLocal = Setting.Create(RootSettingsPath, "commitinfoshowcontainedinbranchesremoteifnolocal", defaultValue: false);

    public static bool CommitInfoShowContainedInBranchesRemoteIfNoLocal
    {
        get => Setting.GetValue(_commitInfoShowContainedInBranchesRemoteIfNoLocal);
        set => Setting.SetValue(_commitInfoShowContainedInBranchesRemoteIfNoLocal, value);
    }

    private static readonly ISetting<bool> _commitInfoShowContainedInTags = Setting.Create(RootSettingsPath, "commitinfoshowcontainedintags", defaultValue: true);

    public static bool CommitInfoShowContainedInTags
    {
        get => Setting.GetValue(_commitInfoShowContainedInTags);
        set => Setting.SetValue(_commitInfoShowContainedInTags, value);
    }

    private static readonly ISetting<bool> _commitInfoShowTagThisCommitDerivesFrom = Setting.Create(RootSettingsPath, "commitinfoshowtagthiscommitderivesfrom", defaultValue: true);

    public static bool CommitInfoShowTagThisCommitDerivesFrom
    {
        get => Setting.GetValue(_commitInfoShowTagThisCommitDerivesFrom);
        set => Setting.SetValue(_commitInfoShowTagThisCommitDerivesFrom, value);
    }

    #region Avatars

    public static string AvatarImageCachePath => Path.Join(LocalApplicationDataPath.Value!, "Images\\");

    public static AvatarFallbackType AvatarFallbackType
    {
        get => GetEnumViaString("GravatarDefaultImageType", AvatarFallbackType.AuthorInitials);
        set => SetString("GravatarDefaultImageType", value.ToString());
    }

    private static readonly ISetting<string> _customAvatarTemplate = Setting.Create(RootSettingsPath, "CustomAvatarTemplate", string.Empty);

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

    private static readonly ISetting<int> _avatarImageCacheDays = Setting.Create(RootSettingsPath, "authorimagecachedays", defaultValue: 13);

    public static int AvatarImageCacheDays
    {
        get => Setting.GetValue(_avatarImageCacheDays);
        set => Setting.SetValue(_avatarImageCacheDays, value);
    }

    private static readonly ISetting<bool> _showAuthorAvatarInCommitInfo = Setting.Create(RootSettingsPath, "showauthorgravatar", defaultValue: true);

    public static bool ShowAuthorAvatarInCommitInfo
    {
        get => Setting.GetValue(_showAuthorAvatarInCommitInfo);
        set => Setting.SetValue(_showAuthorAvatarInCommitInfo, value);
    }

    public static AvatarProvider AvatarProvider
    {
        get => GetEnumViaString("Appearance.AvatarProvider", AvatarProvider.None);
        set => SetString("Appearance.AvatarProvider", value.ToString());
    }

    private static readonly ISetting<int> _avatarCacheSize = Setting.Create(RootSettingsPath, "Appearance.AvatarCacheSize", defaultValue: 200);

    public static int AvatarCacheSize
    {
        get => Setting.GetValue(_avatarCacheSize);
        set => Setting.SetValue(_avatarCacheSize, value);
    }

    // Currently not configurable in UI (Set manually in settings file)
    // Names from here: https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.brushes?view=windowsdesktop-7.0
    // or #AARRGGBB code
    private static readonly ISetting<string> _avatarAuthorInitialsPalette = Setting.Create(RootSettingsPath, "Appearance.AvatarAuthorInitialsPalette", "SlateGray,RoyalBlue,Purple,OrangeRed,Teal,OliveDrab");

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
    private static TEnum GetEnumViaString<TEnum>(string settingName, TEnum defaultValue)
        where TEnum : struct
    {
        string? settingStringValue = GetString(settingName, defaultValue.ToString());

        if (Enum.TryParse(settingStringValue, out TEnum settingEnumValue))
        {
            return settingEnumValue;
        }

        return defaultValue;
    }

    private static void MigrateAvatarSettings()
    {
        // Load settings as strings to support obsolete settings that are no longer
        // part of the enums AvatarProvider or AvatarFallbackType.

        string provider = GetString("Appearance.AvatarProvider", "Default");

        // if the provider turns out to be the obsolete "author initials" we can skip loading
        // the fallback image, because it will be overwritten anyways, so loading it is postponed.
        string fallbackImage;

        bool providerChanged = false;
        bool fallbackImageChanged = false;

        if (provider == "AuthorInitials")
        {
            provider = AvatarProvider.None.ToString();
            fallbackImage = AvatarFallbackType.AuthorInitials.ToString();

            providerChanged = true;
            fallbackImageChanged = true;
        }
        else
        {
            if (provider == "Gravatar")
            {
                provider = AvatarProvider.Default.ToString();
                providerChanged = true;
            }

            // if provider was not "AuthorInitials" the fallback image
            // is loaded to check if it has to be migrated.
            fallbackImage = GetString("GravatarDefaultImageType", "AuthorInitials");

            if (fallbackImage == "None")
            {
                fallbackImage = AvatarFallbackType.AuthorInitials.ToString();
                fallbackImageChanged = true;
            }
        }

        if (providerChanged)
        {
            SetString("Appearance.AvatarProvider", provider);
        }

        if (fallbackImageChanged)
        {
            SetString("GravatarDefaultImageType", fallbackImage);
        }

        if (providerChanged || fallbackImageChanged)
        {
            SaveSettings();
        }
    }

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

    private static readonly ISetting<string> _translation = Setting.Create(RootSettingsPath, "translation", "");

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

    private static readonly ISetting<bool> _userProfileHomeDir = Setting.Create(RootSettingsPath, "userprofilehomedir", defaultValue: false);

    public static bool UserProfileHomeDir
    {
        get => Setting.GetValue(_userProfileHomeDir);
        set => Setting.SetValue(_userProfileHomeDir, value);
    }

    private static readonly ISetting<string> _customHomeDir = Setting.Create(RootSettingsPath, "customhomedir", "");

    public static string CustomHomeDir
    {
        get => Setting.GetValue(_customHomeDir);
        set => Setting.SetValue(_customHomeDir, value);
    }

    private static readonly ISetting<bool> _enableAutoScale = Setting.Create(RootSettingsPath, "enableautoscale", defaultValue: true);

    public static bool EnableAutoScale
    {
        get => Setting.GetValue(_enableAutoScale);
        set => Setting.SetValue(_enableAutoScale, value);
    }

    private static readonly ISetting<bool> _closeCommitDialogAfterCommit = Setting.Create(RootSettingsPath, "closecommitdialogaftercommit", defaultValue: true);

    public static bool CloseCommitDialogAfterCommit
    {
        get => Setting.GetValue(_closeCommitDialogAfterCommit);
        set => Setting.SetValue(_closeCommitDialogAfterCommit, value);
    }

    private static readonly ISetting<bool> _closeCommitDialogAfterLastCommit = Setting.Create(RootSettingsPath, "closecommitdialogafterlastcommit", defaultValue: true);

    public static bool CloseCommitDialogAfterLastCommit
    {
        get => Setting.GetValue(_closeCommitDialogAfterLastCommit);
        set => Setting.SetValue(_closeCommitDialogAfterLastCommit, value);
    }

    private static readonly ISetting<bool> _refreshArtificialCommitOnApplicationActivated = Setting.Create(RootSettingsPath, "refreshcommitdialogonformfocus", defaultValue: false);

    public static bool RefreshArtificialCommitOnApplicationActivated
    {
        get => Setting.GetValue(_refreshArtificialCommitOnApplicationActivated);
        set => Setting.SetValue(_refreshArtificialCommitOnApplicationActivated, value);
    }

    private static readonly ISetting<bool> _stageInSuperprojectAfterCommit = Setting.Create(RootSettingsPath, "stageinsuperprojectaftercommit", defaultValue: true);

    public static bool StageInSuperprojectAfterCommit
    {
        get => Setting.GetValue(_stageInSuperprojectAfterCommit);
        set => Setting.SetValue(_stageInSuperprojectAfterCommit, value);
    }

    private static readonly ISetting<bool> _followRenamesInFileHistory = Setting.Create(RootSettingsPath, "followrenamesinfilehistory", defaultValue: true);

    public static bool FollowRenamesInFileHistory
    {
        get => Setting.GetValue(_followRenamesInFileHistory);
        set => Setting.SetValue(_followRenamesInFileHistory, value);
    }

    private static readonly ISetting<bool> _followRenamesInFileHistoryExactOnly = Setting.Create(RootSettingsPath, "followrenamesinfilehistoryexactonly", defaultValue: false);

    public static bool FollowRenamesInFileHistoryExactOnly
    {
        get => Setting.GetValue(_followRenamesInFileHistoryExactOnly);
        set => Setting.SetValue(_followRenamesInFileHistoryExactOnly, value);
    }

    private static readonly ISetting<bool> _fullHistoryInFileHistory = Setting.Create(RootSettingsPath, "fullhistoryinfilehistory", defaultValue: false);

    public static bool FullHistoryInFileHistory
    {
        get => Setting.GetValue(_fullHistoryInFileHistory);
        set => Setting.SetValue(_fullHistoryInFileHistory, value);
    }

    private static readonly ISetting<bool> _simplifyMergesInFileHistory = Setting.Create(RootSettingsPath, "simplifymergesinfileHistory", defaultValue: false);

    public static bool SimplifyMergesInFileHistory
    {
        get => Setting.GetValue(_simplifyMergesInFileHistory);
        set => Setting.SetValue(_simplifyMergesInFileHistory, value);
    }

    private static readonly ISetting<bool> _loadFileHistoryOnShow = Setting.Create(RootSettingsPath, "LoadFileHistoryOnShow", defaultValue: true);

    public static bool LoadFileHistoryOnShow
    {
        get => Setting.GetValue(_loadFileHistoryOnShow);
        set => Setting.SetValue(_loadFileHistoryOnShow, value);
    }

    private static readonly ISetting<bool> _loadBlameOnShow = Setting.Create(RootSettingsPath, "LoadBlameOnShow", defaultValue: true);

    public static bool LoadBlameOnShow
    {
        get => Setting.GetValue(_loadBlameOnShow);
        set => Setting.SetValue(_loadBlameOnShow, value);
    }

    private static readonly ISetting<bool> _detectCopyInFileOnBlame = Setting.Create(RootSettingsPath, "DetectCopyInFileOnBlame", defaultValue: false);

    public static bool DetectCopyInFileOnBlame
    {
        get => Setting.GetValue(_detectCopyInFileOnBlame);
        set => Setting.SetValue(_detectCopyInFileOnBlame, value);
    }

    private static readonly ISetting<bool> _detectCopyInAllOnBlame = Setting.Create(RootSettingsPath, "DetectCopyInAllOnBlame", defaultValue: false);

    public static bool DetectCopyInAllOnBlame
    {
        get => Setting.GetValue(_detectCopyInAllOnBlame);
        set => Setting.SetValue(_detectCopyInAllOnBlame, value);
    }

    private static readonly ISetting<bool> _ignoreWhitespaceOnBlame = Setting.Create(RootSettingsPath, "IgnoreWhitespaceOnBlame", defaultValue: true);

    public static bool IgnoreWhitespaceOnBlame
    {
        get => Setting.GetValue(_ignoreWhitespaceOnBlame);
        set => Setting.SetValue(_ignoreWhitespaceOnBlame, value);
    }

    private static readonly ISetting<bool> _openSubmoduleDiffInSeparateWindow = Setting.Create(RootSettingsPath, "opensubmodulediffinseparatewindow", defaultValue: false);

    public static bool OpenSubmoduleDiffInSeparateWindow
    {
        get => Setting.GetValue(_openSubmoduleDiffInSeparateWindow);
        set => Setting.SetValue(_openSubmoduleDiffInSeparateWindow, value);
    }

    /// <summary>
    /// Gets or sets whether to show artificial commits in the revision graph.
    /// </summary>
    private static readonly ISetting<bool> _revisionGraphShowArtificialCommits = Setting.Create(RootSettingsPath, "revisiongraphshowworkingdirchanges", defaultValue: true);

    public static bool RevisionGraphShowArtificialCommits
    {
        get => Setting.GetValue(_revisionGraphShowArtificialCommits);
        set => Setting.SetValue(_revisionGraphShowArtificialCommits, value);
    }

    private static readonly ISetting<bool> _revisionGraphDrawAlternateBackColor = Setting.Create(RootSettingsPath, "RevisionGraphDrawAlternateBackColor", defaultValue: true);

    public static bool RevisionGraphDrawAlternateBackColor
    {
        get => Setting.GetValue(_revisionGraphDrawAlternateBackColor);
        set => Setting.SetValue(_revisionGraphDrawAlternateBackColor, value);
    }

    private static readonly ISetting<bool> _revisionGraphDrawNonRelativesGray = Setting.Create(RootSettingsPath, "revisiongraphdrawnonrelativesgray", defaultValue: true);

    public static bool RevisionGraphDrawNonRelativesGray
    {
        get => Setting.GetValue(_revisionGraphDrawNonRelativesGray);
        set => Setting.SetValue(_revisionGraphDrawNonRelativesGray, value);
    }

    private static readonly ISetting<bool> _revisionGraphDrawNonRelativesTextGray = Setting.Create(RootSettingsPath, "revisiongraphdrawnonrelativestextgray", defaultValue: false);

    public static bool RevisionGraphDrawNonRelativesTextGray
    {
        get => Setting.GetValue(_revisionGraphDrawNonRelativesTextGray);
        set => Setting.SetValue(_revisionGraphDrawNonRelativesTextGray, value);
    }

    public static readonly Dictionary<string, Encoding> AvailableEncodings = [];

    /// <summary>
    /// Gets or sets the default pull action that is performed by the toolbar icon when it is clicked on.
    /// </summary>
    private static readonly ISetting<GitPullAction> _defaultPullAction = Setting.Create(RootSettingsPath, "DefaultPullAction", defaultValue: GitPullAction.Merge);

    public static GitPullAction DefaultPullAction
    {
        get => Setting.GetValue(_defaultPullAction);
        set => Setting.SetValue(_defaultPullAction, value);
    }

    /// <summary>
    /// Gets or sets the default pull action as configured in the FormPull dialog.
    /// </summary>
    private static readonly ISetting<GitPullAction> _formPullAction = Setting.Create(RootSettingsPath, "FormPullAction", defaultValue: GitPullAction.Merge);

    public static GitPullAction FormPullAction
    {
        get => Setting.GetValue(_formPullAction);
        set => Setting.SetValue(_formPullAction, value);
    }

    private static readonly ISetting<bool> _autoStash = Setting.Create(RootSettingsPath, "autostash", defaultValue: false);

    public static bool AutoStash
    {
        get => Setting.GetValue(_autoStash);
        set => Setting.SetValue(_autoStash, value);
    }

    private static readonly ISetting<bool> _rebaseAutoStash = Setting.Create(RootSettingsPath, "RebaseAutostash", defaultValue: false);

    public static bool RebaseAutoStash
    {
        get => Setting.GetValue(_rebaseAutoStash);
        set => Setting.SetValue(_rebaseAutoStash, value);
    }

    private static readonly ISetting<LocalChangesAction> _checkoutBranchAction = Setting.Create(RootSettingsPath, "checkoutbranchaction", defaultValue: LocalChangesAction.DontChange);

    public static LocalChangesAction CheckoutBranchAction
    {
        get => Setting.GetValue(_checkoutBranchAction);
        set => Setting.SetValue(_checkoutBranchAction, value);
    }

    private static readonly ISetting<bool> _checkoutOtherBranchAfterReset = Setting.Create(DialogSettingsPath, nameof(CheckoutOtherBranchAfterReset), defaultValue: true);

    public static bool CheckoutOtherBranchAfterReset
    {
        get => Setting.GetValue(_checkoutOtherBranchAfterReset);
        set => Setting.SetValue(_checkoutOtherBranchAfterReset, value);
    }

    private static readonly ISetting<bool> _useDefaultCheckoutBranchAction = Setting.Create(RootSettingsPath, "UseDefaultCheckoutBranchAction", defaultValue: false);

    public static bool UseDefaultCheckoutBranchAction
    {
        get => Setting.GetValue(_useDefaultCheckoutBranchAction);
        set => Setting.SetValue(_useDefaultCheckoutBranchAction, value);
    }

    private static readonly ISetting<bool> _dontShowHelpImages = Setting.Create(RootSettingsPath, "DontShowHelpImages", defaultValue: false);

    public static bool DontShowHelpImages
    {
        get => Setting.GetValue(_dontShowHelpImages);
        set => Setting.SetValue(_dontShowHelpImages, value);
    }

    private static readonly ISetting<bool> _alwaysShowAdvOpt = Setting.Create(RootSettingsPath, "AlwaysShowAdvOpt", defaultValue: false);

    public static bool AlwaysShowAdvOpt
    {
        get => Setting.GetValue(_alwaysShowAdvOpt);
        set => Setting.SetValue(_alwaysShowAdvOpt, value);
    }

    private static readonly ISetting<bool> _dontConfirmAmend = Setting.Create(RootSettingsPath, "DontConfirmAmend", defaultValue: false);

    public static bool DontConfirmAmend
    {
        get => Setting.GetValue(_dontConfirmAmend);
        set => Setting.SetValue(_dontConfirmAmend, value);
    }

    private static readonly ISetting<bool> _dontConfirmDeleteUnmergedBranch = Setting.Create(RootSettingsPath, "DontConfirmDeleteUnmergedBranch", defaultValue: false);

    public static bool DontConfirmDeleteUnmergedBranch
    {
        get => Setting.GetValue(_dontConfirmDeleteUnmergedBranch);
        set => Setting.SetValue(_dontConfirmDeleteUnmergedBranch, value);
    }

    private static readonly ISetting<bool> _dontConfirmCommitIfNoBranch = Setting.Create(RootSettingsPath, "DontConfirmCommitIfNoBranch", defaultValue: false);

    public static bool DontConfirmCommitIfNoBranch
    {
        get => Setting.GetValue(_dontConfirmCommitIfNoBranch);
        set => Setting.SetValue(_dontConfirmCommitIfNoBranch, value);
    }

    private static readonly ISetting<bool> _confirmBranchCheckout = Setting.Create(ConfirmationsSettingsPath, nameof(ConfirmBranchCheckout), false);

    public static bool ConfirmBranchCheckout
    {
        get => Setting.GetValue(_confirmBranchCheckout);
        set => Setting.SetValue(_confirmBranchCheckout, value);
    }

    private static readonly ISetting<bool?> _autoPopStashAfterPull = Setting.Create<bool>(RootSettingsPath, "AutoPopStashAfterPull");

    public static bool? AutoPopStashAfterPull
    {
        get => Setting.GetNullableValue(_autoPopStashAfterPull);
        set => Setting.SetValue(_autoPopStashAfterPull, value);
    }

    private static readonly ISetting<bool?> _autoPopStashAfterCheckoutBranch = Setting.Create<bool>(RootSettingsPath, "AutoPopStashAfterCheckoutBranch");

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

    private static readonly ISetting<bool> _dontConfirmPushNewBranch = Setting.Create(RootSettingsPath, "DontConfirmPushNewBranch", defaultValue: false);

    public static bool DontConfirmPushNewBranch
    {
        get => Setting.GetValue(_dontConfirmPushNewBranch);
        set => Setting.SetValue(_dontConfirmPushNewBranch, value);
    }

    private static readonly ISetting<bool> _dontConfirmAddTrackingRef = Setting.Create(RootSettingsPath, "DontConfirmAddTrackingRef", defaultValue: false);

    public static bool DontConfirmAddTrackingRef
    {
        get => Setting.GetValue(_dontConfirmAddTrackingRef);
        set => Setting.SetValue(_dontConfirmAddTrackingRef, value);
    }

    private static readonly ISetting<bool> _dontConfirmCommitAfterConflictsResolved = Setting.Create(RootSettingsPath, "DontConfirmCommitAfterConflictsResolved", defaultValue: false);

    public static bool DontConfirmCommitAfterConflictsResolved
    {
        get => Setting.GetValue(_dontConfirmCommitAfterConflictsResolved);
        set => Setting.SetValue(_dontConfirmCommitAfterConflictsResolved, value);
    }

    private static readonly ISetting<bool> _dontConfirmSecondAbortConfirmation = Setting.Create(RootSettingsPath, "DontConfirmSecondAbortConfirmation", defaultValue: false);

    public static bool DontConfirmSecondAbortConfirmation
    {
        get => Setting.GetValue(_dontConfirmSecondAbortConfirmation);
        set => Setting.SetValue(_dontConfirmSecondAbortConfirmation, value);
    }

    private static readonly ISetting<bool> _dontConfirmRebase = Setting.Create(RootSettingsPath, "DontConfirmRebase", defaultValue: false);

    public static bool DontConfirmRebase
    {
        get => Setting.GetValue(_dontConfirmRebase);
        set => Setting.SetValue(_dontConfirmRebase, value);
    }

    private static readonly ISetting<bool> _dontConfirmResolveConflicts = Setting.Create(RootSettingsPath, "DontConfirmResolveConflicts", defaultValue: false);

    public static bool DontConfirmResolveConflicts
    {
        get => Setting.GetValue(_dontConfirmResolveConflicts);
        set => Setting.SetValue(_dontConfirmResolveConflicts, value);
    }

    private static readonly ISetting<bool> _dontConfirmUndoLastCommit = Setting.Create(RootSettingsPath, "DontConfirmUndoLastCommit", defaultValue: false);

    public static bool DontConfirmUndoLastCommit
    {
        get => Setting.GetValue(_dontConfirmUndoLastCommit);
        set => Setting.SetValue(_dontConfirmUndoLastCommit, value);
    }

    private static readonly ISetting<bool> _dontConfirmFetchAndPruneAll = Setting.Create(RootSettingsPath, "DontConfirmFetchAndPruneAll", defaultValue: false);

    public static bool DontConfirmFetchAndPruneAll
    {
        get => Setting.GetValue(_dontConfirmFetchAndPruneAll);
        set => Setting.SetValue(_dontConfirmFetchAndPruneAll, value);
    }

    private static readonly ISetting<bool> _dontConfirmSwitchWorktree = Setting.Create(RootSettingsPath, "DontConfirmSwitchWorktree", defaultValue: false);

    public static bool DontConfirmSwitchWorktree
    {
        get => Setting.GetValue(_dontConfirmSwitchWorktree);
        set => Setting.SetValue(_dontConfirmSwitchWorktree, value);
    }

    private static readonly ISetting<bool> _includeUntrackedFilesInAutoStash = Setting.Create(RootSettingsPath, "includeUntrackedFilesInAutoStash", defaultValue: false);

    public static bool IncludeUntrackedFilesInAutoStash
    {
        get => Setting.GetValue(_includeUntrackedFilesInAutoStash);
        set => Setting.SetValue(_includeUntrackedFilesInAutoStash, value);
    }

    private static readonly ISetting<bool> _includeUntrackedFilesInManualStash = Setting.Create(RootSettingsPath, "includeUntrackedFilesInManualStash", defaultValue: false);

    public static bool IncludeUntrackedFilesInManualStash
    {
        get => Setting.GetValue(_includeUntrackedFilesInManualStash);
        set => Setting.SetValue(_includeUntrackedFilesInManualStash, value);
    }

    private static readonly ISetting<bool> _showRemoteBranches = Setting.Create(RootSettingsPath, "showRemoteBranches", defaultValue: true);

    public static bool ShowRemoteBranches
    {
        get => Setting.GetValue(_showRemoteBranches);
        set => Setting.SetValue(_showRemoteBranches, value);
    }

    public static BoolRuntimeSetting ShowReflogReferences { get; } = new(RootSettingsPath, nameof(ShowReflogReferences), false);

    private static readonly ISetting<bool> _showStashes = Setting.Create(RootSettingsPath, "showStashes", defaultValue: true);

    public static bool ShowStashes
    {
        get => Setting.GetValue(_showStashes);
        set => Setting.SetValue(_showStashes, value);
    }

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<int> _maxStashesWithUntrackedFiles = Setting.Create(RootSettingsPath, "maxStashesWithUntrackedFiles", defaultValue: 10);

    public static int MaxStashesWithUntrackedFiles
    {
        get => Setting.GetValue(_maxStashesWithUntrackedFiles);
    }

    private static readonly ISetting<bool> _showSuperprojectTags = Setting.Create(RootSettingsPath, "showSuperprojectTags", defaultValue: false);

    public static bool ShowSuperprojectTags
    {
        get => Setting.GetValue(_showSuperprojectTags);
        set => Setting.SetValue(_showSuperprojectTags, value);
    }

    private static readonly ISetting<bool> _showSuperprojectBranches = Setting.Create(RootSettingsPath, "showSuperprojectBranches", defaultValue: true);

    public static bool ShowSuperprojectBranches
    {
        get => Setting.GetValue(_showSuperprojectBranches);
        set => Setting.SetValue(_showSuperprojectBranches, value);
    }

    private static readonly ISetting<bool> _showSuperprojectRemoteBranches = Setting.Create(RootSettingsPath, "showSuperprojectRemoteBranches", defaultValue: false);

    public static bool ShowSuperprojectRemoteBranches
    {
        get => Setting.GetValue(_showSuperprojectRemoteBranches);
        set => Setting.SetValue(_showSuperprojectRemoteBranches, value);
    }

    private static readonly ISetting<bool?> _updateSubmodulesOnCheckout = Setting.Create<bool>(RootSettingsPath, "updateSubmodulesOnCheckout");

    public static bool? UpdateSubmodulesOnCheckout
    {
        get => Setting.GetNullableValue(_updateSubmodulesOnCheckout);
        set => Setting.SetValue(_updateSubmodulesOnCheckout, value);
    }

    private static readonly ISetting<bool?> _dontConfirmUpdateSubmodulesOnCheckout = Setting.Create<bool>(RootSettingsPath, "dontConfirmUpdateSubmodulesOnCheckout");

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

    private static readonly ISetting<bool> _showGitCommandLine = Setting.Create(RootSettingsPath, "showgitcommandline", defaultValue: false);

    public static bool ShowGitCommandLine
    {
        get => Setting.GetValue(_showGitCommandLine);
        set => Setting.SetValue(_showGitCommandLine, value);
    }

    private static readonly ISetting<bool> _showStashCount = Setting.Create(RootSettingsPath, "showstashcount", defaultValue: false);

    public static bool ShowStashCount
    {
        get => Setting.GetValue(_showStashCount);
        set => Setting.SetValue(_showStashCount, value);
    }

    private static readonly ISetting<bool> _showAheadBehindData = Setting.Create(RootSettingsPath, "showaheadbehinddata", defaultValue: true);

    public static bool ShowAheadBehindData
    {
        get => Setting.GetValue(_showAheadBehindData);
        set => Setting.SetValue(_showAheadBehindData, value);
    }

    private static readonly ISetting<bool> _showSubmoduleStatus = Setting.Create(RootSettingsPath, "showsubmodulestatus", defaultValue: false);

    public static bool ShowSubmoduleStatus
    {
        get => Setting.GetValue(_showSubmoduleStatus);
        set => Setting.SetValue(_showSubmoduleStatus, value);
    }

    private static readonly ISetting<bool> _relativeDate = Setting.Create(RootSettingsPath, "relativedate", defaultValue: true);

    public static bool RelativeDate
    {
        get => Setting.GetValue(_relativeDate);
        set => Setting.SetValue(_relativeDate, value);
    }

    private static readonly ISetting<bool> _showGitNotes = Setting.Create(RootSettingsPath, "showgitnotes", defaultValue: false);

    public static bool ShowGitNotes
    {
        get => Setting.GetValue(_showGitNotes);
        set => Setting.SetValue(_showGitNotes, value);
    }

    private static readonly ISetting<bool> _showSessionRefs = Setting.Create(RootSettingsPath, "showSessionRefs", defaultValue: false);

    public static bool ShowSessionRefs
    {
        get => Setting.GetValue(_showSessionRefs);
        set => Setting.SetValue(_showSessionRefs, value);
    }

    private static readonly ISetting<bool> _showGitNotesColumn = Setting.Create(AppearanceSettingsPath, nameof(ShowGitNotesColumn), false);

    public static bool ShowGitNotesColumn
    {
        get => Setting.GetValue(_showGitNotesColumn);
        set => Setting.SetValue(_showGitNotesColumn, value);
    }

    private static readonly ISetting<bool> _showAnnotatedTagsMessages = Setting.Create(RootSettingsPath, "showannotatedtagsmessages", defaultValue: true);

    public static bool ShowAnnotatedTagsMessages
    {
        get => Setting.GetValue(_showAnnotatedTagsMessages);
        set => Setting.SetValue(_showAnnotatedTagsMessages, value);
    }

    // History Compatibility: The meaning of this value is changed in the GUI, setting name is kept for compatibility
    private static readonly ISetting<bool> _hideMergeCommits = Setting.CreateIntercepted<bool, bool>(
        RootSettingsPath,
        "showmergecommits",
        defaultValue: false,
        read: v => !v,
        store: v => !v);

    public static bool HideMergeCommits
    {
        get => Setting.GetValue(_hideMergeCommits);
        set => Setting.SetValue(_hideMergeCommits, value);
    }

    private static readonly ISetting<bool> _showTags = Setting.Create(RootSettingsPath, "showtags", defaultValue: true);

    public static bool ShowTags
    {
        get => Setting.GetValue(_showTags);
        set => Setting.SetValue(_showTags, value);
    }

    #region Revision grid column visibilities

    private static readonly ISetting<bool> _showRevisionGridGraphColumn = Setting.Create(RootSettingsPath, "showrevisiongridgraphcolumn", defaultValue: true);

    public static bool ShowRevisionGridGraphColumn
    {
        get => Setting.GetValue(_showRevisionGridGraphColumn);
        set => Setting.SetValue(_showRevisionGridGraphColumn, value);
    }

    private static readonly ISetting<bool> _showAuthorAvatarColumn = Setting.Create(RootSettingsPath, "showrevisiongridauthoravatarcolumn", defaultValue: true);

    public static bool ShowAuthorAvatarColumn
    {
        get => Setting.GetValue(_showAuthorAvatarColumn);
        set => Setting.SetValue(_showAuthorAvatarColumn, value);
    }

    private static readonly ISetting<bool> _showAuthorNameColumn = Setting.Create(RootSettingsPath, "showrevisiongridauthornamecolumn", defaultValue: true);

    public static bool ShowAuthorNameColumn
    {
        get => Setting.GetValue(_showAuthorNameColumn);
        set => Setting.SetValue(_showAuthorNameColumn, value);
    }

    private static readonly ISetting<bool> _showDateColumn = Setting.Create(RootSettingsPath, "showrevisiongriddatecolumn", defaultValue: true);

    public static bool ShowDateColumn
    {
        get => Setting.GetValue(_showDateColumn);
        set => Setting.SetValue(_showDateColumn, value);
    }

    private static readonly ISetting<bool> _showObjectIdColumn = Setting.Create(RootSettingsPath, "showids", defaultValue: true);

    public static bool ShowObjectIdColumn
    {
        get => Setting.GetValue(_showObjectIdColumn);
        set => Setting.SetValue(_showObjectIdColumn, value);
    }

    private static readonly ISetting<bool> _showBuildStatusIconColumn = Setting.Create(RootSettingsPath, "showbuildstatusiconcolumn", defaultValue: true);

    public static bool ShowBuildStatusIconColumn
    {
        get => Setting.GetValue(_showBuildStatusIconColumn);
        set => Setting.SetValue(_showBuildStatusIconColumn, value);
    }

    private static readonly ISetting<bool> _showBuildStatusTextColumn = Setting.Create(RootSettingsPath, "showbuildstatustextcolumn", defaultValue: false);

    public static bool ShowBuildStatusTextColumn
    {
        get => Setting.GetValue(_showBuildStatusTextColumn);
        set => Setting.SetValue(_showBuildStatusTextColumn, value);
    }

    #endregion

    private static readonly ISetting<bool> _showAuthorDate = Setting.Create(RootSettingsPath, "showauthordate", defaultValue: true);

    public static bool ShowAuthorDate
    {
        get => Setting.GetValue(_showAuthorDate);
        set => Setting.SetValue(_showAuthorDate, value);
    }

    private static readonly ISetting<bool> _closeProcessDialog = Setting.Create(RootSettingsPath, "closeprocessdialog", defaultValue: false);

    public static bool CloseProcessDialog
    {
        get => Setting.GetValue(_closeProcessDialog);
        set => Setting.SetValue(_closeProcessDialog, value);
    }

    private static readonly ISetting<bool> _showProcessDialogPasswordInput = Setting.Create(DetailedSettingsPath, nameof(ShowProcessDialogPasswordInput), false);

    public static bool ShowProcessDialogPasswordInput
    {
        get => Setting.GetValue(_showProcessDialogPasswordInput);
        set => Setting.SetValue(_showProcessDialogPasswordInput, value);
    }

    public static BoolRuntimeSetting ShowCurrentBranchOnly { get; } = new(RootSettingsPath, nameof(ShowCurrentBranchOnly), false);

    private static readonly ISetting<bool> _showSimplifyByDecoration = Setting.Create(RootSettingsPath, "showsimplifybydecoration", defaultValue: false);

    public static bool ShowSimplifyByDecoration
    {
        get => Setting.GetValue(_showSimplifyByDecoration);
        set => Setting.SetValue(_showSimplifyByDecoration, value);
    }

    public static BoolRuntimeSetting BranchFilterEnabled { get; } = new(RootSettingsPath, nameof(BranchFilterEnabled), false);

    private static readonly ISetting<bool> _showOnlyFirstParent = Setting.Create(RootSettingsPath, "showfirstparent", defaultValue: false);

    public static bool ShowOnlyFirstParent
    {
        get => Setting.GetValue(_showOnlyFirstParent);
        set => Setting.SetValue(_showOnlyFirstParent, value);
    }

    public static string[] RevisionFilterDropdowns
    {
        get => GetString("RevisionFilterDropdowns", string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        set => SetString("RevisionFilterDropdowns", string.Join("\n", value ?? []));
    }

    private static readonly ISetting<bool> _commitDialogSelectionFilter = Setting.Create(RootSettingsPath, "commitdialogselectionfilter", defaultValue: false);

    public static bool CommitDialogSelectionFilter
    {
        get => Setting.GetValue(_commitDialogSelectionFilter);
        set => Setting.SetValue(_commitDialogSelectionFilter, value);
    }

    private static readonly ISetting<string> _defaultCloneDestinationPath = Setting.Create(RootSettingsPath, "defaultclonedestinationpath", string.Empty);

    public static string DefaultCloneDestinationPath
    {
        get => Setting.GetValue(_defaultCloneDestinationPath);
        set => Setting.SetValue(_defaultCloneDestinationPath, value);
    }

    private static readonly ISetting<int> _revisionGridQuickSearchTimeout = Setting.Create(RootSettingsPath, "revisiongridquicksearchtimeout", defaultValue: 4000);

    public static int RevisionGridQuickSearchTimeout
    {
        get => Setting.GetValue(_revisionGridQuickSearchTimeout);
        set => Setting.SetValue(_revisionGridQuickSearchTimeout, value);
    }

    private static readonly ISetting<bool> _showCommitBodyInRevisionGrid = Setting.Create(RootSettingsPath, "ShowCommitBodyInRevisionGrid", defaultValue: true);

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

    private static readonly ISetting<int> _maxRevisionGraphCommits = Setting.Create(RootSettingsPath, "maxrevisiongraphcommits", defaultValue: 100000);

    public static int MaxRevisionGraphCommits
    {
        get => Setting.GetValue(_maxRevisionGraphCommits);
        set => Setting.SetValue(_maxRevisionGraphCommits, value);
    }

    private static readonly ISetting<bool> _showDiffForAllParents = Setting.Create(RootSettingsPath, "showdiffforallparents", defaultValue: true);

    public static bool ShowDiffForAllParents
    {
        get => Setting.GetValue(_showDiffForAllParents);
        set => Setting.SetValue(_showDiffForAllParents, value);
    }

    private static readonly ISetting<bool> _showFindInCommitFilesGitGrep = Setting.Create(AppearanceSettingsPath, nameof(ShowFindInCommitFilesGitGrep), false);

    public static bool ShowFindInCommitFilesGitGrep
    {
        get => Setting.GetValue(_showFindInCommitFilesGitGrep);
        set => Setting.SetValue(_showFindInCommitFilesGitGrep, value);
    }

    private static readonly ISetting<bool> _showRevisionGridTooltips = Setting.Create(AppearanceSettingsPath, nameof(ShowRevisionGridTooltips), true);

    public static bool ShowRevisionGridTooltips
    {
        get => Setting.GetValue(_showRevisionGridTooltips);
        set => Setting.SetValue(_showRevisionGridTooltips, value);
    }

    private static readonly ISetting<bool> _showAvailableDiffTools = Setting.Create(RootSettingsPath, "difftools.showavailable", defaultValue: true);

    public static bool ShowAvailableDiffTools
    {
        get => Setting.GetValue(_showAvailableDiffTools);
        set => Setting.SetValue(_showAvailableDiffTools, value);
    }

    private static readonly ISetting<int> _diffVerticalRulerPosition = Setting.Create(RootSettingsPath, "diffverticalrulerposition", defaultValue: 0);

    public static int DiffVerticalRulerPosition
    {
        get => Setting.GetValue(_diffVerticalRulerPosition);
        set => Setting.SetValue(_diffVerticalRulerPosition, value);
    }

    private static readonly ISetting<string> _gitGrepUserArguments = Setting.Create(DialogSettingsPath, nameof(GitGrepUserArguments), "");

    public static string GitGrepUserArguments
    {
        get => Setting.GetValue(_gitGrepUserArguments);
        set => Setting.SetValue(_gitGrepUserArguments, value);
    }

    private static readonly ISetting<bool> _gitGrepIgnoreCase = Setting.Create(DialogSettingsPath, nameof(GitGrepIgnoreCase), false);

    public static bool GitGrepIgnoreCase
    {
        get => Setting.GetValue(_gitGrepIgnoreCase);
        set => Setting.SetValue(_gitGrepIgnoreCase, value);
    }

    private static readonly ISetting<bool> _gitGrepMatchWholeWord = Setting.Create(DialogSettingsPath, nameof(GitGrepMatchWholeWord), false);

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

    private static readonly ISetting<bool> _startWithRecentWorkingDir = Setting.Create(RootSettingsPath, "StartWithRecentWorkingDir", defaultValue: false);

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

    private static readonly ISetting<bool> _autoStartPageant = Setting.Create(RootSettingsPath, "autostartpageant", defaultValue: true);

    public static bool AutoStartPageant
    {
        get => Setting.GetValue(_autoStartPageant);
        set => Setting.SetValue(_autoStartPageant, value);
    }

    private static readonly ISetting<bool> _markIllFormedLinesInCommitMsg = Setting.Create(RootSettingsPath, "markillformedlinesincommitmsg", defaultValue: true);

    public static bool MarkIllFormedLinesInCommitMsg
    {
        get => Setting.GetValue(_markIllFormedLinesInCommitMsg);
        set => Setting.SetValue(_markIllFormedLinesInCommitMsg, value);
    }

    private static readonly ISetting<bool> _useSystemVisualStyle = Setting.Create(RootSettingsPath, "systemvisualstyle", defaultValue: true);

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
        get
        {
            return GetString("uithemevariations", string.Empty).Split(Delimiters.Comma, StringSplitOptions.RemoveEmptyEntries);
        }
        set
        {
            SetString("uithemevariations", string.Join(",", value ?? []));
        }
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

    private static readonly ISetting<bool> _showEolMarkerAsGlyph = Setting.Create(RootSettingsPath, "ShowEolMarkerAsGlyph", defaultValue: false);

    public static bool ShowEolMarkerAsGlyph
    {
        get => Setting.GetValue(_showEolMarkerAsGlyph);
        set => Setting.SetValue(_showEolMarkerAsGlyph, value);
    }

    #endregion

    private static readonly ISetting<bool> _multicolorBranches = Setting.Create(RootSettingsPath, "multicolorbranches", defaultValue: true);

    public static bool MulticolorBranches
    {
        get => Setting.GetValue(_multicolorBranches);
        set => Setting.SetValue(_multicolorBranches, value);
    }

    private static readonly ISetting<bool> _highlightAuthoredRevisions = Setting.Create(RootSettingsPath, "highlightauthoredrevisions", defaultValue: true);

    public static bool HighlightAuthoredRevisions
    {
        get => Setting.GetValue(_highlightAuthoredRevisions);
        set => Setting.SetValue(_highlightAuthoredRevisions, value);
    }

    private static readonly ISetting<bool> _fillRefLabels = Setting.Create(RootSettingsPath, "FillRefLabels", defaultValue: false);

    public static bool FillRefLabels
    {
        get => Setting.GetValue(_fillRefLabels);
        set => Setting.SetValue(_fillRefLabels, value);
    }

    private static readonly ISetting<bool> _mergeGraphLanesHavingCommonParent = Setting.Create(RevisionGraphSettingsPath, nameof(MergeGraphLanesHavingCommonParent), true);

    public static bool MergeGraphLanesHavingCommonParent
    {
        get => Setting.GetValue(_mergeGraphLanesHavingCommonParent);
        set => Setting.SetValue(_mergeGraphLanesHavingCommonParent, value);
    }

    private static readonly ISetting<bool> _renderGraphWithDiagonals = Setting.Create(RevisionGraphSettingsPath, nameof(RenderGraphWithDiagonals), true);

    public static bool RenderGraphWithDiagonals
    {
        get => Setting.GetValue(_renderGraphWithDiagonals);
        set => Setting.SetValue(_renderGraphWithDiagonals, value);
    }

    private static readonly ISetting<bool> _straightenGraphDiagonals = Setting.Create(RevisionGraphSettingsPath, nameof(StraightenGraphDiagonals), true);

    public static bool StraightenGraphDiagonals
    {
        get => Setting.GetValue(_straightenGraphDiagonals);
        set => Setting.SetValue(_straightenGraphDiagonals, value);
    }

    /// <summary>
    ///  The limit when to skip the straightening of revision graph segments.
    /// </summary>
    /// <remarks>
    ///  Straightening needs to call the expensive RevisionGraphRow.BuildSegmentLanes function.<br></br>
    ///  Straightening inserts gaps making the graph wider. If it already has to display many segments, i.e. parallel branches, there would be a low benefit of straightening.<br></br>
    ///  So rather skip the - in this case particularly expensive - RevisionGraphRow.BuildSegmentLanes function and call it only if the row is visible.
    /// </remarks>
    private static readonly ISetting<int> _straightenGraphSegmentsLimit = Setting.Create(RevisionGraphSettingsPath, nameof(StraightenGraphSegmentsLimit), 80);

    public static int StraightenGraphSegmentsLimit
    {
        get => Setting.GetValue(_straightenGraphSegmentsLimit);
        set => Setting.SetValue(_straightenGraphSegmentsLimit, value);
    }

    private static readonly ISetting<string> _lastFormatPatchDir = Setting.Create(RootSettingsPath, "lastformatpatchdir", "");

    public static string LastFormatPatchDir
    {
        get => Setting.GetValue(_lastFormatPatchDir);
        set => Setting.SetValue(_lastFormatPatchDir, value);
    }

    public static EnumRuntimeSetting<IgnoreWhitespaceKind> IgnoreWhitespaceKind { get; } = new(RootSettingsPath, nameof(IgnoreWhitespaceKind), Settings.IgnoreWhitespaceKind.None);

    private static readonly ISetting<bool> _rememberIgnoreWhiteSpacePreference = Setting.Create(RootSettingsPath, "rememberIgnoreWhiteSpacePreference", defaultValue: true);

    public static bool RememberIgnoreWhiteSpacePreference
    {
        get => Setting.GetValue(_rememberIgnoreWhiteSpacePreference);
        set => Setting.SetValue(_rememberIgnoreWhiteSpacePreference, value);
    }

    public static BoolRuntimeSetting ShowNonPrintingChars { get; } = new(RootSettingsPath, nameof(ShowNonPrintingChars), false);

    private static readonly ISetting<bool> _rememberShowNonPrintingCharsPreference = Setting.Create(RootSettingsPath, "RememberShowNonPrintableCharsPreference", defaultValue: false);

    public static bool RememberShowNonPrintingCharsPreference
    {
        get => Setting.GetValue(_rememberShowNonPrintingCharsPreference);
        set => Setting.SetValue(_rememberShowNonPrintingCharsPreference, value);
    }

    public static BoolRuntimeSetting ShowEntireFile { get; } = new(RootSettingsPath, nameof(ShowEntireFile), false);

    private static readonly ISetting<bool> _rememberShowEntireFilePreference = Setting.Create(RootSettingsPath, "RememberShowEntireFilePreference", defaultValue: false);

    public static bool RememberShowEntireFilePreference
    {
        get => Setting.GetValue(_rememberShowEntireFilePreference);
        set => Setting.SetValue(_rememberShowEntireFilePreference, value);
    }

    /// <summary>
    /// Diff appearance, alternatives to "patch" viewer.
    /// </summary>
    public static EnumRuntimeSetting<DiffDisplayAppearance> DiffDisplayAppearance { get; } = new(RootSettingsPath, nameof(DiffDisplayAppearance), Settings.DiffDisplayAppearance.Patch);

    /// <summary>
    /// Gets or sets whether to remember the preference for diff appearance.
    /// </summary>
    private static readonly ISetting<bool> _rememberDiffDisplayAppearance = Setting.Create(AppearanceSettingsPath, nameof(RememberDiffDisplayAppearance), false);

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

    private static readonly ISetting<bool> _rememberNumberOfContextLines = Setting.Create(RootSettingsPath, "RememberNumberOfContextLines", defaultValue: false);

    public static bool RememberNumberOfContextLines
    {
        get => Setting.GetValue(_rememberNumberOfContextLines);
        set => Setting.SetValue(_rememberNumberOfContextLines, value);
    }

    public static BoolRuntimeSetting ShowSyntaxHighlightingInDiff { get; } = new(RootSettingsPath, nameof(ShowSyntaxHighlightingInDiff), true);

    private static readonly ISetting<bool> _rememberShowSyntaxHighlightingInDiff = Setting.Create(RootSettingsPath, "RememberShowSyntaxHighlightingInDiff", defaultValue: true);

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

    private static readonly ISetting<bool> _showRepoCurrentBranch = Setting.Create(RootSettingsPath, "dashboardshowcurrentbranch", defaultValue: true);

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

    private static readonly ISetting<int> _recursiveSubmodules = Setting.Create(RootSettingsPath, "RecursiveSubmodules", defaultValue: 1);

    public static int RecursiveSubmodules
    {
        get => Setting.GetValue(_recursiveSubmodules);
        set => Setting.SetValue(_recursiveSubmodules, value);
    }

    private static readonly ISetting<ShorteningRecentRepoPathStrategy> _shorteningRecentRepoPathStrategy = Setting.Create(RootSettingsPath, "ShorteningRecentRepoPathStrategy", defaultValue: ShorteningRecentRepoPathStrategy.None);

    public static ShorteningRecentRepoPathStrategy ShorteningRecentRepoPathStrategy
    {
        get => Setting.GetValue(_shorteningRecentRepoPathStrategy);
        set => Setting.SetValue(_shorteningRecentRepoPathStrategy, value);
    }

    // History Compatibility: Keep original key to maintain the compatibility with the existing user settings
    private static readonly ISetting<int> _maxTopRepositories = Setting.Create(RootSettingsPath, "MaxMostRecentRepositories", defaultValue: 0);

    public static int MaxTopRepositories
    {
        get => Setting.GetValue(_maxTopRepositories);
        set => Setting.SetValue(_maxTopRepositories, value);
    }

    private static readonly ISetting<int> _recentRepositoriesHistorySize = Setting.Create(RootSettingsPath, "history size", defaultValue: 30);

    public static int RecentRepositoriesHistorySize
    {
        get => Setting.GetValue(_recentRepositoriesHistorySize);
        set => Setting.SetValue(_recentRepositoriesHistorySize, value);
    }

    private static readonly ISetting<bool> _hideTopRepositoriesFromRecentList = Setting.Create(RecentRepositories, nameof(HideTopRepositoriesFromRecentList), false);

    public static bool HideTopRepositoriesFromRecentList
    {
        get => Setting.GetValue(_hideTopRepositoriesFromRecentList);
        set => Setting.SetValue(_hideTopRepositoriesFromRecentList, value);
    }

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<int> _remotesCacheLength = Setting.Create(RootSettingsPath, "RemotesCacheLength", defaultValue: 30);

    public static int RemotesCacheLength
    {
        get => Setting.GetValue(_remotesCacheLength);
    }

    private static readonly ISetting<int> _recentReposComboMinWidth = Setting.Create(RootSettingsPath, "RecentReposComboMinWidth", defaultValue: 0);

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

    private static readonly ISetting<bool> _sortTopRepos = Setting.Create(RootSettingsPath, "SortMostRecentRepos", defaultValue: false);

    public static bool SortTopRepos
    {
        get => Setting.GetValue(_sortTopRepos);
        set => Setting.SetValue(_sortTopRepos, value);
    }

    private static readonly ISetting<bool> _sortRecentRepos = Setting.Create(RootSettingsPath, "SortLessRecentRepos", defaultValue: false);

    public static bool SortRecentRepos
    {
        get => Setting.GetValue(_sortRecentRepos);
        set => Setting.SetValue(_sortRecentRepos, value);
    }

    private static readonly ISetting<bool> _dontCommitMerge = Setting.Create(RootSettingsPath, "DontCommitMerge", defaultValue: false);

    public static bool DontCommitMerge
    {
        get => Setting.GetValue(_dontCommitMerge);
        set => Setting.SetValue(_dontCommitMerge, value);
    }

    private static readonly ISetting<int> _commitValidationMaxCntCharsFirstLine = Setting.Create(RootSettingsPath, "CommitValidationMaxCntCharsFirstLine", defaultValue: 0);

    public static int CommitValidationMaxCntCharsFirstLine
    {
        get => Setting.GetValue(_commitValidationMaxCntCharsFirstLine);
        set => Setting.SetValue(_commitValidationMaxCntCharsFirstLine, value);
    }

    private static readonly ISetting<int> _commitValidationMaxCntCharsPerLine = Setting.Create(RootSettingsPath, "CommitValidationMaxCntCharsPerLine", defaultValue: 0);

    public static int CommitValidationMaxCntCharsPerLine
    {
        get => Setting.GetValue(_commitValidationMaxCntCharsPerLine);
        set => Setting.SetValue(_commitValidationMaxCntCharsPerLine, value);
    }

    private static readonly ISetting<bool> _commitValidationSecondLineMustBeEmpty = Setting.Create(RootSettingsPath, "CommitValidationSecondLineMustBeEmpty", defaultValue: false);

    public static bool CommitValidationSecondLineMustBeEmpty
    {
        get => Setting.GetValue(_commitValidationSecondLineMustBeEmpty);
        set => Setting.SetValue(_commitValidationSecondLineMustBeEmpty, value);
    }

    private static readonly ISetting<bool> _commitValidationIndentAfterFirstLine = Setting.Create(RootSettingsPath, "CommitValidationIndentAfterFirstLine", defaultValue: true);

    public static bool CommitValidationIndentAfterFirstLine
    {
        get => Setting.GetValue(_commitValidationIndentAfterFirstLine);
        set => Setting.SetValue(_commitValidationIndentAfterFirstLine, value);
    }

    private static readonly ISetting<bool> _commitValidationAutoWrap = Setting.Create(RootSettingsPath, "CommitValidationAutoWrap", defaultValue: true);

    public static bool CommitValidationAutoWrap
    {
        get => Setting.GetValue(_commitValidationAutoWrap);
        set => Setting.SetValue(_commitValidationAutoWrap, value);
    }

    private static readonly ISetting<string> _commitValidationRegEx = Setting.Create(RootSettingsPath, "CommitValidationRegEx", string.Empty);

    public static string CommitValidationRegEx
    {
        get => Setting.GetValue(_commitValidationRegEx);
        set => Setting.SetValue(_commitValidationRegEx, value);
    }

    private static readonly ISetting<string> _commitTemplates = Setting.Create(RootSettingsPath, "CommitTemplates", string.Empty);

    public static string CommitTemplates
    {
        get => Setting.GetValue(_commitTemplates);
        set => Setting.SetValue(_commitTemplates, value);
    }

    private static readonly ISetting<bool> _createLocalBranchForRemote = Setting.Create(RootSettingsPath, "CreateLocalBranchForRemote", defaultValue: false);

    public static bool CreateLocalBranchForRemote
    {
        get => Setting.GetValue(_createLocalBranchForRemote);
        set => Setting.SetValue(_createLocalBranchForRemote, value);
    }

    private static readonly ISetting<bool> _useFormCommitMessage = Setting.Create(RootSettingsPath, "UseFormCommitMessage", defaultValue: true);

    public static bool UseFormCommitMessage
    {
        get => Setting.GetValue(_useFormCommitMessage);
        set => Setting.SetValue(_useFormCommitMessage, value);
    }

    private static readonly ISetting<bool> _commitAutomaticallyAfterCherryPick = Setting.Create(RootSettingsPath, "CommitAutomaticallyAfterCherryPick", defaultValue: false);

    public static bool CommitAutomaticallyAfterCherryPick
    {
        get => Setting.GetValue(_commitAutomaticallyAfterCherryPick);
        set => Setting.SetValue(_commitAutomaticallyAfterCherryPick, value);
    }

    private static readonly ISetting<bool> _addCommitReferenceToCherryPick = Setting.Create(RootSettingsPath, "AddCommitReferenceToCherryPick", defaultValue: false);

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

    private static readonly ISetting<bool> _checkForUpdates = Setting.Create(RootSettingsPath, "CheckForUpdates", defaultValue: true);

    public static bool CheckForUpdates
    {
        get => Setting.GetValue(_checkForUpdates);
        set => Setting.SetValue(_checkForUpdates, value);
    }

    private static readonly ISetting<bool> _checkForReleaseCandidates = Setting.Create(RootSettingsPath, "CheckForReleaseCandidates", defaultValue: false);

    public static bool CheckForReleaseCandidates
    {
        get => Setting.GetValue(_checkForReleaseCandidates);
        set => Setting.SetValue(_checkForReleaseCandidates, value);
    }

    private static readonly ISetting<bool> _omitUninterestingDiff = Setting.Create(RootSettingsPath, "OmitUninterestingDiff", defaultValue: false);

    public static bool OmitUninterestingDiff
    {
        get => Setting.GetValue(_omitUninterestingDiff);
        set => Setting.SetValue(_omitUninterestingDiff, value);
    }

    private static readonly ISetting<bool> _useConsoleEmulatorForCommands = Setting.Create(RootSettingsPath, "UseConsoleEmulatorForCommands", defaultValue: true);

    public static bool UseConsoleEmulatorForCommands
    {
        get => Setting.GetValue(_useConsoleEmulatorForCommands);
        set => Setting.SetValue(_useConsoleEmulatorForCommands, value);
    }

    private static readonly ISetting<GitRefsSortBy> _refsSortBy = Setting.Create(RootSettingsPath, "RefsSortBy", defaultValue: GitRefsSortBy.Default);

    public static GitRefsSortBy RefsSortBy
    {
        get => Setting.GetValue(_refsSortBy);
        set => Setting.SetValue(_refsSortBy, value);
    }

    private static readonly ISetting<GitRefsSortOrder> _refsSortOrder = Setting.Create(RootSettingsPath, "RefsSortOrder", defaultValue: GitRefsSortOrder.Descending);

    public static GitRefsSortOrder RefsSortOrder
    {
        get => Setting.GetValue(_refsSortOrder);
        set => Setting.SetValue(_refsSortOrder, value);
    }

    private static readonly ISetting<DiffListSortType> _diffListSorting = Setting.Create(RootSettingsPath, "DiffListSortType", defaultValue: DiffListSortType.FilePath);

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

    private static readonly ISetting<bool> _repoObjectsTreeShowBranches = Setting.Create(RootSettingsPath, "RepoObjectsTree.ShowBranches", defaultValue: true);

    public static bool RepoObjectsTreeShowBranches
    {
        get => Setting.GetValue(_repoObjectsTreeShowBranches);
        set => Setting.SetValue(_repoObjectsTreeShowBranches, value);
    }

    private static readonly ISetting<bool> _repoObjectsTreeShowRemotes = Setting.Create(RootSettingsPath, "RepoObjectsTree.ShowRemotes", defaultValue: true);

    public static bool RepoObjectsTreeShowRemotes
    {
        get => Setting.GetValue(_repoObjectsTreeShowRemotes);
        set => Setting.SetValue(_repoObjectsTreeShowRemotes, value);
    }

    private static readonly ISetting<bool> _repoObjectsTreeShowTags = Setting.Create(RootSettingsPath, "RepoObjectsTree.ShowTags", defaultValue: true);

    public static bool RepoObjectsTreeShowTags
    {
        get => Setting.GetValue(_repoObjectsTreeShowTags);
        set => Setting.SetValue(_repoObjectsTreeShowTags, value);
    }

    private static readonly ISetting<bool> _repoObjectsTreeShowStashes = Setting.Create(RootSettingsPath, "RepoObjectsTree.ShowStashes", defaultValue: true);

    public static bool RepoObjectsTreeShowStashes
    {
        get => Setting.GetValue(_repoObjectsTreeShowStashes);
        set => Setting.SetValue(_repoObjectsTreeShowStashes, value);
    }

    private static readonly ISetting<bool> _repoObjectsTreeShowSubmodules = Setting.Create(RootSettingsPath, "RepoObjectsTree.ShowSubmodules", defaultValue: true);

    public static bool RepoObjectsTreeShowSubmodules
    {
        get => Setting.GetValue(_repoObjectsTreeShowSubmodules);
        set => Setting.SetValue(_repoObjectsTreeShowSubmodules, value);
    }

    private static readonly ISetting<bool> _repoObjectsTreeShowWorktrees = Setting.Create(RootSettingsPath, "RepoObjectsTree.ShowWorktrees", defaultValue: true);

    public static bool RepoObjectsTreeShowWorktrees
    {
        get => Setting.GetValue(_repoObjectsTreeShowWorktrees);
        set => Setting.SetValue(_repoObjectsTreeShowWorktrees, value);
    }

    private static readonly ISetting<int> _repoObjectsTreeBranchesIndex = Setting.Create(RootSettingsPath, "RepoObjectsTree.BranchesIndex", defaultValue: 0);

    public static int RepoObjectsTreeBranchesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeBranchesIndex);
        set => Setting.SetValue(_repoObjectsTreeBranchesIndex, value);
    }

    private static readonly ISetting<int> _repoObjectsTreeRemotesIndex = Setting.Create(RootSettingsPath, "RepoObjectsTree.RemotesIndex", defaultValue: 1);

    public static int RepoObjectsTreeRemotesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeRemotesIndex);
        set => Setting.SetValue(_repoObjectsTreeRemotesIndex, value);
    }

    private static readonly ISetting<int> _repoObjectsTreeWorktreesIndex = Setting.Create(RootSettingsPath, "RepoObjectsTree.WorktreesIndex", defaultValue: 2);

    public static int RepoObjectsTreeWorktreesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeWorktreesIndex);
        set => Setting.SetValue(_repoObjectsTreeWorktreesIndex, value);
    }

    private static readonly ISetting<int> _repoObjectsTreeTagsIndex = Setting.Create(RootSettingsPath, "RepoObjectsTree.TagsIndex", defaultValue: 3);

    public static int RepoObjectsTreeTagsIndex
    {
        get => Setting.GetValue(_repoObjectsTreeTagsIndex);
        set => Setting.SetValue(_repoObjectsTreeTagsIndex, value);
    }

    private static readonly ISetting<int> _repoObjectsTreeSubmodulesIndex = Setting.Create(RootSettingsPath, "RepoObjectsTree.SubmodulesIndex", defaultValue: 4);

    public static int RepoObjectsTreeSubmodulesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeSubmodulesIndex);
        set => Setting.SetValue(_repoObjectsTreeSubmodulesIndex, value);
    }

    private static readonly ISetting<int> _repoObjectsTreeStashesIndex = Setting.Create(RootSettingsPath, "RepoObjectsTree.StashesIndex", defaultValue: 5);

    public static int RepoObjectsTreeStashesIndex
    {
        get => Setting.GetValue(_repoObjectsTreeStashesIndex);
        set => Setting.SetValue(_repoObjectsTreeStashesIndex, value);
    }

    private static readonly ISetting<string> _prioritizedBranchNames = Setting.Create(RootSettingsPath, "PrioritizedBranchNames", "main[^/]*|master[^/]*|release/.*");

    public static string PrioritizedBranchNames
    {
        get => Setting.GetValue(_prioritizedBranchNames);
        set => Setting.SetValue(_prioritizedBranchNames, value);
    }

    private static readonly ISetting<string> _prioritizedRemoteNames = Setting.Create(RootSettingsPath, "PrioritizedRemoteNames", "origin|upstream");

    public static string PrioritizedRemoteNames
    {
        get => Setting.GetValue(_prioritizedRemoteNames);
        set => Setting.SetValue(_prioritizedRemoteNames, value);
    }

    /// <summary>
    ///  Remote names to prefer when auto-detecting build server integration, separated by <c>|</c>.
    ///  Defaults to <c>upstream|origin</c> so that forks resolve to the upstream project's CI.
    /// </summary>
    private static readonly ISetting<string> _prioritizedBuildServerRemoteNames = Setting.Create(RootSettingsPath, "PrioritizedBuildServerRemoteNames", "upstream|origin|remote");

    public static string PrioritizedBuildServerRemoteNames
    {
        get => Setting.GetValue(_prioritizedBuildServerRemoteNames);
        set => Setting.SetValue(_prioritizedBuildServerRemoteNames, value);
    }

    private static readonly ISetting<bool> _blameDisplayAuthorFirst = Setting.Create(RootSettingsPath, "Blame.DisplayAuthorFirst", defaultValue: false);

    public static bool BlameDisplayAuthorFirst
    {
        get => Setting.GetValue(_blameDisplayAuthorFirst);
        set => Setting.SetValue(_blameDisplayAuthorFirst, value);
    }

    private static readonly ISetting<bool> _blameShowAuthor = Setting.Create(RootSettingsPath, "Blame.ShowAuthor", defaultValue: true);

    public static bool BlameShowAuthor
    {
        get => Setting.GetValue(_blameShowAuthor);
        set => Setting.SetValue(_blameShowAuthor, value);
    }

    private static readonly ISetting<bool> _blameShowAuthorDate = Setting.Create(RootSettingsPath, "Blame.ShowAuthorDate", defaultValue: true);

    public static bool BlameShowAuthorDate
    {
        get => Setting.GetValue(_blameShowAuthorDate);
        set => Setting.SetValue(_blameShowAuthorDate, value);
    }

    private static readonly ISetting<bool> _blameShowAuthorTime = Setting.Create(RootSettingsPath, "Blame.ShowAuthorTime", defaultValue: true);

    public static bool BlameShowAuthorTime
    {
        get => Setting.GetValue(_blameShowAuthorTime);
        set => Setting.SetValue(_blameShowAuthorTime, value);
    }

    private static readonly ISetting<bool> _blameShowLineNumbers = Setting.Create(RootSettingsPath, "Blame.ShowLineNumbers", defaultValue: false);

    public static bool BlameShowLineNumbers
    {
        get => Setting.GetValue(_blameShowLineNumbers);
        set => Setting.SetValue(_blameShowLineNumbers, value);
    }

    private static readonly ISetting<bool> _blameShowOriginalFilePath = Setting.Create(RootSettingsPath, "Blame.ShowOriginalFilePath", defaultValue: true);

    public static bool BlameShowOriginalFilePath
    {
        get => Setting.GetValue(_blameShowOriginalFilePath);
        set => Setting.SetValue(_blameShowOriginalFilePath, value);
    }

    private static readonly ISetting<bool> _blameShowAuthorAvatar = Setting.Create(RootSettingsPath, "Blame.ShowAuthorAvatar", defaultValue: true);

    public static bool BlameShowAuthorAvatar
    {
        get => Setting.GetValue(_blameShowAuthorAvatar);
        set => Setting.SetValue(_blameShowAuthorAvatar, value);
    }

    private static readonly ISetting<bool> _automaticContinuousScroll = Setting.Create(RootSettingsPath, "DiffViewer.AutomaticContinuousScroll", defaultValue: false);

    public static bool AutomaticContinuousScroll
    {
        get => Setting.GetValue(_automaticContinuousScroll);
        set => Setting.SetValue(_automaticContinuousScroll, value);
    }

    private static readonly ISetting<int> _automaticContinuousScrollDelay = Setting.Create(RootSettingsPath, "DiffViewer.AutomaticContinuousScrollDelay", defaultValue: 600);

    public static int AutomaticContinuousScrollDelay
    {
        get => Setting.GetValue(_automaticContinuousScrollDelay);
        set => Setting.SetValue(_automaticContinuousScrollDelay, value);
    }

    public static IEnumerable<string> CustomGenericRemoteNames
    {
        get => GetString("CustomGenericRemoteNames", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    private static readonly ISetting<bool> _logCaptureCallStacks = Setting.Create(RootSettingsPath, "Log.CaptureCallStacks", defaultValue: false);

    public static bool LogCaptureCallStacks
    {
        get => Setting.GetValue(_logCaptureCallStacks);
        set => Setting.SetValue(_logCaptureCallStacks, value);
    }

    // There is a bug in .NET/.NET Designer that fails to execute Properties.Settings.Default call.
    // Return false whilst we're in the designer.
    public static bool IsPortable() => !IsDesignMode && Properties.Settings.Default.IsPortable;

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<bool> _writeErrorLog = Setting.Create(RootSettingsPath, "WriteErrorLog", defaultValue: false);

    public static bool WriteErrorLog
    {
        get => Setting.GetValue(_writeErrorLog);
    }

    // Currently not configurable in UI (Set manually in settings file)
    private static readonly ISetting<bool> _workaroundActivateFromMinimize = Setting.Create(RootSettingsPath, "WorkaroundActivateFromMinimize", defaultValue: false);

    public static bool WorkaroundActivateFromMinimize
    {
        get => Setting.GetValue(_workaroundActivateFromMinimize);
    }

    private static readonly ISetting<bool> _gitAsyncWhenMinimized = Setting.Create(RootSettingsPath, "GitAsyncWhenMinimized", defaultValue: false);

    public static bool GitAsyncWhenMinimized
    {
        get => Setting.GetValue(_gitAsyncWhenMinimized);
    }

    private static readonly ISetting<bool> _isEditorSettingsMigrated = Setting.Create(MigrationSettingsPath, nameof(IsEditorSettingsMigrated), false);

    public static bool IsEditorSettingsMigrated
    {
        get => Setting.GetValue(_isEditorSettingsMigrated);
        set => Setting.SetValue(_isEditorSettingsMigrated, value);
    }

    private static readonly ISetting<string> _uninformativeRepoNameRegex = Setting.Create(DetailedSettingsPath, nameof(UninformativeRepoNameRegex), "app|(repo(sitory)?)");

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
    public AppSettingsPath(string pathName) : base(null, pathName)
    {
    }

    public AppSettingsPath(SettingsPath parent, string pathName) : base(null, parent.PathFor(pathName))
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
