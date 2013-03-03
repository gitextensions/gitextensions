using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SettingsDialog.Plugins;
using GitUI.Plugin;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
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

        readonly CommonLogic _commonLogic;
        readonly CheckSettingsLogic _checkSettingsLogic;
        private IEnumerable<ISettingsPage> SettingsPages { get { return settingsTreeView.SettingsPages; } }

        private FormSettings()
            : this(null)
        { }

        public FormSettings(GitUICommands aCommands, SettingsPageReference initalPage = null)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            settingsTreeView.SuspendLayout();

            //if form is created for translation purpose
            if (aCommands == null)
                return;

            settingsTreeView.AddSettingsPage(new GitExtensionsSettingsGroup(), null);
            SettingsPageReference gitExtPageRef = GitExtensionsSettingsGroup.GetPageReference();
            
            _commonLogic = new CommonLogic(Module);

            _checkSettingsLogic = new CheckSettingsLogic(_commonLogic, Module);

            var checklistSettingsPage = new ChecklistSettingsPage(_commonLogic, _checkSettingsLogic, Module, this);
            settingsTreeView.AddSettingsPage(checklistSettingsPage, gitExtPageRef, true); // as root

            settingsTreeView.AddSettingsPage(new GitSettingsPage(_checkSettingsLogic, this), gitExtPageRef);

            settingsTreeView.AddSettingsPage(new GitExtensionsSettingsPage(), gitExtPageRef);

            settingsTreeView.AddSettingsPage(new AppearanceSettingsPage(), gitExtPageRef);

            settingsTreeView.AddSettingsPage(new ColorsSettingsPage(), gitExtPageRef);

            settingsTreeView.AddSettingsPage(new StartPageSettingsPage(), gitExtPageRef);

            var globalSettingsSettingsPage = new GlobalSettingsSettingsPage(_commonLogic, _checkSettingsLogic, Module);
            settingsTreeView.AddSettingsPage(globalSettingsSettingsPage, gitExtPageRef);

            var localSettingsSettingsPage = new LocalSettingsSettingsPage(_commonLogic, _checkSettingsLogic, Module);
            settingsTreeView.AddSettingsPage(localSettingsSettingsPage, gitExtPageRef);

            var _sshSettingsPage = new SshSettingsPage(_commonLogic);
            settingsTreeView.AddSettingsPage(_sshSettingsPage, gitExtPageRef);
            checklistSettingsPage.SshSettingsPage = _sshSettingsPage;

            settingsTreeView.AddSettingsPage(new ScriptsSettingsPage(), gitExtPageRef);

            settingsTreeView.AddSettingsPage(new HotkeysSettingsPage(), gitExtPageRef);

            settingsTreeView.AddSettingsPage(new ShellExtensionSettingsPage(), gitExtPageRef);

            settingsTreeView.AddSettingsPage(new AdvancedSettingsPage(), gitExtPageRef);
            SettingsPageReference advancedPageRef = AdvancedSettingsPage.GetPageReference();

            settingsTreeView.AddSettingsPage(new ConfirmationsSettingsPage(), advancedPageRef);

            settingsTreeView.AddSettingsPage(new PluginsSettingsGroup(), null);
            SettingsPageReference pluginsPageRef = PluginsSettingsGroup.GetPageReference();
            settingsTreeView.AddSettingsPage(new PluginRootIntroductionPage(), pluginsPageRef, true); // as root
            foreach (var gitPlugin in LoadedPlugins.Plugins)
            {
                var settingsPage = PluginSettingsPage.CreateSettingsPageFromPlugin(gitPlugin);
                settingsTreeView.AddSettingsPage(settingsPage, pluginsPageRef);
            }

            settingsTreeView.GotoPage(initalPage);
            settingsTreeView.ResumeLayout();
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
            LoadSettings();
            Cursor.Current = Cursors.Default;
        }

        private void settingsTreeViewUserControl1_SettingsPageSelected(object sender, SettingsPageSelectedEventArgs e)
        {
            panelCurrentSettingsPage.Controls.Clear();

            var settingsPage = e.SettingsPage;

            if (settingsPage != null && settingsPage.GuiControl != null)
            {
                panelCurrentSettingsPage.Controls.Add(settingsPage.GuiControl);
                e.SettingsPage.GuiControl.Dock = DockStyle.Fill;

                string title = e.SettingsPage.GetTitle();
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
                foreach (var settingsPage in SettingsPages)
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

            foreach (var settingsPage in SettingsPages)
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
            settingsTreeView.GotoPage(settingsPageReference);
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
