using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.Properties;
using ResourceManager;
using ResourceManager.Hotkey;
using ToolStripMenuItem = GitUI.Compat.WinFormsControls.ToolStripMenuItem;
using ToolStripTextBox = Avalonia.Controls.TextBox;

namespace GitUI.CommandsDialogs.Menus;

/// <summary>
///  Represents a split button that contains the recent repositories.
/// </summary>
internal sealed class WorkingDirectoryToolStripSplitButton : IconSplitButton, ITranslate
{
    private static readonly TranslationString _noWorkingFolderText = new("No working directory");

    private static readonly TranslationString _configureWorkingDirMenu = new("Co&nfigure this menu...");
    private static readonly TranslationString _repositorySearchPlaceholder = new("Search repositories...");
    private static readonly TranslationString _toolTip = new("""
        Change working directory
        Left click opens the drop-down menu.
        Then hold Ctrl in order to open the selected repository in a new instance.
        Right click starts the "Open repository" dialog.
        """);

    private sealed class Implementation
    {
        // This is used as Tag in order to mark controls which are to be excluded from the filtering considerations.
        private static readonly object _excludeFromFilterMarker = new();

        /// <summary>
        ///  Gets the current instance of the git module.
        /// </summary>
        private IGitModule Module => UICommands.Module;

        /// <summary>
        ///  Gets the active or the first open form.
        /// </summary>
        private static Window? ActiveOrOpenForm
            => Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.Windows.FirstOrDefault(window => window.IsActive) ?? desktop.Windows.FirstOrDefault()
                : null;

        /// <summary>
        ///  Gets the current instance of the UI commands.
        /// </summary>
        private IGitUICommands UICommands => _getUICommands();

        private readonly Func<IGitUICommands> _getUICommands;

        /// <summary>
        ///  The current instance of the <see cref="RepositoryHistoryUIService"/>.
        /// </summary>
        private readonly IRepositoryHistoryUIService _repositoryHistoryUIService;

        private readonly ToolStripMenuItem _tsmiCategorisedRepos;
        private readonly ToolStripMenuItem _tsmiOpenLocalRepository;
        private readonly ToolStripMenuItem _tsmiCloseRepo;
        private readonly ToolStripMenuItem _tsmiRecentReposSettings;
        private readonly ToolStripTextBox _txtFilter;

        private readonly StartToolStripMenuItem _startToolStripMenuItem;
        private readonly ToolStripMenuItem _closeToolStripMenuItem;

        internal Implementation(
            WorkingDirectoryToolStripSplitButton button,
            Func<IGitUICommands> getUICommands,
            IRepositoryHistoryUIService repositoryHistoryUIService,
            StartToolStripMenuItem startToolStripMenuItem,
            ToolStripMenuItem closeToolStripMenuItem)
        {
            button._menu.Opening += (s, e) => FillDropDown(button);
            button.AddHandler(PointerReleasedEvent, MouseUpHandler, RoutingStrategies.Tunnel);
            ToolTip.SetTip(button, _toolTip.Text);

            _getUICommands = getUICommands;
            _repositoryHistoryUIService = repositoryHistoryUIService;
            _startToolStripMenuItem = startToolStripMenuItem;
            _closeToolStripMenuItem = closeToolStripMenuItem;

            _txtFilter = button._txtFilter;
            _txtFilter.Tag = _excludeFromFilterMarker;
            button._filterHost.Tag = _excludeFromFilterMarker;

            TextBox filterTextbox = _txtFilter;
            filterTextbox.PlaceholderText = _repositorySearchPlaceholder.Text;
            filterTextbox.TextChanged += (s, e) =>
            {
                if (button._rebuilding)
                {
                    // We are clearing the textbox while opening the dropdown
                    return;
                }

                // Default items include:
                //  1. filter
                //  2. separator
                //  3. favourite items
                //      ... recent items
                //  4. "Open repo..."
                //  5. "Close repo..."
                //  6. separator
                //  7. "Configure menu"
                const int defaultItemCount = 7;
                if (button._menu.Items.Count <= defaultItemCount)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(filterTextbox.Text))
                {
                    foreach (Control item in button._menu.Items.OfType<Control>())
                    {
                        item.IsVisible = true;
                    }

                    return;
                }

                foreach (Control item in button._menu.Items.OfType<Control>())
                {
                    if (item is Separator || item.Tag == _excludeFromFilterMarker)
                    {
                        continue;
                    }

                    item.IsVisible = item is MenuItem menuItem
                        && NativeRepositoryMenu.GetSourceItemText(menuItem)?.Contains(filterTextbox.Text, StringComparison.CurrentCultureIgnoreCase) is true;
                }
            };

