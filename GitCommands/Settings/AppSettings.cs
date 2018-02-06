﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GitCommands.Logging;
using GitCommands.Repository;
using GitCommands.Settings;
using Microsoft.Win32;
using System.Linq;
using GitCommands.Utils;

namespace GitCommands
{
    public enum LocalChangesAction
    {
        DontChange,
        Merge,
        Reset,
        Stash
    }

    public static class AppSettings
    {
        //semi-constants
        public static readonly char PosixPathSeparator = '/';
        public static Version AppVersion { get { return Assembly.GetCallingAssembly().GetName().Version; } }
        public static string ProductVersion { get { return Application.ProductVersion; } }
        public static readonly string SettingsFileName = "GitExtensions.settings";
        private static readonly ISshPathLocator SshPathLocatorInstance = new SshPathLocator();

        public static readonly Lazy<string> ApplicationDataPath;
        public static string SettingsFilePath { get { return Path.Combine(ApplicationDataPath.Value, SettingsFileName); } }

        private static RepoDistSettings _SettingsContainer;
        public static RepoDistSettings SettingsContainer { get { return _SettingsContainer; } }
        private static readonly SettingsPath DetailedSettingsPath = new AppSettingsPath("Detailed");

        public static readonly int BranchDropDownMinWidth = 300;
        public static readonly int BranchDropDownMaxWidth = 600;

        static AppSettings()
        {
            ApplicationDataPath = new Lazy<string>(() =>
            {
                if (IsPortable())
                {
                    return GetGitExtensionsDirectory();
                }
                else
                {
                    //Make applicationdatapath version independent
                    return Application.UserAppDataPath.Replace(Application.ProductVersion, string.Empty);
                }
            }
            );

            _SettingsContainer = new RepoDistSettings(null, GitExtSettingsCache.FromCache(SettingsFilePath));

            GitLog = new CommandLogger();

            if (!File.Exists(SettingsFilePath))
            {
                ImportFromRegistry();
            }
        }

        public static bool AutoNormaliseBranchName
        {
            get { return GetBool("AutoNormaliseBranchName", true); }
            set { SetBool("AutoNormaliseBranchName", value); }
        }

