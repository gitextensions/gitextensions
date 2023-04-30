using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs.Menus
{
    internal partial class StartToolStripMenuItem : ToolStripMenuItemEx
    {
        private readonly RepositoryHistoryUIService _repositoryHistoryUIService = new();

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;
        public event EventHandler RecentRepositoriesCleared;

        public StartToolStripMenuItem()
        {
            InitializeComponent();

            _repositoryHistoryUIService.GitModuleChanged += repositoryHistoryUIService_GitModuleChanged;
        }

        internal ToolStripMenuItem OpenRepositoryMenuItem => openToolStripMenuItem;
        internal ToolStripMenuItem FavouriteRepositoriesMenuItem => tsmiFavouriteRepositories;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                _repositoryHistoryUIService.GitModuleChanged -= repositoryHistoryUIService_GitModuleChanged;
            }

            base.Dispose(disposing);
        }

        public override void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
        {
            openToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKey(hotkeys, (int)FormBrowse.Command.OpenRepo);

            base.RefreshShortcutKeys(hotkeys);
        }

        private void CloneToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCloneDialog(OwnerForm, string.Empty, false, GitModuleChanged);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            OwnerForm?.Close();
        }

        private void InitNewRepositoryToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(OwnerForm, gitModuleChanged: GitModuleChanged);
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitModule? module = FormOpenDirectory.OpenModule(OwnerForm, UICommands.Module);
            if (module is not null)
            {
                GitModuleChanged?.Invoke(OwnerForm, new GitModuleEventArgs(module));
            }
        }

        private void repositoryHistoryUIService_GitModuleChanged(object? sender, GitModuleEventArgs e)
        {
            GitModuleChanged?.Invoke(this, e);
        }

        private void tsmiFavouriteRepositories_DropDownOpening(object sender, EventArgs e)
        {
            tsmiFavouriteRepositories.DropDown.SuspendLayout();
            tsmiFavouriteRepositories.DropDownItems.Clear();
            _repositoryHistoryUIService.PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories);
            tsmiFavouriteRepositories.DropDown.ResumeLayout();
        }

        private void tsmiRecentRepositories_DropDownOpening(object sender, EventArgs e)
        {
            tsmiRecentRepositories.DropDown.SuspendLayout();
            tsmiRecentRepositories.DropDownItems.Clear();
            _repositoryHistoryUIService.PopulateRecentRepositoriesMenu(tsmiRecentRepositories);
            if (tsmiRecentRepositories.DropDownItems.Count < 1)
            {
                return;
            }

            tsmiRecentRepositories.DropDownItems.Add(clearRecentRepositoriesListToolStripMenuItem);
            ////TranslateItem(tsmiRecentRepositoriesClear.Name, tsmiRecentRepositoriesClear);
            tsmiRecentRepositories.DropDownItems.Add(tsmiRecentRepositoriesClear);
            tsmiRecentRepositories.DropDown.ResumeLayout();
        }

        private void tsmiRecentRepositoriesClear_Click(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = Array.Empty<Repository>();
                await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(repositoryHistory);

                await this.SwitchToMainThreadAsync();
                RecentRepositoriesCleared?.Invoke(sender, e);
            });
        }
    }
}
