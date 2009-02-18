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

namespace GitUI
{
    public partial class FormSettigns : GitExtensionsForm
    {
        public FormSettigns()
        {
            InitializeComponent();

            LoadSettings();
            CheckSettings();
        }

        private void LoadSettings()
        {
            MaxCommits.Value = GitCommands.Settings.MaxCommits;

            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            GitPath.Text = GitCommands.Settings.GitDir;
            GitBinPath.Text = GitCommands.Settings.GitBinDir;

            UserName.Text = GitCommands.GitCommands.GetSetting("user.name");
            UserEmail.Text = GitCommands.GitCommands.GetSetting("user.email");
            Editor.Text = GitCommands.GitCommands.GetSetting("core.editor");
            MergeTool.Text = GitCommands.GitCommands.GetSetting("merge.tool");

            KeepMergeBackup.Checked = GitCommands.GitCommands.GetSetting("mergetool.keepBackup").Trim() == "true";


            GlobalUserName.Text = gitCommands.GetGlobalSetting("user.name");
            GlobalUserEmail.Text = gitCommands.GetGlobalSetting("user.email");
            GlobalEditor.Text = gitCommands.GetGlobalSetting("core.editor");
            GlobalMergeTool.Text = gitCommands.GetGlobalSetting("merge.tool");

            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                MergetoolPath.Text = gitCommands.GetGlobalSetting("mergetool." + GlobalMergeTool.Text + ".path");
            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                MergeToolCmd.Text = gitCommands.GetGlobalSetting("mergetool." + GlobalMergeTool.Text + ".cmd");

            GlobalKeepMergeBackup.Checked = gitCommands.GetGlobalSetting("mergetool.keepBackup").Trim() == "true";

            PlinkPath.Text = GitCommands.Settings.Plink;
            PuttygenPath.Text = GitCommands.Settings.Puttygen;
            PageantPath.Text = GitCommands.Settings.Pageant;
            AutostartPageant.Checked = GitCommands.Settings.AutoStartPageant;

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

        private void UserName_TextChanged(object sender, EventArgs e)
        {
        }

        private void UserEmail_TextChanged(object sender, EventArgs e)
        {
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Save();

            Close();
        }

        private void Save()
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            GitCommands.Settings.GitDir = GitPath.Text;
            GitCommands.Settings.GitBinDir = GitBinPath.Text;

            GitCommands.GitCommands.SetSetting("user.name", UserName.Text);
            GitCommands.GitCommands.SetSetting("user.email", UserEmail.Text);
            GitCommands.GitCommands.SetSetting("core.editor", Editor.Text);
            GitCommands.GitCommands.SetSetting("merge.tool", MergeTool.Text);


            

            if (KeepMergeBackup.Checked)
                GitCommands.GitCommands.SetSetting("mergetool.keepBackup", "true");
            else
                GitCommands.GitCommands.SetSetting("mergetool.keepBackup", "false");


            gitCommands.SetGlobalSetting("user.name", GlobalUserName.Text);
            gitCommands.SetGlobalSetting("user.email", GlobalUserEmail.Text);
            gitCommands.SetGlobalSetting("core.editor", GlobalEditor.Text);
            gitCommands.SetGlobalSetting("merge.tool", GlobalMergeTool.Text);

            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                gitCommands.SetGlobalSetting("mergetool." + GlobalMergeTool.Text + ".path", MergetoolPath.Text);
            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                gitCommands.SetGlobalSetting("mergetool." + GlobalMergeTool.Text + ".cmd", MergeToolCmd.Text);

            if (GlobalKeepMergeBackup.Checked)
                gitCommands.SetGlobalSetting("mergetool.keepBackup", "true");
            else
                gitCommands.SetGlobalSetting("mergetool.keepBackup", "false");

            GitCommands.Settings.MaxCommits = (int)MaxCommits.Value;

            GitCommands.Settings.Plink = PlinkPath.Text;
            GitCommands.Settings.Puttygen = PuttygenPath.Text;
            GitCommands.Settings.Pageant = PageantPath.Text;
            GitCommands.Settings.AutoStartPageant = AutostartPageant.Checked;

            if (OpenSSH.Checked)
                GitCommands.GitCommands.UnSetSsh();
            
            if (Putty.Checked)
                GitCommands.GitCommands.SetSsh(PlinkPath.Text);
            
            if (Other.Checked)
                GitCommands.GitCommands.SetSsh(OtherSsh.Text);

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        protected string GetRegistryValue(RegistryKey root, string subkey, string key)
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
            catch (UnauthorizedAccessException ex)
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
                reg = "Windows Registry Editor Version 5.00"+ Environment.NewLine+Environment.NewLine+"[" + root.ToString() + "\\" + subkey + "]" + Environment.NewLine + "\"" + key + "\"=\"" + value + "\"";

                TextWriter tw = new StreamWriter(System.IO.Path.GetTempPath() + "GitExtensions.reg", false);
                tw.Write(reg);
                tw.Close();
                GitCommands.GitCommands.RunCmd("regedit", "\"" + System.IO.Path.GetTempPath() + "GitExtensions.reg" + "\"");
            }
            catch(UnauthorizedAccessException ex)
            {
                MessageBox.Show("GitExtensions has insufficient permisions to modify the registry.\nPlease add this key to the registry manually.\nPath:   " + root.ToString() + "\\" + subkey +"\nValue:  " + key + " = " + value);
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
                                DiffTool.Text = "There is a custom configured: " + mergetool;
                            }
                        }
                        else
                        {
                            DiffTool.BackColor = Color.LightGreen;
                            DiffTool.Text = "There is a mergetool configured.";
                        }
                    }
                }

                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
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

                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
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

        private void GitExtensionsInstall_Click(object sender, EventArgs e)
        {
            string fileName = Assembly.GetAssembly(GetType()).Location;
            fileName = fileName.Substring(0, fileName.LastIndexOfAny(new char[]{'\\', '/'}));

            if (File.Exists(fileName+"\\GitExtensions.exe"))
                GitCommands.Settings.SetInstallDir(fileName);

            CheckSettings();
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

        private void DiffTool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (string.IsNullOrEmpty(gitCommands.GetGlobalSetting("merge.tool")))
            {
                if (MessageBox.Show("There is no mergetool configured. Do you want to configure kdiff3 as your mergetool?\nSelect no if you want to configure a different mergetool yourself.", "Mergetool", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    gitCommands.SetGlobalSetting("merge.tool", "kdiff3");
                    GlobalMergeTool.Text = "kdiff3";
                }
                else
                {
                    tabControl1.SelectTab("GlobalSettingsPage");
                    return;
                }
            }

            if (gitCommands.GetGlobalSetting("merge.tool").Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
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
                                MessageBox.Show("Path to kdiff3 could not be found automatically.\nPlease make sure KDiff3 is installed or set path manually.");
                                tabControl1.SelectTab("GlobalSettingsPage");
                                return;

                            }
                        }
                    }
                    MessageBox.Show("KDiff3 located here: " + kdiff3path, "Locate KDiff3");
                }
                gitCommands.SetGlobalSetting("mergetool.kdiff3.path", kdiff3path);
                MergetoolPath.Text = kdiff3path;
            } else
            if (gitCommands.GetGlobalSetting("merge.tool").Equals("p4merge", StringComparison.CurrentCultureIgnoreCase) ||
                gitCommands.GetGlobalSetting("merge.tool").Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
            {
                AutoConfigMergeToolcmd();
                gitCommands.SetGlobalSetting("mergetool." + gitCommands.GetGlobalSetting("merge.tool") + ".cmd", MergeToolCmd.Text);
            }
            Rescan_Click(null, null);
        }

        private void GitFound_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
            {
                GitCommands.Settings.GitDir = @"c:\Program Files\Git\cmd\";
                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
                {
                    GitCommands.Settings.GitDir = @"c:\Program Files (x86)\Git\cmd\";
                    if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
                    {
                        GitCommands.Settings.GitDir = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") + "\\cmd\\";
                        if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
                        {
                            GitCommands.Settings.GitDir = "";

                            MessageBox.Show("The path to git.cmd could not be found automatically.\nPlease make sure git (msysgit) is installed or set the correct path manually.", "Locate git.cmd");

                            tabControl1.SelectTab("TabPageGitExtensions");
                            return;
                        }
                    }
                }
            }

            MessageBox.Show("Command git.cmd can be runned using: " + GitCommands.Settings.GitDir + "git.cmd", "Locate git.cmd");
            GitPath.Text = GitCommands.Settings.GitDir;
            Rescan_Click(null, null);
        }

        private void FormSettigns_Load(object sender, EventArgs e)
        {

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
            Save();
            CheckSettings();
        }

        private void BrowseGitPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                GitPath.Text = browseDialog.SelectedPath;
            }
        }

        private void TabPageGitExtensions_Click(object sender, EventArgs e)
        {
            GitPath.Text = GitCommands.Settings.GitDir;
        }

        private void GitPath_TextChanged(object sender, EventArgs e)
        {
            GitCommands.Settings.GitDir = GitPath.Text;
            LoadSettings();
        }

        private void GitBinFound_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
            {
                GitCommands.Settings.GitBinDir = @"c:\Program Files\Git\bin\";
                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
                {
                    GitCommands.Settings.GitBinDir = @"c:\Program Files (x86)\Git\bin\";
                    if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
                    {
                        GitCommands.Settings.GitBinDir = GitCommands.Settings.GitDir;
                        GitCommands.Settings.GitBinDir = GitCommands.Settings.GitBinDir.Replace("\\cmd\\", "\\bin\\");
                        if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
                        {
                            GitCommands.Settings.GitBinDir = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") + "\\bin\\";
                            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
                            {
                                GitCommands.Settings.GitBinDir = "";
                                MessageBox.Show("The path to git.exe could not be found automatically.\nPlease make sure git (msysgit) is installed or set the correct path manually.", "Locate git.exe");
                                tabControl1.SelectTab("TabPageGitExtensions");
                                return;
                            }
                        }
                    }
                }
            }

            MessageBox.Show("Command git.exe can be runned using: " + GitCommands.Settings.GitBinDir + "git.exe", "Locate git.exe");
            GitBinPath.Text = GitCommands.Settings.GitBinDir;
            Rescan_Click(null, null);
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

            if (File.Exists(PageantPath.Text) &&
                File.Exists(PuttygenPath.Text) &&
                File.Exists(PlinkPath.Text))
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
            PlinkPath.Text = SelectFile(".", "Plink.exe (plink.exe)|plink.exe", PlinkPath.Text);
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
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                GitBinPath.Text = browseDialog.SelectedPath;
            }
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                MergetoolPath.Text = SelectFile(".", "kdiff3.exe (kdiff3.exe)|kdiff3.exe", MergetoolPath.Text);
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
                MessageBox.Show("KDiff3 is supported by Git, you can leave mergetool cmd empty.", "Suggest mergetool cmd");
                MergeToolCmd.Text = "";

                string kdiff3path = gitCommands.GetGlobalSetting("mergetool.kdiff3.path");
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
                MergetoolPath.Text = kdiff3path;
            }
            AutoConfigMergeToolcmd();

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
                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to p4merge.exe and press suggest again.", "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"" + MergetoolPath.Text + "\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"";
                return;
            }

            if (GlobalMergeTool.Text.Equals("TortoiseMerge", StringComparison.CurrentCultureIgnoreCase))
            {
                if (MergetoolPath.Text.Contains("kdiff3") || MergetoolPath.Text.Contains("p4merge"))
                    MergetoolPath.Text = "";
                if (string.IsNullOrEmpty(MergetoolPath.Text) || !File.Exists(MergetoolPath.Text))
                {
                    MergetoolPath.Text = @"c:\Program Files\TortoiseSVN\bin\TortoiseMerge.exe";
                    if (!File.Exists(MergetoolPath.Text))
                    {
                        MergetoolPath.Text = "";
                        MessageBox.Show("Please enter the path to TortoiseMerge.exe and press suggest again.", "Suggest mergetool cmd");
                        return;
                    }
                }

                MergeToolCmd.Text = "\"TortoiseMerge.exe\" /base:\"$BASE\" /mine:\"$LOCAL\" /theirs:\"$REMOTE\" /merged:\"$MERGED\"";
                return;
            }
        }

        private void GlobalMergeTool_SelectedIndexChanged(object sender, EventArgs e)
        {
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();
            MergetoolPath.Text = gitCommands.GetGlobalSetting("mergetool." + GlobalMergeTool.Text.Trim() + ".path");
            MergeToolCmd.Text = gitCommands.GetGlobalSetting("mergetool." + GlobalMergeTool.Text.Trim() + ".cmd");
        }

    }
}