        public static string AutoNormaliseSymbol
        {
            // when persisted "" is treated as null, so use "+" instead
            get
            {
                var value = GetString("AutoNormaliseSymbol", "_");
                return (value == "+") ? "" : value;
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
            get { return GetBool("RememberAmendCommitState", true); }
            set { SetBool("RememberAmendCommitState", value); }
        }

        public static void UsingContainer(RepoDistSettings aSettingsContainer, Action action)
        {
            SettingsContainer.LockedAction(() =>
                {
                    var oldSC = SettingsContainer;
                    try
                    {
                        _SettingsContainer = aSettingsContainer;
                        action();
                    }
                    finally
                    {
                        _SettingsContainer = oldSC;
                        //refresh settings if needed
                        SettingsContainer.GetString(string.Empty, null);
                    }
                }
             );
        }

        public static string GetInstallDir()
        {
            if (IsPortable())
                return GetGitExtensionsDirectory();

            string dir = ReadStringRegValue("InstallDir", string.Empty);
            if (String.IsNullOrEmpty(dir))
                return GetGitExtensionsDirectory();
            return dir;
        }

        public static string GetResourceDir()
        {
#if DEBUG
            string gitExtDir = GetGitExtensionsDirectory().TrimEnd('\\').TrimEnd('/');
            string debugPath = @"GitExtensions\bin\Debug";
            int len = debugPath.Length;
            if (gitExtDir.Length > len)
            {
                var path = gitExtDir.Substring(gitExtDir.Length - len);
                if (debugPath.ToPosixPath().Equals(path.ToPosixPath()))
                {
                    string projectPath = gitExtDir.Substring(0, gitExtDir.Length - len);
                    return Path.Combine(projectPath, "Bin");
                }
            }
#endif
            return GetInstallDir();
        }

        //for repair only
        public static void SetInstallDir(string dir)
        {
            WriteStringRegValue("InstallDir", dir);
        }

        private static bool ReadBoolRegKey(string key, bool defaultValue)
        {
            object obj = VersionIndependentRegKey.GetValue(key);
            if (!(obj is string))
                obj = null;
            if (obj == null)
                return defaultValue;
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

        public static bool CheckSettings
        {
            get { return ReadBoolRegKey("CheckSettings", true); }
            set { WriteBoolRegKey("CheckSettings", value); }
        }

        public static string CascadeShellMenuItems
        {
            get { return ReadStringRegValue("CascadeShellMenuItems", "110111000111111111"); }
            set { WriteStringRegValue("CascadeShellMenuItems", value); }
        }

        public static string SshPath
        {
            get { return ReadStringRegValue("gitssh", null); }
            set { WriteStringRegValue("gitssh", value); }
        }

        public static bool AlwaysShowAllCommands
        {
            get { return ReadBoolRegKey("AlwaysShowAllCommands", false); }
            set { WriteBoolRegKey("AlwaysShowAllCommands", value); }
        }

        public static bool ShowCurrentBranchInVisualStudio
        {
            //This setting MUST be set to false by default, otherwise it will not work in Visual Studio without
            //other changes in the Visual Studio plugin itself.
            get { return ReadBoolRegKey("ShowCurrentBranchInVS", true); }
            set { WriteBoolRegKey("ShowCurrentBranchInVS", value); }
        }

        public static string GitCommandValue
        {
            get
            {
                if (IsPortable())
                    return GetString("gitcommand", "");
                else
                    return ReadStringRegValue("gitcommand", "");
            }
            set
            {
                if (IsPortable())
                    SetString("gitcommand", value);
                else
                    WriteStringRegValue("gitcommand", value);
            }
        }

        public static string GitCommand
        {
            get
            {
                if (String.IsNullOrEmpty(GitCommandValue))
                    return "git";
                return GitCommandValue;
            }
        }

        public static int UserMenuLocationX
        {
            get { return GetInt("usermenulocationx", -1); }
            set { SetInt("usermenulocationx", value); }
        }

        public static int UserMenuLocationY
        {
            get { return GetInt("usermenulocationy", -1); }
            set { SetInt("usermenulocationy", value); }
        }

        public static bool StashKeepIndex
        {
            get { return GetBool("stashkeepindex", false); }
            set { SetBool("stashkeepindex", value); }
        }

        public static bool StashConfirmDropShow
        {
            get { return GetBool("stashconfirmdropshow", true); }
            set { SetBool("stashconfirmdropshow", value); }
        }

        public static bool ApplyPatchIgnoreWhitespace
        {
            get { return GetBool("applypatchignorewhitespace", false); }
            set { SetBool("applypatchignorewhitespace", value); }
        }

        public static bool UsePatienceDiffAlgorithm
        {
            get { return GetBool("usepatiencediffalgorithm", false); }
            set { SetBool("usepatiencediffalgorithm", value); }
        }

        public static bool ShowErrorsWhenStagingFiles
        {
            get { return GetBool("showerrorswhenstagingfiles", true); }
            set { SetBool("showerrorswhenstagingfiles", value); }
        }

        public static bool AddNewlineToCommitMessageWhenMissing
        {
            get { return GetBool("addnewlinetocommitmessagewhenmissing", true); }
            set { SetBool("addnewlinetocommitmessagewhenmissing", value); }
        }

        public static string LastCommitMessage
        {
            get { return GetString("lastCommitMessage", ""); }
            set { SetString("lastCommitMessage", value); }
        }

        public static int CommitDialogNumberOfPreviousMessages
        {
            get { return GetInt("commitDialogNumberOfPreviousMessages", 4); }
            set { SetInt("commitDialogNumberOfPreviousMessages", value); }
        }

        public static bool ShowCommitAndPush
        {
            get { return GetBool("showcommitandpush", true); }
            set { SetBool("showcommitandpush", value); }
        }

        public static bool ShowResetUnstagedChanges
        {
            get { return GetBool("showresetunstagedchanges", true); }
            set { SetBool("showresetunstagedchanges", value); }
        }

        public static bool ShowResetAllChanges
        {
            get { return GetBool("showresetallchanges", true); }
            set { SetBool("showresetallchanges", value); }
        }
        
        public static readonly BoolNullableSetting ShowConEmuTab = new BoolNullableSetting("ShowConEmuTab", DetailedSettingsPath, true);
        public static readonly StringSetting ConEmuStyle = new StringSetting("ConEmuStyle", DetailedSettingsPath, "<Solarized Light>");
        public static readonly StringSetting ConEmuTerminal = new StringSetting("ConEmuTerminal", DetailedSettingsPath, "bash");
        public static readonly StringSetting ConEmuFontSize = new StringSetting("ConEmuFontSize", DetailedSettingsPath, "12");
        public static readonly BoolNullableSetting ShowGpgInformation = new BoolNullableSetting("ShowGpgInformation", DetailedSettingsPath, false);

        public static bool ShowRevisionInfoNextToRevisionGrid
        {
            get { return DetailedSettingsPath.GetBool("ShowRevisionInfoNextToRevisionGrid", false); }
            set { DetailedSettingsPath.SetBool("ShowRevisionInfoNextToRevisionGrid", value); }
        }

        public static bool ProvideAutocompletion
        {
            get { return GetBool("provideautocompletion", true); }
            set { SetBool("provideautocompletion", value); }
        }

        public static string TruncatePathMethod
        {
            get { return GetString("truncatepathmethod", "none"); }
            set { SetString("truncatepathmethod", value); }
        }

        public static bool ShowGitStatusInBrowseToolbar
        {
            get { return GetBool("showgitstatusinbrowsetoolbar", true); }
            set { SetBool("showgitstatusinbrowsetoolbar", value); }
        }

        public static bool CommitInfoShowContainedInBranches
        {
            get
            {
                return CommitInfoShowContainedInBranchesLocal ||
                    CommitInfoShowContainedInBranchesRemote ||
                    CommitInfoShowContainedInBranchesRemoteIfNoLocal;
            }
        }

        public static bool CommitInfoShowContainedInBranchesLocal
        {
            get { return GetBool("commitinfoshowcontainedinbrancheslocal", true); }
            set { SetBool("commitinfoshowcontainedinbrancheslocal", value); }
        }

        public static bool CheckForUncommittedChangesInCheckoutBranch
        {
            get { return GetBool("checkforuncommittedchangesincheckoutbranch", true); }
            set { SetBool("checkforuncommittedchangesincheckoutbranch", value); }
        }

        public static bool AlwaysShowCheckoutBranchDlg
        {
            get { return GetBool("AlwaysShowCheckoutBranchDlg", false); }
            set { SetBool("AlwaysShowCheckoutBranchDlg", value); }
        }

        public static bool CommitAndPushForcedWhenAmend
        {
            get { return GetBool("CommitAndPushForcedWhenAmend", false); }
            set { SetBool("CommitAndPushForcedWhenAmend", value); }
        }

        public static bool CommitInfoShowContainedInBranchesRemote
        {
            get { return GetBool("commitinfoshowcontainedinbranchesremote", false); }
            set { SetBool("commitinfoshowcontainedinbranchesremote", value); }
        }

        public static bool CommitInfoShowContainedInBranchesRemoteIfNoLocal
        {
            get { return GetBool("commitinfoshowcontainedinbranchesremoteifnolocal", false); }
            set { SetBool("commitinfoshowcontainedinbranchesremoteifnolocal", value); }
        }

        public static bool CommitInfoShowContainedInTags
        {
            get { return GetBool("commitinfoshowcontainedintags", true); }
            set { SetBool("commitinfoshowcontainedintags", value); }
        }

        public static string GravatarCachePath
        {
            get { return Path.Combine(ApplicationDataPath.Value, "Images\\"); }
        }

        public static string Translation
        {
            get { return GetString("translation", ""); }
            set { SetString("translation", value); }
        }

        private static string _currentTranslation;
        public static string CurrentTranslation
        {
            get { return _currentTranslation ?? Translation; }
            set { _currentTranslation = value; }
        }


        private static readonly Dictionary<string, string> _languageCodes =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"Czech", "cs"},
                {"Dutch", "nl"},
                {"English", "en"},
                {"French", "fr"},
                {"German", "de"},
                {"Indonesian", "id"},
                {"Italian", "it"},
                {"Japanese", "ja"},
                {"Korean", "ko"},
                {"Polish", "pl"},
                {"Portuguese (Brazil)", "pt-BR"},
                {"Portuguese (Portugal)", "pt-PT"},
                {"Romanian", "ro"},
                {"Russian", "ru"},
                {"Simplified Chinese", "zh-CN"},
                {"Spanish", "es"},
                {"Traditional Chinese", "zh-TW"}
            };

