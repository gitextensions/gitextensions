using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;
using Color = Avalonia.Media.Color;
using Font = GitExtensions.Shims.WinForms.Font;
using Size = Avalonia.Size;
using WinFormsShims = GitExtensions.Shims.WinForms;
using WinFormsControls = GitUI.Compat.WinFormsControls;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

public partial class UserRepositoriesList : TranslatedControl
{
#pragma warning disable SX1309 // Retain the WinForms twin's source field name for structural parity.
    private readonly IReadOnlyList<IImage> imageList1 = [Images.DashboardFolderGit, Images.DashboardFolderError];
#pragma warning restore SX1309
    private readonly TranslationString _groupRecentRepositories = new("Recent repositories");
    private readonly TranslationString _repositorySearchPlaceholder = new("Search repositories...");
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

    private sealed class SelectedRepositoryItem
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
    private static readonly Color DefaultFavouriteColor = Colors.DarkGoldenrod;
    private static readonly Color DefaultBranchNameColor = Color.Parse("#2D5FAF");
    private Color _favouriteColor = DefaultFavouriteColor;
    private Color _branchNameColor = DefaultBranchNameColor;
    private Color _hoverColor = Color.Parse("#ACCFEF");
    private Color _headerColor = Colors.DimGray;
    private Color _headerBackColor = Color.Parse("#ACCFEF");
    private Color _mainBackColor = Colors.White;
    private Color _searchBackColor = Color.FromRgb(248, 248, 255);
    private Color _foreColor = Color.FromRgb(30, 30, 30);
    private Brush _foreColorBrush;
    private Brush _branchNameColorBrush = new SolidColorBrush(DefaultBranchNameColor);
    private Brush _favouriteColorBrush = new SolidColorBrush(DefaultFavouriteColor);
    private Brush _hoverColorBrush = new SolidColorBrush(Color.Parse("#ACCFEF"));
    private ListBoxItem? _hoveredItem;
    private readonly RepositoryGroupItem _lvgRecentRepositories;
    private bool _hasInvalidRepos;
    private ListBoxItem? _rightClickedItem;
    private Func<IGitUICommands>? _getUICommands;
    private bool _isSubscribed;
    private IUserRepositoriesListController? _controller;
    private IRepositoryHistoryUIService? _repositoryHistoryUIService;
    private RepositoryGroupItem? _selectedCategory;

    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;

    private IUserRepositoriesListController Controller
        => _controller ?? throw new InvalidOperationException("The repository list is not initialized.");

