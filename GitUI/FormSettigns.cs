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
    public partial class FormSettigns : Form
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
            GitLibexecPath.Text = GitCommands.Settings.GitLibexecDir;

            UserName.Text = GitCommands.GitCommands.GetSetting("user.name");
            UserEmail.Text = GitCommands.GitCommands.GetSetting("user.email");
            Editor.Text = GitCommands.GitCommands.GetSetting("core.editor");
            MergeTool.Text = GitCommands.GitCommands.GetSetting("merge.tool");
            KeepMergeBackup.Checked = GitCommands.GitCommands.GetSetting("mergetool.keepBackup").Trim() == "true";


            GlobalUserName.Text = gitCommands.GetGlobalSetting("user.name");
            GlobalUserEmail.Text = gitCommands.GetGlobalSetting("user.email");
            GlobalEditor.Text = gitCommands.GetGlobalSetting("core.editor");
            GlobalMergeTool.Text = gitCommands.GetGlobalSetting("merge.tool");
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
            GitCommands.Settings.GitLibexecDir = GitLibexecPath.Text;

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
                    DiffTool.BackColor = Color.LightGreen;
                    DiffTool.Text = "There is a mergetool configured.";
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

                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitLibexecDir + "git-clone.exe", "")))
                {
                    GitlibexecFound.BackColor = Color.LightSalmon;
                    GitlibexecFound.Text = "git-clone.exe not found. To solve this problem you can set the correct path in settings.";
                    bValid = false;
                }
                else
                {
                    GitlibexecFound.BackColor = Color.LightGreen;
                    GitlibexecFound.Text = "git-clone.exe is found on your computer.";
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
            tabControl1.SelectTab("GlobalSettingsPage");
        }

        private void GitFound_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
            {
                GitCommands.Settings.GitDir = @"c:\Program Files\Git\cmd\";
                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
                {
                    GitCommands.Settings.GitDir = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") + "\\cmd\\";
                    if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitDir + "git.cmd", "status")))
                    {
                        GitCommands.Settings.GitDir = "";
                        tabControl1.SelectTab("TabPageGitExtensions");
                        return;
                    }
                }
            }

            MessageBox.Show("Command git.cmd can be runned using: " + GitCommands.Settings.GitDir + "git.cmd", "Locate git.cmd");
            GitPath.Text = GitCommands.Settings.GitDir;
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
                    GitCommands.Settings.GitBinDir = GitCommands.Settings.GitDir;
                    GitCommands.Settings.GitBinDir = GitCommands.Settings.GitBinDir.Replace("\\cmd\\", "\\bin\\");
                    if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
                    {
                        GitCommands.Settings.GitBinDir = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") + "\\bin\\";
                        if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitBinDir + "git.exe", "status")))
                        {
                            GitCommands.Settings.GitBinDir = "";
                            tabControl1.SelectTab("TabPageGitExtensions");
                            return;
                        }
                    }
                }
            }

            MessageBox.Show("Command git.exe can be runned using: " + GitCommands.Settings.GitBinDir + "git.exe", "Locate git.exe");
            GitBinPath.Text = GitCommands.Settings.GitBinDir;

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

        private void GitLibexecPathBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                GitLibexecPath.Text = browseDialog.SelectedPath;
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

        private void GitlibexecFound_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitLibexecDir + "git-clone.exe", "")))
            {
                GitCommands.Settings.GitLibexecDir = @"c:\Program Files\Git\libexec\git-core\";
                if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitLibexecDir + "git-clone.exe", "")))
                {
                    GitCommands.Settings.GitLibexecDir = GitCommands.Settings.GitDir;
                    GitCommands.Settings.GitLibexecDir = GitCommands.Settings.GitBinDir.Replace("\\cmd\\", "\\libexec\\git-core\\");
                    if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitLibexecDir + "git-clone.exe", "")))
                    {
                        GitCommands.Settings.GitLibexecDir = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") + "\\libexec\\git-core\\";
                        if (string.IsNullOrEmpty(GitCommands.GitCommands.RunCmd(GitCommands.Settings.GitLibexecDir + "git-clone.exe", "")))
                        {
                            GitCommands.Settings.GitLibexecDir = "";
                            tabControl1.SelectTab("TabPageGitExtensions");
                            return;
                        }
                    }
                }
            }

            MessageBox.Show("Command git-clone.exe can be runned using: " + GitCommands.Settings.GitLibexecDir + "git-clone.exe", "Locate git-clone.exe");
            GitLibexecPath.Text = GitCommands.Settings.GitLibexecDir;
        }
    }
}