            // Initialize toolstip menu items
            // ----------------------------------------
            _tsmiCategorisedRepos = NativeRepositoryMenu.Copy(_startToolStripMenuItem.FavouriteRepositoriesMenuItem);
            _tsmiCategorisedRepos.Tag = _excludeFromFilterMarker;

            _tsmiOpenLocalRepository = NativeRepositoryMenu.Copy(_startToolStripMenuItem.OpenRepositoryMenuItem);
            _tsmiOpenLocalRepository.Tag = _excludeFromFilterMarker;
            _tsmiOpenLocalRepository.Click += (s, e) => _startToolStripMenuItem.OpenRepositoryMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            _tsmiCloseRepo = NativeRepositoryMenu.Copy(_closeToolStripMenuItem);
            _tsmiCloseRepo.Tag = _excludeFromFilterMarker;
            _tsmiCloseRepo.Click += (hs, he) => _closeToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            _tsmiRecentReposSettings = new NativeToolStripDropDownMenuItem
            {
                UseSourceMnemonicRouting = true,
                Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_configureWorkingDirMenu.Text),
                Tag = _excludeFromFilterMarker,
                Classes = { "gitextensions-working-directory-entry" },
            };
            _tsmiRecentReposSettings.Click += (hs, he) =>
            {
                using (FormRecentReposSettings frm = new())
                {
                    frm.ShowDialog((button._contentOwner ?? ActiveOrOpenForm) as GitExtensions.Shims.WinForms.IWin32Window);
                }

                _repositoryHistoryUIService.Invalidate();
                RefreshContent(button);
            };
        }

        private void FillDropDown(WorkingDirectoryToolStripSplitButton button)
        {
            // Do not rebuild while the dropdown is open — Clear() would close it.
            if (button._menu.IsOpen)
            {
                return;
            }

            button._rebuilding = true;
            button._dropDownLayout.Clear();
            try
            {
                // Avalonia must retain the hosted editor while the popup tears down.
                while (button._menu.Items.Count > 1)
                {
                    button._menu.Items.RemoveAt(1);
                }

                _txtFilter.Text = string.Empty;

                button._menu.Items.Add(new NativeToolStripDropDownSeparator());

                _repositoryHistoryUIService.PopulateFavouriteRepositoriesMenu(_tsmiCategorisedRepos);
                if (_tsmiCategorisedRepos.Items.Count > 0)
                {
                    button._menu.Items.Add(_tsmiCategorisedRepos);
                }

                // Avalonia's split button owns a flyout rather than a MenuItem collection.
                NativeToolStripDropDownMenuItem container = new();
                _repositoryHistoryUIService.PopulateRecentRepositoriesMenu(container);
                while (container.Items.Count > 0)
                {
                    object? item = container.Items[0];
                    container.Items.RemoveAt(0);
                    button._menu.Items.Add(item);
                }

                button._menu.Items.Add(new NativeToolStripDropDownSeparator());
                button._menu.Items.Add(_tsmiOpenLocalRepository);
                button._menu.Items.Add(_tsmiCloseRepo);
                button._menu.Items.Add(new NativeToolStripDropDownSeparator());
                button._menu.Items.Add(_tsmiRecentReposSettings);
            }
            finally
            {
                button._rebuilding = false;
                button._dropDownLayout.Apply(setFilterWidth: true);
            }
        }

        private void MouseUpHandler(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
            {
                _startToolStripMenuItem.OpenRepositoryMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                e.Handled = true;
            }
        }

        internal void RefreshContent(WorkingDirectoryToolStripSplitButton button)
        {
            Window? graphicsForm = button.GetLogicalAncestors().OfType<Window>().FirstOrDefault(window => window.IsVisible)
                ?? TopLevel.GetTopLevel(button) as Window ?? ActiveOrOpenForm;
            if (graphicsForm is null)
            {
                // The component is unparented, no point doing anything.
                return;
            }

            button._contentOwner = graphicsForm;
            string path = Module.WorkingDir;

            // It appears at times Module.WorkingDir path is an empty string,
            // this caused issues like https://github.com/gitextensions/gitextensions/issues/4874.
            if (string.IsNullOrWhiteSpace(path))
            {
                button.Content = _noWorkingFolderText.Text;
                return;
            }

            IList<Repository> recentRepositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
                () => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));

            List<RecentRepoInfo> pinnedRepos = [];
            using GitExtensions.Shims.WinForms.Font measurementFont = new(button.FontFamily.Name, (float)(button.FontSize * 72 / 96),
                (button.FontWeight >= Avalonia.Media.FontWeight.Bold ? GitExtensions.Shims.WinForms.FontStyle.Bold : GitExtensions.Shims.WinForms.FontStyle.Regular)
                | (button.FontStyle == Avalonia.Media.FontStyle.Italic ? GitExtensions.Shims.WinForms.FontStyle.Italic : GitExtensions.Shims.WinForms.FontStyle.Regular));
            RecentRepoSplitter splitter = new()
            {
                MeasureFont = measurementFont,
            };

            splitter.SplitRecentRepos(recentRepositoryHistory, pinnedRepos, pinnedRepos);

            RecentRepoInfo? ri = pinnedRepos.Find(e => e.Repo.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

            button.Content = PathUtil.GetDisplayPath(ri?.Caption ?? path);

            if (AppSettings.RecentReposComboMinWidth > 0)
            {
                button.MinWidth = 0;
                float captionWidth = (float)WinFormsGraphicsTextMeasurer.MeasureSize(button, button.Content as string ?? string.Empty).Width;
                captionWidth = captionWidth + 11 + 5;
                button.Width = Math.Max(AppSettings.RecentReposComboMinWidth, (int)captionWidth);
            }
            else
            {
                button.MinWidth = 0;
                button.Width = double.NaN;
            }
        }

        internal void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
        {
            WinFormsToolStripMenuSizer.SetShortcutDisplayString(_tsmiOpenLocalRepository, hotkeys.GetShortcutDisplay(FormBrowse.Command.OpenRepo));
            WinFormsToolStripMenuSizer.SetShortcutDisplayString(_tsmiCloseRepo, hotkeys.GetShortcutDisplay(FormBrowse.Command.CloseRepository));
            _tsmiOpenLocalRepository.InputGesture = KeysMapper.ToKeyGesture(
                hotkeys?.FirstOrDefault(command => command.CommandCode == (int)FormBrowse.Command.OpenRepo)?.KeyData);
            _tsmiCloseRepo.InputGesture = KeysMapper.ToKeyGesture(
                hotkeys?.FirstOrDefault(command => command.CommandCode == (int)FormBrowse.Command.CloseRepository)?.KeyData);
        }
    }

    private Implementation? _implementation;

    // Avalonia retains the editor and popup layout independently of its repository items.
    private readonly MenuItem _filterHost;
    private readonly MenuFlyout _menu;
    private readonly TextBox _txtFilter = new NativeToolStripMenuTextBox();
    private readonly NativeToolStripDropDownLayout _dropDownLayout;
    private Window? _contentOwner;
    private bool _rebuilding;

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
            _txtFilter, NativeRepositoryMenu.GetSourceItemText, NativeRepositoryMenu.Activate));
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
        _menu.Opened += (_, _) => ApplySourceShortcutText();
        _txtFilter.KeyDown += TxtFilter_KeyDown;
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
    /// <param name="getUICommands">The method that returns the current instance of UI commands.</param>
    public void Initialize(Func<IGitUICommands> getUICommands, IRepositoryHistoryUIService repositoryHistoryUIService,
                           StartToolStripMenuItem startToolStripMenuItem, ToolStripMenuItem closeToolStripMenuItem)
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);

        _implementation = new Implementation(this, getUICommands, repositoryHistoryUIService, startToolStripMenuItem, closeToolStripMenuItem);
    }

    /// <summary>Updates the text shown on the combo button itself.</summary>
    public void RefreshContent()
    {
        // If the component is not initialized, no point doing anything.
        _implementation?.RefreshContent(this);
    }

    public void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
    {
        _implementation?.RefreshShortcutKeys(hotkeys);
        if (_menu.IsOpen)
        {
            ApplySourceShortcutText();
        }
    }

    private void ApplySourceShortcutText()
        => _dropDownLayout.Apply(setFilterWidth: false);

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

    internal void AddControlTranslationItems(ITranslation translation)
        => TranslationUtils.AddTranslationItemsFromFields("FormBrowse", this, translation);

    internal void TranslateControlItems(ITranslation translation)
    {
        TranslationUtils.TranslateItemsFromFields("FormBrowse", this, translation);
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
}
