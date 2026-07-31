using GitCommands.Settings;

namespace GitUI.CommandsDialogs;

// Form for configuring toolbars layout with a visual 2D grid.
// Allows users to drag and drop toolbars to reposition them across rows.
public partial class FormToolbarsLayout : Form
{
    private readonly FormBrowse _formBrowse;
    private readonly Dictionary<string, ToolStrip> _dynamicToolbars;
    private readonly List<ToolbarLayoutItem> _layoutItems = new();
    private readonly List<RowPanel> _rowPanels = new();

    private const int RowHeight = 50;
    private const int RowMargin = 8;
    private const int ToolbarItemWidth = 220;
    private const int ToolbarItemHeight = 36;
    private const int ToolbarItemMargin = 6;

    private static readonly int[] IconSizeOptions = [16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72];

    [System.Diagnostics.Conditional("DEBUG")]
    private static void LogToolbar(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);

        try
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitExtensions", "toolbar_debug.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Ignore file write errors
        }
    }

    // Drag and drop state
    private ToolbarItemPanel? _draggedItem;
    private Point _dragStartPoint;
    private bool _isDragging;
    private Panel? _dropIndicator;

    // Selection state
    private ToolbarItemPanel? _selectedItem;

    // Snapshot of the layout state at form open / after the last Apply. Used by OnFormClosing
    // to detect unapplied edits when the user closes via the X button / Alt+F4.
    private LayoutSnapshot _appliedSnapshot;

    public FormToolbarsLayout(FormBrowse formBrowse, Dictionary<string, ToolStrip> dynamicToolbars)
    {
        _formBrowse = formBrowse;
        _dynamicToolbars = dynamicToolbars;

        InitializeComponent();
        InitializeToolTips();
        LoadCurrentLayout();
        BuildVisualGrid();

        checkBoxSyncIconText.Checked = GitCommands.AppSettings.ToolbarSyncIconTextWithSize;
        toolTip.SetToolTip(checkBoxSyncIconText, "When checked, icon text font size scales with the icon size (baseline: 16 px icons = default font size)");

        // Force the toolbar grid and the OK/Cancel/Apply panel to keep stable margins
        // regardless of how the user resizes the form. Designer Anchor settings are supposed
        // to handle this, but AutoScale + runtime resize sequencing sometimes leave them with
        // stale offsets (grid stays narrow, button panel drifts away from the right edge).
        Resize += (_, _) => SyncRightAnchoredControls();
        Shown += (_, _) => SyncRightAnchoredControls();

        // Baseline the snapshot once everything is loaded and the checkbox reflects the
        // saved AppSettings value, so HasUnsavedChanges() compares against the persisted state.
        _appliedSnapshot = CaptureSnapshot();
    }

    private void SyncRightAnchoredControls()
    {
        // 12px is the original (and intended) margin both left and right of the grid.
        const int rightMargin = 12;

        int targetGridWidth = ClientSize.Width - (2 * panelToolbarGrid.Location.X);
        if (targetGridWidth > 0 && panelToolbarGrid.Width != targetGridWidth)
        {
            panelToolbarGrid.Width = targetGridWidth;
        }

        // Pin the button panel's right edge to (ClientSize.Width - 12), matching the grid.
        int targetLeft = ClientSize.Width - rightMargin - flowLayoutPanelButtons.Width;
        if (targetLeft >= 0 && flowLayoutPanelButtons.Left != targetLeft)
        {
            flowLayoutPanelButtons.Left = targetLeft;
        }
    }

    // Represents a toolbar in the layout grid
    private sealed class ToolbarLayoutItem
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Row { get; set; }
        public int OrderInRow { get; set; }
        public bool IsBuiltIn { get; set; }
        public bool IsVisible { get; set; } = true;
        public int IconSize { get; set; } = 16;
    }

    // Visual panel representing a row of toolbars
    private sealed class RowPanel : Panel
    {
        public int RowIndex { get; set; }
        public Label RowLabel { get; }

        public RowPanel(int rowIndex)
        {
            RowIndex = rowIndex;
            Height = RowHeight;
            BackColor = SystemColors.Control;
            BorderStyle = BorderStyle.FixedSingle;
            Margin = new Padding(0, 0, 0, RowMargin);
            Padding = new Padding(50, 5, 5, 5);

            RowLabel = new Label
            {
                Text = $"Row {rowIndex + 1}",
                AutoSize = false,
                Width = 45,
                Height = RowHeight - 12,
                Location = new Point(3, 6),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SystemColors.GrayText,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 8f)
            };
            Controls.Add(RowLabel);
        }

        public void UpdateRowIndex(int newIndex)
        {
            RowIndex = newIndex;
            RowLabel.Text = $"Row {newIndex + 1}";
        }
    }

    // Visual panel representing a toolbar item that can be dragged
    private sealed class ToolbarItemPanel : Panel
    {
        public ToolbarLayoutItem LayoutItem { get; }
        private readonly Label _label;
        private readonly CheckBox _checkBox;
        private readonly ComboBox _iconSizeCombo;
        private bool _isSelected;

        // Layout constants for child controls
        private const int LabelLeft = 40;   // grip(16) + checkbox offset(4) + checkbox(16) + gap(4)
        private const int ComboWidth = 90;
        private const int ComboMargin = 4;

        // Highlight color used for both drag and selection
        private static readonly Color HighlightColor = Color.FromArgb(255, 255, 180);

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateBackColor();
            }
        }

        public ToolbarItemPanel(ToolbarLayoutItem item)
        {
            LayoutItem = item;
            Width = ToolbarItemWidth;
            Height = ToolbarItemHeight;
            BackColor = Color.FromArgb(220, 235, 252);
            BorderStyle = BorderStyle.FixedSingle;
            Cursor = Cursors.SizeAll;
            Margin = new Padding(ToolbarItemMargin);

            // Grip indicator
            Label gripLabel = new()
            {
                Text = "⋮⋮",
                AutoSize = false,
                Width = 16,
                Height = ToolbarItemHeight - 4,
                Location = new Point(2, 2),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SystemColors.GrayText,
                Cursor = Cursors.SizeAll
            };
            Controls.Add(gripLabel);

            // Visibility checkbox — does not propagate mouse events so drag is not triggered
            _checkBox = new CheckBox
            {
                AutoSize = false,
                Width = 16,
                Height = 16,
                Location = new Point(20, (ToolbarItemHeight - 16) / 2),
                Checked = item.IsVisible,
                Cursor = Cursors.Default
            };
            _checkBox.CheckedChanged += (s, e) =>
            {
                LayoutItem.IsVisible = _checkBox.Checked;
                UpdateLabelStyle();
            };
            Controls.Add(_checkBox);

            // Icon size dropdown — anchored to the right side of the panel
            _iconSizeCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = ComboWidth,
                Location = new Point(ToolbarItemWidth - ComboWidth - ComboMargin, (ToolbarItemHeight - 21) / 2),
                Cursor = Cursors.Default
            };
            foreach (int size in IconSizeOptions)
            {
                _iconSizeCombo.Items.Add($"Icon size: {size}");
            }

            int selectedIndex = Array.IndexOf(IconSizeOptions, item.IconSize);
            _iconSizeCombo.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
            _iconSizeCombo.SelectedIndexChanged += (s, e) =>
            {
                LayoutItem.IconSize = IconSizeOptions[_iconSizeCombo.SelectedIndex];
            };
            Controls.Add(_iconSizeCombo);

            // Toolbar name — fills the space between checkbox and dropdown
            _label = new Label
            {
                AutoSize = false,
                Width = ToolbarItemWidth - LabelLeft - ComboWidth - ComboMargin,
                Height = ToolbarItemHeight - 4,
                Location = new Point(LabelLeft, 2),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.SizeAll
            };
            Controls.Add(_label);

            UpdateLabelStyle();

            // Propagate mouse events from grip and label to the panel (enables drag)
            _label.MouseDown += (s, e) => OnMouseDown(e);
            _label.MouseMove += (s, e) => OnMouseMove(e);
            _label.MouseUp += (s, e) => OnMouseUp(e);
            gripLabel.MouseDown += (s, e) => OnMouseDown(e);
            gripLabel.MouseMove += (s, e) => OnMouseMove(e);
            gripLabel.MouseUp += (s, e) => OnMouseUp(e);
        }

        public void SetHighlight(bool highlight)
        {
            if (highlight)
            {
                BackColor = HighlightColor;
            }
            else
            {
                UpdateBackColor();
            }
        }

        private void UpdateLabelStyle()
        {
            if (!LayoutItem.IsVisible)
            {
                _label.ForeColor = SystemColors.GrayText;
                _label.Text = $"({LayoutItem.DisplayName})";
            }
            else
            {
                _label.ForeColor = SystemColors.ControlText;
                _label.Text = LayoutItem.DisplayName;
            }

            UpdateBackColor();
        }

        private void UpdateBackColor()
        {
            if (_isSelected)
            {
                BackColor = HighlightColor;
            }
            else if (!LayoutItem.IsVisible)
            {
                BackColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                BackColor = Color.FromArgb(220, 235, 252);
            }
        }
    }

    private void InitializeToolTips()
    {
        toolTip.SetToolTip(buttonAddRow, "Add a new empty row at the bottom");
        toolTip.SetToolTip(buttonRemoveRow, "Remove the last empty row");
        toolTip.SetToolTip(buttonReset, "Reset layout to default (built-in toolbars on row 1; each custom toolbar on its own row and disabled)");
        toolTip.SetToolTip(buttonLocate, "Highlight the selected toolbar in the main window (or all toolbars if none selected)");
        toolTip.SetToolTip(buttonOK, "Apply changes and close");
        toolTip.SetToolTip(buttonCancel, "Cancel and close without saving");
        toolTip.SetToolTip(buttonApply, "Apply changes without closing");
        toolTip.SetToolTip(panelToolbarGrid, "Drag toolbars to reposition them");
    }

    private void LoadCurrentLayout()
    {
        _layoutItems.Clear();

        ToolbarLayoutConfig? config = GitCommands.AppSettings.ToolbarLayout;

        // Build a map of actual visual positions from the TopToolStripPanel.
        // ToolStrip.Location.Y inside a ToolStripPanel corresponds to the row index
        // (toolbars on the same physical row share the same Y). We convert Y values
        // to contiguous row indices (0, 1, 2...) sorted ascending.
        // For visible toolbars this is the source of truth: the config Row/OrderInRow
        // may be stale if the panel wrapped toolbars automatically (all stored as Row=0
        // in config while WinForms visually placed them on separate rows).
        // Hidden toolbars are deliberately excluded from this map: they carry no
        // meaningful position. ReorganizeToolbars re-adds them to the panel only to keep
        // them discoverable, which parks them at the top-left (row 0). Including them here
        // would make them resurface on row 0 — and shift the order of the visible toolbars
        // sharing that row. Their row/order is read from the config instead, so a hidden
        // toolbar keeps the row it was assigned.
        ToolStripPanel panel = _formBrowse.TopToolStripPanel;
        List<ToolStrip> allStripsInPanel = panel.Controls.OfType<ToolStrip>().ToList();
        List<ToolStrip> visibleStripsInPanel = allStripsInPanel.Where(ts => ts.Visible).ToList();

        // Map Y coordinate → row index (visible toolbars only)
        List<int> distinctYs = visibleStripsInPanel
            .Select(ts => ts.Location.Y)
            .Distinct()
            .OrderBy(y => y)
            .ToList();

        int VisualRow(ToolStrip ts) => distinctYs.IndexOf(ts.Location.Y);
        int VisualOrderInRow(ToolStrip ts) => visibleStripsInPanel
            .Where(s => s.Location.Y == ts.Location.Y)
            .OrderBy(s => s.Location.X)
            .ToList()
            .IndexOf(ts);

        LoadBuiltInToolbars(config, allStripsInPanel, VisualRow, VisualOrderInRow);
        LoadConfiguredCustomToolbars(config, allStripsInPanel, VisualRow, VisualOrderInRow);
        LoadUnconfiguredCustomToolbars(allStripsInPanel, VisualRow, VisualOrderInRow);

        _layoutItems.Sort((a, b) =>
        {
            int rowCompare = a.Row.CompareTo(b.Row);
            return rowCompare != 0 ? rowCompare : a.OrderInRow.CompareTo(b.OrderInRow);
        });
    }

    private void LoadBuiltInToolbars(
        ToolbarLayoutConfig? config,
        List<ToolStrip> allStripsInPanel,
        Func<ToolStrip, int> visualRow,
        Func<ToolStrip, int> visualOrderInRow)
    {
        (string Name, ToolStrip ToolStrip)[] builtInToolbars =
        {
            ("Standard", _formBrowse.ToolStripMain),
            ("Filters", _formBrowse.ToolStripFilters),
            ("Scripts", _formBrowse.ToolStripScripts)
        };

        foreach ((string name, ToolStrip toolStrip) in builtInToolbars)
        {
            bool useVisualPosition = toolStrip.Visible && allStripsInPanel.Contains(toolStrip);
            int row = useVisualPosition
                ? visualRow(toolStrip)
                : (config?.ToolbarsVisibility?.FirstOrDefault(t => t.Name == name)?.Row ?? 0);
            int orderInRow = useVisualPosition
                ? visualOrderInRow(toolStrip)
                : (config?.ToolbarsVisibility?.FirstOrDefault(t => t.Name == name)?.OrderInRow ?? GetDefaultOrder(name));

            LogToolbar($"[FormToolbarsLayout.LoadCurrentLayout] Toolbar '{name}': Row={row}, OrderInRow={orderInRow}, Visible={toolStrip.Visible}");

            int configIconSize = config?.ToolbarsVisibility?.FirstOrDefault(t => t.Name == name)?.IconSize ?? 16;
            int fallbackIconSize = configIconSize > 0 ? configIconSize : 16;
            int builtInIconSize = allStripsInPanel.Contains(toolStrip) && toolStrip.ImageScalingSize.Width > 0
                ? toolStrip.ImageScalingSize.Width
                : fallbackIconSize;

            _layoutItems.Add(new ToolbarLayoutItem
            {
                Name = name,
                DisplayName = name,
                Row = row,
                OrderInRow = orderInRow,
                IsBuiltIn = true,
                IsVisible = toolStrip.Visible,
                IconSize = builtInIconSize
            });
        }
    }

    private void LoadConfiguredCustomToolbars(
        ToolbarLayoutConfig? config,
        List<ToolStrip> allStripsInPanel,
        Func<ToolStrip, int> visualRow,
        Func<ToolStrip, int> visualOrderInRow)
    {
        if (config?.CustomToolbars is null)
        {
            return;
        }

        foreach (CustomToolbarMetadata customMeta in config.CustomToolbars)
        {
            ToolStrip? actualToolStrip = _dynamicToolbars.Values.FirstOrDefault(ts => ts.Text == customMeta.Name);
            bool visible = actualToolStrip?.Visible ?? customMeta.Visible;

            bool useVisualPosition = actualToolStrip != null && actualToolStrip.Visible && allStripsInPanel.Contains(actualToolStrip);
            int row = useVisualPosition ? visualRow(actualToolStrip!) : customMeta.Row;
            int orderInRow = useVisualPosition ? visualOrderInRow(actualToolStrip!) : customMeta.OrderInRow;

            LogToolbar($"[FormToolbarsLayout.LoadCurrentLayout] Custom '{customMeta.Name}': Row={row}, OrderInRow={orderInRow}, Visible={visible}");

            int customFallbackIconSize = customMeta.IconSize > 0 ? customMeta.IconSize : 16;
            int customIconSize = actualToolStrip != null && allStripsInPanel.Contains(actualToolStrip) && actualToolStrip.ImageScalingSize.Width > 0
                ? actualToolStrip.ImageScalingSize.Width
                : customFallbackIconSize;

            _layoutItems.Add(new ToolbarLayoutItem
            {
                Name = customMeta.Name,
                DisplayName = customMeta.Name,
                Row = row,
                OrderInRow = orderInRow,
                IsBuiltIn = false,
                IsVisible = visible,
                IconSize = customIconSize
            });
        }
    }

    private void LoadUnconfiguredCustomToolbars(
        List<ToolStrip> allStripsInPanel,
        Func<ToolStrip, int> visualRow,
        Func<ToolStrip, int> visualOrderInRow)
    {
        foreach (ToolStrip ts in _dynamicToolbars.Values
            .Where(ts => !string.IsNullOrEmpty(ts.Text) && _layoutItems.All(i => i.Name != ts.Text)))
        {
            string toolbarName = ts.Text;
            bool useVisualPosition = ts.Visible && allStripsInPanel.Contains(ts);
            int row = useVisualPosition ? visualRow(ts) : 0;
            int orderInRow = useVisualPosition ? visualOrderInRow(ts) : _layoutItems.Count;

            int unconfiguredIconSize = ts.ImageScalingSize.Width > 0 ? ts.ImageScalingSize.Width : 16;

            _layoutItems.Add(new ToolbarLayoutItem
            {
                Name = toolbarName,
                DisplayName = toolbarName,
                Row = row,
                OrderInRow = orderInRow,
                IsBuiltIn = false,
                IsVisible = ts.Visible,
                IconSize = unconfiguredIconSize
            });
        }
    }

    private static int GetDefaultOrder(string toolbarName)
    {
        return toolbarName switch
        {
            "Standard" => 0,
            "Filters" => 1,
            "Scripts" => 2,
            _ => 99
        };
    }

    private void BuildVisualGrid()
    {
        // Dispose() removes the control from its parent's Controls collection synchronously,
        // which would invalidate the foreach enumerator if iterating the collection directly.
        foreach (Control c in panelToolbarGrid.Controls.Cast<Control>().ToArray())
        {
            c.Dispose();
        }

        panelToolbarGrid.Controls.Clear();
        _rowPanels.Clear();
        _selectedItem = null;

        // Determine max row
        int maxRow = _layoutItems.Any() ? _layoutItems.Max(i => i.Row) : 0;

        // Create row panels
        int yOffset = 5;
        for (int r = 0; r <= maxRow; r++)
        {
            RowPanel rowPanel = new(r)
            {
                Location = new Point(5, yOffset),
                Width = panelToolbarGrid.ClientSize.Width - 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Enable as drop target
            rowPanel.AllowDrop = true;
            rowPanel.DragEnter += RowPanel_DragEnter;
            rowPanel.DragOver += RowPanel_DragOver;
            rowPanel.DragLeave += RowPanel_DragLeave;
            rowPanel.DragDrop += RowPanel_DragDrop;

            _rowPanels.Add(rowPanel);
            panelToolbarGrid.Controls.Add(rowPanel);

            // Add toolbar items to this row
            var itemsInRow = _layoutItems
                .Where(i => i.Row == r)
                .OrderBy(i => i.OrderInRow)
                .ToList();

            int xOffset = 55; // After row label
            foreach (ToolbarLayoutItem item in itemsInRow)
            {
                ToolbarItemPanel itemPanel = new(item)
                {
                    Location = new Point(xOffset, 7)
                };

                // Enable drag
                itemPanel.MouseDown += ItemPanel_MouseDown;
                itemPanel.MouseMove += ItemPanel_MouseMove;
                itemPanel.MouseUp += ItemPanel_MouseUp;

                rowPanel.Controls.Add(itemPanel);
                xOffset += ToolbarItemWidth + ToolbarItemMargin;
            }

            yOffset += RowHeight + RowMargin;
        }

        // Dispose the previous indicator before creating a new one (BuildVisualGrid clears
        // panelToolbarGrid.Controls first, which disposes it, but the field still holds the
        // reference until we reassign it).
        _dropIndicator?.Dispose();
        _dropIndicator = new Panel
        {
            BackColor = Color.FromArgb(0, 120, 215),
            Height = 3,
            Width = 100,
            Visible = false
        };
        panelToolbarGrid.Controls.Add(_dropIndicator);

        // Enable drop on the grid itself so the user can drop between rows to create a new row.
        panelToolbarGrid.AllowDrop = true;
        panelToolbarGrid.DragEnter -= Grid_DragEnter;
        panelToolbarGrid.DragOver -= Grid_DragOver;
        panelToolbarGrid.DragLeave -= Grid_DragLeave;
        panelToolbarGrid.DragDrop -= Grid_DragDrop;
        panelToolbarGrid.DragEnter += Grid_DragEnter;
        panelToolbarGrid.DragOver += Grid_DragOver;
        panelToolbarGrid.DragLeave += Grid_DragLeave;
        panelToolbarGrid.DragDrop += Grid_DragDrop;

        UpdateRemoveRowButton();
    }

    #region Drag and Drop

    private void ItemPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        if (sender is ToolbarItemPanel itemPanel && e.Button == MouseButtons.Left)
        {
            _draggedItem = itemPanel;
            _dragStartPoint = e.Location;
            _isDragging = false;
        }
    }

    private void ItemPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        // Check if moved enough to start drag
        if (_draggedItem != null && e.Button == MouseButtons.Left
            && !_isDragging
            && (Math.Abs(e.X - _dragStartPoint.X) > 5 || Math.Abs(e.Y - _dragStartPoint.Y) > 5))
        {
            _isDragging = true;
            _draggedItem.SetHighlight(true);
            _draggedItem.DoDragDrop(_draggedItem, DragDropEffects.Move);

            // DoDragDrop is blocking. Reset state unconditionally here in case
            // MouseUp was not fired on this panel (e.g. mouse released outside the form).
            if (_draggedItem != null)
            {
                _draggedItem.SetHighlight(false);
                _draggedItem = null;
            }

            _isDragging = false;
            HideDropIndicator();
        }
    }

    private void ItemPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_draggedItem != null)
        {
            // If not dragging, this is a simple click -> select the item
            if (!_isDragging && sender is ToolbarItemPanel clickedItem)
            {
                SelectToolbarItem(clickedItem);
            }

            _draggedItem.SetHighlight(false);
            _draggedItem = null;
            _isDragging = false;
            HideDropIndicator();
        }
    }

    private void SelectToolbarItem(ToolbarItemPanel itemToSelect)
    {
        ArgumentNullException.ThrowIfNull(itemToSelect);

        // Deselect previously selected item
        if (_selectedItem != null && _selectedItem != itemToSelect)
        {
            _selectedItem.IsSelected = false;
        }

        // Toggle selection if clicking on the same item, otherwise select the new item
        if (_selectedItem == itemToSelect)
        {
            itemToSelect.IsSelected = !itemToSelect.IsSelected;
            if (!itemToSelect.IsSelected)
            {
                _selectedItem = null;
            }
        }
        else
        {
            _selectedItem = itemToSelect;
            _selectedItem.IsSelected = true;
        }
    }

    private static void RowPanel_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data is not null && e.Data.GetDataPresent(typeof(ToolbarItemPanel)))
        {
            e.Effect = DragDropEffects.Move;
        }
    }

    private void RowPanel_DragOver(object? sender, DragEventArgs e)
    {
        if (sender is not RowPanel rowPanel || _draggedItem == null)
        {
            return;
        }

        e.Effect = DragDropEffects.Move;

        // Convert screen coordinates to row panel coordinates
        Point clientPoint = rowPanel.PointToClient(new Point(e.X, e.Y));

        // Find drop position
        int dropIndex = GetDropIndex(rowPanel, clientPoint.X);

        // Show drop indicator
        ShowDropIndicator(rowPanel, dropIndex);
    }

    private void RowPanel_DragLeave(object? sender, EventArgs e)
    {
        HideDropIndicator();
    }

    private void RowPanel_DragDrop(object? sender, DragEventArgs e)
    {
        if (sender is not RowPanel targetRow ||
            e.Data?.GetData(typeof(ToolbarItemPanel)) is not ToolbarItemPanel droppedPanel)
        {
            HideDropIndicator();
            return;
        }

        // Get drop position
        Point clientPoint = targetRow.PointToClient(new Point(e.X, e.Y));
        int dropIndex = GetDropIndex(targetRow, clientPoint.X);

        // Update layout item
        ToolbarLayoutItem item = droppedPanel.LayoutItem;
        int oldRow = item.Row;

        item.Row = targetRow.RowIndex;

        // Recalculate order for all items in target row
        var itemsInTargetRow = _layoutItems
            .Where(i => i.Row == targetRow.RowIndex && i != item)
            .OrderBy(i => i.OrderInRow)
            .ToList();

        // Insert at drop position
        itemsInTargetRow.Insert(Math.Min(dropIndex, itemsInTargetRow.Count), item);

        // Update order values
        for (int i = 0; i < itemsInTargetRow.Count; i++)
        {
            itemsInTargetRow[i].OrderInRow = i;
        }

        // If we moved from a different row, reorder that row too
        if (oldRow != targetRow.RowIndex)
        {
            var itemsInOldRow = _layoutItems
                .Where(i => i.Row == oldRow)
                .OrderBy(i => i.OrderInRow)
                .ToList();

            for (int i = 0; i < itemsInOldRow.Count; i++)
            {
                itemsInOldRow[i].OrderInRow = i;
            }
        }

        droppedPanel.SetHighlight(false);
        _draggedItem = null;
        _isDragging = false;
        HideDropIndicator();

        // Remove empty rows and rebuild
        RemoveEmptyRows();
        BuildVisualGrid();
    }

    // ----- Drop between rows (creates a new row at the chosen position) -----

    // Pixel tolerance around row separators where dropping creates a new row instead of joining.
    private const int RowSeparatorTolerance = 6;

    // Returns the insertion index for a "between rows" drop, or null when the cursor is over a row.
    // Index 0 = above the first row, _rowPanels.Count = below the last row, i = between row[i-1] and row[i].
    private int? GetRowInsertionIndex(int gridY)
    {
        if (_rowPanels.Count == 0)
        {
            return 0;
        }

        for (int i = 0; i < _rowPanels.Count; i++)
        {
            RowPanel rp = _rowPanels[i];
            int top = rp.Top;
            int bottom = rp.Bottom;

            // Above the first row
            if (i == 0 && gridY < top + RowSeparatorTolerance)
            {
                return 0;
            }

            // In the gap between this row and the next
            if (i < _rowPanels.Count - 1)
            {
                int nextTop = _rowPanels[i + 1].Top;
                if (gridY >= bottom - RowSeparatorTolerance && gridY <= nextTop + RowSeparatorTolerance)
                {
                    return i + 1;
                }
            }

            // Below the last row
            if (i == _rowPanels.Count - 1 && gridY > bottom - RowSeparatorTolerance)
            {
                return _rowPanels.Count;
            }
        }

        return null;
    }

    private static void Grid_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(typeof(ToolbarItemPanel)) is ToolbarItemPanel)
        {
            e.Effect = DragDropEffects.Move;
        }
    }

    private void Grid_DragOver(object? sender, DragEventArgs e)
    {
        if (_draggedItem == null)
        {
            return;
        }

        Point gridPoint = panelToolbarGrid.PointToClient(new Point(e.X, e.Y));
        int? insertionIndex = GetRowInsertionIndex(gridPoint.Y);

        if (insertionIndex.HasValue)
        {
            e.Effect = DragDropEffects.Move;
            ShowRowSeparatorIndicator(insertionIndex.Value);
        }
        else
        {
            // Cursor is over a row; let RowPanel_DragOver handle the in-row indicator.
            e.Effect = DragDropEffects.Move;
        }
    }

    private void Grid_DragLeave(object? sender, EventArgs e)
    {
        HideDropIndicator();
    }

    private void Grid_DragDrop(object? sender, DragEventArgs e)
    {
        if (_draggedItem == null ||
            e.Data?.GetData(typeof(ToolbarItemPanel)) is not ToolbarItemPanel droppedPanel)
        {
            HideDropIndicator();
            return;
        }

        Point gridPoint = panelToolbarGrid.PointToClient(new Point(e.X, e.Y));
        int? insertionIndex = GetRowInsertionIndex(gridPoint.Y);

        if (!insertionIndex.HasValue)
        {
            // Not in a between-rows zone; let RowPanel_DragDrop handle it.
            return;
        }

        ToolbarLayoutItem item = droppedPanel.LayoutItem;
        int oldRow = item.Row;
        int newRow = insertionIndex.Value;

        // Shift down all rows at or after the insertion index to make room for the new row.
        // If the dragged item came from a row that gets shifted, we must apply the shift to its
        // own old row index too (so we know which row to reorder afterwards).
        foreach (ToolbarLayoutItem li in _layoutItems)
        {
            if (li == item)
            {
                continue;
            }

            if (li.Row >= newRow)
            {
                li.Row++;
            }
        }

        // Account for the shift on the dragged item's source row index.
        int adjustedOldRow = oldRow >= newRow ? oldRow + 1 : oldRow;

        item.Row = newRow;
        item.OrderInRow = 0;

        // Reorder the items remaining in the source row (their OrderInRow may now have gaps).
        if (adjustedOldRow != newRow)
        {
            var itemsInOldRow = _layoutItems
                .Where(i => i.Row == adjustedOldRow && i != item)
                .OrderBy(i => i.OrderInRow)
                .ToList();
            for (int i = 0; i < itemsInOldRow.Count; i++)
            {
                itemsInOldRow[i].OrderInRow = i;
            }
        }

        droppedPanel.SetHighlight(false);
        _draggedItem = null;
        _isDragging = false;
        HideDropIndicator();

        RemoveEmptyRows();
        BuildVisualGrid();
    }

    private void ShowRowSeparatorIndicator(int insertionIndex)
    {
        if (_dropIndicator == null)
        {
            return;
        }

        int y;
        if (_rowPanels.Count == 0)
        {
            y = 5;
        }
        else if (insertionIndex == 0)
        {
            y = _rowPanels[0].Top - 4;
        }
        else if (insertionIndex >= _rowPanels.Count)
        {
            y = _rowPanels[^1].Bottom + 1;
        }
        else
        {
            int prevBottom = _rowPanels[insertionIndex - 1].Bottom;
            int nextTop = _rowPanels[insertionIndex].Top;
            y = ((prevBottom + nextTop) / 2) - 1;
        }

        int width = panelToolbarGrid.ClientSize.Width - 30;
        _dropIndicator.Location = new Point(5, y);
        _dropIndicator.Width = width;
        _dropIndicator.Height = 3;
        _dropIndicator.Visible = true;
        _dropIndicator.BringToFront();
    }

    private static int GetDropIndex(RowPanel rowPanel, int clientX)
    {
        var itemPanels = rowPanel.Controls.OfType<ToolbarItemPanel>()
            .OrderBy(p => p.Location.X)
            .ToList();

        if (itemPanels.Count == 0)
        {
            return 0;
        }

        for (int i = 0; i < itemPanels.Count; i++)
        {
            int itemCenterX = itemPanels[i].Location.X + (itemPanels[i].Width / 2);
            if (clientX < itemCenterX)
            {
                return i;
            }
        }

        return itemPanels.Count;
    }

    private void ShowDropIndicator(RowPanel rowPanel, int dropIndex)
    {
        if (_dropIndicator == null)
        {
            return;
        }

        var itemPanels = rowPanel.Controls.OfType<ToolbarItemPanel>()
            .OrderBy(p => p.Location.X)
            .ToList();

        int xPos;
        if (itemPanels.Count == 0 || dropIndex == 0)
        {
            xPos = 55; // Start position
        }
        else if (dropIndex >= itemPanels.Count)
        {
            xPos = itemPanels.Last().Right + 3;
        }
        else
        {
            xPos = itemPanels[dropIndex].Location.X - 3;
        }

        // Convert to panelToolbarGrid coordinates
        Point gridLocation = panelToolbarGrid.PointToClient(rowPanel.PointToScreen(new Point(xPos, 5)));

        _dropIndicator.Location = gridLocation;
        _dropIndicator.Height = ToolbarItemHeight + 4;
        _dropIndicator.Width = 4;
        _dropIndicator.Visible = true;
        _dropIndicator.BringToFront();
    }

    private void HideDropIndicator()
    {
        if (_dropIndicator != null)
        {
            _dropIndicator.Visible = false;
        }
    }

    #endregion

    #region Button Handlers

    private void ButtonAddRow_Click(object? sender, EventArgs e)
    {
        int newRowIndex = _rowPanels.Count;

        // Add a new empty row
        int yPos = _rowPanels.Count > 0 ? _rowPanels[^1].Bottom + RowMargin : 5;
        RowPanel newRow = new(newRowIndex)
        {
            Location = new Point(5, yPos),
            Width = panelToolbarGrid.ClientSize.Width - 10,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        newRow.AllowDrop = true;
        newRow.DragEnter += RowPanel_DragEnter;
        newRow.DragOver += RowPanel_DragOver;
        newRow.DragLeave += RowPanel_DragLeave;
        newRow.DragDrop += RowPanel_DragDrop;

        _rowPanels.Add(newRow);
        panelToolbarGrid.Controls.Add(newRow);

        UpdateRemoveRowButton();
    }

    private void ButtonRemoveRow_Click(object? sender, EventArgs e)
    {
        // Remove last empty row
        if (_rowPanels.Count > 1)
        {
            RowPanel lastRow = _rowPanels[^1];

            // Check if empty (no toolbar items)
            bool hasItems = lastRow.Controls.OfType<ToolbarItemPanel>().Any();
            if (!hasItems)
            {
                panelToolbarGrid.Controls.Remove(lastRow);
                _rowPanels.RemoveAt(_rowPanels.Count - 1);
                lastRow.Dispose();
            }
        }

        UpdateRemoveRowButton();
    }

    private void ButtonReset_Click(object? sender, EventArgs e)
    {
        DialogResult result = MessageBoxes.Show(
            "Reset toolbars to their default layout?\n\nThe built-in toolbars (Standard, Filters, Scripts) will be placed on row 1 in that order. "
            + "Each custom toolbar will be moved to its own row and disabled.",
            "Reset Layout",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            // Built-in toolbars: all on row 0, in the default order (Standard, Filters, Scripts), visible.
            // Custom toolbars: each on its own row below, disabled. Keeping a custom toolbar on the same
            // row as the built-in ones makes the total width exceed the panel, which causes WinForms to
            // wrap the toolbars and leaves a large empty gap at the start of the first row.
            int builtInOrder = 0;
            int nextCustomRow = 1;
            foreach (ToolbarLayoutItem item in _layoutItems.OrderBy(i => GetDefaultOrder(i.Name)))
            {
                if (item.IsBuiltIn)
                {
                    item.Row = 0;
                    item.OrderInRow = builtInOrder++;
                    item.IsVisible = true;
                }
                else
                {
                    item.Row = nextCustomRow++;
                    item.OrderInRow = 0;
                    item.IsVisible = false;
                }
            }

            BuildVisualGrid();
        }
    }

    private void ButtonOK_Click(object? sender, EventArgs e)
    {
        ApplyLayout();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ButtonApply_Click(object? sender, EventArgs e)
    {
        ApplyLayout();
    }

    private void ButtonLocate_Click(object? sender, EventArgs e)
    {
        // Get the ToolStripPanel from FormBrowse
        Control? toolPanelContainer = _formBrowse.Controls.Cast<Control>()
            .FirstOrDefault(c => c is ToolStripContainer);

        if (toolPanelContainer is not ToolStripContainer)
        {
            MessageBoxes.Show(
                "Could not find the toolbar panel.",
                "Locate Toolbars",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        // If a toolbar is selected, only locate that one
        if (_selectedItem != null)
        {
            ToolStrip? selectedToolStrip = GetToolStripByName(_selectedItem.LayoutItem.Name);
            if (selectedToolStrip != null && selectedToolStrip.Visible)
            {
                _ = FlashToolbarsAsync(new List<ToolStrip> { selectedToolStrip });
            }
            else
            {
                MessageBoxes.Show(
                    $"The selected toolbar '{_selectedItem.LayoutItem.DisplayName}' is not visible in the main window.",
                    "Locate Toolbar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            return;
        }

        // No toolbar selected: locate all visible toolbars
        List<ToolStrip> allToolStrips = new();

        // Built-in toolbars
        if (_formBrowse.ToolStripMain.Visible)
        {
            allToolStrips.Add(_formBrowse.ToolStripMain);
        }

        if (_formBrowse.ToolStripFilters.Visible)
        {
            allToolStrips.Add(_formBrowse.ToolStripFilters);
        }

        if (_formBrowse.ToolStripScripts.Visible)
        {
            allToolStrips.Add(_formBrowse.ToolStripScripts);
        }

        // Custom toolbars
        foreach (KeyValuePair<string, ToolStrip> kvp in _dynamicToolbars
            .Where(kvp => kvp.Value.Visible && !allToolStrips.Contains(kvp.Value)))
        {
            allToolStrips.Add(kvp.Value);
        }

        if (allToolStrips.Count == 0)
        {
            MessageBoxes.Show(
                "No visible toolbars found.",
                "Locate Toolbars",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // Start the flashing animation for all toolbars (fire and forget)
        _ = FlashToolbarsAsync(allToolStrips);
    }

    private ToolStrip? GetToolStripByName(string name)
    {
        return name switch
        {
            "Standard" => _formBrowse.ToolStripMain,
            "Filters" => _formBrowse.ToolStripFilters,
            "Scripts" => _formBrowse.ToolStripScripts,
            _ => _dynamicToolbars.Values.FirstOrDefault(ts => ts.Text == name)
        };
    }

    private bool _isFlashing;

    private async Task FlashToolbarsAsync(List<ToolStrip> toolbars)
    {
        // Ignore re-entrant flashes: a second request while an animation is still running would
        // capture an already blended (reddish) BackColor as the "original" colour and restore
        // that tint permanently. Let the current animation finish first.
        if (_isFlashing)
        {
            return;
        }

        _isFlashing = true;

        Dictionary<ToolStrip, Color> originalColors = new();
        foreach (ToolStrip toolbar in toolbars)
        {
            originalColors[toolbar] = toolbar.BackColor;
        }

        const int flashDurationMs = 3000; // 3 seconds total
        const int stepDurationMs = 100;    // Update every 100ms
        int steps = flashDurationMs / stepDurationMs;

        try
        {
            // Animate from transparent to red and back using a sine wave for smooth fade in/out
            for (int i = 0; i < steps; i++)
            {
                double progress = (double)i / steps;
                double sineWave = Math.Sin(progress * Math.PI * 4); // 4 complete cycles in 3 seconds
                int alpha = (int)(Math.Abs(sineWave) * 180); // Max alpha = 180 (not fully opaque)

                foreach (ToolStrip toolbar in toolbars)
                {
                    ApplyBlendedColor(toolbar, originalColors[toolbar], alpha);
                }

                // ConfigureAwait(true) preserves the WinForms SynchronizationContext so the
                // continuation resumes on the UI thread; the InvokeRequired guards above
                // handle the rare case where the context is not available.
                await Task.Delay(stepDurationMs).ConfigureAwait(true);
            }
        }
        finally
        {
            _isFlashing = false;
            RestoreOriginalColors(toolbars, originalColors);
        }
    }

    private static void ApplyBlendedColor(ToolStrip toolbar, Color originalColor, int alpha)
    {
        // Skip toolbars disposed since the flash started (e.g. parent form closed).
        if (toolbar.IsDisposed)
        {
            return;
        }

        Color blended = BlendColors(originalColor, Color.FromArgb(alpha, Color.Red));
        if (toolbar.InvokeRequired)
        {
            toolbar.BeginInvoke(() =>
            {
                if (!toolbar.IsDisposed)
                {
                    toolbar.BackColor = blended;
                }
            });
        }
        else
        {
            toolbar.BackColor = blended;
        }
    }

    private static void RestoreOriginalColors(List<ToolStrip> toolbars, Dictionary<ToolStrip, Color> originalColors)
    {
        // Guard against disposal and marshal back to the UI thread if the continuation
        // resumed on a non-UI thread.
        foreach (ToolStrip toolbar in toolbars)
        {
            if (toolbar.IsDisposed)
            {
                continue;
            }

            Color originalColor = originalColors[toolbar];
            if (toolbar.InvokeRequired)
            {
                toolbar.BeginInvoke(() => toolbar.BackColor = originalColor);
            }
            else
            {
                toolbar.BackColor = originalColor;
            }
        }
    }

    private static Color BlendColors(Color background, Color overlay)
    {
        // Alpha blending formula
        int alpha = overlay.A;
        int invAlpha = 255 - alpha;

        int r = ((overlay.R * alpha) + (background.R * invAlpha)) / 255;
        int g = ((overlay.G * alpha) + (background.G * invAlpha)) / 255;
        int b = ((overlay.B * alpha) + (background.B * invAlpha)) / 255;

        return Color.FromArgb(r, g, b);
    }

    #endregion

    private void RemoveEmptyRows()
    {
        // Get rows that have items
        var usedRows = _layoutItems.Select(i => i.Row).Distinct().OrderBy(r => r).ToList();

        // Renumber rows to be contiguous
        for (int i = 0; i < usedRows.Count; i++)
        {
            int oldRow = usedRows[i];
            if (oldRow != i)
            {
                foreach (var item in _layoutItems.Where(it => it.Row == oldRow))
                {
                    item.Row = i;
                }
            }
        }

        // Normalize OrderInRow for each row so values are contiguous starting at 0
        foreach (var rowGroup in _layoutItems.GroupBy(i => i.Row))
        {
            int order = 0;
            foreach (var item in rowGroup.OrderBy(i => i.OrderInRow))
            {
                item.OrderInRow = order++;
            }
        }
    }

    private void UpdateRemoveRowButton()
    {
        // Enable remove button only if last row is empty
        if (_rowPanels.Count > 1)
        {
            RowPanel lastRow = _rowPanels[^1];
            bool hasItems = lastRow.Controls.OfType<ToolbarItemPanel>().Any();
            buttonRemoveRow.Enabled = !hasItems;
        }
        else
        {
            buttonRemoveRow.Enabled = false;
        }
    }

    private void ApplyLayout()
    {
        // Get current config
        ToolbarLayoutConfig config = GitCommands.AppSettings.ToolbarLayout ?? new ToolbarLayoutConfig();

        // Update visibility metadata with row/order. Custom toolbars are written to both
        // ToolbarsVisibility and CustomToolbars via SetCustomToolbarMetadata to keep them
        // in sync. Built-in toolbars only appear in ToolbarsVisibility.
        foreach (ToolbarLayoutItem item in _layoutItems)
        {
            if (item.IsBuiltIn)
            {
                ToolbarMetadata? existing = config.ToolbarsVisibility.FirstOrDefault(t => t.Name == item.Name);
                if (existing != null)
                {
                    existing.Row = item.Row;
                    existing.OrderInRow = item.OrderInRow;
                    existing.Visible = item.IsVisible;
                    existing.IconSize = item.IconSize;
                }
                else
                {
                    config.ToolbarsVisibility.Add(new ToolbarMetadata
                    {
                        Name = item.Name,
                        Visible = item.IsVisible,
                        Row = item.Row,
                        OrderInRow = item.OrderInRow,
                        IconSize = item.IconSize
                    });
                }
            }
            else
            {
                config.SetCustomToolbarMetadata(
                    name: item.Name,
                    row: item.Row,
                    orderInRow: item.OrderInRow,
                    visible: item.IsVisible,
                    iconSize: item.IconSize);
            }
        }

        // Save config
        GitCommands.AppSettings.ToolbarLayout = config;
        GitCommands.AppSettings.ToolbarSyncIconTextWithSize = checkBoxSyncIconText.Checked;

        // Persist to disk immediately
        GitCommands.AppSettings.SettingsContainer.Save();

        // Apply to actual toolbars in FormBrowse
        ApplyToFormBrowse();

        // Refresh the baseline so OnFormClosing won't prompt about changes the user just applied.
        _appliedSnapshot = CaptureSnapshot();
    }

    // Snapshot of the layout state used to detect unapplied edits at form close time.
    private sealed record LayoutSnapshot(
        Dictionary<string, ToolbarSnapshotEntry> Items,
        bool SyncIconTextWithSize)
    {
        public LayoutSnapshot()
            : this(new Dictionary<string, ToolbarSnapshotEntry>(StringComparer.Ordinal), false)
        {
        }
    }

    private readonly record struct ToolbarSnapshotEntry(int Row, int OrderInRow, bool IsVisible, int IconSize);

    private LayoutSnapshot CaptureSnapshot()
    {
        Dictionary<string, ToolbarSnapshotEntry> items = _layoutItems.ToDictionary(
            i => i.Name,
            i => new ToolbarSnapshotEntry(i.Row, i.OrderInRow, i.IsVisible, i.IconSize),
            StringComparer.Ordinal);
        return new LayoutSnapshot(items, checkBoxSyncIconText.Checked);
    }

    private bool HasUnsavedChanges()
    {
        LayoutSnapshot current = CaptureSnapshot();
        if (current.SyncIconTextWithSize != _appliedSnapshot.SyncIconTextWithSize
            || current.Items.Count != _appliedSnapshot.Items.Count)
        {
            return true;
        }

        foreach ((string name, ToolbarSnapshotEntry currentEntry) in current.Items)
        {
            if (!_appliedSnapshot.Items.TryGetValue(name, out ToolbarSnapshotEntry baselineEntry)
                || !currentEntry.Equals(baselineEntry))
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Only prompt for an unsaved-changes confirmation when the user closes via the X
        // button / Alt+F4 / system menu. OK and Cancel are explicit choices that already
        // express the user's intent (apply / discard), so we don't second-guess them.
        if (e.CloseReason == CloseReason.UserClosing
            && DialogResult != DialogResult.OK
            && DialogResult != DialogResult.Cancel
            && HasUnsavedChanges())
        {
            DialogResult answer = MessageBoxes.Show(
                "You have unsaved toolbar layout changes.\n\nApply them before closing?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                ApplyLayout();
                DialogResult = DialogResult.OK;
            }
            else if (answer == DialogResult.Cancel)
            {
                e.Cancel = true;
                base.OnFormClosing(e);
                return;
            }
            else
            {
                // No = close without applying. Mark as Cancel so a later OnFormClosing
                // pass (if any) won't re-prompt.
                DialogResult = DialogResult.Cancel;
            }
        }

        base.OnFormClosing(e);
    }

    private void ApplyToFormBrowse()
    {
        // Propagate IsVisible and IconSize changes to the actual ToolStrips before reorganizing.
        // Font scaling is applied by ReorganizeToolbars via ApplyToolbarFontScaling.
        foreach (ToolbarLayoutItem item in _layoutItems)
        {
            ToolStrip? ts = GetToolStripByName(item.Name);
            if (ts != null)
            {
                ts.Visible = item.IsVisible;
                FormBrowse.ApplyToolbarIconSize(ts, item.IconSize, GitCommands.AppSettings.ToolbarSyncIconTextWithSize);
            }
        }

        _formBrowse.ReorganizeToolbars();
    }
}
