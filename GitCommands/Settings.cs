using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands.Logging;

namespace GitCommands
{
    public static class Settings
    {
        public static string GitExtensionsVersionString = "1.98";
        public static int GitExtensionsVersionInt = 198;
        private static string _gitBinDir = "";
        private static string _workingdir;

        static Settings()
        {
            BranchBorders = true;
            StripedBranchChange = true;
            MulticolorBranches = true;
            DiffAddedExtraColor = Color.FromArgb(135, 255, 135);
            DiffAddedColor = Color.FromArgb(200, 255, 200);
            DiffRemovedExtraColor = Color.FromArgb(255, 150, 150);
            DiffRemovedColor = Color.FromArgb(255, 200, 200);
            DiffSectionColor = Color.FromArgb(230, 230, 230);
            RemoteBranchColor = Color.Green;
            BranchColor = Color.DarkRed;
            GraphColor = Color.Red;
            TagColor = Color.DarkBlue;
            OtherTagColor = Color.Gray;
            AutoStartPageant = true;
            Pageant = "";
            Puttygen = "";
            Plink = "";
            GitLog = new CommandLogger();
            MaxCommits = 2000;
            GitCommand = "git.cmd";
            ShowRevisionGraph = true;
            UseFastChecks = true;
            RelativeDate = true;
            Dictionary = "en-US";
            OrderRevisionByDate = true;
            Smtp = "";
            PullMerge = "merge";
            Encoding = Encoding.UTF8;
            RevisionGraphThickness = 1F;
            RevisionGraphWidth = 6;
            FollowRenamesInFileHistory = true;
            ShowAuthorGravatar = true;
            AuthorImageCacheDays = 5;
            AuthorImageSize = 80;
            IconColor = "default";
            CustomHomeDir = "";
            Translation = "";
        }

        public static string Translation { get; set; }

        public static bool UserProfileHomeDir { get; set; }

        public static string CustomHomeDir { get; set; }

        public static string IconColor { get; set; }

        public static int AuthorImageSize { get; set; }

        public static int AuthorImageCacheDays { get; set; }

        public static bool ShowAuthorGravatar { get; set; }

        public static bool CloseCommitDialogAfterCommit { get; set; }

        public static bool FollowRenamesInFileHistory { get; set; }

        public static int RevisionGraphWidth { get; set; }

        public static float RevisionGraphThickness { get; set; }

        public static Encoding Encoding { get; set; }

        public static string PullMerge { get; set; }

        public static string Smtp { get; set; }

        public static bool AutoStash { get; set; }

        public static bool OrderRevisionByDate { get; set; }

        public static string Dictionary { get; set; }

        public static bool ShowGitCommandLine { get; set; }

        public static bool RelativeDate { get; set; }

        public static bool UseFastChecks { get; set; }

        public static bool ShowRevisionGraph { get; set; }

        public static bool ShowAuthorDate { get; set; }

        public static bool CloseProcessDialog { get; set; }

        public static bool ShowCurrentBranchOnly { get; set; }

        public static bool BranchFilterEnabled { get; set; }

        public static string GitCommand { get; set; }

        public static string GitBinDir
        {
            get { return _gitBinDir; }
            set
            {
                _gitBinDir = value;
                if (_gitBinDir.Length > 0 && _gitBinDir[_gitBinDir.Length - 1] != '\\')
                    _gitBinDir += "\\";

                if (string.IsNullOrEmpty(_gitBinDir))
                    return;

                var path =
                    Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Process) + ";" +
                    _gitBinDir;
                Environment.SetEnvironmentVariable("path", path, EnvironmentVariableTarget.Process);
            }
        }

        public static int MaxCommits { get; set; }

        public static string WorkingDir
        {
            get { return _workingdir; }
            set { _workingdir = GitCommands.FindGitWorkingDir(value.Trim()); }
        }

        public static CommandLogger GitLog { get; private set; }

        public static string Plink { get; set; }

        public static string Puttygen { get; set; }

        public static string Pageant { get; set; }

        public static bool AutoStartPageant { get; set; }

        public static bool MarkIllFormedLinesInCommitMsg { get; set; }

        #region Colors

        public static Color OtherTagColor { get; set; }

        public static Color TagColor { get; set; }

        public static Color GraphColor { get; set; }

        public static Color BranchColor { get; set; }

        public static Color RemoteBranchColor { get; set; }

        public static Color DiffSectionColor { get; set; }

        public static Color DiffRemovedColor { get; set; }

        public static Color DiffRemovedExtraColor { get; set; }

        public static Color DiffAddedColor { get; set; }

        public static Color DiffAddedExtraColor { get; set; }

        public static bool MulticolorBranches { get; set; }

        public static bool StripedBranchChange { get; set; }

        public static bool BranchBorders { get; set; }

        #endregion

