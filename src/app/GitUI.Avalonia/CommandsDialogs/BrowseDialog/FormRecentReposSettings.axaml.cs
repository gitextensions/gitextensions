using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using Microsoft;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog;

public partial class FormRecentReposSettings : GitExtensionsForm
{
    private const int MinComboWidthAllowed = 30;

#pragma warning disable SX1309 // Preserve the original designer field names for port parity.
    private readonly MenuItem anchorToRecentReposToolStripMenuItem = new() { Header = "Anchor to recent repositories" };
    private readonly MenuItem anchorToTopReposToolStripMenuItem = new() { Header = "Anchor to top repositories" };
    private readonly ContextMenu contextMenuStrip1 = new();
    private readonly MenuItem removeAnchorToolStripMenuItem = new() { Header = "Remove anchor" };
    private readonly MenuItem removeRecentToolStripMenuItem = new() { Header = "Remove from recent repositories" };
#pragma warning restore SX1309
    private readonly Func<string, Task<IList<Repository>>> _removeRecentAsync;
    private readonly Func<IEnumerable<Repository>, Task> _saveRecentAsync;
    private IList<Repository>? _repositoryHistory;
    private ListBox? _contextList;
    private decimal _previousValue;
    private bool _updating;

    // Avalonia's designer must not read or mutate repository history.
    public FormRecentReposSettings()
        : this(
            Design.IsDesignMode
                ? []
                : ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync),
            Design.IsDesignMode ? null : RepositoryHistoryManager.Locals.SaveRecentHistoryAsync,
            Design.IsDesignMode ? null : RepositoryHistoryManager.Locals.RemoveRecentAsync)
    {
    }

    internal FormRecentReposSettings(
        IList<Repository> repositoryHistory,
        Func<IEnumerable<Repository>, Task>? saveRecentAsync = null,
        Func<string, Task<IList<Repository>>>? removeRecentAsync = null)
    {
        _repositoryHistory = repositoryHistory;
        _saveRecentAsync = saveRecentAsync ?? (_ => Task.CompletedTask);
        _removeRecentAsync = removeRecentAsync ?? RemoveRecentFromMemoryAsync;

        InitializeComponent();
        ConfigureControls();
        InitializeComplete();
        LoadSettings();
        RefreshRepos();
    }

    private void ConfigureControls()
    {
        contextMenuStrip1.Items.Add(anchorToTopReposToolStripMenuItem);
        contextMenuStrip1.Items.Add(anchorToRecentReposToolStripMenuItem);
        contextMenuStrip1.Items.Add(removeAnchorToolStripMenuItem);
        contextMenuStrip1.Items.Add(removeRecentToolStripMenuItem);
        contextMenuStrip1.Opening += contextMenuStrip1_Opening;
        TopLB.ContextMenu = contextMenuStrip1;
        RecentLB.ContextMenu = contextMenuStrip1;
        TopLB.ItemTemplate = CreateRepositoryTemplate();
        RecentLB.ItemTemplate = CreateRepositoryTemplate();
        TopLB.AddHandler(PointerPressedEvent, ListBox_PointerPressed, RoutingStrategies.Tunnel);
        RecentLB.AddHandler(PointerPressedEvent, ListBox_PointerPressed, RoutingStrategies.Tunnel);
        TopLB.DoubleTapped += TopLB_DoubleClick;
        RecentLB.DoubleTapped += AllRecentLB_DoubleClick;
        Ok.Click += Ok_Click;
        Abort.Click += Abort_Click;
        anchorToTopReposToolStripMenuItem.Click += anchorToMostToolStripMenuItem_Click;
        anchorToRecentReposToolStripMenuItem.Click += anchorToLessToolStripMenuItem_Click;
        removeAnchorToolStripMenuItem.Click += removeAnchorToolStripMenuItem_Click;
        removeRecentToolStripMenuItem.Click += removeRecentToolStripMenuItem_Click;
        _NO_TRANSLATE_maxRecentRepositories.ValueChanged += sortTopRepos_CheckedChanged;
        hideTopRepositoriesFromRecentList.IsCheckedChanged += sortTopRepos_CheckedChanged;
        sortTopRepos.IsCheckedChanged += sortTopRepos_CheckedChanged;
        sortRecentRepos.IsCheckedChanged += sortTopRepos_CheckedChanged;
        dontShortenRB.IsCheckedChanged += sortTopRepos_CheckedChanged;
        middleDotRB.IsCheckedChanged += sortTopRepos_CheckedChanged;
        mostSigDirRB.IsCheckedChanged += sortTopRepos_CheckedChanged;
        comboMinWidthEdit.ValueChanged += comboMinWidthEdit_ValueChanged;
    }

    private static FuncDataTemplate<RecentRepoInfo> CreateRepositoryTemplate()
        => new(
            (repo, _) =>
            {
                if (repo is null)
                {
                    return new Border();
                }

                TextBlock text = new()
                {
                    Margin = new Thickness(4, 2),
                    Text = repo.Caption,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    FontWeight = repo.Anchored ? FontWeight.Bold : FontWeight.Normal,
                };
                if (!Directory.Exists(repo.Repo.Path))
                {
                    text.Foreground = Brushes.Red;
                }

                ToolTip.SetTip(text, repo.Repo.Path);
                return text;
            },
            supportsRecycling: true);

    private void LoadSettings()
    {
        _updating = true;
        try
        {
            SetShorteningStrategy(AppSettings.ShorteningRecentRepoPathStrategy);
            hideTopRepositoriesFromRecentList.IsChecked = AppSettings.HideTopRepositoriesFromRecentList.Value;
            sortTopRepos.IsChecked = AppSettings.SortTopRepos;
            sortRecentRepos.IsChecked = AppSettings.SortRecentRepos;
            comboMinWidthEdit.Value = AppSettings.RecentReposComboMinWidth;
            SetNumericUpDownValue(_NO_TRANSLATE_maxRecentRepositories, AppSettings.MaxTopRepositories);
            SetNumericUpDownValue(_NO_TRANSLATE_RecentRepositoriesHistorySize, AppSettings.RecentRepositoriesHistorySize);
            _previousValue = comboMinWidthEdit.Value ?? 0;
        }
        finally
        {
            _updating = false;
        }

        return;

        static void SetNumericUpDownValue(NumericUpDown control, int value)
            => control.Value = Math.Min(Math.Max(control.Minimum, value), control.Maximum);
    }

    private void SetShorteningStrategy(ShorteningRecentRepoPathStrategy strategy)
    {
        switch (strategy)
        {
            case ShorteningRecentRepoPathStrategy.None:
                dontShortenRB.IsChecked = true;
                break;
            case ShorteningRecentRepoPathStrategy.MostSignDir:
                mostSigDirRB.IsChecked = true;
                break;
            case ShorteningRecentRepoPathStrategy.MiddleDots:
                middleDotRB.IsChecked = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unhandled shortening strategy.");
        }
    }

    private void SaveSettings()
    {
        Validates.NotNull(_repositoryHistory);
        AppSettings.ShorteningRecentRepoPathStrategy = GetShorteningStrategy();
        AppSettings.HideTopRepositoriesFromRecentList.Value = hideTopRepositoriesFromRecentList.IsChecked == true;
        AppSettings.SortTopRepos = sortTopRepos.IsChecked == true;
        AppSettings.SortRecentRepos = sortRecentRepos.IsChecked == true;
        AppSettings.MaxTopRepositories = Convert.ToInt32(_NO_TRANSLATE_maxRecentRepositories.Value);
        AppSettings.RecentReposComboMinWidth = Convert.ToInt32(comboMinWidthEdit.Value);
        AppSettings.RecentRepositoriesHistorySize = Convert.ToInt32(_NO_TRANSLATE_RecentRepositoriesHistorySize.Value);
        ThreadHelper.JoinableTaskFactory.Run(() => _saveRecentAsync(_repositoryHistory));
    }

    private ShorteningRecentRepoPathStrategy GetShorteningStrategy()
    {
        if (TryGetShorteningStrategy(out ShorteningRecentRepoPathStrategy strategy))
        {
            return strategy;
        }

        throw new InvalidOperationException("Cannot determine the shortening strategy.");
    }

    private bool TryGetShorteningStrategy(out ShorteningRecentRepoPathStrategy strategy)
    {
        if (dontShortenRB.IsChecked == true)
        {
            strategy = ShorteningRecentRepoPathStrategy.None;
            return true;
        }

        if (mostSigDirRB.IsChecked == true)
        {
            strategy = ShorteningRecentRepoPathStrategy.MostSignDir;
            return true;
        }

        if (middleDotRB.IsChecked == true)
        {
            strategy = ShorteningRecentRepoPathStrategy.MiddleDots;
            return true;
        }

        strategy = default;
        return false;
    }

    private void RefreshRepos()
    {
        Validates.NotNull(_repositoryHistory);
        List<RecentRepoInfo> topRepos = [];
        List<RecentRepoInfo> recentRepos = [];
        RecentRepoSplitter splitter = new()
        {
            MaxTopRepositories = Convert.ToInt32(_NO_TRANSLATE_maxRecentRepositories.Value),
            HideTopRepositoriesFromRecentList = hideTopRepositoriesFromRecentList.IsChecked == true,
            ShorteningStrategy = GetShorteningStrategy(),
            SortRecentRepos = sortRecentRepos.IsChecked == true,
            SortTopRepos = sortTopRepos.IsChecked == true,
            RecentReposComboMinWidth = Convert.ToInt32(comboMinWidthEdit.Value),
            MeasureFont = AppSettings.Font,
        };

        splitter.SplitRecentRepos(_repositoryHistory, topRepos, recentRepos);
        TopLB.ItemsSource = topRepos;
        RecentLB.ItemsSource = recentRepos;
        SetComboWidth();
    }

    private void SetComboWidth()
    {
        double width = Convert.ToDouble(comboMinWidthEdit.Value);
        double maxWidth = width == 0 ? double.PositiveInfinity : Math.Max(MinComboWidthAllowed, width);
        TopLB.MaxWidth = maxWidth;
        RecentLB.MaxWidth = maxWidth;
    }

    private void sortTopRepos_CheckedChanged(object? sender, EventArgs e)
    {
        if (!_updating && TryGetShorteningStrategy(out _))
        {
            RefreshRepos();
        }
    }

    private void comboMinWidthEdit_ValueChanged(object? sender, EventArgs e)
    {
        if (_updating)
        {
            return;
        }

        decimal value = comboMinWidthEdit.Value ?? 0;
        if (value == _previousValue)
        {
            return;
        }

        if (value < _previousValue && value < MinComboWidthAllowed)
        {
            comboMinWidthEdit.Value = 0;
        }
        else if (value > _previousValue && value < MinComboWidthAllowed)
        {
            comboMinWidthEdit.Value = MinComboWidthAllowed;
        }

        _previousValue = comboMinWidthEdit.Value ?? 0;
        RefreshRepos();
    }

    private void Ok_Click(object? sender, EventArgs e)
    {
        SaveSettings();
        DialogResult = WinFormsShims.DialogResult.OK;
    }

    private void Abort_Click(object? sender, EventArgs e)
        => DialogResult = WinFormsShims.DialogResult.Cancel;

    private void contextMenuStrip1_Opening(object? sender, CancelEventArgs e)
    {
        _contextList = contextMenuStrip1.PlacementTarget as ListBox ?? _contextList;
        List<RecentRepoInfo> repos = GetSelectedRepos();
        e.Cancel = repos.Count == 0;
        if (e.Cancel)
        {
            return;
        }

        anchorToTopReposToolStripMenuItem.IsEnabled = repos.All(
            repo => repo.Repo.Anchor != Repository.RepositoryAnchor.AnchoredInTop);
        anchorToRecentReposToolStripMenuItem.IsEnabled = repos.All(
            repo => repo.Repo.Anchor != Repository.RepositoryAnchor.AnchoredInRecent);
        removeAnchorToolStripMenuItem.IsEnabled = repos.Any(
            repo => repo.Repo.Anchor != Repository.RepositoryAnchor.None);
    }

    private List<RecentRepoInfo> GetSelectedRepos()
        => _contextList?.SelectedItems is { } selectedItems
            ? [.. selectedItems.OfType<RecentRepoInfo>()]
            : [];

    private void ListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox || !e.GetCurrentPoint(listBox).Properties.IsRightButtonPressed)
        {
            return;
        }

        _contextList = listBox;
        ListBoxItem? item = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true);
        if (listBox.SelectedItems is { } selectedItems
            && item?.DataContext is RecentRepoInfo repo
            && !selectedItems.Contains(repo))
        {
            selectedItems.Clear();
            selectedItems.Add(repo);
        }
    }

    private void AllRecentLB_DoubleClick(object? sender, EventArgs e)
        => AnchorToMostRecentRepositories(RecentLB);

    private void TopLB_DoubleClick(object? sender, EventArgs e)
        => AnchorToLessRecentRepositories(TopLB);

    private void anchorToMostToolStripMenuItem_Click(object? sender, EventArgs e)
        => AnchorToMostRecentRepositories(_contextList);

    private void AnchorToMostRecentRepositories(ListBox? list)
    {
        _contextList = list;
        foreach (RecentRepoInfo repo in GetSelectedRepos())
        {
            repo.Repo.Anchor = Repository.RepositoryAnchor.AnchoredInTop;
        }

        RefreshRepos();
    }

    private void anchorToLessToolStripMenuItem_Click(object? sender, EventArgs e)
        => AnchorToLessRecentRepositories(_contextList);

    private void AnchorToLessRecentRepositories(ListBox? list)
    {
        _contextList = list;
        foreach (RecentRepoInfo repo in GetSelectedRepos())
        {
            repo.Repo.Anchor = Repository.RepositoryAnchor.AnchoredInRecent;
        }

        RefreshRepos();
    }

    private void removeAnchorToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        foreach (RecentRepoInfo repo in GetSelectedRepos())
        {
            repo.Repo.Anchor = Repository.RepositoryAnchor.None;
        }

        RefreshRepos();
    }

    private void removeRecentToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        foreach (RecentRepoInfo repo in GetSelectedRepos())
        {
            _repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => _removeRecentAsync(repo.Repo.Path));
        }

        RefreshRepos();
    }

    private Task<IList<Repository>> RemoveRecentFromMemoryAsync(string path)
    {
        Validates.NotNull(_repositoryHistory);
        Repository? repository = _repositoryHistory.FirstOrDefault(
            repo => string.Equals(repo.Path, path, StringComparison.OrdinalIgnoreCase));
        if (repository is not null)
        {
            _repositoryHistory.Remove(repository);
        }

        return Task.FromResult(_repositoryHistory);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormRecentReposSettings form)
    {
        public ListBox TopRepositories => form.TopLB;

        public ListBox RecentRepositories => form.RecentLB;

        public NumericUpDown MaximumTopRepositories => form._NO_TRANSLATE_maxRecentRepositories;

        public NumericUpDown HistorySize => form._NO_TRANSLATE_RecentRepositoriesHistorySize;

        public NumericUpDown MinimumWidth => form.comboMinWidthEdit;

        public CheckBox HideTopRepositories => form.hideTopRepositoriesFromRecentList;

        public CheckBox SortTopRepositories => form.sortTopRepos;

        public CheckBox SortRecentRepositories => form.sortRecentRepos;

        public RadioButton DoNotShorten => form.dontShortenRB;

        public RadioButton MiddleDots => form.middleDotRB;

        public RadioButton MostSignificantDirectory => form.mostSigDirRB;

        public void SaveSettings() => form.SaveSettings();

        public void SetContextList(ListBox list) => form._contextList = list;

        public void AnchorSelectedToTop() => form.AnchorToMostRecentRepositories(form._contextList);

        public void AnchorSelectedToRecent() => form.AnchorToLessRecentRepositories(form._contextList);

        public void RemoveSelectedAnchor() => form.removeAnchorToolStripMenuItem_Click(null, EventArgs.Empty);

        public void RemoveSelectedRecent() => form.removeRecentToolStripMenuItem_Click(null, EventArgs.Empty);
    }
}
