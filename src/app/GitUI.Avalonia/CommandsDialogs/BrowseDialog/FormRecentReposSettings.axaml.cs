using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI.Compat;
using Microsoft;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog;

public partial class FormRecentReposSettings : GitExtensionsForm
{
    private const int MinComboWidthAllowed = 30;
    private readonly Func<string, Task<IList<Repository>>> _removeRecentAsync;
    private readonly Func<IEnumerable<Repository>, Task> _saveRecentAsync;
    private IList<Repository>? _repositoryHistory;
    private decimal _previousValue;

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
        : base(enablePositionRestore: true)
    {
        _repositoryHistory = repositoryHistory;
        _saveRecentAsync = saveRecentAsync ?? (_ => Task.CompletedTask);
        _removeRecentAsync = removeRecentAsync ?? RemoveRecentFromMemoryAsync;

        InitializeComponent();
        contextMenuStrip1.Opening += contextMenuStrip1_Opening;
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
        InitializeComplete();
        LoadSettings();
        RefreshRepos();
        _NO_TRANSLATE_maxRecentRepositories.ValueChanged += sortTopRepos_CheckedChanged;
        hideTopRepositoriesFromRecentList.IsCheckedChanged += sortTopRepos_CheckedChanged;
        sortTopRepos.IsCheckedChanged += sortTopRepos_CheckedChanged;
        sortRecentRepos.IsCheckedChanged += sortTopRepos_CheckedChanged;
        dontShortenRB.IsCheckedChanged += sortTopRepos_CheckedChanged;
        middleDotRB.IsCheckedChanged += sortTopRepos_CheckedChanged;
        mostSigDirRB.IsCheckedChanged += sortTopRepos_CheckedChanged;
        comboMinWidthEdit.ValueChanged += comboMinWidthEdit_ValueChanged;
        return;

        FuncDataTemplate<RecentRepoInfo> CreateRepositoryTemplate()
            => new(
                (repo, _) =>
                {
                    if (repo is null)
                    {
                        return new Border();
                    }

                    return GetRepositoryListViewItem(repo, repo.Anchored);
                },
                supportsRecycling: true);
    }

    private void LoadSettings()
    {
        SetShorteningStrategy(AppSettings.ShorteningRecentRepoPathStrategy);
        hideTopRepositoriesFromRecentList.IsChecked = AppSettings.HideTopRepositoriesFromRecentList;
        sortTopRepos.IsChecked = AppSettings.SortTopRepos;
        sortRecentRepos.IsChecked = AppSettings.SortRecentRepos;
        comboMinWidthEdit.Value = AppSettings.RecentReposComboMinWidth;
        SetNumericUpDownValue(_NO_TRANSLATE_maxRecentRepositories, AppSettings.MaxTopRepositories);
        SetNumericUpDownValue(_NO_TRANSLATE_RecentRepositoriesHistorySize, AppSettings.RecentRepositoriesHistorySize);
        _previousValue = comboMinWidthEdit.Value ?? 0;

        return;

        static void SetNumericUpDownValue(NumericUpDown control, int value)
            => control.Value = Math.Min(Math.Max(control.Minimum, value), control.Maximum);

        void SetShorteningStrategy(ShorteningRecentRepoPathStrategy strategy)
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
        if (dontShortenRB.IsChecked == true)
        {
            return ShorteningRecentRepoPathStrategy.None;
        }
        else if (mostSigDirRB.IsChecked == true)
        {
            return ShorteningRecentRepoPathStrategy.MostSignDir;
        }
        else if (middleDotRB.IsChecked == true)
        {
            return ShorteningRecentRepoPathStrategy.MiddleDots;
        }
        else
        {
            throw new Exception("Can not figure shortening strategy");
        }
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
            MeasureFont = new WinFormsShims.Font(TopLB.FontFamily.Name, AvaloniaFontSettings.ToPoints(TopLB.FontSize),
                (TopLB.FontWeight >= FontWeight.Bold ? WinFormsShims.FontStyle.Bold : WinFormsShims.FontStyle.Regular)
                | (TopLB.FontStyle == FontStyle.Italic ? WinFormsShims.FontStyle.Italic : WinFormsShims.FontStyle.Regular)),
        };

