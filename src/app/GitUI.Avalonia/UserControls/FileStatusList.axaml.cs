using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI;

// Reduced twin of GitUI/UserControls/FileStatusList.cs. It keeps the read-only API used by
// FormBrowse/FormCommit and adds the revision-aware diff entries required by FormStash.
// Filtering, staging interactions, and the full context menu remain deferred.
public partial class FileStatusList : GitModuleControl
{
    private IReadOnlyList<GitItemStatus> _gitItemStatuses = [];
    private Action? _refreshAction;
    private IGitUICommands? _boundCommands;

    public FileStatusList()
    {
        InitializeComponent();

        lstFiles.ItemTemplate = new FuncDataTemplate<object>(CreateFileRow, supportsRecycling: false);
        lstFiles.SelectionChanged += (_, _) => SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        lstFiles.DoubleTapped += (_, _) => DoubleClick?.Invoke(this, EventArgs.Empty);
        UICommandsSourceSet += (_, e) => BindCommands(e.GitUICommandsSource.UICommands);
        AttachedToLogicalTree += (_, _) =>
        {
            if (TryGetUICommandsDirect(out IGitUICommands? commands))
            {
                BindCommands(commands);
            }
        };
        DetachedFromLogicalTree += (_, _) => UnbindCommands();

        InitializeComplete();
    }

    /// <summary>
    ///  Occurs when the selected file changes (named like the WinForms event).
    /// </summary>
    public event EventHandler? SelectedIndexChanged;

    /// <summary>
    ///  Occurs when a file is double-clicked (named like the WinForms event).
    /// </summary>
    public event EventHandler? DoubleClick;

    /// <summary>
    ///  Gets the selected file status item, or <see langword="null"/>.
    /// </summary>
    public GitItemStatus? SelectedItem => GetFileStatusItem(lstFiles.SelectedItem)?.Item
        ?? lstFiles.SelectedItem as GitItemStatus;

    /// <summary>
    ///  Gets the selected revision-aware item, or <see langword="null"/>.
    /// </summary>
    public FileStatusItem? SelectedFileStatusItem => GetFileStatusItem(lstFiles.SelectedItem);

    /// <summary>
    ///  Gets all selected revision-aware items.
    /// </summary>
    public IEnumerable<FileStatusItem> SelectedItems
        => lstFiles.SelectedItems?.Cast<object>().Select(item => GetFileStatusItem(item)).OfType<FileStatusItem>() ?? [];

    /// <summary>
    ///  Gets the displayed file statuses (named like the WinForms property).
    /// </summary>
    public IReadOnlyList<GitItemStatus> GitItemStatuses => _gitItemStatuses;