        public static string CurrentLanguageCode
        {
            get
            {
                string code;
                if (_languageCodes.TryGetValue(CurrentTranslation, out code))
                    return code;
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
                catch (System.Globalization.CultureNotFoundException)
                {
                    Debug.WriteLine("Culture {0} not found", CurrentLanguageCode);
                    return CultureInfo.GetCultureInfo("en");
                }
            }
        }

        public static bool UserProfileHomeDir
        {
            get { return GetBool("userprofilehomedir", false); }
            set { SetBool("userprofilehomedir", value); }
        }

        public static string CustomHomeDir
        {
            get { return GetString("customhomedir", ""); }
            set { SetString("customhomedir", value); }
        }

        public static bool EnableAutoScale
        {
            get { return GetBool("enableautoscale", true); }
            set { SetBool("enableautoscale", value); }
        }

        public static string IconColor
        {
            get { return GetString("iconcolor", "default"); }
            set { SetString("iconcolor", value); }
        }

        public static string IconStyle
        {
            get { return GetString("iconstyle", "default"); }
            set { SetString("iconstyle", value); }
        }

        /// <summary>
        /// Gets the size of the commit author avatar. Set to 80px.
        /// </summary>
        /// <remarks>The value should be scaled with DPI.</remarks>
        public static int AuthorImageSize => 80;

        public static int AuthorImageCacheDays
        {
            get { return GetInt("authorimagecachedays", 5); }
            set { SetInt("authorimagecachedays", value); }
        }

        public static bool ShowAuthorGravatar
        {
            get { return GetBool("showauthorgravatar", true); }
            set { SetBool("showauthorgravatar", value); }
        }

        public static bool CloseCommitDialogAfterCommit
        {
            get { return GetBool("closecommitdialogaftercommit", true); }
            set { SetBool("closecommitdialogaftercommit", value); }
        }

        public static bool CloseCommitDialogAfterLastCommit
        {
            get { return GetBool("closecommitdialogafterlastcommit", true); }
            set { SetBool("closecommitdialogafterlastcommit", value); }
        }

        public static bool RefreshCommitDialogOnFormFocus
        {
            get { return GetBool("refreshcommitdialogonformfocus", false); }
            set { SetBool("refreshcommitdialogonformfocus", value); }
        }

        public static bool StageInSuperprojectAfterCommit
        {
            get { return GetBool("stageinsuperprojectaftercommit", true); }
            set { SetBool("stageinsuperprojectaftercommit", value); }
        }

        public static bool PlaySpecialStartupSound
        {
            get { return GetBool("PlaySpecialStartupSound", false); }
            set { SetBool("PlaySpecialStartupSound", value); }
        }

        public static bool FollowRenamesInFileHistory
        {
            get { return GetBool("followrenamesinfilehistory", true); }
            set { SetBool("followrenamesinfilehistory", value); }
        }

        public static bool FollowRenamesInFileHistoryExactOnly
        {
            get { return GetBool("followrenamesinfilehistoryexactonly", false); }
            set { SetBool("followrenamesinfilehistoryexactonly", value); }
        }

        public static bool FullHistoryInFileHistory
        {
            get { return GetBool("fullhistoryinfilehistory", false); }
            set { SetBool("fullhistoryinfilehistory", value); }
        }

        public static bool LoadFileHistoryOnShow
        {
            get { return GetBool("LoadFileHistoryOnShow", true); }
            set { SetBool("LoadFileHistoryOnShow", value); }
        }

        public static bool LoadBlameOnShow
        {
            get { return GetBool("LoadBlameOnShow", true); }
            set { SetBool("LoadBlameOnShow", value); }
        }

        public static bool OpenSubmoduleDiffInSeparateWindow
        {
            get { return GetBool("opensubmodulediffinseparatewindow", false); }
            set { SetBool("opensubmodulediffinseparatewindow", value); }
        }

