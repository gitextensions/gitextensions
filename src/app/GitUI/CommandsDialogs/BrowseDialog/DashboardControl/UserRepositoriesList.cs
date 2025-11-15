using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using static System.Windows.Forms.ListViewItem;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

public partial class UserRepositoriesList : GitExtensionsControl
{
    private readonly TranslationString _groupRecentRepositories = new("Recent repositories");
    private readonly TranslationString _repositorySearchPlaceholder = new("Search repositories");
    private readonly TranslationString _groupActions = new("Actions");
    private readonly TranslationString _deleteCategoryCaption = new(
        "Delete Category");
    private readonly TranslationString _deleteCategoryQuestion = new(
        "Do you want to delete category \"{0}\" with {1} repositories?\n\nThe action cannot be undone.");

    private readonly TranslationString _clearRecentCategoryCaption = new(
        "Clear recent repositories");
    private readonly TranslationString _clearRecentCategoryQuestion = new(
        "Do you want to clear the list of recent repositories?\n\nThe action cannot be undone.");

    private readonly TranslationString _cannotOpenTheFolder = new("Cannot open the folder");

    private class SelectedRepositoryItem
    {
        public bool IsFavourite { get; }
        public Repository Repository { get; }

        public SelectedRepositoryItem(bool isFavourite, Repository repository)
        {
            IsFavourite = isFavourite;
            Repository = repository;
        }
    }

    private readonly Font _secondaryFont;
    private static readonly Color DefaultFavouriteColor = Color.DarkGoldenrod.AdaptBackColor();
    private static readonly Color DefaultBranchNameColor = SystemColors.HotTrack;
    private Color _favouriteColor = DefaultFavouriteColor;
    private Color _branchNameColor = DefaultBranchNameColor;
    private Color _hoverColor;
    private Color _headerColor;
    private Color _headerBackColor;
    private Color _mainBackColor;
    private Color _searchBackColor;
    private Brush _foreColorBrush;
    private Brush _branchNameColorBrush = new SolidBrush(DefaultBranchNameColor);
    private Brush _favouriteColorBrush = new SolidBrush(DefaultFavouriteColor);
    private Brush _hoverColorBrush = new SolidBrush(SystemColors.InactiveCaption);
    private ListViewItem? _hoveredItem;
    private readonly ListViewGroup _lvgRecentRepositories;
    private readonly IUserRepositoriesListController _controller = new UserRepositoriesListController(RepositoryHistoryManager.Locals, new InvalidRepositoryRemover(), (path) => new GitModule(path));
    private bool _hasInvalidRepos;
    private ListViewItem? _rightClickedItem;

    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;

