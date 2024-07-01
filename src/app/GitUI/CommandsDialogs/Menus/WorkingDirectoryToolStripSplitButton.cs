#nullable enable

using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.BrowseDialog;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.Menus;

/// <summary>
///  Represents a split button that contains the recent repositories.
/// </summary>
internal class WorkingDirectoryToolStripSplitButton : ToolStripSplitButton, ITranslate
{
    private readonly TranslationString _noWorkingFolderText = new("No working directory");
    private readonly TranslationString _configureWorkingDirMenu = new("Co&nfigure this menu...");

    private Func<IGitUICommands>? _getUICommands;
    private IRepositoryHistoryUIService? _repositoryHistoryUIService;

    // NOTE: This is pretty bad, but we want to share the same look and feel of the menu items defined in the Start menu.
    private StartToolStripMenuItem? _startToolStripMenuItem;
    private ToolStripMenuItem? _closeToolStripMenuItem;

    public WorkingDirectoryToolStripSplitButton()
    {
        Name = nameof(WorkingDirectoryToolStripSplitButton);

        Image = Properties.Resources.RepoOpen;
        ImageAlign = ContentAlignment.MiddleLeft;
        ImageTransparentColor = Color.Magenta;
        TextAlign = ContentAlignment.MiddleLeft;
    }

    /// <summary>
    ///  Gets the form that is displaying the menu item.
    /// </summary>
    private static Form? OwnerForm
        => Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

    /// <summary>
    ///  Gets the current instance of the UI commands.
    /// </summary>
    private IGitUICommands UICommands
        => (_getUICommands ?? throw new InvalidOperationException("The button is not initialized")).Invoke();

    /// <summary>
    ///  Initializes the menu item.
    /// </summary>
    /// <param name="getUICommands">The method that returns the current instance of UI commands.</param>
    public void Initialize(Func<IGitUICommands> getUICommands, IRepositoryHistoryUIService repositoryHistoryUIService,
                           StartToolStripMenuItem startToolStripMenuItem, ToolStripMenuItem closeToolStripMenuItem)
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);

        _getUICommands = getUICommands;
        _repositoryHistoryUIService = repositoryHistoryUIService;
        _startToolStripMenuItem = startToolStripMenuItem;
        _closeToolStripMenuItem = closeToolStripMenuItem;
    }

    protected override void OnButtonClick(EventArgs e)
    {
        base.OnButtonClick(e);

        ShowDropDown();
    }

    protected override void OnDropDownShow(EventArgs e)
    {
        Assumes.NotNull(_repositoryHistoryUIService);
        Assumes.NotNull(_startToolStripMenuItem);
        Assumes.NotNull(_closeToolStripMenuItem);

        base.OnDropDownShow(e);

        DropDown.SuspendLayout();
        DropDownItems.Clear();

        ToolStripMenuItem tsmiCategorisedRepos = new(_startToolStripMenuItem.FavouriteRepositoriesMenuItem.Text, _startToolStripMenuItem.FavouriteRepositoriesMenuItem.Image);
        _repositoryHistoryUIService.PopulateFavouriteRepositoriesMenu(tsmiCategorisedRepos);
        if (tsmiCategorisedRepos.DropDownItems.Count > 0)
        {
            DropDownItems.Add(tsmiCategorisedRepos);
        }

        _repositoryHistoryUIService.PopulateRecentRepositoriesMenu(this);

        DropDownItems.Add(new ToolStripSeparator());

        ToolStripMenuItem mnuOpenLocalRepository = new(_startToolStripMenuItem.OpenRepositoryMenuItem.Text, _startToolStripMenuItem.OpenRepositoryMenuItem.Image)
        {
            ShortcutKeyDisplayString = _startToolStripMenuItem.OpenRepositoryMenuItem.ShortcutKeyDisplayString
        };
        mnuOpenLocalRepository.Click += (s, e) => _startToolStripMenuItem.OpenRepositoryMenuItem.PerformClick();
        DropDownItems.Add(mnuOpenLocalRepository);

        ToolStripMenuItem mnuCloseRepo = new(_closeToolStripMenuItem.Text);
        mnuCloseRepo.ShortcutKeyDisplayString = _closeToolStripMenuItem.ShortcutKeyDisplayString;
        mnuCloseRepo.Click += (hs, he) => _closeToolStripMenuItem.PerformClick();
        DropDownItems.Add(mnuCloseRepo);

        DropDownItems.Add(new ToolStripSeparator());

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
            Assumes.NotNull(_startToolStripMenuItem);
            _startToolStripMenuItem.OpenRepositoryMenuItem.PerformClick();
        }
    }

    /// <summary>Updates the text shown on the combo button itself.</summary>
    public void RefreshContent()
    {
        string path = UICommands.Module.WorkingDir;

        // it appears at times Module.WorkingDir path is an empty string, this caused issues like #4874
        if (string.IsNullOrWhiteSpace(path))
        {
            Text = _noWorkingFolderText.Text;
            return;
        }

        IList<Repository> recentRepositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
            () => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));

        List<RecentRepoInfo> topRepos = new();
        RecentRepoSplitter splitter = new()
        {
            MeasureFont = Font,
        };

        splitter.SplitRecentRepos(recentRepositoryHistory, topRepos, topRepos);

        RecentRepoInfo? ri = topRepos.Find(e => e.Repo.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

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
