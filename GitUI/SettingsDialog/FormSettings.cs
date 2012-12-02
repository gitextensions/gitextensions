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
using GitUI.Script;
using Gravatar;
using Microsoft.Win32;
using ResourceManager.Translation;
using GitUI.SettingsDialog;
using GitUI.SettingsDialog.Pages;

namespace GitUI
{
    public sealed partial class FormSettings : GitModuleForm
    {
        #region Translation

        private readonly TranslationString _loadingSettingsFailed =
            new TranslationString("Could not load settings.");

        private readonly TranslationString _cantFindGitMessage =
            new TranslationString("The command to run git is not configured correct." + Environment.NewLine +
                "You need to set the correct path to be able to use GitExtensions." + Environment.NewLine +
                Environment.NewLine + "Do you want to set the correct command now? If not Global and Local Settings will not be saved.");

        private readonly TranslationString _cantFindGitMessageCaption =
            new TranslationString("Incorrect path");

        private readonly TranslationString _linuxToolsShNotFound =
            new TranslationString("The path to linux tools (sh) could not be found automatically." + Environment.NewLine +
                "Please make sure there are linux tools installed (through msysgit or cygwin) or set the correct path manually.");

        private readonly TranslationString _linuxToolsShNotFoundCaption =
            new TranslationString("Locate linux tools");

        private readonly TranslationString _shCanBeRun =
            new TranslationString("Command sh can be run using: {0}sh");

        private readonly TranslationString _shCanBeRunCaption =
            new TranslationString("Locate linux tools");

        private readonly TranslationString _puttyFoundAuto =
            new TranslationString("All paths needed for PuTTY could be automatically found and are set.");

        private readonly TranslationString _puttyFoundAutoCaption =
            new TranslationString("PuTTY");

        #endregion

        SettingsPageRegistry _settingsPageRegistry = new SettingsPageRegistry();
        CommonLogic _commonLogic;
        CheckSettingsLogic _checkSettingsLogic;
        ChecklistSettingsPage _checklistSettingsPage;
        GitSettingsPage _gitSettingsPage;
        GitExtensionsSettingsPage _gitExtensionsSettingsPage;
        AppearanceSettingsPage _appearanceSettingsPage;
        ColorsSettingsPage _colorsSettingsPage;
        GlobalSettingsSettingsPage _globalSettingsSettingsPage;
        HotkeysSettingsPage _hotkeysSettingsPage;
        LocalSettingsSettingsPage _localSettingsSettingsPage;
        ScriptsSettingsPage _scriptsSettingsPage;
        ShellExtensionSettingsPage _shellExtensionSettingsPage;
        SshSettingsPage _sshSettingsPage;
        StartPageSettingsPage _startPageSettingsPage;

        private FormSettings()
            : this(null)
        { }

        Panel settingsPagePanel;

        public FormSettings(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            // NEW:

            _commonLogic = new CommonLogic(Module);

            _checkSettingsLogic = new CheckSettingsLogic(_commonLogic, Module);
            _checklistSettingsPage = new ChecklistSettingsPage(_commonLogic, _checkSettingsLogic, Module);
            _checkSettingsLogic.ChecklistSettingsPage = _checklistSettingsPage; // TODO
            _settingsPageRegistry.RegisterSettingsPage(_checklistSettingsPage);

            _gitSettingsPage = new GitSettingsPage(_checkSettingsLogic);
            _settingsPageRegistry.RegisterSettingsPage(_gitSettingsPage);

            _gitExtensionsSettingsPage = new GitExtensionsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_gitExtensionsSettingsPage);

            _appearanceSettingsPage = new AppearanceSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_appearanceSettingsPage);

