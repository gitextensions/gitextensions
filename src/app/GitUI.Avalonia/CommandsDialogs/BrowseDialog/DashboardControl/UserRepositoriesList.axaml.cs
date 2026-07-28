using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

public partial class UserRepositoriesList : TranslatedControl
{
    private readonly TranslationString _groupRecentRepositories = new("Recent repositories");
    private readonly TranslationString _repositorySearchPlaceholder = new("Search repositories...");
    private Func<IGitUICommands>? _getUICommands;
    private bool _isSubscribed;
    private IRepositoryHistoryUIService? _repositoryHistoryUIService;

    public UserRepositoriesList()
    {
        InitializeComponent();
        listView1.ItemTemplate = new FuncDataTemplate<RepositoryListItem>(
            (item, _) => CreateRow(item),
            supportsRecycling: false);
        listView1.ContainerPrepared += ListView1_ContainerPrepared;
        listView1.AddHandler(
            PointerReleasedEvent,
            ListView1_PointerReleased,
            RoutingStrategies.Tunnel);
        listView1.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                OpenSelected();
                e.Handled = true;
            }
        };
        textBoxSearch.TextChanged += (_, _) => ShowRecentRepositories(reloadData: false);
        mnuConfigure.Click += (_, _) => ConfigureRequested?.Invoke(this, EventArgs.Empty);
        InitializeComplete();
    }

    public event EventHandler? ConfigureRequested;
    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;

    public void Initialize(IRepositoryHistoryUIService repositoryHistoryUIService, Func<IGitUICommands> getUICommands)
    {
        if (_repositoryHistoryUIService is not null && _isSubscribed)
        {
            _repositoryHistoryUIService.HistoryChanged -= RepositoryHistoryUIService_HistoryChanged;
        }

        _repositoryHistoryUIService = repositoryHistoryUIService;
        _getUICommands = getUICommands;
        _repositoryHistoryUIService.HistoryChanged += RepositoryHistoryUIService_HistoryChanged;
        _isSubscribed = true;
        _repositoryHistoryUIService.TriggerBranchNameCacheUpdate(onlyIfEmpty: true);
    }

    public void ShowRecentRepositories(bool reloadData = true)
    {
        if (_repositoryHistoryUIService is null)
        {
            return;
        }

        if (reloadData)
        {
            _repositoryHistoryUIService.Invalidate();
        }

        string filter = textBoxSearch.Text?.Trim() ?? string.Empty;
        RepositoryHistorySnapshot snapshot = _repositoryHistoryUIService.LoadSnapshot();
        List<RepositoryListItem> rows = [];
        AddGroup(rows, _groupRecentRepositories.Text, snapshot.Recent, filter);
        foreach (IGrouping<string?, RepositoryHistoryEntry> category in snapshot.Favourites
                     .GroupBy(entry => entry.Repository.Category)
                     .OrderBy(group => group.Key))
        {
            AddGroup(rows, category.Key ?? string.Empty, category, filter);
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

    private static void AddGroup(
        ICollection<RepositoryListItem> rows,
        string header,
        IEnumerable<RepositoryHistoryEntry> entries,
        string filter)
    {
        RepositoryHistoryEntry[] matches =
        [
            .. entries.Where(entry => string.IsNullOrWhiteSpace(filter)
                                      || entry.Caption.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                                      || entry.Repository.Path.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                                      || (entry.BranchName?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ?? false)),
        ];
        if (matches.Length == 0)
        {
            return;
        }

        rows.Add(new RepositoryListItem(header, Repository: null, IsValid: true));
        foreach (RepositoryHistoryEntry entry in matches)
        {
            rows.Add(new RepositoryListItem(
                entry.Caption,
                entry,
                GitModule.IsValidGitWorkingDir(entry.Repository.Path)));
        }
    }

    private static Control CreateRow(RepositoryListItem? item)
    {
        // Avalonia clears a recycled ContentPresenter by invoking the typed template with
        // null before assigning the replacement item.
        if (item is null)
        {
            return new Border();
        }

        if (item.Repository is null)
        {
            return new TextBlock
            {
                Text = item.Text,
                FontSize = 16,
                FontWeight = FontWeight.SemiBold,
                Margin = new Avalonia.Thickness(2, 10, 2, 4),
                IsHitTestVisible = false,
            };
        }

        Image image = new()
        {
            Width = 20,
            Height = 20,
            Source = item.IsValid ? Images.DashboardFolderGit : Images.DashboardFolderError,
            Margin = new Avalonia.Thickness(0, 1, 8, 0),
        };
        StackPanel text = new()
        {
            Spacing = 1,
            Children =
            {
                new TextBlock
                {
                    Text = item.Text,
                    FontWeight = item.Repository.IsFavourite ? FontWeight.SemiBold : FontWeight.Normal,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
                new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(item.Repository.BranchName)
                        ? item.Repository.Repository.Path
                        : $"{item.Repository.BranchName}  —  {item.Repository.Repository.Path}",
                    Opacity = 0.7,
                    FontSize = 11,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
            },
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Avalonia.Thickness(4, 3),
            Children = { image, text },
        };
    }

    private void ListView1_ContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        bool isHeader = e.Index >= 0
            && e.Index < listView1.ItemCount
            && listView1.Items[e.Index] is RepositoryListItem { Repository: null };
        e.Container.IsEnabled = !isHeader;
        e.Container.Focusable = !isHeader;
        e.Container.IsHitTestVisible = !isHeader;
    }

    private void ListView1_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left
            || e.Source is not Control source
            || (source as ListBoxItem ?? source.FindAncestorOfType<ListBoxItem>()) is null)
        {
            return;
        }

        OpenSelected();
        e.Handled = true;
    }

    private void OpenSelected()
    {
        if (listView1.SelectedItem is not RepositoryListItem { Repository: { } entry }
            || _repositoryHistoryUIService is null
            || _getUICommands is null
            || !_repositoryHistoryUIService.CanOpenRepository(entry.Repository.Path))
        {
            return;
        }

        IGitExecutorProvider executorProvider = _getUICommands().GetRequiredService<IGitExecutorProvider>();
        GitModuleChanged?.Invoke(
            this,
            new GitModuleEventArgs(new GitModule(executorProvider, entry.Repository.Path)));
    }

    private void RepositoryHistoryUIService_HistoryChanged(object? sender, EventArgs e)
        => this.InvokeAndForget(() =>
        {
            ShowRecentRepositories(reloadData: false);
            return Task.CompletedTask;
        });

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(UserRepositoriesList control)
    {
        internal TextBox Search => control.textBoxSearch;
        internal ListBox List => control.listView1;
        internal Button Configure => control.mnuConfigure;
        internal void OpenSelected() => control.OpenSelected();
    }

    internal sealed record RepositoryListItem(
        string Text,
        RepositoryHistoryEntry? Repository,
        bool IsValid);
}
