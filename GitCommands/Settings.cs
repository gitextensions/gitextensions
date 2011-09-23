using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GitCommands.Logging;
using GitCommands.Repository;
using Microsoft.Win32;

namespace GitCommands
{
    public static class Settings
    {
        //Constants
        public static readonly string GitExtensionsVersionString = "2.25";
        public static readonly int GitExtensionsVersionInt = 225;

        //semi-constants
        public static readonly char PathSeparator = '\\';
        public static readonly char PathSeparatorWrong = '/';




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
            get { return SafeGet("showgitstatusinbrowsetoolbar", false, ref _showGitStatusInBrowseToolbar); }
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

        private static Encoding _encoding;
        public static Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    string encoding = GetValue("encoding", "");

                    if (string.IsNullOrEmpty(encoding))
                        _encoding = new UTF8Encoding(false);
                    else if (encoding.Equals("Default", StringComparison.CurrentCultureIgnoreCase))
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
                }
                return _encoding;
            }
            set
            {
                _encoding = value;

                if (VersionIndependentRegKey == null)
                    return;

                string encoding = "";
                if (_encoding.EncodingName == Encoding.ASCII.EncodingName)
                    encoding = "ASCII";
                else if (_encoding.EncodingName == Encoding.Unicode.EncodingName)
                    encoding = "Unicode";
                else if (_encoding.EncodingName == Encoding.UTF7.EncodingName)
                    encoding = "UTF7";
                else if (_encoding.EncodingName == Encoding.UTF8.EncodingName)
                    encoding = "UTF8";
                else if (_encoding.EncodingName == Encoding.UTF32.EncodingName)
                    encoding = "UTF32";
                else if (_encoding.EncodingName == Encoding.Default.EncodingName)
                    encoding = "Default";

                SetValue("encoding", encoding);
            }
        }

        private static string _pullMerge;
        public static string PullMerge
        {
            get { return SafeGet("pullmerge", "merge", ref _pullMerge); }
            set { SafeSet("pullmerge", value, ref _pullMerge); }
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

        public delegate void WorkingDirChangedEventHandler(string oldDir, string newDir);
        public static event WorkingDirChangedEventHandler WorkingDirChanged;

        private static string _workingdir;
        public static string WorkingDir
        {
            get
            {
                return _workingdir;
            }
            set
            {
                string old = _workingdir;
                _workingdir = GitCommandHelpers.FindGitWorkingDir(value.Trim());
                if (WorkingDirChanged != null)
                {
                    WorkingDirChanged(old, _workingdir);
                }
            }
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
            get { return SafeGet("showcurrentbranchinvisualstudio", false, ref _showCurrentBranchInVisualStudio); }
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

        public static bool ValidWorkingDir()
        {
            return ValidWorkingDir(WorkingDir);
        }

        public static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            if (Directory.Exists(dir + PathSeparator + ".git"))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + PathSeparator + "info") &&
                   Directory.Exists(dir + PathSeparator + "objects") &&
                   Directory.Exists(dir + PathSeparator + "refs");
        }

        public static bool IsBareRepository()
        {
            return !Directory.Exists(WorkingDir + PathSeparator + ".git");
        }

        public static string WorkingDirGitDir()
        {
            return GitCommandHelpers.GetGitDirectory(WorkingDir);
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
                if (Repositories.RepositoryHistoryLoaded)
                    SetValue("history", Repositories.SerializeHistoryIntoXml());
                if (Repositories.RepositoryCategoriesLoaded)
                    SetValue("repositories", Repositories.SerializeRepositories());
            }
            catch
            { }
        }

        public static void LoadSettings()
        {
            try
            {
                GitCommandHelpers.SetSsh(GetValue<string>("gitssh", null));
            }
            catch
            { }
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

        private static string VersionIndependentRegKey
        {
            get
            {
                return string.Concat(Registry.CurrentUser, "\\Software\\GitExtensions\\GitExtensions");
                //return Application.UserAppDataRegistry.Name.Replace("\\" + Application.ProductVersion, string.Empty);
            }
        }

        public static T GetValue<T>(string name, T defaultValue)
        {
            T value = (T)Registry.GetValue(VersionIndependentRegKey, name, null);

            if (value != null)
                return value;

            /////////////////////////////////////////////////////////////////////////////////////
            ///// BEGIN TEMPORARY CODE TO CONVERT OLD VERSION DEPENDENT REGISTRY TO NEW 
            ///// VERSION INDEPENDENT REGISTRY KEY!
            /////////////////////////////////////////////////////////////////////////////////////
            value = (T)Registry.GetValue(VersionIndependentRegKey + "\\1.0.0.0", name, null);

            if (value != null)
            {
                SetValue<T>(name, value);
                return value;
            }

            if (defaultValue != null)
            {
                SetValue<T>(name, defaultValue);
            }
            /////////////////////////////////////////////////////////////////////////////////////
            ///// END TEMPORARY CODE TO CONVERT OLD VERSION DEPENDENT REGISTRY TO NEW 
            ///// VERSION INDEPENDENT REGISTRY KEY!
            /////////////////////////////////////////////////////////////////////////////////////

            return defaultValue;
        }

        public static void SetValue<T>(string name, T value)
        {
            Registry.SetValue(VersionIndependentRegKey, name, value);
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