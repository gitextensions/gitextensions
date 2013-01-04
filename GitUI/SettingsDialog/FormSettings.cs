using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Plugin;
using GitUI.SettingsDialog.Plugins;
using ResourceManager.Translation;
using GitUI.SettingsDialog;
using GitUI.SettingsDialog.Pages;

// TODO: fix namespaces
namespace GitUI
{
    public sealed partial class FormSettings : GitModuleForm, ISettingsPageHost
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

        #endregion

        readonly SettingsPageRegistry _settingsPageRegistry = new SettingsPageRegistry();
        readonly CommonLogic _commonLogic;
        readonly CheckSettingsLogic _checkSettingsLogic;
        readonly ChecklistSettingsPage _checklistSettingsPage;
        readonly GitSettingsPage _gitSettingsPage;
        readonly GitExtensionsSettingsPage _gitExtensionsSettingsPage;
        readonly AppearanceSettingsPage _appearanceSettingsPage;
        readonly ColorsSettingsPage _colorsSettingsPage;
        readonly GlobalSettingsSettingsPage _globalSettingsSettingsPage;
        readonly HotkeysSettingsPage _hotkeysSettingsPage;
        readonly LocalSettingsSettingsPage _localSettingsSettingsPage;
        readonly ScriptsSettingsPage _scriptsSettingsPage;
        readonly ShellExtensionSettingsPage _shellExtensionSettingsPage;
        readonly SshSettingsPage _sshSettingsPage;
        readonly StartPageSettingsPage _startPageSettingsPage;

        private FormSettings()
            : this(null)
        { }

        public FormSettings(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            //if form is created for translation purpose
            if (aCommands == null)
                return;
            
            // NEW:

            _commonLogic = new CommonLogic(Module);

            _checkSettingsLogic = new CheckSettingsLogic(_commonLogic, Module);
            _checklistSettingsPage = new ChecklistSettingsPage(_commonLogic, _checkSettingsLogic, Module, this);
            _checkSettingsLogic.ChecklistSettingsPage = _checklistSettingsPage; // TODO
            _settingsPageRegistry.RegisterSettingsPage(_checklistSettingsPage);

            _gitSettingsPage = new GitSettingsPage(_checkSettingsLogic, this);
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
            _checklistSettingsPage.SshSettingsPage = _sshSettingsPage;

            _scriptsSettingsPage = new ScriptsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_scriptsSettingsPage);

            _hotkeysSettingsPage = new HotkeysSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_hotkeysSettingsPage);

            _shellExtensionSettingsPage = new ShellExtensionSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_shellExtensionSettingsPage);

            // register all plugin pages
            _settingsPageRegistry.RegisterPluginSettingsPages();

            settingsTreeViewUserControl1.SetSettingsPages(_settingsPageRegistry);
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            WindowState = FormWindowState.Normal;
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            WindowState = FormWindowState.Normal; // TODO: is that needed?
            LoadSettings();
            Cursor.Current = Cursors.Default;

            GotoPage(ChecklistSettingsPage.GetPageReference());
        }

        private void settingsTreeViewUserControl1_SettingsPageSelected(object sender, SettingsPageSelectedEventArgs e)
        {
            panelCurrentSettingsPage.Controls.Clear();

            var settingsPage = e.SettingsPage;

            if (settingsPage != null)
            {
                panelCurrentSettingsPage.Controls.Add(settingsPage.GuiControl);
                e.SettingsPage.GuiControl.Dock = DockStyle.Fill;

                string title = e.SettingsPage.Title;
                if (e.SettingsPage is PluginSettingsPage)
                {
                    title = "Plugin: " + title;
                }

                labelSettingsPageTitle.Text = title;
                Application.DoEvents();

                Cursor.Current = Cursors.WaitCursor;
                settingsPage.OnPageShown();
                Cursor.Current = Cursors.Default;

                bool isInstantSavePage = settingsPage.IsInstantSavePage;
                labelInstantSaveNotice.Visible = isInstantSavePage;
                buttonOk.Enabled = true;
                buttonCancel.Enabled = true;

                if (e.IsTriggeredByGoto)
                {
                    settingsPage.GuiControl.Focus();
                }
            }
            else
            {
                labelSettingsPageTitle.Text = "[Please select another node]";
            }
        }

        // TODO: see SettingsPageBase.loadingSettings
        ////private bool loadingSettings;

        public void LoadSettings()
        {
            ////loadingSettings = true;

            try
            {
                foreach (var settingsPage in _settingsPageRegistry.GetAllSettingsPages())
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

            ////loadingSettings = false;
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

            foreach (var settingsPage in _settingsPageRegistry.GetAllSettingsPages())
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

        // TODO: needed?
        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            ////Cursor.Current = Cursors.WaitCursor;
            ////if (DialogResult != DialogResult.Abort)
            ////{
            ////    e.Cancel = true;
            ////}
            ////Cursor.Current = Cursors.Default;
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

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
            settingsTreeViewUserControl1.GotoPage(settingsPageReference);
        }

        public void SaveAll()
        {
            Save();
        }

        public void LoadAll()
        {
            LoadSettings();
        }
    }
}
