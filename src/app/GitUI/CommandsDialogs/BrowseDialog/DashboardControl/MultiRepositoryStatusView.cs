using System.Reflection;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

internal sealed class MultiRepositoryStatusView : UserControl
{
    private const string UnclassifiedGroupKey = "__gitextensions_unclassified__";
    private static readonly PropertyInfo? ListViewGroupAccessibilityObjectProperty = typeof(ListViewGroup).GetProperty("AccessibilityObject", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly Button _emptyBackButton = new() { AutoSize = true, Text = "回到传统视图" };
    private readonly Label _emptyDescription = new() { AutoSize = true, Text = "请在传统视图中将仓库设为收藏。" };
    private readonly PictureBox _emptyIcon = new() { SizeMode = PictureBoxSizeMode.CenterImage };
    private readonly TableLayoutPanel _emptyState = new();
    private readonly Label _emptyTitle = new() { AutoSize = true, Text = "暂无收藏仓库" };
    private readonly ImageList _images = new();
    private readonly ListView _list = new();
    private readonly MultiRepositoryStatusLayoutCache _layoutCache = new();

    private Dictionary<string, MultiRepositoryStatus> _statuses = new(StringComparer.OrdinalIgnoreCase);
    private List<Repository> _repositories = [];
    private MultiRepositoryStatusLayout _layout;
    private DashboardTheme _theme = DashboardTheme.Light;
    private Font _secondaryFont;
    private string _searchText = "";
    private string? _selectedPath;
    private ListViewItem? _hoveredItem;
    private string? _draggedRepositoryPath;
    private string? _dropTargetPath;
    private bool _dropAfter;
    private string? _pressedGroupKey;
    private bool _pressedGroupWasCollapsed;
    private string? _focusedGroupKey;
    private string? _groupDropTargetKey;
    private Point _groupDragStart;
    private bool _groupDragging;
    private bool _rebuilding;

    public MultiRepositoryStatusView()
    {
        _layout = _layoutCache.Load();
        _secondaryFont = new Font(AppSettings.Font.FontFamily, Math.Max(6, AppSettings.Font.SizeInPoints - 1f));
        InitializeUi();
        WireEvents();
    }

    public event EventHandler? RepositoryActivated;
    public event EventHandler? SelectedRepositoryChanged;
    public event EventHandler? ReturnToTraditionalRequested;

    public Repository? SelectedRepository
        => _list.SelectedItems.Count == 0 ? null : _list.SelectedItems[0].Tag as Repository;

    public void ApplyTheme(DashboardTheme theme)
    {
        _theme = theme;
        BackColor = SystemColors.Window;
        _list.BackColor = SystemColors.Window;
        _list.ForeColor = theme.PrimaryText;
        _emptyState.BackColor = SystemColors.Window;
        _emptyTitle.ForeColor = theme.PrimaryText;
        _emptyDescription.ForeColor = theme.SecondaryText;
        _list.Invalidate();
    }

    public void SetContent(
        IReadOnlyList<Repository> repositories,
        IReadOnlyDictionary<string, MultiRepositoryStatus> statuses)
    {
        _repositories = [.. repositories];
        _statuses = new Dictionary<string, MultiRepositoryStatus>(statuses, StringComparer.OrdinalIgnoreCase);
        SynchronizeLayout();
        RebuildItems();
    }

    public void SetSearchText(string text)
    {
        string normalized = text.Trim();
        if (string.Equals(_searchText, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _searchText = normalized;
        RebuildItems();
    }

    public void RefreshRelativeTimes()
    {
        foreach (ListViewItem item in _list.Items)
        {
            if (item.Tag is Repository repository)
            {
                _statuses.TryGetValue(repository.Path, out MultiRepositoryStatus? status);
                item.ToolTipText = BuildToolTip(repository, status);
            }
        }

        _list.Invalidate();
    }

    public void ResetOrdering()
    {
        _layout.GroupOrder.Clear();
        _layout.RepositoryOrder.Clear();
        SynchronizeLayout();
        SaveLayout();
        RebuildItems();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _secondaryFont.Dispose();
            _images.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeUi()
    {
        SuspendLayout();

        _images.ColorDepth = ColorDepth.Depth32Bit;
        _images.ImageSize = DpiUtil.Scale(new Size(32, 32));
        _images.Images.Add(Images.DashboardFolderGit);
        _images.Images.Add(Images.DashboardFolderError);

        _list.AllowDrop = true;
        _list.BorderStyle = BorderStyle.None;
        _list.Dock = DockStyle.Fill;
        _list.FullRowSelect = true;
        _list.HeaderStyle = ColumnHeaderStyle.None;
        _list.HideSelection = false;
        _list.LargeImageList = _images;
        _list.MultiSelect = false;
        _list.OwnerDraw = true;
        _list.ShowGroups = true;
        _list.ShowItemToolTips = true;
        _list.UseCompatibleStateImageBehavior = false;
        _list.View = View.Tile;

        _emptyTitle.Font = new Font(AppSettings.Font, FontStyle.Bold);
        _emptyIcon.Image = DpiUtil.Scale(Images.DashboardFolderGit);
        _emptyIcon.Size = DpiUtil.Scale(new Size(64, 64));
        _emptyState.AutoSize = true;
        _emptyState.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _emptyState.ColumnCount = 1;
        _emptyState.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _emptyState.Controls.Add(_emptyIcon, 0, 0);
        _emptyState.Controls.Add(_emptyTitle, 0, 1);
        _emptyState.Controls.Add(_emptyDescription, 0, 2);
        _emptyState.Controls.Add(_emptyBackButton, 0, 3);
        _emptyState.Dock = DockStyle.Fill;
        _emptyState.RowCount = 4;
        _emptyState.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        _emptyState.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _emptyState.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _emptyState.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        foreach (Control control in _emptyState.Controls)
        {
            control.Anchor = AnchorStyles.None;
            control.Margin = new Padding(6);
        }

        Controls.Add(_list);
        Controls.Add(_emptyState);
        Dock = DockStyle.Fill;
        UpdateTileSize();

        ResumeLayout(performLayout: true);
    }

    private void WireEvents()
    {
        _emptyBackButton.Click += (_, _) => ReturnToTraditionalRequested?.Invoke(this, EventArgs.Empty);
        _list.DrawItem += List_DrawItem;
        _list.ItemActivate += (_, _) => RepositoryActivated?.Invoke(this, EventArgs.Empty);
        _list.ItemDrag += List_ItemDrag;
        _list.DragOver += List_DragOver;
        _list.DragDrop += List_DragDrop;
        _list.DragLeave += (_, _) => ClearItemDropTarget();
        _list.SelectedIndexChanged += List_SelectedIndexChanged;
        _list.KeyDown += List_KeyDown;
        _list.MouseDown += List_MouseDown;
        _list.MouseMove += List_MouseMove;
        _list.MouseUp += List_MouseUp;
        _list.MouseLeave += (_, _) => SetHoveredItem(null);
        Resize += (_, _) => UpdateTileSize();
    }

    private void SynchronizeLayout()
    {
        List<string> currentGroups = [.. _repositories
            .Select(GetGroupKey)
            .Where(group => !string.Equals(group, UnclassifiedGroupKey, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)];
        if (_repositories.Any(repository => GetGroupKey(repository) == UnclassifiedGroupKey))
        {
            currentGroups.Add(UnclassifiedGroupKey);
        }

        _layout.GroupOrder.RemoveAll(group => !currentGroups.Contains(group, StringComparer.OrdinalIgnoreCase));
        foreach (string group in currentGroups.Where(group => group != UnclassifiedGroupKey))
        {
            if (!_layout.GroupOrder.Contains(group, StringComparer.OrdinalIgnoreCase))
            {
                _layout.GroupOrder.Add(group);
            }
        }

        _layout.GroupOrder.RemoveAll(group => group == UnclassifiedGroupKey);
        if (currentGroups.Contains(UnclassifiedGroupKey, StringComparer.OrdinalIgnoreCase))
        {
            _layout.GroupOrder.Add(UnclassifiedGroupKey);
        }

        foreach (string obsoleteGroup in _layout.RepositoryOrder.Keys
                     .Where(group => !currentGroups.Contains(group, StringComparer.OrdinalIgnoreCase)).ToList())
        {
            _layout.RepositoryOrder.Remove(obsoleteGroup);
        }

        foreach (string group in currentGroups)
        {
            List<string> currentPaths = [.. _repositories.Where(repository => GroupKeysEqual(GetGroupKey(repository), group)).Select(repository => repository.Path)];
            if (!_layout.RepositoryOrder.TryGetValue(group, out List<string>? paths))
            {
                paths = [];
                _layout.RepositoryOrder[group] = paths;
            }

            paths.RemoveAll(path => !currentPaths.Contains(path, StringComparer.OrdinalIgnoreCase));
            foreach (string path in currentPaths)
            {
                if (!paths.Contains(path, StringComparer.OrdinalIgnoreCase))
                {
                    paths.Add(path);
                }
            }
        }

        _layout.CollapsedGroups.RemoveWhere(group => !currentGroups.Contains(group, StringComparer.OrdinalIgnoreCase));
    }

    private void RebuildItems()
    {
        _rebuilding = true;
        _list.BeginUpdate();
        try
        {
            _list.Items.Clear();
            _list.Groups.Clear();

            foreach (string groupKey in _layout.GroupOrder)
            {
                List<Repository> visibleRepositories = [.. GetOrderedRepositories(groupKey).Where(MatchesSearch)];
                if (visibleRepositories.Count == 0)
                {
                    continue;
                }

                ListViewGroup group = new($"{GetGroupDisplayName(groupKey)}（{visibleRepositories.Count}）", HorizontalAlignment.Left)
                {
                    Name = groupKey,
                    TaskLink = "⋮⋮",
                    CollapsedState = string.IsNullOrEmpty(_searchText) && _layout.CollapsedGroups.Contains(groupKey)
                        ? ListViewGroupCollapsedState.Collapsed
                        : ListViewGroupCollapsedState.Expanded
                };
                _list.Groups.Add(group);

                foreach (Repository repository in visibleRepositories)
                {
                    _statuses.TryGetValue(repository.Path, out MultiRepositoryStatus? status);
                    ListViewItem item = new(GetRepositoryName(repository))
                    {
                        Group = group,
                        ImageIndex = Directory.Exists(repository.Path) ? 0 : 1,
                        Tag = repository,
                        ToolTipText = BuildToolTip(repository, status)
                    };
                    _list.Items.Add(item);
                    if (_selectedPath is not null && PathsEqual(_selectedPath, repository.Path))
                    {
                        item.Selected = true;
                        item.Focused = true;
                    }
                }
            }
        }
        finally
        {
            _list.EndUpdate();
            _rebuilding = false;
        }

        bool isEmpty = _repositories.Count == 0;
        _emptyState.Visible = isEmpty;
        _list.Visible = !isEmpty;
        UpdateTileSize();
        SelectedRepositoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerable<Repository> GetOrderedRepositories(string groupKey)
    {
        Dictionary<string, Repository> repositories = _repositories
            .Where(repository => GroupKeysEqual(GetGroupKey(repository), groupKey))
            .ToDictionary(repository => repository.Path, StringComparer.OrdinalIgnoreCase);

        if (_layout.RepositoryOrder.TryGetValue(groupKey, out List<string>? paths))
        {
            foreach (string path in paths)
            {
                if (repositories.Remove(path, out Repository? repository))
                {
                    yield return repository;
                }
            }
        }

        foreach (Repository repository in repositories.Values)
        {
            yield return repository;
        }
    }

    private bool MatchesSearch(Repository repository)
    {
        if (string.IsNullOrEmpty(_searchText))
        {
            return true;
        }

        _statuses.TryGetValue(repository.Path, out MultiRepositoryStatus? status);
        return GetRepositoryName(repository).Contains(_searchText, StringComparison.CurrentCultureIgnoreCase)
            || repository.Path.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase)
            || (repository.Category?.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase) ?? false)
            || (status?.Branch.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    private void List_DrawItem(object? sender, DrawListViewItemEventArgs e)
    {
        if (e.Item.Tag is not Repository repository)
        {
            return;
        }

        _statuses.TryGetValue(repository.Path, out MultiRepositoryStatus? status);
        bool selected = e.Item.Selected;
        if (selected || e.Item == _hoveredItem)
        {
            using SolidBrush brush = new(_theme.StartBackColor);
            e.Graphics.FillRectangle(brush, e.Bounds);
        }
        else
        {
            e.DrawBackground();
        }

        int padding = DpiUtil.Scale(8);
        int iconSize = DpiUtil.Scale(32);
        int overlaySize = DpiUtil.Scale(16);
        Rectangle iconBounds = new(e.Bounds.Left + padding, e.Bounds.Top + padding, iconSize, iconSize);
        Image folderImage = Directory.Exists(repository.Path) ? Images.DashboardFolderGit : Images.DashboardFolderError;
        e.Graphics.DrawImage(folderImage, iconBounds);
        Image stateImage = GetWorkingTreeStateImage(status);
        e.Graphics.DrawImage(stateImage, iconBounds.Right - overlaySize, iconBounds.Bottom - overlaySize, overlaySize, overlaySize);

        int textLeft = iconBounds.Right + padding;
        int textWidth = Math.Max(0, e.Bounds.Right - textLeft - padding);
        int lineHeight = TextRenderer.MeasureText("Ag", AppSettings.Font, Size.Empty, TextFormatFlags.NoPadding).Height;
        int y = e.Bounds.Top + padding - DpiUtil.Scale(1);

        string name = GetRepositoryName(repository);
        Size nameSize = TextRenderer.MeasureText(name, AppSettings.Font, new Size(textWidth, lineHeight), TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        int nameWidth = Math.Min(textWidth, nameSize.Width + DpiUtil.Scale(8));
        TextRenderer.DrawText(e.Graphics, name, AppSettings.Font, new Rectangle(textLeft, y, nameWidth, lineHeight), _theme.PrimaryText,
            TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
        if (textWidth - nameWidth > DpiUtil.Scale(40))
        {
            TextRenderer.DrawText(e.Graphics, GetShortPath(repository.Path), _secondaryFont,
                new Rectangle(textLeft + nameWidth, y, textWidth - nameWidth, lineHeight), _theme.SecondaryText,
                TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        y += lineHeight + DpiUtil.Scale(2);
        string branchAndWorkingTree = $"{(string.IsNullOrWhiteSpace(status?.Branch) ? "无分支" : status.Branch)} · {MultiRepositoryStatusPresentation.FormatWorkingTree(status)}";
        TextRenderer.DrawText(e.Graphics, branchAndWorkingTree, _secondaryFont, new Rectangle(textLeft, y, textWidth, lineHeight), _theme.SecondaryText,
            TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

        y += lineHeight + DpiUtil.Scale(2);
        Rectangle labelsBounds = new(textLeft, y, textWidth, lineHeight + DpiUtil.Scale(4));
        int labelOffset = 0;
        foreach (MultiRepositorySyncLabel label in MultiRepositoryStatusPresentation.GetSynchronizationLabels(status))
        {
            RevisionGridRefRenderer.DrawRef(selected, _secondaryFont, ref labelOffset, label.Text, GetLabelColor(label.Kind), RefLabelIcon.None,
                labelsBounds, e.Graphics, fill: true);
            if (labelOffset >= labelsBounds.Width)
            {
                break;
            }
        }

        y += labelsBounds.Height + DpiUtil.Scale(1);
        string times = $"上次 Fetch {MultiRepositoryStatusPresentation.FormatFetchTimestamp(status?.LastFetchUtc, DateTimeOffset.UtcNow)} · 检查 {MultiRepositoryStatusPresentation.FormatCheckedTimestamp(status?.LastCheckedUtc)}";
        TextRenderer.DrawText(e.Graphics, times, _secondaryFont, new Rectangle(textLeft, y, textWidth, lineHeight), _theme.SecondaryText,
            TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

        y += lineHeight + DpiUtil.Scale(2);
        DrawError(e.Graphics, selected, status, new Rectangle(textLeft, y, textWidth, lineHeight + DpiUtil.Scale(4)));

        if (e.Item == _hoveredItem && string.IsNullOrEmpty(_searchText))
        {
            TextRenderer.DrawText(e.Graphics, "⋮⋮", _secondaryFont,
                new Rectangle(e.Bounds.Right - DpiUtil.Scale(24), e.Bounds.Top + padding, DpiUtil.Scale(18), lineHeight), _theme.SecondaryText,
                TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter);
        }

        if (_dropTargetPath is not null && PathsEqual(_dropTargetPath, repository.Path))
        {
            using Pen pen = new(_theme.AccentedText, Math.Max(2, DpiUtil.Scale(2)));
            int lineY = _dropAfter ? e.Bounds.Bottom - 1 : e.Bounds.Top + 1;
            e.Graphics.DrawLine(pen, e.Bounds.Left + padding, lineY, e.Bounds.Right - padding, lineY);
        }
    }

    private void DrawError(Graphics graphics, bool selected, MultiRepositoryStatus? status, Rectangle bounds)
    {
        string? error = status?.FetchError ?? status?.StatusError;
        if (string.IsNullOrWhiteSpace(error))
        {
            return;
        }

        string label = status?.FetchError is not null ? "Fetch 失败" : "检查失败";
        int offset = 0;
        Rectangle labelBounds = RevisionGridRefRenderer.DrawRef(selected, _secondaryFont, ref offset, label,
            GetLabelColor(MultiRepositorySyncLabelKind.Error), RefLabelIcon.None, bounds, graphics, fill: true);
        int textLeft = labelBounds.IsEmpty ? bounds.Left : labelBounds.Right + DpiUtil.Scale(5);
        TextRenderer.DrawText(graphics, error, _secondaryFont, new Rectangle(textLeft, bounds.Top, Math.Max(0, bounds.Right - textLeft), bounds.Height),
            GetLabelColor(MultiRepositorySyncLabelKind.Error), TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
    }

    private void List_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_rebuilding)
        {
            return;
        }

        if (SelectedRepository is { } repository)
        {
            _selectedPath = repository.Path;
            _focusedGroupKey = null;
        }

        SelectedRepositoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void List_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && SelectedRepository is not null)
        {
            RepositoryActivated?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.F5 && SelectedRepository is not null)
        {
            RefreshSelectedRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            return;
        }

        if (!e.Alt || e.KeyCode is not (Keys.Up or Keys.Down) || !string.IsNullOrEmpty(_searchText))
        {
            return;
        }

        int direction = e.KeyCode == Keys.Up ? -1 : 1;
        if (_focusedGroupKey is not null)
        {
            MoveGroup(_focusedGroupKey, direction);
        }
        else if (SelectedRepository is { } repository)
        {
            MoveRepository(repository, direction);
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    public event EventHandler? RefreshSelectedRequested;

    private void List_ItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (!string.IsNullOrEmpty(_searchText) || e.Item is not ListViewItem { Tag: Repository repository } item)
        {
            return;
        }

        _draggedRepositoryPath = repository.Path;
        _list.DoDragDrop(item, DragDropEffects.Move);
        _draggedRepositoryPath = null;
        ClearItemDropTarget();
    }

    private void List_DragOver(object? sender, DragEventArgs e)
    {
        e.Effect = DragDropEffects.None;
        if (!string.IsNullOrEmpty(_searchText) || _draggedRepositoryPath is null)
        {
            return;
        }

        Point point = _list.PointToClient(new Point(e.X, e.Y));
        ListViewItem? target = _list.GetItemAt(point.X, point.Y);
        ListViewItem? source = _list.Items.Cast<ListViewItem>()
            .FirstOrDefault(item => item.Tag is Repository repository && PathsEqual(repository.Path, _draggedRepositoryPath));
        if (target?.Tag is not Repository targetRepository || source?.Group != target.Group)
        {
            ClearItemDropTarget();
            return;
        }

        _dropAfter = point.Y > target.Bounds.Top + (target.Bounds.Height / 2)
            || (Math.Abs(point.Y - (target.Bounds.Top + (target.Bounds.Height / 2))) < target.Bounds.Height / 3
                && point.X > target.Bounds.Left + (target.Bounds.Width / 2));
        _dropTargetPath = targetRepository.Path;
        e.Effect = DragDropEffects.Move;
        target.EnsureVisible();
        _list.Invalidate();
    }

    private void List_DragDrop(object? sender, DragEventArgs e)
    {
        if (_draggedRepositoryPath is null || _dropTargetPath is null || PathsEqual(_draggedRepositoryPath, _dropTargetPath))
        {
            ClearItemDropTarget();
            return;
        }

        Repository? source = _repositories.FirstOrDefault(repository => PathsEqual(repository.Path, _draggedRepositoryPath));
        Repository? target = _repositories.FirstOrDefault(repository => PathsEqual(repository.Path, _dropTargetPath));
        if (source is null || target is null || !GroupKeysEqual(GetGroupKey(source), GetGroupKey(target)))
        {
            ClearItemDropTarget();
            return;
        }

        List<string> paths = _layout.RepositoryOrder[GetGroupKey(source)];
        paths.RemoveAll(path => PathsEqual(path, source.Path));
        int targetIndex = paths.FindIndex(path => PathsEqual(path, target.Path));
        paths.Insert(Math.Clamp(targetIndex + (_dropAfter ? 1 : 0), 0, paths.Count), source.Path);
        _selectedPath = source.Path;
        SaveLayout();
        RebuildItems();
        ClearItemDropTarget();
    }

    private void List_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !string.IsNullOrEmpty(_searchText))
        {
            return;
        }

        _pressedGroupKey = FindGroupHeaderAt(e.Location);
        if (_pressedGroupKey is not null)
        {
            _pressedGroupWasCollapsed = _layout.CollapsedGroups.Contains(_pressedGroupKey);
            _focusedGroupKey = _pressedGroupKey;
            _selectedPath = null;
            _list.SelectedItems.Clear();
            _list.Focus();
            _groupDragStart = e.Location;
        }
    }

    private void List_MouseMove(object? sender, MouseEventArgs e)
    {
        SetHoveredItem(_list.GetItemAt(e.X, e.Y));
        if (_pressedGroupKey is null || e.Button != MouseButtons.Left || !string.IsNullOrEmpty(_searchText))
        {
            return;
        }

        if (!_groupDragging
            && Math.Abs(e.X - _groupDragStart.X) < SystemInformation.DragSize.Width / 2
            && Math.Abs(e.Y - _groupDragStart.Y) < SystemInformation.DragSize.Height / 2)
        {
            return;
        }

        _groupDragging = true;
        Cursor = Cursors.SizeAll;
        _groupDropTargetKey = FindGroupAt(e.Location) ?? _groupDropTargetKey;
    }

    private void List_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_groupDragging && _pressedGroupKey is not null && _groupDropTargetKey is not null)
        {
            MoveGroupBefore(_pressedGroupKey, _groupDropTargetKey);
        }
        else if (_pressedGroupKey is not null && string.IsNullOrEmpty(_searchText))
        {
            SetGroupCollapsedState(_pressedGroupKey, !_pressedGroupWasCollapsed);
        }

        _pressedGroupKey = null;
        _groupDropTargetKey = null;
        _groupDragging = false;
        Cursor = Cursors.Default;
    }

    private void MoveRepository(Repository repository, int direction)
    {
        string groupKey = GetGroupKey(repository);
        List<string> paths = _layout.RepositoryOrder[groupKey];
        int index = paths.FindIndex(path => PathsEqual(path, repository.Path));
        int target = index + direction;
        if (index < 0 || target < 0 || target >= paths.Count)
        {
            return;
        }

        (paths[index], paths[target]) = (paths[target], paths[index]);
        _selectedPath = repository.Path;
        SaveLayout();
        RebuildItems();
    }

    private void MoveGroup(string groupKey, int direction)
    {
        if (GroupKeysEqual(groupKey, UnclassifiedGroupKey))
        {
            return;
        }

        int index = _layout.GroupOrder.FindIndex(group => string.Equals(group, groupKey, StringComparison.OrdinalIgnoreCase));
        int target = index + direction;
        if (index < 0 || target < 0 || target >= _layout.GroupOrder.Count
            || GroupKeysEqual(_layout.GroupOrder[target], UnclassifiedGroupKey))
        {
            return;
        }

        (_layout.GroupOrder[index], _layout.GroupOrder[target]) = (_layout.GroupOrder[target], _layout.GroupOrder[index]);
        SaveLayout();
        RebuildItems();
    }

    private void MoveGroupBefore(string source, string target)
    {
        if (GroupKeysEqual(source, target) || GroupKeysEqual(source, UnclassifiedGroupKey))
        {
            return;
        }

        _layout.GroupOrder.RemoveAll(group => string.Equals(group, source, StringComparison.OrdinalIgnoreCase));
        int targetIndex = _layout.GroupOrder.FindIndex(group => string.Equals(group, target, StringComparison.OrdinalIgnoreCase));
        _layout.GroupOrder.Insert(Math.Max(0, targetIndex), source);
        SaveLayout();
        RebuildItems();
    }

    private void SetGroupCollapsedState(string groupKey, bool collapsed)
    {
        ListViewGroup? group = _list.Groups.Cast<ListViewGroup>()
            .FirstOrDefault(candidate => candidate.Name is not null && GroupKeysEqual(candidate.Name, groupKey));
        if (group is null)
        {
            return;
        }

        group.CollapsedState = collapsed ? ListViewGroupCollapsedState.Collapsed : ListViewGroupCollapsedState.Expanded;
        if (collapsed)
        {
            _layout.CollapsedGroups.Add(groupKey);
            if (_selectedPath is not null
                && _repositories.FirstOrDefault(repository => PathsEqual(repository.Path, _selectedPath)) is { } selected
                && GroupKeysEqual(GetGroupKey(selected), groupKey))
            {
                _list.SelectedItems.Clear();
            }
        }
        else
        {
            _layout.CollapsedGroups.Remove(groupKey);
        }

        SaveLayout();
    }

    private string? FindGroupHeaderAt(Point point)
    {
        foreach (ListViewGroup group in _list.Groups)
        {
            if (GetGroupHeaderBounds(group).Contains(point))
            {
                return group.Name;
            }
        }

        return null;
    }

    private Rectangle GetGroupHeaderBounds(ListViewGroup group)
    {
        if (ListViewGroupAccessibilityObjectProperty?.GetValue(group) is not AccessibleObject accessibilityObject)
        {
            return Rectangle.Empty;
        }

        Rectangle groupBounds = _list.RectangleToClient(accessibilityObject.Bounds);
        int firstItemTop = group.Items.Cast<ListViewItem>()
            .Where(item => item.Bounds != Rectangle.Empty)
            .Select(item => item.Bounds.Top)
            .DefaultIfEmpty(groupBounds.Top + DpiUtil.Scale(24))
            .Min();
        return new Rectangle(groupBounds.Left, groupBounds.Top, groupBounds.Width, Math.Max(DpiUtil.Scale(16), firstItemTop - groupBounds.Top));
    }

    private string? FindGroupAt(Point point)
    {
        string? header = FindGroupHeaderAt(point);
        if (header is not null)
        {
            return header;
        }

        return _list.GetItemAt(point.X, point.Y)?.Group?.Name;
    }

    private void SetHoveredItem(ListViewItem? item)
    {
        if (_hoveredItem == item)
        {
            return;
        }

        ListViewItem? previous = _hoveredItem;
        _hoveredItem = item;
        previous?.ListView?.Invalidate(previous.Bounds);
        item?.ListView?.Invalidate(item.Bounds);
    }

    private void ClearItemDropTarget()
    {
        _dropTargetPath = null;
        _list.Invalidate();
    }

    private void UpdateTileSize()
    {
        int padding = DpiUtil.Scale(12);
        int available = Math.Max(DpiUtil.Scale(280), _list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - padding);
        int minimumWidth = DpiUtil.Scale(440);
        int columns = Math.Clamp(available / minimumWidth, 1, 3);
        int width = Math.Max(DpiUtil.Scale(280), (available - (padding * Math.Max(0, columns - 1))) / columns);
        _list.TileSize = new Size(width, DpiUtil.Scale(118));
    }

    private void SaveLayout()
        => _layoutCache.Save(_layout);

    private static string GetGroupKey(Repository repository)
        => string.IsNullOrWhiteSpace(repository.Category) ? UnclassifiedGroupKey : repository.Category.Trim();

    private static string GetGroupDisplayName(string groupKey)
        => GroupKeysEqual(groupKey, UnclassifiedGroupKey) ? "未分类" : groupKey;

    private static bool GroupKeysEqual(string left, string right)
        => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static string GetRepositoryName(Repository repository)
    {
        string path = repository.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.GetFileName(path) is { Length: > 0 } name ? name : path;
    }

    private static string GetShortPath(string path)
        => Path.GetDirectoryName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? path;

    private static Image GetWorkingTreeStateImage(MultiRepositoryStatus? status)
    {
        if (status is null)
        {
            return RepoStateVisualiser.Unknown.Item1;
        }

        if (!status.HasWorkingTreeChanges)
        {
            return RepoStateVisualiser.Clean.Item1;
        }

        if (status.StagedCount != 0 && (status.ModifiedCount != 0 || status.UntrackedCount != 0))
        {
            return RepoStateVisualiser.Mixed.Item1;
        }

        if (status.StagedCount != 0)
        {
            return RepoStateVisualiser.Staged.Item1;
        }

        if (status.UntrackedCount != 0 && status.ModifiedCount == 0)
        {
            return RepoStateVisualiser.UntrackedOnly.Item1;
        }

        return RepoStateVisualiser.Dirty.Item1;
    }

    private static Color GetLabelColor(MultiRepositorySyncLabelKind kind)
        => kind switch
        {
            MultiRepositorySyncLabelKind.Synchronized => Color.ForestGreen.AdaptBackColor(),
            MultiRepositorySyncLabelKind.Ahead => Color.DodgerBlue.AdaptBackColor(),
            MultiRepositorySyncLabelKind.Behind => Color.DarkOrange.AdaptBackColor(),
            MultiRepositorySyncLabelKind.Diverged => Color.MediumPurple.AdaptBackColor(),
            MultiRepositorySyncLabelKind.Error => Color.Firebrick.AdaptBackColor(),
            _ => SystemColors.GrayText
        };

    private static string BuildToolTip(Repository repository, MultiRepositoryStatus? status)
    {
        string synchronization = string.Join(" · ", MultiRepositoryStatusPresentation.GetSynchronizationLabels(status).Select(label => label.Text));
        List<string> lines =
        [
            repository.Path,
            $"分支：{(string.IsNullOrWhiteSpace(status?.Branch) ? "无分支" : status.Branch)}",
            $"工作区：{MultiRepositoryStatusPresentation.FormatWorkingTree(status)}",
            $"同步：{synchronization}",
            $"上次 Fetch：{MultiRepositoryStatusPresentation.FormatFetchTimestamp(status?.LastFetchUtc, DateTimeOffset.UtcNow)}",
            $"检查时间：{MultiRepositoryStatusPresentation.FormatCheckedTimestamp(status?.LastCheckedUtc)}"
        ];
        if (!string.IsNullOrWhiteSpace(status?.Error))
        {
            lines.Add(status.FetchError is not null ? $"Fetch 失败：{status.FetchError}" : $"检查失败：{status.StatusError}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static bool PathsEqual(string left, string right)
        => string.Equals(left.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                         right.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                         StringComparison.OrdinalIgnoreCase);
}