        public static bool RevisionGraphShowWorkingDirChanges
        {
            get { return GetBool("revisiongraphshowworkingdirchanges", false); }
            set { SetBool("revisiongraphshowworkingdirchanges", value); }
        }

        public static bool RevisionGraphDrawAlternateBackColor
        {
            get { return GetBool("RevisionGraphDrawAlternateBackColor", true); }
            set { SetBool("RevisionGraphDrawAlternateBackColor", value); }
        }

        public static bool RevisionGraphDrawNonRelativesGray
        {
            get { return GetBool("revisiongraphdrawnonrelativesgray", true); }
            set { SetBool("revisiongraphdrawnonrelativesgray", value); }
        }

        public static bool RevisionGraphDrawNonRelativesTextGray
        {
            get { return GetBool("revisiongraphdrawnonrelativestextgray", false); }
            set { SetBool("revisiongraphdrawnonrelativestextgray", value); }
        }

        public static readonly Dictionary<string, Encoding> AvailableEncodings = new Dictionary<string, Encoding>();

        public enum PullAction
        {
            None,
            Merge,
            Rebase,
            Fetch,
            FetchAll,
            Default
        }

        public static PullAction FormPullAction
        {
            get { return GetEnum("FormPullAction", PullAction.Merge); }
            set { SetEnum("FormPullAction", value); }
        }

        public static bool SetNextPullActionAsDefault
        {
            get { return !GetBool("DonSetAsLastPullAction", true); }
            set { SetBool("DonSetAsLastPullAction", !value); }
        }

        public static string SmtpServer
        {
            get { return GetString("SmtpServer", "smtp.gmail.com"); }
            set { SetString("SmtpServer", value); }
        }

        public static int SmtpPort
        {
            get { return GetInt("SmtpPort", 465); }
            set { SetInt("SmtpPort", value); }
        }

        public static bool SmtpUseSsl
        {
            get { return GetBool("SmtpUseSsl", true); }
            set { SetBool("SmtpUseSsl", value); }
        }

        public static bool AutoStash
        {
            get { return GetBool("autostash", false); }
            set { SetBool("autostash", value); }
        }

        public static bool RebaseAutoStash
        {
            get { return GetBool("RebaseAutostash", false); }
            set { SetBool("RebaseAutostash", value); }
        }

        public static LocalChangesAction CheckoutBranchAction
        {
            get { return GetEnum("checkoutbranchaction", LocalChangesAction.DontChange); }
            set { SetEnum("checkoutbranchaction", value); }
        }

        public static bool UseDefaultCheckoutBranchAction
        {
            get { return GetBool("UseDefaultCheckoutBranchAction", false); }
            set { SetBool("UseDefaultCheckoutBranchAction", value); }
        }

        public static bool DontShowHelpImages
        {
            get { return GetBool("DontShowHelpImages", false); }
            set { SetBool("DontShowHelpImages", value); }
        }

        public static bool AlwaysShowAdvOpt
        {
            get { return GetBool("AlwaysShowAdvOpt", false); }
            set { SetBool("AlwaysShowAdvOpt", value); }
        }

        public static bool DontConfirmAmend
        {
            get { return GetBool("DontConfirmAmend", false); }
            set { SetBool("DontConfirmAmend", value); }
        }

        public static bool DontConfirmCommitIfNoBranch
        {
            get { return GetBool("DontConfirmCommitIfNoBranch", false); }
            set { SetBool("DontConfirmCommitIfNoBranch", value); }
        }

        public static bool? AutoPopStashAfterPull
        {
            get { return GetBool("AutoPopStashAfterPull"); }
            set { SetBool("AutoPopStashAfterPull", value); }
        }

        public static bool? AutoPopStashAfterCheckoutBranch
        {
            get { return GetBool("AutoPopStashAfterCheckoutBranch"); }
            set { SetBool("AutoPopStashAfterCheckoutBranch", value); }
        }

        public static PullAction? AutoPullOnPushRejectedAction
        {
            get { return GetNullableEnum<PullAction>("AutoPullOnPushRejectedAction"); }
            set { SetNullableEnum<PullAction>("AutoPullOnPushRejectedAction", value); }
        }

        public static bool DontConfirmPushNewBranch
        {
            get { return GetBool("DontConfirmPushNewBranch", false); }
            set { SetBool("DontConfirmPushNewBranch", value); }
        }

        public static bool DontConfirmAddTrackingRef
        {
            get { return GetBool("DontConfirmAddTrackingRef", false); }
            set { SetBool("DontConfirmAddTrackingRef", value); }
        }

        public static bool DontConfirmCommitAfterConflictsResolved
        {
            get { return GetBool("DontConfirmCommitAfterConflictsResolved", false); }
            set { SetBool("DontConfirmCommitAfterConflictsResolved", value); }
        }

        public static bool DontConfirmSecondAbortConfirmation
        {
            get { return GetBool("DontConfirmSecondAbortConfirmation", false); }
            set { SetBool("DontConfirmSecondAbortConfirmation", value); }
        }

        public static bool DontConfirmResolveConflicts
        {
            get { return GetBool("DontConfirmResolveConflicts", false); }
            set { SetBool("DontConfirmResolveConflicts", value); }
        }

        public static bool IncludeUntrackedFilesInAutoStash
        {
            get { return GetBool("includeUntrackedFilesInAutoStash", false); }
            set { SetBool("includeUntrackedFilesInAutoStash", value); }
        }

