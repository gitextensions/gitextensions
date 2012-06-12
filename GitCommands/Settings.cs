using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GitCommands.Config;
using GitCommands.Logging;
using GitCommands.Repository;
using Microsoft.Win32;

namespace GitCommands
{
    public static class Settings
    {
        //Constants
        public const string GitExtensionsVersionString = "2.33";
        public const int GitExtensionsVersionInt = 233;

        //semi-constants
        public static readonly char PathSeparator = '\\';
        public static readonly char PathSeparatorWrong = '/';

        private static readonly Dictionary<String, object> byNameMap = new Dictionary<String, object>();
        static Settings()
        {
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

        private static string _translation;
        public static string Translation
        {
            get { return SafeGet("translation", "", ref _translation); }
            set { SafeSet("translation", value, ref _translation); }
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

        private static bool? _revisionGraphShowWorkingDirChanges;
        public static bool RevisionGraphShowWorkingDirChanges
        {
            get { return SafeGet("revisiongraphshowworkingdirchanges", false, ref _revisionGraphShowWorkingDirChanges); }
            set { SafeSet("revisiongraphshowworkingdirchanges", value, ref _revisionGraphShowWorkingDirChanges); }
        }

        private static bool? _DirtyDirWarnBeforeCheckoutBranch;
        public static bool DirtyDirWarnBeforeCheckoutBranch
        {
            get { return SafeGet("DirtyDirWarnBeforeCheckoutBranch", false, ref _DirtyDirWarnBeforeCheckoutBranch); }
            set { SafeSet("DirtyDirWarnBeforeCheckoutBranch", value, ref _DirtyDirWarnBeforeCheckoutBranch); }
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

        public static readonly Dictionary<string, Encoding> availableEncodings = new Dictionary<string, Encoding>();
           
        private static Encoding GetEncoding(bool local, string settingName, bool fromSettings)
        {
            Encoding result;
            string lname = local ? "_local" + '_' + WorkingDir : "_global";
            lname = settingName + lname;
            object o;
            if (byNameMap.TryGetValue(lname, out o))
                result = o as Encoding;
            else
            {
                string encodingName;
                if (fromSettings)
                    encodingName = GetString("n_" + lname, null);
                else
                {
                    ConfigFile cfg;
                    if (local)
                        cfg = Module.GetLocalConfig();
                    else
                        cfg = GitCommandHelpers.GetGlobalConfig();

                    encodingName = cfg.GetValue(settingName);
                }

                if (string.IsNullOrEmpty(encodingName))
                    result = null;
                else if (!availableEncodings.TryGetValue(encodingName, out result))
                {
                    try
                    {
                        result = Encoding.GetEncoding(encodingName);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new Exception(ex.Message + Environment.NewLine + "Unsupported encoding set in git config file: " + encodingName + Environment.NewLine + "Please check the setting i18n.commitencoding in your local and/or global config files. Command aborted.", ex);
                    }
                }
                byNameMap[lname] = result;                
            }

            return result;

        }

        private static void SetEncoding(bool local, string settingName, Encoding encoding, bool toSettings)
        {
            string lname = local ? "_local" + '_' + WorkingDir : "_global";
            lname = settingName + lname;
            byNameMap[lname] = encoding;
            //storing to config file is handled by FormSettings
            if (toSettings)
                SetString("n_" + lname, encoding == null ? null : encoding.HeaderName);
        }

        //encoding for files paths
        public static Encoding SystemEncoding;
        //Encoding that let us read all bytes without replacing any char
        public static readonly Encoding LosslessEncoding = Encoding.GetEncoding("ISO-8859-1");//is any better?
        //follow by git i18n CommitEncoding and LogOutputEncoding is a hell
        //command output may consist of:
        //1) commit message encoded in CommitEncoding, recoded to LogOutputEncoding or not dependent of 
        //   pretty parameter (pretty=raw - recoded, pretty=format - not recoded)
        //2) author name encoded dependently on config file encoding, not recoded to LogOutputEncoding
        //3) file content encoded in its original encoding, not recoded to LogOutputEncoding
        //4) file path (file name is encoded in system default encoding), not recoded to LogOutputEncoding,
        //   every not ASCII character is escaped with \ followed by its code as a three digit octal number
        //5) branch or tag name encoded in system default encoding, not recoded to LogOutputEncoding
        //saying that "At the core level, git is character encoding agnostic." is not enough
        //In my opinion every data not encoded in utf8 should contain information
        //about its encoding, also git should emit structuralized data
        //i18n CommitEncoding and LogOutputEncoding properties are stored in config file, because of 2)
        //it is better to encode this file in utf8 for international projects. To read config file properly
        //we must know its encoding, let user decide by setting AppEncoding property which encoding has to be used
        //to read/write config file
        public static Encoding GetAppEncoding(bool local, bool returnDefault)
        {
            Encoding result = GetEncoding(local, "AppEncoding", true);
            if (result == null && returnDefault)
                result = new UTF8Encoding(false);
            return result;
        }
        public static void SetAppEncoding(bool local, Encoding encoding)
        {
            SetEncoding(local, "AppEncoding", encoding, true);
        }
        public static Encoding AppEncoding
        {
            get
            {
                Encoding result = GetAppEncoding(true, false);
                if (result == null)
                    result = GetAppEncoding(false, true);
                return result;
            }
        }

        public static Encoding GetFilesEncoding(bool local) 
        {
            return GetEncoding(local, "i18n.filesEncoding", false);                 
        }
        public static void SetFilesEncoding(bool local, Encoding encoding)
        {
            SetEncoding(local, "i18n.filesEncoding", encoding, false);
        }
        public static Encoding FilesEncoding
        {
            get
            {
                Encoding result = GetFilesEncoding(true);
                if (result == null)
                    result = GetFilesEncoding(false);
                if (result == null)
                    result = new UTF8Encoding(false);
                return result;
            }
        }

        public static Encoding GetCommitEncoding(bool local)
        {
            return GetEncoding(local, "i18n.commitEncoding", false);
        }
        public static void SetCommitEncoding(bool local, Encoding encoding)
        {
            SetEncoding(local, "i18n.commitEncoding", encoding, false);
        }
        public static Encoding CommitEncoding
        {
            get
            {
                Encoding result = GetCommitEncoding(true);
                if (result == null)
                    result = GetCommitEncoding(false);
                if (result == null)
                    result = new UTF8Encoding(false);
                return result;
            }
        }

        public static Encoding GetLogOutputEncoding(bool local)
        {
            return GetEncoding(local, "i18n.logoutputencoding", false);
        }
        public static void SetLogOutputEncoding(bool local, Encoding encoding)
        {
            SetEncoding(local, "i18n.logoutputencoding", encoding, false);
        }
        public static Encoding LogOutputEncoding
        {
            get
            {
                Encoding result = GetLogOutputEncoding(true);
                if (result == null)
                    result = GetLogOutputEncoding(false);
                if (result == null)
                    result = CommitEncoding;
                if (result == null)
                    result = new UTF8Encoding(false);
                return result;
            }
        }

        public enum PullAction
        {
            None,
            Merge,
            Rebase,
            Fetch,
            FetchAll
        }

        public static PullAction PullMerge
        {
            get { return GetEnum<PullAction>("pullmerge", PullAction.Merge); }
            set { SetEnum<PullAction>("pullmerge", value); }
        }

        public static bool DonSetAsLastPullAction
        {
            get { return GetBool("DonSetAsLastPullAction", true).Value; }
            set { SetBool("DonSetAsLastPullAction", value); }
        }

        public static PullAction LastPullAction
        {
            get { return GetEnum<PullAction>("LastPullAction_" + WorkingDir, PullAction.None); }
            set { SetEnum<PullAction>("LastPullAction_" + WorkingDir, value); }
        }

        public static void LastPullActionToPullMerge()
        {
            if (LastPullAction == PullAction.FetchAll)
                PullMerge = PullAction.Fetch;
            else if (LastPullAction != PullAction.None)
                PullMerge = LastPullAction;
        }


        private static string _smtp;
        public static string Smtp
        {
            get { return SafeGet("smtp", "", ref _smtp); }
            set { SafeSet("smtp", value, ref _smtp); }
        }
        
        private static bool? _autoStash;
        public static bool AutoStash
        {
            get { return SafeGet("autostash", false, ref _autoStash); }
            set { SafeSet("autostash", value, ref _autoStash); }
        }

        private static bool? _mergeAtCheckout;
        public static bool MergeAtCheckout
        {
            get { return SafeGet("mergeAtCheckout", true, ref _mergeAtCheckout); }
            set { SafeSet("mergeAtCheckout", value, ref _mergeAtCheckout); }
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
            get { return SafeGet("commitdialogsplitter", 400, ref _commitDialogSplitter); }
            set { SafeSet("commitdialogsplitter", value, ref _commitDialogSplitter); }
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

        public delegate void WorkingDirChangedEventHandler(string oldDir, string newDir, string newGitDir);
        public static event WorkingDirChangedEventHandler WorkingDirChanged;

        private static readonly GitModule _module = new GitModule();
        public static GitModule Module
        {
            [DebuggerStepThrough]
            get
            {
                return _module;
            }
        }

        public static string WorkingDir
        {
            get
            {
                return _module.WorkingDir;
            }
            set
            {
                string old = _module.WorkingDir;
                _module.WorkingDir = value;
                RecentWorkingDir = _module.WorkingDir;
                if (WorkingDirChanged != null)
                {
                    WorkingDirChanged(old, _module.WorkingDir, _module.GetGitDirectory());
                }
            }
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

        private static void TransferEncodings()
        {
            string encoding = GetValue("encoding", "");
            if (!encoding.IsNullOrEmpty())
            {
                Encoding _encoding;

                if (encoding.Equals("Default", StringComparison.CurrentCultureIgnoreCase))
                    _encoding = Encoding.Default;
                else if (encoding.Equals("Unicode", StringComparison.CurrentCultureIgnoreCase))
                    _encoding = new UnicodeEncoding();
                else if (encoding.Equals("ASCII", StringComparison.CurrentCultureIgnoreCase))
                    _encoding = new ASCIIEncoding();
                else if (encoding.Equals("UTF7", StringComparison.CurrentCultureIgnoreCase))
                    _encoding = new UTF7Encoding();
                else if (encoding.Equals("UTF32", StringComparison.CurrentCultureIgnoreCase))
                    _encoding = new UTF32Encoding(true, false);
                else
                    _encoding = new UTF8Encoding(false);

                SetFilesEncoding(false, _encoding);
                SetAppEncoding(false, _encoding);
                SetValue("encoding", null as string);
            }
        }

        private static void SetupSystemEncoding()
        {
            //check whether GitExtensions works with standard msysgit or msysgit-unicode

            // invoke a git command that returns an invalid argument in its response, and
            // check if a unicode-only character is reported back. If so assume msysgit-unicode

            // git config --get with a malformed key (no section) returns:
            // "error: key does not contain a section: <key>"
            const string controlStr = "ą"; // "a caudata"
            string arguments = string.Format("config --get {0}", controlStr);

            int exitCode;
            String s = Module.RunGitCmd(arguments, out exitCode, null, Encoding.UTF8);
            if (s != null && s.IndexOf(controlStr) != -1)
                SystemEncoding = Encoding.UTF8;
            else
                SystemEncoding = Encoding.Default;
        
        }

        public static void LoadSettings()
        {

            SetupSystemEncoding();

            Action<Encoding> addEncoding = delegate(Encoding e) { availableEncodings[e.HeaderName] = e; };
            addEncoding(Encoding.Default);
            addEncoding(new ASCIIEncoding());
            addEncoding(new UnicodeEncoding());
            addEncoding(new UTF7Encoding());
            addEncoding(new UTF8Encoding());

            try
            {
                TransferEncodings();
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

        public static bool? _dashboardShowCurrentBranch;
        public static bool DashboardShowCurrentBranch
        {
            get { return SafeGet("dashboardshowcurrentbranch", true, ref _dashboardShowCurrentBranch); }
            set { SafeSet("dashboardshowcurrentbranch", value, ref _dashboardShowCurrentBranch); }
        }

        public static string _ownScripts;
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

        private static bool? _AutoPullOnRejected;
        public static bool AutoPullOnRejected
        {
            get { return SafeGet("AutoPullOnRejected", false, ref _AutoPullOnRejected); }
            set { SafeSet("AutoPullOnRejected", value, ref _AutoPullOnRejected); }
        }

        private static bool? _RecursiveSubmodulesCheck;
        public static bool RecursiveSubmodulesCheck
        {
            get { return SafeGet("RecursiveSubmodulesCheck", true, ref _RecursiveSubmodulesCheck); }
            set { SafeSet("RecursiveSubmodulesCheck", value, ref _RecursiveSubmodulesCheck); }
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

        private static bool SafeGet(string key, bool defaultValue, ref bool? field)
        {
            return SafeGet(key, defaultValue, ref field, x => x == "True").Value;
        }

        private static int SafeGet(string key, int defaultValue, ref int? field)
        {
            return SafeGet(key, defaultValue, ref field, x =>
            {
                int result;
                return int.TryParse(x, out result) ? result : defaultValue;
            }).Value;
        }

        private static Color SafeGet(string key, Color defaultValue, ref Color? field)
        {
            return SafeGet(key, defaultValue, ref field, x => ColorTranslator.FromHtml(x)).Value;
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
                SetBool("TransferedVerDependentReg", true);
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
            if (byNameMap.TryGetValue(name, out o))
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

                byNameMap[name] = result;
                return result;
            }
        }

        public static void SetByName<T>(string name, T value, Func<T, object> encode)
        {
            object o;
            if (byNameMap.TryGetValue(name, out o))
                if (Object.Equals(o, value))
                    return;
            if (value == null)
                o = null;
            else
                o = encode(value);
            SetValue<object>(name, o);
            byNameMap[name] = value;
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
        public static string AsString(this Font value)
        {
            return String.Format("{0};{1}", value.FontFamily.Name, value.Size);
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
                return new Font(parts[0], Single.Parse(parts[1]));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}