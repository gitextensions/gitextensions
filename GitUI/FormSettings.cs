using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using GitCommands;

namespace GitUI
{
    public partial class FormSettings : GitExtensionsForm
    {
        public FormSettings()
        {
            InitializeComponent();


        }

        public static bool AutoSolveAllSettings()
        {
            return SolveGitCmdDir() &&
                    SolveGitBinDir() &&
                    SolveKDiff() &&
                    SolveKDiffTool2() &&
                    SolveGitExtensionsDir();
        }

        private void SetCheckboxFromString(CheckBox checkBox, string str)
        {
            str = str.Trim().ToLower();

            if (str == "true")
                checkBox.CheckState = CheckState.Checked;
            else
                if (str == "false")
                    checkBox.CheckState = CheckState.Unchecked;
                else
                    checkBox.CheckState = CheckState.Indeterminate;
        }

        private static void SetComboBoxFromString(ComboBox comboBox, string option)
        {
            option = option.Trim().ToLower();

            switch (option)
            {
                case "true":
                    comboBox.SelectedItem = option;
                    break;

                case "false":
                    comboBox.SelectedItem = option;
                    break;

                case "input":
                    comboBox.SelectedItem = option;
                    break;

                default:
                    comboBox.SelectedItem = "";
                    break;
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (GitCommands.Settings.Encoding.GetType() == typeof(ASCIIEncoding))
                    Encoding.Text = "ASCII";
                else
                    if (GitCommands.Settings.Encoding.GetType() == typeof(UnicodeEncoding))
                        Encoding.Text = "Unicode";
                    else
                        if (GitCommands.Settings.Encoding.GetType() == typeof(UTF7Encoding))
                            Encoding.Text = "UTF7";
                        else
                            if (GitCommands.Settings.Encoding.GetType() == typeof(UTF8Encoding))
                                Encoding.Text = "UTF8";
                            else
                                if (GitCommands.Settings.Encoding.GetType() == typeof(UTF32Encoding))
                                    Encoding.Text = "UTF32";
                                else
                                    if (GitCommands.Settings.Encoding == System.Text.Encoding.Default)
                                        Encoding.Text = "Default";

                FollowRenamesInFileHistory.Checked = Settings.FollowRenamesInFileHistory;

                DaysToCacheImages.Value = GitCommands.Settings.AuthorImageCacheDays;

                authorImageSize.Value = Settings.AuthorImageSize;
                ShowAuthorGravatar.Checked = Settings.ShowAuthorGravatar;

                RevisionGraphColorLabel.BackColor = Settings.RevisionGraphColor;
                RevisionGraphColorLabel.Text = Settings.RevisionGraphColor.Name;
                RevisionGraphColorLabel.ForeColor = ColorHelper.GetForeColorForBackColor(RevisionGraphColorLabel.BackColor);
                RevisionGraphColorSelected.BackColor = Settings.RevisionGraphColorSelected;
                RevisionGraphColorSelected.Text = Settings.RevisionGraphColorSelected.Name;
                RevisionGraphColorSelected.ForeColor = ColorHelper.GetForeColorForBackColor(RevisionGraphColorSelected.BackColor);
                ColorTagLabel.BackColor = Settings.TagColor;
                ColorTagLabel.Text = Settings.TagColor.Name;
                ColorTagLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorTagLabel.BackColor);
                ColorBranchLabel.BackColor = Settings.BranchColor;
                ColorBranchLabel.Text = Settings.BranchColor.Name;
                ColorBranchLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorBranchLabel.BackColor);
                ColorRemoteBranchLabel.BackColor = Settings.RemoteBranchColor;
                ColorRemoteBranchLabel.Text = Settings.RemoteBranchColor.Name;
                ColorRemoteBranchLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorRemoteBranchLabel.BackColor);
                ColorOtherLabel.BackColor = Settings.OtherTagColor;
                ColorOtherLabel.Text = Settings.OtherTagColor.Name;
                ColorOtherLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorOtherLabel.BackColor);


                ColorAddedLineLabel.BackColor = Settings.DiffAddedColor;
                ColorAddedLineLabel.Text = Settings.DiffAddedColor.Name;
                ColorAddedLineLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorAddedLineLabel.BackColor);
                ColorAddedLineDiffLabel.BackColor = Settings.DiffAddedExtraColor;
                ColorAddedLineDiffLabel.Text = Settings.DiffAddedExtraColor.Name;
                ColorAddedLineDiffLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorAddedLineDiffLabel.BackColor);

                ColorRemovedLine.BackColor = Settings.DiffRemovedColor;
                ColorRemovedLine.Text = Settings.DiffRemovedColor.Name;
                ColorRemovedLine.ForeColor = ColorHelper.GetForeColorForBackColor(ColorRemovedLine.BackColor);
                ColorRemovedLineDiffLabel.BackColor = Settings.DiffRemovedExtraColor;
                ColorRemovedLineDiffLabel.Text = Settings.DiffRemovedExtraColor.Name;
                ColorRemovedLineDiffLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorRemovedLineDiffLabel.BackColor);
                ColorSectionLabel.BackColor = Settings.DiffSectionColor;
                ColorSectionLabel.Text = Settings.DiffSectionColor.Name;
                ColorSectionLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorSectionLabel.BackColor);

                SmtpServer.Text = GitCommands.Settings.Smtp;

                MaxCommits.Value = GitCommands.Settings.MaxCommits;

                GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

                GitPath.Text = GitCommands.Settings.GitCommand;
                GitBinPath.Text = GitCommands.Settings.GitBinDir;

                ConfigFile localConfig = GitCommands.GitCommands.GetLocalConfig();
                ConfigFile globalConfig = GitCommands.GitCommands.GetGlobalConfig();

                UserName.Text = localConfig.GetValue("user.name");
                UserEmail.Text = localConfig.GetValue("user.email");
                Editor.Text = localConfig.GetValue("core.editor");
                MergeTool.Text = localConfig.GetValue("merge.tool");

                Dictionary.Text = GitCommands.Settings.Dictionary;

                GlobalUserName.Text = globalConfig.GetValue("user.name");
                GlobalUserEmail.Text = globalConfig.GetValue("user.email");
                GlobalEditor.Text = globalConfig.GetValue("core.editor");
                GlobalMergeTool.Text = globalConfig.GetValue("merge.tool");

                SetCheckboxFromString(KeepMergeBackup, localConfig.GetValue("mergetool.keepBackup"));
                SetComboBoxFromString(LocalAutoCRLF, localConfig.GetValue("core.autocrlf"));

                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    MergetoolPath.Text = globalConfig.GetValue("mergetool." + GlobalMergeTool.Text + ".path");
                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    MergeToolCmd.Text = globalConfig.GetValue("mergetool." + GlobalMergeTool.Text + ".cmd");

                DefaultIcon.Checked = GitCommands.Settings.IconColor.Equals("default" , StringComparison.CurrentCultureIgnoreCase);
                BlueIcon.Checked = GitCommands.Settings.IconColor.Equals("blue", StringComparison.CurrentCultureIgnoreCase);
                GreenIcon.Checked = GitCommands.Settings.IconColor.Equals("green", StringComparison.CurrentCultureIgnoreCase);
                PurpleIcon.Checked = GitCommands.Settings.IconColor.Equals("purple", StringComparison.CurrentCultureIgnoreCase);
                RedIcon.Checked = GitCommands.Settings.IconColor.Equals("red", StringComparison.CurrentCultureIgnoreCase);
                YellowIcon.Checked = GitCommands.Settings.IconColor.Equals("yellow", StringComparison.CurrentCultureIgnoreCase);
                RandomIcon.Checked = GitCommands.Settings.IconColor.Equals("random", StringComparison.CurrentCultureIgnoreCase);

                if (GitCommands.GitCommands.VersionInUse.GuiDiffToolExist)
                    GlobalDiffTool.Text = globalConfig.GetValue("diff.guitool");
                else
                    GlobalDiffTool.Text = globalConfig.GetValue("diff.tool");

                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    DifftoolPath.Text = globalConfig.GetValue("difftool." + GlobalDiffTool.Text + ".path");
                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    DifftoolCmd.Text = globalConfig.GetValue("difftool." + GlobalDiffTool.Text + ".cmd");

                SetCheckboxFromString(GlobalKeepMergeBackup, globalConfig.GetValue("mergetool.keepBackup"));
                SetComboBoxFromString(GlobalAutoCRLF, globalConfig.GetValue("core.autocrlf"));

                PlinkPath.Text = GitCommands.Settings.Plink;
                PuttygenPath.Text = GitCommands.Settings.Puttygen;
                PageantPath.Text = GitCommands.Settings.Pageant;
                AutostartPageant.Checked = GitCommands.Settings.AutoStartPageant;

                CloseProcessDialog.Checked = GitCommands.Settings.CloseProcessDialog;
                ShowRevisionGraph.Checked = GitCommands.Settings.ShowRevisionGraph;
                ShowGitCommandLine.Checked = GitCommands.Settings.ShowGitCommandLine;

                UseFastChecks.Checked = GitCommands.Settings.UseFastChecks;
                ShowRelativeDate.Checked = GitCommands.Settings.RelativeDate;

                if (string.IsNullOrEmpty(GitCommands.GitCommands.GetSsh()))
                    OpenSSH.Checked = true;
                else
                    if (GitCommands.GitCommands.Plink())
                        Putty.Checked = true;
                    else
                    {
                        OtherSsh.Text = GitCommands.GitCommands.GetSsh();
                        Other.Checked = true;
                    }

                EnableSshOptions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load settigns.\n\n" + ex.Message);
            }
        }

        private void UserName_TextChanged(object sender, EventArgs e)
        {
        }

        private void UserEmail_TextChanged(object sender, EventArgs e)
        {
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool Save()
        {
            GitCommands.Settings.FollowRenamesInFileHistory = FollowRenamesInFileHistory.Checked;

            if ((int)authorImageSize.Value != GitCommands.Settings.AuthorImageSize)
            {
                GitCommands.Settings.AuthorImageSize = (int)authorImageSize.Value;
                Gravatar.ClearImageCache();
            }

            GitCommands.Settings.AuthorImageCacheDays = (int)DaysToCacheImages.Value;

            GitCommands.Settings.Smtp = SmtpServer.Text;

            GitCommands.Settings.GitCommand = GitPath.Text;
            GitCommands.Settings.GitBinDir = GitBinPath.Text;

            GitCommands.Settings.ShowAuthorGravatar = ShowAuthorGravatar.Checked;

            GitCommands.Settings.CloseProcessDialog = CloseProcessDialog.Checked;
            GitCommands.Settings.ShowRevisionGraph = ShowRevisionGraph.Checked;
            GitCommands.Settings.ShowGitCommandLine = ShowGitCommandLine.Checked;

            GitCommands.Settings.UseFastChecks = UseFastChecks.Checked;
            GitCommands.Settings.RelativeDate = ShowRelativeDate.Checked;

            GitCommands.Settings.Dictionary = Dictionary.Text;

            GitCommands.Settings.MaxCommits = (int)MaxCommits.Value;

            GitCommands.Settings.Plink = PlinkPath.Text;
            GitCommands.Settings.Puttygen = PuttygenPath.Text;
            GitCommands.Settings.Pageant = PageantPath.Text;
            GitCommands.Settings.AutoStartPageant = AutostartPageant.Checked;

            if (string.IsNullOrEmpty(Encoding.Text) || Encoding.Text.Equals("Default", StringComparison.CurrentCultureIgnoreCase))
                GitCommands.Settings.Encoding = System.Text.Encoding.Default;
            else
                if (Encoding.Text.Equals("ASCII", StringComparison.CurrentCultureIgnoreCase))
                    GitCommands.Settings.Encoding = new ASCIIEncoding();
                else
                    if (Encoding.Text.Equals("Unicode", StringComparison.CurrentCultureIgnoreCase))
                        GitCommands.Settings.Encoding = new UnicodeEncoding();
                    else
                        if (Encoding.Text.Equals("UTF7", StringComparison.CurrentCultureIgnoreCase))
                            GitCommands.Settings.Encoding = new UTF7Encoding();
                        else
                            if (Encoding.Text.Equals("UTF8", StringComparison.CurrentCultureIgnoreCase))
                                GitCommands.Settings.Encoding = new UTF8Encoding(false);
                            else
                                if (Encoding.Text.Equals("UTF32", StringComparison.CurrentCultureIgnoreCase))
                                    GitCommands.Settings.Encoding = new UTF32Encoding(true, false);
                                else
                                    GitCommands.Settings.Encoding = System.Text.Encoding.Default;

            Settings.RevisionGraphColor = RevisionGraphColorLabel.BackColor;
            Settings.RevisionGraphColorSelected = RevisionGraphColorSelected.BackColor;
            Settings.TagColor = ColorTagLabel.BackColor;
            Settings.BranchColor = ColorBranchLabel.BackColor;
            Settings.RemoteBranchColor = ColorRemoteBranchLabel.BackColor;
            Settings.OtherTagColor = ColorOtherLabel.BackColor;

            Settings.DiffAddedColor = ColorAddedLineLabel.BackColor;
            Settings.DiffRemovedColor = ColorRemovedLine.BackColor;
            Settings.DiffAddedExtraColor = ColorAddedLineDiffLabel.BackColor;
            Settings.DiffRemovedExtraColor = ColorRemovedLineDiffLabel.BackColor;

            Settings.DiffSectionColor = ColorSectionLabel.BackColor;

            if (DefaultIcon.Checked)
                GitCommands.Settings.IconColor = "default";
            if (BlueIcon.Checked)
                GitCommands.Settings.IconColor = "blue";
            if (GreenIcon.Checked)
                GitCommands.Settings.IconColor = "green";
            if (PurpleIcon.Checked)
                GitCommands.Settings.IconColor = "purple";
            if (RedIcon.Checked)
                GitCommands.Settings.IconColor = "red";
            if (YellowIcon.Checked)
                GitCommands.Settings.IconColor = "yellow";
            if (RandomIcon.Checked)
                GitCommands.Settings.IconColor = "random";

            EnableSettings();

            if (!CanFindGitCmd())
            {
                if (MessageBox.Show("The path to git.cmd is not configured correct." + Environment.NewLine + "You need to set the correct path to be able to use GitExtensions." + Environment.NewLine + Environment.NewLine + "Do you want to set the correct path now?", "Incorrect path", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;
            }
            else
            {
                ConfigFile localConfig = GitCommands.GitCommands.GetLocalConfig();
                ConfigFile globalConfig = GitCommands.GitCommands.GetGlobalConfig();

                GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

                if (string.IsNullOrEmpty(UserName.Text) || !UserName.Text.Equals(localConfig.GetValue("user.name")))
                    localConfig.SetValue("user.name", UserName.Text);
                if (string.IsNullOrEmpty(UserEmail.Text) || !UserEmail.Text.Equals(localConfig.GetValue("user.email")))
                    localConfig.SetValue("user.email", UserEmail.Text);
                localConfig.SetValue("core.editor", Editor.Text);
                localConfig.SetValue("merge.tool", MergeTool.Text);


                if (KeepMergeBackup.CheckState == CheckState.Checked)
                    localConfig.SetValue("mergetool.keepBackup", "true");
                else
                    if (KeepMergeBackup.CheckState == CheckState.Unchecked)
                        localConfig.SetValue("mergetool.keepBackup", "false");

                localConfig.SetValue("core.autocrlf", LocalAutoCRLF.SelectedItem as string);

                if (string.IsNullOrEmpty(GlobalUserName.Text) || !GlobalUserName.Text.Equals(globalConfig.GetValue("user.name")))
                    globalConfig.SetValue("user.name", GlobalUserName.Text);
                if (string.IsNullOrEmpty(GlobalUserEmail.Text) || !GlobalUserEmail.Text.Equals(globalConfig.GetValue("user.email")))
                    globalConfig.SetValue("user.email", GlobalUserEmail.Text);
                globalConfig.SetValue("core.editor", GlobalEditor.Text);

                if (GitCommands.GitCommands.VersionInUse.GuiDiffToolExist)
                    globalConfig.SetValue("diff.guitool", GlobalDiffTool.Text);
                else
                    globalConfig.SetValue("diff.tool", GlobalDiffTool.Text);

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
                else
                    if (GlobalKeepMergeBackup.CheckState == CheckState.Unchecked)
                        globalConfig.SetValue("mergetool.keepBackup", "false");

                globalConfig.SetValue("core.autocrlf", GlobalAutoCRLF.SelectedItem as string);
                globalConfig.Save();
                localConfig.Save();
            }

            if (OpenSSH.Checked)
                GitCommands.GitCommands.UnSetSsh();

            if (Putty.Checked)
                GitCommands.GitCommands.SetSsh(PlinkPath.Text);

            if (Other.Checked)
                GitCommands.GitCommands.SetSsh(OtherSsh.Text);

            GitCommands.Settings.SaveSettings();
            
            return true;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        protected static string GetRegistryValue(RegistryKey root, string subkey, string key)
        {
            try
            {
                RegistryKey rk;
                rk = root.OpenSubKey(subkey, false);

                string value = "";

                if (rk != null && rk.GetValue(key) is string)
                {
                    value = rk.GetValue(key).ToString();
                    rk.Flush();
                    rk.Close();
                }

                return value;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("GitExtensions has insufficient permisions to check the registry.");
            }
            return "";
        }


        protected void SetRegistryValue(RegistryKey root, string subkey, string key, string value)
        {
            try
            {
                string reg;
                value = value.Replace("\\", "\\\\");
                reg = "Windows Registry Editor Version 5.00" + Environment.NewLine + Environment.NewLine + "[" + root.ToString() + "\\" + subkey + "]" + Environment.NewLine + "\"" + key + "\"=\"" + value + "\"";

                TextWriter tw = new StreamWriter(System.IO.Path.GetTempPath() + "GitExtensions.reg", false);
                tw.Write(reg);
                tw.Close();
                GitCommands.GitCommands.RunCmd("regedit", "\"" + System.IO.Path.GetTempPath() + "GitExtensions.reg" + "\"");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("GitExtensions has insufficient permisions to modify the registry." + Environment.NewLine + "Please add this key to the registry manually." + Environment.NewLine + "Path:   " + root.ToString() + "\\" + subkey + Environment.NewLine + "Value:  " + key + " = " + value);
            }
        }

        public bool CheckSettings()
        {
            bool bValid = true;
            try
            {
                if (string.IsNullOrEmpty(GitCommands.Settings.GetInstallDir()))
                {
                    GitExtensionsInstall.BackColor = Color.LightSalmon;
                    GitExtensionsInstall.Text = "Registry entry missing [Software\\GitExtensions\\GitExtensions\\1.0.0.0\\InstallDir].";
                    bValid = false;
                }
                else
                {
                    GitExtensionsInstall.BackColor = Color.LightGreen;
                    GitExtensionsInstall.Text = "GitExtensions is properly registered.";
                }

                if (string.IsNullOrEmpty(GetRegistryValue(Registry.LocalMachine, "Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", "{3C16B20A-BA16-4156-916F-0A375ECFFE24}")) ||
                    string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot, "*\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                    string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot, "Directory\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                    string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot, "Directory\\Background\\shellex\\ContextMenuHandlers\\GitExtensions2", null)))
                {
                    ShellExtensionsRegistered.BackColor = Color.LightSalmon;
                    ShellExtensionsRegistered.Text = "GitExtensionsShellEx.dll needs to be registered in order to use the shell extensions.";
                    bValid = false;
                }
                else
                {
                    ShellExtensionsRegistered.BackColor = Color.LightGreen;
                    ShellExtensionsRegistered.Text = "Shell extensions registered properly.";
                }

                GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();
                if (string.IsNullOrEmpty(gitCommands.GetGlobalSetting("user.name")) ||
                    string.IsNullOrEmpty(gitCommands.GetGlobalSetting("user.email")))
                {
                    UserNameSet.BackColor = Color.LightSalmon;
                    UserNameSet.Text = "You need to configure a user name and an email address.";
                    bValid = false;
                }
                else
                {
                    UserNameSet.BackColor = Color.LightGreen;
                    UserNameSet.Text = "There is a user name and an email address configured.";
                }

                if (string.IsNullOrEmpty(gitCommands.GetGlobalSetting("merge.tool")))
                {
                    DiffTool.BackColor = Color.LightSalmon;
                    DiffTool.Text = "You need to configure merge tool in order to solve mergeconflicts (kdiff3 for example).";
                    bValid = false;
                }
                else
                {
                    if (gitCommands.GetGlobalSetting("merge.tool").Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string p = gitCommands.GetGlobalSetting("mergetool.kdiff3.path");
                        if (string.IsNullOrEmpty(p) || !File.Exists(p))
                        {
                            DiffTool.BackColor = Color.LightSalmon;
                            DiffTool.Text = "KDiff3 is configured as mergetool, but the path to kdiff.exe is not configured.";
                            bValid = false;
                        }
                        else
                        {
                            DiffTool.BackColor = Color.LightGreen;
                            DiffTool.Text = "KDiff3 is configured as mergetool.";
                        }
                    }
                    else
                    {
                        string mergetool = gitCommands.GetGlobalSetting("merge.tool");
                        if (mergetool.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase) ||
                            mergetool.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string p = gitCommands.GetGlobalSetting("mergetool." + mergetool + ".cmd");
                            if (string.IsNullOrEmpty(p))
                            {
                                DiffTool.BackColor = Color.LightSalmon;
                                DiffTool.Text = mergetool + " is configured as mergetool, this is a custom mergetool and needs a custom cmd to be configured.";
                                bValid = false;
                            }
                            else
                            {
                                DiffTool.BackColor = Color.LightGreen;
                                DiffTool.Text = "There is a custom mergetool configured: " + mergetool;
                            }
                        }
                        else
                        {
                            DiffTool.BackColor = Color.LightGreen;
                            DiffTool.Text = "There is a mergetool configured.";
                        }
                    }
                }

                if (string.IsNullOrEmpty(gitCommands.GetGlobalSetting("diff.tool")))
                {
                    DiffTool2.BackColor = Color.LightSalmon;
                    DiffTool2.Text = "You should configure a diff tool to show file diff in external program (kdiff3 for example).";
                    bValid = false;
                }
                else
                {
                    if (gitCommands.GetGlobalSetting("diff.tool").Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string p = gitCommands.GetGlobalSetting("difftool.kdiff3.path");
                        if (string.IsNullOrEmpty(p) || !File.Exists(p))
                        {
                            DiffTool2.BackColor = Color.LightSalmon;
                            DiffTool2.Text = "KDiff3 is configured as difftool, but the path to kdiff.exe is not configured.";
                            bValid = false;
                        }
                        else
                        {
                            DiffTool2.BackColor = Color.LightGreen;
                            DiffTool2.Text = "KDiff3 is configured as difftool.";
                        }
                    }
                    else
                    {
                        string difftool = gitCommands.GetGlobalSetting("diff.tool");
                        DiffTool2.BackColor = Color.LightGreen;
                        DiffTool2.Text = "There is a difftool configured: " + difftool;
                    }
                }

                if (!CanFindGitCmd())
                {
                    GitFound.BackColor = Color.LightSalmon;
                    GitFound.Text = "git.cmd not found. To solve this problem you can set the correct path in settings.";
                    bValid = false;
                }
                else
                {
                    GitFound.BackColor = Color.LightGreen;
                    GitFound.Text = "git.cmd is found on your computer.";
                }

                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "")))
                {
                    GitBinFound.BackColor = Color.LightSalmon;
                    GitBinFound.Text = "git.exe not found. To solve this problem you can set the correct path in settings.";
                    bValid = false;
                }
                else
                {
                    GitBinFound.BackColor = Color.LightGreen;
                    GitBinFound.Text = "git.exe is found on your computer.";
                }
                if (GitCommands.GitCommands.Plink())
                {
                    if (!File.Exists(GitCommands.Settings.Plink) || !File.Exists(GitCommands.Settings.Puttygen) || !File.Exists(GitCommands.Settings.Pageant))
                    {
                        SshConfig.BackColor = Color.LightSalmon;
                        SshConfig.Text = "PuTTY is configured as SSH client but cannot find plink.exe, puttygen.exe or pageant.exe.";
                        bValid = false;
                    }
                    else
                    {
                        SshConfig.BackColor = Color.LightGreen;
                        SshConfig.Text = "SSH client PuTTY is configured properly";
                    }
                }
                else
                {
                    SshConfig.BackColor = Color.LightGreen;
                    if (string.IsNullOrEmpty(GitCommands.GitCommands.GetSsh()))
                        SshConfig.Text = "Default SSH client, OpenSSH, will be used. (commandline window will appear on pull, push and clone operations)";
                    else
                        SshConfig.Text = "Unknown SSH client configured: " + GitCommands.GitCommands.GetSsh();
                }

            }
            catch
            {
            }

            if ((Application.UserAppDataRegistry.GetValue("checksettings") == null ||
                  Application.UserAppDataRegistry.GetValue("checksettings").ToString() == "true"))
            {
                CheckAtStartup.Checked = true;
            }
            else
            {
                CheckAtStartup.Checked = false;
            }


            if (bValid && CheckAtStartup.Checked)
            {
                Application.UserAppDataRegistry.SetValue("checksettings", false);
                CheckAtStartup.Checked = false;
            }

            return bValid;
        }

        private static bool CanFindGitCmd()
        {
            return !string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand, ""));
        }

        private void GitExtensionsInstall_Click(object sender, EventArgs e)
        {
            SolveGitExtensionsDir();

            CheckSettings();
        }

        public static bool SolveGitExtensionsDir()
        {
            string fileName = Assembly.GetAssembly(typeof(FormSettings)).Location;
            fileName = fileName.Substring(0, fileName.LastIndexOfAny(new char[] { '\\', '/' }));

            if (File.Exists(fileName + "\\GitExtensions.exe"))
            {
                GitCommands.Settings.SetInstallDir(fileName);
                return true;
            }

            return false;
        }

        private void ShellExtensionsRegistered_Click(object sender, EventArgs e)
        {

            if (File.Exists(GitCommands.Settings.GetInstallDir() + "\\GitExtensionsShellEx.dll"))
                GitCommands.GitCommands.RunCmd("regsvr32", "\"" + GitCommands.Settings.GetInstallDir() + "\\GitExtensionsShellEx.dll\"");
            else
            {
                string fileName = Assembly.GetAssembly(GetType()).Location;
                fileName = fileName.Substring(0, fileName.LastIndexOfAny(new char[] { '\\', '/' })) + "\\GitExtensionsShellEx.dll";

                if (File.Exists(fileName))
                    GitCommands.GitCommands.RunCmd("regsvr32", "\"" + fileName + "\"");
            }

            CheckSettings();
        }

        private void UserNameSet_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("GlobalSettingsPage");
        }

        private void DiffTool2_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (string.IsNullOrEmpty(gitCommands.GetGlobalSetting("diff.tool")))
            {
                if (MessageBox.Show("There is no difftool configured. Do you want to configure kdiff3 as your difftool?" + Environment.NewLine + "Select no if you want to configure a different difftool yourself.", "Mergetool", MessageBoxButtons.YesNo) == DialogResult.Yes)
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

            if (gitCommands.GetGlobalSetting("diff.tool").Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                SolveKDiffTool2Path(gitCommands);
            }

            if (gitCommands.GetGlobalSetting("diff.tool").Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(gitCommands.GetGlobalSetting("difftool.kdiff3.path")))
            {
                MessageBox.Show("Path to kdiff3 could not be found automatically." + Environment.NewLine + "Please make sure KDiff3 is installed or set path manually.");
                tabControl1.SelectTab("GlobalSettingsPage");
                return;
            }

            Rescan_Click(null, null);
        }

        private void DiffTool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (string.IsNullOrEmpty(gitCommands.GetGlobalSetting("merge.tool")))
            {
                if (MessageBox.Show("There is no mergetool configured. Do you want to configure kdiff3 as your mergetool?" + Environment.NewLine + "Select no if you want to configure a different mergetool yourself.", "Mergetool", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveKDiff();
                    GlobalDiffTool.Text = "kdiff3";
                }
                else
                {
                    tabControl1.SelectTab("GlobalSettingsPage");
                    return;
                }
            }

            if (gitCommands.GetGlobalSetting("merge.tool").Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                SolveKDiffPath(gitCommands);
            }
            else
                if (gitCommands.GetGlobalSetting("merge.tool").Equals("p4merge", StringComparison.CurrentCultureIgnoreCase) ||
                    gitCommands.GetGlobalSetting("merge.tool").Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
                {
                    AutoConfigMergeToolcmd();
                    gitCommands.SetGlobalSetting("mergetool." + gitCommands.GetGlobalSetting("merge.tool") + ".cmd", MergeToolCmd.Text);
                }


            if (gitCommands.GetGlobalSetting("merge.tool").Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(gitCommands.GetGlobalSetting("mergetool.kdiff3.path")))
            {
                MessageBox.Show("Path to kdiff3 could not be found automatically." + Environment.NewLine + "Please make sure KDiff3 is installed or set path manually.");
                tabControl1.SelectTab("GlobalSettingsPage");
                return;
            }

            Rescan_Click(null, null);
        }

        public static bool SolveKDiff()
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();
            string mergeTool = gitCommands.GetGlobalSetting("merge.tool");
            if (string.IsNullOrEmpty(mergeTool))
            {
                mergeTool = "kdiff3";
                gitCommands.SetGlobalSetting("merge.tool", mergeTool);
            }

            if (mergeTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveKDiffPath(gitCommands);

            return true;
        }

        public static bool SolveKDiffTool2()
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();
            string diffTool = gitCommands.GetGlobalSetting("diff.tool");
            if (string.IsNullOrEmpty(diffTool))
            {
                diffTool = "kdiff3";
                gitCommands.SetGlobalSetting("diff.tool", diffTool);
            }

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveKDiffTool2Path(gitCommands);

            return true;
        }

        public static bool SolveKDiffPath(GitCommands.GitCommands gitCommands)
        {
            string kdiff3path = gitCommands.GetGlobalSetting("mergetool.kdiff3.path");
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
                            kdiff3path = "";
                            return false;
                        }
                    }
                }
            }
            gitCommands.SetGlobalSetting("mergetool.kdiff3.path", kdiff3path);

            return true;
        }

        public static bool SolveKDiffTool2Path(GitCommands.GitCommands gitCommands)
        {
            string kdiff3path = gitCommands.GetGlobalSetting("difftool.kdiff3.path");
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
                            kdiff3path = "";
                            return false;
                        }
                    }
                }
            }
            gitCommands.SetGlobalSetting("difftool.kdiff3.path", kdiff3path);

            return true;
        }

        private void GitFound_Click(object sender, EventArgs e)
        {
            SolveGitCmdDir();

            if (string.IsNullOrEmpty(GitCommands.Settings.GitCommand))
            {
                MessageBox.Show("The command to run git could not be determined automatically." + Environment.NewLine + "Please make sure git (msysgit) is installed or set the correct path manually.", "Locate git.cmd");

                tabControl1.SelectTab("TabPageGitExtensions");
                return;
            }

            MessageBox.Show("Command git.cmd can be runned using: " + GitCommands.Settings.GitCommand, "Locate git.cmd");
            GitPath.Text = GitCommands.Settings.GitCommand;
            Rescan_Click(null, null);
        }

        public static bool SolveGitCmdDir()
        {
            GitCommands.Settings.GitCommand = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") + "cmd\\git.cmd";
            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand, "")))
            {
                GitCommands.Settings.GitCommand = @"c:\Program Files (x86)\Git\cmd\git.cmd";
                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand, "")))
                {
                    GitCommands.Settings.GitCommand = @"c:\Program Files\Git\cmd\git.cmd";
                    if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand, "")))
                    {
                        GitCommands.Settings.GitCommand = @"C:\cygwin\bin\git";
                        if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand, "")))
                        {
                            GitCommands.Settings.GitCommand = "git.cmd";
                            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand, "")))
                            {
                                GitCommands.Settings.GitCommand = "git";
                                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand, "")))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private void FormSettigns_Load(object sender, EventArgs e)
        {
            EnableSettings();

            this.WindowState = FormWindowState.Normal;
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

            bool valid = GitCommands.Settings.ValidWorkingDir() && canFindGitCmd;
            UserName.Enabled = valid;
            UserEmail.Enabled = valid;
            Editor.Enabled = valid;
            MergeTool.Enabled = valid;
            KeepMergeBackup.Enabled = valid;
            LocalAutoCRLF.Enabled = valid;
            NoGitRepo.Visible = !valid;

        }

        private void CheckAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckAtStartup.Checked)
                Application.UserAppDataRegistry.SetValue("checksettings", "true");
            else
                Application.UserAppDataRegistry.SetValue("checksettings", "false");
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Save();
            CheckSettings();
        }

        private void BrowseGitPath_Click(object sender, EventArgs e)
        {
            SolveGitCmdDir();

            OpenFileDialog browseDialog = new OpenFileDialog();
            browseDialog.FileName = GitCommands.Settings.GitCommand;
            browseDialog.Filter = "Git.cmd (git.cmd)|git.cmd|Git.exe (git.exe)|git.exe|Git (git)|git";

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                GitPath.Text = browseDialog.FileName;
            }
        }

        private void TabPageGitExtensions_Click(object sender, EventArgs e)
        {
            GitPath.Text = GitCommands.Settings.GitCommand;
        }

        private void GitPath_TextChanged(object sender, EventArgs e)
        {
            GitCommands.Settings.GitCommand = GitPath.Text;
            LoadSettings();
        }

        private void GitBinFound_Click(object sender, EventArgs e)
        {
            SolveGitBinDir();

            if (string.IsNullOrEmpty(GitCommands.Settings.GitBinDir))
            {
                MessageBox.Show("The path to git.exe could not be found automatically." + Environment.NewLine + "Please make sure git (msysgit) is installed or set the correct path manually.", "Locate git.exe");
                tabControl1.SelectTab("TabPageGitExtensions");
                return;
            }

            MessageBox.Show("Command git.exe can be runned using: " + GitCommands.Settings.GitBinDir + "git.exe", "Locate git.exe");
            GitBinPath.Text = GitCommands.Settings.GitBinDir;
            Rescan_Click(null, null);
        }

        public static bool SolveGitBinDir()
        {
            GitCommands.Settings.GitBinDir = @"c:\Program Files\Git\bin\";
            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "")))
            {
                GitCommands.Settings.GitBinDir = @"c:\Program Files (x86)\Git\bin\";
                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "")))
                {
                    GitCommands.Settings.GitBinDir = "C:\\cygwin\\bin";
                    if (!Directory.Exists(GitCommands.Settings.GitBinDir))
                    {
                        GitCommands.Settings.GitBinDir = GitCommands.Settings.GitCommand;
                        GitCommands.Settings.GitBinDir = GitCommands.Settings.GitBinDir.Replace("\\cmd\\git.cmd", "\\bin\\");
                        if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "")))
                        {
                            GitCommands.Settings.GitBinDir = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") + "\\bin\\";
                            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "")))
                            {
                                GitCommands.Settings.GitBinDir = "";
                                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitCommand.Replace("git.cmd", "git.exe"))))
                                {
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
            if (AutoFindPuttyPathsInDir("c:\\Program Files\\PuTTY\\")) return true;
            if (AutoFindPuttyPathsInDir("c:\\Program Files (x86)\\PuTTY\\")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files\\TortoiseGit\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files (x86)\\TortoiseGit\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files\\TortoiseSvn\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files (x86)\\TortoiseSvn\\bin")) return true;
            if (AutoFindPuttyPathsInDir(GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\PuTTY_is1", "InstallLocation"))) return true;
            if (AutoFindPuttyPathsInDir(GitCommands.Settings.GetInstallDir() + "\\PuTTY\\")) return true;

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
            else
                return false;
        }

        private string SelectFile(string initialDirectory, string filter, string prev)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = filter;
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select file";
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
            PlinkPath.Text = SelectFile(".", "Plink.exe (plink.exe)|plink.exe|TortoisePlink.exe (tortoiseplink.exe)|tortoiseplink.exe", PlinkPath.Text);
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
                    MessageBox.Show("All paths needed for PuTTY could be automaticly found and are set.", "PuTTY");
                else
                    tabControl1.SelectTab("ssh");
            }
        }

        private void BrowseGitBinPath_Click(object sender, EventArgs e)
        {
            SolveGitBinDir();

            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.SelectedPath = GitCommands.Settings.GitBinDir;

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                GitBinPath.Text = browseDialog.SelectedPath;
            }
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                MergetoolPath.Text = SelectFile(".", "kdiff3.exe (kdiff3.exe)|kdiff3.exe", MergetoolPath.Text);
            }
            else
                if (GlobalMergeTool.Text.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase))
                    MergetoolPath.Text = SelectFile(".", "p4merge.exe (p4merge.exe)|p4merge.exe", MergetoolPath.Text);
                else
                    if (GlobalMergeTool.Text.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
                        MergetoolPath.Text = SelectFile(".", "TortoiseMerge.exe (TortoiseMerge.exe)|TortoiseMerge.exe", MergetoolPath.Text);
                    else
                        MergetoolPath.Text = SelectFile(".", "*.exe (*.exe)|*.exe", MergetoolPath.Text);

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                string kdiff3path = gitCommands.GetGlobalSetting("mergetool.kdiff3.path");
                string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                MergetoolPath.Text = FindFileInFolders("kdiff3.exe", kdiff3path,
                                                                    @"c:\Program Files\KDiff3\",
                                                                    @"c:\Program Files (x86)\KDiff3\",
                                                                    regkdiff3path);
            }
            if (GlobalMergeTool.Text.Equals("winmerge", StringComparison.CurrentCultureIgnoreCase))
            {
                string winmergepath = gitCommands.GetGlobalSetting("mergetool.winmerge.path");

                MergetoolPath.Text = FindFileInFolders("winmergeu.exe", winmergepath,
                                                                    @"c:\Program Files\winmerge\",
                                                                    @"c:\Program Files (x86)\winmerge\");
            }
            AutoConfigMergeToolcmd();
        }

        private string FindFileInFolders(string fileName, params string[] locations)
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
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();
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
                        MessageBox.Show("Please enter the path to p4merge.exe and press suggest.", "Suggest mergetool cmd");
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
                                                           @"C:\Program Files\Araxis\Araxis Merge\");

                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to Compare.exe and press suggest.", "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"" + MergetoolPath.Text + "\" -wait -merge -3 -a1 \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                return;
            }

            if (GlobalMergeTool.Text.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.Contains("kdiff3") || MergetoolPath.Text.Contains("p4merge"))
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
                        MessageBox.Show("Please enter the path to TortoiseMerge.exe and press suggest.", "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"TortoiseMerge.exe\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                return;
            }

            if (GlobalMergeTool.Text.Equals("DiffMerge", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.Contains("kdiff3") || MergetoolPath.Text.Contains("p4merge"))
                    MergetoolPath.Text = "";
                if (string.IsNullOrEmpty(MergetoolPath.Text) || !File.Exists(MergetoolPath.Text))
                {
                    MergetoolPath.Text = FindFileInFolders("DiffMerge.exe",
                                       @"C:\Program Files (x86)\SourceGear\DiffMerge\",
                                       @"C:\Program Files\SourceGear\DiffMerge\");

                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to DiffMerge.exe and press suggest.", "Suggest mergetool cmd");
                        return;
                    }
                }

                ///m /r=%merged /t1=%yname /t2=%bname /t3=%tname /c=%mname %mine %base %theirs
                MergeToolCmd.Text = "\"" + MergetoolPath.Text + "\" /m /r=\"$MERGED\" \"$LOCAL\" \"$BASE\" \"$REMOTE\"";
                return;
            }
        }

        private void GlobalMergeTool_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ShowRevisionGraph_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void FormSettigns_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.WindowState = FormWindowState.Normal;
            LoadSettings();
            CheckSettings();
            this.WindowState = FormWindowState.Normal;
        }

        private void FormSettigns_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (!Save())
                e.Cancel = true;
        }

        private void Dictionary_DropDown(object sender, EventArgs e)
        {
            try
            {
                Dictionary.Items.Clear();
                Dictionary.Items.Add("None");
                foreach (string fileName in Directory.GetFiles(GitCommands.Settings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    FileInfo file = new FileInfo(fileName);
                    Dictionary.Items.Add(file.Name.Replace(".dic", ""));
                }
            }
            catch
            {
                MessageBox.Show("No dictionary files found in: " + GitCommands.Settings.GetDictionaryDir());
            }
        }

        private void Dictionary_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ExternalDiffTool_TextChanged(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();
            DifftoolPath.Text = gitCommands.GetGlobalSetting("difftool." + GlobalDiffTool.Text.Trim() + ".path");
            DifftoolCmd.Text = gitCommands.GetGlobalSetting("difftool." + GlobalDiffTool.Text.Trim() + ".cmd");

            if (GlobalDiffTool.Text.Trim().Equals("winmerge", StringComparison.CurrentCultureIgnoreCase))
                DiffToolCmdSuggest_Click(null, null);

            ResolveDiffToolPath();
        }

        private void ResolveDiffToolPath()
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (GlobalDiffTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                string kdiff3path = gitCommands.GetGlobalSetting("difftool.kdiff3.path");
                if (!kdiff3path.Contains("kdiff3.exe"))
                    kdiff3path = "";
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
                                kdiff3path = MergetoolPath.Text;
                                if (!kdiff3path.Contains("kdiff3.exe"))
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
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (GlobalDiffTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                DifftoolPath.Text = SelectFile(".", "kdiff3.exe (kdiff3.exe)|kdiff3.exe", DifftoolPath.Text);
            }
            else
                if (GlobalDiffTool.Text.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase))
                    DifftoolPath.Text = SelectFile(".", "p4merge.exe (p4merge.exe)|p4merge.exe", DifftoolPath.Text);
                else
                    if (GlobalDiffTool.Text.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
                        DifftoolPath.Text = SelectFile(".", "TortoiseMerge.exe (TortoiseMerge.exe)|TortoiseMerge.exe", DifftoolPath.Text);
                    else
                        DifftoolPath.Text = SelectFile(".", "*.exe (*.exe)|*.exe", DifftoolPath.Text);
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();
            MergetoolPath.Text = gitCommands.GetGlobalSetting("mergetool." + GlobalMergeTool.Text.Trim() + ".path");
            MergeToolCmd.Text = gitCommands.GetGlobalSetting("mergetool." + GlobalMergeTool.Text.Trim() + ".cmd");

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(MergeToolCmd.Text))
                MergeToolCmd.Enabled = false;
            else
                MergeToolCmd.Enabled = true;

            button1_Click_1(null, null);
        }

        private void label25_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = RevisionGraphColorLabel.BackColor;
            colorDialog.ShowDialog();
            RevisionGraphColorLabel.BackColor = colorDialog.Color;
            RevisionGraphColorLabel.Text = colorDialog.Color.Name;
            RevisionGraphColorLabel.ForeColor = ColorHelper.GetForeColorForBackColor(RevisionGraphColorLabel.BackColor);
        }

        private void label25_Click_1(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = RevisionGraphColorSelected.BackColor;
            colorDialog.ShowDialog();
            RevisionGraphColorSelected.BackColor = colorDialog.Color;
            RevisionGraphColorSelected.Text = colorDialog.Color.Name;
            RevisionGraphColorSelected.ForeColor = ColorHelper.GetForeColorForBackColor(RevisionGraphColorSelected.BackColor);
        }


        private void ColorAddedLineDiffLabel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorAddedLineDiffLabel.BackColor;
            colorDialog.ShowDialog();
            ColorAddedLineDiffLabel.BackColor = colorDialog.Color;
            ColorAddedLineDiffLabel.Text = colorDialog.Color.Name;
            ColorAddedLineDiffLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorAddedLineDiffLabel.BackColor);
        }


        private void label28_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorAddedLineLabel.BackColor;
            colorDialog.ShowDialog();
            ColorAddedLineLabel.BackColor = colorDialog.Color;
            ColorAddedLineLabel.Text = colorDialog.Color.Name;
            ColorAddedLineLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorAddedLineLabel.BackColor);
        }

        private void ColorRemovedLineDiffLabel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorRemovedLineDiffLabel.BackColor;
            colorDialog.ShowDialog();
            ColorRemovedLineDiffLabel.BackColor = colorDialog.Color;
            ColorRemovedLineDiffLabel.Text = colorDialog.Color.Name;
            ColorRemovedLineDiffLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorRemovedLineDiffLabel.BackColor);

        }

        private void ColorRemovedLine_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorRemovedLine.BackColor;
            colorDialog.ShowDialog();
            ColorRemovedLine.BackColor = colorDialog.Color;
            ColorRemovedLine.Text = colorDialog.Color.Name;
            ColorRemovedLine.ForeColor = ColorHelper.GetForeColorForBackColor(ColorRemovedLine.BackColor);
        }

        private void ColorSectionLabel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorSectionLabel.BackColor;
            colorDialog.ShowDialog();
            ColorSectionLabel.BackColor = colorDialog.Color;
            ColorSectionLabel.Text = colorDialog.Color.Name;
            ColorSectionLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorSectionLabel.BackColor);
        }

        private void ColorTagLabel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorTagLabel.BackColor;
            colorDialog.ShowDialog();
            ColorTagLabel.BackColor = colorDialog.Color;
            ColorTagLabel.Text = colorDialog.Color.Name;
            ColorTagLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorTagLabel.BackColor);
        }

        private void ColorBranchLabel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorBranchLabel.BackColor;
            colorDialog.ShowDialog();
            ColorBranchLabel.BackColor = colorDialog.Color;
            ColorBranchLabel.Text = colorDialog.Color.Name;
            ColorBranchLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorBranchLabel.BackColor);
        }

        private void ColorRemoteBranchLabel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorRemoteBranchLabel.BackColor;
            colorDialog.ShowDialog();
            ColorRemoteBranchLabel.BackColor = colorDialog.Color;
            ColorRemoteBranchLabel.Text = colorDialog.Color.Name;
            ColorRemoteBranchLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorRemoteBranchLabel.BackColor);
        }

        private void ColorOtherLabel_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = ColorOtherLabel.BackColor;
            colorDialog.ShowDialog();
            ColorOtherLabel.BackColor = colorDialog.Color;
            ColorOtherLabel.Text = colorDialog.Color.Name;
            ColorOtherLabel.ForeColor = ColorHelper.GetForeColorForBackColor(ColorOtherLabel.BackColor);
       }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(MergeToolCmd.Text))
                MergeToolCmd.Enabled = false;
            else
                MergeToolCmd.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void ClearImageCache_Click(object sender, EventArgs e)
        {
            Gravatar.ClearImageCache();
        }

        private void DiffToolCmdSuggest_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (GlobalDiffTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                string kdiff3path = gitCommands.GetGlobalSetting("difftool.kdiff3.path");
                string regkdiff3path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\KDiff3", "") + "\\kdiff3.exe";

                DifftoolPath.Text = FindFileInFolders("kdiff3.exe", kdiff3path,
                                                                    @"c:\Program Files\KDiff3\",
                                                                    @"c:\Program Files (x86)\KDiff3\",
                                                                    regkdiff3path);
            }
            if (GlobalDiffTool.Text.Equals("winmerge", StringComparison.CurrentCultureIgnoreCase))
            {
                string winmergepath = gitCommands.GetGlobalSetting("difftool.winmerge.path");

                DifftoolPath.Text = FindFileInFolders("winmergeu.exe", winmergepath,
                                                                    @"c:\Program Files\winmerge\",
                                                                    @"c:\Program Files (x86)\winmerge\");
            }
            if (File.Exists(DifftoolPath.Text))
                DifftoolCmd.Text = "\"" + DifftoolPath.Text + "\" \"$LOCAL\" \"$REMOTE\"";
        }

        private void GlobalDiffTool_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
