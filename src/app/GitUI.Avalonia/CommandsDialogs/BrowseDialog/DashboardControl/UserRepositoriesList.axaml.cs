using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
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
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

public partial class UserRepositoriesList : TranslatedControl
{
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
    private Func<IGitUICommands>? _getUICommands;
    private bool _hasInvalidRepos;
    private bool _isSubscribed;
    private IUserRepositoriesListController? _controller;
    private IRepositoryHistoryUIService? _repositoryHistoryUIService;
    private RepositoryListItem? _rightClickedItem;
    private RepositoryGroupItem? _selectedCategory;

    public event EventHandler? ConfigureRequested;
    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;

    private IUserRepositoriesListController Controller
        => _controller ?? throw new InvalidOperationException("The repository list is not initialized.");

    public UserRepositoriesList()
    {
        InitializeComponent();
        InitializeComplete();

        listView1.ItemTemplate = new FuncDataTemplate<object>(
            (item, _) => CreateRow(item),
            supportsRecycling: false);
        listView1.ContainerPrepared += ListView1_ContainerPrepared;
        listView1.AddHandler(PointerPressedEvent, listView1_PointerPressed, RoutingStrategies.Tunnel);
        listView1.AddHandler(PointerReleasedEvent, listView1_PointerReleased, RoutingStrategies.Tunnel);
        listView1.KeyDown += listView1_KeyDown;
        listView1.GotFocus += listView1_GotFocus;
        textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
        textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
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
            if (e.Property == IsVisibleProperty && IsVisible)
            {
                // Reset the search
                textBoxSearch.Text = string.Empty;
                textBoxSearch.Focus();
            }
        };

