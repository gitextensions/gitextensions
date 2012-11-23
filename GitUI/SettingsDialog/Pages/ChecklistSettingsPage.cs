using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using Microsoft.Win32;
using System.IO;
using GitCommands.Config;

namespace GitUI.SettingsDialog.Pages
{
    public partial class ChecklistSettingsPage : UserControl
    {
        CommonLogic _commonLogic = new CommonLogic();
        CheckSettingsLogic _checkSettingsLogic = new CheckSettingsLogic(null, null); // TODO

        public ChecklistSettingsPage()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        public GitModule Module { 
            get { return null; /* TODO: see GitModuleForm */ }
            set { _commonLogic.Module = value; } }

        private void gitCredentialWinStore_Fix_Click(object sender, EventArgs e)
        {
            if (_checkSettingsLogic.SolveGitCredentialStore())
            {
                MessageBox.Show(this, _gitCredentialWinStoreHelperInstalled.Text);
            }
            else
            {
                MessageBox.Show(this, _noCredentialsHelperInstalledTryGCS.Text);
            }

            CheckSettings();
        }

        private void translationConfig_Click(object sender, EventArgs e)
        {

        }

        private void SshConfig_Click(object sender, EventArgs e)
        {

        }

        private void GitExtensionsInstall_Click(object sender, EventArgs e)
        {
            _checkSettingsLogic.SolveGitExtensionsDir();
            _checkSettingsLogic.CheckSettings();
        }

        private void GitBinFound_Click(object sender, EventArgs e)
        {

        }

        private void ShellExtensionsRegistered_Click(object sender, EventArgs e)
        {

        }

        private void DiffToolFix_Click(object sender, EventArgs e)
        {

        }

