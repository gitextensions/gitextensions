using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
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
    private static readonly TranslationString _noWorkingFolderText = new("No working directory");

    private static readonly TranslationString _configureWorkingDirMenu = new("Co&nfigure this menu...");
    private static readonly TranslationString _repositorySearchPlaceholder = new("Search repositories...");
    private static readonly TranslationString _toolTip = new("""
        Change working directory
        Left click opens the drop-down menu.
        Then hold Ctrl in order to open the selected repository in a new instance.
        Right click starts the "Open repository" dialog.
        """);

    private sealed class Implementation(WorkingDirectoryToolStripSplitButton button)
    {
        /// <summary>
        ///  Gets the current instance of the git module.
        /// </summary>
        private IGitModule? Module => button._getUICommands?.Invoke().Module;

        /// <summary>
        ///  The current instance of the <see cref="RepositoryHistoryUIService"/>.
        /// </summary>
        private IRepositoryHistoryUIService? RepositoryHistoryUIService => button._repositoryHistoryUIService;

        internal void FillDropDown()
        {
            // Do not rebuild while the dropdown is open — Clear() would close it.
            if (button._menu.IsOpen)
            {
                return;
            }

            if (RepositoryHistoryUIService is not null)
            {
                RepositoryHistorySnapshot snapshot = RepositoryHistoryUIService.LoadSnapshot();
                button.FillDropDown(snapshot);
                return;
            }

            IList<Repository> favourites = ThreadHelper.JoinableTaskFactory.Run(
                RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);
            IList<Repository> recent = ThreadHelper.JoinableTaskFactory.Run(
                RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
            button.FillDropDown(favourites, recent);
        }

        internal void RefreshContent()
        {
            Window? owner = TopLevel.GetTopLevel(button) as Window
                ?? button.GetLogicalAncestors().OfType<Window>().FirstOrDefault(window => window.IsVisible);
            if (owner is null)
            {
                // The component is unparented, no point doing anything.
                return;
            }

            // The source uses an open Form for measurement, not the item's current
            // ToolStrip parent. Overflow retains its logical owner even when its visual
            // parent is a PopupRoot or the closed overflow has no visual parent.
            if (Module is not IGitModule module)
            {
                return;
            }

            // A successful explicit refresh already crossed the source open-form boundary.
            // Its first overflow attachment must not repeat repository-history writes.
            button._contentOwner = owner;
            string path = module.WorkingDir;

            // It appears at times Module.WorkingDir path is an empty string,
            // this caused issues like https://github.com/gitextensions/gitextensions/issues/4874.
            if (string.IsNullOrWhiteSpace(path))
            {
                button.Content = _noWorkingFolderText.Text;
                return;
            }

            IList<Repository> recentRepositoryHistory = RepositoryHistoryUIService?.AddAsMostRecent(path)
                ?? ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));
            button.RefreshContent(path, recentRepositoryHistory);
        }

        internal void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
        {
            button._openRepositoryShortcutDisplay = hotkeys.GetShortcutDisplay(FormBrowse.Command.OpenRepo);
            button._closeRepositoryShortcutDisplay = hotkeys.GetShortcutDisplay(FormBrowse.Command.CloseRepository);
            button._openRepositoryGesture = KeysMapper.ToKeyGesture(
                hotkeys?.FirstOrDefault(command => command.CommandCode == (int)FormBrowse.Command.OpenRepo)?.KeyData);
            button._closeRepositoryGesture = KeysMapper.ToKeyGesture(
                hotkeys?.FirstOrDefault(command => command.CommandCode == (int)FormBrowse.Command.CloseRepository)?.KeyData);

            if (button._tsmiOpenLocalRepository is MenuItem open)
            {
                open.InputGesture = button._openRepositoryGesture;
                WinFormsToolStripMenuSizer.SetShortcutDisplayString(open, button._openRepositoryShortcutDisplay);
            }

            if (button._tsmiCloseRepo is MenuItem close)
            {
                close.InputGesture = button._closeRepositoryGesture;
                WinFormsToolStripMenuSizer.SetShortcutDisplayString(close, button._closeRepositoryShortcutDisplay);
            }

            if (button._menu.IsOpen)
            {
                button.ApplySourceShortcutText();
            }
        }
    }

    private readonly HashSet<MenuItem> _fixedItems = [];
    private readonly Dictionary<MenuItem, string> _repositoryItemTexts = [];
    private readonly MenuItem _filterHost;
    private readonly MenuFlyout _menu;
    private readonly TextBox _txtFilter = new NativeToolStripMenuTextBox();
    private readonly NativeToolStripDropDownLayout _dropDownLayout;

    private MenuItem? _tsmiCategorisedRepos;
    private MenuItem? _tsmiOpenLocalRepository;
    private MenuItem? _tsmiCloseRepo;
    private MenuItem? _tsmiRecentReposSettings;

    private Implementation? _implementation;
    private Window? _contentOwner;

    private bool _dropDownPreparedForTest;
    private KeyModifiers _repositoryActivationModifiers;
    private Action? _closeRepository;
    private Action? _configure;
    private Func<IGitUICommands>? _getUICommands;
    private IRepositoryHistoryUIService? _repositoryHistoryUIService;
    private Action<string>? _launchRepository;
    private Action? _openRepository;
    private Action<string>? _setWorkingDirectory;
    private KeyGesture? _closeRepositoryGesture;
    private KeyGesture? _openRepositoryGesture;
    private string? _closeRepositoryShortcutDisplay;
    private string? _openRepositoryShortcutDisplay;
    private string _closeRepositoryText = "Close (go to Dashboard)";
    private string _favouriteRepositoriesText = "&Favorite repositories";
    private string _openRepositoryText = "Open repository";

    public WorkingDirectoryToolStripSplitButton()
    {
        Name = nameof(WorkingDirectoryToolStripSplitButton);
        Content = "WorkingDir";
        Icon = Images.RepoOpen;

        // Avalonia separates horizontal alignment from the source MiddleLeft value.
        ImageAlign = HorizontalAlignment.Left;
        TextAlign = HorizontalAlignment.Left;
        TranslationCompat.SetConvertMnemonics(this, false);
        _menu = new NativeToolStripDropDownMenuFlyout(new WorkingDirectoryMenuInteractionHandler(
            _txtFilter, GetSourceItemText, ActivateRepositoryItem));
        Flyout = _menu;
        _menu.Placement = PlacementMode.BottomEdgeAlignedLeft;
        _menu.FlyoutPresenterClasses.Add("gitextensions-branch-menu");
        _menu.FlyoutPresenterClasses.Add("gitextensions-working-directory-menu");
        ToolTip.SetTip(this, _toolTip.Text);
        TranslationCompat.SetUseToolTipText(this, true);

        // A focusable MenuItem consumes the pointer focus intended for its TextBox header.
        _filterHost = new NativeToolStripDropDownMenuItem
        {
            UseSourceMnemonicRouting = true,
            Focusable = false,
            Header = _txtFilter,
            StaysOpenOnClick = true,
        };
        _filterHost.Classes.Add("gitextensions-working-directory-filter");
        _txtFilter.Classes.Add("gitextensions-working-directory-input");
        _txtFilter.Margin = new Thickness(1);
        _txtFilter.Width = 100;
        _dropDownLayout = new NativeToolStripDropDownLayout(this, _menu, _filterHost, _txtFilter);
        _menu.Items.Add(_filterHost);
        Click += (_, _) => OpenFlyout();
        _menu.Opening += Menu_Opening;
        _menu.Opened += (_, _) => ApplySourceShortcutText();
        _txtFilter.TextChanged += (_, _) => ApplyFilter();
        _txtFilter.KeyDown += TxtFilter_KeyDown;
        AddHandler(PointerReleasedEvent, MouseUpHandler, RoutingStrategies.Tunnel);
        _implementation = new Implementation(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // The source waits for an open graphics-owning form. First owner attachment supplies
        // that boundary; moving the same owned item into overflow must not refresh its text
        // or repository history merely because its visual parent changed.
        Window? owner = this.GetLogicalAncestors().OfType<Window>().FirstOrDefault();
        if (owner is not null && !ReferenceEquals(owner, _contentOwner))
        {
            _contentOwner = owner;
            RefreshContent();
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _contentOwner = null;
        base.OnDetachedFromLogicalTree(e);
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
        => _implementation?.RefreshContent();

    private void RefreshContent(string path, IList<Repository> recentRepositoryHistory)
    {
        List<RecentRepoInfo> pinnedRepos = [];
        using GitExtensions.Shims.WinForms.Font measurementFont = new(FontFamily.Name, (float)(FontSize * 72 / 96),
            (FontWeight >= Avalonia.Media.FontWeight.Bold ? GitExtensions.Shims.WinForms.FontStyle.Bold : GitExtensions.Shims.WinForms.FontStyle.Regular)
            | (FontStyle == Avalonia.Media.FontStyle.Italic ? GitExtensions.Shims.WinForms.FontStyle.Italic : GitExtensions.Shims.WinForms.FontStyle.Regular));
        RecentRepoSplitter splitter = new()
        {
            MeasureFont = measurementFont,
        };
        splitter.SplitRecentRepos(recentRepositoryHistory, pinnedRepos, pinnedRepos);
        RecentRepoInfo? repositoryInfo = pinnedRepos.Find(
            item => item.Repo.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

        Content = PathUtil.GetDisplayPath(repositoryInfo?.Caption ?? path);
        MinWidth = 0;
        if (AppSettings.RecentReposComboMinWidth > 0)
        {
            // The source deliberately uses GDI+ MeasureString here, not the
            // ToolStrip's padded TextRenderer preferred size or its image width.
            float captionWidth = (float)WinFormsGraphicsTextMeasurer.MeasureSize(this, Content as string ?? string.Empty).Width;
            captionWidth = captionWidth + 11 + 5;
            Width = Math.Max(AppSettings.RecentReposComboMinWidth, (int)captionWidth);
        }
        else
        {
            Width = double.NaN;
        }
    }

    public void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
        => _implementation?.RefreshShortcutKeys(hotkeys);

    private void FillDropDown()
        => _implementation?.FillDropDown();

    private void FillDropDown(RepositoryHistorySnapshot snapshot)
    {
        ResetDropDown();
        AddFavouriteRepositories(snapshot.Favourites);
        AddRecentRepositories(snapshot.Recent, snapshot.TopCount);
        AddFixedItems();
        ApplySourceDropDownWidth();
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
        ApplySourceDropDownWidth();
    }

    private void ResetDropDown()
    {
        _dropDownLayout.Clear();
        while (_menu.Items.Count > 1)
        {
            _menu.Items.RemoveAt(1);
        }

        _fixedItems.Clear();
        _repositoryItemTexts.Clear();
        _tsmiCategorisedRepos?.Items.Clear();
        _txtFilter.Text = string.Empty;
        _txtFilter.PlaceholderText = _repositorySearchPlaceholder.Text;
        _menu.Items.Add(new NativeToolStripDropDownSeparator());
    }

    private void AddFixedItems()
    {
        _menu.Items.Add(new NativeToolStripDropDownSeparator());
        _tsmiOpenLocalRepository ??= CreateFixedItem(
            _openRepositoryText,
            Images.RepoOpen,
            _openRepositoryGesture,
            _openRepositoryShortcutDisplay,
            () => _openRepository?.Invoke());
        _tsmiOpenLocalRepository.Header = _openRepositoryText;
        AddFixedItem(_tsmiOpenLocalRepository);
        _tsmiCloseRepo ??= CreateFixedItem(
            _closeRepositoryText,
            Images.DashboardFolderGit,
            _closeRepositoryGesture,
            _closeRepositoryShortcutDisplay,
            () => _closeRepository?.Invoke());
        _tsmiCloseRepo.Header = _closeRepositoryText;
        AddFixedItem(_tsmiCloseRepo);
        _menu.Items.Add(new NativeToolStripDropDownSeparator());
        _tsmiRecentReposSettings ??= CreateFixedItem(
            AvaloniaTranslationUtils.ToAvaloniaMnemonics(_configureWorkingDirMenu.Text),
            icon: null,
            gesture: null,
            shortcutDisplay: null,
            () => _configure?.Invoke());
        _tsmiRecentReposSettings.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_configureWorkingDirMenu.Text);
        AddFixedItem(_tsmiRecentReposSettings);
    }

    private void ApplySourceDropDownWidth()
        => _dropDownLayout.Apply(setFilterWidth: true);

    private void ApplySourceShortcutText()
        => _dropDownLayout.Apply(setFilterWidth: false);

    private void AddFavouriteRepositories(IReadOnlyList<RepositoryHistoryEntry> repositories)
    {
        if (repositories.Count == 0)
        {
            return;
        }

        MenuItem favourites = GetCategorisedRepositoriesItem();
        foreach (IGrouping<string?, RepositoryHistoryEntry> category in repositories
                     .GroupBy(item => item.Repository.Category)
                     .OrderBy(item => item.Key))
        {
            MenuItem categoryItem = new NativeToolStripDropDownMenuItem
            {
                UseSourceMnemonicRouting = true,
                Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(category.Key ?? string.Empty),
            };
            categoryItem.Classes.Add("gitextensions-working-directory-entry");
            int number = 0;
            foreach (RepositoryHistoryEntry repository in category)
            {
                categoryItem.Items.Add(CreateRepositoryItem(repository, ++number));
            }

            favourites.Items.Add(categoryItem);
        }

        _menu.Items.Add(favourites);
        _fixedItems.Add(favourites);
    }

    private void AddRecentRepositories(IReadOnlyList<RepositoryHistoryEntry> repositories, int topCount)
    {
        int number = 0;
        foreach (RepositoryHistoryEntry repository in repositories.Take(topCount))
        {
            _menu.Items.Add(CreateRepositoryItem(repository, ++number, repository.IsAnchored));
        }

        if (topCount > 0 && repositories.Count > topCount)
        {
            _menu.Items.Add(new NativeToolStripDropDownSeparator());
        }

        foreach (RepositoryHistoryEntry repository in repositories.Skip(topCount))
        {
            _menu.Items.Add(CreateRepositoryItem(repository, ++number, repository.IsAnchored));
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

        MenuItem favourites = GetCategorisedRepositoriesItem();
        foreach (IGrouping<string?, RecentRepoInfo> category in top
                     .Union(recent)
                     .GroupBy(item => item.Repo.Category)
                     .OrderBy(item => item.Key))
        {
            MenuItem categoryItem = new NativeToolStripDropDownMenuItem
            {
                UseSourceMnemonicRouting = true,
                Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(category.Key ?? string.Empty),
            };
            categoryItem.Classes.Add("gitextensions-working-directory-entry");
            int number = 0;
            foreach (RecentRepoInfo repository in category)
            {
                categoryItem.Items.Add(CreateRepositoryItem(repository, ++number));
            }

            favourites.Items.Add(categoryItem);
        }

        _menu.Items.Add(favourites);
        _fixedItems.Add(favourites);
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
            _menu.Items.Add(CreateRepositoryItem(repository, ++number, repository.Anchored));
        }

        if (pinned.Count > 0 && recent.Count > 0)
        {
            _menu.Items.Add(new NativeToolStripDropDownSeparator());
        }

        foreach (RecentRepoInfo repository in recent)
        {
            _menu.Items.Add(CreateRepositoryItem(repository, ++number, repository.Anchored));
        }
    }

    private MenuItem CreateRepositoryItem(RecentRepoInfo repository, int number, bool anchored = false)
    {
        string numberString = number switch
        {
            < 10 => $"&{number}",
            10 => "1&0",
            _ => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        string sourceText = $"{numberString}: {repository.Caption}";
        MenuItem item = new NativeToolStripDropDownMenuItem
        {
            UseSourceMnemonicRouting = true,
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(sourceText),
            Tag = repository,
            Icon = anchored ? CreateIcon(Images.Pin) : null,
        };
        _repositoryItemTexts.Add(item, sourceText);
        item.Classes.Add("gitextensions-working-directory-entry");
        item.Classes.Add("gitextensions-working-directory-repository");
        ToolTip.SetTip(item, repository.Repo.Path == repository.Caption ? null : repository.Repo.Path);
        item.Click += RepositoryItem_Click;
        return item;
    }

    private MenuItem CreateRepositoryItem(RepositoryHistoryEntry repository, int number, bool anchored = false)
    {
        string numberString = number switch
        {
            < 10 => $"&{number}",
            10 => "1&0",
            _ => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        string sourceText = $"{numberString}: {repository.Caption}";
        MenuItem item = new NativeToolStripDropDownMenuItem
        {
            UseSourceMnemonicRouting = true,
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(sourceText),
            Tag = repository,
            Icon = anchored ? CreateIcon(Images.Pin) : null,
        };
        _repositoryItemTexts.Add(item, sourceText);
        item.Classes.Add("gitextensions-working-directory-entry");
        item.Classes.Add("gitextensions-working-directory-repository");
        WinFormsToolStripMenuSizer.SetShortcutDisplayString(item, repository.BranchName);
        ToolTip.SetTip(
            item,
            repository.Repository.Path == repository.Caption ? null : repository.Repository.Path);
        item.Click += RepositoryItem_Click;
        return item;
    }

    private void RepositoryItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem item)
        {
            return;
        }

        KeyModifiers sourceControlModifier = KeysMapper.ToKeyGesture(
            GitExtensions.Shims.WinForms.Keys.Control | GitExtensions.Shims.WinForms.Keys.A)!.KeyModifiers;
        bool openInNewInstance = _repositoryActivationModifiers == sourceControlModifier;
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

    private void ActivateRepositoryItem(KeyModifiers modifiers, Action activate)
    {
        KeyModifiers previous = _repositoryActivationModifiers;
        _repositoryActivationModifiers = modifiers;
        try
        {
            activate();
        }
        finally
        {
            _repositoryActivationModifiers = previous;
        }
    }

    private string? GetSourceItemText(MenuItem item)
    {
        if (_repositoryItemTexts.TryGetValue(item, out string? sourceText))
        {
            return sourceText;
        }

        if (item.Header is not string text)
        {
            return null;
        }

        // Header's encoding is reversible; the owned presenter normalizes only its
        // painted Content, so fixed/category captions retain every native marker.
        System.Text.StringBuilder source = new(text.Length);
        for (int index = 0; index < text.Length; index++)
        {
            char character = text[index];
            if (character == '_')
            {
                if (index + 1 < text.Length && text[index + 1] == '_')
                {
                    source.Append('_');
                    index++;
                }
                else
                {
                    source.Append('&');
                }
            }
            else
            {
                source.Append(character);
                if (character == '&')
                {
                    source.Append('&');
                }
            }
        }

        return source.ToString();
    }

    private void OpenRepository(string path, bool openInNewInstance)
    {
        if (openInNewInstance)
        {
            _launchRepository?.Invoke(path);
            return;
        }

        if (_repositoryHistoryUIService is not null
            && !_repositoryHistoryUIService.CanOpenRepository(path))
        {
            return;
        }

        _setWorkingDirectory?.Invoke(path);
    }

    private MenuItem GetCategorisedRepositoriesItem()
    {
        _tsmiCategorisedRepos ??= new NativeToolStripDropDownMenuItem
        {
            UseSourceMnemonicRouting = true,
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_favouriteRepositoriesText),
            Icon = CreateIcon(Images.Star),
        };
        if (!_tsmiCategorisedRepos.Classes.Contains("gitextensions-working-directory-entry"))
        {
            _tsmiCategorisedRepos.Classes.Add("gitextensions-working-directory-entry");
        }

        _tsmiCategorisedRepos.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_favouriteRepositoriesText);
        _tsmiCategorisedRepos.Items.Clear();
        return _tsmiCategorisedRepos;
    }

    private static MenuItem CreateFixedItem(
        string header,
        Avalonia.Media.IImage? icon,
        KeyGesture? gesture,
        string? shortcutDisplay,
        Action? action)
    {
        MenuItem item = new NativeToolStripDropDownMenuItem
        {
            UseSourceMnemonicRouting = true,
            Header = header,
            Icon = CreateIcon(icon),
            InputGesture = gesture,
        };
        item.Classes.Add("gitextensions-working-directory-entry");
        WinFormsToolStripMenuSizer.SetShortcutDisplayString(item, shortcutDisplay);
        item.Click += (_, _) => action?.Invoke();
        return item;
    }

    private void AddFixedItem(MenuItem item)
    {
        _fixedItems.Add(item);
        _menu.Items.Add(item);
    }

    private void ApplyFilter()
    {
        string filter = _txtFilter.Text ?? string.Empty;
        foreach (object? entry in _menu.Items)
        {
            if (entry is MenuItem item && !_fixedItems.Contains(item) && item.Header != _txtFilter)
            {
                // WinForms filters only the displayed recent-repository captions. Favourite
                // categories and fixed actions are explicitly excluded, including their children.
                // Its ToolStripItem.Text retains the source '&' markers and literal '_';
                // Avalonia's escaped AccessText Header must not become the search model.
                string? sourceText = _repositoryItemTexts.TryGetValue(item, out string? repositoryText)
                    ? repositoryText : item.Header as string;
                item.IsVisible = string.IsNullOrWhiteSpace(filter)
                    || sourceText?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) is true;
            }
        }
    }

    private void TxtFilter_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            // The native hosted input preprocesses Escape by closing its dropdown;
            // FillDropDown clears the retained text only on the next actual opening.
            _menu.Hide();
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
                () => "&Close (go to Dashboard)") ?? "&Close (go to Dashboard)");
        ToolTip.SetTip(this, _toolTip.Text);
    }

    void ITranslate.AddTranslationItems(ITranslation translation)
        => AddControlTranslationItems(translation);

    void ITranslate.TranslateItems(ITranslation translation)
        => TranslateControlItems(translation);

    void IDisposable.Dispose()
    {
        _menu.Hide();
        _dropDownLayout.Dispose();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(WorkingDirectoryToolStripSplitButton control)
    {
        public MenuFlyout Menu => control._menu;

        public TextBox Filter => control._txtFilter;

        public MenuItem FilterHost => control._filterHost;

        // parity-scaffolding: expose the consumed layout model, not native-looking capture projections.
        public NativeToolStripDropDownLayout.Group Layout => control._dropDownLayout.Root;

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

        public void PrepareDropDown(RepositoryHistorySnapshot snapshot)
        {
            control.FillDropDown(snapshot);
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
