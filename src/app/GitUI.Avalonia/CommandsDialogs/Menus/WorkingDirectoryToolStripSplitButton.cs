using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using GitUI.Properties;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs.Menus;

/// <summary>
///  Represents a split button that contains the recent repositories.
/// </summary>
internal sealed class WorkingDirectoryToolStripSplitButton : IconSplitButton, ITranslate
{
    private const string TranslationCategory = nameof(FormBrowse);

    private static readonly TranslationString _configureWorkingDirMenu = new("Co&nfigure this menu...");
    private static readonly TranslationString _noWorkingFolderText = new("No working directory");
    private static readonly TranslationString _repositorySearchPlaceholder = new("Search repositories...");
    private static readonly TranslationString _toolTip = new("""
        Change working directory
        Left click opens the drop-down menu.
        Then hold Ctrl in order to open the selected repository in a new instance.
        Right click starts the "Open repository" dialog.
        """);

    private readonly HashSet<MenuItem> _fixedItems = [];
    private readonly MenuItem _filterHost;
    private readonly MenuFlyout _menu = new();
    private readonly TextBox _txtFilter = new()
    {
        MinWidth = 260,
    };

    private bool _dropDownPreparedForTest;
    private Action? _closeRepository;
    private Action? _configure;
    private Func<IGitUICommands>? _getUICommands;
    private IRepositoryHistoryUIService? _repositoryHistoryUIService;
    private Action<string>? _launchRepository;
    private Action? _openRepository;
    private Action<string>? _setWorkingDirectory;
    private KeyGesture? _closeRepositoryGesture;
    private KeyGesture? _openRepositoryGesture;
    private string _closeRepositoryText = "Close repository";
    private string _favouriteRepositoriesText = "&Favorite repositories";
    private string _openRepositoryText = "Open repository";

    public WorkingDirectoryToolStripSplitButton()
    {
        Name = nameof(WorkingDirectoryToolStripSplitButton);
        Content = "WorkingDir";
        Icon = Images.RepoOpen;
        Flyout = _menu;
        ToolTip.SetTip(this, _toolTip.Text);
        TranslationCompat.SetUseToolTipText(this, true);

        // A focusable MenuItem consumes the pointer focus intended for its TextBox header.
        _filterHost = new MenuItem
        {
            Focusable = false,
            Header = _txtFilter,
            StaysOpenOnClick = true,
        };
        _menu.Items.Add(_filterHost);
        Click += (_, _) => OpenFlyout();
        _menu.Opening += Menu_Opening;
        _txtFilter.TextChanged += (_, _) => ApplyFilter();
        _txtFilter.KeyDown += TxtFilter_KeyDown;
        AddHandler(PointerReleasedEvent, MouseUpHandler, RoutingStrategies.Tunnel);
    }

    /// <summary>
    ///  Initializes the menu item.
    /// </summary>
    /// <param name="getUICommands">The method that returns the current UI commands.</param>
    /// <param name="setWorkingDirectory">Changes the repository in the current window.</param>
    /// <param name="launchRepository">Opens a repository in a new application instance.</param>
    /// <param name="openRepository">Opens the repository folder picker.</param>
    /// <param name="closeRepository">Closes the repository in the current window.</param>
    /// <param name="configure">Opens the recent-repository settings dialog.</param>
    public void Initialize(
        Func<IGitUICommands> getUICommands,
        IRepositoryHistoryUIService repositoryHistoryUIService,
        Action<string> setWorkingDirectory,
        Action<string> launchRepository,
        Action openRepository,
        Action closeRepository,
        Action configure)
    {
        ArgumentNullException.ThrowIfNull(getUICommands);
        ArgumentNullException.ThrowIfNull(repositoryHistoryUIService);
        ArgumentNullException.ThrowIfNull(setWorkingDirectory);
        ArgumentNullException.ThrowIfNull(launchRepository);
        ArgumentNullException.ThrowIfNull(openRepository);
        ArgumentNullException.ThrowIfNull(closeRepository);
        ArgumentNullException.ThrowIfNull(configure);

        _getUICommands = getUICommands;
        _repositoryHistoryUIService = repositoryHistoryUIService;
        _setWorkingDirectory = setWorkingDirectory;
        _launchRepository = launchRepository;
        _openRepository = openRepository;
        _closeRepository = closeRepository;
        _configure = configure;
        Translator.Translate(this, AppSettings.CurrentTranslation);
        RefreshContent();
    }

