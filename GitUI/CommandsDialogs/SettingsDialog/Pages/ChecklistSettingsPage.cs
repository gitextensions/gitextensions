using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Utils;
using Microsoft.Win32;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ChecklistSettingsPage : SettingsPageWithHeader
    {
        #region Translations
        private readonly TranslationString _wrongGitVersion =
            new TranslationString("Git found but version {0} is not supported. Upgrade to version {1} or later");

        private readonly TranslationString _notRecommendedGitVersion =
            new TranslationString("Git found but version {0} is older than recommended. Upgrade to version {1} or later");

        private readonly TranslationString _gitVersionFound =
            new TranslationString("Git {0} is found on your computer.");

        private readonly TranslationString _unknownSshClient =
            new TranslationString("Unknown SSH client configured: {0}.");

        private readonly TranslationString _linuxToolsSshNotFound =
            new TranslationString("Linux tools (sh) not found. To solve this problem you can set the correct path in settings.");

        private readonly TranslationString _solveGitCommandFailedCaption =
            new TranslationString("Locate git");

        private readonly TranslationString _gitCanBeRun =
            new TranslationString("Git can be run using: {0}");

        private readonly TranslationString _gitCanBeRunCaption =
            new TranslationString("Locate git");

        private readonly TranslationString _solveGitCommandFailed =
            new TranslationString("The command to run git could not be determined automatically." + Environment.NewLine +
                "Please make sure git (Git for Windows or cygwin) is installed or set the correct command manually.");

        private readonly TranslationString _shellExtRegistered =
            new TranslationString("Shell extensions registered properly.");

        private readonly TranslationString _shellExtNoInstalled =
            new TranslationString("Shell extensions are not installed. Run the installer to install the shell extensions.");

        private readonly TranslationString _shellExtNeedsToBeRegistered =
            new TranslationString("{0} needs to be registered in order to use the shell extensions.");

        private readonly TranslationString _registryKeyGitExtensionsMissing =
            new TranslationString("Registry entry missing [Software\\GitExtensions\\InstallDir].");

        private readonly TranslationString _registryKeyGitExtensionsFaulty =
            new TranslationString("Invalid installation directory stored in [Software\\GitExtensions\\InstallDir].");

        private readonly TranslationString _registryKeyGitExtensionsCorrect =
            new TranslationString("GitExtensions is properly registered.");

        private readonly TranslationString _plinkputtyGenpageantNotFound =
            new TranslationString("PuTTY is configured as SSH client but cannot find plink.exe, puttygen.exe or pageant.exe.");

        private readonly TranslationString _puttyConfigured =
            new TranslationString("SSH client PuTTY is configured properly.");

        private readonly TranslationString _opensshUsed =
            new TranslationString("Default SSH client, OpenSSH, will be used. (commandline window will appear on pull, push and clone operations)");

        private readonly TranslationString _noMergeToolConfigured =
            new TranslationString("There is no mergetool configured. Do you want to configure kdiff3 as your mergetool?" +
                Environment.NewLine + "Select no if you want to configure a different mergetool yourself.");

        private readonly TranslationString _noMergeToolConfiguredCaption =
            new TranslationString("Mergetool");

        private readonly TranslationString _languageConfigured =
            new TranslationString("The configured language is {0}.");

        private readonly TranslationString _noLanguageConfigured =
            new TranslationString("There is no language configured for Git Extensions.");

        private readonly TranslationString _noEmailSet =
            new TranslationString("You need to configure a username and an email address.");

        private readonly TranslationString _emailSet =
            new TranslationString("A username and an email address are configured.");

        private readonly TranslationString _mergeToolXConfiguredNeedsCmd =
            new TranslationString("{0} is configured as mergetool, this is a custom mergetool and needs a custom cmd to be configured.");

        private readonly TranslationString _customMergeToolXConfigured =
            new TranslationString("There is a custom mergetool configured: {0}");

        private readonly TranslationString _mergeToolXConfigured =
            new TranslationString("There is a mergetool configured: {0}");

        private readonly TranslationString _linuxToolsSshFound =
            new TranslationString("Linux tools (sh) found on your computer.");

        private readonly TranslationString _gitNotFound =
            new TranslationString("Git not found. To solve this problem you can set the correct path in settings.");

        private readonly TranslationString _adviceDiffToolConfiguration =
            new TranslationString("You should configure a diff tool to show file diff in external program (kdiff3 for example).");

        private readonly TranslationString _kdiffAsDiffConfiguredButNotFound =
            new TranslationString("KDiff3 is configured as difftool, but the path to kdiff.exe is not configured.");

        private readonly TranslationString _kdiffAsDiffConfigured =
            new TranslationString("KDiff3 is configured as difftool.");

        private readonly TranslationString _diffToolXConfigured =
            new TranslationString("There is a difftool configured: {0}");

        private readonly TranslationString _configureMergeTool =
            new TranslationString("You need to configure merge tool in order to solve merge conflicts (kdiff3 for example).");

        private readonly TranslationString _kdiffAsMergeConfiguredButNotFound =
            new TranslationString("KDiff3 is configured as mergetool, but the path to kdiff.exe is not configured.");

        private readonly TranslationString _kdiffAsMergeConfigured =
            new TranslationString("KDiff3 is configured as mergetool.");

        private readonly TranslationString _kdiff3NotFoundAuto =
            new TranslationString("Path to kdiff3 could not be found automatically." + Environment.NewLine +
                "Please make sure KDiff3 is installed or set path manually.");

        private readonly TranslationString _cantRegisterShellExtension =
            new TranslationString("Could not register the shell extension because '{0}' could not be found.");

        private readonly TranslationString _noDiffToolConfigured =
            new TranslationString("There is no difftool configured. Do you want to configure kdiff3 as your difftool?" +
                Environment.NewLine + "Select no if you want to configure a different difftool yourself.");

        private readonly TranslationString _noDiffToolConfiguredCaption =
            new TranslationString("Difftool");

        private readonly TranslationString _puttyFoundAuto =
            new TranslationString("All paths needed for PuTTY could be automatically found and are set.");

        private readonly TranslationString _linuxToolsShNotFound =
            new TranslationString("The path to linux tools (sh) could not be found automatically." + Environment.NewLine +
                "Please make sure there are linux tools installed (through Git for Windows or cygwin) or set the correct path manually.");

        private readonly TranslationString _linuxToolsShNotFoundCaption =
            new TranslationString("Locate linux tools");

        private readonly TranslationString _shCanBeRun =
            new TranslationString("Command sh can be run using: {0}sh");

        private readonly TranslationString _shCanBeRunCaption =
            new TranslationString("Locate linux tools");

        private readonly TranslationString _gcmDetectedCaption = new TranslationString("Obsolete git-credential-winstore.exe detected");
        #endregion

        private const string _putty = "PuTTY";
        private readonly ISshPathLocator _sshPathLocator = new SshPathLocator();

        /// <summary>
        /// TODO: remove this direct dependency to another SettingsPage later when possible
        /// </summary>
        public SshSettingsPage SshSettingsPage { get; set; }

        public ChecklistSettingsPage()
        {
            InitializeComponent();
            Text = "Checklist";
            InitializeComplete();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(ChecklistSettingsPage));
        }

        public override bool IsInstantSavePage => true;

        protected override void SettingsToPage()
        {
        }

        protected override void PageToSettings()
        {
        }

        public override void OnPageShown()
        {
            ////LoadSettings();
            CheckSettings();
        }

        private string GetGlobalSetting(string settingName)
        {
            return CommonLogic.ConfigFileSettingsSet.GlobalSettings.GetValue(settingName);
        }

        private void SetGlobalPathSetting(string settingName, string value)
        {
            CommonLogic.ConfigFileSettingsSet.GlobalSettings.SetPathValue(settingName, value);
        }

        private void translationConfig_Click(object sender, EventArgs e)
        {
            using (var frm = new FormChooseTranslation())
            {
                frm.ShowDialog(this); // will set Settings.Translation
            }

            PageHost.LoadAll();

            Translator.Translate(this, AppSettings.CurrentTranslation);
            SaveAndRescan_Click(null, null);
        }

        private void SshConfig_Click(object sender, EventArgs e)
        {
            if (GitCommandHelpers.Plink())
            {
                if (SshSettingsPage.AutoFindPuttyPaths())
                {
                    MessageBox.Show(this, _puttyFoundAuto.Text, _putty);
                }
                else
                {
                    PageHost.GotoPage(SshSettingsPage.GetPageReference());
                }
            }
        }

        private void GitExtensionsInstall_Click(object sender, EventArgs e)
        {
            CheckSettingsLogic.SolveGitExtensionsDir();
            CheckSettings();
        }

        private void GitBinFound_Click(object sender, EventArgs e)
        {
            if (!CheckSettingsLogic.SolveLinuxToolsDir())
            {
                MessageBox.Show(this, _linuxToolsShNotFound.Text, _linuxToolsShNotFoundCaption.Text);
                PageHost.GotoPage(GitSettingsPage.GetPageReference());
                return;
            }

            MessageBox.Show(this, string.Format(_shCanBeRun.Text, AppSettings.GitBinDir), _shCanBeRunCaption.Text);
            ////GitBinPath.Text = Settings.GitBinDir;
            PageHost.LoadAll(); // apply settings to dialog controls (otherwise the later called SaveAndRescan_Click would overwrite settings again)
            SaveAndRescan_Click(null, null);
        }

        private void ShellExtensionsRegistered_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(AppSettings.GetInstallDir(), CommonLogic.GitExtensionsShellEx32Name);

            if (!File.Exists(path))
            {
                path = Assembly.GetAssembly(GetType()).Location;
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, CommonLogic.GitExtensionsShellEx32Name);
            }

            if (File.Exists(path))
            {
                try
                {
                    var pi = new ProcessStartInfo
                    {
                        FileName = "regsvr32",
                        Arguments = path.Quote(),
                        Verb = "RunAs",
                        UseShellExecute = true
                    };

                    var process = Process.Start(pi);
                    process.WaitForExit();

                    if (IntPtr.Size == 8)
                    {
                        path = path.Replace(CommonLogic.GitExtensionsShellEx32Name, CommonLogic.GitExtensionsShellEx64Name);
                        if (File.Exists(path))
                        {
                            pi.Arguments = path.Quote();

                            var process64 = Process.Start(pi);
                            process64.WaitForExit();
                        }
                        else
                        {
                            MessageBox.Show(this, string.Format(_cantRegisterShellExtension.Text, CommonLogic.GitExtensionsShellEx64Name));
                        }
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // User cancel operation, continue;
                }
            }
            else
            {
                MessageBox.Show(this, string.Format(_cantRegisterShellExtension.Text, CommonLogic.GitExtensionsShellEx32Name));
            }

            CheckSettings();
        }

        private void DiffToolFix_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CheckSettingsLogic.GetDiffToolFromConfig(CheckSettingsLogic.CommonLogic.ConfigFileSettingsSet.GlobalSettings)))
            {
                if (MessageBox.Show(this, _noDiffToolConfigured.Text, _noDiffToolConfiguredCaption.Text,
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CheckSettingsLogic.SolveDiffToolForKDiff();
                    PageHost.LoadAll(); // apply settings to dialog controls (otherwise the later called SaveAndRescan_Click would overwrite settings again)
                }
                else
                {
                    GotoPageGlobalSettings();
                    return;
                }
            }

            if (CheckSettingsLogic.GetDiffToolFromConfig(CheckSettingsLogic.CommonLogic.ConfigFileSettingsSet.GlobalSettings).Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                CheckSettingsLogic.SolveDiffToolPathForKDiff();
            }

            if (CheckSettingsLogic.GetDiffToolFromConfig(CheckSettingsLogic.CommonLogic.ConfigFileSettingsSet.GlobalSettings).Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(GetGlobalSetting("difftool.kdiff3.path")))
            {
                MessageBox.Show(this, _kdiff3NotFoundAuto.Text);
                GotoPageGlobalSettings();
                return;
            }

            SaveAndRescan_Click(null, null);
        }

        private readonly string[] _autoConfigMergeTools =
        {
            "p4merge",
            "TortoiseMerge",
            "meld",
            "beyondcompare3",
            "beyondcompare4",
            "diffmerge",
            "semanticmerge",
            "vscode",
            "vsdiffmerge",
            "winmerge"
        };

        private void MergeToolFix_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CommonLogic.GetGlobalMergeTool()))
            {
                if (
                    MessageBox.Show(this, _noMergeToolConfigured.Text,
                        _noMergeToolConfiguredCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CommonLogic.SetGlobalMergeTool("kdiff3");
                    PageHost.LoadAll(); // apply settings to dialog controls (otherwise the later called SaveAndRescan_Click would overwrite settings again)
                }
                else
                {
                    GotoPageGlobalSettings();
                    return;
                }
            }

            if (CommonLogic.IsMergeTool("kdiff3"))
            {
                CheckSettingsLogic.SolveMergeToolPathForKDiff();
            }
            else if (_autoConfigMergeTools.Any(tool => CommonLogic.IsMergeTool(tool)))
            {
                CheckSettingsLogic.AutoConfigMergeToolCmd();

                SetGlobalPathSetting(
                    string.Format("mergetool.{0}.cmd", CommonLogic.GetGlobalMergeTool()), CheckSettingsLogic.GetMergeToolCmdText());
            }

            if (CommonLogic.IsMergeTool("kdiff3") &&
                string.IsNullOrEmpty(GetGlobalSetting("mergetool.kdiff3.path")))
            {
                MessageBox.Show(this, _kdiff3NotFoundAuto.Text);
                GotoPageGlobalSettings();
                return;
            }

            SaveAndRescan_Click(null, null);
        }

        // TODO: needed somewhere?
        ////private void SetGlobalMergeToolText(string text)
        ////{
        ////    throw new NotImplementedException("GlobalMergeTool.Text = ...");
        ////}

        private void GotoPageGlobalSettings()
        {
            PageHost.GotoPage(GitConfigSettingsPage.GetPageReference());
        }

        private void UserNameSet_Click(object sender, EventArgs e)
        {
            GotoPageGlobalSettings();

            // nice-to-have: jump directly to correct text box
        }

        private void GitFound_Click(object sender, EventArgs e)
        {
            if (!CheckSettingsLogic.SolveGitCommand())
            {
                MessageBox.Show(this, _solveGitCommandFailed.Text, _solveGitCommandFailedCaption.Text);

                PageHost.GotoPage(GitSettingsPage.GetPageReference());
                return;
            }

            MessageBox.Show(this, string.Format(_gitCanBeRun.Text, AppSettings.GitCommandValue), _gitCanBeRunCaption.Text);

            PageHost.GotoPage(GitSettingsPage.GetPageReference());
            SaveAndRescan_Click(null, null);
        }

        private void SaveAndRescan_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                PageHost.SaveAll();
                PageHost.LoadAll();
                CheckSettings();
            }
        }

        private void CheckAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.CheckSettings = CheckAtStartup.Checked;
        }

        public bool CheckSettings()
        {
            var isValid = PerformChecks();
            CheckAtStartup.Checked = IsCheckAtStartupChecked(isValid);
            return isValid;

            bool PerformChecks()
            {
                bool result = true;
                foreach (var func in CheckFuncs())
                {
                    try
                    {
                        result &= func();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(this, e.Message);
                    }
                }

                return result;

                IEnumerable<Func<bool>> CheckFuncs()
                {
                    yield return CheckGitCmdValid;
                    yield return CheckGlobalUserSettingsValid;
                    yield return CheckEditorTool;
                    yield return CheckMergeTool;
                    yield return CheckDiffToolConfiguration;
                    yield return CheckTranslationConfigSettings;

                    if (EnvUtils.RunningOnWindows())
                    {
                        yield return CheckGitExtensionsInstall;
                        yield return CheckGitExtensionRegistrySettings;
                        yield return CheckGitExe;
                        yield return CheckSSHSettings;
                        yield return CheckGitCredentialWinStore;
                    }
                }
            }
        }

        private static bool IsCheckAtStartupChecked(bool isValid)
        {
            var retValue = AppSettings.CheckSettings;

            if (isValid && retValue)
            {
                AppSettings.CheckSettings = false;
                retValue = false;
            }

            return retValue;
        }

        /// <summary>
        /// The Git Credential Manager for Windows (GCM) provides secure Git credential storage for Windows.
        /// It's the successor to the Windows Credential Store for Git (git-credential-winstore), which is no longer maintained.
        /// Check whether the user has an outdated setting pointing to git-credential-winstore and, if so,
        /// notify the user and point to our GitHub thread with more information.
        /// </summary>
        /// <seealso href="https://github.com/gitextensions/gitextensions/issues/3511#issuecomment-313633897"/>
        private bool CheckGitCredentialWinStore()
        {
            var setting = GetGlobalSetting(SettingKeyString.CredentialHelper) ?? string.Empty;
            if (setting.IndexOf("git-credential-winstore.exe", StringComparison.OrdinalIgnoreCase) < 0)
            {
                GcmDetected.Visible = false;
                GcmDetectedFix.Visible = false;
                return true;
            }

            GcmDetected.Visible = true;
            GcmDetectedFix.Visible = true;

            RenderSettingUnset(GcmDetected, GcmDetectedFix, _gcmDetectedCaption.Text);
            return false;
        }

        private bool CheckTranslationConfigSettings()
        {
            return RenderSettingSetUnset(() => string.IsNullOrEmpty(AppSettings.Translation),
                                    translationConfig, translationConfig_Fix,
                                    _noLanguageConfigured.Text, string.Format(_languageConfigured.Text, AppSettings.Translation));
        }

        private bool CheckSSHSettings()
        {
            SshConfig.Visible = true;
            if (GitCommandHelpers.Plink())
            {
                return RenderSettingSetUnset(() => !File.Exists(AppSettings.Plink) || !File.Exists(AppSettings.Puttygen) || !File.Exists(AppSettings.Pageant),
                                        SshConfig, SshConfig_Fix,
                                        _plinkputtyGenpageantNotFound.Text,
                                        _puttyConfigured.Text);
            }

            var ssh = _sshPathLocator.Find(AppSettings.GitBinDir);
            RenderSettingSet(SshConfig, SshConfig_Fix, string.IsNullOrEmpty(ssh) ? _opensshUsed.Text : string.Format(_unknownSshClient.Text, ssh));
            return true;
        }

        private bool CheckGitExe()
        {
            return RenderSettingSetUnset(() => !File.Exists(AppSettings.GitBinDir + "sh.exe") && !File.Exists(AppSettings.GitBinDir + "sh") &&
                                         !CheckSettingsLogic.CheckIfFileIsInPath("sh.exe") && !CheckSettingsLogic.CheckIfFileIsInPath("sh"),
                                   GitBinFound, GitBinFound_Fix,
                                   _linuxToolsSshNotFound.Text, _linuxToolsSshFound.Text);
        }

        private bool CheckGitCmdValid()
        {
            GitFound.Visible = true;
            if (!CheckSettingsLogic.CanFindGitCmd())
            {
                RenderSettingUnset(GitFound, GitFound_Fix, _gitNotFound.Text);
                return false;
            }

            if (GitVersion.Current < GitVersion.LastSupportedVersion)
            {
                RenderSettingUnset(GitFound, GitFound_Fix, string.Format(_wrongGitVersion.Text, GitVersion.Current, GitVersion.LastRecommendedVersion));
                return false;
            }

            if (GitVersion.Current < GitVersion.LastRecommendedVersion)
            {
                RenderSettingNotRecommended(GitFound, GitFound_Fix, string.Format(_notRecommendedGitVersion.Text, GitVersion.Current, GitVersion.LastRecommendedVersion));
                return false;
            }

            RenderSettingSet(GitFound, GitFound_Fix, string.Format(_gitVersionFound.Text, GitVersion.Current));
            return true;
        }

        private bool CheckDiffToolConfiguration()
        {
            DiffTool.Visible = true;
            if (string.IsNullOrEmpty(CommonLogic.GetGlobalDiffTool()))
            {
                RenderSettingUnset(DiffTool, DiffTool_Fix, _adviceDiffToolConfiguration.Text);
                return false;
            }

            if (EnvUtils.RunningOnWindows())
            {
                if (CommonLogic.IsDiffTool("kdiff3"))
                {
                    string p = GetGlobalSetting("difftool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        RenderSettingUnset(DiffTool, DiffTool_Fix, _kdiffAsDiffConfiguredButNotFound.Text);
                        return false;
                    }

                    RenderSettingSet(DiffTool, DiffTool_Fix, _kdiffAsDiffConfigured.Text);
                    return true;
                }
            }

            string difftool = CommonLogic.GetGlobalDiffTool().ToLowerInvariant();
            RenderSettingSet(DiffTool, DiffTool_Fix, string.Format(_diffToolXConfigured.Text, difftool));
            return true;
        }

        private bool CheckMergeTool()
        {
            MergeTool.Visible = true;
            if (string.IsNullOrEmpty(CommonLogic.GetGlobalMergeTool()))
            {
                RenderSettingUnset(MergeTool, MergeTool_Fix, _configureMergeTool.Text);
                return false;
            }

            string mergetool = CommonLogic.GetGlobalMergeTool().ToLowerInvariant();
            if (EnvUtils.RunningOnWindows())
            {
                if (CommonLogic.IsMergeTool("kdiff3"))
                {
                    string p = GetGlobalSetting("mergetool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        RenderSettingUnset(MergeTool, MergeTool_Fix, _kdiffAsMergeConfiguredButNotFound.Text);
                        return false;
                    }

                    RenderSettingSet(MergeTool, MergeTool_Fix, _kdiffAsMergeConfigured.Text);
                    return true;
                }

                if (mergetool == "p4merge" || mergetool == "tmerge" || mergetool == "meld")
                {
                    string p = GetGlobalSetting($"mergetool.{mergetool}.cmd");
                    if (string.IsNullOrEmpty(p))
                    {
                        RenderSettingUnset(MergeTool, MergeTool_Fix, string.Format(_mergeToolXConfiguredNeedsCmd.Text, mergetool));
                        return false;
                    }

                    RenderSettingSet(MergeTool, MergeTool_Fix, string.Format(_customMergeToolXConfigured.Text, mergetool));
                    return true;
                }
            }

            RenderSettingSet(MergeTool, MergeTool_Fix, string.Format(_mergeToolXConfigured.Text, mergetool));
            return true;
        }

        private bool CheckGlobalUserSettingsValid()
        {
            return RenderSettingSetUnset(() => string.IsNullOrEmpty(GetGlobalSetting(SettingKeyString.UserName)) ||
                                         string.IsNullOrEmpty(GetGlobalSetting(SettingKeyString.UserEmail)),
                                   UserNameSet, UserNameSet_Fix,
                                   _noEmailSet.Text, _emailSet.Text);
        }

        private bool CheckEditorTool()
        {
            string editor = CommonLogic.GetGlobalEditor();
            return !string.IsNullOrEmpty(editor);
        }

        private bool CheckGitExtensionRegistrySettings()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return true;
            }

            ShellExtensionsRegistered.Visible = true;

            if (string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved",
                                                      "{3C16B20A-BA16-4156-916F-0A375ECFFE24}")) ||
                string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot,
                                                      @"*\shellex\ContextMenuHandlers\GitExtensions2", null)) ||
                string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot,
                                                      @"Directory\shellex\ContextMenuHandlers\GitExtensions2", null)) ||
                string.IsNullOrEmpty(CommonLogic.GetRegistryValue(Registry.ClassesRoot,
                                                      @"Directory\Background\shellex\ContextMenuHandlers\GitExtensions2",
                                                      null)))
            {
                // Check if shell extensions are installed
                string path32 = Path.Combine(AppSettings.GetInstallDir(), CommonLogic.GitExtensionsShellEx32Name);
                string path64 = Path.Combine(AppSettings.GetInstallDir(), CommonLogic.GitExtensionsShellEx64Name);
                if (!File.Exists(path32) || (IntPtr.Size == 8 && !File.Exists(path64)))
                {
                    RenderSettingSet(ShellExtensionsRegistered, ShellExtensionsRegistered_Fix, _shellExtNoInstalled.Text);
                    return true;
                }

                RenderSettingUnset(ShellExtensionsRegistered, ShellExtensionsRegistered_Fix, string.Format(_shellExtNeedsToBeRegistered.Text, CommonLogic.GitExtensionsShellEx32Name));
                return false;
            }

            RenderSettingSet(ShellExtensionsRegistered, ShellExtensionsRegistered_Fix, _shellExtRegistered.Text);
            return true;
        }

        private bool CheckGitExtensionsInstall()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return true;
            }

            GitExtensionsInstall.Visible = true;

            var installDir = AppSettings.GetInstallDir();

            if (string.IsNullOrEmpty(installDir))
            {
                RenderSettingUnset(GitExtensionsInstall, GitExtensionsInstall_Fix, _registryKeyGitExtensionsMissing.Text);
                return false;
            }

            if (installDir.EndsWith(".exe"))
            {
                RenderSettingUnset(GitExtensionsInstall, GitExtensionsInstall_Fix, _registryKeyGitExtensionsFaulty.Text);
                return false;
            }

            RenderSettingSet(GitExtensionsInstall, GitExtensionsInstall_Fix, _registryKeyGitExtensionsCorrect.Text);
            return true;
        }

        /// <summary>
        /// Renders settings as configured or not depending on the supplied condition.
        /// </summary>
        private static bool RenderSettingSetUnset(Func<bool> condition, Button settingButton, Button settingFixButton,
            string textSettingUnset, string textSettingGood)
        {
            settingButton.Visible = true;
            if (condition())
            {
                RenderSettingUnset(settingButton, settingFixButton, textSettingUnset);
                return false;
            }

            RenderSettingSet(settingButton, settingFixButton, textSettingGood);
            return true;
        }

        /// <summary>
        /// Renders settings as correctly configured.
        /// </summary>
        private static void RenderSettingSet(Button settingButton, Button settingFixButton, string text)
        {
            settingButton.BackColor = Color.PaleGreen;
            settingButton.ForeColor = Color.DarkGreen;
            settingButton.Text = text;
            settingFixButton.Visible = false;
        }

        /// <summary>
        /// Renders settings as misconfigured.
        /// </summary>
        private static void RenderSettingUnset(Button settingButton, Button settingFixButton, string text)
        {
            settingButton.BackColor = Color.LavenderBlush;
            settingButton.ForeColor = Color.Crimson;
            settingButton.Text = text;
            settingFixButton.Visible = true;
        }

        private static void RenderSettingNotRecommended(Button settingButton, Button settingFixButton, string text)
        {
            settingButton.BackColor = Color.Coral;
            settingButton.ForeColor = Color.Black;
            settingButton.Text = text;
            settingFixButton.Visible = true;
        }

        private void GcmDetectedFix_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/gitextensions/gitextensions/wiki/How-To:-fix-GitCredentialWinStore-missing");
        }
    }
}