        public static string GetDictionaryDir()
        {
            if (Application.UserAppDataRegistry != null &&
                Application.UserAppDataRegistry.GetValue("InstallDir") != null)
                return GetInstallDir() + "\\Dictionaries\\";

            return "";
        }


        public static string GetInstallDir()
        {
            if (Application.UserAppDataRegistry != null &&
                Application.UserAppDataRegistry.GetValue("InstallDir") != null)
                return Application.UserAppDataRegistry.GetValue("InstallDir").ToString();

            return "";
        }

        public static void SetInstallDir(string dir)
        {
            if (Application.UserAppDataRegistry != null)
                Application.UserAppDataRegistry.SetValue("InstallDir", dir);
        }

        public static bool ValidWorkingDir()
        {
            return ValidWorkingDir(WorkingDir);
        }

        public static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            if (Directory.Exists(dir + "\\" + ".git"))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + "\\" + "info") &&
                   Directory.Exists(dir + "\\" + "objects") &&
                   Directory.Exists(dir + "\\" + "refs");
        }

        public static bool IsBareRepository()
        {
            return !Directory.Exists(WorkingDir + "\\" + ".git");
        }

        public static string WorkingDirGitDir()
        {
            var workingDir = WorkingDir;

            if (Directory.Exists(workingDir + ".git"))
                return workingDir + ".git";

            if (Directory.Exists(workingDir + "\\" + ".git"))
                return workingDir + "\\" + ".git";

            return WorkingDir;
        }


        public static void SaveSettings()
        {
            try
            {
                if (Application.UserAppDataRegistry == null)
                    throw new Exception("Application.UserAppDataRegistry is not available");

                var appData = Application.UserAppDataRegistry;
                if (Encoding.GetType() == typeof (ASCIIEncoding))
                    appData.SetValue("encoding", "ASCII");
                else if (Encoding.GetType() == typeof (UnicodeEncoding))
                    appData.SetValue("encoding", "Unicode");
                else if (Encoding.GetType() == typeof (UTF7Encoding))
                    appData.SetValue("encoding", "UTF7");
                else if (Encoding.GetType() == typeof (UTF8Encoding))
                    appData.SetValue("encoding", "UTF8");
                else if (Encoding.GetType() == typeof (UTF32Encoding))
                    appData.SetValue("encoding", "UTF32");
                else if (Encoding == Encoding.Default)
                    appData.SetValue("encoding", "Default");

                appData.SetValue("history", Repositories.SerializeHistory());
                appData.SetValue("repositories", Repositories.SerializeRepositories());
                appData.SetValue("showauthorgravatar", ShowAuthorGravatar);
                appData.SetValue("userprofilehomedir", UserProfileHomeDir);
                appData.SetValue("customhomedir", CustomHomeDir);
                appData.SetValue("closeCommitDialogAfterCommit", CloseCommitDialogAfterCommit);
                appData.SetValue("markIllFormedLinesInCommitMsg", MarkIllFormedLinesInCommitMsg);
                appData.SetValue("diffaddedcolor", ColorTranslator.ToHtml(DiffAddedColor));
                appData.SetValue("diffremovedcolor", ColorTranslator.ToHtml(DiffRemovedColor));
                appData.SetValue("diffaddedextracolor", ColorTranslator.ToHtml(DiffAddedExtraColor));
                appData.SetValue("diffremovedextracolor", ColorTranslator.ToHtml(DiffRemovedExtraColor));
                appData.SetValue("diffsectioncolor", ColorTranslator.ToHtml(DiffSectionColor));
                appData.SetValue("multicolorbranches", MulticolorBranches);
                appData.SetValue("branchborders", BranchBorders);
                appData.SetValue("stripedbanchchange", StripedBranchChange);
                appData.SetValue("tagcolor", ColorTranslator.ToHtml(TagColor));
                appData.SetValue("graphcolor", ColorTranslator.ToHtml(GraphColor));
                appData.SetValue("branchcolor", ColorTranslator.ToHtml(BranchColor));
                appData.SetValue("remotebranchcolor", ColorTranslator.ToHtml(RemoteBranchColor));
                appData.SetValue("othertagcolor", ColorTranslator.ToHtml(OtherTagColor));
                appData.SetValue("iconcolor", IconColor);
                appData.SetValue("translation", Translation);
                appData.SetValue("authorImageCacheDays", AuthorImageCacheDays);
                appData.SetValue("authorimagesize", AuthorImageSize);
                appData.SetValue("maxcommits", MaxCommits);
                appData.SetValue("gitdir", GitCommand);
                appData.SetValue("gitbindir", GitBinDir);
                appData.SetValue("showallbranches", ShowCurrentBranchOnly);
                appData.SetValue("branchfilterenabled", BranchFilterEnabled);
                appData.SetValue("closeprocessdialog", CloseProcessDialog);
                appData.SetValue("showrevisiongraph", ShowRevisionGraph);
                appData.SetValue("showauthordate", ShowAuthorDate);
                appData.SetValue("orderrevisiongraphbydate", OrderRevisionByDate);
                appData.SetValue("showgitcommandline", ShowGitCommandLine);
                appData.SetValue("usefastchecks", UseFastChecks);
                appData.SetValue("relativedate", RelativeDate);
                appData.SetValue("gitssh", GitCommands.GetSsh());
                appData.SetValue("pullmerge", PullMerge);
                appData.SetValue("autostash", AutoStash);
                appData.SetValue("followrenamesinfilehistory", FollowRenamesInFileHistory);
                appData.SetValue("plink", Plink);
                appData.SetValue("puttygen", Puttygen);
                appData.SetValue("pageant", Pageant);
                appData.SetValue("smtp", Smtp);
                appData.SetValue("dictionary", Dictionary);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load settings.\n\n" + ex.Message);
            }
        }

        public static void LoadSettings()
        {
            try
            {
                var appData = Application.UserAppDataRegistry;

                if (appData != null)
                {
                    if (appData.GetValue("maxcommits") != null)
                    {
                        int result;
                        if (int.TryParse(appData.GetValue("maxcommits").ToString(), out result))
                        {
                            MaxCommits = result;
                        }
                    }

                    if (appData.GetValue("authorImageCacheDays") != null)
                    {
                        int result;
                        if (int.TryParse(appData.GetValue("authorImageCacheDays").ToString(),
                                         out result))
                        {
                            AuthorImageCacheDays = result;
                        }
                    }


                    if (appData.GetValue("authorimagesize") != null)
                    {
                        int result;
                        if (int.TryParse(appData.GetValue("authorimagesize").ToString(), out result))
                        {
                            AuthorImageSize = result;
                        }
                    }
                }

                string encoding = null;
                if (appData != null &&
                    appData.GetValue("encoding") != null)
                    encoding = appData.GetValue("encoding").ToString();

                if (string.IsNullOrEmpty(encoding))
                    Encoding = new UTF8Encoding(false);
                else if (encoding.Equals("Default", StringComparison.CurrentCultureIgnoreCase))
                    Encoding = Encoding.Default;
                else if (encoding.Equals("Unicode", StringComparison.CurrentCultureIgnoreCase))
                    Encoding = new UnicodeEncoding();
                else if (encoding.Equals("ASCII", StringComparison.CurrentCultureIgnoreCase))
                    Encoding = new ASCIIEncoding();
                else if (encoding.Equals("UTF7", StringComparison.CurrentCultureIgnoreCase))
                    Encoding = new UTF7Encoding();
                else if (encoding.Equals("UTF32", StringComparison.CurrentCultureIgnoreCase))
                    Encoding = new UTF32Encoding(true, false);
                else
                    Encoding = new UTF8Encoding(false);

                try
                {
                    if (appData != null)
                    {
                        if (appData.GetValue("diffaddedcolor") != null)
                            DiffAddedColor =
                                ColorTranslator.FromHtml(
                                    appData.GetValue("diffaddedcolor").ToString());
                        if (appData.GetValue("diffremovedcolor") != null)
                            DiffRemovedColor =
                                ColorTranslator.FromHtml(
                                    appData.GetValue("diffremovedcolor").ToString());
                        if (appData.GetValue("diffaddedextracolor") != null)
                            DiffAddedExtraColor =
                                ColorTranslator.FromHtml(
                                    appData.GetValue("diffaddedextracolor").ToString());
                        if (appData.GetValue("diffremovedextracolor") != null)
                            DiffRemovedExtraColor =
                                ColorTranslator.FromHtml(
                                    appData.GetValue("diffremovedextracolor").ToString());
                        if (appData.GetValue("diffsectioncolor") != null)
                            DiffSectionColor =
                                ColorTranslator.FromHtml(
                                    appData.GetValue("diffsectioncolor").ToString());
                        if (appData.GetValue("tagcolor") != null)
                            TagColor =
                                ColorTranslator.FromHtml(appData.GetValue("tagcolor").ToString());
                        if (appData.GetValue("graphcolor") != null)
                            GraphColor =
                                ColorTranslator.FromHtml(appData.GetValue("graphcolor").ToString());
                        if (appData.GetValue("branchcolor") != null)
                            BranchColor =
                                ColorTranslator.FromHtml(appData.GetValue("branchcolor").ToString());
                        if (appData.GetValue("remotebranchcolor") != null)
                            RemoteBranchColor =
                                ColorTranslator.FromHtml(
                                    appData.GetValue("remotebranchcolor").ToString());
                        if (appData.GetValue("othertagcolor") != null)
                            OtherTagColor =
                                ColorTranslator.FromHtml(
                                    appData.GetValue("othertagcolor").ToString());
                        if (appData.GetValue("multicolorbranches") != null)
                            MulticolorBranches =
                                appData.GetValue("multicolorbranches").ToString() == "True";
                        if (appData.GetValue("branchborders") != null)
                            BranchBorders = appData.GetValue("branchborders").ToString() == "True";
                        if (appData.GetValue("stripedbanchchange") != null)
                            StripedBranchChange =
                                appData.GetValue("stripedbanchchange").ToString() == "True";
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }

                if (appData != null)
                {
                    if (appData.GetValue("translation") != null)
                        Translation = appData.GetValue("translation").ToString();
                    if (appData.GetValue("pullmerge") != null)
                        PullMerge = appData.GetValue("pullmerge").ToString();
                    if (appData.GetValue("gitssh") != null)
                        GitCommands.SetSsh(appData.GetValue("gitssh").ToString());
                    if (appData.GetValue("plink") != null)
                        Plink = appData.GetValue("plink").ToString();
                    if (appData.GetValue("puttygen") != null)
                        Puttygen = appData.GetValue("puttygen").ToString();
                    if (appData.GetValue("pageant") != null)
                        Pageant = appData.GetValue("pageant").ToString();

                    if (appData.GetValue("dictionary") != null)
                        Dictionary = appData.GetValue("dictionary").ToString();
                    if (appData.GetValue("smtp") != null)
                        Smtp = appData.GetValue("smtp").ToString();

                    if (appData.GetValue("showauthorgravatar") != null)
                        ShowAuthorGravatar = appData.GetValue("showauthorgravatar").ToString() ==
                                             "True";

                    if (appData.GetValue("userprofilehomedir") != null)
                        UserProfileHomeDir = appData.GetValue("userprofilehomedir").ToString() ==
                                             "True";
                    if (appData.GetValue("customhomedir") != null)
                        CustomHomeDir = appData.GetValue("customhomedir").ToString();

                    if (appData.GetValue("closeCommitDialogAfterCommit") != null)
                        CloseCommitDialogAfterCommit =
                            appData.GetValue("closeCommitDialogAfterCommit").ToString() == "True";
                    if (appData.GetValue("markIllFormedLinesInCommitMsg") != null)
                        MarkIllFormedLinesInCommitMsg =
                            appData.GetValue("markIllFormedLinesInCommitMsg").ToString() == "True";
                    if (appData.GetValue("followrenamesinfilehistory") != null)
                        FollowRenamesInFileHistory =
                            appData.GetValue("followrenamesinfilehistory").ToString() == "True";
                    if (appData.GetValue("autostash") != null)
                        AutoStash = appData.GetValue("autostash").ToString() == "True";

                    if (appData.GetValue("iconcolor") != null)
                        IconColor = appData.GetValue("iconcolor").ToString();
                    if (appData.GetValue("relativedate") != null)
                        RelativeDate = appData.GetValue("relativedate").ToString() == "True";
                    if (appData.GetValue("usefastchecks") != null)
                        UseFastChecks = appData.GetValue("usefastchecks").ToString() == "True";
                    if (appData.GetValue("showgitcommandline") != null)
                        ShowGitCommandLine = appData.GetValue("showgitcommandline").ToString() ==
                                             "True";
                    if (appData.GetValue("showrevisiongraph") != null)
                        ShowRevisionGraph = appData.GetValue("showrevisiongraph").ToString() ==
                                            "True";
                    if (appData.GetValue("showauthordate") != null)
                        ShowAuthorDate = appData.GetValue("showauthordate").ToString() == "True";
                    if (appData.GetValue("closeprocessdialog") != null)
                        CloseProcessDialog = appData.GetValue("closeprocessdialog").ToString() ==
                                             "True";
                    if (appData.GetValue("showallbranches") != null)
                        ShowCurrentBranchOnly = appData.GetValue("showallbranches").ToString() ==
                                                "False";
                    if (appData.GetValue("branchfilterenabled") != null)
                        BranchFilterEnabled = appData.GetValue("branchfilterenabled").ToString() ==
                                              "True";
                    if (appData.GetValue("orderrevisiongraphbydate") != null)
                        OrderRevisionByDate =
                            appData.GetValue("orderrevisiongraphbydate").ToString() == "True";
                    if (appData.GetValue("gitdir") != null)
                        GitCommand = appData.GetValue("gitdir").ToString();
                    if (appData.GetValue("gitbindir") != null)
                        GitBinDir = appData.GetValue("gitbindir").ToString();

                    if (appData.GetValue("history") != null)
                        Repositories.DeserializeHistory(appData.GetValue("history").ToString());
                    if (appData.GetValue("repositories") != null)
                        Repositories.DeserializeRepositories(
                            appData.GetValue("repositories").ToString());
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}