        DragDrop.SetAllowDrop(this, true);
        DragDrop.AddDragEnterHandler(this, OnDragEnter);
        DragDrop.AddDragOverHandler(this, OnDragEnter);
        DragDrop.AddDropHandler(this, OnDragDrop);
        textBoxSearch.PlaceholderText = _repositorySearchPlaceholder.Text;
    }

    [Category("Appearance")]
    public Color BranchNameColor
    {
        get => _branchNameColor;
        set => SetAppearance(ref _branchNameColor, value);
    }

    [Category("Appearance")]
    public Color FavouriteColor
    {
        get => _favouriteColor;
        set => SetAppearance(ref _favouriteColor, value);
    }

    [Category("Appearance")]
    public Color ForeColor
    {
        get => _foreColor;
        set => SetAppearance(ref _foreColor, value);
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
            Resources["DashboardRepositoryHoverBrush"] = new SolidColorBrush(value);
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

    private static StringComparer GroupHeaderComparer => StringComparer.CurrentCulture;

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        textBoxSearch.PlaceholderText = _repositorySearchPlaceholder.Text;
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

        List<object> rows = [];
        _hasInvalidRepos = false;
        BindRepositories(rows, _groupRecentRepositories.Text, recentRepositories, isFavourite: false, isRecentGroup: true);
        foreach (IGrouping<string?, RecentRepoInfo> category in favouriteRepositories
                     .GroupBy(repo => repo.Repo.Category, GroupHeaderComparer)
                     .OrderBy(group => group.Key, GroupHeaderComparer))
        {
            BindRepositories(rows, category.Key ?? string.Empty, category, isFavourite: true, isRecentGroup: false);
        }

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

    protected virtual void OnModuleChanged(GitModuleEventArgs args)
    {
        EventHandler<GitModuleEventArgs>? handler = GitModuleChanged;
        handler?.Invoke(this, args);
    }

    private void BindRepositories(
        ICollection<object> rows,
        string groupName,
        IEnumerable<RecentRepoInfo> repositories,
        bool isFavourite,
        bool isRecentGroup)
    {
        RecentRepoInfo[] items = [.. repositories];
        if (items.Length == 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(groupName))
        {
            rows.Add(new RepositoryGroupItem(groupName, isRecentGroup, items.Length));
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
                isValidGitDir));
        }
    }

    private Control CreateRow(object? item)
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
            actions.Click += ListView1_GroupTaskLinkClick;
            Grid header = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Avalonia.Thickness(2, 10, 2, 4) };
            header.Children.Add(new TextBlock
            {
                Text = group.Name,
                FontSize = 16,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(HeaderColor),
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
            });
            Grid.SetColumn(actions, 1);
            header.Children.Add(actions);
            return header;
        }

        RepositoryListItem repository = (RepositoryListItem)item;
        Image image = new()
        {
            Width = 20,
            Height = 20,
            Source = repository.IsValid ? Images.DashboardFolderGit : Images.DashboardFolderError,
            Margin = new Avalonia.Thickness(0, 1, 8, 0),
        };
        if (!string.IsNullOrWhiteSpace(repository.Repository.Repo.Category))
        {
            image.Opacity = 0.9;
        }

        StackPanel text = new()
        {
            Spacing = 1,
            Children =
            {
                new TextBlock
                {
                    Text = repository.Text,
                    Foreground = new SolidColorBrush(ForeColor),
                    FontWeight = repository.IsFavourite ? FontWeight.SemiBold : FontWeight.Normal,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
                new TextBlock
                {
                    Text = repository.BranchName,
                    Foreground = new SolidColorBrush(BranchNameColor),
                    FontSize = 11,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
            },
        };
        StackPanel row = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Avalonia.Thickness(4, 3),
            Children = { image, text },
        };
        ToolTip.SetTip(row, repository.Repository.Repo.Path);
        return row;
    }

    private void SetAppearance(ref Color field, Color value)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        ShowRecentRepositories(reloadData: false);
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
        RepositoryListItem? selected = _rightClickedItem ?? listView1.SelectedItem as RepositoryListItem;
        if (string.IsNullOrWhiteSpace(selected?.Repository.Repo.Path))
        {
            return null;
        }

        return new SelectedRepositoryItem(selected.IsFavourite, selected.Repository.Repo);
    }

    private Repository? GetSelectedRepository()
        => (listView1.SelectedItem as RepositoryListItem)?.Repository.Repo;

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
        }
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
            _rightClickedItem = item;
        }
    }

    private void listView1_PointerReleased(object? sender, PointerReleasedEventArgs e)
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

    private void listView1_GotFocus(object? sender, RoutedEventArgs e)
    {
        if (listView1.SelectedItem is null)
        {
            listView1.SelectedItem = listView1.Items.OfType<RepositoryListItem>().FirstOrDefault();
        }
    }

    private void listView1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TryOpenRepository(GetSelectedRepository());
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
        ConfigureRequested?.Invoke(this, EventArgs.Empty);
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
        internal Button Configure => control.mnuConfigure;
        internal MenuItem Categories => control.tsmiCategories;
        internal MenuItem CategoryNone => control.tsmiCategoryNone;
        internal MenuItem CategoryAdd => control.tsmiCategoryAdd;
        internal MenuItem Remove => control.tsmiRemoveFromList;
        internal void OpenSelected() => control.TryOpenRepository(control.GetSelectedRepository());
        internal void OpenCategories() => control.tsmiCategories_DropDownOpening(control.tsmiCategories, EventArgs.Empty);
        internal bool UpdateContextMenu()
        {
            CancelEventArgs eventArgs = new();
            control._rightClickedItem = control.listView1.SelectedItem as RepositoryListItem;
            control.contextMenuStrip_Opening(control.contextMenuStripRepository, eventArgs);
            return !eventArgs.Cancel;
        }
    }

    internal sealed record RepositoryListItem(
        string Text,
        RecentRepoInfo Repository,
        string BranchName,
        bool IsFavourite,
        bool IsValid);

    internal sealed record RepositoryGroupItem(
        string Name,
        bool IsRecentGroup,
        int RepositoryCount);
}
