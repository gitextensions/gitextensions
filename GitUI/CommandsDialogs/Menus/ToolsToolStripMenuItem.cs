using GitCommands;
using GitCommands.Utils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Infrastructure;
using GitUI.Shells;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.Menus
{
    internal partial class ToolsToolStripMenuItem : ToolStripMenuItemEx
    {
        public event EventHandler<SettingsChangedEventArgs> SettingsChanged;

        public ToolsToolStripMenuItem()
        {
            InitializeComponent();

            gitBashToolStripMenuItem.Tag = new ShellProvider().GetShell(BashShell.ShellName);

            if (!EnvUtils.RunningOnWindows())
            {
                toolStripSeparator6.Visible = false;
                PuTTYToolStripMenuItem.Visible = false;
            }
        }

        public override void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
        {
            gitBashToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKey(hotkeys, (int)FormBrowse.Command.GitBash);
            gitGUIToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKey(hotkeys, (int)FormBrowse.Command.GitGui);
            kGitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKey(hotkeys, (int)FormBrowse.Command.GitGitK);
            settingsToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKey(hotkeys, (int)FormBrowse.Command.OpenSettings);

            base.RefreshShortcutKeys(hotkeys);
        }

        public override void RefreshState(bool bareRepository)
        {
            gitGUIToolStripMenuItem.Enabled = !bareRepository;

            base.RefreshState(bareRepository);
        }

        private void GitcommandLogToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormGitCommandLog.ShowOrActivate(OwnerForm);
        }

        private void GitGuiToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.Module.RunGui();
        }

        private void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.Module.RunGitK();
        }

        private void StartAuthenticationAgentToolStripMenuItemClick(object sender, EventArgs e)
        {
            PuttyHelpers.StartPageant(UICommands.Module.WorkingDir);
        }

        private void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            PuttyHelpers.StartPuttygen(UICommands.Module.WorkingDir);
        }

        private void OnShowSettingsClick(object sender, EventArgs e)
        {
            string translation = AppSettings.Translation;
            CommitInfoPosition commitInfoPosition = AppSettings.CommitInfoPosition;

            UICommands.StartSettingsDialog(OwnerForm);

            SettingsChanged?.Invoke(sender, new(translation, commitInfoPosition));
        }

        private void gitBashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gitBashToolStripMenuItem.Tag is not IShellDescriptor shell)
            {
                return;
            }

            try
            {
                Validates.NotNull(shell.ExecutablePath);

                Executable executable = new(shell.ExecutablePath, UICommands.Module.WorkingDir);
                executable.Start(createWindow: true, throwOnErrorExit: false); // throwOnErrorExit would redirect the output
            }
            catch (Exception exception)
            {
                MessageBoxes.FailedToRunShell(OwnerForm, shell.Name, exception);
            }
        }
    }
}
