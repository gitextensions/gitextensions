using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.Properties;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs.Menus;

internal partial class StartToolStripMenuItem : ToolStripMenuItemEx
{
    private static readonly AttachedProperty<bool> OpenInNewInstanceProperty =
        AvaloniaProperty.RegisterAttached<StartToolStripMenuItem, MenuItem, bool>("OpenInNewInstance");

    private IRepositoryHistoryUIService? _repositoryHistoryUIService;

    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;
    public event EventHandler? RecentRepositoriesCleared;

    public StartToolStripMenuItem()
    {
        InitializeComponent();

        initNewRepositoryToolStripMenuItem.Click += InitNewRepositoryToolStripMenuItemClick;
        openToolStripMenuItem.Click += OpenToolStripMenuItemClick;
        tsmiFavouriteRepositories.SubmenuOpened += tsmiFavouriteRepositories_DropDownOpening;
        tsmiRecentRepositories.SubmenuOpened += tsmiRecentRepositories_DropDownOpening;
        tsmiRecentRepositoriesClear.Click += tsmiRecentRepositoriesClear_Click;
        cloneToolStripMenuItem.Click += CloneToolStripMenuItemClick;
        exitToolStripMenuItem.Click += ExitToolStripMenuItemClick;
        InputAccessibility.Apply(this);
    }

    internal MenuItem OpenRepositoryMenuItem => openToolStripMenuItem;
    internal MenuItem FavouriteRepositoriesMenuItem => tsmiFavouriteRepositories;

    public override void OnInitialized()
    {
        base.OnInitialized();

        _repositoryHistoryUIService = UICommands.GetRequiredService<IRepositoryHistoryUIService>();
    }

    public override void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
    {
        openToolStripMenuItem.InputGesture = KeysMapper.ToKeyGesture(
            hotkeys?.FirstOrDefault(command => command.CommandCode == (int)FormBrowse.Command.OpenRepo)?.KeyData);

        base.RefreshShortcutKeys(hotkeys);
    }

    private void CloneToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCloneDialog(OwnerForm, string.Empty, false, GitModuleChanged);
    }

    private void ExitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        OwnerWindow?.Close();
    }

    private void InitNewRepositoryToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartInitializeDialog(OwnerForm, gitModuleChanged: GitModuleChanged);
    }

    private void OpenToolStripMenuItemClick(object? sender, EventArgs e)
    {
        IGitModule? module = FormOpenDirectory.OpenModule(OwnerForm!, UICommands.GetRequiredService<IGitExecutorProvider>(), UICommands.Module);
        if (module is not null)
        {
            GitModuleChanged?.Invoke(OwnerForm, new GitModuleEventArgs(module));
        }
    }

    private void tsmiFavouriteRepositories_DropDownOpening(object? sender, EventArgs e)
    {
        tsmiFavouriteRepositories.Items.Clear();
        RepositoryHistorySnapshot snapshot = GetRepositoryHistoryUIService().LoadSnapshot();
        foreach (IGrouping<string?, RepositoryHistoryEntry> category in snapshot.Favourites
                     .GroupBy(item => item.Repository.Category)
                     .OrderBy(item => item.Key))
        {
            MenuItem categoryItem = new() { Header = category.Key ?? string.Empty };
            int number = 0;
            foreach (RepositoryHistoryEntry repository in category)
            {
                categoryItem.Items.Add(CreateRepositoryItem(repository, ++number));
            }

            tsmiFavouriteRepositories.Items.Add(categoryItem);
        }
    }

    private void tsmiRecentRepositories_DropDownOpening(object? sender, EventArgs e)
    {
        // Note: repo-branch name cache is shared with the dashboard, no update needed
        tsmiRecentRepositories.Items.Clear();
        RepositoryHistorySnapshot snapshot = GetRepositoryHistoryUIService().LoadSnapshot();
        int number = 0;
        bool hasAnchored = false;
        foreach (RepositoryHistoryEntry repository in snapshot.Recent)
        {
            if (!repository.IsAnchored && hasAnchored)
            {
                tsmiRecentRepositories.Items.Add(new Separator());
                hasAnchored = false;
            }

            tsmiRecentRepositories.Items.Add(CreateRepositoryItem(repository, ++number));
            hasAnchored |= repository.IsAnchored;
        }

        if (tsmiRecentRepositories.Items.Count < 1)
        {
            return;
        }

        tsmiRecentRepositories.Items.Add(clearRecentRepositoriesListToolStripMenuItem);
        tsmiRecentRepositories.Items.Add(tsmiRecentRepositoriesClear);
    }

    private void tsmiRecentRepositoriesClear_Click(object? sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.SaveRecentHistoryAsync([]));
        GetRepositoryHistoryUIService().Invalidate();
        RecentRepositoriesCleared?.Invoke(sender, e);
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
            Header = CreateRepositoryHeader($"{numberString}: {repository.Caption}", repository.BranchName),
            Tag = repository,
            Icon = repository.IsAnchored
                ? new Image { Classes = { "gitextensions-icon-16" }, Source = Images.Pin }
                : null,
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
        if (sender is not MenuItem { Tag: RepositoryHistoryEntry repository } item)
        {
            return;
        }

        bool openInNewInstance = item.GetValue(OpenInNewInstanceProperty);
        item.ClearValue(OpenInNewInstanceProperty);
        OpenRepository(repository.Repository.Path, openInNewInstance);
    }

    private void OpenRepository(string path, bool openInNewInstance)
    {
        if (!GetRepositoryHistoryUIService().CanOpenRepository(path))
        {
            return;
        }

        if (openInNewInstance)
        {
            GitUICommands.LaunchBrowse(path);
        }
        else
        {
            SetGitModule(path);
        }
    }

    private void SetGitModule(string path)
    {
        IGitExecutorProvider executorProvider = UICommands.GetRequiredService<IGitExecutorProvider>();
        GitModuleChanged?.Invoke(OwnerForm, new GitModuleEventArgs(new GitModule(executorProvider, path)));
    }

    private IRepositoryHistoryUIService GetRepositoryHistoryUIService()
        => _repositoryHistoryUIService
            ?? throw new InvalidOperationException("The menu is not initialized.");

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(StartToolStripMenuItem menu)
    {
        public MenuItem RecentRepositoriesMenuItem => menu.tsmiRecentRepositories;
        public MenuItem FavouriteRepositoriesMenuItem => menu.tsmiFavouriteRepositories;
        public MenuItem InitNewRepositoryMenuItem => menu.initNewRepositoryToolStripMenuItem;
        public MenuItem CloneMenuItem => menu.cloneToolStripMenuItem;
        public MenuItem ExitMenuItem => menu.exitToolStripMenuItem;
    }
}
