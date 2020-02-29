﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GitCommands.Settings;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.Win32;
using StringSetting = GitCommands.Settings.StringSetting;

namespace GitCommands
{
    public enum LocalChangesAction
    {
        // DO NOT RENAME THESE -- doing so will break user preferences
        DontChange,
        Merge,
        Reset,
        Stash
    }

    public enum TruncatePathMethod
    {
        // DO NOT RENAME THESE -- doing so will break user preferences
        None,
        Compact,
        TrimStart,
        FileNameOnly
    }

    public enum ShorteningRecentRepoPathStrategy
    {
        // DO NOT RENAME THESE -- doing so will break user preferences
        None,
        MostSignDir,
        MiddleDots
    }

    public enum CommitInfoPosition
    {
        BelowList = 0,
        LeftwardFromList = 1,
        RightwardFromList = 2
    }

    public static class AppSettings
    {
        // semi-constants
        public static Version AppVersion => Assembly.GetCallingAssembly().GetName().Version;
        public static string ProductVersion => Application.ProductVersion;
        public static readonly string SettingsFileName = "GitExtensions.settings";
        public static readonly string UserPluginsDirectoryName = "UserPlugins";
        private static string _applicationExecutablePath = Application.ExecutablePath;
        private static readonly ISshPathLocator SshPathLocatorInstance = new SshPathLocator();

        public static readonly Lazy<string> ApplicationDataPath;
        public static readonly Lazy<string> LocalApplicationDataPath;
        public static string SettingsFilePath => Path.Combine(ApplicationDataPath.Value, SettingsFileName);
        public static string UserPluginsPath => Path.Combine(LocalApplicationDataPath.Value, UserPluginsDirectoryName);

        public static RepoDistSettings SettingsContainer { get; private set; }

        private static readonly SettingsPath DetailedSettingsPath = new AppSettingsPath("Detailed");

        public static readonly int BranchDropDownMinWidth = 300;
        public static readonly int BranchDropDownMaxWidth = 600;

        public static event Action Saved;

