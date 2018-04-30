using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SettingsDialog.Plugins;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormSettings : GitModuleForm, ISettingsPageHost
    {
        #region Translation

        private readonly TranslationString _cantFindGitMessage =
            new TranslationString("The command to run git is not configured correct." + Environment.NewLine +
                "You need to set the correct path to be able to use GitExtensions." + Environment.NewLine +
                Environment.NewLine + "Do you want to set the correct command now? If not Global and Local Settings will not be saved.");

        private readonly TranslationString _cantFindGitMessageCaption =
            new TranslationString("Incorrect path");

        #endregion

        private readonly CommonLogic _commonLogic;
        private readonly string _translatedTitle;

        public CheckSettingsLogic CheckSettingsLogic { get; }

        private IEnumerable<ISettingsPage> SettingsPages => settingsTreeView.SettingsPages;

        private FormSettings()
            : this(null)
        {
        }

        public FormSettings(GitUICommands commands, SettingsPageReference initalPage = null)
            : base(commands)
        {
            InitializeComponent();
            Translate();
            _translatedTitle = Text;

            settingsTreeView.SuspendLayout();

            // if form is created for translation purpose
            if (commands == null)
            {
                return;
            }

#if DEBUG
            buttonDiscard.Visible = true;
#endif

            settingsTreeView.AddSettingsPage(new GitExtensionsSettingsGroup(), null);
            SettingsPageReference gitExtPageRef = GitExtensionsSettingsGroup.GetPageReference();

            _commonLogic = new CommonLogic(Module);
            CheckSettingsLogic = new CheckSettingsLogic(_commonLogic);

            var checklistSettingsPage = SettingsPageBase.Create<ChecklistSettingsPage>(this);
            settingsTreeView.AddSettingsPage(checklistSettingsPage, gitExtPageRef, true); // as root

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GitSettingsPage>(this), gitExtPageRef);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GitExtensionsSettingsPage>(this), gitExtPageRef);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<CommitDialogSettingsPage>(this), gitExtPageRef);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AppearanceSettingsPage>(this), gitExtPageRef);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<RevisionLinksSettingsPage>(this), gitExtPageRef);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ColorsSettingsPage>(this), gitExtPageRef);

            var gitConfigSettingsSettingsPage = SettingsPageBase.Create<GitConfigSettingsPage>(this);
            settingsTreeView.AddSettingsPage(gitConfigSettingsSettingsPage, gitExtPageRef);

            var gitConfigAdvancedSettingsPage = SettingsPageBase.Create<GitConfigAdvancedSettingsPage>(this);
            settingsTreeView.AddSettingsPage(gitConfigAdvancedSettingsPage, gitConfigSettingsSettingsPage.PageReference);

            var buildServerIntegrationSettingsPage = SettingsPageBase.Create<BuildServerIntegrationSettingsPage>(this);
            settingsTreeView.AddSettingsPage(buildServerIntegrationSettingsPage, gitExtPageRef);

            var sshSettingsPage = SettingsPageBase.Create<SshSettingsPage>(this);
            settingsTreeView.AddSettingsPage(sshSettingsPage, gitExtPageRef);
            checklistSettingsPage.SshSettingsPage = sshSettingsPage;

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ScriptsSettingsPage>(this), gitExtPageRef);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<HotkeysSettingsPage>(this), gitExtPageRef);

            if (EnvUtils.RunningOnWindows())
            {
                settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ShellExtensionSettingsPage>(this), gitExtPageRef);
            }

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AdvancedSettingsPage>(this), gitExtPageRef);
            SettingsPageReference advancedPageRef = AdvancedSettingsPage.GetPageReference();

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<DetailedSettingsPage>(this), gitExtPageRef);
            var detailedSettingsPage = DetailedSettingsPage.GetPageReference();

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ConfirmationsSettingsPage>(this), advancedPageRef);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<FormBrowseRepoSettingsPage>(this), detailedSettingsPage);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<DiffViewerSettingsPage>(this), detailedSettingsPage);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ToolbarSettingsPage>(this), detailedSettingsPage);

            settingsTreeView.AddSettingsPage(new PluginsSettingsGroup(), null);
            SettingsPageReference pluginsPageRef = PluginsSettingsGroup.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<PluginRootIntroductionPage>(this), pluginsPageRef, true); // as root
            foreach (var gitPlugin in PluginRegistry.Plugins)
            {
                var settingsPage = PluginSettingsPage.CreateSettingsPageFromPlugin(this, gitPlugin);
                settingsTreeView.AddSettingsPage(settingsPage, pluginsPageRef);
            }

            settingsTreeView.GotoPage(initalPage);
            settingsTreeView.ResumeLayout();

            this.AdjustForDpiScaling();
        }

        public static DialogResult ShowSettingsDialog(GitUICommands uiCommands, IWin32Window owner, SettingsPageReference initalPage = null)
        {
            DialogResult result = DialogResult.None;

            using (var form = new FormSettings(uiCommands, initalPage))
            {
                AppSettings.UsingContainer(form._commonLogic.RepoDistSettingsSet.GlobalSettings, () =>
                {
                    result = form.ShowDialog(owner);
                });
            }

            return result;
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            WindowState = FormWindowState.Normal;
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                LoadSettings();
            }
        }

        private void settingsTreeViewUserControl1_SettingsPageSelected(object sender, SettingsPageSelectedEventArgs e)
        {
            panelCurrentSettingsPage.Controls.Clear();

            var settingsPage = e.SettingsPage;

            if (settingsPage?.GuiControl != null)
            {
                panelCurrentSettingsPage.Controls.Add(settingsPage.GuiControl);
                e.SettingsPage.GuiControl.Dock = DockStyle.Fill;

                string title = e.SettingsPage.GetTitle();
                if (e.SettingsPage is PluginSettingsPage)
                {
                    title = "Plugin: " + title;
                }

                Text = _translatedTitle + " - " + title;
                Application.DoEvents();

                using (WaitCursorScope.Enter())
                {
                    settingsPage.OnPageShown();
                }

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
                Text = _translatedTitle;
            }
        }

        public void LoadSettings()
        {
            try
            {
                foreach (var settingsPage in SettingsPages)
                {
                    settingsPage.LoadSettings();
                }
            }
            catch (Exception e)
            {
                ExceptionUtils.ShowException(e);

                // Bail out before the user saves the incompletely loaded settings
                // and has their day ruined.
                DialogResult = DialogResult.Abort;

                throw;
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                if (Save())
                {
                    Close();
                }
            }
        }

        private bool Save()
        {
            if (!CheckSettingsLogic.CanFindGitCmd())
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

            _commonLogic.ConfigFileSettingsSet.EffectiveSettings.Save();
            _commonLogic.RepoDistSettingsSet.EffectiveSettings.Save();

            if (EnvUtils.RunningOnWindows())
            {
                FormFixHome.CheckHomePath();
            }

            // TODO: this method has a generic sounding name but only saves some specific settings
            AppSettings.SaveSettings();

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

        public static readonly string HotkeySettingsName = "Scripts";

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonDiscard_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                LoadSettings();
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                Save();
            }
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