            _colorsSettingsPage = new ColorsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_colorsSettingsPage);

            _startPageSettingsPage = new StartPageSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_startPageSettingsPage);

            _globalSettingsSettingsPage = new GlobalSettingsSettingsPage(_commonLogic, _checkSettingsLogic, Module);
            _settingsPageRegistry.RegisterSettingsPage(_globalSettingsSettingsPage);

            _localSettingsSettingsPage = new LocalSettingsSettingsPage(_commonLogic, _checkSettingsLogic, Module);
            _settingsPageRegistry.RegisterSettingsPage(_localSettingsSettingsPage);

            _sshSettingsPage = new SshSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_sshSettingsPage);

            _scriptsSettingsPage = new ScriptsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_scriptsSettingsPage);
            
            _hotkeysSettingsPage = new HotkeysSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_hotkeysSettingsPage);
            
            _shellExtensionSettingsPage = new ShellExtensionSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_shellExtensionSettingsPage);
            
            settingsTreeViewUserControl1.SetSettingsPages(_settingsPageRegistry);

            // todo: alter this when all tab pages are converted
            //this.tableLayoutPanel3.Controls.Add(this.tabControl1, 1, 0);
            tableLayoutPanel3.Controls.Remove(tabControl1);
            settingsPagePanel = new Panel();
            settingsPagePanel.Dock = DockStyle.Fill;
            tableLayoutPanel3.Controls.Add(settingsPagePanel, 1, 1);
        }

        private void settingsTreeViewUserControl1_SettingsPageSelected(object sender, SettingsPageSelectedEventArgs e)
        {
            settingsPagePanel.Controls.Clear();
            if (e.SettingsPage == null)
            {
                settingsPagePanel.Controls.Add(tabControl1);
                labelSettingsPageTitle.Text = "(TabControl to be migrated)";
            }
            else
            {
                var settingsPage = e.SettingsPage;

                settingsPagePanel.Controls.Add(settingsPage.GuiControl);
                e.SettingsPage.GuiControl.Dock = DockStyle.Fill;
                
                labelSettingsPageTitle.Text = e.SettingsPage.Text;
                Application.DoEvents();

                Cursor.Current = Cursors.WaitCursor;
                settingsPage.OnPageShown();
                Cursor.Current = Cursors.Default;

                bool isInstantApplyPage = settingsPage.IsInstantApplyPage;
                buttonApply.Enabled = !isInstantApplyPage;
                buttonDiscard.Enabled = !isInstantApplyPage;
            }
        }

        private bool loadingSettings;

        public void LoadSettings()
        {
            loadingSettings = true;
            try
            {
                foreach (var settingsPage in _settingsPageRegistry.GetSettingsPages())
                {
                    settingsPage.LoadSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _loadingSettingsFailed.Text + Environment.NewLine + ex);

                // Bail out before the user saves the incompletely loaded settings
                // and has their day ruined.
                DialogResult = DialogResult.Abort;
            }
            loadingSettings = false;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Save())
            {
                Close();
            }
            Cursor.Current = Cursors.Default;
        }

        private bool Save()
        {
            if (!_checkSettingsLogic.CanFindGitCmd())
            {
                if (MessageBox.Show(this, _cantFindGitMessage.Text, _cantFindGitMessageCaption.Text,
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    return false;
                }
            }

            foreach (var settingsPage in _settingsPageRegistry.GetSettingsPages())
            {
                settingsPage.SaveSettings();
            }

            if (Settings.RunningOnWindows())
                FormFixHome.CheckHomePath();

            // TODO: to which settings page does this belong?
            GitCommandHelpers.SetEnvironmentVariable(true);

            // TODO: this method has a generic sounding name but only saves some specific settings
            Settings.SaveSettings();

            return true;
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            WindowState = FormWindowState.Normal;
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Save();
            LoadSettings();
            _checklistSettingsPage.CheckSettings();
            Cursor.Current = Cursors.Default;
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            WindowState = FormWindowState.Normal;
            LoadSettings();
            _checklistSettingsPage.CheckSettings();
            WindowState = FormWindowState.Normal;
            Cursor.Current = Cursors.Default;
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            ////Cursor.Current = Cursors.WaitCursor;
            ////if (DialogResult != DialogResult.Abort)
            ////{
            ////    e.Cancel = true;
            ////}
            ////Cursor.Current = Cursors.Default;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tc = (TabControl)sender;

            if (tc.SelectedTab == tpHotkeys)
                controlHotkeys.ReloadSettings();
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "Scripts";

        internal enum Commands
        {
            NothingYet
        }

        #endregion

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonDiscard_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadSettings();
            Cursor.Current = Cursors.Default;            
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Save();
            Cursor.Current = Cursors.Default;
        }
    }
}
