using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUIPluginInterfaces;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs.Menus
{
    internal partial class StartToolStripMenuItem : ToolStripMenuItemEx
    {
        private IRepositoryHistoryUIService? _repositoryHistoryUIService;

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;
        public event EventHandler RecentRepositoriesCleared;

        public StartToolStripMenuItem()
        {
            InitializeComponent();
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

                if (_repositoryHistoryUIService is not null)
                {
                    _repositoryHistoryUIService.GitModuleChanged -= repositoryHistoryUIService_GitModuleChanged;
                }
            }

            base.Dispose(disposing);
        }

        public override void OnInitialized()
        {
            base.OnInitialized();

            _repositoryHistoryUIService = UICommands.GetRequiredService<IRepositoryHistoryUIService>();
            _repositoryHistoryUIService.GitModuleChanged += repositoryHistoryUIService_GitModuleChanged;
        }

        public override void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
        {
            openToolStripMenuItem.ShortcutKeyDisplayString = hotkeys.GetShortcutDisplay(FormBrowse.Command.OpenRepo);

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
            IGitModule? module = FormOpenDirectory.OpenModule(OwnerForm, UICommands.Module);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(Array.Empty<Repository>()));
            RecentRepositoriesCleared?.Invoke(sender, e);
        }
    }
}
