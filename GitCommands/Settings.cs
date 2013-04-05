using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
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

        private static readonly Dictionary<String, object> ByNameMap = new Dictionary<String, object>();

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

            //Make applicationdatapath version dependent
            ApplicationDataPath = Application.UserAppDataPath.Replace(Application.ProductVersion, string.Empty);
        }

        private static int? _UserMenuLocationX;
        public static int UserMenuLocationX
        {
            get { return SafeGet("usermenulocationx", -1, ref _UserMenuLocationX); }
            set { SafeSet("usermenulocationx", value, ref _UserMenuLocationX); }
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
            get { return GetBool("AlwaysShowCheckoutBranchDlg", false).Value; }
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
            get { return GetBool("LoadFileHistoryOnShow", true).Value; }
            set { SetBool("LoadFileHistoryOnShow", value); }
        }

        public static bool LoadBlameOnShow
        {
            get { return GetBool("LoadBlameOnShow", true).Value; }
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
            get { return GetBool("DonSetAsLastPullAction", true).Value; }
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
            get { return GetBool("UseDefaultCheckoutBranchAction", false).Value; }
            set { SetBool("UseDefaultCheckoutBranchAction", value); }
        }

        public static bool DontShowHelpImages
        {
            get { return GetBool("DontShowHelpImages", false).Value; }
            set { SetBool("DontShowHelpImages", value); }
        }

        public static bool DontConfirmAmend
        {
            get { return GetBool("DontConfirmAmend", false).Value; }
            set { SetBool("DontConfirmAmend", value); }
        }

        public static bool? AutoPopStashAfterPull
        {
            get { return GetBool("AutoPopStashAfterPull", null); }
            set { SetBool("AutoPopStashAfterPull", value); }
        }

        public static bool? AutoPopStashAfterCheckoutBranch
        {
            get { return GetBool("AutoPopStashAfterCheckoutBranch", null); }
            set { SetBool("AutoPopStashAfterCheckoutBranch", value); }
        }

        public static PullAction? AutoPullOnPushRejectedAction
        {
            get { return GetNullableEnum<PullAction>("AutoPullOnPushRejectedAction", null); }
            set { SetNullableEnum<PullAction>("AutoPullOnPushRejectedAction", value); }
        }

        public static bool DontConfirmPushNewBranch
        {
            get { return GetBool("DontConfirmPushNewBranch", false).Value; }
            set { SetBool("DontConfirmPushNewBranch", value); }
        }

        public static bool DontConfirmAddTrackingRef
        {
            get { return GetBool("DontConfirmAddTrackingRef", false).Value; }
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
        /// <summary>Gets or sets the path to the git application executable.</summary>
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
            get { return GetBool("StartWithRecentWorkingDir", false).Value; }
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
        /// <summary>Gets the path to Pageant (SSH auth agent).</summary>
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

        private static Color? _otherTagColor;
        public static Color OtherTagColor
        {
            get { return SafeGet("othertagcolor", Color.Gray, ref _otherTagColor); }
            set { SafeSet("othertagcolor", value, ref _otherTagColor); }
        }
        private static Color? _tagColor;
        public static Color TagColor
        {
            get { return SafeGet("tagcolor", Color.DarkBlue, ref _tagColor); }
            set { SafeSet("tagcolor", value, ref _tagColor); }
        }

        private static Color? _graphColor;
        public static Color GraphColor
        {
            get { return SafeGet("graphcolor", Color.DarkRed, ref _graphColor); }
            set { SafeSet("graphcolor", value, ref _graphColor); }
        }

        private static Color? _branchColor;
        public static Color BranchColor
        {
            get { return SafeGet("branchcolor", Color.DarkRed, ref _branchColor); }
            set { SafeSet("branchcolor", value, ref _branchColor); }
        }

        private static Color? _remoteBranchColor;
        public static Color RemoteBranchColor
        {
            get { return SafeGet("remotebranchcolor", Color.Green, ref _remoteBranchColor); }
            set { SafeSet("remotebranchcolor", value, ref _remoteBranchColor); }
        }

        private static Color? _diffSectionColor;
        public static Color DiffSectionColor
        {
            get { return SafeGet("diffsectioncolor", Color.FromArgb(230, 230, 230), ref _diffSectionColor); }
            set { SafeSet("diffsectioncolor", value, ref _diffSectionColor); }
        }

        private static Color? _diffRemovedColor;
        public static Color DiffRemovedColor
        {
            get { return SafeGet("diffremovedcolor", Color.FromArgb(255, 200, 200), ref _diffRemovedColor); }
            set { SafeSet("diffremovedcolor", value, ref _diffRemovedColor); }
        }

        private static Color? _diffRemovedExtraColor;
        public static Color DiffRemovedExtraColor
        {
            get { return SafeGet("diffremovedextracolor", Color.FromArgb(255, 150, 150), ref _diffRemovedExtraColor); }
            set { SafeSet("diffremovedextracolor", value, ref _diffRemovedExtraColor); }
        }

        private static Color? _diffAddedColor;
        public static Color DiffAddedColor
        {
            get { return SafeGet("diffaddedcolor", Color.FromArgb(200, 255, 200), ref _diffAddedColor); }
            set { SafeSet("diffaddedcolor", value, ref _diffAddedColor); }
        }

        private static Color? _diffAddedExtraColor;
        public static Color DiffAddedExtraColor
        {
            get { return SafeGet("diffaddedextracolor", Color.FromArgb(135, 255, 135), ref _diffAddedExtraColor); }
            set { SafeSet("diffaddedextracolor", value, ref _diffAddedExtraColor); }
        }

        private static Font _diffFont;
        public static Font DiffFont
        {
            get { return SafeGet("difffont", new Font("Courier New", 10), ref _diffFont); }
            set { SafeSet("difffont", value, ref _diffFont); }
        }

        private static Font _commitFont;
        public static Font CommitFont
        {
            get { return SafeGet("commitfont", new Font(SystemFonts.MessageBoxFont.Name, SystemFonts.MessageBoxFont.Size), ref _commitFont); }
            set { SafeSet("commitfont", value, ref _commitFont); }
        }

        private static Font _font;
        public static Font Font
        {
            get { return SafeGet("font", new Font(SystemFonts.MessageBoxFont.Name, SystemFonts.MessageBoxFont.Size), ref _font); }
            set { SafeSet("font", value, ref _font); }
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
            return GetValue("InstallDir", "");
        }

        public static void SetInstallDir(string dir)
        {
            if (VersionIndependentRegKey != null)
                SetValue("InstallDir", dir);
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
                SetValue("gitssh", GitCommandHelpers.GetSsh());
                Repositories.SaveSettings();
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
                TransferVerDependentReg();
            }
            catch
            { }

            try
            {
                GitCommandHelpers.SetSsh(GetValue<string>("gitssh", null));
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

        private static DateTime? _lastUpdateCheck;
        public static DateTime LastUpdateCheck
        {
            get { return SafeGet("LastUpdateCheck", default(DateTime), ref _lastUpdateCheck); }
            set { SafeSet("LastUpdateCheck", value, ref _lastUpdateCheck); }
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

        private static T SafeGet<T>(string key, T defaultValue, ref T field, Func<string, T> converter)
        {
            if (field == null && VersionIndependentRegKey != null)
            {
                var value = GetValue<object>(key, null);
                field = value == null ? defaultValue : converter(value.ToString());
            }
            return field;
        }

        private static string SafeGet(string key, string defaultValue, ref string field)
        {
            return SafeGet(key, defaultValue, ref field, x => x);
        }

        private static Font SafeGet(string key, Font defaultValue, ref Font field)
        {
            return SafeGet(key, defaultValue, ref field, x => x.Parse(defaultValue));
        }

        private static DateTime SafeGet(string key, DateTime? defaultValue, ref DateTime? field)
        {
            return SafeGet(key, defaultValue, ref field,
                x =>
                {
                    DateTime result;
                    if (DateTime.TryParseExact(x, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                        return result;
                    return null;
                }).GetValueOrDefault();
        }

        private static bool SafeGet(string key, bool defaultValue, ref bool? field)
        {
            return SafeGet(key, defaultValue, ref field, x => x == "True").GetValueOrDefault();
        }

        private static int SafeGet(string key, int defaultValue, ref int? field)
        {
            return SafeGet(key, defaultValue, ref field, x =>
            {
                int result;
                return int.TryParse(x, out result) ? result : defaultValue;
            }).GetValueOrDefault();
        }

        private static Color SafeGet(string key, Color defaultValue, ref Color? field)
        {
            return SafeGet(key, defaultValue, ref field, x => ColorTranslator.FromHtml(x)).GetValueOrDefault();
        }

        private static void SafeSet(string key, DateTime? value, ref DateTime? field)
        {
            field = value;
            SetValue(key, field != null ? field.Value.ToString("yyyy/M/dd", CultureInfo.InvariantCulture) : null);
        }

        private static void SafeSet<T>(string key, T value, ref T field)
        {
            if (Object.Equals(field, value))
                return;
            field = value;
            SetValue(key, field);
        }

        private static void SafeSet(string key, Color value, ref Color? field)
        {
            field = value;
            SetValue(key, ColorTranslator.ToHtml(field.Value));
        }

        private static void SafeSet(string key, Font value, ref Font field)
        {
            field = value;
            SetValue(key, field.AsString());
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

        public static T GetValue<T>(string name, T defaultValue)
        {
            T value = (T)VersionIndependentRegKey.GetValue(name, null);

            if (value != null)
                return value;
          
            return defaultValue;
        }

        //temporary code to transfer version dependent registry caused that there was no way to set value to null
        //if there was the same named key in version dependent registry
        private static void TransferVerDependentReg()
        {
            bool? transfered = GetBool("TransferedVerDependentReg", false);
            if (!transfered.Value)
            {
                string r = Application.UserAppDataRegistry.Name.Replace(Application.ProductVersion, "1.0.0.0");
                r = r.Substring(Registry.CurrentUser.Name.Length + 1, r.Length - Registry.CurrentUser.Name.Length - 1);
                RegistryKey versionDependentRegKey = Registry.CurrentUser.OpenSubKey(r, true);
                SetBool("TransferedVerDependentReg", true);
                if (versionDependentRegKey == null)
                    return;

                try
                {
                    foreach (string key in versionDependentRegKey.GetValueNames())
                    {
                        object val = versionDependentRegKey.GetValue(key);
                        object independentVal = VersionIndependentRegKey.GetValue(key, null);
                        if (independentVal == null)
                            SetValue<object>(key, val);
                    }
                }
                finally
                {
                    versionDependentRegKey.Close();
                }
            }
            
        }

        public static void SetValue<T>(string name, T value)
        {
            if (value == null)
                VersionIndependentRegKey.DeleteValue(name);
            else
                VersionIndependentRegKey.SetValue(name, value);
        }

        public static T GetByName<T>(string name, T defaultValue, Func<object, T> decode)
        {
            object o;
            if (ByNameMap.TryGetValue(name, out o))
            {
                if( o == null || o is T)
                    return (T)o;
                else
                    throw new Exception("Incompatible class for settings: " + name + ". Expected: " + typeof(T).FullName + ", found: " + o.GetType().FullName);
            }
            else
            {
                o = GetValue<object>(name, null);
                T result = o == null ? defaultValue : decode(o);

                ByNameMap[name] = result;
                return result;
            }
        }

        public static void SetByName<T>(string name, T value, Func<T, object> encode)
        {
            object o;
            if (ByNameMap.TryGetValue(name, out o))
                if (Object.Equals(o, value))
                    return;
            if (value == null)
                o = null;
            else
                o = encode(value);
            SetValue<object>(name, o);
            ByNameMap[name] = value;
        }

        public static bool? GetBool(string name, bool? defaultValue)
        {
            return GetByName<bool?>(name, defaultValue, x => {
                var val = x.ToString().ToLower();
                if (val == "true") return true;
                if (val == "false") return false;
                return null;
            });
        }

        public static void SetBool(string name, bool? value)
        {
            SetByName<bool?>(name, value, (bool? b) => b.Value ? "true" : "false");
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