        public static bool IncludeUntrackedFilesInManualStash
        {
            get { return GetBool("includeUntrackedFilesInManualStash", false); }
            set { SetBool("includeUntrackedFilesInManualStash", value); }
        }

        public static bool OrderRevisionByDate
        {
            get { return GetBool("orderrevisionbydate", true); }
            set { SetBool("orderrevisionbydate", value); }
        }

        public static bool ShowRemoteBranches
        {
            get { return GetBool("showRemoteBranches", true); }
            set { SetBool("showRemoteBranches", value); }
        }

        public static bool ShowReflogReferences
        {
            get { return GetBool("showReflogReferences", false); }
            set { SetBool("showReflogReferences", value); }
        }

        public static bool ShowSuperprojectTags
        {
            get { return GetBool("showSuperprojectTags", false); }
            set { SetBool("showSuperprojectTags", value); }
        }

        public static bool ShowSuperprojectBranches
        {
            get { return GetBool("showSuperprojectBranches", true); }
            set { SetBool("showSuperprojectBranches", value); }
        }

        public static bool ShowSuperprojectRemoteBranches
        {
            get { return GetBool("showSuperprojectRemoteBranches", false); }
            set { SetBool("showSuperprojectRemoteBranches", value); }
        }

        public static bool? UpdateSubmodulesOnCheckout
        {
            get { return GetBool("updateSubmodulesOnCheckout"); }
            set { SetBool("updateSubmodulesOnCheckout", value); }
        }

        public static string Dictionary
        {
            get { return SettingsContainer.Dictionary; }
            set { SettingsContainer.Dictionary = value; }
        }

        public static bool ShowGitCommandLine
        {
            get { return GetBool("showgitcommandline", false); }
            set { SetBool("showgitcommandline", value); }
        }

        public static bool ShowStashCount
        {
            get { return GetBool("showstashcount", false); }
            set { SetBool("showstashcount", value); }
        }

        public static bool RelativeDate
        {
            get { return GetBool("relativedate", true); }
            set { SetBool("relativedate", value); }
        }

        public static bool UseFastChecks
        {
            get { return GetBool("usefastchecks", false); }
            set { SetBool("usefastchecks", value); }
        }

        public static bool ShowGitNotes
        {
            get { return GetBool("showgitnotes", false); }
            set { SetBool("showgitnotes", value); }
        }

        public static bool ShowIndicatorForMultilineMessage
        {
            get { return GetBool("showindicatorformultilinemessage", false); }
            set { SetBool("showindicatorformultilinemessage", value); }
        }

        public static bool ShowAnnotatedTagsMessages
        {
            get { return GetBool("showannotatedtagsmessages", true); }
            set { SetBool("showannotatedtagsmessages", value); }
        }

        public static bool ShowMergeCommits
        {
            get { return GetBool("showmergecommits", true); }
            set { SetBool("showmergecommits", value); }
        }

        public static bool ShowTags
        {
            get { return GetBool("showtags", true); }
            set { SetBool("showtags", value); }
        }

        public static bool ShowIds
        {
            get { return GetBool("showids", false); }
            set { SetBool("showids", value); }
        }

        public static int RevisionGraphLayout
        {
            get { return GetInt("revisiongraphlayout", 2); }
            set { SetInt("revisiongraphlayout", value); }
        }

        public static bool ShowAuthorDate
        {
            get { return GetBool("showauthordate", true); }
            set { SetBool("showauthordate", value); }
        }

        public static bool CloseProcessDialog
        {
            get { return GetBool("closeprocessdialog", false); }
            set { SetBool("closeprocessdialog", value); }
        }

        public static bool ShowCurrentBranchOnly
        {
            get { return GetBool("showcurrentbranchonly", false); }
            set { SetBool("showcurrentbranchonly", value); }
        }

        public static bool ShowSimplifyByDecoration
        {
            get { return GetBool("showsimplifybydecoration", false); }
            set { SetBool("showsimplifybydecoration", value); }
        }

        public static bool BranchFilterEnabled
        {
            get { return GetBool("branchfilterenabled", false); }
            set { SetBool("branchfilterenabled", value); }
        }

        public static bool ShowFirstParent
        {
            get { return GetBool("showfirstparent", false); }
            set { SetBool("showfirstparent", value); }
        }

        public static bool CommitDialogSelectionFilter
        {
            get { return GetBool("commitdialogselectionfilter", false); }
            set { SetBool("commitdialogselectionfilter", value); }
        }

        public static string DefaultCloneDestinationPath
        {
            get { return GetString("defaultclonedestinationpath", string.Empty); }
            set { SetString("defaultclonedestinationpath", value); }
        }

        public static int RevisionGridQuickSearchTimeout
        {
            get { return GetInt("revisiongridquicksearchtimeout", 750); }
            set { SetInt("revisiongridquicksearchtimeout", value); }
        }

        public static string GravatarDefaultImageType
        {
            get { return GetString("gravatarfallbackservice", "Identicon"); }
            set { SetString("gravatarfallbackservice", value); }
        }

        /// <summary>Gets or sets the path to the git application executable.</summary>
        public static string GitBinDir
        {
            get { return GetString("gitbindir", ""); }
            set
            {
                var temp = value.EnsureTrailingPathSeparator();
                SetString("gitbindir", temp);

                //if (string.IsNullOrEmpty(_gitBinDir))
                //    return;

                //var path =
                //    Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Process) + ";" +
                //    _gitBinDir;
                //Environment.SetEnvironmentVariable("path", path, EnvironmentVariableTarget.Process);
            }
        }

