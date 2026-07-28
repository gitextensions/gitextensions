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
    private readonly MenuFlyout _menu = new();
    private readonly TextBox _txtFilter = new()
    {
        MinWidth = 260,
    };

    private bool _dropDownPreparedForTest;
    private Action? _closeRepository;
    private Action? _configure;
    private Func<IGitUICommands>? _getUICommands;
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

        Click += (_, _) => _menu.ShowAt(this);
        _menu.Opening += Menu_Opening;
        _txtFilter.TextChanged += (_, _) => ApplyFilter();
        _txtFilter.KeyDown += TxtFilter_KeyDown;
        AddHandler(PointerPressedEvent, MouseUpHandler, RoutingStrategies.Tunnel);
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
        Action<string> setWorkingDirectory,
        Action<string> launchRepository,
        Action openRepository,
        Action closeRepository,
        Action configure)
    {
        ArgumentNullException.ThrowIfNull(getUICommands);
        ArgumentNullException.ThrowIfNull(setWorkingDirectory);
        ArgumentNullException.ThrowIfNull(launchRepository);
        ArgumentNullException.ThrowIfNull(openRepository);
        ArgumentNullException.ThrowIfNull(closeRepository);
        ArgumentNullException.ThrowIfNull(configure);

        _getUICommands = getUICommands;
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

        IList<Repository> recentRepositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
            () => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));
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
        IList<Repository> favourites = ThreadHelper.JoinableTaskFactory.Run(
            RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);
        IList<Repository> recent = ThreadHelper.JoinableTaskFactory.Run(
            RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        FillDropDown(favourites, recent);
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
        _menu.Items.Clear();
        _fixedItems.Clear();
        _txtFilter.Text = string.Empty;
        _txtFilter.PlaceholderText = _repositorySearchPlaceholder.Text;

        MenuItem filterHost = new()
        {
            Header = _txtFilter,
            StaysOpenOnClick = true,
        };
        _menu.Items.Add(filterHost);
        _menu.Items.Add(new Separator());

        AddFavouriteRepositories(favourites);
        AddRecentRepositories(recent);

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
        if (sender is not MenuItem { Tag: RecentRepoInfo repository } item)
        {
            return;
        }

        bool openInNewInstance = item.GetValue(OpenInNewInstanceProperty);
        item.ClearValue(OpenInNewInstanceProperty);
        OpenRepository(repository.Repo.Path, openInNewInstance);
    }

    private void OpenRepository(string path, bool openInNewInstance)
    {
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

        string text = item.Header as string ?? string.Empty;
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

    private void MouseUpHandler(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
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

    void ITranslate.AddTranslationItems(ITranslation translation)
    {
        TranslationUtils.AddTranslationItemsFromFields(TranslationCategory, this, translation);
        translation.AddTranslationItem(TranslationCategory, "tsmiFavouriteRepositories", "Text", "&Favorite repositories");
        translation.AddTranslationItem(TranslationCategory, "openToolStripMenuItem", "Text", "Open repository...");
        translation.AddTranslationItem(TranslationCategory, "closeToolStripMenuItem", "Text", "&Close repository");
    }

    void ITranslate.TranslateItems(ITranslation translation)
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
                () => "Open repository...") ?? "Open repository...");
        _closeRepositoryText = AvaloniaTranslationUtils.ToAvaloniaMnemonics(
            translation.TranslateItem(
                TranslationCategory,
                "closeToolStripMenuItem",
                "Text",
                () => "&Close repository") ?? "&Close repository");
        ToolTip.SetTip(this, _toolTip.Text);
    }

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

        public void ApplyFilter() => control.ApplyFilter();

        public void ShowDropDown(IList<Repository> favourites, IList<Repository> recent)
        {
            control.FillDropDown(favourites, recent);
            control._dropDownPreparedForTest = true;
            control._menu.ShowAt(control);
        }

        public void RefreshContent(string path, IList<Repository> recent)
            => control.RefreshContent(path, recent);

        public void SetRepositoryActions(Action<string> setWorkingDirectory, Action<string> launchRepository)
        {
            control._setWorkingDirectory = setWorkingDirectory;
            control._launchRepository = launchRepository;
        }

        public void OpenRepository(string path, bool openInNewInstance)
            => control.OpenRepository(path, openInNewInstance);
    }
}