        splitter.SplitRecentRepos(_repositoryHistory, topRepos, recentRepos);
        TopLB.ItemsSource = topRepos;
        RecentLB.ItemsSource = recentRepos;
        SetComboWidth();
    }

    private Control GetRepositoryListViewItem(RecentRepoInfo repo, bool anchored)
    {
        TextBlock item = listView_DrawItem(repo);
        item.FontWeight = anchored ? FontWeight.Bold : FontWeight.Normal;

        if (!Directory.Exists(repo.Repo.Path))
        {
            item.Foreground = Brushes.Red;
        }

        ToolTip.SetTip(item, repo.Repo.Path);
        return item;
    }

    private void SetComboWidth()
    {
        double width = Convert.ToDouble(comboMinWidthEdit.Value);
        double columnWidth = width == 0 ? double.NaN : Math.Max(MinComboWidthAllowed, width);
        chdrRepository.SourceWidth = columnWidth;
        chdrRepository1.SourceWidth = columnWidth;
    }

    private void sortTopRepos_CheckedChanged(object? sender, EventArgs e)
    {
        if (dontShortenRB.IsChecked == true || mostSigDirRB.IsChecked == true || middleDotRB.IsChecked == true)
        {
            RefreshRepos();
        }
    }

    private void comboMinWidthEdit_ValueChanged(object? sender, EventArgs e)
    {
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
        if (GetSelectedRepos(sender, out List<RecentRepoInfo>? repos))
        {
            e.Cancel = false;

            foreach (RecentRepoInfo repo in repos)
            {
                anchorToTopReposToolStripMenuItem.IsEnabled = repo.Repo.Anchor != Repository.RepositoryAnchor.AnchoredInTop;
                anchorToRecentReposToolStripMenuItem.IsEnabled = repo.Repo.Anchor != Repository.RepositoryAnchor.AnchoredInRecent;
                removeAnchorToolStripMenuItem.IsEnabled = repo.Repo.Anchor != Repository.RepositoryAnchor.None;
            }
        }
        else
        {
            e.Cancel = true;
        }
    }

    private bool GetSelectedRepos(object? sender, [NotNullWhen(returnValue: true)] out List<RecentRepoInfo>? repos)
    {
        if (sender is ContextMenu strip)
        {
            sender = strip.PlacementTarget;
        }
        else if (sender is MenuItem item)
        {
            return GetSelectedRepos(item.FindLogicalAncestorOfType<ContextMenu>(), out repos);
        }

        ListBox? list = sender == TopLB
            ? TopLB
            : sender == RecentLB
                ? RecentLB
                : null;

        repos = [];
        if (list?.SelectedItems is { } selectedItems)
        {
            repos.AddRange(selectedItems.OfType<RecentRepoInfo>());
        }

        return repos.Count != 0;
    }

    private void ListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox || !e.GetCurrentPoint(listBox).Properties.IsRightButtonPressed)
        {
            return;
        }

        // Avalonia needs an explicit source for a context menu shared by both lists.
        contextMenuStrip1.PlacementTarget = listBox;
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
        => AnchorToMostRecentRepositories(sender!);

    private void TopLB_DoubleClick(object? sender, EventArgs e)
        => AnchorToLessRecentRepositories(sender!);

    private void anchorToMostToolStripMenuItem_Click(object? sender, EventArgs e)
        => AnchorToMostRecentRepositories(sender!);

    private void AnchorToMostRecentRepositories(object sender)
    {
        if (GetSelectedRepos(sender, out List<RecentRepoInfo>? repos))
        {
            foreach (RecentRepoInfo repo in repos)
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.AnchoredInTop;
            }

            RefreshRepos();
        }
    }

    private void anchorToLessToolStripMenuItem_Click(object? sender, EventArgs e)
        => AnchorToLessRecentRepositories(sender!);

    private void AnchorToLessRecentRepositories(object sender)
    {
        if (GetSelectedRepos(sender, out List<RecentRepoInfo>? repos))
        {
            foreach (RecentRepoInfo repo in repos)
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.AnchoredInRecent;
            }

            RefreshRepos();
        }
    }

    private void removeAnchorToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (GetSelectedRepos(sender, out List<RecentRepoInfo>? repos))
        {
            foreach (RecentRepoInfo repo in repos)
            {
                repo.Repo.Anchor = Repository.RepositoryAnchor.None;
            }

            RefreshRepos();
        }
    }

    private void removeRecentToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (!GetSelectedRepos(sender, out List<RecentRepoInfo>? repos))
        {
            return;
        }

        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            foreach (RecentRepoInfo repo in repos)
            {
                _repositoryHistory = await _removeRecentAsync(repo.Repo.Path);
            }
        });

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

    // Avalonia paints ListBox rows through a TextBlock template rather than a GDI DrawItem event.
    private TextBlock listView_DrawItem(RecentRepoInfo repo)
        => new()
        {
            Width = chdrRepository.SourceWidth,
            Margin = new Thickness(4, 2),
            Text = repo.Caption,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
}