        static AppSettings()
        {
            ApplicationDataPath = new Lazy<string>(() =>
            {
                if (IsPortable())
                {
                    return GetGitExtensionsDirectory();
                }

                // Make ApplicationDataPath version independent
                return Application.UserAppDataPath.Replace(Application.ProductVersion, string.Empty)
                                                  .Replace("Git Extensions", "GitExtensions"); // 'GitExtensions' has been changed to 'Git Extensions' in v3.0
            });

            LocalApplicationDataPath = new Lazy<string>(() =>
            {
                if (IsPortable())
                {
                    return GetGitExtensionsDirectory();
                }

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitExtensions");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            });

            SettingsContainer = new RepoDistSettings(null, GitExtSettingsCache.FromCache(SettingsFilePath), SettingLevel.Unknown);

            if (!File.Exists(SettingsFilePath))
            {
                ImportFromRegistry();
            }
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
                var value = GetString("AutoNormaliseSymbol", "_");
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

        public static bool RememberAmendCommitState
        {
            get => GetBool("RememberAmendCommitState", true);
            set => SetBool("RememberAmendCommitState", value);
        }

        public static void UsingContainer(RepoDistSettings settingsContainer, Action action)
        {
            SettingsContainer.LockedAction(() =>
                {
                    var oldSC = SettingsContainer;
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

        [CanBeNull]
        public static string GetInstallDir()
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

        [CanBeNull]
        public static string GetResourceDir()
        {
#if DEBUG
            string gitExtDir = GetGitExtensionsDirectory().TrimEnd('\\').TrimEnd('/');
            const string debugPath = @"GitExtensions\bin\Debug";
            int len = debugPath.Length;
            if (gitExtDir.Length > len)
            {
                var path = gitExtDir.Substring(gitExtDir.Length - len);

                if (debugPath.ToPosixPath() == path.ToPosixPath())
                {
                    string projectPath = gitExtDir.Substring(0, gitExtDir.Length - len);
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
            object obj = VersionIndependentRegKey.GetValue(key);
            if (!(obj is string))
            {
                obj = null;
            }

            if (obj == null)
            {
                return defaultValue;
            }

            return ((string)obj).Equals("true", StringComparison.CurrentCultureIgnoreCase);
        }

        private static void WriteBoolRegKey(string key, bool value)
        {
            VersionIndependentRegKey.SetValue(key, value ? "true" : "false");
        }

        private static string ReadStringRegValue(string key, string defaultValue)
        {
            return (string)VersionIndependentRegKey.GetValue(key, defaultValue);
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

        [NotNull]
        public static string CascadeShellMenuItems
        {
            get => ReadStringRegValue("CascadeShellMenuItems", "110111000111111111");
            set => WriteStringRegValue("CascadeShellMenuItems", value);
        }

        [CanBeNull]
        public static string SshPath
        {
            get => ReadStringRegValue("gitssh", null);
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

        [NotNull]
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
                if (IsPortable())
                {
                    SetString("gitcommand", value);
                }
                else
                {
                    WriteStringRegValue("gitcommand", value);
                }
            }
        }

        [NotNull]
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

        public static bool StashKeepIndex
        {
            get => GetBool("stashkeepindex", false);
            set => SetBool("stashkeepindex", value);
        }

        public static bool StashConfirmDropShow
        {
            get => GetBool("stashconfirmdropshow", true);
            set => SetBool("stashconfirmdropshow", value);
        }

        public static bool ApplyPatchIgnoreWhitespace
        {
            get => GetBool("applypatchignorewhitespace", false);
            set => SetBool("applypatchignorewhitespace", value);
        }

        public static bool UseHistogramDiffAlgorithm
        {
            // The settings key has patience in the name for historical reasons
            get => GetBool("usepatiencediffalgorithm", false);
            set => SetBool("usepatiencediffalgorithm", value);
        }

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

        public static readonly BoolNullableSetting ShowConEmuTab = new BoolNullableSetting("ShowConEmuTab", DetailedSettingsPath, true);
        public static readonly StringSetting ConEmuStyle = new StringSetting("ConEmuStyle", DetailedSettingsPath, "<Solarized Light>");
        public static readonly StringSetting ConEmuTerminal = new StringSetting("ConEmuTerminal", DetailedSettingsPath, "bash");
        public static readonly StringSetting ConEmuFontSize = new StringSetting("ConEmuFontSize", DetailedSettingsPath, "12");
        public static readonly BoolNullableSetting ShowGpgInformation = new BoolNullableSetting("ShowGpgInformation", DetailedSettingsPath, true);

        public static CommitInfoPosition CommitInfoPosition
        {
            get => DetailedSettingsPath.GetNullableEnum<CommitInfoPosition>("CommitInfoPosition") ?? (
                DetailedSettingsPath.GetBool("ShowRevisionInfoNextToRevisionGrid") == true // legacy setting
                    ? CommitInfoPosition.RightwardFromList
                    : CommitInfoPosition.BelowList);
            set => DetailedSettingsPath.SetEnum("CommitInfoPosition", value);
        }

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

        public static bool SortByAuthorDate
        {
            get => GetBool("sortbyauthordate", false);
            set => SetBool("sortbyauthordate", value);
        }

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

        [NotNull]
        public static string AvatarImageCachePath => Path.Combine(ApplicationDataPath.Value, "Images\\");

        public static GravatarFallbackAvatarType GravatarFallbackAvatarType
        {
            get => Enum.TryParse(GetString("GravatarDefaultImageType", "Identicon"), out GravatarFallbackAvatarType type)
                ? type
                : GravatarFallbackAvatarType.Identicon;
            set => SetString("GravatarDefaultImageType", value.ToString());
        }

        /// <summary>
        /// Gets the size of the commit author avatar. Set to 80px.
        /// </summary>
        /// <remarks>The value should be scaled with DPI.</remarks>
        public static int AuthorImageSizeInCommitInfo => 80;

        public static int AvatarImageCacheDays
        {
            get => GetInt("authorimagecachedays", 5);
            set => SetInt("authorimagecachedays", value);
        }

        public static bool ShowAuthorAvatarInCommitInfo
        {
            get => GetBool("showauthorgravatar", true);
            set => SetBool("showauthorgravatar", value);
        }

        public static AvatarProvider AvatarProvider
        {
            get => GetEnum("Appearance.AvatarProvider", AvatarProvider.Gravatar);
            set => SetEnum("Appearance.AvatarProvider", value);
        }

        #endregion

        [NotNull]
        public static string Translation
        {
            get => GetString("translation", "");
            set => SetString("translation", value);
        }

        private static string _currentTranslation;

        [NotNull]
        public static string CurrentTranslation
        {
            get => _currentTranslation ?? Translation;
            set => _currentTranslation = value;
        }

        private static readonly Dictionary<string, string> _languageCodes =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
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
                if (_languageCodes.TryGetValue(CurrentTranslation, out var code))
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

        public static bool RefreshCommitDialogOnFormFocus
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
            get => GetBool("simplifymergesinfileHistory", true);
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
            get => GetBool("DetectCopyInFileOnBlame", true);
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

        public static bool RevisionGraphShowWorkingDirChanges
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

        public static readonly Dictionary<string, Encoding> AvailableEncodings = new Dictionary<string, Encoding>();

        public enum PullAction
        {
            None,
            Merge,
            Rebase,
            Fetch,
            FetchAll,
            FetchPruneAll,
            Default
        }

        /// <summary>
        /// Gets or sets the default pull action that is performed by the toolbar icon when it is clicked on.
        /// </summary>
        public static PullAction DefaultPullAction
        {
            get => GetEnum("DefaultPullAction", PullAction.Merge);
            set => SetEnum("DefaultPullAction", value);
        }

        /// <summary>
        /// Gets or sets the default pull action as configured in the FormPull dialog.
        /// </summary>
        public static PullAction FormPullAction
        {
            get => GetEnum("FormPullAction", PullAction.Merge);
            set => SetEnum("FormPullAction", value);
        }

        public static string SmtpServer
        {
            get => GetString("SmtpServer", "smtp.gmail.com");
            set => SetString("SmtpServer", value);
        }

        public static int SmtpPort
        {
            get => GetInt("SmtpPort", 465);
            set => SetInt("SmtpPort", value);
        }

        public static bool SmtpUseSsl
        {
            get => GetBool("SmtpUseSsl", true);
            set => SetBool("SmtpUseSsl", value);
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

        public static bool DontConfirmCommitIfNoBranch
        {
            get => GetBool("DontConfirmCommitIfNoBranch", false);
            set => SetBool("DontConfirmCommitIfNoBranch", value);
        }

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

        public static PullAction? AutoPullOnPushRejectedAction
        {
            get => GetNullableEnum<PullAction>("AutoPullOnPushRejectedAction");
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

        public static bool ShowReflogReferences
        {
            get => GetBool("showReflogReferences", false);
            set => SetBool("showReflogReferences", value);
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
            get => SettingsContainer.Dictionary;
            set => SettingsContainer.Dictionary = value;
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

        public static bool UseFastChecks
        {
            get => GetBool("usefastchecks", false);
            set => SetBool("usefastchecks", value);
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

        public static bool ShowMergeCommits
        {
            get => GetBool("showmergecommits", true);
            set => SetBool("showmergecommits", value);
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

        public static bool ShowCurrentBranchOnly
        {
            get => GetBool("showcurrentbranchonly", false);
            set => SetBool("showcurrentbranchonly", value);
        }

        public static bool ShowSimplifyByDecoration
        {
            get => GetBool("showsimplifybydecoration", false);
            set => SetBool("showsimplifybydecoration", value);
        }

        public static bool BranchFilterEnabled
        {
            get => GetBool("branchfilterenabled", false);
            set => SetBool("branchfilterenabled", value);
        }

        public static bool ShowFirstParent
        {
            get => GetBool("showfirstparent", false);
            set => SetBool("showfirstparent", value);
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

        /// <summary>Gets or sets the path to the git application executable.</summary>
        public static string GitBinDir
        {
            get => GetString("gitbindir", "");
            set
            {
                var temp = value.EnsureTrailingPathSeparator();
                SetString("gitbindir", temp);

                ////if (string.IsNullOrEmpty(_gitBinDir))
                ////   return;
                ////
                ////var path =
                ////   Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Process) + ";" +
                ////   _gitBinDir;
                ////Environment.SetEnvironmentVariable("path", path, EnvironmentVariableTarget.Process);
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

        public static int DiffVerticalRulerPosition
        {
            get => GetInt("diffverticalrulerposition", 0);
            set => SetInt("diffverticalrulerposition", value);
        }

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
            get
            {
                return new ThemeId(
                    GetString("uitheme", string.Empty),
                    GetBool("uithemeisbuiltin", true));
            }
            set
            {
                SetString("uitheme", value.Name ?? string.Empty);
                SetBool("uithemeisbuiltin", value.IsBuiltin);
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

        public static string LastFormatPatchDir
        {
            get => GetString("lastformatpatchdir", "");
            set => SetString("lastformatpatchdir", value);
        }

        public static IgnoreWhitespaceKind IgnoreWhitespaceKind
        {
            get => GetEnum("IgnoreWhitespaceKind", IgnoreWhitespaceKind.None);
            set => SetEnum("IgnoreWhitespaceKind", value);
        }

        public static bool RememberIgnoreWhiteSpacePreference
        {
            get => GetBool("rememberIgnoreWhiteSpacePreference", true);
            set => SetBool("rememberIgnoreWhiteSpacePreference", value);
        }

        public static bool ShowNonPrintingChars
        {
            get => RememberShowNonPrintingCharsPreference && GetBool("ShowNonPrintingChars", false);
            set
            {
                if (RememberShowNonPrintingCharsPreference)
                {
                    SetBool("ShowNonPrintingChars", value);
                }
            }
        }

        public static bool RememberShowNonPrintingCharsPreference
        {
            get => GetBool("RememberShowNonPrintableCharsPreference", false);
            set => SetBool("RememberShowNonPrintableCharsPreference", value);
        }

        public static bool ShowEntireFile
        {
            get => RememberShowEntireFilePreference && GetBool("ShowEntireFile", false);
            set
            {
                if (RememberShowEntireFilePreference)
                {
                    SetBool("ShowEntireFile", value);
                }
            }
        }

        public static bool RememberShowEntireFilePreference
        {
            get => GetBool("RememberShowEntireFilePreference", false);
            set => SetBool("RememberShowEntireFilePreference", value);
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
            get => GetBool("RememberNumberOfContextLines", false);
            set => SetBool("RememberNumberOfContextLines", value);
        }

        public static bool ShowSyntaxHighlightingInDiff
        {
            get => RememberShowSyntaxHighlightingInDiff && GetBool("ShowSyntaxHighlightingInDiff", true);
            set
            {
                if (RememberShowSyntaxHighlightingInDiff)
                {
                    SetBool("ShowSyntaxHighlightingInDiff", value);
                }
            }
        }

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
                    SshPath = SshPathLocatorInstance.Find(GitBinDir);
                    SettingsContainer.Save();
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
                GitCommandHelpers.SetSsh(SshPath);
            }
            catch
            {
            }
        }

        public static bool DashboardShowCurrentBranch
        {
            get => GetBool("dashboardshowcurrentbranch", true);
            set => SetBool("dashboardshowcurrentbranch", value);
        }

        public static string OwnScripts
        {
            get => GetString("ownScripts", "");
            set => SetString("ownScripts", value);
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

        public static int MaxMostRecentRepositories
        {
            get => GetInt("MaxMostRecentRepositories", 0);
            set => SetInt("MaxMostRecentRepositories", value);
        }

        public static int RecentRepositoriesHistorySize
        {
            get => GetInt("history size", 30);
            set => SetInt("history size", value);
        }

        public static int RecentReposComboMinWidth
        {
            get => GetInt("RecentReposComboMinWidth", 0);
            set => SetInt("RecentReposComboMinWidth", value);
        }

        public static string SerializedHotkeys
        {
            get => GetString("SerializedHotkeys", null);
            set => SetString("SerializedHotkeys", value);
        }

        public static bool SortMostRecentRepos
        {
            get => GetBool("SortMostRecentRepos", false);
            set => SetBool("SortMostRecentRepos", value);
        }

        public static bool SortLessRecentRepos
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

        public static DiffListSortType DiffListSorting
        {
            get => GetEnum("DiffListSortType", DiffListSortType.FilePath);
            set => SetEnum("DiffListSortType", value);
        }

        public static string GetGitExtensionsFullPath()
        {
            return _applicationExecutablePath;
        }

        [CanBeNull]
        public static string GetGitExtensionsDirectory()
        {
            return Path.GetDirectoryName(GetGitExtensionsFullPath());
        }

        private static RegistryKey _versionIndependentRegKey;

        [CanBeNull]
        private static RegistryKey VersionIndependentRegKey
        {
            get
            {
                if (_versionIndependentRegKey == null)
                {
                    _versionIndependentRegKey = Registry.CurrentUser.CreateSubKey("Software\\GitExtensions", RegistryKeyPermissionCheck.ReadWriteSubTree);
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
            get => GetBool("RepoObjectsTree.ShowTags", false);
            set => SetBool("RepoObjectsTree.ShowTags", value);
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

        public static bool IsPortable()
        {
            return Properties.Settings.Default.IsPortable;
        }

        private static IEnumerable<(string name, string value)> GetSettingsFromRegistry()
        {
            RegistryKey oldSettings = VersionIndependentRegKey.OpenSubKey("GitExtensions");

            if (oldSettings == null)
            {
                yield break;
            }

            foreach (string name in oldSettings.GetValueNames())
            {
                object value = oldSettings.GetValue(name, null);

                if (value != null)
                {
                    yield return (name, value.ToString());
                }
            }
        }

        private static void ImportFromRegistry()
        {
            SettingsContainer.SettingsCache.Import(GetSettingsFromRegistry());
        }

        public static bool? GetBool(string name)
        {
            return SettingsContainer.GetBool(name);
        }

        public static bool GetBool(string name, bool defaultValue)
        {
            return SettingsContainer.GetBool(name, defaultValue);
        }

        public static void SetBool(string name, bool? value)
        {
            SettingsContainer.SetBool(name, value);
        }

        public static void SetInt(string name, int? value)
        {
            SettingsContainer.SetInt(name, value);
        }

        public static int? GetInt(string name)
        {
            return SettingsContainer.GetInt(name);
        }

        public static DateTime GetDate(string name, DateTime defaultValue)
        {
            return SettingsContainer.GetDate(name, defaultValue);
        }

        public static void SetDate(string name, DateTime? value)
        {
            SettingsContainer.SetDate(name, value);
        }

        public static DateTime? GetDate(string name)
        {
            return SettingsContainer.GetDate(name);
        }

        public static int GetInt(string name, int defaultValue)
        {
            return SettingsContainer.GetInt(name, defaultValue);
        }

        public static void SetFont(string name, Font value)
        {
            SettingsContainer.SetFont(name, value);
        }

        public static Font GetFont(string name, Font defaultValue)
        {
            return SettingsContainer.GetFont(name, defaultValue);
        }

        [Obsolete("AppSettings is no longer responsible for colors, ThemeModule is")]
        public static Color GetColor(AppColor name)
        {
            return SettingsContainer.GetColor(name.ToString().ToLowerInvariant() + "color", AppColorDefaults.GetBy(name));
        }

        public static void SetEnum<T>(string name, T value)
        {
            SettingsContainer.SetEnum(name, value);
        }

        public static T GetEnum<T>(string name, T defaultValue) where T : struct, Enum
        {
            return SettingsContainer.GetEnum(name, defaultValue);
        }

        public static void SetNullableEnum<T>(string name, T? value) where T : struct, Enum
        {
            SettingsContainer.SetNullableEnum(name, value);
        }

        public static T? GetNullableEnum<T>(string name) where T : struct
        {
            return SettingsContainer.GetNullableEnum<T>(name);
        }

        public static void SetString(string name, string value)
        {
            SettingsContainer.SetString(name, value);
        }

        public static string GetString(string name, string defaultValue)
        {
            return SettingsContainer.GetString(name, defaultValue);
        }

        private static void LoadEncodings()
        {
            void AddEncoding(Encoding e)
            {
                AvailableEncodings[e.HeaderName] = e;
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
                AddEncoding(new UTF7Encoding());
                AddEncoding(new UTF8Encoding(false));
                try
                {
                    AddEncoding(Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage));
                }
                catch
                {
                    // there are CultureInfo values without a code page
                }
            }
            else
            {
                var utf8 = new UTF8Encoding(false);
                foreach (var encodingName in availableEncodings.Split(';'))
                {
                    // create utf-8 without BOM
                    if (encodingName == utf8.HeaderName)
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
            string availableEncodings = AvailableEncodings.Values.Select(e => e.HeaderName).Join(";");
            availableEncodings = availableEncodings.Replace(Encoding.Default.HeaderName, "Default");
            SetString("AvailableEncodings", availableEncodings);
        }

        internal static TestAccessor GetTestAccessor() => new TestAccessor();

        internal struct TestAccessor
        {
            public string ApplicationExecutablePath
            {
                get => _applicationExecutablePath;
                set => _applicationExecutablePath = value;
            }
        }
    }

    public class AppSettingsPath : SettingsPath
    {
        public AppSettingsPath([NotNull] string pathName) : base(null, pathName)
        {
        }

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            return AppSettings.SettingsContainer.GetValue(PathFor(name), defaultValue, decode);
        }

        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            AppSettings.SettingsContainer.SetValue(PathFor(name), value, encode);
        }
    }
}