        public static int MaxRevisionGraphCommits
        {
            get { return GetInt("maxrevisiongraphcommits", 100000); }
            set { SetInt("maxrevisiongraphcommits", value); }
        }

        public static bool ShowDiffForAllParents
        {
            get { return GetBool("showdiffforallparents", true); }
            set { SetBool("showdiffforallparents", value); }
        }

        public static int DiffVerticalRulerPosition
        {
            get { return GetInt( "diffverticalrulerposition", 80 ); }
            set { SetInt( "diffverticalrulerposition", value ); }
        }

        public static string RecentWorkingDir
        {
            get { return GetString("RecentWorkingDir", null); }
            set { SetString("RecentWorkingDir", value); }
        }

        public static bool StartWithRecentWorkingDir
        {
            get { return GetBool("StartWithRecentWorkingDir", false); }
            set { SetBool("StartWithRecentWorkingDir", value); }
        }

        public static CommandLogger GitLog { get; private set; }

        public static string Plink
        {
            get { return GetString("plink", Environment.GetEnvironmentVariable("GITEXT_PLINK") ?? ReadStringRegValue("plink", "")); }
            set { if (value != Environment.GetEnvironmentVariable("GITEXT_PLINK")) SetString("plink", value); }
        }
        public static string Puttygen
        {
            get { return GetString("puttygen", Environment.GetEnvironmentVariable("GITEXT_PUTTYGEN") ?? ReadStringRegValue("puttygen", "")); }
            set { if (value != Environment.GetEnvironmentVariable("GITEXT_PUTTYGEN")) SetString("puttygen", value); }
        }

        /// <summary>Gets the path to Pageant (SSH auth agent).</summary>
        public static string Pageant
        {
            get { return GetString("pageant", Environment.GetEnvironmentVariable("GITEXT_PAGEANT") ?? ReadStringRegValue("pageant", "")); }
            set { if (value != Environment.GetEnvironmentVariable("GITEXT_PAGEANT")) SetString("pageant", value); }
        }

        public static bool AutoStartPageant
        {
            get { return GetBool("autostartpageant", true); }
            set { SetBool("autostartpageant", value); }
        }

        public static bool MarkIllFormedLinesInCommitMsg
        {
            get { return GetBool("markillformedlinesincommitmsg", false); }
            set { SetBool("markillformedlinesincommitmsg", value); }
        }

        #region Colors

        public static Color OtherTagColor
        {
            get { return GetColor("othertagcolor", Color.Gray); }
            set { SetColor("othertagcolor", value); }
        }

        public static Color TagColor
        {
            get { return GetColor("tagcolor", Color.DarkBlue); }
            set { SetColor("tagcolor", value); }
        }

        public static Color GraphColor
        {
            get { return GetColor("graphcolor", Color.DarkRed); }
            set { SetColor("graphcolor", value); }
        }

        public static Color BranchColor
        {
            get { return GetColor("branchcolor", Color.DarkRed); }
            set { SetColor("branchcolor", value); }
        }

        public static Color RemoteBranchColor
        {
            get { return GetColor("remotebranchcolor", Color.Green); }
            set { SetColor("remotebranchcolor", value); }
        }

        public static Color DiffSectionColor
        {
            get { return GetColor("diffsectioncolor", Color.FromArgb(230, 230, 230)); }
            set { SetColor("diffsectioncolor", value); }
        }

        public static Color DiffRemovedColor
        {
            get { return GetColor("diffremovedcolor", Color.FromArgb(255, 200, 200)); }
            set { SetColor("diffremovedcolor", value); }
        }

        public static Color DiffRemovedExtraColor
        {
            get { return GetColor("diffremovedextracolor", Color.FromArgb(255, 150, 150)); }
            set { SetColor("diffremovedextracolor", value); }
        }

        public static Color DiffAddedColor
        {
            get { return GetColor("diffaddedcolor", Color.FromArgb(200, 255, 200)); }
            set { SetColor("diffaddedcolor", value); }
        }

        public static Color DiffAddedExtraColor
        {
            get { return GetColor("diffaddedextracolor", Color.FromArgb(135, 255, 135)); }
            set { SetColor("diffaddedextracolor", value); }
        }

        public static Color AuthoredRevisionsColor
        {
            get { return GetColor("authoredrevisionscolor", Color.LightYellow); }
            set { SetColor("authoredrevisionscolor", value); }
        }

        public static Font DiffFont
        {
            get { return GetFont("difffont", new Font("Courier New", 10)); }
            set { SetFont("difffont", value); }
        }

        public static Font CommitFont
        {
            get { return GetFont("commitfont", new Font(SystemFonts.DialogFont.Name, SystemFonts.MessageBoxFont.Size)); }
            set { SetFont("commitfont", value); }
        }

        public static Font Font
        {
            get { return GetFont("font", new Font(SystemFonts.DialogFont.Name, SystemFonts.DefaultFont.Size)); }
            set { SetFont("font", value); }
        }

        #endregion

        public static bool MulticolorBranches
        {
            get { return GetBool("multicolorbranches", true); }
            set { SetBool("multicolorbranches", value); }
        }

        public static bool StripedBranchChange
        {
            get { return GetBool("stripedbranchchange", true); }
            set { SetBool("stripedbranchchange", value); }
        }

        public static bool BranchBorders
        {
            get { return GetBool("branchborders", true); }
            set { SetBool("branchborders", value); }
        }

        public static bool HighlightAuthoredRevisions
        {
            get { return GetBool("highlightauthoredrevisions", true); }
            set { SetBool("highlightauthoredrevisions", value); }
        }

