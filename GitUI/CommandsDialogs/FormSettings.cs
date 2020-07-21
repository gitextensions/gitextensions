using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitCommands.Utils;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SettingsDialog.Plugins;
using GitUI.Properties;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormSettings : GitModuleForm, ISettingsPageHost
    {
        public static readonly string HotkeySettingsName = "Scripts";

        [CanBeNull] private static Type _lastSelectedSettingsPageType;

        #region Translation

        private readonly TranslationString _cantFindGitMessage1 =
            new TranslationString("Without a valid git command some settings may fail to save.\r\nDo you wish to continue?");

        #endregion

        private readonly CommonLogic _commonLogic;
        private readonly string _translatedTitle;

        public CheckSettingsLogic CheckSettingsLogic { get; }

        private IEnumerable<ISettingsPage> SettingsPages => settingsTreeView.SettingsPages;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormSettings()
        {
            InitializeComponent();
        }

        public FormSettings([NotNull] GitUICommands commands, SettingsPageReference initialPage = null)
            : base(commands)
        {
            InitializeComponent();
            _translatedTitle = Text;

            settingsTreeView.SuspendLayout();

#if DEBUG
            buttonDiscard.Visible = true;
#endif

            _commonLogic = new CommonLogic(Module);
            CheckSettingsLogic = new CheckSettingsLogic(_commonLogic);

            var checklistSettingsPage = SettingsPageBase.Create<ChecklistSettingsPage>(this);

            // Git Extensions settings
            settingsTreeView.AddSettingsPage(new GitExtensionsSettingsGroup(), null, Images.GitExtensionsLogo16);
            var gitExtPageRef = GitExtensionsSettingsGroup.GetPageReference();
            settingsTreeView.AddSettingsPage(checklistSettingsPage, gitExtPageRef, icon: null, asRoot: true);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<GeneralSettingsPage>(this), gitExtPageRef, Images.GeneralSettings);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AppearanceSettingsPage>(this), gitExtPageRef, Images.Appearance);
            var appearanceSettingsPage = AppearanceSettingsPage.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ColorsSettingsPage>(this), appearanceSettingsPage, Images.Colors);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AppearanceFontsSettingsPage>(this), appearanceSettingsPage, Images.Font.AdaptLightness());

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<RevisionLinksSettingsPage>(this), gitExtPageRef, Images.Link.AdaptLightness());
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<BuildServerIntegrationSettingsPage>(this), gitExtPageRef, Images.Integration);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ScriptsSettingsPage>(this), gitExtPageRef, Images.Console);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<HotkeysSettingsPage>(this), gitExtPageRef, Images.Hotkey);

            if (EnvUtils.RunningOnWindows())
            {
                settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ShellExtensionSettingsPage>(this), gitExtPageRef, Images.ShellExtensions);
            }

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<AdvancedSettingsPage>(this), gitExtPageRef, Images.AdvancedSettings);
            SettingsPageReference advancedPageRef = AdvancedSettingsPage.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<ConfirmationsSettingsPage>(this), advancedPageRef, Images.BisectGood);

            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<DetailedSettingsPage>(this), gitExtPageRef, Images.Settings);
            var detailedSettingsPage = DetailedSettingsPage.GetPageReference();
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<FormBrowseRepoSettingsPage>(this), detailedSettingsPage, Images.BranchFolder);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<CommitDialogSettingsPage>(this), detailedSettingsPage, Images.CommitSummary);
            settingsTreeView.AddSettingsPage(SettingsPageBase.Create<DiffViewerSettingsPage>(this), detailedSettingsPage, Images.Diff);

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

            if (initialPage == null && _lastSelectedSettingsPageType != null)
            {
                initialPage = new SettingsPageReferenceByType(_lastSelectedSettingsPageType);
            }

            settingsTreeView.GotoPage(initialPage);
            settingsTreeView.ResumeLayout();

            InitializeComplete();
        }

        public static DialogResult ShowSettingsDialog(GitUICommands uiCommands, IWin32Window owner, SettingsPageReference initialPage = null)
        {
            DialogResult result = DialogResult.None;

            using (var form = new FormSettings(uiCommands, initialPage))
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

            if (HasClippedControl())
            {
                var appFont = AppSettings.Font;
                var smallFont = new Font(appFont.FontFamily, emSize: 8);
                ReplaceFont(Controls, appFont, smallFont);
                foreach (var page in settingsTreeView.SettingsPages)
                {
                    ReplaceFont(page?.GuiControl?.Controls, appFont, smallFont);
                }
            }

            return;

            // TODO: C#8 static
            void ReplaceFont(Control.ControlCollection controls, Font oldFont, Font newFont)
            {
                if (controls == null)
                {
                    return;
                }

                foreach (Control control in controls)
                {
                    if (control.Font.Equals(oldFont))
                    {
                        control.Font = newFont;
                    }

                    ReplaceFont(control.Controls, oldFont, newFont);
                }
            }

            bool HasClippedControl()
            {
                foreach (var page in settingsTreeView.SettingsPages)
                {
                    if (ContainsClippedControl(page?.GuiControl?.Controls))
                    {
                        return true;
                    }
                }

                return false;

                bool ContainsClippedControl(Control.ControlCollection controls)
                {
                    if (controls == null)
                    {
                        return false;
                    }

                    foreach (Control control in controls)
                    {
                        if (control.Bottom > panelCurrentSettingsPage.Bottom && control.Visible && control.GetType() != typeof(TableLayoutPanel))
                        {
                            return true;
                        }

                        if (ContainsClippedControl(control.Controls))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                LoadSettings();
            }
        }

        private void OnSettingsPageSelected(object sender, SettingsPageSelectedEventArgs e)
        {
            panelCurrentSettingsPage.Controls.Clear();

            var settingsPage = e.SettingsPage;

            if (settingsPage != null)
            {
                _lastSelectedSettingsPageType = settingsPage.GetType();
            }

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
                using var dialog = new TaskDialog
                {
                    OwnerWindowHandle = Handle,
                    Text = _cantFindGitMessage1.Text,
                    InstructionText = ResourceManager.Strings.GitExecutableNotFound,
                    Caption = Strings.Error,
                    StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No,
                    DefaultButton = TaskDialogDefaultButton.No,
                    Icon = TaskDialogStandardIcon.Error,
                    Cancelable = true,
                };

                if (dialog.Show() == TaskDialogResult.Yes)
                {
                    return false;
                }
            }

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

                return true;
            }
            catch (SaveSettingsException ex) when (ex.InnerException != null)
            {
                using var dialog = new TaskDialog
                {
                    OwnerWindowHandle = Handle,
                    Text = ex.InnerException.Message,
                    InstructionText = "Failed to save all settings",
                    Caption = Strings.Error,
                    StandardButtons = TaskDialogStandardButtons.Ok,
                    Icon = TaskDialogStandardIcon.Error,
                    Cancelable = true,
                };
                dialog.Show();

                return false;
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
