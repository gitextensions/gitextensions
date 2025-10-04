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

namespace GitCommands
{
    public static partial class AppSettings
    {
        // semi-constants
        public static Version AppVersion => Assembly.GetCallingAssembly().GetName().Version;
        public static string ProductVersion => Application.ProductVersion;
        public static readonly string ApplicationName = "Git Extensions";
        public static readonly string ApplicationId = ApplicationName.Replace(" ", "");
        public static readonly string SettingsFileName = ApplicationId + ".settings";
        public static readonly string UserPluginsDirectoryName = "UserPlugins";
        private static string _applicationExecutablePath = Application.ExecutablePath;
        private static string? _documentationBaseUrl;

        public static Lazy<string?> ApplicationDataPath { get; private set; }
        public static readonly Lazy<string?> LocalApplicationDataPath;
        public static string SettingsFilePath => Path.Combine(ApplicationDataPath.Value, SettingsFileName);
        public static string UserPluginsPath => Path.Combine(LocalApplicationDataPath.Value, UserPluginsDirectoryName);

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

        private static Mutex _globalMutex;


        [GeneratedRegex(@"^(?<major>\d+)\.(?<minor>\d+)", RegexOptions.ExplicitCapture)]
        private static partial Regex VersionRegex();

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

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationId);
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
                    string dir = Path.GetDirectoryName(SettingsFilePath);
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
                Match match = VersionRegex().Match(version);
                if (match.Success)
                {
                    docVersion = $"en/release-{match.Groups["major"]}.{match.Groups["minor"]}/";
                }
            }

            _documentationBaseUrl = $"https://git-extensions-documentation.readthedocs.org/{docVersion}";
        }

        public static bool? TelemetryEnabled
        {
            get => GetBool("TelemetryEnabled");
            set => SetBool("TelemetryEnabled", value);
        }

        public static bool AutoNormaliseBranchName
        {
            get => GetBool("AutoNormaliseBranchName", true);
            set => SetBool("AutoNormaliseBranchName", value);
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

        public static bool RememberAmendCommitState
        {
            get => GetBool("RememberAmendCommitState", true);
            set => SetBool("RememberAmendCommitState", value);
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
                    return Path.Combine(projectPath, "Bin");
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

        [return: NotNullIfNotNull("defaultValue")]
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

        public static ISetting<int> FileStatusFindInFilesGitGrepTypeIndex { get; } = Setting.Create(FileStatusSettingsPath, nameof(FileStatusFindInFilesGitGrepTypeIndex), 1);

        public static ISetting<bool> FileStatusMergeSingleItemWithFolder { get; } = Setting.Create(FileStatusSettingsPath, nameof(FileStatusMergeSingleItemWithFolder), false);

        public static ISetting<bool> FileStatusShowGroupNodesInFlatList { get; } = Setting.Create(FileStatusSettingsPath, nameof(FileStatusShowGroupNodesInFlatList), false);

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
        public static bool WslGitEnabled
        {
            get => GetBool("WslGitEnabled", true);
        }

        // Currently not configurable in UI (Set manually in settings file)
        public static string WslCommand
        {
            get => GetString(nameof(WslCommand), "wsl");
        }

        // Currently not configurable in UI (Set manually in settings file)
        public static string WslGitCommand
        {
            get => GetString(nameof(WslGitCommand), "git");
        }

        public static bool StashKeepIndex
        {
            get => GetBool("stashkeepindex", false);
            set => SetBool("stashkeepindex", value);
        }

        public static bool DontConfirmStashDrop
        {
            // History Compatibility: The settings was originally was called 'StashConfirmDropShow', and then it was inverted.
            // To maintain the compat with the existing user settings negate the retrieved value.
            get => !GetBool("stashconfirmdropshow", true);
            set => SetBool("stashconfirmdropshow", !value);
        }

        public static bool ApplyPatchIgnoreWhitespace
        {
            get => GetBool("applypatchignorewhitespace", false);
            set => SetBool("applypatchignorewhitespace", value);
        }

        public static bool ApplyPatchSignOff
        {
            get => GetBool("applypatchsignoff", true);
            set => SetBool("applypatchsignoff", value);
        }

        public static bool UseHistogramDiffAlgorithm
        {
            // History Compatibility: The settings key has patience in the name for historical reasons
            get => GetBool("usepatiencediffalgorithm", false);
            set => SetBool("usepatiencediffalgorithm", value);
        }

        /// <summary>
        /// Use Git coloring for selected commands
        /// </summary>
        public static ISetting<bool> UseGitColoring { get; } = Setting.Create(AppearanceSettingsPath, nameof(UseGitColoring), true);

        /// <summary>
        /// Color the background at changes (invert colors).
        /// </summary>
        public static ISetting<bool> ReverseGitColoring { get; } = Setting.Create(AppearanceSettingsPath, nameof(ReverseGitColoring), true);

        public static bool ShowErrorsWhenStagingFiles
        {
            get => GetBool("showerrorswhenstagingfiles", true);
            set => SetBool("showerrorswhenstagingfiles", value);
        }

        public static bool EnsureCommitMessageSecondLineEmpty
        {
            get => GetBool("addnewlinetocommitmessagewhenmissing", true);
            set => SetBool("addnewlinetocommitmessagewhenmissing", value);
        }

        public static string LastCommitMessage
        {
            get => GetString("lastCommitMessage", "");
            set => SetString("lastCommitMessage", value);
        }
        
        public static int CommitDialogNumberOfPreviousMessages
        {
            get => GetInt("commitDialogNumberOfPreviousMessages", 6);
            set => SetInt("commitDialogNumberOfPreviousMessages", value);
        }

        public static bool CommitDialogShowOnlyMyMessages
        {
            get => GetBool("commitDialogShowOnlyMyMessages", false);
            set => SetBool("commitDialogShowOnlyMyMessages", value);
        }

        public static bool ShowCommitAndPush
        {
            get => GetBool("showcommitandpush", true);
            set => SetBool("showcommitandpush", value);
        }

        public static bool ShowResetWorkTreeChanges
        {
            get => GetBool("showresetunstagedchanges", true);
            set => SetBool("showresetunstagedchanges", value);
        }

        public static bool ShowResetAllChanges
        {
            get => GetBool("showresetallchanges", true);
            set => SetBool("showresetallchanges", value);
        }

        public static ISetting<bool> ShowConEmuTab { get; } = Setting.Create(DetailedSettingsPath, nameof(ShowConEmuTab), true);
        public static ISetting<string> ConEmuStyle { get; } = Setting.Create(DetailedSettingsPath, nameof(ConEmuStyle), "<Solarized Light>");
        public static ISetting<string> ConEmuTerminal { get; } = Setting.Create(DetailedSettingsPath, nameof(ConEmuTerminal), "bash");
        public static ISetting<int> OutputHistoryDepth { get; } = Setting.Create(DetailedSettingsPath, nameof(OutputHistoryDepth), 20);
        public static ISetting<bool> OutputHistoryPanelVisible { get; } = Setting.Create(DetailedSettingsPath, nameof(OutputHistoryPanelVisible), false);
        public static ISetting<bool> ShowOutputHistoryAsTab { get; } = Setting.Create(DetailedSettingsPath, nameof(ShowOutputHistoryAsTab), true);
        public static ISetting<bool> UseBrowseForFileHistory { get; } = Setting.Create(DetailedSettingsPath, nameof(UseBrowseForFileHistory), true);
        public static ISetting<bool> UseDiffViewerForBlame { get; } = Setting.Create(DetailedSettingsPath, nameof(UseDiffViewerForBlame), false);
        public static ISetting<bool> ShowGpgInformation { get; } = Setting.Create(DetailedSettingsPath, nameof(ShowGpgInformation), true);

        public static CommitInfoPosition CommitInfoPosition
        {
            get => ((ISettingsValueGetter)DetailedSettingsPath).GetValue<CommitInfoPosition>("CommitInfoPosition") ?? (
                DetailedSettingsPath.GetBool("ShowRevisionInfoNextToRevisionGrid") == true // legacy setting
                    ? CommitInfoPosition.RightwardFromList
                    : CommitInfoPosition.BelowList);
            set => DetailedSettingsPath.SetEnum("CommitInfoPosition", value);
        }

        public static ISetting<bool> MessageEditorWordWrap => Setting.Create(DetailedSettingsPath, nameof(MessageEditorWordWrap), false);

        public static bool ShowSplitViewLayout
        {
            get => DetailedSettingsPath.GetBool("ShowSplitViewLayout", true);
            set => DetailedSettingsPath.SetBool("ShowSplitViewLayout", value);
        }

        public static bool ProvideAutocompletion
        {
            get => GetBool("provideautocompletion", true);
            set => SetBool("provideautocompletion", value);
        }

        public static TruncatePathMethod TruncatePathMethod
        {
            get => GetEnum("truncatepathmethod", TruncatePathMethod.None);
            set => SetEnum("truncatepathmethod", value);
        }

        public static bool ShowGitStatusInBrowseToolbar
        {
            get => GetBool("showgitstatusinbrowsetoolbar", true);
            set => SetBool("showgitstatusinbrowsetoolbar", value);
        }

        public static bool ShowGitStatusForArtificialCommits
        {
            get => GetBool("showgitstatusforartificialcommits", true);
            set => SetBool("showgitstatusforartificialcommits", value);
        }

        public static EnumRuntimeSetting<RevisionSortOrder> RevisionSortOrder { get; } = new(RootSettingsPath, nameof(RevisionSortOrder), GitCommands.RevisionSortOrder.GitDefault);

        public static bool CommitInfoShowContainedInBranches => CommitInfoShowContainedInBranchesLocal ||
                                                                CommitInfoShowContainedInBranchesRemote ||
                                                                CommitInfoShowContainedInBranchesRemoteIfNoLocal;

        public static bool CommitInfoShowContainedInBranchesLocal
        {
            get => GetBool("commitinfoshowcontainedinbrancheslocal", true);
            set => SetBool("commitinfoshowcontainedinbrancheslocal", value);
        }

        public static bool CheckForUncommittedChangesInCheckoutBranch
        {
            get => GetBool("checkforuncommittedchangesincheckoutbranch", true);
            set => SetBool("checkforuncommittedchangesincheckoutbranch", value);
        }

        public static bool AlwaysShowCheckoutBranchDlg
        {
            get => GetBool("AlwaysShowCheckoutBranchDlg", false);
            set => SetBool("AlwaysShowCheckoutBranchDlg", value);
        }

        public static bool CommitAndPushForcedWhenAmend
        {
            get => GetBool("CommitAndPushForcedWhenAmend", false);
            set => SetBool("CommitAndPushForcedWhenAmend", value);
        }

        public static bool CommitInfoShowContainedInBranchesRemote
        {
            get => GetBool("commitinfoshowcontainedinbranchesremote", false);
            set => SetBool("commitinfoshowcontainedinbranchesremote", value);
        }

        public static bool CommitInfoShowContainedInBranchesRemoteIfNoLocal
        {
            get => GetBool("commitinfoshowcontainedinbranchesremoteifnolocal", false);
            set => SetBool("commitinfoshowcontainedinbranchesremoteifnolocal", value);
        }

        public static bool CommitInfoShowContainedInTags
        {
            get => GetBool("commitinfoshowcontainedintags", true);
            set => SetBool("commitinfoshowcontainedintags", value);
        }

        public static bool CommitInfoShowTagThisCommitDerivesFrom
        {
            get => GetBool("commitinfoshowtagthiscommitderivesfrom", true);
            set => SetBool("commitinfoshowtagthiscommitderivesfrom", value);
        }

        #region Avatars

        public static string AvatarImageCachePath => Path.Combine(LocalApplicationDataPath.Value, "Images\\");

        public static AvatarFallbackType AvatarFallbackType
        {
            get => GetEnumViaString("GravatarDefaultImageType", AvatarFallbackType.AuthorInitials);
            set => SetString("GravatarDefaultImageType", value.ToString());
        }

        public static string CustomAvatarTemplate
        {
            get => GetString("CustomAvatarTemplate", string.Empty);
            set => SetString("CustomAvatarTemplate", value);
        }

        /// <summary>
        /// Gets the size of the commit author avatar. Set to 80px.
        /// </summary>
        /// <remarks>The value should be scaled with DPI.</remarks>
        public static int AuthorImageSizeInCommitInfo => 80;

        public static int AvatarImageCacheDays
        {
            get => GetInt("authorimagecachedays", 13);
            set => SetInt("authorimagecachedays", value);
        }

        public static bool ShowAuthorAvatarInCommitInfo
        {
            get => GetBool("showauthorgravatar", true);
            set => SetBool("showauthorgravatar", value);
        }

        public static AvatarProvider AvatarProvider
        {
            get => GetEnumViaString("Appearance.AvatarProvider", AvatarProvider.None);
            set => SetString("Appearance.AvatarProvider", value.ToString());
        }

        public static int AvatarCacheSize
        {
            get => GetInt("Appearance.AvatarCacheSize", 200);
            set => SetInt("Appearance.AvatarCacheSize", value);
        }

        // Currently not configurable in UI (Set manually in settings file)
        // Names from here: https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.brushes?view=windowsdesktop-7.0
        // or #AARRGGBB code
        public static string AvatarAuthorInitialsPalette
        {
            get => GetString("Appearance.AvatarAuthorInitialsPalette", "SlateGray,RoyalBlue,Purple,OrangeRed,Teal,OliveDrab");
            set => SetString("Appearance.AvatarAuthorInitialsPalette", value);
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
            string settingStringValue = GetString(settingName, defaultValue.ToString());

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
                string path = new SshPathLocator().GetSshFromGitDir(LinuxToolsDir);
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
            bool isMigrated = IsEditorSettingsMigrated.Value;
            IsEditorSettingsMigrated.Value = true;
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
            get => GetString("translation", "");
            set => SetString("translation", value);
        }

        private static string? _currentTranslation;

        public static string CurrentTranslation
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
                if (_languageCodes.TryGetValue(CurrentTranslation, out string code))
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
                    Debug.WriteLine("Culture {0} not found", new object[] { CurrentLanguageCode });
                    return CultureInfo.GetCultureInfo("en");
                }
            }
        }

        public static bool UserProfileHomeDir
        {
            get => GetBool("userprofilehomedir", false);
            set => SetBool("userprofilehomedir", value);
        }

        public static string CustomHomeDir
        {
            get => GetString("customhomedir", "");
            set => SetString("customhomedir", value);
        }

        public static bool EnableAutoScale
        {
            get => GetBool("enableautoscale", true);
            set => SetBool("enableautoscale", value);
        }

        public static bool CloseCommitDialogAfterCommit
        {
            get => GetBool("closecommitdialogaftercommit", true);
            set => SetBool("closecommitdialogaftercommit", value);
        }

        public static bool CloseCommitDialogAfterLastCommit
        {
            get => GetBool("closecommitdialogafterlastcommit", true);
            set => SetBool("closecommitdialogafterlastcommit", value);
        }

        public static bool RefreshArtificialCommitOnApplicationActivated
        {
            get => GetBool("refreshcommitdialogonformfocus", false);
            set => SetBool("refreshcommitdialogonformfocus", value);
        }

        public static bool StageInSuperprojectAfterCommit
        {
            get => GetBool("stageinsuperprojectaftercommit", true);
            set => SetBool("stageinsuperprojectaftercommit", value);
        }

        public static bool FollowRenamesInFileHistory
        {
            get => GetBool("followrenamesinfilehistory", true);
            set => SetBool("followrenamesinfilehistory", value);
        }

        public static bool FollowRenamesInFileHistoryExactOnly
        {
            get => GetBool("followrenamesinfilehistoryexactonly", false);
            set => SetBool("followrenamesinfilehistoryexactonly", value);
        }

        public static bool FullHistoryInFileHistory
        {
            get => GetBool("fullhistoryinfilehistory", false);
            set => SetBool("fullhistoryinfilehistory", value);
        }

        public static bool SimplifyMergesInFileHistory
        {
            get => GetBool("simplifymergesinfileHistory", false);
            set => SetBool("simplifymergesinfileHistory", value);
        }

        public static bool LoadFileHistoryOnShow
        {
            get => GetBool("LoadFileHistoryOnShow", true);
            set => SetBool("LoadFileHistoryOnShow", value);
        }

        public static bool LoadBlameOnShow
        {
            get => GetBool("LoadBlameOnShow", true);
            set => SetBool("LoadBlameOnShow", value);
        }

        public static bool DetectCopyInFileOnBlame
        {
            get => GetBool("DetectCopyInFileOnBlame", false);
            set => SetBool("DetectCopyInFileOnBlame", value);
        }

        public static bool DetectCopyInAllOnBlame
        {
            get => GetBool("DetectCopyInAllOnBlame", false);
            set => SetBool("DetectCopyInAllOnBlame", value);
        }

        public static bool IgnoreWhitespaceOnBlame
        {
            get => GetBool("IgnoreWhitespaceOnBlame", true);
            set => SetBool("IgnoreWhitespaceOnBlame", value);
        }

        public static bool OpenSubmoduleDiffInSeparateWindow
        {
            get => GetBool("opensubmodulediffinseparatewindow", false);
            set => SetBool("opensubmodulediffinseparatewindow", value);
        }

        /// <summary>
        /// Gets or sets whether to show artificial commits in the revision graph.
        /// </summary>
        public static bool RevisionGraphShowArtificialCommits
        {
            get => GetBool("revisiongraphshowworkingdirchanges", true);
            set => SetBool("revisiongraphshowworkingdirchanges", value);
        }

        public static bool RevisionGraphDrawAlternateBackColor
        {
            get => GetBool("RevisionGraphDrawAlternateBackColor", true);
            set => SetBool("RevisionGraphDrawAlternateBackColor", value);
        }

        public static bool RevisionGraphDrawNonRelativesGray
        {
            get => GetBool("revisiongraphdrawnonrelativesgray", true);
            set => SetBool("revisiongraphdrawnonrelativesgray", value);
        }

        public static bool RevisionGraphDrawNonRelativesTextGray
        {
            get => GetBool("revisiongraphdrawnonrelativestextgray", false);
            set => SetBool("revisiongraphdrawnonrelativestextgray", value);
        }

        public static readonly Dictionary<string, Encoding> AvailableEncodings = [];

        /// <summary>
        /// Gets or sets the default pull action that is performed by the toolbar icon when it is clicked on.
        /// </summary>
        public static GitPullAction DefaultPullAction
        {
            get => GetEnum("DefaultPullAction", GitPullAction.Merge);
            set => SetEnum("DefaultPullAction", value);
        }

        /// <summary>
        /// Gets or sets the default pull action as configured in the FormPull dialog.
        /// </summary>
        public static GitPullAction FormPullAction
        {
            get => GetEnum("FormPullAction", GitPullAction.Merge);
            set => SetEnum("FormPullAction", value);
        }

        public static bool AutoStash
        {
            get => GetBool("autostash", false);
            set => SetBool("autostash", value);
        }

        public static bool RebaseAutoStash
        {
            get => GetBool("RebaseAutostash", false);
            set => SetBool("RebaseAutostash", value);
        }

        public static LocalChangesAction CheckoutBranchAction
        {
            get => GetEnum("checkoutbranchaction", LocalChangesAction.DontChange);
            set => SetEnum("checkoutbranchaction", value);
        }

        public static ISetting<bool> CheckoutOtherBranchAfterReset { get; } = Setting.Create(DialogSettingsPath, nameof(CheckoutOtherBranchAfterReset), defaultValue: true);

        public static bool UseDefaultCheckoutBranchAction
        {
            get => GetBool("UseDefaultCheckoutBranchAction", false);
            set => SetBool("UseDefaultCheckoutBranchAction", value);
        }

        public static bool DontShowHelpImages
        {
            get => GetBool("DontShowHelpImages", false);
            set => SetBool("DontShowHelpImages", value);
        }

        public static bool AlwaysShowAdvOpt
        {
            get => GetBool("AlwaysShowAdvOpt", false);
            set => SetBool("AlwaysShowAdvOpt", value);
        }

        public static bool DontConfirmAmend
        {
            get => GetBool("DontConfirmAmend", false);
            set => SetBool("DontConfirmAmend", value);
        }

        public static bool DontConfirmDeleteUnmergedBranch
        {
            get => GetBool("DontConfirmDeleteUnmergedBranch", false);
            set => SetBool("DontConfirmDeleteUnmergedBranch", value);
        }

        public static bool DontConfirmCommitIfNoBranch
        {
            get => GetBool("DontConfirmCommitIfNoBranch", false);
            set => SetBool("DontConfirmCommitIfNoBranch", value);
        }

        public static ISetting<bool> ConfirmBranchCheckout { get; } = Setting.Create(ConfirmationsSettingsPath, nameof(ConfirmBranchCheckout), false);

        public static bool? AutoPopStashAfterPull
        {
            get => GetBool("AutoPopStashAfterPull");
            set => SetBool("AutoPopStashAfterPull", value);
        }

        public static bool? AutoPopStashAfterCheckoutBranch
        {
            get => GetBool("AutoPopStashAfterCheckoutBranch");
            set => SetBool("AutoPopStashAfterCheckoutBranch", value);
        }

        public static GitPullAction? AutoPullOnPushRejectedAction
        {
            get => GetNullableEnum<GitPullAction>("AutoPullOnPushRejectedAction");
            set => SetNullableEnum("AutoPullOnPushRejectedAction", value);
        }

        public static bool DontConfirmPushNewBranch
        {
            get => GetBool("DontConfirmPushNewBranch", false);
            set => SetBool("DontConfirmPushNewBranch", value);
        }

        public static bool DontConfirmAddTrackingRef
        {
            get => GetBool("DontConfirmAddTrackingRef", false);
            set => SetBool("DontConfirmAddTrackingRef", value);
        }

        public static bool DontConfirmCommitAfterConflictsResolved
        {
            get => GetBool("DontConfirmCommitAfterConflictsResolved", false);
            set => SetBool("DontConfirmCommitAfterConflictsResolved", value);
        }

        public static bool DontConfirmSecondAbortConfirmation
        {
            get => GetBool("DontConfirmSecondAbortConfirmation", false);
            set => SetBool("DontConfirmSecondAbortConfirmation", value);
        }

        public static bool DontConfirmRebase
        {
            get => GetBool("DontConfirmRebase", false);
            set => SetBool("DontConfirmRebase", value);
        }

        public static bool DontConfirmResolveConflicts
        {
            get => GetBool("DontConfirmResolveConflicts", false);
            set => SetBool("DontConfirmResolveConflicts", value);
        }

        public static bool DontConfirmUndoLastCommit
        {
            get => GetBool("DontConfirmUndoLastCommit", false);
            set => SetBool("DontConfirmUndoLastCommit", value);
        }

        public static bool DontConfirmFetchAndPruneAll
        {
            get => GetBool("DontConfirmFetchAndPruneAll", false);
            set => SetBool("DontConfirmFetchAndPruneAll", value);
        }

        public static bool DontConfirmSwitchWorktree
        {
            get => GetBool("DontConfirmSwitchWorktree", false);
            set => SetBool("DontConfirmSwitchWorktree", value);
        }

        public static bool IncludeUntrackedFilesInAutoStash
        {
            get => GetBool("includeUntrackedFilesInAutoStash", false);
            set => SetBool("includeUntrackedFilesInAutoStash", value);
        }

        public static bool IncludeUntrackedFilesInManualStash
        {
            get => GetBool("includeUntrackedFilesInManualStash", false);
            set => SetBool("includeUntrackedFilesInManualStash", value);
        }

        public static bool ShowRemoteBranches
        {
            get => GetBool("showRemoteBranches", true);
            set => SetBool("showRemoteBranches", value);
        }

        public static BoolRuntimeSetting ShowReflogReferences { get; } = new(RootSettingsPath, nameof(ShowReflogReferences), false);

        public static bool ShowStashes
        {
            get => GetBool("showStashes", true);
            set => SetBool("showStashes", value);
        }

        // Currently not configurable in UI (Set manually in settings file)
        public static int MaxStashesWithUntrackedFiles
        {
            get => GetInt("maxStashesWithUntrackedFiles", 10);
        }

        public static bool ShowSuperprojectTags
        {
            get => GetBool("showSuperprojectTags", false);
            set => SetBool("showSuperprojectTags", value);
        }

        public static bool ShowSuperprojectBranches
        {
            get => GetBool("showSuperprojectBranches", true);
            set => SetBool("showSuperprojectBranches", value);
        }

        public static bool ShowSuperprojectRemoteBranches
        {
            get => GetBool("showSuperprojectRemoteBranches", false);
            set => SetBool("showSuperprojectRemoteBranches", value);
        }

        public static bool? UpdateSubmodulesOnCheckout
        {
            get => GetBool("updateSubmodulesOnCheckout");
            set => SetBool("updateSubmodulesOnCheckout", value);
        }

        public static bool? DontConfirmUpdateSubmodulesOnCheckout
        {
            get => GetBool("dontConfirmUpdateSubmodulesOnCheckout");
            set => SetBool("dontConfirmUpdateSubmodulesOnCheckout", value);
        }

        public static string Dictionary
        {
            get => SettingsContainer.Detached().Dictionary;
            set => SettingsContainer.Detached().Dictionary = value;
        }

        public static bool ShowGitCommandLine
        {
            get => GetBool("showgitcommandline", false);
            set => SetBool("showgitcommandline", value);
        }

        public static bool ShowStashCount
        {
            get => GetBool("showstashcount", false);
            set => SetBool("showstashcount", value);
        }

        public static bool ShowAheadBehindData
        {
            get => GetBool("showaheadbehinddata", true);
            set => SetBool("showaheadbehinddata", value);
        }

        public static bool ShowSubmoduleStatus
        {
            get => GetBool("showsubmodulestatus", false);
            set => SetBool("showsubmodulestatus", value);
        }

        public static bool RelativeDate
        {
            get => GetBool("relativedate", true);
            set => SetBool("relativedate", value);
        }

        public static bool ShowGitNotes
        {
            get => GetBool("showgitnotes", false);
            set => SetBool("showgitnotes", value);
        }

        public static bool ShowAnnotatedTagsMessages
        {
            get => GetBool("showannotatedtagsmessages", true);
            set => SetBool("showannotatedtagsmessages", value);
        }

        // History Compatibility: The meaning of this value is changed in the GUI, setting name is kept for compatibility
        public static bool HideMergeCommits
        {
            get => !GetBool("showmergecommits", true);
            set => SetBool("showmergecommits", !value);
        }

        public static bool ShowTags
        {
            get => GetBool("showtags", true);
            set => SetBool("showtags", value);
        }

        #region Revision grid column visibilities

        public static bool ShowRevisionGridGraphColumn
        {
            get => GetBool("showrevisiongridgraphcolumn", true);
            set => SetBool("showrevisiongridgraphcolumn", value);
        }

        public static bool ShowAuthorAvatarColumn
        {
            get => GetBool("showrevisiongridauthoravatarcolumn", true);
            set => SetBool("showrevisiongridauthoravatarcolumn", value);
        }

        public static bool ShowAuthorNameColumn
        {
            get => GetBool("showrevisiongridauthornamecolumn", true);
            set => SetBool("showrevisiongridauthornamecolumn", value);
        }

        public static bool ShowDateColumn
        {
            get => GetBool("showrevisiongriddatecolumn", true);
            set => SetBool("showrevisiongriddatecolumn", value);
        }

        public static bool ShowObjectIdColumn
        {
            get => GetBool("showids", true);
            set => SetBool("showids", value);
        }

        public static bool ShowBuildStatusIconColumn
        {
            get => GetBool("showbuildstatusiconcolumn", true);
            set => SetBool("showbuildstatusiconcolumn", value);
        }

        public static bool ShowBuildStatusTextColumn
        {
            get => GetBool("showbuildstatustextcolumn", false);
            set => SetBool("showbuildstatustextcolumn", value);
        }

        #endregion

        public static bool ShowAuthorDate
        {
            get => GetBool("showauthordate", true);
            set => SetBool("showauthordate", value);
        }

        public static bool CloseProcessDialog
        {
            get => GetBool("closeprocessdialog", false);
            set => SetBool("closeprocessdialog", value);
        }

        public static ISetting<bool> ShowProcessDialogPasswordInput => Setting.Create(DetailedSettingsPath, nameof(ShowProcessDialogPasswordInput), false);

        public static BoolRuntimeSetting ShowCurrentBranchOnly { get; } = new(RootSettingsPath, nameof(ShowCurrentBranchOnly), false);

        public static bool ShowSimplifyByDecoration
        {
            get => GetBool("showsimplifybydecoration", false);
            set => SetBool("showsimplifybydecoration", value);
        }

        public static BoolRuntimeSetting BranchFilterEnabled { get; } = new(RootSettingsPath, nameof(BranchFilterEnabled), false);

        public static bool ShowOnlyFirstParent
        {
            get => GetBool("showfirstparent", false);
            set => SetBool("showfirstparent", value);
        }

        public static string[] RevisionFilterDropdowns
        {
            get => GetString("RevisionFilterDropdowns", string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            set => SetString("RevisionFilterDropdowns", string.Join("\n", value ?? Array.Empty<string>()));
        }

        public static bool CommitDialogSelectionFilter
        {
            get => GetBool("commitdialogselectionfilter", false);
            set => SetBool("commitdialogselectionfilter", value);
        }

        public static string DefaultCloneDestinationPath
        {
            get => GetString("defaultclonedestinationpath", string.Empty);
            set => SetString("defaultclonedestinationpath", value);
        }

        public static int RevisionGridQuickSearchTimeout
        {
            get => GetInt("revisiongridquicksearchtimeout", 4000);
            set => SetInt("revisiongridquicksearchtimeout", value);
        }

        public static bool ShowCommitBodyInRevisionGrid
        {
            get => GetBool("ShowCommitBodyInRevisionGrid", true);
            set => SetBool("ShowCommitBodyInRevisionGrid", value);
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
            get => GetInt("maxrevisiongraphcommits", 100000);
            set => SetInt("maxrevisiongraphcommits", value);
        }

        public static bool ShowDiffForAllParents
        {
            get => GetBool("showdiffforallparents", true);
            set => SetBool("showdiffforallparents", value);
        }

        public static ISetting<bool> ShowFindInCommitFilesGitGrep { get; } = Setting.Create(AppearanceSettingsPath, nameof(ShowFindInCommitFilesGitGrep), false);

        public static bool ShowAvailableDiffTools
        {
            get => GetBool("difftools.showavailable", true);
            set => SetBool("difftools.showavailable", value);
        }

        public static int DiffVerticalRulerPosition
        {
            get => GetInt("diffverticalrulerposition", 0);
            set => SetInt("diffverticalrulerposition", value);
        }

        public static ISetting<string> GitGrepUserArguments { get; } = Setting.Create(DialogSettingsPath, nameof(GitGrepUserArguments), "");

        public static ISetting<bool> GitGrepIgnoreCase { get; } = Setting.Create(DialogSettingsPath, nameof(GitGrepIgnoreCase), false);

        public static ISetting<bool> GitGrepMatchWholeWord { get; } = Setting.Create(DialogSettingsPath, nameof(GitGrepMatchWholeWord), false);

        [MaybeNull]
        public static string RecentWorkingDir
        {
            get => GetString("RecentWorkingDir", null);
            set => SetString("RecentWorkingDir", value);
        }

        public static bool StartWithRecentWorkingDir
        {
            get => GetBool("StartWithRecentWorkingDir", false);
            set => SetBool("StartWithRecentWorkingDir", value);
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
            get => GetBool("autostartpageant", true);
            set => SetBool("autostartpageant", value);
        }

        public static bool MarkIllFormedLinesInCommitMsg
        {
            get => GetBool("markillformedlinesincommitmsg", true);
            set => SetBool("markillformedlinesincommitmsg", value);
        }

        public static bool UseSystemVisualStyle
        {
            get => GetBool("systemvisualstyle", true);
            set => SetBool("systemvisualstyle", value);
        }

        public static ThemeId ThemeId
        {
            // Updating key names in v4.0, to force resetting to default
            // (as dark themes look bad due to SUPPORT_THEME_HOOKS no longer supported)
            get
            {
                return new ThemeId(
                    GetString("uitheme_v2", ThemeId.Default.Name),
                    GetBool("uithemeisbuiltin_v2", ThemeId.Default.IsBuiltin));
            }
            set
            {
                SetString("uitheme_v2", value.Name ?? string.Empty);
                SetBool("uithemeisbuiltin_v2", value.IsBuiltin);
            }
        }

        public static string? ThemeIdName_v1
        {
            get
            {
                return GetString("uitheme", null);
            }
            set
            {
                SetString("uitheme", value);
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
                SetString("uithemevariations", string.Join(",", value ?? Array.Empty<string>()));
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
            get => GetFont("commitfont", SystemFonts.MessageBoxFont);
            set => SetFont("commitfont", value);
        }

        public static Font MonospaceFont
        {
            get => GetFont("monospacefont", new Font("Consolas", 9));
            set => SetFont("monospacefont", value);
        }

        public static Font Font
        {
            get => GetFont("font", SystemFonts.MessageBoxFont);
            set => SetFont("font", value);
        }

        public static Font ConEmuConsoleFont
        {
            get => GetFont("conemuconsolefont", new Font("Consolas", 12));
            set => SetFont("conemuconsolefont", value);
        }

        public static bool ShowEolMarkerAsGlyph
        {
            get => GetBool("ShowEolMarkerAsGlyph", false);
            set => SetBool("ShowEolMarkerAsGlyph", value);
        }

        #endregion

        public static bool MulticolorBranches
        {
            get => GetBool("multicolorbranches", true);
            set => SetBool("multicolorbranches", value);
        }

        public static bool HighlightAuthoredRevisions
        {
            get { return GetBool("highlightauthoredrevisions", true); }
            set { SetBool("highlightauthoredrevisions", value); }
        }

        public static bool FillRefLabels
        {
            get => GetBool("FillRefLabels", false);
            set => SetBool("FillRefLabels", value);
        }

        public static ISetting<bool> MergeGraphLanesHavingCommonParent { get; } = Setting.Create(RevisionGraphSettingsPath, nameof(MergeGraphLanesHavingCommonParent), true);

        public static ISetting<bool> RenderGraphWithDiagonals { get; } = Setting.Create(RevisionGraphSettingsPath, nameof(RenderGraphWithDiagonals), true);

        public static ISetting<bool> StraightenGraphDiagonals { get; } = Setting.Create(RevisionGraphSettingsPath, nameof(StraightenGraphDiagonals), true);

        /// <summary>
        ///  The limit when to skip the straightening of revision graph segments.
        /// </summary>
        /// <remarks>
        ///  Straightening needs to call the expensive RevisionGraphRow.BuildSegmentLanes function.<br></br>
        ///  Straightening inserts gaps making the graph wider. If it already has to display many segments, i.e. parallel branches, there would be a low benefit of straightening.<br></br>
        ///  So rather skip the - in this case particularly expensive - RevisionGraphRow.BuildSegmentLanes function and call it only if the row is visible.
        /// </remarks>
        public static ISetting<int> StraightenGraphSegmentsLimit { get; } = Setting.Create(RevisionGraphSettingsPath, nameof(StraightenGraphSegmentsLimit), 80);

        public static string LastFormatPatchDir
        {
            get => GetString("lastformatpatchdir", "");
            set => SetString("lastformatpatchdir", value);
        }

        public static EnumRuntimeSetting<IgnoreWhitespaceKind> IgnoreWhitespaceKind { get; } = new(RootSettingsPath, nameof(IgnoreWhitespaceKind), Settings.IgnoreWhitespaceKind.None);

        public static bool RememberIgnoreWhiteSpacePreference
        {
            get => GetBool("rememberIgnoreWhiteSpacePreference", true);
            set => SetBool("rememberIgnoreWhiteSpacePreference", value);
        }

        public static BoolRuntimeSetting ShowNonPrintingChars { get; } = new(RootSettingsPath, nameof(ShowNonPrintingChars), false);

        public static bool RememberShowNonPrintingCharsPreference
        {
            get => GetBool("RememberShowNonPrintableCharsPreference", false);
            set => SetBool("RememberShowNonPrintableCharsPreference", value);
        }

        public static BoolRuntimeSetting ShowEntireFile { get; } = new(RootSettingsPath, nameof(ShowEntireFile), false);

        public static bool RememberShowEntireFilePreference
        {
            get => GetBool("RememberShowEntireFilePreference", false);
            set => SetBool("RememberShowEntireFilePreference", value);
        }

        /// <summary>
        /// Diff appearance, alternatives to "patch" viewer.
        /// </summary>
        public static EnumRuntimeSetting<DiffDisplayAppearance> DiffDisplayAppearance { get; } = new(RootSettingsPath, nameof(DiffDisplayAppearance), Settings.DiffDisplayAppearance.Patch);

        /// <summary>
        /// Gets or sets whether to remember the preference for diff appearance.
        /// </summary>
        public static ISetting<bool> RememberDiffDisplayAppearance { get; } = Setting.Create(AppearanceSettingsPath, nameof(RememberDiffDisplayAppearance), false);

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
            get => GetBool("RememberNumberOfContextLines", false);
            set => SetBool("RememberNumberOfContextLines", value);
        }

        public static BoolRuntimeSetting ShowSyntaxHighlightingInDiff { get; } = new(RootSettingsPath, nameof(ShowSyntaxHighlightingInDiff), true);

        public static bool RememberShowSyntaxHighlightingInDiff
        {
            get => GetBool("RememberShowSyntaxHighlightingInDiff", true);
            set => SetBool("RememberShowSyntaxHighlightingInDiff", value);
        }

        public static string GetDictionaryDir()
        {
            return Path.Combine(GetResourceDir(), "Dictionaries");
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
            get => GetBool("dashboardshowcurrentbranch", true);
            set => SetBool("dashboardshowcurrentbranch", value);
        }

        public static string? OwnScripts
        {
            get => GetString("ownScripts", "");
            set => SetString("ownScripts", value ?? "");
        }

        public static int RecursiveSubmodules
        {
            get => GetInt("RecursiveSubmodules", 1);
            set => SetInt("RecursiveSubmodules", value);
        }

        public static ShorteningRecentRepoPathStrategy ShorteningRecentRepoPathStrategy
        {
            get => GetEnum("ShorteningRecentRepoPathStrategy", ShorteningRecentRepoPathStrategy.None);
            set => SetEnum("ShorteningRecentRepoPathStrategy", value);
        }

        public static int MaxTopRepositories
        {
            // History Compatibility: Keep original key to maintain the compatibility with the existing user settings
            get => GetInt("MaxMostRecentRepositories", 0);
            set => SetInt("MaxMostRecentRepositories", value);
        }

        public static int RecentRepositoriesHistorySize
        {
            get => GetInt("history size", 30);
            set => SetInt("history size", value);
        }

        public static ISetting<bool> HideTopRepositoriesFromRecentList { get; } = Setting.Create(RecentRepositories, nameof(HideTopRepositoriesFromRecentList), false);

        // Currently not configurable in UI (Set manually in settings file)
        public static int RemotesCacheLength
        {
            get => GetInt("RemotesCacheLength", 30);
        }

        public static int RecentReposComboMinWidth
        {
            get => GetInt("RecentReposComboMinWidth", 0);
            set => SetInt("RecentReposComboMinWidth", value);
        }

        [MaybeNull]
        public static string SerializedHotkeys
        {
            get => GetString("SerializedHotkeys", null);
            set => SetString("SerializedHotkeys", value);
        }

        public static bool SortTopRepos
        {
            get => GetBool("SortMostRecentRepos", false);
            set => SetBool("SortMostRecentRepos", value);
        }

        public static bool SortRecentRepos
        {
            get => GetBool("SortLessRecentRepos", false);
            set => SetBool("SortLessRecentRepos", value);
        }

        public static bool DontCommitMerge
        {
            get => GetBool("DontCommitMerge", false);
            set => SetBool("DontCommitMerge", value);
        }

        public static int CommitValidationMaxCntCharsFirstLine
        {
            get => GetInt("CommitValidationMaxCntCharsFirstLine", 0);
            set => SetInt("CommitValidationMaxCntCharsFirstLine", value);
        }

        public static int CommitValidationMaxCntCharsPerLine
        {
            get => GetInt("CommitValidationMaxCntCharsPerLine", 0);
            set => SetInt("CommitValidationMaxCntCharsPerLine", value);
        }

        public static bool CommitValidationSecondLineMustBeEmpty
        {
            get => GetBool("CommitValidationSecondLineMustBeEmpty", false);
            set => SetBool("CommitValidationSecondLineMustBeEmpty", value);
        }

        public static bool CommitValidationIndentAfterFirstLine
        {
            get => GetBool("CommitValidationIndentAfterFirstLine", true);
            set => SetBool("CommitValidationIndentAfterFirstLine", value);
        }

        public static bool CommitValidationAutoWrap
        {
            get => GetBool("CommitValidationAutoWrap", true);
            set => SetBool("CommitValidationAutoWrap", value);
        }

        public static string CommitValidationRegEx
        {
            get => GetString("CommitValidationRegEx", string.Empty);
            set => SetString("CommitValidationRegEx", value);
        }

        public static string CommitTemplates
        {
            get => GetString("CommitTemplates", string.Empty);
            set => SetString("CommitTemplates", value);
        }

        public static bool CreateLocalBranchForRemote
        {
            get => GetBool("CreateLocalBranchForRemote", false);
            set => SetBool("CreateLocalBranchForRemote", value);
        }

        public static bool UseFormCommitMessage
        {
            get => GetBool("UseFormCommitMessage", true);
            set => SetBool("UseFormCommitMessage", value);
        }

        public static bool CommitAutomaticallyAfterCherryPick
        {
            get => GetBool("CommitAutomaticallyAfterCherryPick", false);
            set => SetBool("CommitAutomaticallyAfterCherryPick", value);
        }

        public static bool AddCommitReferenceToCherryPick
        {
            get => GetBool("AddCommitReferenceToCherryPick", false);
            set => SetBool("AddCommitReferenceToCherryPick", value);
        }

        public static DateTime LastUpdateCheck
        {
            get => GetDate("LastUpdateCheck", default);
            set => SetDate("LastUpdateCheck", value);
        }

        public static bool CheckForUpdates
        {
            get => GetBool("CheckForUpdates", true);
            set => SetBool("CheckForUpdates", value);
        }

        public static bool CheckForReleaseCandidates
        {
            get => GetBool("CheckForReleaseCandidates", false);
            set => SetBool("CheckForReleaseCandidates", value);
        }

        public static bool OmitUninterestingDiff
        {
            get => GetBool("OmitUninterestingDiff", false);
            set => SetBool("OmitUninterestingDiff", value);
        }

        public static bool UseConsoleEmulatorForCommands
        {
            get => GetBool("UseConsoleEmulatorForCommands", true);
            set => SetBool("UseConsoleEmulatorForCommands", value);
        }

        public static GitRefsSortBy RefsSortBy
        {
            get => GetEnum("RefsSortBy", GitRefsSortBy.Default);
            set => SetEnum("RefsSortBy", value);
        }

        public static GitRefsSortOrder RefsSortOrder
        {
            get => GetEnum("RefsSortOrder", GitRefsSortOrder.Descending);
            set => SetEnum("RefsSortOrder", value);
        }

        public static DiffListSortType DiffListSorting
        {
            get => GetEnum("DiffListSortType", DiffListSortType.FilePath);
            set => SetEnum("DiffListSortType", value);
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
            get => GetBool("RepoObjectsTree.ShowBranches", true);
            set => SetBool("RepoObjectsTree.ShowBranches", value);
        }

        public static bool RepoObjectsTreeShowRemotes
        {
            get => GetBool("RepoObjectsTree.ShowRemotes", true);
            set => SetBool("RepoObjectsTree.ShowRemotes", value);
        }

        public static bool RepoObjectsTreeShowTags
        {
            get => GetBool("RepoObjectsTree.ShowTags", true);
            set => SetBool("RepoObjectsTree.ShowTags", value);
        }

        public static bool RepoObjectsTreeShowStashes
        {
            get => GetBool("RepoObjectsTree.ShowStashes", true);
            set => SetBool("RepoObjectsTree.ShowStashes", value);
        }

        public static bool RepoObjectsTreeShowSubmodules
        {
            get => GetBool("RepoObjectsTree.ShowSubmodules", true);
            set => SetBool("RepoObjectsTree.ShowSubmodules", value);
        }

        public static int RepoObjectsTreeBranchesIndex
        {
            get => GetInt("RepoObjectsTree.BranchesIndex", 0);
            set => SetInt("RepoObjectsTree.BranchesIndex", value);
        }

        public static int RepoObjectsTreeRemotesIndex
        {
            get => GetInt("RepoObjectsTree.RemotesIndex", 1);
            set => SetInt("RepoObjectsTree.RemotesIndex", value);
        }

        public static int RepoObjectsTreeTagsIndex
        {
            get => GetInt("RepoObjectsTree.TagsIndex", 2);
            set => SetInt("RepoObjectsTree.TagsIndex", value);
        }

        public static int RepoObjectsTreeSubmodulesIndex
        {
            get => GetInt("RepoObjectsTree.SubmodulesIndex", 3);
            set => SetInt("RepoObjectsTree.SubmodulesIndex", value);
        }

        public static int RepoObjectsTreeStashesIndex
        {
            get => GetInt("RepoObjectsTree.StashesIndex", 4);
            set => SetInt("RepoObjectsTree.StashesIndex", value);
        }

        public static string PrioritizedBranchNames
        {
            get => GetString("PrioritizedBranchNames", "main[^/]*|master[^/]*|release/.*");
            set => SetString("PrioritizedBranchNames", value);
        }

        public static string PrioritizedRemoteNames
        {
            get => GetString("PrioritizedRemoteNames", "origin|upstream");
            set => SetString("PrioritizedRemoteNames", value);
        }

        public static bool BlameDisplayAuthorFirst
        {
            get => GetBool("Blame.DisplayAuthorFirst", false);
            set => SetBool("Blame.DisplayAuthorFirst", value);
        }

        public static bool BlameShowAuthor
        {
            get => GetBool("Blame.ShowAuthor", true);
            set => SetBool("Blame.ShowAuthor", value);
        }

        public static bool BlameShowAuthorDate
        {
            get => GetBool("Blame.ShowAuthorDate", true);
            set => SetBool("Blame.ShowAuthorDate", value);
        }

        public static bool BlameShowAuthorTime
        {
            get => GetBool("Blame.ShowAuthorTime", true);
            set => SetBool("Blame.ShowAuthorTime", value);
        }

        public static bool BlameShowLineNumbers
        {
            get => GetBool("Blame.ShowLineNumbers", false);
            set => SetBool("Blame.ShowLineNumbers", value);
        }

        public static bool BlameShowOriginalFilePath
        {
            get => GetBool("Blame.ShowOriginalFilePath", true);
            set => SetBool("Blame.ShowOriginalFilePath", value);
        }

        public static bool BlameShowAuthorAvatar
        {
            get => GetBool("Blame.ShowAuthorAvatar", true);
            set => SetBool("Blame.ShowAuthorAvatar", value);
        }

        public static bool AutomaticContinuousScroll
        {
            get => GetBool("DiffViewer.AutomaticContinuousScroll", false);
            set => SetBool("DiffViewer.AutomaticContinuousScroll", value);
        }

        public static int AutomaticContinuousScrollDelay
        {
            get => GetInt("DiffViewer.AutomaticContinuousScrollDelay", 600);
            set => SetInt("DiffViewer.AutomaticContinuousScrollDelay", value);
        }

        public static IEnumerable<string> CustomGenericRemoteNames
        {
            get => GetString("CustomGenericRemoteNames", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        public static bool LogCaptureCallStacks
        {
            get => GetBool("Log.CaptureCallStacks", false);
            set => SetBool("Log.CaptureCallStacks", value);
        }

        public static int CommentStrategyId
        {
            get => GetInt("CommentStrategyId", 1);
            set => SetInt("CommentStrategyId", value);
        }

        // There is a bug in .NET/.NET Designer that fails to execute Properties.Settings.Default call.
        // Return false whilst we're in the designer.
        public static bool IsPortable() => !IsDesignMode && Properties.Settings.Default.IsPortable;

        // Currently not configurable in UI (Set manually in settings file)
        public static bool WriteErrorLog
        {
            get => GetBool("WriteErrorLog", false);
        }

        // Currently not configurable in UI (Set manually in settings file)
        public static bool WorkaroundActivateFromMinimize
        {
            get => GetBool("WorkaroundActivateFromMinimize", false);
        }

        public static bool GitAsyncWhenMinimized
        {
            get => GetBool("GitAsyncWhenMinimized", false);
        }

        public static ISetting<bool> IsEditorSettingsMigrated { get; } = Setting.Create(MigrationSettingsPath, nameof(IsEditorSettingsMigrated), false);

        public static ISetting<string> UninformativeRepoNameRegex { get; } = Setting.Create(DetailedSettingsPath, nameof(UninformativeRepoNameRegex), "app|(repo(sitory)?)");

        private static IEnumerable<(string name, string value)> GetSettingsFromRegistry()
        {
            RegistryKey oldSettings = VersionIndependentRegKey.OpenSubKey("GitExtensions");

            if (oldSettings is null)
            {
                yield break;
            }

            foreach (string name in oldSettings.GetValueNames())
            {
                object value = oldSettings.GetValue(name, null);

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

        // String
        [return: NotNullIfNotNull("defaultValue")]
        public static string? GetString(string name, string? defaultValue) => SettingsContainer.GetString(name, defaultValue);
        public static void SetString(string name, string value) => SettingsContainer.SetString(name, value);

        // Bool
        public static bool? GetBool(string name) => SettingsContainer.GetBool(name);
        public static bool GetBool(string name, bool defaultValue) => SettingsContainer.GetBool(name, defaultValue);
        public static void SetBool(string name, bool? value) => SettingsContainer.SetBool(name, value);

        // Int
        public static int? GetInt(string name) => SettingsContainer.GetInt(name);
        public static int GetInt(string name, int defaultValue) => SettingsContainer.GetInt(name, defaultValue);
        public static void SetInt(string name, int? value) => SettingsContainer.SetInt(name, value);

        // Float
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
        public static T GetEnum<T>(string name, T defaultValue) where T : struct, Enum => SettingsContainer.GetEnum(name, defaultValue);
        public static void SetEnum<T>(string name, T value) where T : Enum => SettingsContainer.SetEnum(name, value);

        public static T? GetNullableEnum<T>(string name) where T : struct, Enum => ((ISettingsValueGetter)SettingsContainer).GetValue<T>(name);
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
            public string ApplicationExecutablePath
            {
                get => _applicationExecutablePath;
                set => _applicationExecutablePath = value;
            }

            public Lazy<string?> ApplicationDataPath
            {
                get => AppSettings.ApplicationDataPath;
                set => AppSettings.ApplicationDataPath = value;
            }

            public void ResetDocumentationBaseUrl() => AppSettings._documentationBaseUrl = null;
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
}
