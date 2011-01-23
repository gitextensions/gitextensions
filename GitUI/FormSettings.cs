using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.Editor;
using Gravatar;
using Microsoft.Win32;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormSettings : GitExtensionsForm
    {
        private const string GitExtensionsShellExName = "GitExtensionsShellEx32.dll";

        public FormSettings()
        {
            InitializeComponent();
            Translate();

            _NO_TRANSLATE_Encoding.Items.AddRange(new Object[]
                                                      {
                                                          "Default (" + Encoding.Default.HeaderName + ")", "ASCII",
                                                          "Unicode", "UTF7", "UTF8", "UTF32"
                                                      });
            GlobalEditor.Items.AddRange(new Object[] { "\"" + GetGitExtensionsFullPath() + "\" fileeditor", "vi", "notepad" });

            defaultHome.Text = string.Format(defaultHome.Text + " ({0})", GitCommandHelpers.GetDefaultHomeDir());
            userprofileHome.Text = string.Format(userprofileHome.Text + " ({0})",
                                                 Environment.GetEnvironmentVariable("USERPROFILE"));
        }

        private int selectedScriptItem { get; set; }

        public static bool AutoSolveAllSettings()
        {
            if (!Settings.RunningOnWindows())
                return SolveGitCommand();

            return SolveGitCommand() &&
                   SolveLinuxToolsDir() &&
                   SolveKDiff() &&
                   SolveKDiffTool2() &&
                   SolveGitExtensionsDir() &&
                   SolveEditor();
        }

        private static bool SolveEditor()
        {
            string editor = GitCommandHelpers.GetGlobalSetting("core.editor");
            if (string.IsNullOrEmpty(editor))
            {
                GitCommandHelpers.SetGlobalSetting("core.editor", "\"" + GetGitExtensionsFullPath() + "\" fileeditor");
            }

            return true;
        }

        private static void SetCheckboxFromString(CheckBox checkBox, string str)
        {
            str = str.Trim().ToLower();

            switch (str)
            {
                case "true":
                    {
                        checkBox.CheckState = CheckState.Checked;
                        return;
                    }
                case "false":
                    {
                        checkBox.CheckState = CheckState.Unchecked;
                        return;
                    }
                default:
                    checkBox.CheckState = CheckState.Indeterminate;
                    return;
            }
        }

        private static string GetGlobalDiffToolFromConfig()
        {
            if (GitCommandHelpers.VersionInUse.GuiDiffToolExist)
                return GitCommandHelpers.GetGlobalConfig().GetValue("diff.guitool");
            return GitCommandHelpers.GetGlobalConfig().GetValue("diff.tool");
        }

        private static void SetGlobalDiffToolToConfig(ConfigFile configFile, string diffTool)
        {
            if (GitCommandHelpers.VersionInUse.GuiDiffToolExist)
            {
                configFile.SetValue("diff.guitool", diffTool);
                return;
            }
            configFile.SetValue("diff.tool", diffTool);
        }

        public void LoadSettings()
        {
            try
            {
                if (Settings.Encoding.GetType() == typeof(ASCIIEncoding))
                    _NO_TRANSLATE_Encoding.Text = "ASCII";
                else if (Settings.Encoding.GetType() == typeof(UnicodeEncoding))
                    _NO_TRANSLATE_Encoding.Text = "Unicode";
                else if (Settings.Encoding.GetType() == typeof(UTF7Encoding))
                    _NO_TRANSLATE_Encoding.Text = "UTF7";
                else if (Settings.Encoding.GetType() == typeof(UTF8Encoding))
                    _NO_TRANSLATE_Encoding.Text = "UTF8";
                else if (Settings.Encoding.GetType() == typeof(UTF32Encoding))
                    _NO_TRANSLATE_Encoding.Text = "UTF32";
                else if (Settings.Encoding == Encoding.Default)
                    _NO_TRANSLATE_Encoding.Text = "Default (" + Encoding.Default.HeaderName + ")";

                RevisionGridQuickSearchTimeout.Value = Settings.RevisionGridQuickSearchTimeout;

                FollowRenamesInFileHistory.Checked = Settings.FollowRenamesInFileHistory;

                _NO_TRANSLATE_DaysToCacheImages.Value = Settings.AuthorImageCacheDays;

                _NO_TRANSLATE_authorImageSize.Value = Settings.AuthorImageSize;
                ShowAuthorGravatar.Checked = Settings.ShowAuthorGravatar;
                noImageService.Text = Settings.GravatarFallbackService;

                showErrorsWhenStagingFiles.Checked = Settings.ShowErrorsWhenStagingFiles;

                Language.Items.Clear();
                Language.Items.Add("English");
                Language.Items.AddRange(Translator.GetAllTranslations());
                Language.Text = Settings.Translation;

                MulticolorBranches.Checked = Settings.MulticolorBranches;
                MulticolorBranches_CheckedChanged(null, null);
                DrawNonRelativesGray.Checked = Settings.RevisionGraphDrawNonRelativesGray;
                DrawNonRelativesTextGray.Checked = Settings.RevisionGraphDrawNonRelativesTextGray;
                ShowCurrentChangesInRevisionGraph.Checked = Settings.RevisionGraphShowWorkingDirChanges;
                ShowStashCountInBrowseWindow.Checked = Settings.ShowStashCount;
                BranchBorders.Checked = Settings.BranchBorders;
                StripedBanchChange.Checked = Settings.StripedBranchChange;

                ShowGitStatusInToolbar.Checked = Settings.ShowGitStatusInBrowseToolbar;

                _NO_TRANSLATE_ColorGraphLabel.BackColor = Settings.GraphColor;
                _NO_TRANSLATE_ColorGraphLabel.Text = Settings.GraphColor.Name;
                _NO_TRANSLATE_ColorGraphLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorGraphLabel.BackColor);
                _NO_TRANSLATE_ColorTagLabel.BackColor = Settings.TagColor;
                _NO_TRANSLATE_ColorTagLabel.Text = Settings.TagColor.Name;
                _NO_TRANSLATE_ColorTagLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorTagLabel.BackColor);
                _NO_TRANSLATE_ColorBranchLabel.BackColor = Settings.BranchColor;
                _NO_TRANSLATE_ColorBranchLabel.Text = Settings.BranchColor.Name;
                _NO_TRANSLATE_ColorBranchLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorBranchLabel.BackColor);
                _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = Settings.RemoteBranchColor;
                _NO_TRANSLATE_ColorRemoteBranchLabel.Text = Settings.RemoteBranchColor.Name;
                _NO_TRANSLATE_ColorRemoteBranchLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemoteBranchLabel.BackColor);
                _NO_TRANSLATE_ColorOtherLabel.BackColor = Settings.OtherTagColor;
                _NO_TRANSLATE_ColorOtherLabel.Text = Settings.OtherTagColor.Name;
                _NO_TRANSLATE_ColorOtherLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorOtherLabel.BackColor);


                _NO_TRANSLATE_ColorAddedLineLabel.BackColor = Settings.DiffAddedColor;
                _NO_TRANSLATE_ColorAddedLineLabel.Text = Settings.DiffAddedColor.Name;
                _NO_TRANSLATE_ColorAddedLineLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineLabel.BackColor);
                _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = Settings.DiffAddedExtraColor;
                _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = Settings.DiffAddedExtraColor.Name;
                _NO_TRANSLATE_ColorAddedLineDiffLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);

                _NO_TRANSLATE_ColorRemovedLine.BackColor = Settings.DiffRemovedColor;
                _NO_TRANSLATE_ColorRemovedLine.Text = Settings.DiffRemovedColor.Name;
                _NO_TRANSLATE_ColorRemovedLine.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLine.BackColor);
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = Settings.DiffRemovedExtraColor;
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = Settings.DiffRemovedExtraColor.Name;
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor);
                _NO_TRANSLATE_ColorSectionLabel.BackColor = Settings.DiffSectionColor;
                _NO_TRANSLATE_ColorSectionLabel.Text = Settings.DiffSectionColor.Name;
                _NO_TRANSLATE_ColorSectionLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorSectionLabel.BackColor);

                if (!string.IsNullOrEmpty(Settings.CustomHomeDir))
                {
                    defaultHome.Checked = userprofileHome.Checked = false;
                    otherHome.Checked = true;
                    otherHomeDir.Text = Settings.CustomHomeDir;
                }
                else if (Settings.UserProfileHomeDir)
                {
                    defaultHome.Checked = otherHome.Checked = false;
                    userprofileHome.Checked = true;
                }
                else
                {
                    userprofileHome.Checked = otherHome.Checked = false;
                    defaultHome.Checked = true;
                }

                SmtpServer.Text = Settings.Smtp;

                _NO_TRANSLATE_MaxCommits.Value = Settings.MaxCommits;

                GitPath.Text = Settings.GitCommand;
                GitBinPath.Text = Settings.GitBinDir;

                ConfigFile localConfig = GitCommandHelpers.GetLocalConfig();
                ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();

                UserName.Text = localConfig.GetValue("user.name");
                UserEmail.Text = localConfig.GetValue("user.email");
                Editor.Text = localConfig.GetValue("core.editor");
                MergeTool.Text = localConfig.GetValue("merge.tool");

                Dictionary.Text = Settings.Dictionary;

                GlobalUserName.Text = globalConfig.GetValue("user.name");
                GlobalUserEmail.Text = globalConfig.GetValue("user.email");
                GlobalEditor.Text = globalConfig.GetValue("core.editor");
                GlobalMergeTool.Text = globalConfig.GetValue("merge.tool");

                SetCheckboxFromString(KeepMergeBackup, localConfig.GetValue("mergetool.keepBackup"));

                localAutoCrlfFalse.Checked = localConfig.GetValue("core.autocrlf").Equals("false",
                                                                                          StringComparison.
                                                                                              OrdinalIgnoreCase);
                localAutoCrlfInput.Checked = localConfig.GetValue("core.autocrlf").Equals("input",
                                                                                          StringComparison.
                                                                                              OrdinalIgnoreCase);
                localAutoCrlfTrue.Checked = localConfig.GetValue("core.autocrlf").Equals("true",
                                                                                         StringComparison.
                                                                                             OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    MergetoolPath.Text = globalConfig.GetValue("mergetool." + GlobalMergeTool.Text + ".path");
                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    MergeToolCmd.Text = globalConfig.GetValue("mergetool." + GlobalMergeTool.Text + ".cmd");

                DefaultIcon.Checked = Settings.IconColor.Equals("default", StringComparison.CurrentCultureIgnoreCase);
                BlueIcon.Checked = Settings.IconColor.Equals("blue", StringComparison.CurrentCultureIgnoreCase);
                GreenIcon.Checked = Settings.IconColor.Equals("green", StringComparison.CurrentCultureIgnoreCase);
                PurpleIcon.Checked = Settings.IconColor.Equals("purple", StringComparison.CurrentCultureIgnoreCase);
                RedIcon.Checked = Settings.IconColor.Equals("red", StringComparison.CurrentCultureIgnoreCase);
                YellowIcon.Checked = Settings.IconColor.Equals("yellow", StringComparison.CurrentCultureIgnoreCase);
                RandomIcon.Checked = Settings.IconColor.Equals("random", StringComparison.CurrentCultureIgnoreCase);

                GlobalDiffTool.Text = GetGlobalDiffToolFromConfig();

                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    DifftoolPath.Text = globalConfig.GetValue("difftool." + GlobalDiffTool.Text + ".path");
                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    DifftoolCmd.Text = globalConfig.GetValue("difftool." + GlobalDiffTool.Text + ".cmd");

                SetCheckboxFromString(GlobalKeepMergeBackup, globalConfig.GetValue("mergetool.keepBackup"));

                string globalAutocrlf = string.Empty;
                if (globalConfig.HasValue("core.autocrlf"))
                {
                    globalAutocrlf = globalConfig.GetValue("core.autocrlf");
                }
                else if (!string.IsNullOrEmpty(Settings.GitBinDir))
                {
                    try
                    {
                        //the following lines only work for msysgit (i think). MSysgit has a config file
                        //in the etc directory which is checked after the local and global config. In
                        //practice this is only used to core.autocrlf. If there are more cases, we might
                        //need to consider a better solution.
                        var configFile =
                            new ConfigFile(Path.GetDirectoryName(Settings.GitBinDir).Replace("bin", "etc\\gitconfig"));
                        globalAutocrlf = configFile.GetValue("core.autocrlf");
                    }
                    catch
                    {
                    }
                }

                globalAutoCrlfFalse.Checked = globalAutocrlf.Equals("false", StringComparison.OrdinalIgnoreCase);
                globalAutoCrlfInput.Checked = globalAutocrlf.Equals("input", StringComparison.OrdinalIgnoreCase);
                globalAutoCrlfTrue.Checked = globalAutocrlf.Equals("true", StringComparison.OrdinalIgnoreCase);

                PlinkPath.Text = Settings.Plink;
                PuttygenPath.Text = Settings.Puttygen;
                PageantPath.Text = Settings.Pageant;
                AutostartPageant.Checked = Settings.AutoStartPageant;

                CloseProcessDialog.Checked = Settings.CloseProcessDialog;
                ShowRevisionGraph.Checked = Settings.ShowRevisionGraph;
                ShowGitCommandLine.Checked = Settings.ShowGitCommandLine;

                UseFastChecks.Checked = Settings.UseFastChecks;
                ShowRelativeDate.Checked = Settings.RelativeDate;

                if (string.IsNullOrEmpty(GitCommandHelpers.GetSsh()))
                    OpenSSH.Checked = true;
                else if (GitCommandHelpers.Plink())
                    Putty.Checked = true;
                else
                {
                    OtherSsh.Text = GitCommandHelpers.GetSsh();
                    Other.Checked = true;
                }

                EnableSshOptions();
                LoadScripts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load settings.\n\n" + ex);

                // Bail out before the user saves the incompletely loaded settings
                // and has their day ruined.
                DialogResult = DialogResult.Abort;
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool Save()
        {
            if (otherHome.Checked)
            {
                Settings.UserProfileHomeDir = false;
                if (string.IsNullOrEmpty(otherHomeDir.Text))
                {
                    MessageBox.Show("Please enter a valid HOME directory.");
                    new FormFixHome().ShowDialog();
                }
                else
                    Settings.CustomHomeDir = otherHomeDir.Text;
            }
            else
            {
                Settings.CustomHomeDir = "";
                Settings.UserProfileHomeDir = userprofileHome.Checked;
            }

            FormFixHome.CheckHomePath();

            GitCommandHelpers.SetEnvironmentVariable(true);

            Settings.ShowErrorsWhenStagingFiles = showErrorsWhenStagingFiles.Checked;

            Settings.FollowRenamesInFileHistory = FollowRenamesInFileHistory.Checked;

            if ((int)_NO_TRANSLATE_authorImageSize.Value != Settings.AuthorImageSize)
            {
                Settings.AuthorImageSize = (int)_NO_TRANSLATE_authorImageSize.Value;
                GravatarService.ClearImageCache();
            }
            Settings.Translation = Language.Text;

            Settings.ShowGitStatusInBrowseToolbar = ShowGitStatusInToolbar.Checked;

            Settings.AuthorImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;

            Settings.Smtp = SmtpServer.Text;

            Settings.GitCommand = GitPath.Text;
            Settings.GitBinDir = GitBinPath.Text;

            Settings.ShowAuthorGravatar = ShowAuthorGravatar.Checked;
            Settings.GravatarFallbackService = noImageService.Text;

            Settings.CloseProcessDialog = CloseProcessDialog.Checked;
            Settings.ShowRevisionGraph = ShowRevisionGraph.Checked;
            Settings.ShowGitCommandLine = ShowGitCommandLine.Checked;

            Settings.UseFastChecks = UseFastChecks.Checked;
            Settings.RelativeDate = ShowRelativeDate.Checked;

            Settings.Dictionary = Dictionary.Text;

            Settings.MaxCommits = (int)_NO_TRANSLATE_MaxCommits.Value;

            Settings.Plink = PlinkPath.Text;
            Settings.Puttygen = PuttygenPath.Text;
            Settings.Pageant = PageantPath.Text;
            Settings.AutoStartPageant = AutostartPageant.Checked;

            if (string.IsNullOrEmpty(_NO_TRANSLATE_Encoding.Text) ||
                _NO_TRANSLATE_Encoding.Text.StartsWith("Default", StringComparison.CurrentCultureIgnoreCase))
                Settings.Encoding = Encoding.Default;
            else if (_NO_TRANSLATE_Encoding.Text.Equals("ASCII", StringComparison.CurrentCultureIgnoreCase))
                Settings.Encoding = new ASCIIEncoding();
            else if (_NO_TRANSLATE_Encoding.Text.Equals("Unicode", StringComparison.CurrentCultureIgnoreCase))
                Settings.Encoding = new UnicodeEncoding();
            else if (_NO_TRANSLATE_Encoding.Text.Equals("UTF7", StringComparison.CurrentCultureIgnoreCase))
                Settings.Encoding = new UTF7Encoding();
            else if (_NO_TRANSLATE_Encoding.Text.Equals("UTF8", StringComparison.CurrentCultureIgnoreCase))
                Settings.Encoding = new UTF8Encoding(false);
            else if (_NO_TRANSLATE_Encoding.Text.Equals("UTF32", StringComparison.CurrentCultureIgnoreCase))
                Settings.Encoding = new UTF32Encoding(true, false);
            else
                Settings.Encoding = Encoding.Default;

            Settings.RevisionGridQuickSearchTimeout = (int)RevisionGridQuickSearchTimeout.Value;

            Settings.MulticolorBranches = MulticolorBranches.Checked;
            Settings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.Checked;
            Settings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.Checked;
            Settings.RevisionGraphShowWorkingDirChanges = ShowCurrentChangesInRevisionGraph.Checked;
            Settings.ShowStashCount = ShowStashCountInBrowseWindow.Checked;
            Settings.BranchBorders = BranchBorders.Checked;
            Settings.StripedBranchChange = StripedBanchChange.Checked;
            Settings.GraphColor = _NO_TRANSLATE_ColorGraphLabel.BackColor;
            Settings.TagColor = _NO_TRANSLATE_ColorTagLabel.BackColor;
            Settings.BranchColor = _NO_TRANSLATE_ColorBranchLabel.BackColor;
            Settings.RemoteBranchColor = _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor;
            Settings.OtherTagColor = _NO_TRANSLATE_ColorOtherLabel.BackColor;

            Settings.DiffAddedColor = _NO_TRANSLATE_ColorAddedLineLabel.BackColor;
            Settings.DiffRemovedColor = _NO_TRANSLATE_ColorRemovedLine.BackColor;
            Settings.DiffAddedExtraColor = _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor;
            Settings.DiffRemovedExtraColor = _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor;

            Settings.DiffSectionColor = _NO_TRANSLATE_ColorSectionLabel.BackColor;

            if (DefaultIcon.Checked)
                Settings.IconColor = "default";
            if (BlueIcon.Checked)
                Settings.IconColor = "blue";
            if (GreenIcon.Checked)
                Settings.IconColor = "green";
            if (PurpleIcon.Checked)
                Settings.IconColor = "purple";
            if (RedIcon.Checked)
                Settings.IconColor = "red";
            if (YellowIcon.Checked)
                Settings.IconColor = "yellow";
            if (RandomIcon.Checked)
                Settings.IconColor = "random";

            EnableSettings();

            if (!CanFindGitCmd())
            {
                if (
                    MessageBox.Show(
                        "The command to run git is not configured correct." + Environment.NewLine +
                        "You need to set the correct path to be able to use GitExtensions." + Environment.NewLine +
                        Environment.NewLine + "Do you want to set the correct command now?", "Incorrect path",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;
            }
            else
            {
                handleCanFindGitCommand();
            }

            if (OpenSSH.Checked)
                GitCommandHelpers.UnsetSsh();

            if (Putty.Checked)
                GitCommandHelpers.SetSsh(PlinkPath.Text);

            if (Other.Checked)
                GitCommandHelpers.SetSsh(OtherSsh.Text);

            Settings.SaveSettings();

            return true;
        }

        private void handleCanFindGitCommand()
        {
            ConfigFile localConfig = GitCommandHelpers.GetLocalConfig();
            ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();

            if (string.IsNullOrEmpty(UserName.Text) || !UserName.Text.Equals(localConfig.GetValue("user.name")))
                localConfig.SetValue("user.name", UserName.Text);
            if (string.IsNullOrEmpty(UserEmail.Text) || !UserEmail.Text.Equals(localConfig.GetValue("user.email")))
                localConfig.SetValue("user.email", UserEmail.Text);
            localConfig.SetValue("core.editor", Editor.Text);
            localConfig.SetValue("merge.tool", MergeTool.Text);


            if (KeepMergeBackup.CheckState == CheckState.Checked)
                localConfig.SetValue("mergetool.keepBackup", "true");
            else if (KeepMergeBackup.CheckState == CheckState.Unchecked)
                localConfig.SetValue("mergetool.keepBackup", "false");

            if (localAutoCrlfFalse.Checked) localConfig.SetValue("core.autocrlf", "false");
            if (localAutoCrlfInput.Checked) localConfig.SetValue("core.autocrlf", "input");
            if (localAutoCrlfTrue.Checked) localConfig.SetValue("core.autocrlf", "true");

            if (string.IsNullOrEmpty(GlobalUserName.Text) ||
                !GlobalUserName.Text.Equals(globalConfig.GetValue("user.name")))
                globalConfig.SetValue("user.name", GlobalUserName.Text);
            if (string.IsNullOrEmpty(GlobalUserEmail.Text) ||
                !GlobalUserEmail.Text.Equals(globalConfig.GetValue("user.email")))
                globalConfig.SetValue("user.email", GlobalUserEmail.Text);
            globalConfig.SetValue("core.editor", GlobalEditor.Text);

            SetGlobalDiffToolToConfig(globalConfig, GlobalDiffTool.Text);

            if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                globalConfig.SetValue("difftool." + GlobalDiffTool.Text + ".path", DifftoolPath.Text);
            if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                globalConfig.SetValue("difftool." + GlobalDiffTool.Text + ".cmd", DifftoolCmd.Text);

            globalConfig.SetValue("merge.tool", GlobalMergeTool.Text);

            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                globalConfig.SetValue("mergetool." + GlobalMergeTool.Text + ".path", MergetoolPath.Text);
            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                globalConfig.SetValue("mergetool." + GlobalMergeTool.Text + ".cmd", MergeToolCmd.Text);

            if (GlobalKeepMergeBackup.CheckState == CheckState.Checked)
                globalConfig.SetValue("mergetool.keepBackup", "true");
            else if (GlobalKeepMergeBackup.CheckState == CheckState.Unchecked)
                globalConfig.SetValue("mergetool.keepBackup", "false");

            if (globalAutoCrlfFalse.Checked) globalConfig.SetValue("core.autocrlf", "false");
            if (globalAutoCrlfInput.Checked) globalConfig.SetValue("core.autocrlf", "input");
            if (globalAutoCrlfTrue.Checked) globalConfig.SetValue("core.autocrlf", "true");

            globalConfig.Save();

            //Only save local settings when we are inside a valid working dir
            if (Settings.ValidWorkingDir())
                localConfig.Save();
        }

        private static string GetRegistryValue(RegistryKey root, string subkey, string key)
        {
            string value = null;
            try
            {
                RegistryKey registryKey = root.OpenSubKey(subkey, false);
                if (registryKey != null)
                {
                    using (registryKey)
                    {
                        value = registryKey.GetValue(key) as string;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("GitExtensions has insufficient permissions to check the registry.");
            }
            return value ?? string.Empty;
        }

        protected void SetRegistryValue(RegistryKey root, string subkey, string key, string value)
        {
            try
            {
                value = value.Replace("\\", "\\\\");
                string reg = "Windows Registry Editor Version 5.00" + Environment.NewLine + Environment.NewLine + "[" + root +
                             "\\" + subkey + "]" + Environment.NewLine + "\"" + key + "\"=\"" + value + "\"";

                TextWriter tw = new StreamWriter(Path.GetTempPath() + "GitExtensions.reg", false);
                tw.Write(reg);
                tw.Close();
                GitCommandHelpers.RunCmd("regedit", "\"" + Path.GetTempPath() + "GitExtensions.reg" + "\"");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("GitExtensions has insufficient permissions to modify the registry." +
                                Environment.NewLine + "Please add this key to the registry manually." +
                                Environment.NewLine + "Path:   " + root + "\\" + subkey + Environment.NewLine +
                                "Value:  " + key + " = " + value);
            }
        }

        public bool CheckSettings()
        {
            bool bValid = true;
            try
            {
                // once a check fails, we want bValid to stay false
                bValid = CheckGitCmdValid();
                bValid = CheckGlobalUserSettingsValid() && bValid;
                bValid = CheckMergeTool() && bValid;
                bValid = CheckDiffToolConfiguration() && bValid;
                bValid = CheckTranslationConfigSettings() && bValid;

                if (Settings.RunningOnWindows())
                {
                    bValid = CheckGitExtensionsInstall() && bValid;
                    bValid = CheckGitExtensionRegistrySettings() && bValid;
                    bValid = CheckGitExe() && bValid;
                    bValid = CheckSSHSettings() && bValid;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            CheckAtStartup.Checked = getCheckAtStartupChecked(bValid);
            return bValid;
        }

        private static bool CanFindGitCmd()
        {
            return !string.IsNullOrEmpty(GitCommandHelpers.RunCmd(Settings.GitCommand, ""));
        }

        private void GitExtensionsInstall_Click(object sender, EventArgs e)
        {
            SolveGitExtensionsDir();

            CheckSettings();
        }

        public static bool SolveGitExtensionsDir()
        {
            string fileName = GetGitExtensionsDirectory();

            if (File.Exists(fileName))
            {
                Settings.SetInstallDir(fileName);
                return true;
            }

            return false;
        }

        private static string GetGitExtensionsFullPath()
        {
            return GetGitExtensionsDirectory() + "\\GitExtensions.exe";
        }

        private static string GetGitExtensionsDirectory()
        {
            string fileName = Assembly.GetAssembly(typeof(FormSettings)).Location;
            fileName = fileName.Substring(0, fileName.LastIndexOfAny(new[] { '\\', '/' }));
            return fileName;
        }


        private void ShellExtensionsRegistered_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Settings.GetInstallDir(), GitExtensionsShellExName);
            if (!File.Exists(path))
            {
                path = Assembly.GetAssembly(GetType()).Location;
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, GitExtensionsShellExName);
            }
            if (File.Exists(path))
            {
                GitCommandHelpers.RunCmd("regsvr32", string.Format("\"{0}\"", path));
            }
            else
            {
                MessageBox.Show(string.Format(
                    "Could not register the shell extension because '{0}' could not be found.", GitExtensionsShellExName));
            }

            CheckSettings();
        }

        private void UserNameSet_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("GlobalSettingsPage");
        }

        private void DiffTool2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GetGlobalDiffToolFromConfig()))
            {
                if (
                    MessageBox.Show(
                        "There is no difftool configured. Do you want to configure kdiff3 as your difftool?" +
                        Environment.NewLine + "Select no if you want to configure a different difftool yourself.",
                        "Mergetool", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveKDiffTool2();
                    GlobalDiffTool.Text = "kdiff3";
                }
                else
                {
                    tabControl1.SelectTab("GlobalSettingsPage");
                    return;
                }
            }

            if (GetGlobalDiffToolFromConfig().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                SolveKDiffTool2Path();
            }

            if (GetGlobalDiffToolFromConfig().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(GitCommandHelpers.GetGlobalSetting("difftool.kdiff3.path")))
            {
                MessageBox.Show("Path to kdiff3 could not be found automatically." + Environment.NewLine +
                                "Please make sure KDiff3 is installed or set path manually.");
                tabControl1.SelectTab("GlobalSettingsPage");
                return;
            }

            Rescan_Click(null, null);
        }

        private void DiffTool_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GitCommandHelpers.GetGlobalSetting("merge.tool")))
            {
                if (
                    MessageBox.Show(
                        "There is no mergetool configured. Do you want to configure kdiff3 as your mergetool?" +
                        Environment.NewLine + "Select no if you want to configure a different mergetool yourself.",
                        "Mergetool", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveKDiff();
                    GlobalMergeTool.Text = "kdiff3";
                }
                else
                {
                    tabControl1.SelectTab("GlobalSettingsPage");
                    return;
                }
            }

            if (GitCommandHelpers.GetGlobalSetting("merge.tool").Equals("kdiff3",
                                                                        StringComparison.CurrentCultureIgnoreCase))
            {
                SolveKDiffPath();
            }
            else if (
                GitCommandHelpers.GetGlobalSetting("merge.tool").Equals("p4merge",
                                                                        StringComparison.CurrentCultureIgnoreCase) ||
                GitCommandHelpers.GetGlobalSetting("merge.tool").Equals("TortoiseMerge",
                                                                        StringComparison.CurrentCultureIgnoreCase))
            {
                AutoConfigMergeToolcmd();
                GitCommandHelpers.SetGlobalSetting(
                    "mergetool." + GitCommandHelpers.GetGlobalSetting("merge.tool") + ".cmd", MergeToolCmd.Text);
            }


            if (
                GitCommandHelpers.GetGlobalSetting("merge.tool").Equals("kdiff3",
                                                                        StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(GitCommandHelpers.GetGlobalSetting("mergetool.kdiff3.path")))
            {
                MessageBox.Show("Path to kdiff3 could not be found automatically." + Environment.NewLine +
                                "Please make sure KDiff3 is installed or set path manually.");
                tabControl1.SelectTab("GlobalSettingsPage");
                return;
            }

            Rescan_Click(null, null);
        }

        public static bool SolveKDiff()
        {
            string mergeTool = GitCommandHelpers.GetGlobalSetting("merge.tool");
            if (string.IsNullOrEmpty(mergeTool))
            {
                mergeTool = "kdiff3";
                GitCommandHelpers.SetGlobalSetting("merge.tool", mergeTool);
            }

            if (mergeTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveKDiffPath();

            return true;
        }

        public static bool SolveKDiffTool2()
        {
            string diffTool = GetGlobalDiffToolFromConfig();
            if (string.IsNullOrEmpty(diffTool))
            {
                diffTool = "kdiff3";
                ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
                SetGlobalDiffToolToConfig(globalConfig, diffTool);
                globalConfig.Save();
            }

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveKDiffTool2Path();

            return true;
        }

        public static bool SolveKDiffPath()
        {
            if (!Settings.RunningOnWindows())
                return false;

            string kdiff3path = GitCommandHelpers.GetGlobalSetting("mergetool.kdiff3.path");
            if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
            {
                kdiff3path = @"c:\Program Files\KDiff3\kdiff3.exe";
                if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                {
                    kdiff3path = @"c:\Program Files (x86)\KDiff3\kdiff3.exe";
                    if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                    {
                        kdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";
                        if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                        {
                            return false;
                        }
                    }
                }
            }
            GitCommandHelpers.SetGlobalSetting("mergetool.kdiff3.path", kdiff3path);

            return true;
        }

        public static bool SolveKDiffTool2Path()
        {
            if (!Settings.RunningOnWindows())
                return false;

            string kdiff3path = GitCommandHelpers.GetGlobalSetting("difftool.kdiff3.path");
            if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
            {
                kdiff3path = @"c:\Program Files\KDiff3\kdiff3.exe";
                if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                {
                    kdiff3path = @"c:\Program Files (x86)\KDiff3\kdiff3.exe";
                    if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                    {
                        kdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";
                        if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                        {
                            return false;
                        }
                    }
                }
            }
            GitCommandHelpers.SetGlobalSetting("difftool.kdiff3.path", kdiff3path);

            return true;
        }

        private void GitFound_Click(object sender, EventArgs e)
        {
            if (!SolveGitCommand())
            {
                MessageBox.Show(
                    "The command to run git could not be determined automatically." + Environment.NewLine +
                    "Please make sure git (msysgit or cygwin) is installed or set the correct command manually.",
                    "Locate git");

                tabControl1.SelectTab("TabPageGit");
                return;
            }

            MessageBox.Show("Git can be run using: " + Settings.GitCommand, "Locate git");
            GitPath.Text = Settings.GitCommand;
            Rescan_Click(null, null);
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            EnableSettings();

            WindowState = FormWindowState.Normal;
        }

        private void EnableSettings()
        {
            bool canFindGitCmd = CanFindGitCmd();
            GlobalUserName.Enabled = canFindGitCmd;
            GlobalUserEmail.Enabled = canFindGitCmd;
            GlobalEditor.Enabled = canFindGitCmd;
            GlobalMergeTool.Enabled = canFindGitCmd;
            MergetoolPath.Enabled = canFindGitCmd;
            MergeToolCmd.Enabled = canFindGitCmd;
            GlobalKeepMergeBackup.Enabled = canFindGitCmd;

            InvalidGitPathGlobal.Visible = !canFindGitCmd;
            InvalidGitPathLocal.Visible = !canFindGitCmd;

            bool valid = Settings.ValidWorkingDir() && canFindGitCmd;
            UserName.Enabled = valid;
            UserEmail.Enabled = valid;
            Editor.Enabled = valid;
            MergeTool.Enabled = valid;
            KeepMergeBackup.Enabled = valid;
            localAutoCrlfFalse.Enabled = valid;
            localAutoCrlfInput.Enabled = valid;
            localAutoCrlfTrue.Enabled = valid;
            NoGitRepo.Visible = !valid;
        }

        private void CheckAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            Application.UserAppDataRegistry.SetValue("checksettings", CheckAtStartup.Checked ? "true" : "false");
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Save();
            LoadSettings();
            CheckSettings();
            Cursor.Current = Cursors.Default;
        }

        private void BrowseGitPath_Click(object sender, EventArgs e)
        {
            SolveGitCommand();

            var browseDialog = new OpenFileDialog
                                   {
                                       FileName = Settings.GitCommand,
                                       Filter = "Git.cmd (git.cmd)|git.cmd|Git.exe (git.exe)|git.exe|Git (git)|git"
                                   };

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                GitPath.Text = browseDialog.FileName;
            }
        }

        private void TabPageGitExtensions_Click(object sender, EventArgs e)
        {
            GitPath.Text = Settings.GitCommand;
        }

        private void GitPath_TextChanged(object sender, EventArgs e)
        {
            Settings.GitCommand = GitPath.Text;
            LoadSettings();
        }

        private void GitBinFound_Click(object sender, EventArgs e)
        {
            if (!SolveLinuxToolsDir())
            {
                MessageBox.Show(
                    "The path to linux tools (sh) could not be found automatically." + Environment.NewLine +
                    "Please make sure there are linux tools installed (through msysgit or cygwin) or set the correct path manually.",
                    "Locate linux tools");
                tabControl1.SelectTab("TabPageGit");
                return;
            }

            MessageBox.Show("Command sh can be run using: " + Settings.GitBinDir + "sh", "Locate linux tools");
            GitBinPath.Text = Settings.GitBinDir;
            Rescan_Click(null, null);
        }

        public static bool CheckIfFileIsInPath(string fileName)
        {
            string path = string.Concat(Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User), ";",
                                        Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Machine));

            return path.Split(';').Any(dir => File.Exists(dir + " \\" + fileName) || File.Exists(dir + fileName));
        }

        public static bool SolveLinuxToolsDir()
        {
            if (!Settings.RunningOnWindows())
            {
                Settings.GitBinDir = "";
                return true;
            }

            if (CheckIfFileIsInPath("sh.exe") || CheckIfFileIsInPath("sh"))
            {
                Settings.GitBinDir = "";
                return true;
            }

            if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
            {
                Settings.GitBinDir = @"c:\Program Files\Git\bin\";
                if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                {
                    Settings.GitBinDir = @"c:\Program Files (x86)\Git\bin\";
                    if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                    {
                        Settings.GitBinDir = "C:\\cygwin\\bin\\";
                        if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                        {
                            Settings.GitBinDir = Settings.GitCommand;
                            Settings.GitBinDir =
                                Settings.GitBinDir.Replace("\\cmd\\git.cmd", "\\bin\\").Replace("\\bin\\git.exe",
                                                                                                "\\bin\\");
                            if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                            {
                                Settings.GitBinDir =
                                    GetRegistryValue(Registry.LocalMachine,
                                                     "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1",
                                                     "InstallLocation") + "\\bin\\";
                                if (!File.Exists(Settings.GitBinDir + "sh.exe") &&
                                    !File.Exists(Settings.GitBinDir + "sh"))
                                {
                                    Settings.GitBinDir = "";
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void OpenSSH_CheckedChanged(object sender, EventArgs e)
        {
            EnableSshOptions();
        }

        private void Putty_CheckedChanged(object sender, EventArgs e)
        {
            if (Putty.Checked)
            {
                AutoFindPuttyPaths();
            }
            EnableSshOptions();
        }

        private bool AutoFindPuttyPaths()
        {
            if (!Settings.RunningOnWindows())
                return false;

            if (AutoFindPuttyPathsInDir("c:\\Program Files\\PuTTY\\")) return true;
            if (AutoFindPuttyPathsInDir("c:\\Program Files (x86)\\PuTTY\\")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files\\TortoiseGit\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files (x86)\\TortoiseGit\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files\\TortoiseSvn\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files (x86)\\TortoiseSvn\\bin")) return true;
            if (
                AutoFindPuttyPathsInDir(GetRegistryValue(Registry.LocalMachine,
                                                         "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\PuTTY_is1",
                                                         "InstallLocation"))) return true;
            if (AutoFindPuttyPathsInDir(Settings.GetInstallDir() + "\\PuTTY\\")) return true;

            return false;
        }

        private bool AutoFindPuttyPathsInDir(string installdir)
        {
            if (!installdir.EndsWith("\\"))
                installdir += "\\";

            if (!File.Exists(PlinkPath.Text))
            {
                if (File.Exists(installdir + "plink.exe"))
                    PlinkPath.Text = installdir + "plink.exe";
            }

            if (!File.Exists(PlinkPath.Text))
            {
                if (File.Exists(installdir + "TortoisePlink.exe"))
                    PlinkPath.Text = installdir + "TortoisePlink.exe";
            }

            if (!File.Exists(PuttygenPath.Text))
            {
                if (File.Exists(installdir + "puttygen.exe"))
                    PuttygenPath.Text = installdir + "puttygen.exe";
            }

            if (!File.Exists(PageantPath.Text))
            {
                if (File.Exists(installdir + "pageant.exe"))
                    PageantPath.Text = installdir + "pageant.exe";
            }

            if (File.Exists(PlinkPath.Text) && File.Exists(PuttygenPath.Text) && File.Exists(PageantPath.Text))
                return true;
            return false;
        }

        private static string SelectFile(string initialDirectory, string filter, string prev)
        {
            var dialog = new OpenFileDialog
                             {
                                 Filter = filter,
                                 InitialDirectory = initialDirectory,
                                 Title = "Select file"
                             };
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : prev;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OtherSsh.Text = SelectFile(".", "Executable file (*.exe)|*.exe", OtherSsh.Text);
        }

        private void Other_CheckedChanged(object sender, EventArgs e)
        {
            EnableSshOptions();
        }

        private void EnableSshOptions()
        {
            OtherSsh.Enabled = Other.Checked;
            OtherSshBrowse.Enabled = Other.Checked;

            PlinkPath.Enabled = Putty.Checked;
            PuttygenPath.Enabled = Putty.Checked;
            PageantPath.Enabled = Putty.Checked;
            PlinkBrowse.Enabled = Putty.Checked;
            PuttygenBrowse.Enabled = Putty.Checked;
            PageantBrowse.Enabled = Putty.Checked;
            AutostartPageant.Enabled = Putty.Checked;
        }

        private void PuttyBrowse_Click(object sender, EventArgs e)
        {
            PlinkPath.Text = SelectFile(".",
                                        "Plink.exe (plink.exe)|plink.exe|TortoisePlink.exe (tortoiseplink.exe)|tortoiseplink.exe",
                                        PlinkPath.Text);
        }

        private void PuttygenBrowse_Click(object sender, EventArgs e)
        {
            PuttygenPath.Text = SelectFile(".", "puttygen.exe (puttygen.exe)|puttygen.exe", PuttygenPath.Text);
        }

        private void PageantBrowse_Click(object sender, EventArgs e)
        {
            PageantPath.Text = SelectFile(".", "pageant.exe (pageant.exe)|pageant.exe", PageantPath.Text);
        }

        private void SshConfig_Click(object sender, EventArgs e)
        {
            if (Putty.Checked)
            {
                if (AutoFindPuttyPaths())
                    MessageBox.Show("All paths needed for PuTTY could be automatically found and are set.", "PuTTY");
                else
                    tabControl1.SelectTab("ssh");
            }
        }

        private void BrowseGitBinPath_Click(object sender, EventArgs e)
        {
            SolveLinuxToolsDir();

            var browseDialog = new FolderBrowserDialog { SelectedPath = Settings.GitBinDir };

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                GitBinPath.Text = browseDialog.SelectedPath;
            }
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                MergetoolPath.Text = SelectFile(".", "kdiff3.exe (kdiff3.exe)|kdiff3.exe", MergetoolPath.Text);
            }
            else if (GlobalMergeTool.Text.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase))
                MergetoolPath.Text = SelectFile(".", "p4merge.exe (p4merge.exe)|p4merge.exe", MergetoolPath.Text);
            else if (GlobalMergeTool.Text.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
                MergetoolPath.Text = SelectFile(".", "TortoiseMerge.exe (TortoiseMerge.exe)|TortoiseMerge.exe",
                                                MergetoolPath.Text);
            else
                MergetoolPath.Text = SelectFile(".", "*.exe (*.exe)|*.exe", MergetoolPath.Text);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!Settings.RunningOnWindows())
                return;

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                string kdiff3path = GitCommandHelpers.GetGlobalSetting("mergetool.kdiff3.path");
                string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                MergetoolPath.Text = FindFileInFolders("kdiff3.exe", kdiff3path,
                                                       @"c:\Program Files\KDiff3\",
                                                       @"c:\Program Files (x86)\KDiff3\",
                                                       regkdiff3path);
            }
            if (GlobalMergeTool.Text.Equals("winmerge", StringComparison.CurrentCultureIgnoreCase))
            {
                string winmergepath = GitCommandHelpers.GetGlobalSetting("mergetool.winmerge.path");

                MergetoolPath.Text = FindFileInFolders("winmergeu.exe", winmergepath,
                                                       @"c:\Program Files\winmerge\",
                                                       @"c:\Program Files (x86)\winmerge\");
            }
            AutoConfigMergeToolcmd();
        }

        private static string FindFileInFolders(string fileName, params string[] locations)
        {
            foreach (string location in locations)
            {
                if (!string.IsNullOrEmpty(location) && File.Exists(location))
                    return location;
                if (!string.IsNullOrEmpty(location) && File.Exists(location + fileName))
                    return location + fileName;
                if (!string.IsNullOrEmpty(location) && File.Exists(location + "\\" + fileName))
                    return location + "\\" + fileName;
            }

            return "";
        }

        private void AutoConfigMergeToolcmd()
        {
            if (GlobalMergeTool.Text.Equals("BeyondCompare3", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.Contains("kdiff3") || MergetoolPath.Text.Contains("TortoiseMerge"))
                    MergetoolPath.Text = "";
                if (string.IsNullOrEmpty(MergetoolPath.Text) || !File.Exists(MergetoolPath.Text))
                {
                    MergetoolPath.Text = @"C:\Program Files\Beyond Compare 3\bcomp.exe";

                    MergetoolPath.Text = FindFileInFolders("bcomp.exe",
                                                           @"C:\Program Files\Beyond Compare 3 (x86)\",
                                                           @"C:\Program Files\Beyond Compare 3\");

                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to bcomp.exe and press suggest.", "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"" + MergetoolPath.Text + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\"";
                return;
            }

            if (GlobalMergeTool.Text.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.Contains("kdiff3") || MergetoolPath.Text.Contains("TortoiseMerge"))
                    MergetoolPath.Text = "";
                if (string.IsNullOrEmpty(MergetoolPath.Text) || !File.Exists(MergetoolPath.Text))
                {
                    MergetoolPath.Text = @"c:\Program Files\Perforce\p4merge.exe";

                    MergetoolPath.Text = FindFileInFolders("p4merge.exe",
                                                           @"c:\Program Files (x86)\Perforce\",
                                                           @"c:\Program Files\Perforce\");

                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to p4merge.exe and press suggest.",
                                        "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"" + MergetoolPath.Text + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                return;
            }

            if (GlobalMergeTool.Text.Equals("Araxis", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.Contains("kdiff3") || MergetoolPath.Text.Contains("TortoiseMerge"))
                    MergetoolPath.Text = "";
                if (string.IsNullOrEmpty(MergetoolPath.Text) || !File.Exists(MergetoolPath.Text))
                {
                    MergetoolPath.Text = FindFileInFolders("Compare.exe",
                                                           @"C:\Program Files (x86)\Araxis\Araxis Merge\",
                                                           @"C:\Program Files\Araxis\Araxis Merge\",
                                                           @"C:\Program Files\Araxis 6.5\Araxis Merge\");

                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to Compare.exe and press suggest.",
                                        "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"" + MergetoolPath.Text +
                                    "\" -wait -merge -3 -a1 \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                return;
            }

            if (GlobalMergeTool.Text.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.ToLower().Contains("kdiff3") || MergetoolPath.Text.ToLower().Contains("p4merge"))
                    MergetoolPath.Text = "";
                if (string.IsNullOrEmpty(MergetoolPath.Text) || !File.Exists(MergetoolPath.Text))
                {
                    MergetoolPath.Text = FindFileInFolders("TortoiseMerge.exe",
                                                           @"c:\Program Files (x86)\TortoiseSVN\bin\",
                                                           @"c:\Program Files\TortoiseSVN\bin\",
                                                           @"c:\Program Files (x86)\TortoiseGit\bin\",
                                                           @"c:\Program Files\TortoiseGit\bin\");

                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to TortoiseMerge.exe and press suggest.",
                                        "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"" + MergetoolPath.Text +
                                    "\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                return;
            }

            if (GlobalMergeTool.Text.Equals("DiffMerge", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.ToLower().Contains("kdiff3") || MergetoolPath.Text.ToLower().Contains("p4merge"))
                    MergetoolPath.Text = "";
                if (string.IsNullOrEmpty(MergetoolPath.Text) || !File.Exists(MergetoolPath.Text))
                {
                    MergetoolPath.Text = FindFileInFolders("DiffMerge.exe",
                                                           @"C:\Program Files (x86)\SourceGear\DiffMerge\",
                                                           @"C:\Program Files\SourceGear\DiffMerge\");

                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to DiffMerge.exe and press suggest.",
                                        "Suggest mergetool cmd");
                        return;
                    }
                }

                // /m /r=%merged /t1=%yname /t2=%bname /t3=%tname /c=%mname %mine %base %theirs
                MergeToolCmd.Text = "\"" + MergetoolPath.Text + "\" /m /r=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"";
                return;
            }
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            WindowState = FormWindowState.Normal;
            LoadSettings();
            CheckSettings();
            WindowState = FormWindowState.Normal;
            Cursor.Current = Cursors.Default;
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (DialogResult != DialogResult.Abort && !Save())
                e.Cancel = true;
            Cursor.Current = Cursors.Default;
        }

        private void Dictionary_DropDown(object sender, EventArgs e)
        {
            try
            {
                Dictionary.Items.Clear();
                Dictionary.Items.Add("None");
                foreach (
                    string fileName in
                        Directory.GetFiles(Settings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);
                    Dictionary.Items.Add(file.Name.Replace(".dic", ""));
                }
            }
            catch
            {
                MessageBox.Show("No dictionary files found in: " + Settings.GetDictionaryDir());
            }
        }

        private void ExternalDiffTool_TextChanged(object sender, EventArgs e)
        {
            DifftoolPath.Text = GitCommandHelpers.GetGlobalSetting("difftool." + GlobalDiffTool.Text.Trim() + ".path");
            DifftoolCmd.Text = GitCommandHelpers.GetGlobalSetting("difftool." + GlobalDiffTool.Text.Trim() + ".cmd");

            if (GlobalDiffTool.Text.Trim().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                ResolveDiffToolPath();

            DiffToolCmdSuggest_Click(null, null);
        }

        private void ResolveDiffToolPath()
        {
            if (!Settings.RunningOnWindows())
                return;

            if (GlobalDiffTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                string kdiff3path = GitCommandHelpers.GetGlobalSetting("difftool.kdiff3.path");
                if (!kdiff3path.ToLower().Contains("kdiff3.exe"))
                    kdiff3path = "";
                if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                {
                    kdiff3path = @"c:\Program Files\KDiff3\kdiff3.exe";
                    if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                    {
                        kdiff3path = @"c:\Program Files (x86)\KDiff3\kdiff3.exe";
                        if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                        {
                            kdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") +
                                         "\\kdiff3.exe";
                            if (string.IsNullOrEmpty(kdiff3path) || !File.Exists(kdiff3path))
                            {
                                kdiff3path = MergetoolPath.Text;
                                if (!kdiff3path.ToLower().Contains("kdiff3.exe"))
                                    kdiff3path = "";
                            }
                        }
                    }
                }
                DifftoolPath.Text = kdiff3path;
            }
        }

        private void BrowseDiffTool_Click(object sender, EventArgs e)
        {
            if (GlobalDiffTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                DifftoolPath.Text = SelectFile(".", "kdiff3.exe (kdiff3.exe)|kdiff3.exe", DifftoolPath.Text);
            }
            else if (GlobalDiffTool.Text.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase))
                DifftoolPath.Text = SelectFile(".", "p4merge.exe (p4merge.exe)|p4merge.exe", DifftoolPath.Text);
            else if (GlobalDiffTool.Text.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
                DifftoolPath.Text = SelectFile(".", "TortoiseMerge.exe (TortoiseMerge.exe)|TortoiseMerge.exe",
                                               DifftoolPath.Text);
            else
                DifftoolPath.Text = SelectFile(".", "*.exe (*.exe)|*.exe", DifftoolPath.Text);
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            MergetoolPath.Text = GitCommandHelpers.GetGlobalSetting("mergetool." + GlobalMergeTool.Text.Trim() + ".path");
            MergeToolCmd.Text = GitCommandHelpers.GetGlobalSetting("mergetool." + GlobalMergeTool.Text.Trim() + ".cmd");

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(MergeToolCmd.Text))
                MergeToolCmd.Enabled = false;
            else
                MergeToolCmd.Enabled = true;

            button1_Click_1(null, null);
        }

        private void ColorAddedLineDiffLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor
                                  };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);
        }

        private void _ColorGraphLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorGraphLabel.BackColor };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorGraphLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorGraphLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorGraphLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);
        }

        private void label28_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorAddedLineLabel.BackColor
                                  };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorAddedLineLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorAddedLineLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorAddedLineLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineLabel.BackColor);
        }

        private void ColorRemovedLineDiffLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor
                                  };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor);
        }

        private void ColorRemovedLine_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorRemovedLine.BackColor };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorRemovedLine.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorRemovedLine.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorRemovedLine.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLine.BackColor);
        }

        private void ColorSectionLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorSectionLabel.BackColor };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorSectionLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorSectionLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorSectionLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorSectionLabel.BackColor);
        }

        private void ColorTagLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorTagLabel.BackColor };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorTagLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorTagLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorTagLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorTagLabel.BackColor);
        }

        private void ColorBranchLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorBranchLabel.BackColor };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorBranchLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorBranchLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorBranchLabel.BackColor);
        }

        private void ColorRemoteBranchLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor
                                  };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorRemoteBranchLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorRemoteBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemoteBranchLabel.BackColor);
        }

        private void ColorOtherLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorOtherLabel.BackColor };
            colorDialog.ShowDialog();
            _NO_TRANSLATE_ColorOtherLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorOtherLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorOtherLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorOtherLabel.BackColor);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(MergeToolCmd.Text))
                MergeToolCmd.Enabled = false;
            else
                MergeToolCmd.Enabled = true;
        }

        private static void ClearImageCache_Click(object sender, EventArgs e)
        {
            GravatarService.ClearImageCache();
        }

        private void DiffToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!Settings.RunningOnWindows())
                return;

            if (GlobalDiffTool.Text.Equals("BeyondCompare3", StringComparison.CurrentCultureIgnoreCase))
            {
                string bcomppath = GitCommandHelpers.GetGlobalSetting("difftool.beyondcompare3.path");

                DifftoolPath.Text = FindFileInFolders("bcomp.exe",
                                                      bcomppath,
                                                      @"C:\Program Files\Beyond Compare 3 (x86)\",
                                                      @"C:\Program Files\Beyond Compare 3\");

                if (File.Exists(DifftoolPath.Text))
                    DifftoolCmd.Text = "\"" + DifftoolPath.Text + "\" \"$LOCAL\" \"$REMOTE\"";
            }

            if (GlobalDiffTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                string kdiff3path = GitCommandHelpers.GetGlobalSetting("difftool.kdiff3.path");
                string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                DifftoolPath.Text = FindFileInFolders("kdiff3.exe", kdiff3path,
                                                      @"c:\Program Files\KDiff3\",
                                                      @"c:\Program Files (x86)\KDiff3\",
                                                      regkdiff3path);
            }
            if (GlobalDiffTool.Text.Equals("winmerge", StringComparison.CurrentCultureIgnoreCase))
            {
                string winmergepath = GitCommandHelpers.GetGlobalSetting("difftool.winmerge.path");

                DifftoolPath.Text = FindFileInFolders("winmergeu.exe", winmergepath,
                                                      @"c:\Program Files\winmerge\",
                                                      @"c:\Program Files (x86)\winmerge\");
            }
            if (File.Exists(DifftoolPath.Text))
                DifftoolCmd.Text = "\"" + DifftoolPath.Text + "\" \"$LOCAL\" \"$REMOTE\"";
        }

        private static void helpTranslate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new FormTranslate().ShowDialog();
        }

        private void otherHomeBrowse_Click(object sender, EventArgs e)
        {
            var browseDialog = new FolderBrowserDialog
                    {
                        SelectedPath = Environment.GetEnvironmentVariable("USERPROFILE")
                    };

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                otherHomeDir.Text = browseDialog.SelectedPath;
            }
        }

        private void otherHome_CheckedChanged(object sender, EventArgs e)
        {
            otherHomeDir.ReadOnly = !otherHome.Checked;
        }

        private void MulticolorBranches_CheckedChanged(object sender, EventArgs e)
        {
            if (MulticolorBranches.Checked)
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = false;
                StripedBanchChange.Enabled = true;
            }
            else
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = true;
                StripedBanchChange.Enabled = false;
            }
        }

        private static bool getCheckAtStartupChecked(bool bValid)
        {
            bool retValue = false;
            if ((Application.UserAppDataRegistry.GetValue("checksettings") == null ||
                 Application.UserAppDataRegistry.GetValue("checksettings").ToString() == "true"))
            {
                retValue = true;
            }

            if (bValid && retValue)
            {
                Application.UserAppDataRegistry.SetValue("checksettings", false);
                retValue = false;
            }
            return retValue;
        }

        private bool CheckTranslationConfigSettings()
        {
            translationConfig.Visible = true;
            if (string.IsNullOrEmpty(Settings.Translation))
            {
                translationConfig.BackColor = Color.LightSalmon;
                translationConfig.Text = "There is no language configured for Git Extensions.";
                return false;
            }
            translationConfig.BackColor = Color.LightGreen;
            translationConfig.Text = "The configured language is " + Settings.Translation + ".";
            return true;
        }

        private bool CheckSSHSettings()
        {
            SshConfig.Visible = true;
            if (GitCommandHelpers.Plink())
            {
                if (!File.Exists(Settings.Plink) || !File.Exists(Settings.Puttygen) || !File.Exists(Settings.Pageant))
                {
                    SshConfig.BackColor = Color.LightSalmon;
                    SshConfig.Text =
                        "PuTTY is configured as SSH client but cannot find plink.exe, puttygen.exe or pageant.exe.";
                    return false;
                }
                SshConfig.BackColor = Color.LightGreen;
                SshConfig.Text = "SSH client PuTTY is configured properly.";
                return true;
            }
            SshConfig.BackColor = Color.LightGreen;
            if (string.IsNullOrEmpty(GitCommandHelpers.GetSsh()))
                SshConfig.Text =
                    "Default SSH client, OpenSSH, will be used. (commandline window will appear on pull, push and clone operations)";
            else
                SshConfig.Text = "Unknown SSH client configured: " + GitCommandHelpers.GetSsh() + ".";
            return true;
        }

        private bool CheckGitExe()
        {
            GitBinFound.Visible = true;
            if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh") &&
                !CheckIfFileIsInPath("sh.exe") && !CheckIfFileIsInPath("sh"))
            {
                GitBinFound.BackColor = Color.LightSalmon;
                GitBinFound.Text =
                    "Linux tools (sh) not found. To solve this problem you can set the correct path in settings.";
                return false;
            }
            GitBinFound.BackColor = Color.LightGreen;
            GitBinFound.Text = "Linux tools (sh) found on your computer.";
            return true;
        }

        private bool CheckGitCmdValid()
        {
            GitFound.Visible = true;
            if (!CanFindGitCmd())
            {
                GitFound.BackColor = Color.LightSalmon;
                GitFound.Text = "Git not found. To solve this problem you can set the correct path in settings.";
                return false;
            }
            GitFound.BackColor = Color.LightGreen;
            GitFound.Text = "Git is found on your computer.";
            return true;
        }

        private bool CheckDiffToolConfiguration()
        {
            DiffTool2.Visible = true;
            if (string.IsNullOrEmpty(GetGlobalDiffToolFromConfig()))
            {
                DiffTool2.BackColor = Color.LightSalmon;
                DiffTool2.Text =
                    "You should configure a diff tool to show file diff in external program (kdiff3 for example).";
                return false;
            }
            if (Settings.RunningOnWindows())
            {
                if (GetGlobalDiffToolFromConfig().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                {
                    string p = GitCommandHelpers.GetGlobalSetting("difftool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        DiffTool2.BackColor = Color.LightSalmon;
                        DiffTool2.Text =
                            "KDiff3 is configured as difftool, but the path to kdiff.exe is not configured.";
                        return false;
                    }
                    DiffTool2.BackColor = Color.LightGreen;
                    DiffTool2.Text = "KDiff3 is configured as difftool.";
                    return true;
                }
            }
            string difftool = GetGlobalDiffToolFromConfig();
            DiffTool2.BackColor = Color.LightGreen;
            DiffTool2.Text = "There is a difftool configured: " + difftool;
            return true;
        }

        private bool CheckMergeTool()
        {
            DiffTool.Visible = true;
            if (string.IsNullOrEmpty(GitCommandHelpers.GetGlobalSetting("merge.tool")))
            {
                DiffTool.BackColor = Color.LightSalmon;
                DiffTool.Text =
                    "You need to configure merge tool in order to solve mergeconflicts (kdiff3 for example).";
                return false;
            }

            if (Settings.RunningOnWindows())
            {
                if (GitCommandHelpers.GetGlobalSetting("merge.tool").Equals("kdiff3",
                                                                            StringComparison.CurrentCultureIgnoreCase))
                {
                    string p = GitCommandHelpers.GetGlobalSetting("mergetool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        DiffTool.BackColor = Color.LightSalmon;
                        DiffTool.Text =
                            "KDiff3 is configured as mergetool, but the path to kdiff.exe is not configured.";
                        return false;
                    }
                    DiffTool.BackColor = Color.LightGreen;
                    DiffTool.Text = "KDiff3 is configured as mergetool.";
                    return true;
                }
                string mergetool = GitCommandHelpers.GetGlobalSetting("merge.tool");
                if (mergetool.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase) ||
                    mergetool.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
                {
                    string p = GitCommandHelpers.GetGlobalSetting("mergetool." + mergetool + ".cmd");
                    if (string.IsNullOrEmpty(p))
                    {
                        DiffTool.BackColor = Color.LightSalmon;
                        DiffTool.Text = mergetool +
                                        " is configured as mergetool, this is a custom mergetool and needs a custom cmd to be configured.";
                        return false;
                    }
                    DiffTool.BackColor = Color.LightGreen;
                    DiffTool.Text = "There is a custom mergetool configured: " + mergetool;
                    return true;
                }
            }
            DiffTool.BackColor = Color.LightGreen;
            DiffTool.Text = "There is a mergetool configured.";
            return true;
        }

        private bool CheckGlobalUserSettingsValid()
        {
            UserNameSet.Visible = true;
            if (string.IsNullOrEmpty(GitCommandHelpers.GetGlobalSetting("user.name")) ||
                string.IsNullOrEmpty(GitCommandHelpers.GetGlobalSetting("user.email")))
            {
                UserNameSet.BackColor = Color.LightSalmon;
                UserNameSet.Text = "You need to configure a username and an email address.";
                return false;
            }
            UserNameSet.BackColor = Color.LightGreen;
            UserNameSet.Text = "A username and an email address are configured.";
            return true;
        }

        private bool CheckGitExtensionRegistrySettings()
        {
            if (!Settings.RunningOnWindows())
                return true;

            ShellExtensionsRegistered.Visible = true;
            if (
                string.IsNullOrEmpty(GetRegistryValue(Registry.LocalMachine,
                                                      "Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved",
                                                      "{3C16B20A-BA16-4156-916F-0A375ECFFE24}")) ||
                string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot,
                                                      "*\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot,
                                                      "Directory\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot,
                                                      "Directory\\Background\\shellex\\ContextMenuHandlers\\GitExtensions2",
                                                      null)))
            {
                ShellExtensionsRegistered.BackColor = Color.LightSalmon;
                ShellExtensionsRegistered.Text =
                    String.Format("{0} needs to be registered in order to use the shell extensions.",
                                  GitExtensionsShellExName);
                return false;
            }
            ShellExtensionsRegistered.BackColor = Color.LightGreen;
            ShellExtensionsRegistered.Text = "Shell extensions registered properly.";
            return true;
        }

        private bool CheckGitExtensionsInstall()
        {
            if (!Settings.RunningOnWindows())
                return true;

            GitExtensionsInstall.Visible = true;
            if (string.IsNullOrEmpty(Settings.GetInstallDir()))
            {
                GitExtensionsInstall.BackColor = Color.LightSalmon;
                GitExtensionsInstall.Text =
                    "Registry entry missing [Software\\GitExtensions\\GitExtensions\\1.0.0.0\\InstallDir].";
                return false;
            }
            if (Settings.GetInstallDir() != null && Settings.GetInstallDir().EndsWith(".exe"))
            {
                GitExtensionsInstall.BackColor = Color.LightSalmon;
                GitExtensionsInstall.Text =
                    "Invalid installation directory stored in [Software\\GitExtensions\\GitExtensions\\1.0.0.0\\InstallDir].";
                return false;
            }
            GitExtensionsInstall.BackColor = Color.LightGreen;
            GitExtensionsInstall.Text = "GitExtensions is properly registered.";
            return true;
        }

        private static IEnumerable<string> GetWindowsCommandLocations()
        {
            if (!string.IsNullOrEmpty(Settings.GitCommand))
                yield return Settings.GitCommand;

            yield return @"C:\cygwin\bin\git.exe";
            yield return @"C:\cygwin\bin\git";
            yield return
                GetRegistryValue(Registry.LocalMachine,
                                 "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") +
                "bin\\git.exe";
            yield return @"c:\Program Files (x86)\Git\bin\git.exe";
            yield return @"c:\Program Files\Git\bin\git.exe";
            yield return @"c:\Program Files (x86)\msysgit\bin\git.exe";
            yield return @"c:\Program Files\msysgit\bin\git.exe";
            yield return
                GetRegistryValue(Registry.LocalMachine,
                                 "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") +
                "cmd\\git.cmd";
            yield return @"c:\Program Files (x86)\Git\cmd\git.cmd";
            yield return @"c:\Program Files\Git\cmd\git.cmd";
            yield return "git";
            yield return "git.cmd";
            yield return @"C:\msysgit\bin\git.exe";
            yield return @"C:\msysgit\cmd\git.cmd";
        }

        private static bool SolveGitCommand()
        {
            if (Settings.RunningOnWindows())
            {
                string command = (from cmd in GetWindowsCommandLocations()
                                  let output = GitCommandHelpers.RunCmd(cmd, string.Empty)
                                  where !string.IsNullOrEmpty(output)
                                  select cmd).FirstOrDefault();

                if (command != null)
                {
                    Settings.GitCommand = command;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Settings.GitCommand = "git";
                if (string.IsNullOrEmpty(GitCommandHelpers.RunCmd(Settings.GitCommand, "")))
                {
                    return false;
                }
            }

            return true;
        }

        private void SaveScripts()
        {
            var scripts_params = new string[ScriptList.Items.Count][];
            for (int i = 0; i < ScriptList.Items.Count; i++)
            {
                ListViewItem item = ScriptList.Items[i];
                var parameters = new string[item.SubItems.Count - 1];
                for (int j = 1; j < item.SubItems.Count; j++)
                    parameters[j - 1] = item.SubItems[j].Text;
                scripts_params[i] = parameters;
            }
            Settings.SaveScripts(scripts_params);
        }

        private void LoadScripts()
        {
            ScriptList.Items.Clear();
            string[][] scripts = Settings.GetScripts();
            foreach (var parameters in scripts)
            {
                ScriptList.Items.Add((ScriptList.Items.Count + 1).ToString());
                foreach (string param in parameters)
                    ScriptList.Items[ScriptList.Items.Count - 1].SubItems.Add(param);
            }
        }

        private void ClearScriptDetails()
        {
            nameTextBox.Clear();
            commandTextBox.Clear();
            argumentsTextBox.Clear();
            inMenuCheckBox.Checked = false;
        }

        private void DisableScriptDetails()
        {
            nameTextBox.Enabled = false;
            commandTextBox.Enabled = false;
            argumentsTextBox.Enabled = false;
            inMenuCheckBox.Enabled = false;
            saveScriptButton.Enabled = false;
            cancelScriptButton.Enabled = false;
            browseScriptButton.Enabled = false;
        }

        private void EnableScriptDetails()
        {
            nameTextBox.Enabled = true;
            commandTextBox.Enabled = true;
            argumentsTextBox.Enabled = true;
            inMenuCheckBox.Enabled = true;
            saveScriptButton.Enabled = true;
            cancelScriptButton.Enabled = true;
            browseScriptButton.Enabled = true;
        }

        private void RefreshScriptDetails()
        {
            nameTextBox.Text = ScriptList.SelectedItems[0].SubItems[1].Text;
            commandTextBox.Text = ScriptList.SelectedItems[0].SubItems[2].Text;
            argumentsTextBox.Text = ScriptList.SelectedItems[0].SubItems[3].Text;
            if (ScriptList.Items[ScriptList.SelectedIndices[0]].SubItems[4].Text.Equals("yes"))
                inMenuCheckBox.Checked = true;
        }

        private void addScriptButton_Click(object sender, EventArgs e)
        {
            ScriptList.SelectedItems.Clear();
            ListViewItem lv = ScriptList.Items.Add((ScriptList.Items.Count + 1).ToString());
            for (int i = 0; i < 4; i++)
                ScriptList.Items[ScriptList.Items.Count - 1].SubItems.Add(string.Empty);
            lv.Selected = true;
            editScriptButton_Click(sender, e);
        }

        private void removeScriptButton_Click(object sender, EventArgs e)
        {
            if (ScriptList.SelectedItems.Count > 0)
            {
                ScriptList.Items.RemoveAt(ScriptList.SelectedItems[0].Index);
                ClearScriptDetails();
                DisableScriptDetails();
                SaveScripts();
            }
        }

        private void editScriptButton_Click(object sender, EventArgs e)
        {
            if (ScriptList.SelectedItems.Count > 0)
            {
                selectedScriptItem = ScriptList.SelectedIndices[0];
                RefreshScriptDetails();
                EnableScriptDetails();
                nameTextBox.Focus();
            }
        }

        private void saveScriptButton_Click(object sender, EventArgs e)
        {
            ScriptList.Items[selectedScriptItem].SubItems[1].Text = nameTextBox.Text;
            ScriptList.Items[selectedScriptItem].SubItems[2].Text = commandTextBox.Text;
            ScriptList.Items[selectedScriptItem].SubItems[3].Text = argumentsTextBox.Text;
            ScriptList.Items[selectedScriptItem].SubItems[4].Text = inMenuCheckBox.Checked ? "yes" : "no";
            DisableScriptDetails();
            ScriptList.Focus();
            SaveScripts();
        }

        private void ScriptList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (ScriptList.SelectedIndices.Count > 0)
            {
                RefreshScriptDetails();
                selectedScriptItem = ScriptList.SelectedIndices[0];
                editScriptButton.Enabled = true;
                removeScriptButton.Enabled = true;
                moveDownButton.Enabled = moveUpButton.Enabled = false;
                if (ScriptList.SelectedIndices[0] > 0)
                    moveUpButton.Enabled = true;
                if (ScriptList.SelectedIndices[0] < ScriptList.Items.Count - 1)
                    moveDownButton.Enabled = true;
            }
            else
            {
                editScriptButton.Enabled = false;
                removeScriptButton.Enabled = false;
                moveUpButton.Enabled = false;
                moveDownButton.Enabled = false;
                ClearScriptDetails();
            }
        }

        private void cancelScriptButton_Click(object sender, EventArgs e)
        {
            DisableScriptDetails();
            ClearScriptDetails();
            ScriptList.Focus();
            ScriptList_ItemSelectionChanged(sender, null);
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            int no;
            var lv = new ListView();
            foreach (ListViewItem lvi in ScriptList.Items)
                lv.Items.Add((ListViewItem)lvi.Clone());
            ScriptList.Items.Clear();
            for (int i = 0; i < selectedScriptItem - 1; i++)
                ScriptList.Items.Add((ListViewItem)lv.Items[i].Clone());
            var currentItem = (ListViewItem)lv.Items[selectedScriptItem].Clone();
            var aboveItem = (ListViewItem)lv.Items[selectedScriptItem - 1].Clone();
            int.TryParse(currentItem.SubItems[0].Text, out no);
            currentItem.SubItems[0].Text = (no - 1).ToString();
            aboveItem.SubItems[0].Text = (no).ToString();
            ScriptList.Items.Add(currentItem);
            ScriptList.Items.Add(aboveItem);
            for (int i = selectedScriptItem + 1; i < lv.Items.Count; i++)
                ScriptList.Items.Add((ListViewItem)lv.Items[i].Clone());
            SaveScripts();
            selectedScriptItem--;
            ScriptList.Items[selectedScriptItem].Selected = true;
            ScriptList.Focus();
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            int no;
            var lv = new ListView();
            foreach (ListViewItem lvi in ScriptList.Items)
                lv.Items.Add((ListViewItem)lvi.Clone());
            ScriptList.Items.Clear();
            for (int i = 0; i < selectedScriptItem; i++)
                ScriptList.Items.Add((ListViewItem)lv.Items[i].Clone());
            var currentItem = (ListViewItem)lv.Items[selectedScriptItem].Clone();
            var underItem = (ListViewItem)lv.Items[selectedScriptItem + 1].Clone();
            int.TryParse(currentItem.SubItems[0].Text, out no);
            currentItem.SubItems[0].Text = (no + 1).ToString();
            underItem.SubItems[0].Text = (no).ToString();
            ScriptList.Items.Add(underItem);
            ScriptList.Items.Add(currentItem);
            for (int i = selectedScriptItem + 2; i < lv.Items.Count; i++)
                ScriptList.Items.Add((ListViewItem)lv.Items[i].Clone());
            SaveScripts();
            selectedScriptItem++;
            ScriptList.Items[selectedScriptItem].Selected = true;
            ScriptList.Focus();
        }

        private void browseScriptButton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              InitialDirectory = "c:\\",
                              Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                              RestoreDirectory = true
                          };
            if (ofd.ShowDialog() == DialogResult.OK)
                commandTextBox.Text = ofd.FileName;
        }

        private void argumentsTextBox_Enter(object sender, EventArgs e)
        {
            helpLabel.Visible = true;
        }

        private void argumentsTextBox_Leave(object sender, EventArgs e)
        {
            helpLabel.Visible = false;
        }

        private void translationConfig_Click(object sender, EventArgs e)
        {
            new FormChooseTranslation().ShowDialog();
            Translate();
            Language.Text = Settings.Translation;
            Rescan_Click(null, null);
        }

        private static void downloadDictionary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://code.google.com/p/gitextensions/wiki/Spelling");
        }
    }
}