    /// <summary>Updates the text shown on the combo button itself.</summary>
    public void RefreshContent()
    {
        if (_getUICommands is null)
        {
            return;
        }

        string path = _getUICommands().Module.WorkingDir;
        if (string.IsNullOrWhiteSpace(path))
        {
            Content = _noWorkingFolderText.Text;
            MinWidth = AppSettings.RecentReposComboMinWidth;
            return;
        }

        IList<Repository> recentRepositoryHistory = _repositoryHistoryUIService?.AddAsMostRecent(path)
            ?? ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));
        RefreshContent(path, recentRepositoryHistory);
    }

    private void RefreshContent(string path, IList<Repository> recentRepositoryHistory)
    {
        List<RecentRepoInfo> pinnedRepos = [];
        RecentRepoSplitter splitter = new()
        {
            MeasureFont = AppSettings.Font,
        };
        splitter.SplitRecentRepos(recentRepositoryHistory, pinnedRepos, pinnedRepos);
        RecentRepoInfo? repositoryInfo = pinnedRepos.Find(
            item => item.Repo.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

        Content = PathUtil.GetDisplayPath(repositoryInfo?.Caption ?? path);
        MinWidth = AppSettings.RecentReposComboMinWidth;
    }

    public void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
    {
        _openRepositoryGesture = KeysMapper.ToKeyGesture(
            hotkeys?.FirstOrDefault(command => command.CommandCode == (int)FormBrowse.Command.OpenRepo)?.KeyData);
        _closeRepositoryGesture = KeysMapper.ToKeyGesture(
            hotkeys?.FirstOrDefault(command => command.CommandCode == (int)FormBrowse.Command.CloseRepository)?.KeyData);
    }

    private void FillDropDown()
    {
        if (_repositoryHistoryUIService is not null)
        {
            RepositoryHistorySnapshot snapshot = _repositoryHistoryUIService.LoadSnapshot();
            FillDropDown(snapshot);
            return;
        }

        IList<Repository> favourites = ThreadHelper.JoinableTaskFactory.Run(
            RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);
        IList<Repository> recent = ThreadHelper.JoinableTaskFactory.Run(
            RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        FillDropDown(favourites, recent);
    }

    private void FillDropDown(RepositoryHistorySnapshot snapshot)
    {
        ResetDropDown();
        AddFavouriteRepositories(snapshot.Favourites);
        AddRecentRepositories(snapshot.Recent);
        AddFixedItems();
    }

    private void Menu_Opening(object? sender, EventArgs e)
    {
        if (_dropDownPreparedForTest)
        {
            _dropDownPreparedForTest = false;
            return;
        }

        FillDropDown();
    }

    private void FillDropDown(IList<Repository> favourites, IList<Repository> recent)
    {
        ResetDropDown();

        AddFavouriteRepositories(favourites);
        AddRecentRepositories(recent);
        AddFixedItems();
    }

    private void ResetDropDown()
    {
        while (_menu.Items.Count > 1)
        {
            _menu.Items.RemoveAt(1);
        }

        _fixedItems.Clear();
        _txtFilter.Text = string.Empty;
        _txtFilter.PlaceholderText = _repositorySearchPlaceholder.Text;
        _menu.Items.Add(new Separator());
    }

    private void AddFixedItems()
    {
        _menu.Items.Add(new Separator());
        AddFixedItem(_openRepositoryText, Images.RepoOpen, _openRepositoryGesture, _openRepository);
        AddFixedItem(_closeRepositoryText, icon: null, _closeRepositoryGesture, _closeRepository);
        _menu.Items.Add(new Separator());
        AddFixedItem(
            AvaloniaTranslationUtils.ToAvaloniaMnemonics(_configureWorkingDirMenu.Text),
            Images.RecentRepositories,
            gesture: null,
            _configure);
    }

    private void AddFavouriteRepositories(IReadOnlyList<RepositoryHistoryEntry> repositories)
    {
        if (repositories.Count == 0)
        {
            return;
        }

        MenuItem favourites = new()
        {
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_favouriteRepositoriesText),
            Icon = CreateIcon(Images.Pin),
        };
        foreach (IGrouping<string?, RepositoryHistoryEntry> category in repositories
                     .GroupBy(item => item.Repository.Category)
                     .OrderBy(item => item.Key))
        {
            MenuItem categoryItem = new() { Header = category.Key ?? string.Empty };
            int number = 0;
            foreach (RepositoryHistoryEntry repository in category)
            {
                categoryItem.Items.Add(CreateRepositoryItem(repository, ++number));
            }

            favourites.Items.Add(categoryItem);
        }

        _menu.Items.Add(favourites);
    }

    private void AddRecentRepositories(IReadOnlyList<RepositoryHistoryEntry> repositories)
    {
        int number = 0;
        foreach (RepositoryHistoryEntry repository in repositories)
        {
            _menu.Items.Add(CreateRepositoryItem(repository, ++number));
        }
    }

    private void AddFavouriteRepositories(IList<Repository> repositories)
    {
        if (repositories.Count == 0)
        {
            return;
        }

        List<RecentRepoInfo> top = [];
        List<RecentRepoInfo> recent = [];
        RecentRepoSplitter splitter = new()
        {
            MeasureFont = AppSettings.Font,
        };
        splitter.SplitRecentRepos(repositories, top, recent);

        MenuItem favourites = new()
        {
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_favouriteRepositoriesText),
            Icon = CreateIcon(Images.Pin),
        };
        foreach (IGrouping<string?, RecentRepoInfo> category in top
                     .Union(recent)
                     .GroupBy(item => item.Repo.Category)
                     .OrderBy(item => item.Key))
        {
            MenuItem categoryItem = new()
            {
                Header = category.Key ?? string.Empty,
            };
            int number = 0;
            foreach (RecentRepoInfo repository in category)
            {
                categoryItem.Items.Add(CreateRepositoryItem(repository, ++number));
            }

            favourites.Items.Add(categoryItem);
        }

        _menu.Items.Add(favourites);
    }

    private void AddRecentRepositories(IList<Repository> repositories)
    {
        List<RecentRepoInfo> pinned = [];
        List<RecentRepoInfo> recent = [];
        RecentRepoSplitter splitter = new()
        {
            MeasureFont = AppSettings.Font,
        };
        splitter.SplitRecentRepos(repositories, pinned, recent);

        int number = 0;
        foreach (RecentRepoInfo repository in pinned)
        {
            _menu.Items.Add(CreateRepositoryItem(repository, ++number));
        }

        if (pinned.Count > 0 && recent.Count > 0)
        {
            _menu.Items.Add(new Separator());
        }

        foreach (RecentRepoInfo repository in recent)
        {
            _menu.Items.Add(CreateRepositoryItem(repository, ++number));
        }
    }

    private MenuItem CreateRepositoryItem(RecentRepoInfo repository, int number)
    {
        string numberString = number switch
        {
            < 10 => $"_{number}",
            10 => "1_0",
            _ => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        MenuItem item = new()
        {
            Header = $"{numberString}: {repository.Caption}",
            Tag = repository,
            Icon = repository.Anchored ? CreateIcon(Images.Pin) : null,
        };
        ToolTip.SetTip(item, repository.Repo.Path == repository.Caption ? null : repository.Repo.Path);
        item.PointerPressed += RepositoryItem_PointerPressed;
        item.KeyDown += RepositoryItem_KeyDown;
        item.Click += RepositoryItem_Click;
        return item;
    }

    private MenuItem CreateRepositoryItem(RepositoryHistoryEntry repository, int number)
    {
        string numberString = number switch
        {
            < 10 => $"_{number}",
            10 => "1_0",
            _ => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        MenuItem item = new()
        {
            Header = CreateRepositoryHeader(
                $"{numberString}: {repository.Caption}",
                repository.BranchName),
            Tag = repository,
            Icon = repository.IsAnchored ? CreateIcon(Images.Pin) : null,
        };
        ToolTip.SetTip(
            item,
            string.IsNullOrWhiteSpace(repository.BranchName)
                ? repository.Repository.Path
                : $"{repository.Repository.Path}{Environment.NewLine}{repository.BranchName}");
        item.PointerPressed += RepositoryItem_PointerPressed;
        item.KeyDown += RepositoryItem_KeyDown;
        item.Click += RepositoryItem_Click;
        return item;
    }

    private static Control CreateRepositoryHeader(string caption, string? branchName)
    {
        Grid header = new()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,18,Auto"),
            MinWidth = 260,
        };
        header.Children.Add(new TextBlock
        {
            Text = caption,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        });
        if (!string.IsNullOrWhiteSpace(branchName))
        {
            TextBlock branch = new()
            {
                Text = branchName,
                Opacity = 0.7,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            Grid.SetColumn(branch, 2);
            header.Children.Add(branch);
        }

        return header;
    }

    private void RepositoryItem_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            item.SetValue(OpenInNewInstanceProperty, e.KeyModifiers.HasFlag(KeyModifiers.Control));
        }
    }

    private void RepositoryItem_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is MenuItem item)
        {
            item.SetValue(OpenInNewInstanceProperty, e.KeyModifiers.HasFlag(KeyModifiers.Control));
        }
    }

    private void RepositoryItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem item)
        {
            return;
        }

        bool openInNewInstance = item.GetValue(OpenInNewInstanceProperty);
        item.ClearValue(OpenInNewInstanceProperty);
        string? path = item.Tag switch
        {
            RecentRepoInfo repository => repository.Repo.Path,
            RepositoryHistoryEntry repository => repository.Repository.Path,
            _ => null,
        };
        if (path is not null)
        {
            OpenRepository(path, openInNewInstance);
        }
    }

    private void OpenRepository(string path, bool openInNewInstance)
    {
        if (_repositoryHistoryUIService is not null
            && !_repositoryHistoryUIService.CanOpenRepository(path))
        {
            return;
        }

        if (openInNewInstance)
        {
            _launchRepository?.Invoke(path);
        }
        else
        {
            _setWorkingDirectory?.Invoke(path);
        }
    }

    private void AddFixedItem(string header, Avalonia.Media.IImage? icon, KeyGesture? gesture, Action? action)
    {
        MenuItem item = new()
        {
            Header = header,
            Icon = CreateIcon(icon),
            InputGesture = gesture,
        };
        item.Click += (_, _) => action?.Invoke();
        _fixedItems.Add(item);
        _menu.Items.Add(item);
    }

    private void ApplyFilter()
    {
        string filter = _txtFilter.Text?.Trim() ?? string.Empty;
        foreach (object? entry in _menu.Items)
        {
            if (entry is MenuItem item && !_fixedItems.Contains(item) && item.Header != _txtFilter)
            {
                ApplyFilter(item, filter);
            }
        }
    }

    private static bool ApplyFilter(MenuItem item, string filter)
    {
        bool childMatch = false;
        foreach (object? child in item.Items)
        {
            if (child is MenuItem childItem)
            {
                childMatch |= ApplyFilter(childItem, filter);
            }
        }

        string text = item.Tag switch
        {
            RepositoryHistoryEntry repository
                => $"{repository.Caption} {repository.Repository.Path} {repository.BranchName}",
            RecentRepoInfo repository
                => $"{repository.Caption} {repository.Repo.Path}",
            _ => item.Header as string ?? string.Empty,
        };
        bool match = string.IsNullOrWhiteSpace(filter)
            || text.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
            || childMatch;
        item.IsVisible = match;
        return match;
    }

    private void TxtFilter_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _txtFilter.Text = string.Empty;
            e.Handled = true;
        }
    }

    private void MouseUpHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            _openRepository?.Invoke();
            e.Handled = true;
        }
    }

    private static Image? CreateIcon(Avalonia.Media.IImage? image)
        => image is null
            ? null
            : new Image
            {
                Width = 16,
                Height = 16,
                Source = image,
            };

    internal void AddControlTranslationItems(ITranslation translation)
    {
        TranslationUtils.AddTranslationItemsFromFields(TranslationCategory, this, translation);
        translation.AddTranslationItem(TranslationCategory, "tsmiFavouriteRepositories", "Text", "&Favorite repositories");
        translation.AddTranslationItem(TranslationCategory, "closeToolStripMenuItem", "Text", "&Close repository");
    }

    internal void TranslateControlItems(ITranslation translation)
    {
        TranslationUtils.TranslateItemsFromFields(TranslationCategory, this, translation);
        _favouriteRepositoriesText = translation.TranslateItem(
            TranslationCategory,
            "tsmiFavouriteRepositories",
            "Text",
            () => "&Favorite repositories") ?? "&Favorite repositories";
        _openRepositoryText = AvaloniaTranslationUtils.ToAvaloniaMnemonics(
            translation.TranslateItem(
                TranslationCategory,
                "openToolStripMenuItem",
                "Text",
                () => "&Open...") ?? "&Open...");
        _closeRepositoryText = AvaloniaTranslationUtils.ToAvaloniaMnemonics(
            translation.TranslateItem(
                TranslationCategory,
                "closeToolStripMenuItem",
                "Text",
                () => "&Close repository") ?? "&Close repository");
        ToolTip.SetTip(this, _toolTip.Text);
    }

    void ITranslate.AddTranslationItems(ITranslation translation)
        => AddControlTranslationItems(translation);

    void ITranslate.TranslateItems(ITranslation translation)
        => TranslateControlItems(translation);

    void IDisposable.Dispose()
    {
    }

    private static readonly AttachedProperty<bool> OpenInNewInstanceProperty =
        AvaloniaProperty.RegisterAttached<WorkingDirectoryToolStripSplitButton, MenuItem, bool>("OpenInNewInstance");

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(WorkingDirectoryToolStripSplitButton control)
    {
        public MenuFlyout Menu => control._menu;

        public TextBox Filter => control._txtFilter;

        public void FillDropDown(IList<Repository> favourites, IList<Repository> recent)
            => control.FillDropDown(favourites, recent);

        public void FillDropDown(RepositoryHistorySnapshot snapshot)
            => control.FillDropDown(snapshot);

        public void ApplyFilterForTesting() => control.ApplyFilter();

        public void ShowDropDown(IList<Repository> favourites, IList<Repository> recent)
        {
            PrepareDropDown(favourites, recent);
            control._menu.ShowAt(control);
        }

        public void PrepareDropDown(IList<Repository> favourites, IList<Repository> recent)
        {
            control.FillDropDown(favourites, recent);
            control._dropDownPreparedForTest = true;
        }

        public void RefreshContent(string path, IList<Repository> recent)
            => control.RefreshContent(path, recent);

        public void SetRepositoryActions(Action<string> setWorkingDirectory, Action<string> launchRepository)
        {
            control._setWorkingDirectory = setWorkingDirectory;
            control._launchRepository = launchRepository;
        }

        public void SetOpenRepositoryAction(Action openRepository)
            => control._openRepository = openRepository;

        public void OpenRepository(string path, bool openInNewInstance)
            => control.OpenRepository(path, openInNewInstance);
    }
}