    public UserRepositoriesList()
    {
        InitializeComponent();
        InitializeComplete();

        // Native's single AutoSize column retains the ListView's Designer preferred width,
        // then receives any surplus width. Grid's star column would shrink both inputs.
        tableLayoutPanel2.SizeChanged += (_, _) =>
            tableLayoutPanel2.ColumnDefinitions[0].Width = new GridLength(Math.Max(445, tableLayoutPanel2.Bounds.Width));

        mnuTop.Items.Clear();
        _lvgRecentRepositories = new RepositoryGroupItem(_groupRecentRepositories.Text, isRecentGroup: true, repositoryCount: 0);
        _foreColorBrush = new SolidColorBrush(_foreColor);
        Foreground = _foreColorBrush;
        listView1.Foreground = _foreColorBrush;
        menuStripRecentMenu.Foreground = _foreColorBrush;
        mnuTop.Foreground = _foreColorBrush;
        Focusable = true;
        GotFocus += (_, e) =>
        {
            if (ReferenceEquals(e.Source, this))
            {
                textBoxSearch.Focus();
            }
        };
        Resources["DashboardRepositoryHoverBrush"] = _hoverColorBrush;
        _secondaryFont = new Font(AppSettings.Font.FontFamily, AppSettings.Font.Size - 1F);
        lblRecentRepositories.FontFamily = new FontFamily(AppSettings.Font.Name);
        lblRecentRepositories.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size + 5.5F);
        lblRecentRepositories.Height = Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(
            lblRecentRepositories, lblRecentRepositories.Text ?? string.Empty).Height);

        // Apply owned defaults even when the first theme assignment equals a backing field;
        // the source Designer has already painted these properties before its setters run.
        lblRecentRepositories.Foreground = new SolidColorBrush(_headerColor);
        pnlHeader.Background = new SolidColorBrush(_headerBackColor);
        Background = new SolidColorBrush(_mainBackColor);
        listView1.Background = new SolidColorBrush(_mainBackColor);
        menuStripRecentMenu.Background = new SolidColorBrush(_mainBackColor);
        mnuTop.Background = menuStripRecentMenu.Background;
        textBoxSearch.Background = new SolidColorBrush(_searchBackColor);
        listView1.AddColumns(clmhdrPath, clmhdrBranch, clmhdrCategory);
        listView1.ItemTemplate = new FuncDataTemplate<object>(
            (item, _) => listView1_DrawItem(item),
            supportsRecycling: false);
        listView1.ContainerPrepared += ListView1_ContainerPrepared;
        listView1.AddHandler(PointerPressedEvent, listView1_PointerPressed, RoutingStrategies.Tunnel);
        listView1.PointerReleased += listView1_MouseClick;
        listView1.PointerMoved += listView1_MouseMove;
        listView1.PointerExited += listView1_MouseLeave;
        listView1.KeyDown += listView1_KeyDown;
        listView1.GotFocus += listView1_GotFocus;
        listView1.GroupTaskLinkClick += ListView1_GroupTaskLinkClick;
        textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
        textBoxSearch.KeyDown += TextBoxSearch_KeyDown;

        // Native TextBox cue banners are hidden while the search input has focus.
        textBoxSearch.GotFocus += (_, _) => textBoxSearch.PlaceholderText = null;
        textBoxSearch.LostFocus += (_, _) => textBoxSearch.PlaceholderText = _repositorySearchPlaceholder.Text;
        mnuConfigure.Click += mnuConfigure_Click;
        contextMenuStripRepository.Opening += contextMenuStrip_Opening;
        contextMenuStripRepository.Closed += contextMenuStrip_Closed;
        tsmiCategories.SubmenuOpened += tsmiCategories_DropDownOpening;
        tsmiCategoryNone.Click += tsmiCategory_Click;
        tsmiCategoryAdd.Click += tsmiCategoryAdd_Click;
        tsmiOpenFolder.Click += tsmiOpenFolder_Click;
        tsmiRemoveFromList.Click += tsmiRemoveFromList_Click;
        tsmiRemoveMissingReposFromList.Click += tsmiRemoveMissingReposFromList_Click;
        tsmiCategoryRename.Click += tsmiCategoryRename_Click;
        tsmiCategoryDelete.Click += tsmiCategoryDelete_Click;
        tsmiCategoryClear.Click += tsmiCategoryClear_Click;
        PropertyChanged += (_, e) =>
        {
            if (e.Property == IsVisibleProperty)
            {
                OnVisibleChanged(EventArgs.Empty);
            }
        };
        AttachedToLogicalTree += RecentRepositoriesList_Load;

        DragDrop.SetAllowDrop(this, true);
        DragDrop.AddDragEnterHandler(this, OnDragEnter);
        DragDrop.AddDragOverHandler(this, OnDragEnter);
        DragDrop.AddDropHandler(this, OnDragDrop);
        textBoxSearch.PlaceholderText = textBoxSearch.IsFocused ? null : _repositorySearchPlaceholder.Text;
    }

    [Category("Appearance")]
    public Color BranchNameColor
    {
        get => _branchNameColor;
        set => SetAppearance(ref _branchNameColor, ref _branchNameColorBrush, value);
    }

    [Category("Appearance")]
    public Color FavouriteColor
    {
        get => _favouriteColor;
        set => SetAppearance(ref _favouriteColor, ref _favouriteColorBrush, value);
    }

    [Category("Appearance")]
    public Color ForeColor
    {
        get => _foreColor;
        set
        {
            SetAppearance(ref _foreColor, ref _foreColorBrush, value);

            // Avalonia's Menu style supplies a local palette instead of inheriting its
            // owning control's foreground as the source MenuStrip does.
            menuStripRecentMenu.Foreground = _foreColorBrush;
            mnuTop.Foreground = _foreColorBrush;
        }
    }

    [Category("Appearance")]
    public Color HeaderColor
    {
        get => _headerColor;
        set
        {
            if (_headerColor == value)
            {
                return;
            }

            _headerColor = value;
            lblRecentRepositories.Foreground = new SolidColorBrush(value);
        }
    }

    [Category("Appearance")]
    public Color HeaderBackColor
    {
        get => _headerBackColor;
        set
        {
            if (_headerBackColor == value)
            {
                return;
            }

            _headerBackColor = value;
            pnlHeader.Background = new SolidColorBrush(value);
        }
    }

    [Category("Appearance")]
    [DefaultValue(50)]
    public int HeaderHeight
    {
        get => (int)pnlHeader.Height;
        set => pnlHeader.Height = value;
    }

    [Category("Appearance")]
    public Color HoverColor
    {
        get => _hoverColor;
        set
        {
            if (_hoverColor == value)
            {
                return;
            }

            _hoverColor = value;
            ((SolidColorBrush)_hoverColorBrush).Color = value;
            Resources["DashboardRepositoryHoverBrush"] = _hoverColorBrush;
            InvalidateVisual();
        }
    }

    [Category("Appearance")]
    public Color MainBackColor
    {
        get => _mainBackColor;
        set
        {
            if (_mainBackColor == value)
            {
                return;
            }

            _mainBackColor = value;
            Background = new SolidColorBrush(value);
            listView1.Background = new SolidColorBrush(value);
            menuStripRecentMenu.Background = listView1.Background;
            mnuTop.Background = listView1.Background;
        }
    }

    [Category("Appearance")]
    public Color SearchBackColor
    {
        get => _searchBackColor;
        set
        {
            if (_searchBackColor == value)
            {
                return;
            }

            _searchBackColor = value;
            textBoxSearch.Background = new SolidColorBrush(value);
        }
    }

    private ListBoxItem? HoveredItem
    {
        get => _hoveredItem;
        set => _hoveredItem = value;
    }

    private static StringComparer GroupHeaderComparer => StringComparer.CurrentCulture;

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        textBoxSearch.PlaceholderText = textBoxSearch.IsFocused ? null : _repositorySearchPlaceholder.Text;
        ShowRecentRepositories(reloadData: false);
    }

    public void Initialize(
        IUserRepositoriesListController controller,
        IRepositoryHistoryUIService? repositoryHistoryUIService,
        Func<IGitUICommands> getUICommands)
    {
        if (_repositoryHistoryUIService is not null && _isSubscribed)
        {
            _repositoryHistoryUIService.HistoryChanged -= RepositoryHistoryUIService_HistoryChanged;
        }

        _controller = controller;
        _repositoryHistoryUIService = repositoryHistoryUIService;
        _getUICommands = getUICommands;
        if (_repositoryHistoryUIService is not null)
        {
            _repositoryHistoryUIService.HistoryChanged += RepositoryHistoryUIService_HistoryChanged;
            _isSubscribed = true;
            _repositoryHistoryUIService.TriggerBranchNameCacheUpdate(onlyIfEmpty: true);
        }
    }

    public void ShowRecentRepositories(bool reloadData = true)
    {
        if (_controller is null)
        {
            return;
        }

        if (reloadData)
        {
            Controller.ClearCache();
        }

        IReadOnlyList<RecentRepoInfo> recentRepositories;
        IReadOnlyList<RecentRepoInfo> favouriteRepositories;
        (recentRepositories, favouriteRepositories) = Controller.PreRenderRepositories(textBoxSearch.Text ?? string.Empty);

        Size tileSize = recentRepositories.Count > 0 || favouriteRepositories.Count > 0
            ? GetTileSize(recentRepositories, favouriteRepositories)
            : new Size(0, 50);
        List<object> rows = [];
        _hasInvalidRepos = false;
        _lvgRecentRepositories.RepositoryCount = recentRepositories.Count;
        BindRepositories(rows, _lvgRecentRepositories, recentRepositories, tileSize, isFavourite: false);
        foreach (IGrouping<string?, RecentRepoInfo> category in favouriteRepositories
                     .GroupBy(repo => repo.Repo.Category, GroupHeaderComparer)
                     .OrderBy(group => group.Key, GroupHeaderComparer))
        {
            RecentRepoInfo[] repositories = [.. category];
            RepositoryGroupItem group = GetTileGroup(repositories[0].Repo);
            group.RepositoryCount = repositories.Length;
            BindRepositories(rows, group, repositories, tileSize, isFavourite: true);
        }

        HoveredItem = null;
        listView1.ItemsSource = rows;
        listView1.SelectedItem = null;
    }

    protected override void OnDetachedFromLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        if (_repositoryHistoryUIService is not null && _isSubscribed)
        {
            _repositoryHistoryUIService.HistoryChanged -= RepositoryHistoryUIService_HistoryChanged;
            _isSubscribed = false;
        }

        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (_repositoryHistoryUIService is not null && !_isSubscribed)
        {
            _repositoryHistoryUIService.HistoryChanged += RepositoryHistoryUIService_HistoryChanged;
            _isSubscribed = true;
        }
    }

    protected virtual void OnVisibleChanged(EventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        // Reset the search
        textBoxSearch.Text = string.Empty;
        textBoxSearch.Focus();
    }

    protected virtual void OnModuleChanged(GitModuleEventArgs args)
    {
        EventHandler<GitModuleEventArgs>? handler = GitModuleChanged;
        handler?.Invoke(this, args);
    }

    protected virtual bool ProcessDialogKey(WinFormsShims.Keys keyData)
    {
        return keyData == WinFormsShims.Keys.Enter
            && TryOpenRepository(GetSelectedRepository());
    }

    private void BindRepositories(
        ICollection<object> rows,
        RepositoryGroupItem group,
        IEnumerable<RecentRepoInfo> repositories,
        Size tileSize,
        bool isFavourite)
    {
        RecentRepoInfo[] items = [.. repositories];
        if (items.Length == 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(group.Name))
        {
            rows.Add(group);
        }

        foreach (RecentRepoInfo recent in items)
        {
            bool isValidGitDir = Controller.IsValidGitWorkingDir(recent.Repo.Path);
            string branchName = isValidGitDir ? Controller.GetCurrentBranchName(recent.Repo.Path) : string.Empty;
            _hasInvalidRepos |= !isValidGitDir;
            rows.Add(new RepositoryListItem(
                recent.Caption ?? recent.Repo.Path,
                recent,
                branchName,
                isFavourite,
                isValidGitDir,
                tileSize));
        }
    }

    private Control CreateRowCore(object? item)
    {
        // Avalonia templates the original owner-drawn ListView rows as native controls.
        // Avalonia clears a recycled ContentPresenter by invoking the typed template with
        // null before assigning the replacement item.
        if (item is null)
        {
            return new Border();
        }

        if (item is RepositoryGroupItem group)
        {
            Button actions = new()
            {
                Content = _groupActions.Text,
                Classes = { "dashboard-group-action" },
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = group,
            };
            actions.Click += listView1.RaiseGroupTaskLinkClick;
            Grid header = new() { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), Margin = new Avalonia.Thickness(9, 10, 4, 4) };
            header.Children.Add(new TextBlock
            {
                Text = group.Name,
                Foreground = new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(
                    AvaloniaThemeResources.ResolveSystemColor(GitUI.Theming.ThemeModule.Settings, System.Drawing.KnownColor.HotTrack))),
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
            });
            Border rule = new()
            {
                Height = 1,
                Background = new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(
                    AvaloniaThemeResources.ResolveSystemColor(GitUI.Theming.ThemeModule.Settings, System.Drawing.KnownColor.InactiveCaption))),
                Margin = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(rule, 1);
            header.Children.Add(rule);

            // The native group task link is revealed by the header hover, not always painted.
            actions.IsVisible = false;
            header.PointerEntered += (_, _) => actions.IsVisible = true;
            header.PointerExited += (_, _) => actions.IsVisible = false;
            Grid.SetColumn(actions, 2);
            header.Children.Add(actions);
            return header;
        }

        RepositoryListItem repository = (RepositoryListItem)item;
        Image image = new()
        {
            // The source ImageList stream retains the authored 32px dashboard resource size.
            Width = imageList1[0].Size.Width,
            Height = imageList1[0].Size.Height,
            Source = imageList1[repository.IsValid ? 0 : 1],
            Margin = new Avalonia.Thickness(4, 8, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };
        Grid row = new()
        {
            MinHeight = repository.TileSize.Height,
            Width = repository.TileSize.Width,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        if (!string.IsNullOrWhiteSpace(repository.Repository.Repo.Category))
        {
            row.Children.Add(new Image
            {
                Width = 16,
                Height = 16,
                Source = Images.Star,
                Margin = new Avalonia.Thickness(4 + image.Width - 12, 2, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false,
            });
        }

        // WinForms paints the category star before the folder so their overlapping pixels
        // retain the original layer order.
        row.Children.Add(image);

        StackPanel text = new()
        {
            Spacing = 1,
            Margin = new Avalonia.Thickness(4 + 2 + image.Width + 2, 6, 4, 0),
            Children =
            {
                new TextBlock
                {
                    Text = ShortenText(
                        repository.Text,
                        AppSettings.Font,
                        (float)Math.Max(1, repository.TileSize.Width - 2 - image.Width - 2)),
                    Foreground = _foreColorBrush,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
                new TextBlock
                {
                    Text = repository.BranchName,
                    IsVisible = !string.IsNullOrWhiteSpace(repository.BranchName),
                    Foreground = _branchNameColorBrush,
                    FontFamily = new FontFamily(_secondaryFont.Name),
                    FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(_secondaryFont.Size),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
            },
        };
        row.Children.Add(text);
        ToolTip.SetTip(row, repository.Repository.Repo.Path);
        return row;
    }

    private void SetAppearance(ref Color field, ref Brush brush, Color value)
    {
        if (field == value)
        {
            return;
        }

        field = value;

        // WinForms repaints existing tiles without changing selection or repository data.
        // Mutate the retained brush so every realized tile reacts through the same boundary.
        ((SolidColorBrush)brush).Color = value;
        InvalidateVisual();
    }

    private List<string> GetCategories()
    {
        return [.. GetRepositories()
            .Select(repository => repository.Category)
            .WhereNotNullOrWhiteSpace()
            .OrderBy(x => x)
            .Distinct()];
    }

    private IEnumerable<Repository> GetRepositories()
    {
        return listView1.Items
            .OfType<RepositoryListItem>()
            .Select(item => item.Repository.Repo);
    }

    private SelectedRepositoryItem? GetSelectedRepositoryItem()
    {
        RepositoryListItem? selected = _rightClickedItem?.DataContext as RepositoryListItem
            ?? listView1.SelectedItem as RepositoryListItem;
        if (string.IsNullOrWhiteSpace(selected?.Repository.Repo.Path))
        {
            return null;
        }

        return new SelectedRepositoryItem(selected.IsFavourite, selected.Repository.Repo);
    }

    private Repository? GetSelectedRepository()
        => (listView1.SelectedItem as RepositoryListItem)?.Repository.Repo;

    private static RepositoryGroupItem GetTileGroup(Repository repository)
        => new(repository.Category ?? string.Empty, isRecentGroup: false, repositoryCount: 0);

    private Size GetTileSize(
        IEnumerable<RecentRepoInfo> recentRepositories,
        IEnumerable<RecentRepoInfo> favouriteRepositories)
    {
        TextBlock primaryText = new()
        {
            FontFamily = new FontFamily(AppSettings.Font.Name),
            FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
            FontStyle = AppSettings.Font.Italic ? FontStyle.Italic : FontStyle.Normal,
            FontWeight = AppSettings.Font.Bold ? FontWeight.Bold : FontWeight.Normal,
        };
        TextBlock secondaryText = new()
        {
            FontFamily = new FontFamily(_secondaryFont.Name),
            FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(_secondaryFont.Size),
        };
        Size longestPath = recentRepositories.Union(favouriteRepositories)
            .Select(repository => WinFormsTextMeasurer.MeasureTextRenderer(primaryText, repository.Caption ?? repository.Repo.Path))
            .OrderByDescending(size => size.Width)
            .First();
        Size branchTextSize = WinFormsTextMeasurer.MeasureTextRenderer(secondaryText, "A");

        double width = AppSettings.RecentReposComboMinWidth;
        if (width < 1)
        {
            width = longestPath.Width + imageList1[0].Size.Width;
        }

        double height = longestPath.Height + (2 * branchTextSize.Height)
            + /* offset from top and bottom */ (2 * 2)
            + /* twice space between text */ (2 * 1);
        return new Size(Math.Ceiling(width + 50), Math.Ceiling(Math.Max(height, 50)));
    }

    private static string ShortenText(string text, Font font, float maxWidth)
    {
        const char ellipsis = '…';
        TextBlock measurement = new()
        {
            FontFamily = new FontFamily(font.Name),
            FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(font.Size),
            FontStyle = font.Italic ? FontStyle.Italic : FontStyle.Normal,
            FontWeight = font.Bold ? FontWeight.Bold : FontWeight.Normal,
        };
        if (WinFormsTextMeasurer.MeasureTextRenderer(measurement, text).Width < maxWidth)
        {
            return text;
        }

        while (text.Length > 1
               && WinFormsTextMeasurer.MeasureTextRenderer(measurement, text + ellipsis).Width >= maxWidth)
        {
            text = text[..^1];
        }

        return text + ellipsis;
    }

    private void RepositoryContextAction(Action<SelectedRepositoryItem> action)
    {
        SelectedRepositoryItem? selected = GetSelectedRepositoryItem();
        if (selected is not null)
        {
            action(selected);
        }
    }

    private bool PromptCategoryName(List<string> categories, string? originalName, [NotNullWhen(returnValue: true)] out string? name)
    {
        FormDashboardCategoryTitle dialog = new(categories, originalName);
        if (dialog.ShowDialog(GetOwner()) == WinFormsShims.DialogResult.OK)
        {
            name = dialog.Category;
            return name is not null;
        }

        name = null;
        return false;
    }

    private bool PromptUserConfirm(string question, string caption)
    {
        WinFormsShims.DialogResult dialogResult = MessageBoxes.Show(GetOwner(),
            question,
            caption,
            WinFormsShims.MessageBoxButtons.YesNo,
            WinFormsShims.MessageBoxIcon.Question,
            WinFormsShims.MessageBoxDefaultButton.Button2);

        return dialogResult == WinFormsShims.DialogResult.Yes;
    }

    private void UpdateCategoryName(string? originalName, string? newName)
    {
        foreach (Repository repository in GetRepositories().Where(r => r.Category == originalName))
        {
            ThreadHelper.JoinableTaskFactory.Run(() => Controller.AssignCategoryAsync(repository, newName));
        }

        ShowRecentRepositories();
    }

    private void contextMenuStrip_Closed(object? sender, EventArgs e)
    {
        _rightClickedItem = null;
        ShowRecentRepositories();
    }

    private void contextMenuStrip_Opening(object? sender, CancelEventArgs e)
    {
        RepositoryListItem? selected = listView1.SelectedItem as RepositoryListItem;
        tsmiOpenFolder.IsVisible = selected is not null;
        toolStripMenuItem1.IsVisible = selected is not null;
        tsmiCategories.IsVisible = selected is not null;
        toolStripMenuItem2.IsVisible = selected is not null;
        tsmiRemoveFromList.IsVisible = selected is not null;
        tsmiRemoveMissingReposFromList.IsVisible = _hasInvalidRepos;

        if (selected is null || _rightClickedItem is null)
        {
            e.Cancel = true;
            return;
        }

        // Avalonia menu items share their owning context menu instead of WinForms SourceControl.
        // Keep the clicked container so nested category actions resolve the same repository.
    }

    private Control listView1_DrawItem(object? item)
    {
        // render anchor icon
        // render icon
        // render path
        // render branch
        // render category
        return CreateRowCore(item);
    }

    private void ListView1_GroupTaskLinkClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: RepositoryGroupItem group } button)
        {
            return;
        }

        _selectedCategory = group;
        tsmiCategoryDelete.IsVisible = !group.IsRecentGroup;
        tsmiCategoryRename.IsVisible = !group.IsRecentGroup;
        tsmiCategoryClear.IsVisible = group.IsRecentGroup;
        contextMenuStripCategory.Open(button);
    }

    private void listView1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control source
            || (source as ListBoxItem ?? source.FindAncestorOfType<ListBoxItem>()) is not { DataContext: RepositoryListItem item } container)
        {
            return;
        }

        listView1.SelectedItem = item;
        if (e.GetCurrentPoint(container).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            _rightClickedItem = container;
        }
    }

    private void listView1_MouseClick(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left
            || e.Source is not Control source
            || (source as ListBoxItem ?? source.FindAncestorOfType<ListBoxItem>()) is not { DataContext: RepositoryListItem })
        {
            return;
        }

        TryOpenRepository(GetSelectedRepository());
        e.Handled = true;
    }

    private void TextBoxSearch_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ShowRecentRepositories(reloadData: false);
    }

    private void TextBoxSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Open the first repo in the list
            Repository? repository = listView1.Items.OfType<RepositoryListItem>().FirstOrDefault()?.Repository.Repo;
            if (repository is null)
            {
                return;
            }

            TryOpenRepository(repository);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            listView1.Focus();
            e.Handled = true;
        }
    }

    private void listView1_MouseMove(object? sender, PointerEventArgs e)
    {
        HoveredItem = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true);
    }

    private void listView1_MouseLeave(object? sender, PointerEventArgs e)
    {
        HoveredItem = null;
    }

    private void listView1_GotFocus(object? sender, RoutedEventArgs e)
    {
        if (listView1.SelectedItem is null)
        {
            listView1.SelectedItem = listView1.Items.OfType<RepositoryListItem>().FirstOrDefault();
        }
    }

    private void listView1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (ProcessDialogKey(KeysMapper.ToKeys(e)))
        {
            e.Handled = true;
        }
        else if (e.Key == Key.Up
                 && listView1.SelectedItem is RepositoryListItem selected
                 && listView1.Items.OfType<RepositoryListItem>().FirstOrDefault() == selected)
        {
            // Compare current item to the very first item to see if it's at the top
            textBoxSearch.Focus();
            listView1.SelectedItem = selected;
            e.Handled = true;
        }
    }

    private void mnuConfigure_Click(object? sender, RoutedEventArgs e)
    {
        using FormRecentReposSettings frm = new();
        WinFormsShims.DialogResult result = frm.ShowDialog(GetOwner());
        if (result == WinFormsShims.DialogResult.OK)
        {
            _repositoryHistoryUIService?.Invalidate();
            ShowRecentRepositories();
        }
    }

    private void RecentRepositoriesList_Load(object? sender, EventArgs e)
    {
        if (this.FindLogicalAncestorOfType<FormBrowse>() is not FormBrowse form)
        {
            return;
        }

        WinFormsControls.ToolStripMenuItem? dashboardMenu =
            form.FindControl<WinFormsControls.ToolStripMenuItem>("dashboardToolStripMenuItem");
        if (dashboardMenu is not null && !dashboardMenu.Items.Contains(mnuConfigure))
        {
            dashboardMenu.Items.Add(mnuConfigure);
        }

        Dispatcher.UIThread.Post(() => textBoxSearch.Focus(), DispatcherPriority.Loaded);
    }

    private void tsmiCategories_DropDownOpening(object? sender, EventArgs e)
    {
        if (sender != tsmiCategories)
        {
            return;
        }

        tsmiCategories.Items.Clear();
        List<string> categories = GetCategories();
        if (categories.Count > 0)
        {
            tsmiCategories.Items.Add(tsmiCategoryNone);
            foreach (string category in categories)
            {
                MenuItem item = new() { Header = category, Tag = category };
                item.Click += tsmiCategory_Click;
                tsmiCategories.Items.Add(item);
            }

            tsmiCategories.Items.Add(new Separator());
        }

        tsmiCategories.Items.Add(tsmiCategoryAdd);
        RepositoryContextAction(selectedRepositoryItem =>
        {
            foreach (MenuItem item in tsmiCategories.Items.OfType<MenuItem>())
            {
                item.IsEnabled = item == tsmiCategoryAdd
                    || !Equals(item.Tag, selectedRepositoryItem.Repository.Category);
            }

            if (string.IsNullOrWhiteSpace(selectedRepositoryItem.Repository.Category) && categories.Count > 0)
            {
                tsmiCategoryNone.IsEnabled = false;
            }
        });
    }

    private void tsmiCategory_Click(object? sender, RoutedEventArgs e)
    {
        SelectedRepositoryItem? selectedRepositoryItem = GetSelectedRepositoryItem();
        if (selectedRepositoryItem is null)
        {
            return;
        }

        string? category = (sender as MenuItem)?.Tag as string;
        ThreadHelper.JoinableTaskFactory.Run(() => Controller.AssignCategoryAsync(selectedRepositoryItem.Repository, category));
        ShowRecentRepositories();
    }

    private void tsmiCategoryAdd_Click(object? sender, RoutedEventArgs e)
    {
        RepositoryContextAction(selectedRepositoryItem =>
        {
            if (PromptCategoryName(GetCategories(), originalName: null, out string? categoryName))
            {
                ThreadHelper.JoinableTaskFactory.Run(() => Controller.AssignCategoryAsync(selectedRepositoryItem.Repository, categoryName));
                ShowRecentRepositories();
            }
        });
    }

    private void tsmiOpenFolder_Click(object? sender, RoutedEventArgs e)
        => RepositoryContextAction(selectedRepositoryItem => OsShellUtil.OpenWithFileExplorer(selectedRepositoryItem.Repository.Path));

    private void tsmiRemoveFromList_Click(object? sender, RoutedEventArgs e)
    {
        RepositoryContextAction(selectedRepositoryItem =>
        {
            ThreadHelper.JoinableTaskFactory.Run(() =>
                selectedRepositoryItem.IsFavourite
                    ? RepositoryHistoryManager.Locals.RemoveFavouriteAsync(selectedRepositoryItem.Repository.Path)
                    : RepositoryHistoryManager.Locals.RemoveRecentAsync(selectedRepositoryItem.Repository.Path));
            ShowRecentRepositories();
        });
    }

    private void tsmiRemoveMissingReposFromList_Click(object? sender, RoutedEventArgs e)
    {
        RepositoryContextAction(_ =>
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(Controller.IsValidGitWorkingDir));
            ShowRecentRepositories();
        });
    }

    private void tsmiCategoryRename_Click(object? sender, RoutedEventArgs e)
    {
        string? originalName = _selectedCategory?.Name;
        List<string> categories = GetCategories();
        categories.Remove(originalName!);

        if (PromptCategoryName(categories, originalName, out string? newName))
        {
            UpdateCategoryName(originalName, newName);
        }
    }

    private void tsmiCategoryDelete_Click(object? sender, RoutedEventArgs e)
    {
        string? name = _selectedCategory?.Name;
        string question = string.Format(_deleteCategoryQuestion.Text, name, _selectedCategory?.RepositoryCount ?? 0);
        if (!PromptUserConfirm(question, _deleteCategoryCaption.Text))
        {
            return;
        }

        UpdateCategoryName(name, null);
    }

    private void tsmiCategoryClear_Click(object? sender, RoutedEventArgs e)
    {
        List<Repository> repositories = [.. GetRepositories()];
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

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        string[] fileNameArray = GetDroppedFileNames(e.DataTransfer);
        if (fileNameArray.Length != 1)
        {
            return;
        }

        string dir = fileNameArray[0];
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            IGitExecutorProvider executorProvider = _getUICommands?.Invoke().GetRequiredService<IGitExecutorProvider>()
                ?? throw new InvalidOperationException("The repository list is not initialized.");
            GitModule module = new(executorProvider, dir);
            if (!module.IsValidGitWorkingDir())
            {
                MessageBoxes.Show(GetOwner(), TranslatedStrings.DirectoryInvalidRepository,
                    _cannotOpenTheFolder.Text, WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Exclamation, WinFormsShims.MessageBoxDefaultButton.Button1);
                return;
            }

            OnModuleChanged(new GitModuleEventArgs(module));
        }
    }

    private static void OnDragEnter(object? sender, DragEventArgs e)
    {
        string[] fileNameArray = GetDroppedFileNames(e.DataTransfer);

        // Allow drop (copy, not move) folders
        e.DragEffects = CanDropRepositoryDirectory(fileNameArray)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    internal static bool CanDropRepositoryDirectory(IReadOnlyList<string> fileNameArray)
        => fileNameArray.Count == 1
            && !string.IsNullOrEmpty(fileNameArray[0])
            && Directory.Exists(fileNameArray[0]);

    private static string[] GetDroppedFileNames(IDataTransfer dataTransfer)
    {
        // Avalonia exposes native dropped paths as storage items instead of a FileDrop string array.
        return [.. (dataTransfer.TryGetFiles() ?? [])
            .Select(file => file.TryGetLocalPath())
            .OfType<string>()];
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

        if (Controller.IsValidGitWorkingDir(repository.Path))
        {
            IGitExecutorProvider executorProvider = _getUICommands?.Invoke().GetRequiredService<IGitExecutorProvider>()
                ?? throw new InvalidOperationException("The repository list is not initialized.");
            OnModuleChanged(new GitModuleEventArgs(new GitModule(executorProvider, repository.Path)));
            return true;
        }

        if (Controller.RemoveInvalidRepository(repository.Path))
        {
            ShowRecentRepositories();
            return true;
        }

        return true;
    }

    private void ListView1_ContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        bool isHeader = e.Index >= 0
            && e.Index < listView1.ItemCount
            && listView1.Items[e.Index] is RepositoryGroupItem;
        e.Container.IsEnabled = true;
        e.Container.Focusable = !isHeader;
        e.Container.Classes.Set("repository-group", isHeader);
        e.Container.Classes.Set("repository-tile", !isHeader);
    }

    private void RepositoryHistoryUIService_HistoryChanged(object? sender, EventArgs e)
        => this.InvokeAndForget(() =>
        {
            Controller.ClearCache();
            ShowRecentRepositories(reloadData: false);
            return Task.CompletedTask;
        });

    private WinFormsShims.IWin32Window? GetOwner()
        => this.FindAncestorOfType<Window>() as WinFormsShims.IWin32Window;

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(UserRepositoriesList control)
    {
        internal TextBox Search => control.textBoxSearch;
        internal ListBox List => control.listView1;
        internal MenuItem Configure => control.mnuConfigure;
        internal MenuItem Categories => control.tsmiCategories;
        internal MenuItem CategoryNone => control.tsmiCategoryNone;
        internal MenuItem CategoryAdd => control.tsmiCategoryAdd;
        internal MenuItem Remove => control.tsmiRemoveFromList;
        internal void OpenSelected() => control.TryOpenRepository(control.GetSelectedRepository());
        internal void OpenCategories() => control.tsmiCategories_DropDownOpening(control.tsmiCategories, EventArgs.Empty);
        internal bool UpdateContextMenu()
        {
            CancelEventArgs eventArgs = new();
            control._rightClickedItem = new ListBoxItem { DataContext = control.listView1.SelectedItem };
            control.contextMenuStrip_Opening(control.contextMenuStripRepository, eventArgs);
            return !eventArgs.Cancel;
        }
    }

    internal sealed record RepositoryListItem(
        string Text,
        RecentRepoInfo Repository,
        string BranchName,
        bool IsFavourite,
        bool IsValid,
        Size TileSize);

    internal sealed class RepositoryGroupItem(string name, bool isRecentGroup, int repositoryCount)
    {
        public string Name { get; } = name;

        public bool IsRecentGroup { get; } = isRecentGroup;

        public int RepositoryCount { get; set; } = repositoryCount;
    }
}
