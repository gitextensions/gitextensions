using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs.Menus
{
    /// <summary>
    ///  Represents a split button that contains the recent repositories.
    /// </summary>
    internal class WorkingDirectoryToolStripSplitButton : ToolStripSplitButton, ITranslate
    {
        private readonly TranslationString _noWorkingFolderText = new("No working directory");
        private readonly TranslationString _configureWorkingDirMenu = new("&Configure this menu");

        private Func<GitUICommands>? _getUICommands;
        private RepositoryHistoryUIService _dont_use_me_repositoryHistoryUIService;

        // NOTE: This is pretty bad, but we want to share the same look and feel of the menu items defined in the Start menu.
        private StartToolStripMenuItem _startToolStripMenuItem;

        public WorkingDirectoryToolStripSplitButton()
        {
            Name = nameof(WorkingDirectoryToolStripSplitButton);

            Image = Properties.Resources.RepoOpen;
            ImageAlign = ContentAlignment.MiddleLeft;
            ImageTransparentColor = Color.Magenta;
            TextAlign = ContentAlignment.MiddleLeft;
        }

        /// <summary>
        ///  Gets the current instance of the git module.
        /// </summary>
        protected IGitModule Module => UICommands.Module;

        /// <summary>
        ///  Gets the form that is displaying the menu item.
        /// </summary>
        protected static Form? OwnerForm
            => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

        /// <summary>
        ///  Gets the current instance of the <see cref="RepositoryHistoryUIService"/>.
        /// </summary>
        protected RepositoryHistoryUIService RepositoryHistoryUIService
            => _dont_use_me_repositoryHistoryUIService ?? throw new InvalidOperationException("The button is not initialized");

        /// <summary>
        ///  Gets the current instance of the UI commands.
        /// </summary>
        protected GitUICommands UICommands
            => (_getUICommands ?? throw new InvalidOperationException("The button is not initialized")).Invoke();

        /// <summary>
        ///  Initializes the menu item.
        /// </summary>
        /// <param name="getUICommands">The method that returns the current instance of UI commands.</param>
        public void Initialize(Func<GitUICommands> getUICommands, RepositoryHistoryUIService repositoryHistoryUIService,
                               StartToolStripMenuItem startToolStripMenuItem)
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);

            _getUICommands = getUICommands;
            _dont_use_me_repositoryHistoryUIService = repositoryHistoryUIService;
            _startToolStripMenuItem = startToolStripMenuItem;
        }

        protected override void OnButtonClick(EventArgs e)
        {
            base.OnButtonClick(e);
            ShowDropDown();
        }

        protected override void OnDropDownShow(EventArgs e)
        {
            DropDown.SuspendLayout();
            DropDownItems.Clear();

            ToolStripMenuItem tsmiCategorisedRepos = new(_startToolStripMenuItem.FavouriteRepositoriesMenuItem.Text, _startToolStripMenuItem.FavouriteRepositoriesMenuItem.Image);
            RepositoryHistoryUIService.PopulateFavouriteRepositoriesMenu(tsmiCategorisedRepos);
            if (tsmiCategorisedRepos.DropDownItems.Count > 0)
            {
                DropDownItems.Add(tsmiCategorisedRepos);
            }

            RepositoryHistoryUIService.PopulateRecentRepositoriesMenu(this);

            DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuOpenLocalRepository = new(_startToolStripMenuItem.OpenRepositoryMenuItem.Text, _startToolStripMenuItem.OpenRepositoryMenuItem.Image)
            {
                ShortcutKeys = _startToolStripMenuItem.OpenRepositoryMenuItem.ShortcutKeys
            };
            mnuOpenLocalRepository.Click += (s, e) => _startToolStripMenuItem.OpenRepositoryMenuItem.PerformClick();
            DropDownItems.Add(mnuOpenLocalRepository);

            ToolStripMenuItem mnuRecentReposSettings = new(_configureWorkingDirMenu.Text);
            mnuRecentReposSettings.Click += (hs, he) =>
            {
                using (FormRecentReposSettings frm = new())
                {
                    frm.ShowDialog(OwnerForm);
                }

                RefreshContent();
            };

            DropDownItems.Add(mnuRecentReposSettings);

            DropDown.ResumeLayout();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                _startToolStripMenuItem.OpenRepositoryMenuItem.PerformClick();
            }
        }

        /// <summary>Updates the text shown on the combo button itself.</summary>
        public void RefreshContent()
        {
            string path = Module.WorkingDir;

            // it appears at times Module.WorkingDir path is an empty string, this caused issues like #4874
            if (string.IsNullOrWhiteSpace(path))
            {
                Text = _noWorkingFolderText.Text;
                return;
            }

            IList<Repository> recentRepositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
                () => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));

            List<RecentRepoInfo> pinnedRepos = new();
            RecentRepoSplitter splitter = new()
            {
                MeasureFont = Font,
            };

            splitter.SplitRecentRepos(recentRepositoryHistory, pinnedRepos, pinnedRepos);

            RecentRepoInfo ri = pinnedRepos.Find(e => e.Repo.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

            Text = PathUtil.GetDisplayPath(ri?.Caption ?? path);

            if (AppSettings.RecentReposComboMinWidth > 0)
            {
                AutoSize = false;
                int captionWidth = TextRenderer.MeasureText(Text, Font).Width;
                captionWidth = captionWidth + DropDownButtonWidth + 5;
                Width = Math.Max(AppSettings.RecentReposComboMinWidth, captionWidth);
            }
            else
            {
                AutoSize = true;
            }
        }

        void ITranslate.AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields("FormBrowse", this, translation);
        }

        void ITranslate.TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields("FormBrowse", this, translation);
        }
    }
}
