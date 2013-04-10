using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using GitCommands.Logging;
using GitCommands.Repository;
using Microsoft.Win32;

namespace GitCommands
{
    public enum LocalChangesAction
    {
        DontChange,
        Merge,
        Reset,
        Stash
    }

    public static class Settings
    {
        //semi-constants
        public static readonly string GitExtensionsVersionString;
        public static readonly int GitExtensionsVersionInt;
        public static readonly char PathSeparator = '\\';
        public static readonly char PathSeparatorWrong = '/';
        private static readonly string SettingsFileName = "GitExtensions.settings";
        private static readonly double SAVETIME = 2000;

        private static DateTime? LastFileRead = null;

        private static readonly Dictionary<String, object> ByNameMap = new Dictionary<String, object>();
        private static readonly XmlSerializableDictionary<string, string> EncodedNameMap = new XmlSerializableDictionary<string, string>();
        static System.Timers.Timer SaveTimer = new System.Timers.Timer(SAVETIME);
        private static bool UseTimer = true;

        static Settings()
        {
            Version version = Assembly.GetCallingAssembly().GetName().Version;
            GitExtensionsVersionString = version.Major.ToString() + '.' + version.Minor.ToString();
            GitExtensionsVersionInt = version.Major * 100 + version.Minor;
            if (version.Build > 0)
            {
                GitExtensionsVersionString += '.' + version.Build.ToString();
                GitExtensionsVersionInt = GitExtensionsVersionInt * 100 + version.Build;
            }
            if (!RunningOnWindows())
            {
                PathSeparator = '/';
                PathSeparatorWrong = '\\';
            }

            GitLog = new CommandLogger();

            if (IsPortable())
            {
                ApplicationDataPath = GetGitExtensionsDirectory();
            }
            else
            {
                //Make applicationdatapath version independent
                ApplicationDataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, string.Empty);

            }
            string settingsFile = Path.Combine(ApplicationDataPath, SettingsFileName);
            if (!File.Exists(settingsFile))
            {
                ImportFromRegistry();
                SaveXMLDictionarySettings(EncodedNameMap, settingsFile);
            }
            SaveTimer.Enabled = false;
            SaveTimer.AutoReset = false;
            SaveTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnSaveTimer);
        }

        public static int UserMenuLocationX
        {
            get { return GetInt("usermenulocationx", -1); }
            set { SetInt("usermenulocationx", value); }
        }

        private static int? _UserMenuLocationY;
        public static int UserMenuLocationY
        {
            get { return SafeGet("usermenulocationy", -1, ref _UserMenuLocationY); }
            set { SafeSet("usermenulocationy", value, ref _UserMenuLocationY); }
        }

        private static bool? _stashKeepIndex;
        public static bool StashKeepIndex
        {
            get { return SafeGet("stashkeepindex", false, ref _stashKeepIndex); }
            set { SafeSet("stashkeepindex", value, ref _stashKeepIndex); }
        }

        private static bool? _stashConfirmDropShow;
        public static bool StashConfirmDropShow
        {
            get { return SafeGet("stashconfirmdropshow", true, ref _stashConfirmDropShow); }
            set { SafeSet("stashconfirmdropshow", value, ref _stashConfirmDropShow); }
        }

        private static bool? _applyPatchIgnoreWhitespace;
        public static bool ApplyPatchIgnoreWhitespace
        {
            get { return SafeGet("applypatchignorewhitespace", false, ref _applyPatchIgnoreWhitespace); }
            set { SafeSet("applypatchignorewhitespace", value, ref _applyPatchIgnoreWhitespace); }
        }

        private static bool? _usePatienceDiffAlgorithm;
        public static bool UsePatienceDiffAlgorithm
        {
            get { return SafeGet("usepatiencediffalgorithm", false, ref _usePatienceDiffAlgorithm); }
            set { SafeSet("usepatiencediffalgorithm", value, ref _usePatienceDiffAlgorithm); }
        }

        private static bool? _showErrorsWhenStagingFiles;
        public static bool ShowErrorsWhenStagingFiles
        {
            get { return SafeGet("showerrorswhenstagingfiles", true, ref _showErrorsWhenStagingFiles); }
            set { SafeSet("showerrorswhenstagingfiles", value, ref _showErrorsWhenStagingFiles); }
        }

        private static string _lastCommitMessage;
        public static string LastCommitMessage
        {
            get { return SafeGet("lastCommitMessage", "", ref _lastCommitMessage); }
            set { SafeSet("lastCommitMessage", value, ref _lastCommitMessage); }
        }

        private static string _truncatePathMethod;
        public static string TruncatePathMethod
        {
            get { return SafeGet("truncatepathmethod", "none", ref _truncatePathMethod); }
            set { SafeSet("truncatepathmethod", value, ref _truncatePathMethod); }
        }

        private static bool? _showGitStatusInBrowseToolbar;
        public static bool ShowGitStatusInBrowseToolbar
        {
            get { return SafeGet("showgitstatusinbrowsetoolbar", true, ref _showGitStatusInBrowseToolbar); }
            set { SafeSet("showgitstatusinbrowsetoolbar", value, ref _showGitStatusInBrowseToolbar); }
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

        private static bool? _commitInfoShowContainedInBranchesLocal;
        public static bool CommitInfoShowContainedInBranchesLocal
        {
            get { return SafeGet("commitinfoshowcontainedinbrancheslocal", true, ref _commitInfoShowContainedInBranchesLocal); }
            set { SafeSet("commitinfoshowcontainedinbrancheslocal", value, ref _commitInfoShowContainedInBranchesLocal); }
        }

        private static bool? _checkForUncommittedChangesInCheckoutBranch;
        public static bool CheckForUncommittedChangesInCheckoutBranch
        {
            get { return SafeGet("checkforuncommittedchangesincheckoutbranch", false, ref _checkForUncommittedChangesInCheckoutBranch); }
            set { SafeSet("checkforuncommittedchangesincheckoutbranch", value, ref _checkForUncommittedChangesInCheckoutBranch); }
        }

        public static bool AlwaysShowCheckoutBranchDlg
        {
            get { return GetBool("AlwaysShowCheckoutBranchDlg", false); }
            set { SetBool("AlwaysShowCheckoutBranchDlg", value); }
        }

        private static bool? _commitInfoShowContainedInBranchesRemote;
        public static bool CommitInfoShowContainedInBranchesRemote
        {
            get { return SafeGet("commitinfoshowcontainedinbranchesremote", false, ref _commitInfoShowContainedInBranchesRemote); }
            set { SafeSet("commitinfoshowcontainedinbranchesremote", value, ref _commitInfoShowContainedInBranchesRemote); }
        }

        private static bool? _commitInfoShowContainedInBranchesRemoteIfNoLocal;
        public static bool CommitInfoShowContainedInBranchesRemoteIfNoLocal
        {
            get { return SafeGet("commitinfoshowcontainedinbranchesremoteifnolocal", false, ref _commitInfoShowContainedInBranchesRemoteIfNoLocal); }
            set { SafeSet("commitinfoshowcontainedinbranchesremoteifnolocal", value, ref _commitInfoShowContainedInBranchesRemoteIfNoLocal); }
        }

        private static bool? _commitInfoShowContainedInTags;
        public static bool CommitInfoShowContainedInTags
        {
            get { return SafeGet("commitinfoshowcontainedintags", true, ref _commitInfoShowContainedInTags); }
            set { SafeSet("commitinfoshowcontainedintags", value, ref _commitInfoShowContainedInTags); }
        }

        public static string ApplicationDataPath { get; private set; }

        public static string GravatarCachePath
        {
            get { return ApplicationDataPath + "Images\\"; }
        }

        private static string _translation;
        public static string Translation
        {
            get { return SafeGet("translation", "", ref _translation); }
            set { SafeSet("translation", value, ref _translation); }
        }

        private static string _currentTranslation;
        public static string CurrentTranslation
        {
            get { return _currentTranslation ?? Translation; }
            set { _currentTranslation = value; }
        }

        private static bool? _userProfileHomeDir;
        public static bool UserProfileHomeDir
        {
            get { return SafeGet("userprofilehomedir", false, ref _userProfileHomeDir); }
            set { SafeSet("userprofilehomedir", value, ref _userProfileHomeDir); }
        }

        private static string _customHomeDir;
        public static string CustomHomeDir
        {
            get { return SafeGet("customhomedir", "", ref _customHomeDir); }
            set { SafeSet("customhomedir", value, ref _customHomeDir); }
        }

        private static bool? _enableAutoScale;
        public static bool EnableAutoScale
        {
            get { return SafeGet("enableautoscale", true, ref _enableAutoScale); }
            set { SafeSet("enableautoscale", value, ref _enableAutoScale); }
        }

        private static string _iconColor;
        public static string IconColor
        {
            get { return SafeGet("iconcolor", "default", ref _iconColor); }
            set { SafeSet("iconcolor", value, ref _iconColor); }
        }

        private static string _iconStyle;
        public static string IconStyle
        {
            get { return SafeGet("iconstyle", "default", ref _iconStyle); }
            set { SafeSet("iconstyle", value, ref _iconStyle); }
        }

        private static int? _authorImageSize;
        public static int AuthorImageSize
        {
            get { return SafeGet("authorimagesize", 80, ref _authorImageSize); }
            set { SafeSet("authorimagesize", value, ref _authorImageSize); }
        }

        private static int? _authorImageCacheDays;
        public static int AuthorImageCacheDays
        {
            get { return SafeGet("authorimagecachedays", 5, ref _authorImageCacheDays); }
            set { SafeSet("authorimagecachedays", value, ref _authorImageCacheDays); }
        }

        private static bool? _showAuthorGravatar;
        public static bool ShowAuthorGravatar
        {
            get { return SafeGet("showauthorgravatar", true, ref _showAuthorGravatar); }
            set { SafeSet("showauthorgravatar", value, ref _showAuthorGravatar); }
        }

        private static bool? _closeCommitDialogAfterCommit;
        public static bool CloseCommitDialogAfterCommit
        {
            get { return SafeGet("closecommitdialogaftercommit", true, ref _closeCommitDialogAfterCommit); }
            set { SafeSet("closecommitdialogaftercommit", value, ref _closeCommitDialogAfterCommit); }
        }

        private static bool? _closeCommitDialogAfterLastCommit;
        public static bool CloseCommitDialogAfterLastCommit
        {
            get { return SafeGet("closecommitdialogafterlastcommit", true, ref _closeCommitDialogAfterLastCommit); }
            set { SafeSet("closecommitdialogafterlastcommit", value, ref _closeCommitDialogAfterLastCommit); }
        }

        private static bool? _refreshCommitDialogOnFormFocus;
        public static bool RefreshCommitDialogOnFormFocus
        {
            get { return SafeGet("refreshcommitdialogonformfocus", false, ref _refreshCommitDialogOnFormFocus); }
            set { SafeSet("refreshcommitdialogonformfocus", value, ref _refreshCommitDialogOnFormFocus); }
        }

        private static bool? _stageInSuperprojectAfterCommit;
        public static bool StageInSuperprojectAfterCommit
        {
            get { return SafeGet("stageinsuperprojectaftercommit", true, ref _stageInSuperprojectAfterCommit); }
            set { SafeSet("stageinsuperprojectaftercommit", value, ref _stageInSuperprojectAfterCommit); }
        }

        private static bool? _PlaySpecialStartupSound;
        public static bool PlaySpecialStartupSound
        {
            get { return SafeGet("PlaySpecialStartupSound", false, ref _PlaySpecialStartupSound); }
            set { SafeSet("PlaySpecialStartupSound", value, ref _PlaySpecialStartupSound); }
        }

        private static bool? _followRenamesInFileHistory;
        public static bool FollowRenamesInFileHistory
        {
            get { return SafeGet("followrenamesinfilehistory", true, ref _followRenamesInFileHistory); }
            set { SafeSet("followrenamesinfilehistory", value, ref _followRenamesInFileHistory); }
        }

        private static bool? _fullHistoryInFileHistory;
        public static bool FullHistoryInFileHistory
        {
            get { return SafeGet("fullhistoryinfilehistory", false, ref _fullHistoryInFileHistory); }
            set { SafeSet("fullhistoryinfilehistory", value, ref _fullHistoryInFileHistory); }
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

        private static bool? _revisionGraphShowWorkingDirChanges;
        public static bool RevisionGraphShowWorkingDirChanges
        {
            get { return SafeGet("revisiongraphshowworkingdirchanges", false, ref _revisionGraphShowWorkingDirChanges); }
            set { SafeSet("revisiongraphshowworkingdirchanges", value, ref _revisionGraphShowWorkingDirChanges); }
        }

        private static bool? _revisionGraphDrawNonRelativesGray;
        public static bool RevisionGraphDrawNonRelativesGray
        {
            get { return SafeGet("revisiongraphdrawnonrelativesgray", true, ref _revisionGraphDrawNonRelativesGray); }
            set { SafeSet("revisiongraphdrawnonrelativesgray", value, ref _revisionGraphDrawNonRelativesGray); }
        }

        private static bool? _revisionGraphDrawNonRelativesTextGray;
        public static bool RevisionGraphDrawNonRelativesTextGray
        {
            get { return SafeGet("revisiongraphdrawnonrelativestextgray", false, ref _revisionGraphDrawNonRelativesTextGray); }
            set { SafeSet("revisiongraphdrawnonrelativestextgray", value, ref _revisionGraphDrawNonRelativesTextGray); }
        }

        public static readonly Dictionary<string, Encoding> AvailableEncodings = new Dictionary<string, Encoding>();
        private static readonly Dictionary<string, Encoding> EncodingSettings = new Dictionary<string, Encoding>();

        internal static bool GetEncoding(string settingName, out Encoding encoding)
        {
            lock (EncodingSettings)
            {
                return EncodingSettings.TryGetValue(settingName, out encoding);
            }
        }

        internal static void SetEncoding(string settingName, Encoding encoding)
        {
            lock (EncodingSettings)
            {
                var items = EncodingSettings.Keys.Where(item => item.StartsWith(settingName)).ToList();
                foreach (var item in items)
                    EncodingSettings.Remove(item);
                EncodingSettings[settingName] = encoding;
            }
        }

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

        public static bool DonSetAsLastPullAction
        {
            get { return GetBool("DonSetAsLastPullAction", true); }
            set { SetBool("DonSetAsLastPullAction", value); }
        }

        private static string _smtp;
        public static string SmtpServer
        {
            get { return SafeGet("SmtpServer", "smtp.gmail.com", ref _smtp); }
            set { SafeSet("SmtpServer", value, ref _smtp); }
        }

        private static int? _smtpPort;
        public static int SmtpPort
        {
            get { return SafeGet("SmtpPort", 465, ref _smtpPort); }
            set { SafeSet("SmtpPort", value, ref _smtpPort); }
        }

        private static bool? _smtpUseSSL;
        public static bool SmtpUseSsl
        {
            get { return SafeGet("SmtpUseSsl", true, ref _smtpUseSSL); }
            set { SafeSet("SmtpUseSsl", value, ref _smtpUseSSL); }
        }

        private static bool? _autoStash;
        public static bool AutoStash
        {
            get { return SafeGet("autostash", false, ref _autoStash); }
            set { SafeSet("autostash", value, ref _autoStash); }
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

        public static bool DontConfirmAmend
        {
            get { return GetBool("DontConfirmAmend", false); }
            set { SetBool("DontConfirmAmend", value); }
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
            get { return GetNullableEnum<PullAction>("AutoPullOnPushRejectedAction", null); }
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

        private static bool? _includeUntrackedFilesInAutoStash;
        public static bool IncludeUntrackedFilesInAutoStash
        {
            get { return SafeGet("includeUntrackedFilesInAutoStash", true, ref _includeUntrackedFilesInAutoStash); }
            set { SafeSet("includeUntrackedFilesInAutoStash", value, ref _includeUntrackedFilesInAutoStash); }
        }

        private static bool? _includeUntrackedFilesInManualStash;
        public static bool IncludeUntrackedFilesInManualStash
        {
            get { return SafeGet("includeUntrackedFilesInManualStash", true, ref _includeUntrackedFilesInManualStash); }
            set { SafeSet("includeUntrackedFilesInManualStash", value, ref _includeUntrackedFilesInManualStash); }
        }

        private static bool? _orderRevisionByDate;
        public static bool OrderRevisionByDate
        {
            get { return SafeGet("orderrevisionbydate", true, ref _orderRevisionByDate); }
            set { SafeSet("orderrevisionbydate", value, ref _orderRevisionByDate); }
        }

        private static string _dictionary;
        public static string Dictionary
        {
            get { return SafeGet("dictionary", "en-US", ref _dictionary); }
            set { SafeSet("dictionary", value, ref _dictionary); }
        }

        private static bool? _showGitCommandLine;
        public static bool ShowGitCommandLine
        {
            get { return SafeGet("showgitcommandline", false, ref _showGitCommandLine); }
            set { SafeSet("showgitcommandline", value, ref _showGitCommandLine); }
        }

        private static bool? _showStashCount;
        public static bool ShowStashCount
        {
            get { return SafeGet("showstashcount", false, ref _showStashCount); }
            set { SafeSet("showstashcount", value, ref _showStashCount); }
        }

        private static bool? _relativeDate;
        public static bool RelativeDate
        {
            get { return SafeGet("relativedate", true, ref _relativeDate); }
            set { SafeSet("relativedate", value, ref _relativeDate); }
        }

        private static bool? _useFastChecks;
        public static bool UseFastChecks
        {
            get { return SafeGet("usefastchecks", false, ref _useFastChecks); }
            set { SafeSet("usefastchecks", value, ref _useFastChecks); }
        }

        private static bool? _showGitNotes;
        public static bool ShowGitNotes
        {
            get { return SafeGet("showgitnotes", false, ref _showGitNotes); }
            set { SafeSet("showgitnotes", value, ref _showGitNotes); }
        }

        private static int? _revisionGraphLayout;
        public static int RevisionGraphLayout
        {
            get { return SafeGet("revisiongraphlayout", 2, ref _revisionGraphLayout); }
            set { SafeSet("revisiongraphlayout", value, ref _revisionGraphLayout); }
        }

        private static bool? _showAuthorDate;
        public static bool ShowAuthorDate
        {
            get { return SafeGet("showauthordate", true, ref _showAuthorDate); }
            set { SafeSet("showauthordate", value, ref _showAuthorDate); }
        }

        private static bool? _closeProcessDialog;
        public static bool CloseProcessDialog
        {
            get { return SafeGet("closeprocessdialog", false, ref _closeProcessDialog); }
            set { SafeSet("closeprocessdialog", value, ref _closeProcessDialog); }
        }

        private static bool? _showCurrentBranchOnly;
        public static bool ShowCurrentBranchOnly
        {
            get { return SafeGet("showcurrentbranchonly", false, ref _showCurrentBranchOnly); }
            set { SafeSet("showcurrentbranchonly", value, ref _showCurrentBranchOnly); }
        }

        private static bool? _branchFilterEnabled;
        public static bool BranchFilterEnabled
        {
            get { return SafeGet("branchfilterenabled", false, ref _branchFilterEnabled); }
            set { SafeSet("branchfilterenabled", value, ref _branchFilterEnabled); }
        }

        private static int? _commitDialogSplitter;
        public static int CommitDialogSplitter
        {
            get { return SafeGet("commitdialogsplitter", -1, ref _commitDialogSplitter); }
            set { SafeSet("commitdialogsplitter", value, ref _commitDialogSplitter); }
        }

        private static int? _commitDialogRightSplitter;
        public static int CommitDialogRightSplitter
        {
            get { return SafeGet("commitdialogrightsplitter", -1, ref _commitDialogRightSplitter); }
            set { SafeSet("commitdialogrightsplitter", value, ref _commitDialogRightSplitter); }
        }

        private static int? _revisionGridQuickSearchTimeout;
        public static int RevisionGridQuickSearchTimeout
        {
            get { return SafeGet("revisiongridquicksearchtimeout", 750, ref _revisionGridQuickSearchTimeout); }
            set { SafeSet("revisiongridquicksearchtimeout", value, ref _revisionGridQuickSearchTimeout); }
        }

        private static string _gravatarFallbackService;
        public static string GravatarFallbackService
        {
            get { return SafeGet("gravatarfallbackservice", "Identicon", ref _gravatarFallbackService); }
            set { SafeSet("gravatarfallbackservice", value, ref _gravatarFallbackService); }
        }

        private static string _gitCommand;
        public static string GitCommand
        {
            get { return SafeGet("gitcommand", "git", ref _gitCommand); }
            set { SafeSet("gitcommand", value, ref _gitCommand); }
        }

        private static string _gitBinDir;
        public static string GitBinDir
        {
            get { return SafeGet("gitbindir", "", ref _gitBinDir); }
            set
            {
                var temp = value;
                if (temp.Length > 0 && temp[temp.Length - 1] != PathSeparator)
                    temp += PathSeparator;
                SafeSet("gitbindir", temp, ref _gitBinDir);

                //if (string.IsNullOrEmpty(_gitBinDir))
                //    return;

                //var path =
                //    Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Process) + ";" +
                //    _gitBinDir;
                //Environment.SetEnvironmentVariable("path", path, EnvironmentVariableTarget.Process);
            }
        }

        private static int? _maxRevisionGraphCommits;
        public static int MaxRevisionGraphCommits
        {
            get { return SafeGet("maxrevisiongraphcommits", 100000, ref _maxRevisionGraphCommits); }
            set { SafeSet("maxrevisiongraphcommits", value, ref _maxRevisionGraphCommits); }
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

        private static string _plink;
        public static string Plink
        {
            get { return SafeGet("plink", "", ref _plink); }
            set { SafeSet("plink", value, ref _plink); }
        }
        private static string _puttygen;
        public static string Puttygen
        {
            get { return SafeGet("puttygen", "", ref _puttygen); }
            set { SafeSet("puttygen", value, ref _puttygen); }
        }

        private static string _pageant;
        public static string Pageant
        {
            get { return SafeGet("pageant", "", ref _pageant); }
            set { SafeSet("pageant", value, ref _pageant); }
        }

        private static bool? _autoStartPageant;
        public static bool AutoStartPageant
        {
            get { return SafeGet("autostartpageant", true, ref _autoStartPageant); }
            set { SafeSet("autostartpageant", value, ref _autoStartPageant); }
        }

        private static bool? _markIllFormedLinesInCommitMsg;
        public static bool MarkIllFormedLinesInCommitMsg
        {
            get { return SafeGet("markillformedlinesincommitmsg", false, ref _markIllFormedLinesInCommitMsg); }
            set { SafeSet("markillformedlinesincommitmsg", value, ref _markIllFormedLinesInCommitMsg); }
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

        public static Font DiffFont
        {
            get { return GetFont("difffont", new Font("Courier New", 10)); }
            set { SetFont("difffont", value); }
        }

        public static Font CommitFont
        {
            get { return GetFont("commitfont", new Font(SystemFonts.MessageBoxFont.Name, SystemFonts.MessageBoxFont.Size)); }
            set { SetFont("commitfont", value); }
        }

        public static Font Font
        {
            get { return GetFont("font", new Font(SystemFonts.MessageBoxFont.Name, SystemFonts.MessageBoxFont.Size)); }
            set { SetFont("font", value); }
        }

        #endregion

        private static bool? _multicolorBranches;
        public static bool MulticolorBranches
        {
            get { return SafeGet("multicolorbranches", true, ref _multicolorBranches); }
            set { SafeSet("multicolorbranches", value, ref _multicolorBranches); }
        }

        private static bool? _stripedBranchChange;
        public static bool StripedBranchChange
        {
            get { return SafeGet("stripedbranchchange", true, ref _stripedBranchChange); }
            set { SafeSet("stripedbranchchange", value, ref _stripedBranchChange); }
        }

        private static bool? _branchBorders;
        public static bool BranchBorders
        {
            get { return SafeGet("branchborders", true, ref _branchBorders); }
            set { SafeSet("branchborders", value, ref _branchBorders); }
        }

        private static bool? _showCurrentBranchInVisualStudio;
        public static bool ShowCurrentBranchInVisualStudio
        {
            //This setting MUST be set to false by default, otherwise it will not work in Visual Studio without
            //other changes in the Visual Studio plugin itself.
            get { return SafeGet("showcurrentbranchinvisualstudio", true, ref _showCurrentBranchInVisualStudio); }
            set { SafeSet("showcurrentbranchinvisualstudio", value, ref _showCurrentBranchInVisualStudio); }
        }

        private static string _lastFormatPatchDir;
        public static string LastFormatPatchDir
        {
            get { return SafeGet("lastformatpatchdir", "", ref _lastFormatPatchDir); }
            set { SafeSet("lastformatpatchdir", value, ref _lastFormatPatchDir); }
        }

        public static string GetDictionaryDir()
        {
            return GetInstallDir() + "\\Dictionaries\\";
        }

        public static string GetInstallDir()
        {
            return GetString("InstallDir", string.Empty);
        }

        public static void SetInstallDir(string dir)
        {
            if (VersionIndependentRegKey != null)
                SetString("InstallDir", dir);
        }

        public static bool RunningOnWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return true;
                default:
                    return false;
            }
        }

        public static bool RunningOnUnix()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return true;
                default:
                    return false;
            }
        }

        public static bool RunningOnMacOSX()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMonoRuntime()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public static void SaveSettings()
        {
            try
            {
                UseTimer = false;

                SetString("gitssh", GitCommandHelpers.GetSsh());
                Repositories.SaveSettings();

                UseTimer = true;

                SaveXMLDictionarySettings(EncodedNameMap, Path.Combine(ApplicationDataPath, SettingsFileName));
            }
            catch
            { }
        }

        public static void LoadSettings()
        {
            Action<Encoding> addEncoding = delegate(Encoding e) { AvailableEncodings[e.HeaderName] = e; };
            addEncoding(Encoding.Default);
            addEncoding(new ASCIIEncoding());
            addEncoding(new UnicodeEncoding());
            addEncoding(new UTF7Encoding());
            addEncoding(new UTF8Encoding());

            try
            {
                GitCommandHelpers.SetSsh(GetString("gitssh", null));
            }
            catch
            { }
        }

        private static bool? _dashboardShowCurrentBranch;
        public static bool DashboardShowCurrentBranch
        {
            get { return SafeGet("dashboardshowcurrentbranch", true, ref _dashboardShowCurrentBranch); }
            set { SafeSet("dashboardshowcurrentbranch", value, ref _dashboardShowCurrentBranch); }
        }

        private static string _ownScripts;
        public static string ownScripts
        {
            get { return SafeGet("ownScripts", "", ref _ownScripts); }
            set { SafeSet("ownScripts", value, ref _ownScripts); }
        }

        private static bool? _pushAllTags;
        public static bool PushAllTags
        {
            get { return SafeGet("pushalltags", false, ref _pushAllTags); }
            set { SafeSet("pushalltags", value, ref _pushAllTags); }
        }

        private static int? _RecursiveSubmodules;
        public static int RecursiveSubmodules
        {
            get { return SafeGet("RecursiveSubmodules", 1, ref _RecursiveSubmodules); }
            set { SafeSet("RecursiveSubmodules", value, ref _RecursiveSubmodules); }
        }

        private static string _ShorteningRecentRepoPathStrategy;
        public static string ShorteningRecentRepoPathStrategy
        {
            get { return SafeGet("ShorteningRecentRepoPathStrategy", "", ref _ShorteningRecentRepoPathStrategy); }
            set { SafeSet("ShorteningRecentRepoPathStrategy", value, ref _ShorteningRecentRepoPathStrategy); }
        }

        private static int? _MaxMostRecentRepositories;
        public static int MaxMostRecentRepositories
        {
            get { return SafeGet("MaxMostRecentRepositories", 0, ref _MaxMostRecentRepositories); }
            set { SafeSet("MaxMostRecentRepositories", value, ref _MaxMostRecentRepositories); }
        }

        private static int? _RecentReposComboMinWidth;
        public static int RecentReposComboMinWidth
        {
            get { return SafeGet("RecentReposComboMinWidth", 0, ref _RecentReposComboMinWidth); }
            set { SafeSet("RecentReposComboMinWidth", value, ref _RecentReposComboMinWidth); }
        }

        private static bool? _SortMostRecentRepos;
        public static bool SortMostRecentRepos
        {
            get { return SafeGet("SortMostRecentRepos", false, ref _SortMostRecentRepos); }
            set { SafeSet("SortMostRecentRepos", value, ref _SortMostRecentRepos); }
        }

        private static bool? _SortLessRecentRepos;
        public static bool SortLessRecentRepos
        {
            get { return SafeGet("SortLessRecentRepos", false, ref _SortLessRecentRepos); }
            set { SafeSet("SortLessRecentRepos", value, ref _SortLessRecentRepos); }
        }

        private static bool? _NoFastForwardMerge;
        public static bool NoFastForwardMerge
        {
            get { return SafeGet("NoFastForwardMerge", false, ref _NoFastForwardMerge); }
            set { SafeSet("NoFastForwardMerge", value, ref _NoFastForwardMerge); }
        }

        private static int? _CommitValidationMaxCntCharsFirstLine;
        public static int CommitValidationMaxCntCharsFirstLine
        {
            get { return SafeGet("CommitValidationMaxCntCharsFirstLine", 0, ref _CommitValidationMaxCntCharsFirstLine); }
            set { SafeSet("CommitValidationMaxCntCharsFirstLine", value, ref _CommitValidationMaxCntCharsFirstLine); }
        }

        private static int? _CommitValidationMaxCntCharsPerLine;
        public static int CommitValidationMaxCntCharsPerLine
        {
            get { return SafeGet("CommitValidationMaxCntCharsPerLine", 0, ref _CommitValidationMaxCntCharsPerLine); }
            set { SafeSet("CommitValidationMaxCntCharsPerLine", value, ref _CommitValidationMaxCntCharsPerLine); }
        }

        private static bool? _CommitValidationSecondLineMustBeEmpty;
        public static bool CommitValidationSecondLineMustBeEmpty
        {
            get { return SafeGet("CommitValidationSecondLineMustBeEmpty", false, ref _CommitValidationSecondLineMustBeEmpty); }
            set { SafeSet("CommitValidationSecondLineMustBeEmpty", value, ref _CommitValidationSecondLineMustBeEmpty); }
        }

        private static bool? _CommitValidationIndentAfterFirstLine;
        public static bool CommitValidationIndentAfterFirstLine
        {
            get { return SafeGet("CommitValidationIndentAfterFirstLine", true, ref _CommitValidationIndentAfterFirstLine); }
            set { SafeSet("CommitValidationIndentAfterFirstLine", value, ref _CommitValidationIndentAfterFirstLine); }
        }

        private static bool? _CommitValidationAutoWrap;
        public static bool CommitValidationAutoWrap
        {
            get { return SafeGet("CommitValidationAutoWrap", true, ref _CommitValidationAutoWrap); }
            set { SafeSet("CommitValidationAutoWrap", value, ref _CommitValidationAutoWrap); }
        }

        private static string _CommitValidationRegEx;
        public static string CommitValidationRegEx
        {
            get { return SafeGet("CommitValidationRegEx", String.Empty, ref _CommitValidationRegEx); }
            set { SafeSet("CommitValidationRegEx", value, ref _CommitValidationRegEx); }
        }

        private static string _CommitTemplates;
        public static string CommitTemplates
        {
            get { return SafeGet("CommitTemplates", String.Empty, ref _CommitTemplates); }
            set { SafeSet("CommitTemplates", value, ref _CommitTemplates); }
        }

        private static bool? _CreateLocalBranchForRemote;
        public static bool CreateLocalBranchForRemote
        {
            get { return SafeGet("CreateLocalBranchForRemote", false, ref _CreateLocalBranchForRemote); }
            set { SafeSet("CreateLocalBranchForRemote", value, ref _CreateLocalBranchForRemote); }
        }

        private static string _CascadeShellMenuItems;
        public static string CascadeShellMenuItems
        {
            get { return SafeGet("CascadeShellMenuItems", "110111000111111111", ref _CascadeShellMenuItems); }
            set { SafeSet("CascadeShellMenuItems", value, ref _CascadeShellMenuItems); }
        }

        private static bool? _UseFormCommitMessage;
        public static bool UseFormCommitMessage
        {
            get { return SafeGet("UseFormCommitMessage", true, ref _UseFormCommitMessage); }
            set { SafeSet("UseFormCommitMessage", value, ref _UseFormCommitMessage); }
        }

        private static DateTime _lastUpdateCheck;
        public static DateTime LastUpdateCheck
        {
            get { return SafeGet("LastUpdateCheck", default(DateTime), ref _lastUpdateCheck); }
            set { SetDate("LastUpdateCheck", value); }
        }

        public static string GetGitExtensionsFullPath()
        {
            return GetGitExtensionsDirectory() + "\\GitExtensions.exe";
        }

        public static string GetGitExtensionsDirectory()
        {
            string fileName = Assembly.GetAssembly(typeof(Settings)).Location;
            fileName = fileName.Substring(0, fileName.LastIndexOfAny(new[] { '\\', '/' }));
            return fileName;
        }

        private static T SafeGet<T>(string key, T defaultValue, ref T field, Func<string, T> decode)
        {
            field = GetByName(key, defaultValue, decode);
            return field;
        }

        private static string SafeGet(string key, string defaultValue, ref string field)
        {
            return GetString(key, defaultValue);
        }

        private static DateTime SafeGet(string key, DateTime defaultValue, ref DateTime field)
        {
            return GetDate(key, defaultValue);
        }

        private static bool SafeGet(string key, bool defaultValue, ref bool? field)
        {
            return SafeGet(key, defaultValue, ref field, x => x == "True").GetValueOrDefault();
        }

        private static int SafeGet(string key, int defaultValue, ref int? field)
        {
            return GetInt(key, defaultValue);
        }

        private static void SafeSet(string key, DateTime? value, ref DateTime? field)
        {
            field = value;
            SetDate(key, value);
        }

        private static void SafeSet(string key, int? value, ref int? field)
        {
            field = value;
            SetInt(key, value);
        }

        private static void SafeSet(string key, string value, ref string field)
        {
            field = value;
            SetString(key, value);
        }

        private static void SafeSet(string key, bool? value, ref bool? field)
        {
            field = value;
            SetBool(key, value);
        }

        private static void SafeSet<T>(string key, T value, ref T field, Func<T, string> encode)
        {
            field = value;
            SetByName(key, value, encode);
        }

        private static RegistryKey _VersionIndependentRegKey;

        private static RegistryKey VersionIndependentRegKey
        {
            get
            {
                if (_VersionIndependentRegKey == null)
                    _VersionIndependentRegKey = Registry.CurrentUser.CreateSubKey("Software\\GitExtensions\\GitExtensions", RegistryKeyPermissionCheck.ReadWriteSubTree);
                return _VersionIndependentRegKey;
            }
        }
        private static void ReadXMLDicSettings<T>(XmlSerializableDictionary<string, T> Dic, string FilePath)
        {

            if (File.Exists(FilePath))
            {

                try
                {
                    lock (Dic)
                    {

                        XmlReaderSettings rSettings = new XmlReaderSettings
                        {
                            IgnoreWhitespace = true
                        };
                        using (System.Xml.XmlReader xr = XmlReader.Create(FilePath, rSettings))
                        {

                            Dic.ReadXml(xr);
                            LastFileRead = DateTime.UtcNow;
                        }
                    }
                }
                catch (IOException)
                {
                    throw;
                }

            }

        }
        public static bool IsPortable()
        {
            return Properties.Settings.Default.IsPortable;

        }

        public static void ImportFromRegistry()
        {
            lock (EncodedNameMap)
            {
                foreach (String name in VersionIndependentRegKey.GetValueNames())
                {
                    object value = VersionIndependentRegKey.GetValue(name, null);
                    if (value != null)
                        EncodedNameMap[name] = value.ToString();
                }
            }
        }

        private static DateTime GetLastFileModificationUTC(String FilePath)
        {
            try
            {
                if (File.Exists(FilePath))
                    return File.GetLastWriteTimeUtc(FilePath);
                else
                    return DateTime.MaxValue;
            }
            catch (Exception)
            {
                return DateTime.MaxValue;
            }

        }

        private static void SaveXMLDictionarySettings<T>(XmlSerializableDictionary<string, T> Dic, String FilePath)
        {
            try
            {
                using (System.Xml.XmlTextWriter xtw = new System.Xml.XmlTextWriter(FilePath, Encoding.UTF8))
                {
                    lock (Dic)
                    {
                        xtw.Formatting = Formatting.Indented;
                        xtw.WriteStartDocument();
                        xtw.WriteStartElement("dictionary");

                        Dic.WriteXml(xtw);
                        xtw.WriteEndElement();
                    }
                }
                LastFileRead = GetLastFileModificationUTC(FilePath);
            }
            catch (IOException)
            {
                throw;
            }

        }
        //Used to eliminate multiple settings file open and close to save multiple values.  Settings will be saved SAVETIME milliseconds after the last setvalue is called
        private static void OnSaveTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer t = (System.Timers.Timer)source;
            t.Stop();
            SaveXMLDictionarySettings(EncodedNameMap, Path.Combine(ApplicationDataPath, SettingsFileName));
        }

        static void StartSaveTimer()
        {
            //Resets timer so that the last call will let the timer event run and will cause the settings to be saved.
            SaveTimer.Stop();
            SaveTimer.AutoReset = true;
            SaveTimer.Interval = SAVETIME;
            SaveTimer.AutoReset = false;

            SaveTimer.Start();
        }

        private static void SetValue(string name, string value)
        {
            lock (EncodedNameMap)
            {
                if (value == null)
                    EncodedNameMap.Remove(name);
                else
                    EncodedNameMap[name] = value;
            }

            if (UseTimer)
                StartSaveTimer();
        }

        public static string GetFreshValue(string name, String FilePath)
        {
            lock (EncodedNameMap)
            {
                DateTime lastMod = GetLastFileModificationUTC(FilePath);
                if (!LastFileRead.HasValue || lastMod > LastFileRead.Value)
                {
                    ReadXMLDicSettings(EncodedNameMap, FilePath);
                }

                string o = null;
                EncodedNameMap.TryGetValue(name, out o);
                return o;
            }
        }

        private static string GetValue(string name)
        {
            string fPath = Path.Combine(ApplicationDataPath, SettingsFileName);
            return GetFreshValue(name, fPath);
        }

        public static T GetByName<T>(string name, T defaultValue, Func<string, T> decode)
        {
            object o;

            if (ByNameMap.TryGetValue(name, out o))
            {
                if (o == null || o is T)
                {
                    return (T)o;
                }
                else
                {
                    throw new Exception("Incompatible class for settings: " + name + ". Expected: " + typeof(T).FullName + ", found: " + o.GetType().FullName);
                }
            }
            else
            {
                if (decode == null)
                    throw new ArgumentNullException("decode", string.Format("The decode parameter for setting {0} is null.", name));

                string s = GetValue(name);
                T result = s == null ? defaultValue : decode(s);
                ByNameMap.Add(name, result);
                return result;
            }
        }

        public static void SetByName<T>(string name, T value, Func<T, string> encode)
        {
            object o;
            if (ByNameMap.TryGetValue(name, out o))
                if (Object.Equals(o, value))
                    return;

            string s;
            if (value == null)
                s = null;
            else
                s = encode(value);

            SetValue(name, s);
            ByNameMap[name] = value;
        }

        public static bool? GetBool(string name)
        {
            return GetByName<bool?>(name, null, x =>
            {
                var val = x.ToString().ToLower();
                if (val == "true") return true;
                if (val == "false") return false;
                return null;
            });
        }

        public static bool GetBool(string name, bool defaultValue)
        {
            return GetBool(name) ?? defaultValue;
        }

        public static void SetBool(string name, bool? value)
        {
            SetByName<bool?>(name, value, (bool? b) => b.Value ? "true" : "false");
        }

        public static void SetInt(string name, int? value)
        {
            SetByName<int?>(name, value, (int? b) => b.HasValue ? b.ToString() : null);
        }

        public static int? GetInt(string name)
        {
            return GetByName<int?>(name, null, x =>
            {
                int result;
                if (int.TryParse(x, out result))
                {
                    return result;
                }

                return null;
            });
        }

        public static DateTime GetDate(string name, DateTime defaultValue)
        {
            return GetDate(name) ?? defaultValue;
        }

        public static void SetDate(string name, DateTime? value)
        {
            SetByName<DateTime?>(name, value, (DateTime? b) => b.HasValue ? b.Value.ToString("yyyy/M/dd", CultureInfo.InvariantCulture) : null);
        }

        public static DateTime? GetDate(string name)
        {
            return GetByName<DateTime?>(name, null, x =>
            {
                DateTime result;
                if (DateTime.TryParseExact(x, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return result;

                return null;
            });
        }

        public static int GetInt(string name, int defaultValue)
        {
            return GetInt(name) ?? defaultValue;
        }

        public static void SetFont(string name, Font value)
        {
            SetByName<Font>(name, value, x => x.AsString());
        }

        public static Font GetFont(string name, Font defaultValue)
        {
            return GetByName<Font>(name, defaultValue, x => x.Parse(defaultValue));
        }

        public static void SetColor(string name, Color? value)
        {
            SetByName<Color?>(name, value, x => x.HasValue ? ColorTranslator.ToHtml(x.Value) : null);
        }

        public static Color? GetColor(string name)
        {
            return GetByName<Color?>(name, null, x => ColorTranslator.FromHtml(x));
        }

        public static Color GetColor(string name, Color defaultValue)
        {
            return GetColor(name) ?? defaultValue;
        }

        public static void SetEnum<T>(string name, T value)
        {
            SetByName<T>(name, value, x => x.ToString());
        }

        public static T GetEnum<T>(string name, T defaultValue)
        {
            return GetByName<T>(name, defaultValue, x =>
            {
                var val = x.ToString();
                return (T)Enum.Parse(typeof(T), val, true);
            });
        }

        public static void SetNullableEnum<T>(string name, T? value) where T : struct
        {
            SetByName<T?>(name, value, x => x.HasValue ? x.ToString() : string.Empty);
        }

        public static T? GetNullableEnum<T>(string name, T? defaultValue) where T : struct
        {
            return GetByName<T?>(name, defaultValue, x =>
            {
                var val = x.ToString();

                if (val.IsNullOrEmpty())
                    return null;

                return (T?)Enum.Parse(typeof(T), val, true);
            });
        }

        public static void SetString(string name, string value)
        {
            SetByName<string>(name, value, s => s);
        }

        public static string GetString(string name, string defaultValue)
        {
            return GetByName<string>(name, defaultValue, x => x.ToString());
        }

        public static string PrefixedName(string prefix, string name)
        {
            return prefix == null ? name : prefix + '_' + name;
        }
    }

    public static class FontParser
    {

        private static readonly string InvariantCultureId = "_IC_";
        public static string AsString(this Font value)
        {
            return String.Format(CultureInfo.InvariantCulture,
                "{0};{1};{2}", value.FontFamily.Name, value.Size, InvariantCultureId);
        }

        public static Font Parse(this string value, Font defaultValue)
        {
            if (value == null)
                return defaultValue;

            string[] parts = value.Split(';');

            if (parts.Length < 2)
                return defaultValue;

            try
            {
                string fontSize;
                if (parts.Length == 3 && InvariantCultureId.Equals(parts[2]))
                    fontSize = parts[1];
                else
                {
                    fontSize = parts[1].Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                    fontSize = fontSize.Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                }

                return new Font(parts[0], Single.Parse(fontSize, CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}