        private void MergeToolFix_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_commonLogic.GetMergeTool()))
            {
                if (
                    MessageBox.Show(this, _noMergeToolConfigured.Text,
                        _noMergeToolConfiguredCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _checkSettingsLogic.SolveMergeToolForKDiff();
                    GlobalMergeTool.Text = "kdiff3";
                }
                else
                {
                    GotoPageGlobalSettings();
                    return;
                }
            }

            if (_commonLogic.IsMergeTool("kdiff3"))
            {
                _checkSettingsLogic.SolveMergeToolPathForKDiff();
            }
            else if (_commonLogic.IsMergeTool("p4merge") || _commonLogic.IsMergeTool("TortoiseMerge"))
            {
                AutoConfigMergeToolCmd(true);

                Module.SetGlobalPathSetting(
                    string.Format("mergetool.{0}.cmd", _commonLogic.GetMergeTool()), MergeToolCmd.Text);
            }

            if (_commonLogic.IsMergeTool("kdiff3") &&
                string.IsNullOrEmpty(Module.GetGlobalSetting("mergetool.kdiff3.path")))
            {
                MessageBox.Show(this, _kdiff3NotFoundAuto.Text);
                GotoPageGlobalSettings();
                return;
            }

            Rescan_Click(null, null);
        }

        private void GotoPageGlobalSettings()
        {
            throw new NotImplementedException("tabControl1.SelectTab(tpGlobalSettings);");
        }

        private void UserNameSet_Click(object sender, EventArgs e)
        {

        }

        private void GitFound_Click(object sender, EventArgs e)
        {
            if (!_checkSettingsLogic.SolveGitCommand())
            {
                MessageBox.Show(this, _solveGitCommandFailed.Text, _solveGitCommandFailedCaption.Text);

                GotoPageGit();
                return;
            }

            MessageBox.Show(this, String.Format(_gitCanBeRun.Text, Settings.GitCommand), _gitCanBeRunCaption.Text);

            GitPath.Text = Settings.GitCommand;
            Rescan_Click(null, null);
        }

        private void GotoPageGit()
        {
            throw new NotImplementedException("tabControl1.SelectTab(tpGit);");
        }


        private void Rescan_Click(object sender, EventArgs e)
        {

        }

        private void CheckAtStartup_CheckedChanged(object sender, EventArgs e)
        {

        }

        private bool CheckTranslationConfigSettings()
        {
            translationConfig.Visible = true;
            if (string.IsNullOrEmpty(Settings.Translation))
            {
                translationConfig.BackColor = Color.LightSalmon;
                translationConfig.Text = _noLanguageConfigured.Text;
                translationConfig_Fix.Visible = true;
                return false;
            }
            translationConfig.BackColor = Color.LightGreen;
            translationConfig_Fix.Visible = false;
            translationConfig.Text = String.Format(_languageConfigured.Text, Settings.Translation);
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
                    SshConfig.Text = _plinkputtyGenpageantNotFound.Text;
                    SshConfig_Fix.Visible = true;
                    return false;
                }
                SshConfig.BackColor = Color.LightGreen;
                SshConfig_Fix.Visible = false;
                SshConfig.Text = _puttyConfigured.Text;
                return true;
            }
            SshConfig.BackColor = Color.LightGreen;
            SshConfig_Fix.Visible = false;
            if (string.IsNullOrEmpty(GitCommandHelpers.GetSsh()))
                SshConfig.Text = _opensshUsed.Text;
            else
                SshConfig.Text = String.Format(_unknownSshClient.Text, GitCommandHelpers.GetSsh());
            return true;
        }

        private bool CheckGitExe()
        {
            GitBinFound.Visible = true;
            if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh") &&
                !CheckIfFileIsInPath("sh.exe") && !CheckIfFileIsInPath("sh"))
            {
                GitBinFound.BackColor = Color.LightSalmon;
                GitBinFound.Text = _linuxToolsSshNotFound.Text;
                GitBinFound_Fix.Visible = true;
                return false;
            }
            GitBinFound_Fix.Visible = false;
            GitBinFound.BackColor = Color.LightGreen;
            GitBinFound.Text = _linuxToolsSshFound.Text;
            return true;
        }

        private bool CheckGitCmdValid()
        {
            GitFound.Visible = true;
            if (!CanFindGitCmd())
            {
                GitFound.BackColor = Color.LightSalmon;
                GitFound.Text = _gitNotFound.Text;
                GitFound_Fix.Visible = true;
                return false;
            }

            if (GitCommandHelpers.VersionInUse < GitVersion.LastSupportedVersion)
            {
                GitFound.BackColor = Color.LightSalmon;
                GitFound.Text = String.Format(_wrongGitVersion.Text, GitCommandHelpers.VersionInUse, GitVersion.LastSupportedVersion);
                GitFound_Fix.Visible = true;
                return false;
            }

            GitFound_Fix.Visible = false;
            GitFound.BackColor = Color.LightGreen;
            GitFound.Text = String.Format(_gitVersionFound.Text, GitCommandHelpers.VersionInUse);
            return true;
        }

        private bool CheckGitCredentialStore()
        {
            gitCredentialWinStore.Visible = true;
            bool isValid = !string.IsNullOrEmpty(GitCommandHelpers.GetGlobalConfig().GetValue("credential.helper"));

            if (isValid)
            {
                gitCredentialWinStore.BackColor = Color.LightGreen;
                gitCredentialWinStore.Text = _credentialHelperInstalled.Text;
                gitCredentialWinStore_Fix.Visible = false;
            }
            else
            {
                gitCredentialWinStore.BackColor = Color.LightSalmon;
                gitCredentialWinStore.Text = _noCredentialsHelperInstalled.Text;
                gitCredentialWinStore_Fix.Visible = true;
            }

            return isValid;
        }

        private bool CheckDiffToolConfiguration()
        {
            DiffTool.Visible = true;
            if (string.IsNullOrEmpty(CheckSettingsLogic.GetGlobalDiffToolFromConfig()))
            {
                DiffTool.BackColor = Color.LightSalmon;
                DiffTool_Fix.Visible = true;
                DiffTool.Text = _adviceDiffToolConfiguration.Text;
                return false;
            }
            if (Settings.RunningOnWindows())
            {
                if (GetGlobalDiffToolFromConfig().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                {
                    string p = Module.GetGlobalSetting("difftool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        DiffTool.BackColor = Color.LightSalmon;
                        DiffTool.Text = _kdiffAsDiffConfiguredButNotFound.Text;
                        DiffTool_Fix.Visible = true;
                        return false;
                    }
                    DiffTool.BackColor = Color.LightGreen;
                    DiffTool.Text = _kdiffAsDiffConfigured.Text;
                    DiffTool_Fix.Visible = false;
                    return true;
                }
            }
            string difftool = CheckSettingsLogic.GetGlobalDiffToolFromConfig();
            DiffTool.BackColor = Color.LightGreen;
            DiffTool.Text = String.Format(_diffToolXConfigured.Text, difftool);
            DiffTool_Fix.Visible = false;
            return true;
        }

        private bool CheckMergeTool()
        {
            MergeTool.Visible = true;
            if (string.IsNullOrEmpty(GetMergeTool()))
            {
                MergeTool.BackColor = Color.LightSalmon;
                MergeTool.Text = _configureMergeTool.Text;
                MergeTool_Fix.Visible = true;
                return false;
            }

            if (Settings.RunningOnWindows())
            {
                if (_commonLogic.IsMergeTool("kdiff3"))
                {
                    string p = Module.GetGlobalSetting("mergetool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        MergeTool.BackColor = Color.LightSalmon;
                        MergeTool.Text = _kdiffAsMergeConfiguredButNotFound.Text;
                        MergeTool_Fix.Visible = true;
                        return false;
                    }
                    MergeTool.BackColor = Color.LightGreen;
                    MergeTool.Text = _kdiffAsMergeConfigured.Text;
                    MergeTool_Fix.Visible = false;
                    return true;
                }
                string mergetool = GetMergeTool().ToLowerInvariant();
                if (mergetool == "p4merge" || mergetool == "tmerge")
                {
                    string p = Module.GetGlobalSetting(string.Format("mergetool.{0}.cmd", mergetool));
                    if (string.IsNullOrEmpty(p))
                    {
                        MergeTool.BackColor = Color.LightSalmon;
                        MergeTool.Text = String.Format(_mergeToolXConfiguredNeedsCmd.Text, mergetool);
                        MergeTool_Fix.Visible = true;
                        return false;
                    }
                    MergeTool.BackColor = Color.LightGreen;
                    MergeTool.Text = String.Format(_customMergeToolXConfigured.Text, mergetool);
                    MergeTool_Fix.Visible = false;
                    return true;
                }
            }
            MergeTool.BackColor = Color.LightGreen;
            MergeTool.Text = _mergeToolXConfigured.Text;
            MergeTool_Fix.Visible = false;
            return true;
        }

        private bool CheckGlobalUserSettingsValid()
        {
            UserNameSet.Visible = true;
            if (string.IsNullOrEmpty(Module.GetGlobalSetting("user.name")) ||
                string.IsNullOrEmpty(Module.GetGlobalSetting("user.email")))
            {
                UserNameSet.BackColor = Color.LightSalmon;
                UserNameSet.Text = _noEmailSet.Text;
                UserNameSet_Fix.Visible = true;
                return false;
            }
            UserNameSet.BackColor = Color.LightGreen;
            UserNameSet.Text = _emailSet.Text;
            UserNameSet_Fix.Visible = false;
            return true;
        }

        private bool CheckGitExtensionRegistrySettings()
        {
            if (!Settings.RunningOnWindows())
                return true;

            ShellExtensionsRegistered.Visible = true;

            if (string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.LocalMachine,
                                                      "Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved",
                                                      "{3C16B20A-BA16-4156-916F-0A375ECFFE24}")) ||
                string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot,
                                                      "*\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot,
                                                      "Directory\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot,
                                                      "Directory\\Background\\shellex\\ContextMenuHandlers\\GitExtensions2",
                                                      null)))
            {
                //Check if shell extensions are installed
                string path = Path.Combine(Settings.GetInstallDir(), CommonLogic.GitExtensionsShellExName);
                if (!File.Exists(path))
                {
                    ShellExtensionsRegistered.BackColor = Color.LightGreen;
                    ShellExtensionsRegistered.Text = String.Format(_shellExtNoInstalled.Text);
                    ShellExtensionsRegistered_Fix.Visible = false;
                    return true;
                }

                ShellExtensionsRegistered.BackColor = Color.LightSalmon;
                ShellExtensionsRegistered.Text = String.Format(_shellExtNeedsToBeRegistered.Text, CommonLogic.GitExtensionsShellExName);
                ShellExtensionsRegistered_Fix.Visible = true;
                return false;
            }
            ShellExtensionsRegistered.BackColor = Color.LightGreen;
            ShellExtensionsRegistered.Text = _shellExtRegistered.Text;
            ShellExtensionsRegistered_Fix.Visible = false;
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
                GitExtensionsInstall.Text = _registryKeyGitExtensionsMissing.Text;
                GitExtensionsInstall_Fix.Visible = true;
                return false;
            }
            if (Settings.GetInstallDir() != null && Settings.GetInstallDir().EndsWith(".exe"))
            {
                GitExtensionsInstall.BackColor = Color.LightSalmon;
                GitExtensionsInstall.Text = _registryKeyGitExtensionsFaulty.Text;
                GitExtensionsInstall_Fix.Visible = true;
                return false;
            }
            GitExtensionsInstall.BackColor = Color.LightGreen;
            GitExtensionsInstall.Text = _registryKeyGitExtensionsCorrect.Text;
            GitExtensionsInstall_Fix.Visible = false;
            return true;
        }
    }
}