        public static string LastFormatPatchDir
        {
            get { return GetString("lastformatpatchdir", ""); }
            set { SetString("lastformatpatchdir", value); }
        }

        public static bool IgnoreWhitespaceChanges
        {
            get
            {
                return RememberIgnoreWhiteSpacePreference && GetBool("IgnoreWhitespaceChanges", false);
            }
            set
            {
                if (RememberIgnoreWhiteSpacePreference)
                {
                    SetBool("IgnoreWhitespaceChanges", value);
                }
            }
        }

        public static bool IgnoreAllWhitespaceChanges
        {
            get
            {
                return RememberIgnoreWhiteSpacePreference && GetBool("IgnoreAllWhitespaceChanges", false);
            }
            set
            {
                if (RememberIgnoreWhiteSpacePreference)
                {
                    SetBool("IgnoreAllWhitespaceChanges", value);
                }
            }
        }

        public static bool RememberIgnoreWhiteSpacePreference
        {
            get { return GetBool("rememberIgnoreWhiteSpacePreference", true); }
            set { SetBool("rememberIgnoreWhiteSpacePreference", value); }
        }

        public static bool ShowNonPrintingChars
        {
            get
            {
                return RememberShowNonPrintingCharsPreference && GetBool("ShowNonPrintingChars", false);
            }
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
            get { return GetBool("RememberShowNonPrintableCharsPreference", false); }
            set { SetBool("RememberShowNonPrintableCharsPreference", value); }
        }