    public UserRepositoriesList()
    {
        InitializeComponent();
        InitializeComplete();

        mnuTop.DropDownItems.Clear();

        _lvgRecentRepositories = new ListViewGroup(_groupRecentRepositories.Text, HorizontalAlignment.Left)
        {
            Name = string.Empty,
            CollapsedState = ListViewGroupCollapsedState.Expanded,
            TaskLink = _groupActions.Text
        };

        _foreColorBrush = new SolidBrush(base.ForeColor);

        _secondaryFont = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints - 1f);
        lblRecentRepositories.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f);
        lblRecentRepositories.ForeColor.AdaptTextColor();

        textBoxSearch.PlaceholderText = _repositorySearchPlaceholder.Text;

        listView1.Items.Clear();
        listView1.Groups.Clear();

        imageList1.Images.Clear();
        imageList1.ImageSize = DpiUtil.Scale(imageList1.ImageSize);
        imageList1.Images.Add(Images.DashboardFolderGit);
        imageList1.Images.Add(Images.DashboardFolderError);

        DragEnter += OnDragEnter;
        DragDrop += OnDragDrop;
    }

    [Category("Appearance")]
    [DefaultValue(typeof(SystemColors), "HotTrack")]
    public Color BranchNameColor
    {
        get { return _branchNameColor; }
        set
        {
            if (_branchNameColor == value)
            {
                return;
            }

            _branchNameColor = value;
            _branchNameColorBrush?.Dispose();
            _branchNameColorBrush = new SolidBrush(value);
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "DarkGoldenrod")]
    public Color FavouriteColor
    {
        get { return _favouriteColor; }
        set
        {
            if (_favouriteColor == value)
            {
                return;
            }

            _favouriteColor = value;
            _favouriteColorBrush?.Dispose();
            _favouriteColorBrush = new SolidBrush(value);
            Invalidate();
        }
    }

    [Category("Appearance")]
    public override Color ForeColor
    {
        get { return base.ForeColor; }
        set
        {
            if (base.ForeColor == value)
            {
                return;
            }

            base.ForeColor = value;
            listView1.ForeColor = value;
            _foreColorBrush?.Dispose();
            _foreColorBrush = new SolidBrush(value);
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color HeaderColor
    {
        get { return _headerColor; }
        set
        {
            if (_headerColor == value)
            {
                return;
            }

            _headerColor = value;
            lblRecentRepositories.ForeColor = value;
            lblRecentRepositories.Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(SystemColors), "Window")]
    public Color HeaderBackColor
    {
        get { return _headerBackColor; }
        set
        {
            if (_headerBackColor == value)
            {
                return;
            }

            _headerBackColor = value;
            pnlHeader.BackColor = value;
            pnlHeader.Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(50)]
    public int HeaderHeight
    {
        get { return pnlHeader.Height; }
        set
        {
            pnlHeader.Height = value;
            pnlHeader.Invalidate();
        }
    }

    [Category("Appearance")]
    public Color HoverColor
    {
        get { return _hoverColor; }
        set
        {
            if (_hoverColor == value)
            {
                return;
            }

            _hoverColor = value;
            _hoverColorBrush?.Dispose();
            _hoverColorBrush = new SolidBrush(value);
            Invalidate();
        }
    }

    [Category("Appearance")]
    public Color MainBackColor
    {
        get { return _mainBackColor; }
        set
        {
            if (_mainBackColor == value)
            {
                return;
            }

            _mainBackColor = value;
            BackColor = value;
            listView1.BackColor = value;
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(SystemColors), "Window")]
    public Color SearchBackColor
    {
        get { return _searchBackColor; }
        set
        {
            if (_searchBackColor == value)
            {
                return;
            }

            _searchBackColor = value;
            textBoxSearch.BackColor = value;
        }
    }

    private ListViewItem? HoveredItem
    {
        get => _hoveredItem;
        set
        {
            if (value == _hoveredItem)
            {
                return;
            }

            if (_hoveredItem is not null)
            {
                // The previously hovered item may be already removed from the view, example:
                // user locates mouse pointer over an item and triggers data refresh by pressing F5
                if (listView1.Items.Contains(_hoveredItem))
                {
                    listView1.Invalidate(listView1.GetItemRect(_hoveredItem.Index));
                }
            }

            if (value is not null)
            {
                listView1.Invalidate(listView1.GetItemRect(value.Index));
            }

            _hoveredItem = value;
        }
    }

    private static StringComparer GroupHeaderComparer =>
        StringComparer.CurrentCulture;

    public void ShowRecentRepositories(bool reloadData = true)
    {
        if (reloadData)
        {
            _controller.ClearCache();
        }

        IReadOnlyList<RecentRepoInfo> recentRepositories;
        IReadOnlyList<RecentRepoInfo> favouriteRepositories;
        (recentRepositories, favouriteRepositories) = _controller.PreRenderRepositories(textBoxSearch.Text);

        try
        {
            listView1.BeginUpdate();
            listView1.Groups.Clear();
            listView1.Items.Clear();
            if (recentRepositories.Count > 0 || favouriteRepositories.Count > 0)
            {
                listView1.TileSize = GetTileSize(recentRepositories, favouriteRepositories);
            }

            _hasInvalidRepos = false;

            ListViewGroup[] groups = new[] { _lvgRecentRepositories }
                .Concat(recentRepositories.Concat(favouriteRepositories)
                    .Select(repo => repo.Repo.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(GroupHeaderComparer)
                    .OrderBy(c => c)
                    .Select(c => new ListViewGroup(c, c)
                    {
                        CollapsedState = ListViewGroupCollapsedState.Expanded,
                        TaskLink = _groupActions.Text
                    }))
                .ToArray();

            listView1.Groups.AddRange(groups);
            BindRepositories(recentRepositories, isFavourite: false);
            BindRepositories(favouriteRepositories, isFavourite: true);
        }
        finally
        {
            listView1.EndUpdate();
        }

        void BindRepositories(IReadOnlyList<RecentRepoInfo> repos, bool isFavourite)
        {
            for (int index = 0; index < repos.Count; index++)
            {
                RecentRepoInfo recent = repos[index];
                ListViewItem item = new(recent.Caption)
                {
                    ForeColor = ForeColor,
                    Font = AppSettings.Font,
                    Group = isFavourite ? GetTileGroup(recent.Repo) : _lvgRecentRepositories,
                    ImageIndex = 0,
                    UseItemStyleForSubItems = false,
                    Tag = recent.Repo,
                    ToolTipText = recent.Repo.Path
                };
                listView1.Items.Add(item);

                ThreadHelper.FileAndForget(async () =>
                {
                    bool isValidGitDir = _controller.IsValidGitWorkingDir(recent.Repo.Path);
                    string branchName = isValidGitDir ? _controller.GetCurrentBranchName(recent.Repo.Path) : "";
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    if (isValidGitDir)
                    {
                        item.SubItems.Add(new ListViewSubItem(item, branchName, BranchNameColor, BackColor, _secondaryFont));
                        //// NB: we can add a 3rd row as well: { repository.Repo.Category, SystemColors.GrayText, BackColor, _secondaryFont }
                    }
                    else
                    {
                        item.ImageIndex = 1;
                        _hasInvalidRepos = true;
                    }
                });
            }
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        // Reset the search
        textBoxSearch.Text = "";
    }

    protected virtual void OnModuleChanged(GitModuleEventArgs args)
    {
        EventHandler<GitModuleEventArgs> handler = GitModuleChanged;
        handler?.Invoke(this, args);
    }

    protected override bool ProcessDialogKey(Keys keyData)
    {
        if (keyData == Keys.Enter)
        {
            // .NET 5.0 introduced collapsible `ListViewGroup`s, which we do not yet have API access to but still get rendered in the UI.
            // Whenever the list of repos is collapsed but we still have a repo selected (and hidden), if we hit enter, the selected repo will
            // be opened. This should be a no-op instead, however, since we cannot visually tell what repo is selected. When we upgrade
            // to .NET 5.0, we can check ListViewGroup.CollapsedState to fix this issue.
            return TryOpenRepository(GetSelectedRepository());
        }

        return base.ProcessDialogKey(keyData);
    }

    private List<string> GetCategories()
    {
        return GetRepositories()
            .Select(repository => repository.Category)
            .WhereNotNullOrWhiteSpace()
            .OrderBy(x => x)
            .Distinct()
            .ToList();
    }

    private IEnumerable<Repository> GetRepositories()
    {
        return listView1.Items.Cast<ListViewItem>()
            .Select(lvi => (Repository)lvi.Tag)
            .Where(r => r is not null);
    }

    private static SelectedRepositoryItem? GetSelectedRepositoryItem(ToolStripItem? menuItem)
    {
        // Retrieve the ContextMenuStrip that owns this ToolStripItem
        ContextMenuStrip contextMenu = menuItem?.Owner as ContextMenuStrip;

        // Get the control that is displaying this context menu
        SelectedRepositoryItem selected = contextMenu?.Tag as SelectedRepositoryItem;
        if (string.IsNullOrWhiteSpace(selected?.Repository?.Path))
        {
            return null;
        }

        return selected;
    }

    private Repository? GetSelectedRepository()
    {
        if (listView1.SelectedItems.Count < 1)
        {
            return null;
        }

        Repository selected = listView1.SelectedItems[0].Tag as Repository;
        if (string.IsNullOrWhiteSpace(selected?.Path))
        {
            return null;
        }

        return selected;
    }

    private ListViewGroup GetTileGroup(Repository repository)
    {
        return listView1.Groups.Cast<ListViewGroup>()
            .SingleOrDefault(x => GroupHeaderComparer.Equals(x.Header, repository.Category));
    }

    private Size GetTileSize(IEnumerable<RecentRepoInfo> recentRepositories, IEnumerable<RecentRepoInfo> favouriteRepositories)
    {
        float spacing1 = DpiUtil.Scale(1f);
        float spacing2 = DpiUtil.Scale(2f);

        var longestPath = recentRepositories.Union(favouriteRepositories)
                                            .Select(r =>
                                            {
                                                Size size = TextRenderer.MeasureText(r.Caption, AppSettings.Font);
                                                return new
                                                {
                                                    r.Caption,
                                                    size.Width,
                                                    size.Height
                                                };
                                            })
                                           .OrderByDescending(r => r.Width)
                                           .First();
        Size branchTextSize = TextRenderer.MeasureText("A", _secondaryFont);

        int width = AppSettings.RecentReposComboMinWidth;
        if (width < 1)
        {
            width = longestPath.Width + imageList1.ImageSize.Width;
        }

        float height = longestPath.Height + (2 * branchTextSize.Height) +
            /* offset from top and bottom */(2 * spacing2) +
            /* twice space between text */(2 * spacing1);

        return new Size((int)(width + DpiUtil.Scale(50f)), (int)Math.Max(height, DpiUtil.Scale(50f)));
    }

    private static void RepositoryContextAction(ToolStripItem? menuItem, Action<SelectedRepositoryItem> action)
    {
        SelectedRepositoryItem selected = GetSelectedRepositoryItem(menuItem);
        if (selected is not null)
        {
            action(selected);
        }
    }

    private static string ShortenText(string text, Font font, float maxWidth)
    {
        char ellipsis = '…';
        int width = TextRenderer.MeasureText(text, font).Width;
        if (width < maxWidth)
        {
            return text;
        }

        while (text.Length > 1)
        {
            width = TextRenderer.MeasureText(text + ellipsis, font).Width;
            if (width < maxWidth)
            {
                break;
            }

            text = text[..^1];
        }

        return text + ellipsis;
    }

    private bool PromptCategoryName(List<string> categories, string? originalName, [NotNullWhen(returnValue: true)] out string? name)
    {
        using FormDashboardCategoryTitle dialog = new(categories, originalName);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            name = dialog.Category;
            return name is not null;
        }

        name = null;
        return false;
    }

    private bool PromptUserConfirm(string question, string caption)
    {
        DialogResult dialogResult = MessageBox.Show(this,
            question,
            caption,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        return dialogResult == DialogResult.Yes;
    }

    private void UpdateCategoryName(string originalName, string? newName)
    {
        foreach (Repository repository in GetRepositories().Where(r => r.Category == originalName))
        {
            ThreadHelper.JoinableTaskFactory.Run(() => _controller.AssignCategoryAsync(repository, newName));
        }

        ShowRecentRepositories();
    }

    private void contextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
    {
        _rightClickedItem = null;
        ShowRecentRepositories();
    }

    private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
        Repository selected = GetSelectedRepository();

        tsmiRemoveFromList.Visible =
            toolStripMenuItem1.Visible =
                tsmiCategories.Visible =
                    toolStripMenuItem2.Visible =
                        tsmiOpenFolder.Visible = selected is not null;

        tsmiRemoveMissingReposFromList.Visible = _hasInvalidRepos;

        if (selected is null || _rightClickedItem is null)
        {
            e.Cancel = true;
            return;
        }

        // address a bug in context menu implementation
        // nested toolstrip items can't get source control
        // http://stackoverflow.com/questions/30534417/
        contextMenuStripRepository.Tag = new SelectedRepositoryItem(isFavourite: _rightClickedItem.Group != _lvgRecentRepositories, repository: selected);
    }

    private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
    {
        float spacing1 = DpiUtil.Scale(1f);
        float spacing2 = DpiUtil.Scale(2f);
        float spacing4 = DpiUtil.Scale(4f);
        float spacing6 = DpiUtil.Scale(6f);

        float textOffset = spacing2 + imageList1.ImageSize.Width + spacing2;
        int textWidth = e.Bounds.Width - (int)textOffset;

        if (e.Item == HoveredItem || e.Item.Selected)
        {
            e.Graphics.FillRectangle(_hoverColorBrush, e.Bounds);
        }
        else
        {
            e.DrawBackground();
        }

        PointF pointImage = new(e.Bounds.Left + spacing4, e.Bounds.Top + (spacing2 * 4));

        // render anchor icon
        if (!string.IsNullOrWhiteSpace((e.Item.Tag as Repository)?.Category))
        {
            PointF pointImage1 = new(pointImage.X + imageList1.ImageSize.Width - 12, e.Bounds.Top + spacing2);
            e.Graphics.DrawImage(Images.Star, pointImage1.X, pointImage1.Y, 16, 16);
        }

        // render icon
        e.Graphics.DrawImage(imageList1.Images[e.Item.ImageIndex], pointImage);

        // render path
        PointF textPadding = new(e.Bounds.Left + spacing4, e.Bounds.Top + spacing6);
        PointF pointPath = new(textPadding.X + textOffset, textPadding.Y);
        RectangleF pathBounds = DrawText(e.Graphics, e.Item.Text, AppSettings.Font, _foreColorBrush, textWidth, pointPath, spacing4 * 2);

        if (e.Item.SubItems.Count > 1)
        {
            // render branch
            PointF pointBranch = new(pointPath.X, pointPath.Y + pathBounds.Height + spacing1);
            RectangleF branchBounds = DrawText(e.Graphics, e.Item.SubItems[1].Text, _secondaryFont, _branchNameColorBrush, textWidth, pointBranch, spacing4 * 2);

            // render category
            if (e.Item.SubItems.Count > 2 && !string.IsNullOrWhiteSpace(e.Item.SubItems[2].Text))
            {
                PointF pointCategory = string.IsNullOrWhiteSpace(e.Item.SubItems[1].Text)
                                    ? pointBranch
                                    : new PointF(pointBranch.X, pointBranch.Y + branchBounds.Height + spacing1);
                DrawText(e.Graphics, e.Item.SubItems[2].Text, _secondaryFont, SystemBrushes.GrayText, textWidth, pointCategory, spacing4 * 2);
            }
        }

        RectangleF DrawText(Graphics g, string text, Font font, Brush brush, int maxTextWidth, PointF location, float spacing)
        {
            Size textBounds = TextRenderer.MeasureText(text, font);
            float minWidth = Math.Min(textBounds.Width + spacing, maxTextWidth);
            RectangleF bounds = new(location, new SizeF(minWidth, textBounds.Height));
            string text1 = Math.Abs(maxTextWidth - minWidth) < float.Epsilon ? ShortenText(text, font, minWidth) : text;
            g.DrawString(text1, font, brush, bounds, StringFormat.GenericTypographic);

            return bounds;
        }
    }

    private void ListView1_GroupTaskLinkClick(object sender, ListViewGroupEventArgs e)
    {
        ListViewGroup group = listView1.Groups[e.GroupIndex];
        bool isRecentRepositoriesGroup = group == _lvgRecentRepositories;
        tsmiCategoryDelete.Visible = !isRecentRepositoriesGroup;
        tsmiCategoryRename.Visible = !isRecentRepositoriesGroup;
        tsmiCategoryClear.Visible = isRecentRepositoriesGroup;

        contextMenuStripCategory.Tag = group;
        contextMenuStripCategory.Show(listView1, listView1.PointToClient(Cursor.Position));
    }

    private void listView1_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            TryOpenRepository(GetSelectedRepository());
        }
        else if (e.Button == MouseButtons.Right)
        {
            _rightClickedItem = listView1.GetItemAt(e.X, e.Y);
            if (_rightClickedItem is not null)
            {
                _rightClickedItem.Selected = true;
            }
        }
    }

    private void TextBoxSearch_TextChanged(object sender, EventArgs e)
    {
        ShowRecentRepositories(reloadData: false);
    }

    private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            // Open the first repo in the list
            ListView.ListViewItemCollection items = listView1.Items;
            if (items.Count == 0)
            {
                return;
            }

            TryOpenRepository(items[0].Tag as Repository);
        }
        else if (e.KeyCode == Keys.Down)
        {
            listView1.Focus();
        }
    }

    private void listView1_MouseMove(object sender, MouseEventArgs e)
    {
        HoveredItem = listView1.GetItemAt(e.X, e.Y);
    }

    private void listView1_MouseLeave(object sender, EventArgs e)
    {
        HoveredItem = null;
    }

    private void listView1_GotFocus(object sender, EventArgs e)
    {
        if (listView1.Items.Count > 0 && listView1.SelectedItems.Count == 0)
        {
            listView1.Items[0].Selected = true;
        }
    }

    private void listView1_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up && listView1.SelectedItems.Count > 0)
        {
            // Compare current item to the very first item to see if it's at the top
            ListViewItem selectedItem = listView1.SelectedItems[0];
            if (selectedItem.Bounds.Y == listView1.Items[0].Bounds.Y)
            {
                textBoxSearch.Focus();
                selectedItem.Selected = true;
                e.Handled = true;
            }
        }
    }

    private void mnuConfigure_Click(object sender, EventArgs e)
    {
        using FormRecentReposSettings frm = new();
        DialogResult result = frm.ShowDialog(this);
        if (result == DialogResult.OK)
        {
            ShowRecentRepositories();
        }
    }

    private void RecentRepositoriesList_Load(object sender, EventArgs e)
    {
        if (!(Parent.FindForm() is FormBrowse form))
        {
            return;
        }

        ToolStripItem[] menus = new ToolStripItem[] { mnuConfigure };
        MenuStrip menuStrip = form.FindDescendantOfType<MenuStrip>(p => p.Name == "mainMenuStrip");
        Validates.NotNull(menuStrip);
        ToolStripMenuItem dashboardMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "dashboardToolStripMenuItem");
        dashboardMenu?.DropDownItems.AddRange(menus);
    }

    private void tsmiCategories_DropDownOpening(object sender, EventArgs e)
    {
        if (sender != tsmiCategories)
        {
            return;
        }

        tsmiCategories.DropDown.SuspendLayout();
        tsmiCategories.DropDownItems.Clear();

        List<string> categories = GetCategories();
        if (categories.Count > 0)
        {
            tsmiCategories.DropDownItems.Add(tsmiCategoryNone);
            tsmiCategories.DropDownItems.AddRange(categories.Select(category =>
            {
                ToolStripMenuItem item = new(category) { Tag = category };
                item.Click += tsmiCategory_Click;
                return item;
            }).ToArray<ToolStripItem>());
            tsmiCategories.DropDownItems.Add(new ToolStripSeparator());
        }

        tsmiCategories.DropDownItems.Add(tsmiCategoryAdd);
        tsmiCategories.DropDown.ResumeLayout();

        RepositoryContextAction(tsmiCategories, selectedRepositoryItem =>
        {
            Validates.NotNull(selectedRepositoryItem.Repository);

            foreach (ToolStripItem item in tsmiCategories.DropDownItems)
            {
                item.Enabled = item.Text != selectedRepositoryItem.Repository.Category;
            }

            if (string.IsNullOrWhiteSpace(selectedRepositoryItem.Repository.Category) && tsmiCategories.DropDownItems.Count > 1)
            {
                tsmiCategories.DropDownItems[0].Enabled = false;
            }
        });
    }

    private void tsmiCategory_Click(object sender, EventArgs e)
    {
        SelectedRepositoryItem selectedRepositoryItem = GetSelectedRepositoryItem((sender as ToolStripMenuItem)?.OwnerItem);
        if (selectedRepositoryItem is null)
        {
            return;
        }

        string category = (sender as ToolStripMenuItem)?.Tag as string;
        ThreadHelper.JoinableTaskFactory.Run(() => _controller.AssignCategoryAsync(selectedRepositoryItem.Repository, category));
        ShowRecentRepositories();
    }

    private void tsmiCategoryAdd_Click(object sender, EventArgs e)
    {
        RepositoryContextAction((sender as ToolStripMenuItem)?.OwnerItem, selectedRepositoryItem =>
        {
            if (PromptCategoryName(GetCategories(), originalName: null, out string? categoryName))
            {
                ThreadHelper.JoinableTaskFactory.Run(() => _controller.AssignCategoryAsync(selectedRepositoryItem.Repository, categoryName));
                ShowRecentRepositories();
            }
        });
    }

    private void tsmiOpenFolder_Click(object sender, EventArgs e)
        => RepositoryContextAction(sender as ToolStripMenuItem, selectedRepositoryItem => OsShellUtil.OpenWithFileExplorer(selectedRepositoryItem.Repository.Path));

    private void tsmiRemoveFromList_Click(object sender, EventArgs e)
    {
        RepositoryContextAction(sender as ToolStripMenuItem, selectedRepositoryItem =>
        {
            ThreadHelper.JoinableTaskFactory.Run(() =>
                selectedRepositoryItem.IsFavourite
                    ? RepositoryHistoryManager.Locals.RemoveFavouriteAsync(selectedRepositoryItem.Repository.Path)
                    : RepositoryHistoryManager.Locals.RemoveRecentAsync(selectedRepositoryItem.Repository.Path));
            ShowRecentRepositories();
        });
    }

    private void tsmiRemoveMissingReposFromList_Click(object sender, EventArgs e)
    {
        RepositoryContextAction(sender as ToolStripMenuItem, selectedRepositoryItem =>
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(_controller.IsValidGitWorkingDir));
            ShowRecentRepositories();
        });
    }

    private void tsmiCategoryRename_Click(object sender, EventArgs e)
    {
        ListViewGroup categoryGroup = (ListViewGroup)contextMenuStripCategory.Tag;
        string originalName = categoryGroup.Name;

        List<string> categories = GetCategories();
        categories.Remove(originalName);

        if (PromptCategoryName(categories, originalName, out string? newName))
        {
            UpdateCategoryName(originalName, newName);
        }
    }

    private void tsmiCategoryDelete_Click(object sender, EventArgs e)
    {
        ListViewGroup categoryGroup = (ListViewGroup)contextMenuStripCategory.Tag;
        string name = categoryGroup.Name;
        string question = string.Format(_deleteCategoryQuestion.Text, name, categoryGroup.Items.Count);

        if (!PromptUserConfirm(question, _deleteCategoryCaption.Text))
        {
            return;
        }

        UpdateCategoryName(name, null);
    }

    private void tsmiCategoryClear_Click(object sender, EventArgs e)
    {
        List<Repository> repositories = GetRepositories().ToList();
        string question = string.Format(_clearRecentCategoryQuestion.Text, repositories.Count);
        if (!PromptUserConfirm(question, _clearRecentCategoryCaption.Text))
        {
            return;
        }

        foreach (Repository repository in repositories)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                () => RepositoryHistoryManager.Locals.RemoveRecentAsync(repository.Path));
        }

        ShowRecentRepositories();
    }

    private void OnDragDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] fileNameArray)
        {
            if (fileNameArray.Length != 1)
            {
                return;
            }

            string dir = fileNameArray[0];
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                GitModule module = new(dir);

                if (!module.IsValidGitWorkingDir())
                {
                    MessageBox.Show(this, TranslatedStrings.DirectoryInvalidRepository,
                        _cannotOpenTheFolder.Text, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    return;
                }

                OnModuleChanged(new GitModuleEventArgs(module));
            }
        }
    }

    private static void OnDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] fileNameArray)
        {
            if (fileNameArray.Length != 1)
            {
                return;
            }

            string dir = fileNameArray[0];
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                // Allow drop (copy, not move) folders
                e.Effect = DragDropEffects.Copy;
            }
        }
    }

    /// <summary>
    /// Tries to open the currently selected repository
    /// </summary>
    /// <returns>False if no repo is selected, true otherwise</returns>
    private bool TryOpenRepository(Repository? repository)
    {
        if (repository is null)
        {
            return false;
        }

        if (_controller.IsValidGitWorkingDir(repository.Path))
        {
            OnModuleChanged(new GitModuleEventArgs(new GitModule(repository.Path)));
            return true;
        }

        if (_controller.RemoveInvalidRepository(repository.Path))
        {
            ShowRecentRepositories();
            return true;
        }

        return true;
    }
}