    /// <summary>
    ///  Gets or sets the selection mode of the underlying list.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => lstFiles.SelectionMode;
        set => lstFiles.SelectionMode = value;
    }

    /// <summary>
    ///  Shows the given diff items.
    /// </summary>
    public void SetDiffs(IReadOnlyList<GitItemStatus> items)
    {
        _gitItemStatuses = items;
        lstFiles.ItemsSource = items;
        UpdateCount(items.Count);

        // Like the WinForms FileStatusList, select the first file automatically.
        if (items.Count > 0)
        {
            lstFiles.SelectedIndex = 0;
        }
    }

    /// <summary>
    ///  Shows files changed between two revisions.
    /// </summary>
    public void SetDiffs(GitRevision? firstRev, GitRevision secondRev, IReadOnlyList<GitItemStatus> items)
    {
        List<FileStatusEntry> entries = items
            .Select(item => new FileStatusEntry(new FileStatusItem(firstRev, secondRev, item), null))
            .ToList();
        SetRevisionEntries(entries);
    }

    /// <summary>
    ///  Shows the worktree and index portions of the current working directory.
    /// </summary>
    public void SetStashDiffs(
        GitRevision headRev,
        GitRevision indexRev,
        string indexDesc,
        IReadOnlyList<GitItemStatus> indexItems,
        GitRevision workTreeRev,
        string workTreeDesc,
        IReadOnlyList<GitItemStatus> workTreeItems)
    {
        List<FileStatusEntry> entries = [];
        entries.AddRange(CreateGroup(indexRev, workTreeRev, workTreeDesc, workTreeItems));
        entries.AddRange(CreateGroup(headRev, indexRev, indexDesc, indexItems));
        SetRevisionEntries(entries);
    }

    /// <summary>
    ///  Clears all diff entries.
    /// </summary>
    public void ClearDiffs() => Clear();

    public void Clear()
    {
        _gitItemStatuses = [];
        lstFiles.ItemsSource = null;
        lblCount.Text = string.Empty;
    }

    /// <summary>
    ///  Clears the current selection (named like the WinForms method).
    /// </summary>
    public void ClearSelected()
    {
        lstFiles.SelectedItem = null;
    }

    /// <summary>
    ///  Selects the preceding visible file, wrapping to the first file at the boundary.
    /// </summary>
    public void SelectPreviousVisibleItem()
    {
        if (lstFiles.ItemCount == 0)
        {
            return;
        }

        lstFiles.SelectedIndex = lstFiles.SelectedIndex > 0
            ? lstFiles.SelectedIndex - 1
            : 0;
        if (lstFiles.SelectedItem is object selectedItem)
        {
            lstFiles.ScrollIntoView(selectedItem);
        }
    }

    /// <summary>
    ///  Selects the following visible file, wrapping to the first file at the boundary.
    /// </summary>
    public void SelectNextVisibleItem()
    {
        if (lstFiles.ItemCount == 0)
        {
            return;
        }

        lstFiles.SelectedIndex = lstFiles.SelectedIndex + 1 < lstFiles.ItemCount
            ? lstFiles.SelectedIndex + 1
            : 0;
        if (lstFiles.SelectedItem is object selectedItem)
        {
            lstFiles.ScrollIntoView(selectedItem);
        }
    }

    /// <summary>
    ///  Refreshes this list when repository-change notifications are raised.
    /// </summary>
    public void Bind(Action refreshAction)
    {
        _refreshAction = refreshAction;
        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            BindCommands(commands);
        }
    }

    private static IEnumerable<FileStatusEntry> CreateGroup(
        GitRevision firstRev,
        GitRevision secondRev,
        string description,
        IReadOnlyList<GitItemStatus> items)
    {
        for (int index = 0; index < items.Count; index++)
        {
            yield return new FileStatusEntry(
                new FileStatusItem(firstRev, secondRev, items[index]),
                index == 0 ? description : null);
        }
    }

    private void SetRevisionEntries(IReadOnlyList<FileStatusEntry> entries)
    {
        _gitItemStatuses = entries.Select(entry => entry.Item.Item).ToList();
        lstFiles.ItemsSource = entries;
        UpdateCount(entries.Count);
        if (entries.Count > 0)
        {
            lstFiles.SelectedIndex = 0;
        }
    }

    private void UpdateCount(int count)
    {
        lblCount.Text = $"{count} {(count == 1 ? "file" : "files")}";
    }

    private void BindCommands(IGitUICommands commands)
    {
        if (_refreshAction is null || ReferenceEquals(_boundCommands, commands))
        {
            return;
        }

        UnbindCommands();
        _boundCommands = commands;
        _boundCommands.PostRepositoryChanged += BoundCommands_PostRepositoryChanged;
    }

    private void UnbindCommands()
    {
        if (_boundCommands is not null)
        {
            _boundCommands.PostRepositoryChanged -= BoundCommands_PostRepositoryChanged;
            _boundCommands = null;
        }
    }

    private void BoundCommands_PostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        _refreshAction?.Invoke();
    }

    private static FileStatusItem? GetFileStatusItem(object? item)
        => item switch
        {
            FileStatusEntry entry => entry.Item,
            FileStatusItem fileStatusItem => fileStatusItem,
            _ => null,
        };

    private static Control CreateFileRow(object item, INameScope nameScope)
    {
        // Avalonia can briefly rebuild a recycled presenter with null while ItemsSource is
        // replaced after staging. The typed template annotation does not expose that state.
        if (item is null)
        {
            return new Panel();
        }

        FileStatusEntry? entry = item as FileStatusEntry;
        GitItemStatus? gitItemStatus = entry?.Item.Item ?? item as GitItemStatus;
        if (gitItemStatus is null)
        {
            return new Panel();
        }

        (string marker, Avalonia.Media.Color color) = gitItemStatus switch
        {
            { IsNew: true } => ("A", Colors.SeaGreen),
            { IsDeleted: true } => ("D", Colors.IndianRed),
            { IsRenamed: true } => ("R", Colors.SteelBlue),
            _ => ("M", Colors.Goldenrod),
        };

        StackPanel row = new()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };
        if (entry?.GroupDescription is not null)
        {
            row.Children.Add(new TextBlock
            {
                Text = entry.GroupDescription,
                FontWeight = FontWeight.Bold,
                Margin = new Avalonia.Thickness(4, 5, 4, 2),
            });
        }

        row.Children.Add(
            new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = marker,
                        Foreground = new SolidColorBrush(color),
                        FontWeight = FontWeight.Bold,
                        Width = 18,
                        Margin = new Avalonia.Thickness(4, 0, 0, 0),
                    },
                    new TextBlock
                    {
                        Text = gitItemStatus.Name,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                    },
                },
            });

        return row;
    }

    private sealed record FileStatusEntry(FileStatusItem Item, string? GroupDescription);
}
