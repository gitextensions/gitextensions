using GitCommands;
using GitCommands.Settings;
using GitCommands.Utils;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SettingsDialog.Plugins;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormSettings : GitModuleForm, ISettingsPageHost
    {
        public static readonly string HotkeySettingsName = "Scripts";

        #region Translation

        private readonly TranslationString _cantSaveSettings = new("Failed to save all settings");

        #endregion

        private static Type? _lastSelectedSettingsPageType;
        private readonly CommonLogic _commonLogic;
        private readonly string _translatedTitle;
        private SettingsPageReference? _initialPage;
        private bool _saved = false;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormSettings()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormSettings(GitUICommands commands, SettingsPageReference? initialPage = null)
            : base(commands)
        {
            InitializeComponent();

            _translatedTitle = Text;

#if DEBUG
            buttonDiscard.Visible = true;
#endif

            _commonLogic = new CommonLogic(Module);

            CheckSettingsLogic = new CheckSettingsLogic(_commonLogic);

            InitializeComplete();
            _initialPage = initialPage;
        }

        public CheckSettingsLogic CheckSettingsLogic { get; }

        private IEnumerable<ISettingsPage> SettingsPages => settingsTreeView.SettingsPages;

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
            settingsTreeView.GotoPage(settingsPageReference);
        }

        public void LoadAll()
        {
            LoadSettings();
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

        public void SaveAll()
        {
            Save();
        }

        public static DialogResult ShowSettingsDialog(GitUICommands uiCommands, IWin32Window? owner, SettingsPageReference? initialPage = null)
        {
            DialogResult result = DialogResult.None;

            using FormSettings form = new(uiCommands, initialPage);
            AppSettings.UsingContainer(form._commonLogic.RepoDistSettingsSet.GlobalSettings, () =>
            {
                result = form.ShowDialog(owner);
            });

            return result;
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            settingsTreeView.SuspendLayout();

            ChecklistSettingsPage checklistSettingsPage = SettingsPageBase.Create<ChecklistSettingsPage>(this);

            // Git Extensions settings
            settingsTreeView.AddSettingsPage(new GitExtensionsSettingsGroup(), null, Images.GitExtensionsLogo16);
            var gitExtPageRef = GitExtensionsSettingsGroup.GetPageReference();
            settingsTreeView.AddSettingsPage(checklistSettingsPage, gitExtPageRef, icon: null, asRoot: true);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GeneralSettingsPage>(this), gitExtPageRef, Images.GeneralSettings);

            // >> Appearance
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AppearanceSettingsPage>(this), gitExtPageRef, Images.Appearance);
            var appearanceSettingsPage = AppearanceSettingsPage.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ColorsSettingsPage>(this), appearanceSettingsPage, Images.Colors);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AppearanceFontsSettingsPage>(this), appearanceSettingsPage, Images.Font.AdaptLightness());
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ConsoleStyleSettingsPage>(this), appearanceSettingsPage, Images.Console);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<RevisionLinksSettingsPage>(this), gitExtPageRef, Images.Link.AdaptLightness());

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<BuildServerIntegrationSettingsPage>(this), gitExtPageRef, Images.Integration);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ScriptsSettingsPage>(this), gitExtPageRef, Images.Console);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<HotkeysSettingsPage>(this), gitExtPageRef, Images.Hotkey);

            if (EnvUtils.RunningOnWindows())
            {
                settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ShellExtensionSettingsPage>(this), gitExtPageRef, Images.ShellExtensions);
            }

            // >> Advanced
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AdvancedSettingsPage>(this), gitExtPageRef, Images.AdvancedSettings);
            SettingsPageReference advancedPageRef = AdvancedSettingsPage.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ConfirmationsSettingsPage>(this), advancedPageRef, Images.BisectGood);

            // >> Detailed
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<DetailedSettingsPage>(this), gitExtPageRef, Images.Settings);
            var detailedSettingsPage = DetailedSettingsPage.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<FormBrowseRepoSettingsPage>(this), detailedSettingsPage, Images.BranchFolder);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<CommitDialogSettingsPage>(this), detailedSettingsPage, Images.CommitSummary);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<DiffViewerSettingsPage>(this), detailedSettingsPage, Images.Diff);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<BlameViewerSettingsPage>(this), detailedSettingsPage, Images.Blame);

            var sshSettingsPage = SettingsPageBase.Create<SshSettingsPage>(this);
            settingsTreeView.AddSettingsPage(sshSettingsPage, gitExtPageRef, Images.Key);
            checklistSettingsPage.SshSettingsPage = sshSettingsPage;

            // Git settings
            settingsTreeView.AddSettingsPage(new GitSettingsGroup(), null, Images.GitLogo16);
            var gitPageRef = GitSettingsGroup.GetPageReference();

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GitSettingsPage>(this), gitPageRef, Images.FolderOpen);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GitConfigSettingsPage>(this), gitPageRef, Images.GeneralSettings);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GitConfigAdvancedSettingsPage>(this), gitPageRef, Images.AdvancedSettings);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GitRootIntroductionPage>(this), gitPageRef, icon: null, asRoot: true);

            // Plugins settings
            settingsTreeView.AddSettingsPage(new PluginsSettingsGroup(), null, Images.Plugin);
            SettingsPageReference pluginsPageRef = PluginsSettingsGroup.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<PluginRootIntroductionPage>(this), pluginsPageRef, icon: null, asRoot: true);

            lock (PluginRegistry.Plugins)
            {
                var pluginEntries = PluginRegistry.Plugins
                    .Where(p => p.HasSettings)
                    .Select(plugin => (Plugin: plugin, Page: PluginSettingsPage.CreateSettingsPageFromPlugin(this, plugin)))
                    .OrderBy(entry => entry.Page.GetTitle(), StringComparer.CurrentCultureIgnoreCase);

                foreach (var entry in pluginEntries)
                {
                    settingsTreeView.AddSettingsPage(entry.Page, pluginsPageRef, entry.Plugin.Icon as Bitmap);
                }
            }

            settingsTreeView.ResumeLayout();
        }

        private void OnSettingsPageSelected(object sender, SettingsPageSelectedEventArgs e)
        {
            panelCurrentSettingsPage.Controls.Clear();

            var settingsPage = e.SettingsPage;

            _lastSelectedSettingsPageType = settingsPage.GetType();

            if (settingsPage.GuiControl is not null)
            {
                panelCurrentSettingsPage.Controls.Add(settingsPage.GuiControl);
                settingsPage.GuiControl.Dock = DockStyle.Fill;

                string title = settingsPage.GetTitle();
                if (settingsPage is PluginSettingsPage)
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

        private bool Save()
        {
            try
            {
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

                _saved = true;

                return true;
            }
            catch (SaveSettingsException ex) when (ex.InnerException is not null)
            {
                TaskDialogPage page = new()
                {
                    Text = ex.InnerException.Message,
                    Heading = _cantSaveSettings.Text,
                    Caption = TranslatedStrings.Error,
                    Buttons = { TaskDialogButton.OK },
                    Icon = TaskDialogIcon.Error,
                    AllowCancel = true,
                    SizeToContent = true
                };
                TaskDialog.ShowDialog(Handle, page);

                return false;
            }
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                LoadSettings();

                if (_initialPage is null && _lastSelectedSettingsPageType is not null)
                {
                    _initialPage = new SettingsPageReferenceByType(_lastSelectedSettingsPageType);
                }

                settingsTreeView.GotoPage(_initialPage);

                settingsTreeView.Focus();
            }
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_saved)
            {
                DialogResult = DialogResult.OK;
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
    }
}