        public static bool ShowEntireFile
        {
            get
            {
                return RememberShowEntireFilePreference && GetBool("ShowEntireFile", false);
            }
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
            get { return GetBool("RememberShowEntireFilePreference", false); }
            set { SetBool("RememberShowEntireFilePreference", value); }
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
            get { return GetBool("RememberNumberOfContextLines", false); }
            set { SetBool("RememberNumberOfContextLines", value); }
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
                    Repositories.SaveSettings();

                    SettingsContainer.Save();
                });
            }
            catch
            { }
        }

        public static void LoadSettings()
        {
            LoadEncodings();

            try
            {
                GitCommandHelpers.SetSsh(SshPath);
            }
            catch
            { }
        }

        public static bool DashboardShowCurrentBranch
        {
            get { return GetBool("dashboardshowcurrentbranch", true); }
            set { SetBool("dashboardshowcurrentbranch", value); }
        }

        public static string ownScripts
        {
            get { return GetString("ownScripts", ""); }
            set { SetString("ownScripts", value); }
        }

        public static int RecursiveSubmodules
        {
            get { return GetInt("RecursiveSubmodules", 1); }
            set { SetInt("RecursiveSubmodules", value); }
        }

        public static string ShorteningRecentRepoPathStrategy
        {
            get { return GetString("ShorteningRecentRepoPathStrategy", ""); }
            set { SetString("ShorteningRecentRepoPathStrategy", value); }
        }

        public static int MaxMostRecentRepositories
        {
            get { return GetInt("MaxMostRecentRepositories", 0); }
            set { SetInt("MaxMostRecentRepositories", value); }
        }

        public static int RecentRepositoriesHistorySize
        {
            get { return GetInt("history size", 30); }
            set { SetInt("history size", value); }
        }

        public static int RecentReposComboMinWidth
        {
            get { return GetInt("RecentReposComboMinWidth", 0); }
            set { SetInt("RecentReposComboMinWidth", value); }
        }

        public static string SerializedHotkeys
        {
            get { return GetString("SerializedHotkeys", null); }
            set { SetString("SerializedHotkeys", value); }
        }

        public static bool SortMostRecentRepos
        {
            get { return GetBool("SortMostRecentRepos", false); }
            set { SetBool("SortMostRecentRepos", value); }
        }

        public static bool SortLessRecentRepos
        {
            get { return GetBool("SortLessRecentRepos", false); }
            set { SetBool("SortLessRecentRepos", value); }
        }

        public static bool DontCommitMerge
        {
            get { return GetBool("DontCommitMerge", false); }
            set { SetBool("DontCommitMerge", value); }
        }

        public static int CommitValidationMaxCntCharsFirstLine
        {
            get { return GetInt("CommitValidationMaxCntCharsFirstLine", 0); }
            set { SetInt("CommitValidationMaxCntCharsFirstLine", value); }
        }

        public static int CommitValidationMaxCntCharsPerLine
        {
            get { return GetInt("CommitValidationMaxCntCharsPerLine", 0); }
            set { SetInt("CommitValidationMaxCntCharsPerLine", value); }
        }

        public static bool CommitValidationSecondLineMustBeEmpty
        {
            get { return GetBool("CommitValidationSecondLineMustBeEmpty", false); }
            set { SetBool("CommitValidationSecondLineMustBeEmpty", value); }
        }

        public static bool CommitValidationIndentAfterFirstLine
        {
            get { return GetBool("CommitValidationIndentAfterFirstLine", true); }
            set { SetBool("CommitValidationIndentAfterFirstLine", value); }
        }

        public static bool CommitValidationAutoWrap
        {
            get { return GetBool("CommitValidationAutoWrap", true); }
            set { SetBool("CommitValidationAutoWrap", value); }
        }

        public static string CommitValidationRegEx
        {
            get { return GetString("CommitValidationRegEx", String.Empty); }
            set { SetString("CommitValidationRegEx", value); }
        }

        public static string CommitTemplates
        {
            get { return GetString("CommitTemplates", String.Empty); }
            set { SetString("CommitTemplates", value); }
        }

        public static bool CreateLocalBranchForRemote
        {
            get { return GetBool("CreateLocalBranchForRemote", false); }
            set { SetBool("CreateLocalBranchForRemote", value); }
        }

        public static bool UseFormCommitMessage
        {
            get { return GetBool("UseFormCommitMessage", true); }
            set { SetBool("UseFormCommitMessage", value); }
        }

        public static bool CommitAutomaticallyAfterCherryPick
        {
            get { return GetBool("CommitAutomaticallyAfterCherryPick", false); }
            set { SetBool("CommitAutomaticallyAfterCherryPick", value); }
        }

        public static bool AddCommitReferenceToCherryPick
        {
            get { return GetBool("AddCommitReferenceToCherryPick", false); }
            set { SetBool("AddCommitReferenceToCherryPick", value); }
        }

        public static DateTime LastUpdateCheck
        {
            get { return GetDate("LastUpdateCheck", default(DateTime)); }
            set { SetDate("LastUpdateCheck", value); }
        }

        public static bool CheckForReleaseCandidates
        {
            get { return GetBool("CheckForReleaseCandidates", false); }
            set { SetBool("CheckForReleaseCandidates", value); }
        }

        public static bool OmitUninterestingDiff
        {
            get { return GetBool("OmitUninterestingDiff", false); }
            set { SetBool("OmitUninterestingDiff", value); }
        }

        public static bool UseConsoleEmulatorForCommands
        {
            get { return GetBool("UseConsoleEmulatorForCommands", true); }
            set { SetBool("UseConsoleEmulatorForCommands", value); }
        }

        public static BranchOrdering BranchOrderingCriteria
        {
            get { return GetEnum("BranchOrderingCriteria", BranchOrdering.ByLastAccessDate); }
            set { SetEnum("BranchOrderingCriteria", value); }
        }

        public static string GetGitExtensionsFullPath()
        {
            return Application.ExecutablePath;
        }

        public static string GetGitExtensionsDirectory()
        {
            return Path.GetDirectoryName(GetGitExtensionsFullPath());
        }

        private static RegistryKey _VersionIndependentRegKey;

        private static RegistryKey VersionIndependentRegKey
        {
            get
            {
                if (_VersionIndependentRegKey == null)
                    _VersionIndependentRegKey = Registry.CurrentUser.CreateSubKey("Software\\GitExtensions", RegistryKeyPermissionCheck.ReadWriteSubTree);
                return _VersionIndependentRegKey;
            }
        }

        public static bool IsPortable()
        {
            return Properties.Settings.Default.IsPortable;

        }

        private static IEnumerable<Tuple<string, string>> GetSettingsFromRegistry()
        {
            RegistryKey oldSettings = VersionIndependentRegKey.OpenSubKey("GitExtensions");
            if (oldSettings == null)
                yield break;

            foreach (String name in oldSettings.GetValueNames())
            {
                object value = oldSettings.GetValue(name, null);
                if (value != null)
                    yield return new Tuple<string, string>(name, value.ToString());
            }
        }

        private static void ImportFromRegistry()
        {
            SettingsContainer.SettingsCache.Import(GetSettingsFromRegistry());
        }

        public static string PrefixedName(string prefix, string name)
        {
            return prefix == null ? name : prefix + '_' + name;
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

        public static void SetColor(string name, Color? value)
        {
            SettingsContainer.SetColor(name, value);
        }

        public static Color? GetColor(string name)
        {
            return SettingsContainer.GetColor(name);
        }

        public static Color GetColor(string name, Color defaultValue)
        {
            return SettingsContainer.GetColor(name, defaultValue);
        }

        public static void SetEnum<T>(string name, T value)
        {
            SettingsContainer.SetEnum(name, value);
        }

        public static T GetEnum<T>(string name, T defaultValue) where T : struct
        {
            return SettingsContainer.GetEnum(name, defaultValue);
        }

        public static void SetNullableEnum<T>(string name, T? value) where T : struct
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
            Action<Encoding> addEncoding = delegate (Encoding e) { AvailableEncodings[e.HeaderName] = e; };
            Action<string> addEncodingByName = delegate (string s) { try { addEncoding(Encoding.GetEncoding(s)); } catch { } };

            string availableEncodings = GetString("AvailableEncodings", "");
            if (string.IsNullOrWhiteSpace(availableEncodings))
            {
                // Default encoding set
                addEncoding(Encoding.Default);
                addEncoding(new ASCIIEncoding());
                addEncoding(new UnicodeEncoding());
                addEncoding(new UTF7Encoding());
                addEncoding(new UTF8Encoding(false));
                try
                {
                    addEncoding(Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage));
                }
                catch
                {
                    //there are CultureInfos without a code page
                }
            }
            else
            {
                var utf8 = new UTF8Encoding(false);
                foreach(var encodingName in availableEncodings.Split(';'))
                {
                    // create utf-8 without BOM
                    if (encodingName == utf8.HeaderName)
                        addEncoding(utf8);
                    // default encoding
                    else if (encodingName == "Default")
                        addEncoding(Encoding.Default);
                    // add encoding by name
                    else
                        addEncodingByName(encodingName);
                }
            }
        }

        private static void SaveEncodings()
        {
            string availableEncodings = AvailableEncodings.Values.Select(e => e.HeaderName).Join(";");
            availableEncodings = availableEncodings.Replace(Encoding.Default.HeaderName, "Default");
            SetString("AvailableEncodings", availableEncodings);
        }

    }

    public class AppSettingsPath : SettingsPath
    {
        public AppSettingsPath(string aPathName) : base(null, aPathName)
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
