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
using GitCommands.Utils;
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

        public static Lazy<string> ApplicationDataPath;
        private static string SettingsFilePath { get { return Path.Combine(ApplicationDataPath.Value, SettingsFileName); } }

        static Settings()
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
            
            Version version = Assembly.GetCallingAssembly().GetName().Version;
            GitExtensionsVersionString = version.Major.ToString() + '.' + version.Minor.ToString();
            GitExtensionsVersionInt = version.Major * 100 + version.Minor;
            if (version.Build > 0)
            {
                GitExtensionsVersionString += '.' + version.Build.ToString();
                GitExtensionsVersionInt = GitExtensionsVersionInt * 100 + version.Build;
            }
            if (!EnvUtils.RunningOnWindows())
            {
                PathSeparator = '/';
                PathSeparatorWrong = '\\';
            }

            GitLog = new CommandLogger();
            
            if (!File.Exists(SettingsFilePath))
            {
                ImportFromRegistry();
                SaveXMLDictionarySettings(EncodedNameMap, SettingsFilePath);
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

        public static string LastCommitMessage
        {
            get { return GetString("lastCommitMessage", ""); }
            set { SetString("lastCommitMessage", value); }
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
            get { return GetBool("checkforuncommittedchangesincheckoutbranch", false); }
            set { SetBool("checkforuncommittedchangesincheckoutbranch", value); }
        }

        public static bool AlwaysShowCheckoutBranchDlg
        {
            get { return GetBool("AlwaysShowCheckoutBranchDlg", false); }
            set { SetBool("AlwaysShowCheckoutBranchDlg", value); }
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
            get { return ApplicationDataPath.Value + "Images\\"; }
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

        public static int AuthorImageSize
        {
            get { return GetInt("authorimagesize", 80); }
            set { SetInt("authorimagesize", value); }
        }

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

        public static bool RevisionGraphShowWorkingDirChanges
        {
            get { return GetBool("revisiongraphshowworkingdirchanges", false); }
            set { SetBool("revisiongraphshowworkingdirchanges", value); }
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

        public static bool IncludeUntrackedFilesInAutoStash
        {
            get { return GetBool("includeUntrackedFilesInAutoStash", true); }
            set { SetBool("includeUntrackedFilesInAutoStash", value); }
        }

        public static bool IncludeUntrackedFilesInManualStash
        {
            get { return GetBool("includeUntrackedFilesInManualStash", true); }
            set { SetBool("includeUntrackedFilesInManualStash", value); }
        }

        public static bool OrderRevisionByDate
        {
            get { return GetBool("orderrevisionbydate", true); }
            set { SetBool("orderrevisionbydate", value); }
        }

        public static string Dictionary
        {
            get { return GetString("dictionary", "en-US"); }
            set { SetString("dictionary", value); }
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

        public static bool ShowTags
        {
            get { return GetBool("showtags", true); }
            set { SetBool("showtags", value); }
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

        public static bool BranchFilterEnabled
        {
            get { return GetBool("branchfilterenabled", false); }
            set { SetBool("branchfilterenabled", value); }
        }

        public static int CommitDialogSplitter
        {
            get { return GetInt("commitdialogsplitter", -1); }
            set { SetInt("commitdialogsplitter", value); }
        }

        public static int CommitDialogRightSplitter
        {
            get { return GetInt("commitdialogrightsplitter", -1); }
            set { SetInt("commitdialogrightsplitter", value); }
        }

        public static int RevisionGridQuickSearchTimeout
        {
            get { return GetInt("revisiongridquicksearchtimeout", 750); }
            set { SetInt("revisiongridquicksearchtimeout", value); }
        }

        public static string GravatarFallbackService
        {
            get { return GetString("gravatarfallbackservice", "Identicon"); }
            set { SetString("gravatarfallbackservice", value); }
        }

        /// <summary>Gets or sets the path to the git application executable.</summary>
        public static string GitCommand
        {
            get { return GetString("gitcommand", "git"); }
            set { SetString("gitcommand", value); }
        }

        public static string GitBinDir
        {
            get { return GetString("gitbindir", ""); }
            set
            {
                var temp = value;
                if (temp.Length > 0 && temp[temp.Length - 1] != PathSeparator)
                    temp += PathSeparator;
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
            get { return GetString("plink", ""); }
            set { SetString("plink", value); }
        }
        public static string Puttygen
        {
            get { return GetString("puttygen", ""); }
            set { SetString("puttygen", value); }
        }

        /// <summary>Gets the path to Pageant (SSH auth agent).</summary>
        public static string Pageant
        {
            get { return GetString("pageant", ""); }
            set { SetString("pageant", value); }
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

        public static bool ShowCurrentBranchInVisualStudio
        {
            //This setting MUST be set to false by default, otherwise it will not work in Visual Studio without
            //other changes in the Visual Studio plugin itself.
            get { return GetBool("showcurrentbranchinvisualstudio", true); }
            set { SetBool("showcurrentbranchinvisualstudio", value); }
        }

        public static string LastFormatPatchDir
        {
            get { return GetString("lastformatpatchdir", ""); }
            set { SetString("lastformatpatchdir", value); }
        }

        public static string GetDictionaryDir()
        {
            return Path.Combine(GetInstallDir(), "Dictionaries");
        }

        public static string GetInstallDir()
        {
#if DEBUG
#if INSTALL_DIR_FROM_REG
            VersionIndependentRegKey.GetValue("InstallDir", string.Empty);
#else
            string gitExtDir = GetGitExtensionsDirectory().TrimEnd('\\').TrimEnd('/');
            string debugPath = @"GitExtensions\bin\Debug";
            int len = debugPath.Length;
            var path = gitExtDir.Substring(gitExtDir.Length - len);
            if (debugPath.Replace('\\', '/').Equals(path.Replace('\\', '/')))
            {
                string projectPath = gitExtDir.Substring(0, len + 2);
                return Path.Combine(projectPath, "Bin");
            }
#endif
#endif

            return GetGitExtensionsDirectory();            
        }

        //for repair only
        public static void SetInstallDir(string dir)
        {
            VersionIndependentRegKey.SetValue("InstallDir", dir);
        }

        public static void SaveSettings()
        {
            try
            {
                UseTimer = false;

                SetString("gitssh", GitCommandHelpers.GetSsh());
                Repositories.SaveSettings();

                UseTimer = true;

                SaveXMLDictionarySettings(EncodedNameMap, SettingsFilePath);
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

        public static bool PushAllTags
        {
            get { return GetBool("pushalltags", false); }
            set { SetBool("pushalltags", value); }
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

        public static int RecentReposComboMinWidth
        {
            get { return GetInt("RecentReposComboMinWidth", 0); }
            set { SetInt("RecentReposComboMinWidth", value); }
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

        public static bool NoFastForwardMerge
        {
            get { return GetBool("NoFastForwardMerge", false); }
            set { SetBool("NoFastForwardMerge", value); }
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

        public static string CascadeShellMenuItems
        {
            get { return GetString("CascadeShellMenuItems", "110111000111111111"); }
            set { SetString("CascadeShellMenuItems", value); }
        }

        public static bool UseFormCommitMessage
        {
            get { return GetBool("UseFormCommitMessage", true); }
            set { SetBool("UseFormCommitMessage", value); }
        }

        public static DateTime LastUpdateCheck
        {
            get { return GetDate("LastUpdateCheck", default(DateTime)); }
            set { SetDate("LastUpdateCheck", value); }
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
                catch (IOException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
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
                var tmpFile = FilePath + ".tmp";
                lock (Dic)
                {

                    using (System.Xml.XmlTextWriter xtw = new System.Xml.XmlTextWriter(tmpFile, Encoding.UTF8))
                    {
                        xtw.Formatting = Formatting.Indented;
                        xtw.WriteStartDocument();
                        xtw.WriteStartElement("dictionary");

                        Dic.WriteXml(xtw);
                        xtw.WriteEndElement();
                    }
                    if (File.Exists(FilePath))
                    {
                        File.Replace(tmpFile, FilePath, FilePath + ".backup", true);
                    }
                    else
                    {
                        File.Move(tmpFile, FilePath);
                    }
                    LastFileRead = GetLastFileModificationUTC(FilePath);
                }
            }
            catch (IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw;
            }

        }
        //Used to eliminate multiple settings file open and close to save multiple values.  Settings will be saved SAVETIME milliseconds after the last setvalue is called
        private static void OnSaveTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer t = (System.Timers.Timer)source;
            t.Stop();
            SaveXMLDictionarySettings(EncodedNameMap, SettingsFilePath);
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

        private static bool NeedRefresh()
        {
            DateTime lastMod = GetLastFileModificationUTC(SettingsFilePath);
            return !LastFileRead.HasValue || lastMod > LastFileRead.Value;
        }

        private static void EnsureSettingsAreUpToDate()
        {
            if (NeedRefresh())
            {
                lock (EncodedNameMap)
                {
                    ByNameMap.Clear();
                    EncodedNameMap.Clear();
                    ReadXMLDicSettings(EncodedNameMap, SettingsFilePath);
                }
            }
        }

        private static void SetValue(string name, string value)
        {
            lock (EncodedNameMap)
            {
                //will refresh EncodedNameMap if needed
                string inMemValue = GetValue(name);
                
                if (string.Equals(inMemValue, value))
                    return;

                if (value == null)
                    EncodedNameMap.Remove(name);
                else
                    EncodedNameMap[name] = value;
            }

            if (UseTimer)
                StartSaveTimer();
        }

        private static string GetValue(string name)
        {
            lock (EncodedNameMap)
            {
                EnsureSettingsAreUpToDate();
                string o = null;
                EncodedNameMap.TryGetValue(name, out o);
                return o;
            }
        }

        public static T GetByName<T>(string name, T defaultValue, Func<string, T> decode)
        {
            object o;
            lock (EncodedNameMap)
            {
                EnsureSettingsAreUpToDate();

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
                    ByNameMap[name] = result;
                    return result;
                }
            }
        }

        public static void SetByName<T>(string name, T value, Func<T, string> encode)
        {
            string s;
            if (value == null)
                s = null;
            else
                s = encode(value);

            lock (EncodedNameMap)
            {
                SetValue(name, s);
                ByNameMap[name] = value;
            }
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
            return GetByName<string>(name, defaultValue, x => x);
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
