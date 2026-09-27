using Avalonia.Controls;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using ResourceManager;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs.Menus;

internal partial class StartToolStripMenuItem : ToolStripMenuItemEx
{
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

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _repositoryHistoryUIService = UICommands.GetRequiredService<IRepositoryHistoryUIService>();
        _repositoryHistoryUIService.GitModuleChanged += (_, e) => GitModuleChanged?.Invoke(OwnerForm, e);
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
        GetRepositoryHistoryUIService().PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories);
    }

    private void tsmiRecentRepositories_DropDownOpening(object? sender, EventArgs e)
    {
        // Note: repo-branch name cache is shared with the dashboard, no update needed
        GetRepositoryHistoryUIService().PopulateRecentRepositoriesMenu(tsmiRecentRepositories);

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
