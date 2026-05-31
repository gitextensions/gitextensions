using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitUI.UserControls;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ToolbarsSettingsPage : SettingsPageWithHeader
{
    // Built-in toolbar names.
    private const string StandardToolbarName = "Standard";
    private const string FiltersToolbarName = "Filters";
    private const string ScriptsToolbarName = "Scripts";

    // The default action category and the display-name markers used for synthetic wrapper rows.
    private const string AllActionsCategory = "All Actions";
    private const string CustomToolbarDisplayPrefix = "Custom ";
    private const string SeparatorDisplayName = "--- separator ---";
    private const string ExpandingSpacerDisplayName = "--- expanding spacer ---";
    private const string LabelDisplayNamePrefix = "--- label:";
    private const string LabelDisplayNameSuffix = " ---";

    private readonly Dictionary<string, List<ToolStripItemWrapper>> _toolbarItems = new();
    private readonly Dictionary<string, ToolStrip> _dynamicToolbars = new();
    private readonly Dictionary<string, ToolStripItem> _originalItems = new();
    private List<ToolStripItemWrapper> _defaultStandardItems = new();
    private List<ToolStripItemWrapper> _defaultFiltersItems = new();
    private string _currentToolbarName = StandardToolbarName;
    private FormBrowse? _formBrowse;

    // Items excluded from AllActionsCategory because they are non-functional on a toolbar,
    // dashboard-only, or exact duplicates of another item already present.
    private static readonly HashSet<string> _excludedFromAllActions = new()
    {
        "refreshDashboardToolStripMenuItem",  // only works on the dashboard screen, not in a repo
        "RefreshButton",                      // exact duplicate of refreshToolStripMenuItem
        "closeToolStripMenuItem",             // "Close (go to Dashboard)" — not useful as a toolbar action
        "dashboardToolStripMenuItem",         // visible only when dashboard is active
        "commitInfoBelowMenuItem",            // sub-items of menuCommitInfoPosition split button (already in Standard toolbar)
        "commitInfoLeftwardMenuItem",
        "commitInfoRightwardMenuItem",
    };

    private readonly Stack<(string toolbarName, List<ToolStripItemWrapper> items)> _undoStack = new();
    private int _dragSourceIndex = -1;
    private bool _isDragging;
    private bool _dragUndoSnapshotTaken;
    private bool _allIconTextActive;
    private bool _isInitialized;
    private bool _isFlashing;

    public ToolbarsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();

        // Initialize toolbar combo box with only built-in toolbars
        comboBoxToolbar.Items.AddRange(new object[] { StandardToolbarName, FiltersToolbarName, ScriptsToolbarName });
        comboBoxToolbar.SelectedIndex = 0;

        // Initialize category combo box
        comboBoxCategory.Items.AddRange(new object[]
        {
            AllActionsCategory,
            "Start",
            "Repository",
            "Navigate",
            "View",
            "Commands",
            "GitHub",
            "Plugins",
            "Tools",
            "Help",
            "Right click menu",
            "Default Standard toolbar",
            "Default Filters toolbar"
        });
        comboBoxCategory.SelectedIndex = 0;

        // Add triple-click support to search textboxes
        textBoxFilterAvailable.MouseClick += TextBox_TripleClick;
        textBoxFilterCurrent.MouseClick += TextBox_TripleClick;

        // Add event handlers for ListBox selection changes
        listBoxAvailable.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        listBoxCurrent.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        listBoxAvailable.Click += (s, e) =>
        {
            listBoxCurrent.ClearSelected();
            ListBox_SelectedIndexChanged(s, e);
        };
        listBoxCurrent.Click += (s, e) =>
        {
            listBoxAvailable.ClearSelected();
            ListBox_SelectedIndexChanged(s, e);
        };

        listBoxCurrent.MouseDown += ListBoxCurrent_MouseDown;
        listBoxCurrent.MouseMove += ListBoxCurrent_MouseMove;
        listBoxCurrent.MouseUp += ListBoxCurrent_MouseUp;

        // Initialize tooltips
        InitializeToolTips();

        // Update button states
        UpdateToolbarButtons();
    }

    public string? InitialToolbarName { get; set; }

    public static SettingsPageReference GetPageReference(string? toolbarName = null)
    {
        return new ToolbarsSettingsPageReference(toolbarName);
    }

    public override void OnPageShown()
    {
        base.OnPageShown();

        if (!string.IsNullOrEmpty(InitialToolbarName) &&
            comboBoxToolbar.Items.Contains(InitialToolbarName))
        {
            comboBoxToolbar.SelectedItem = InitialToolbarName;
        }

        InitialToolbarName = null;
    }

    public sealed class ToolbarsSettingsPageReference : SettingsPageReferenceByType
    {
        public ToolbarsSettingsPageReference(string? initialToolbarName = null)
            : base(typeof(ToolbarsSettingsPage))
        {
            InitialToolbarName = initialToolbarName;
        }

        public string? InitialToolbarName { get; }
    }

    protected override void SettingsToPage()
    {
        // Find FormBrowse from open forms
        _formBrowse = FindFormBrowse();

        if (_formBrowse != null)
        {
            LoadToolbarItems();
            LoadCurrentLayout();
            LoadToolbarVisibility();
        }
        else
        {
            // Disable all controls if FormBrowse is not available
            SetControlsEnabled(false);
            labelNoFormBrowse.Visible = true;
        }

        _isInitialized = true;
        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        if (_formBrowse != null)
        {
            ApplyChanges();
        }

        base.PageToSettings();
    }

    private static FormBrowse? FindFormBrowse()
    {
        return Application.OpenForms.OfType<FormBrowse>().FirstOrDefault();
    }

    private void SetControlsEnabled(bool enabled)
    {
        comboBoxToolbar.Enabled = enabled;
        checkBoxToolbarVisible.Enabled = enabled;
        buttonAddToolbar.Enabled = enabled;
        buttonRemoveToolbar.Enabled = enabled;
        comboBoxCategory.Enabled = enabled;
        textBoxFilterAvailable.Enabled = enabled;
        textBoxFilterCurrent.Enabled = enabled;
        listBoxAvailable.Enabled = enabled;
        listBoxCurrent.Enabled = enabled;
        buttonAddAll.Enabled = enabled;
        buttonAdd.Enabled = enabled;
        buttonRemove.Enabled = enabled;
        buttonMoveUp.Enabled = enabled;
        buttonMoveDown.Enabled = enabled;
        buttonClearCurrent.Enabled = enabled;
        buttonToolbarLayout.Enabled = enabled;
        buttonLocateToolbar.Enabled = enabled;
        buttonShowIconText.Enabled = enabled;
        buttonShowAllIconText.Enabled = enabled;
    }

    private void InitializeToolTips()
    {
        ToolTip.SetToolTip(comboBoxToolbar, "Select which toolbar to customize");
        ToolTip.SetToolTip(checkBoxToolbarVisible, "Show or hide the selected toolbar in the main window");
        ToolTip.SetToolTip(buttonAddToolbar,
            "Add a new custom toolbar\n" +
            "Hold SHIFT while clicking to specify a custom name instead of 'Custom XX'");
        ToolTip.SetToolTip(buttonRemoveToolbar, "Delete the selected custom toolbar (built-in toolbars cannot be deleted)");
        ToolTip.SetToolTip(comboBoxCategory, "Filter available actions by category");
        ToolTip.SetToolTip(textBoxFilterAvailable, "Type to search and filter available actions");
        ToolTip.SetToolTip(buttonClearAvailableFilter, "Clear the search filter for available actions");
        ToolTip.SetToolTip(textBoxFilterCurrent, "Type to search and filter current toolbar actions");
        ToolTip.SetToolTip(buttonClearCurrentFilter, "Clear the search filter for current toolbar actions");
        ToolTip.SetToolTip(listBoxAvailable, "Available actions that can be added to the toolbar\nDouble-click an action to add it");
        ToolTip.SetToolTip(listBoxCurrent, "Actions currently in the selected toolbar\nDouble-click an action to remove it");
        ToolTip.SetToolTip(buttonAddAll, "Add all available actions to the current toolbar");
        ToolTip.SetToolTip(buttonAdd, "Add the selected action to the current toolbar (→)");
        ToolTip.SetToolTip(buttonRemove, "Remove the selected action from the current toolbar (←)");
        ToolTip.SetToolTip(buttonMoveUp, "Move the selected action up in the toolbar order (↑)");
        ToolTip.SetToolTip(buttonMoveDown, "Move the selected action down in the toolbar order (↓)");
        ToolTip.SetToolTip(buttonClearCurrent, "Remove all actions from the current toolbar");
        ToolTip.SetToolTip(buttonUndo, "Undo the last toolbar customization");
        ToolTip.SetToolTip(buttonShowIconText,
            "Show text label next to the selected icon only");
        ToolTip.SetToolTip(buttonShowAllIconText,
            "Show text labels next to all icons in this toolbar\nDisables per-icon text toggle");
        ToolTip.SetToolTip(buttonToolbarLayout,
            "Open the toolbar layout configuration window\n" +
            "Allows you to arrange toolbars in rows and adjust their visual positions");
        ToolTip.SetToolTip(buttonLocateToolbar,
            "Highlight the selected toolbar in the main window\n" +
            "Useful for finding where a toolbar is located");
    }

    private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender == listBoxAvailable)
        {
            buttonMoveUp.Enabled = false;
            buttonMoveDown.Enabled = false;
            buttonRemove.Enabled = false;
            buttonAdd.Enabled = listBoxAvailable.SelectedIndex >= 0;
            UpdateButtonAddAllEnabled();
        }
        else if (sender == listBoxCurrent)
        {
            int index = listBoxCurrent.SelectedIndex;
            buttonMoveUp.Enabled = index > 0;
            buttonMoveDown.Enabled = index >= 0 && index < listBoxCurrent.Items.Count - 1;
            buttonRemove.Enabled = index >= 0 && listBoxCurrent.Items.Count > 0;
            buttonAdd.Enabled = listBoxAvailable.SelectedIndex >= 0;
            UpdateButtonAddAllEnabled();

            bool selectedIsRealItem = index >= 0 &&
                listBoxCurrent.Items[index] is ToolStripItemWrapper w &&
                w.Item is not null;
            buttonShowIconText.Enabled = selectedIsRealItem && !_allIconTextActive;
        }
    }

    private void LoadToolbarItems()
    {
        if (_formBrowse == null)
        {
            return;
        }

        // Seed from FormBrowse's authoritative dictionary — contains all items
        // regardless of which toolbar they currently live in.
        foreach ((string name, ToolStripItem item) in _formBrowse.OriginalToolbarItems)
        {
            if (!_originalItems.ContainsKey(name))
            {
                _originalItems[name] = item;
            }
        }

        StoreOriginalItems(_formBrowse.ToolStripMain);
        StoreOriginalItems(_formBrowse.ToolStripFilters);
        StoreOriginalItems(_formBrowse.ToolStripScripts);

        _toolbarItems[StandardToolbarName] = GetToolStripItems(_formBrowse.ToolStripMain);
        _toolbarItems[FiltersToolbarName] = GetToolStripItems(_formBrowse.ToolStripFilters);
        _toolbarItems[ScriptsToolbarName] = GetToolStripItems(_formBrowse.ToolStripScripts);

        _defaultStandardItems = GetItemWrappersFromSnapshot(_formBrowse.DefaultStandardToolbarSnapshot);
        _defaultFiltersItems = GetItemWrappersFromSnapshot(_formBrowse.DefaultFiltersToolbarSnapshot);
        InjectDefaultFiltersLabels(_defaultFiltersItems);

        // Load custom toolbars
        Control? toolPanelContainer = _formBrowse.Controls.Cast<Control>()
            .FirstOrDefault(c => c is ToolStripContainer);

        if (toolPanelContainer is ToolStripContainer toolPanel)
        {
            foreach (Control control in toolPanel.TopToolStripPanel.Controls)
            {
                if (control is ToolStripEx customToolStrip &&
                    customToolStrip.Name.StartsWith("ToolStripCustom"))
                {
                    string toolbarName = customToolStrip.Text;
                    StoreOriginalItems(customToolStrip);
                    _toolbarItems[toolbarName] = GetToolStripItems(customToolStrip);
                    _dynamicToolbars[toolbarName] = customToolStrip;

                    if (!comboBoxToolbar.Items.Contains(toolbarName))
                    {
                        comboBoxToolbar.Items.Add(toolbarName);
                    }
                }
            }
        }
    }

    private void StoreOriginalItems(ToolStrip? toolStrip)
    {
        if (toolStrip == null)
        {
            return;
        }

        foreach (ToolStripItem item in toolStrip.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.Name) && !_originalItems.ContainsKey(item.Name))
            {
                _originalItems[item.Name] = item;
            }
        }
    }

    private static List<ToolStripItemWrapper> GetToolStripItems(ToolStrip? toolStrip)
    {
        if (toolStrip == null)
        {
            return new List<ToolStripItemWrapper>();
        }

        List<ToolStripItemWrapper> items = new();
        foreach (ToolStripItem item in toolStrip.Items)
        {
            ToolStripItemWrapper? wrapper = WrapLiveToolStripItem(item);
            if (wrapper != null)
            {
                items.Add(wrapper);
            }
        }

        return items;
    }

    private static ToolStripItemWrapper? WrapLiveToolStripItem(ToolStripItem item)
    {
        if (item is ToolStripSeparator)
        {
            // Keep the real item so ApplyChanges can retrieve its Name for serialization.
            // Display name stays SeparatorDisplayName regardless.
            return !string.IsNullOrWhiteSpace(item.Name)
                ? new ToolStripItemWrapper(item, SeparatorDisplayName)
                : new ToolStripItemWrapper(null, SeparatorDisplayName);
        }

        if (item is ToolStripLabel label)
        {
            bool isSpacer = string.IsNullOrWhiteSpace(label.Text) ||
                            (label.Name?.Contains("spacer", StringComparison.OrdinalIgnoreCase) ?? false);
            bool isEditable = label.Name is not null
                              && (label.Name.StartsWith("editableLabel_") || label.Name.StartsWith("_LABEL_"));

            if (isSpacer)
            {
                return new ToolStripItemWrapper(null, ExpandingSpacerDisplayName);
            }

            if (isEditable && !string.IsNullOrWhiteSpace(label.Text))
            {
                return new ToolStripItemWrapper(null, $"--- label:{label.Text} ---");
            }

            return null;
        }

        if (!string.IsNullOrWhiteSpace(item.Name) &&
            !item.Name.StartsWith(FormBrowse.FetchPullToolbarShortcutsPrefix, StringComparison.Ordinal))
        {
            return new ToolStripItemWrapper(item);
        }

        return null;
    }

    private static List<ToolStripItemWrapper> GetItemWrappersFromSnapshot(IReadOnlyList<ToolStripItem> items)
    {
        List<ToolStripItemWrapper> wrappers = new();
        foreach (ToolStripItem item in items)
        {
            if (item is ToolStripSeparator)
            {
                wrappers.Add(new ToolStripItemWrapper(null, SeparatorDisplayName));
            }
            else if (item is ToolStripLabel label)
            {
                bool isSpacer = string.IsNullOrWhiteSpace(label.Text) ||
                                (label.Name?.Contains("spacer", StringComparison.OrdinalIgnoreCase) ?? false);
                bool isEditable = label.Name is not null
                                  && (label.Name.StartsWith("editableLabel_") || label.Name.StartsWith("_LABEL_"));
                if (isSpacer)
                {
                    wrappers.Add(new ToolStripItemWrapper(null, ExpandingSpacerDisplayName));
                }
                else if (isEditable && !string.IsNullOrWhiteSpace(label.Text))
                {
                    wrappers.Add(new ToolStripItemWrapper(null, $"--- label:{label.Text} ---"));
                }

                // Static text labels (e.g. "Branches:", "Filter:") are intentionally skipped.
            }
            else if (!string.IsNullOrWhiteSpace(item.Name))
            {
                wrappers.Add(new ToolStripItemWrapper(item));
            }
        }

        return wrappers;
    }

    private static void InjectDefaultFiltersLabels(List<ToolStripItemWrapper> items)
    {
        // Insert "[Label] Branches:" after the "All branches" split button (tssbtnShowBranches)
        int allBranchesIdx = items.FindIndex(w => w.Item?.Name == "tssbtnShowBranches");
        if (allBranchesIdx >= 0)
        {
            items.Insert(allBranchesIdx + 1, new ToolStripItemWrapper(null, "--- label:Branches: ---"));
        }

        // Insert "[Label] Filter:" before the revision filter textbox (tstxtRevisionFilter)
        int revFilterIdx = items.FindIndex(w => w.Item?.Name == "tstxtRevisionFilter");
        if (revFilterIdx >= 0)
        {
            items.Insert(revFilterIdx, new ToolStripItemWrapper(null, "--- label:Filter: ---"));
        }
    }

    private void LoadCurrentLayout()
    {
        listBoxAvailable.Items.Clear();
        listBoxCurrent.Items.Clear();

        if (!_toolbarItems.ContainsKey(_currentToolbarName))
        {
            return;
        }

        List<ToolStripItemWrapper> currentItems = _toolbarItems[_currentToolbarName];

        foreach (ToolStripItemWrapper wrapper in currentItems)
        {
            listBoxCurrent.Items.Add(wrapper);
        }

        FilterAvailableActionsByCategory();

        buttonMoveUp.Enabled = false;
        buttonMoveDown.Enabled = false;
        buttonRemove.Enabled = false;
        UpdateButtonAddAllEnabled();
    }

    private void ComboBoxToolbar_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isInitialized && !string.IsNullOrEmpty(_currentToolbarName) && _toolbarItems.ContainsKey(_currentToolbarName))
        {
            SaveCurrentToolbarLayout();
        }

        _currentToolbarName = comboBoxToolbar.SelectedItem?.ToString() ?? StandardToolbarName;
        ToolStrip? newToolbar = GetToolStripByName(_currentToolbarName);
        _allIconTextActive = newToolbar != null && AllRealItemsShowText(newToolbar);
        UpdateToggleButtonAppearance();

        LoadToolbarVisibility();
        LoadCurrentLayout();
        UpdateToolbarButtons();
    }

    private void CheckBoxToolbarVisible_CheckedChanged(object? sender, EventArgs e)
    {
        SaveToolbarVisibility();
        buttonLocateToolbar.Enabled = checkBoxToolbarVisible.Checked;
    }

    private void LoadToolbarVisibility()
    {
        ToolStrip? toolbar = GetToolStripByName(_currentToolbarName);

        checkBoxToolbarVisible.CheckedChanged -= CheckBoxToolbarVisible_CheckedChanged;
        checkBoxToolbarVisible.Checked = toolbar?.Visible ?? true;
        checkBoxToolbarVisible.CheckedChanged += CheckBoxToolbarVisible_CheckedChanged;

        buttonLocateToolbar.Enabled = checkBoxToolbarVisible.Checked;
        buttonShowIconText.Enabled = false;
    }

    private void SaveToolbarVisibility()
    {
        ToolStrip? toolbar = GetToolStripByName(_currentToolbarName);

        if (toolbar != null)
        {
            bool wasVisible = toolbar.Visible;
            bool isVisible = checkBoxToolbarVisible.Checked;

            toolbar.Visible = isVisible;

            if (wasVisible != isVisible)
            {
                _formBrowse?.ReorganizeToolbars();
            }
        }
    }

    private void ButtonClearCurrent_Click(object? sender, EventArgs e)
    {
        DialogResult result = MessageBoxes.Show(
            "Remove all items from the current toolbar?",
            "Clear Toolbar",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            PushUndoSnapshot();
            listBoxCurrent.Items.Clear();
            FilterAvailableActionsByCategory();
            buttonMoveUp.Enabled = false;
            buttonMoveDown.Enabled = false;
            buttonRemove.Enabled = false;
        }
    }

    private void ButtonAddToolbar_Click(object? sender, EventArgs e)
    {
        if (_formBrowse == null)
        {
            return;
        }

        int nextNumber = 1;
        while (comboBoxToolbar.Items.Cast<string>().Any(name => name == $"Custom {nextNumber:D2}"))
        {
            nextNumber++;
        }

        string newToolbarName = $"Custom {nextNumber:D2}";

        if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
        {
            string? customName = ShowToolbarNameDialog(newToolbarName);
            if (customName == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(customName))
            {
                MessageBoxes.Show(
                    "Toolbar name cannot be empty.",
                    "Invalid Name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxToolbar.Items.Cast<string>().Any(name => name.Equals(customName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBoxes.Show(
                    $"A toolbar named '{customName}' already exists.",
                    "Duplicate Name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            newToolbarName = customName;
        }

        comboBoxToolbar.Items.Add(newToolbarName);

        string sanitizedName = new string(newToolbarName.Where(c => char.IsLetterOrDigit(c)).ToArray());
        string controlName = $"ToolStripCustom{sanitizedName}";

        // A different display name can produce the same sanitized WinForms control Name
        // (e.g. "Mon-Outil" and "Mon Outil" both yield "ToolStripCustomMonOutil"). The
        // display-name check above passes in the Shift path, but two sibling controls sharing
        // the same Name violates a WinForms invariant and breaks FindControl. Abort if already
        // taken — display name collision was already caught above, so this can only fire when
        // two distinct names produce identical sanitized output.
        if (_dynamicToolbars.Values.Any(ts => ts.Name == controlName))
        {
            comboBoxToolbar.Items.Remove(newToolbarName);
            MessageBoxes.Show(
                $"The toolbar name '{newToolbarName}' conflicts with an existing toolbar's internal name.\nPlease choose a different name.",
                "Duplicate Internal Name",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        ToolStripEx newToolStrip = new()
        {
            Name = controlName,
            Text = newToolbarName,
            Visible = true,
            ClickThrough = true,
            Dock = DockStyle.None,
            DrawBorder = false,
            GripEnabled = false,
            GripStyle = ToolStripGripStyle.Visible,
            GripMargin = new Padding(0),
            Padding = new Padding(0),
            LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow,
            BackColor = _formBrowse.ToolStripMain.BackColor,
            ForeColor = _formBrowse.ToolStripMain.ForeColor
        };

        _dynamicToolbars[newToolbarName] = newToolStrip;
        _toolbarItems[newToolbarName] = new List<ToolStripItemWrapper>();

        AssignNewToolbarToNewRow(newToolbarName);

        comboBoxToolbar.SelectedItem = newToolbarName;
    }

    private static void AssignNewToolbarToNewRow(string newToolbarName)
    {
        ToolbarLayoutConfig config = AppSettings.ToolbarLayout ?? new ToolbarLayoutConfig();

        int maxRow = config.ToolbarsVisibility is { Count: > 0 }
            ? config.ToolbarsVisibility.Max(t => t.Row)
            : -1;
        if (config.CustomToolbars is { Count: > 0 })
        {
            maxRow = Math.Max(maxRow, config.CustomToolbars.Max(c => c.Row));
        }

        config.SetCustomToolbarMetadata(
            name: newToolbarName,
            row: maxRow + 1,
            orderInRow: 0,
            visible: true,
            iconSize: 16);

        AppSettings.ToolbarLayout = config;
        AppSettings.SettingsContainer.Save();
    }

    private string? ShowToolbarNameDialog(string defaultName)
    {
        using Form inputForm = new()
        {
            Text = "New Toolbar",
            Width = 350,
            Height = 175,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        Label label = new()
        {
            Text = "Enter toolbar name:",
            Left = 15,
            Top = 20,
            Width = 300
        };

        TextBox textBox = new()
        {
            Text = defaultName,
            Left = 15,
            Top = 45,
            Width = 300
        };

        Button confirmButton = new()
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Left = 155,
            Top = 80,
            Width = 75
        };

        Button cancelButton = new()
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Left = 240,
            Top = 80,
            Width = 75
        };

        inputForm.Controls.Add(label);
        inputForm.Controls.Add(textBox);
        inputForm.Controls.Add(confirmButton);
        inputForm.Controls.Add(cancelButton);
        inputForm.AcceptButton = confirmButton;
        inputForm.CancelButton = cancelButton;

        textBox.SelectAll();

        DialogResult result = inputForm.ShowDialog(this);

        if (result == DialogResult.OK)
        {
            return textBox.Text.Trim();
        }

        return null;
    }

    private string? ShowLabelTextDialog()
    {
        using Form inputForm = new()
        {
            Text = "Add Label",
            Width = 350,
            Height = 175,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        Label label = new()
        {
            Text = "Enter label text:",
            Left = 15,
            Top = 20,
            Width = 300
        };

        TextBox textBox = new()
        {
            Left = 15,
            Top = 45,
            Width = 300
        };

        Button confirmButton = new()
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Left = 155,
            Top = 80,
            Width = 75
        };

        Button cancelButton = new()
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Left = 240,
            Top = 80,
            Width = 75
        };

        inputForm.Controls.Add(label);
        inputForm.Controls.Add(textBox);
        inputForm.Controls.Add(confirmButton);
        inputForm.Controls.Add(cancelButton);
        inputForm.AcceptButton = confirmButton;
        inputForm.CancelButton = cancelButton;

        return inputForm.ShowDialog(this) == DialogResult.OK ? textBox.Text.Trim() : null;
    }

    private void ButtonRemoveToolbar_Click(object? sender, EventArgs e)
    {
        if (_currentToolbarName.StartsWith(CustomToolbarDisplayPrefix))
        {
            DialogResult result = MessageBoxes.Show(
                $"Delete toolbar '{_currentToolbarName}'?",
                "Delete Toolbar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (_dynamicToolbars.TryGetValue(_currentToolbarName, out ToolStrip? toolStripToRemove))
                {
                    toolStripToRemove.Parent?.Controls.Remove(toolStripToRemove);
                    toolStripToRemove.Items.Clear();
                    toolStripToRemove.Visible = false;
                    toolStripToRemove.Dispose();

                    _dynamicToolbars.Remove(_currentToolbarName);
                }

                // Remove from _toolbarItems BEFORE touching the combobox to prevent
                // ComboBoxToolbar_SelectedIndexChanged from saving a layout for a toolbar
                // that no longer exists (Items.Remove fires SelectedIndexChanged).
                _toolbarItems.Remove(_currentToolbarName);
                comboBoxToolbar.Items.Remove(_currentToolbarName);
                comboBoxToolbar.SelectedIndex = 0;

                // Persist the deletion so the removed toolbar is not recreated on next startup.
                ToolbarLayoutConfig config = AppSettings.ToolbarLayout ?? new ToolbarLayoutConfig();
                config.RemoveCustomToolbarMetadata(_currentToolbarName);
                AppSettings.ToolbarLayout = config;
                AppSettings.SettingsContainer.Save();

                _formBrowse?.ReorganizeToolbars();
            }
        }
    }

    private void UpdateToolbarButtons()
    {
        buttonRemoveToolbar.Enabled = _currentToolbarName.StartsWith(CustomToolbarDisplayPrefix);
        buttonAdd.Enabled = listBoxAvailable.SelectedIndex >= 0;
    }

    private void ButtonToolbarLayout_Click(object? sender, EventArgs e)
    {
        if (_formBrowse == null)
        {
            return;
        }

        SaveCurrentToolbarLayout();

        using FormToolbarsLayout layoutForm = new(_formBrowse, _dynamicToolbars);
        if (layoutForm.ShowDialog(this) == DialogResult.OK)
        {
            LoadToolbarVisibility();
        }
    }

    private void ButtonShowIconText_Click(object? sender, EventArgs e)
    {
        if (listBoxCurrent.SelectedItem is not ToolStripItemWrapper wrapper || wrapper.Item == null)
        {
            return;
        }

        if (wrapper.Item is ToolStripSeparator or ToolStripLabel)
        {
            return;
        }

        wrapper.ShowText = !wrapper.ShowText;

        // Apply the change visually on the live toolbar item (may differ from wrapper.Item
        // when wrapper.Item is a ToolStripMenuItem and the toolbar holds a converted button).
        ApplyShowTextToLiveItem(wrapper);

        listBoxCurrent.Invalidate();
    }

    // Finds the live ToolStripItem for this wrapper across all toolbars and updates its DisplayStyle.
    // With IPushLabelItem, each toolbar instance (original or clone) carries independent state,
    // so we act directly on each matching live item without resolving to the original.
    private void ApplyShowTextToLiveItem(ToolStripItemWrapper wrapper)
    {
        if (wrapper.Item == null || string.IsNullOrEmpty(wrapper.Item.Name))
        {
            return;
        }

        // Use the wrapper's display name as the label so items with dynamic ToolTipText
        // (e.g. tsbtnAdvancedFilter whose tooltip reflects active filter state) get a
        // stable, human-readable label instead of whatever the tooltip says at that moment.
        string labelText = wrapper.DisplayName;

        // Try wrapper.Item directly first (it may already be the live toolbar item).
        if (wrapper.Item is not ToolStripSeparator and not ToolStripLabel)
        {
            FormBrowse.ApplyItemDisplayStyle(wrapper.Item, wrapper.ShowText, labelText);
        }

        // For IPushLabelItem (ToolStripPushButton / ToolStripPushButtonClone), each toolbar
        // instance carries its own independent ShowLabel state. Do not propagate this wrapper's
        // ShowText to other toolbar instances — each must be controlled through its own wrapper.
        if (wrapper.Item is IPushLabelItem)
        {
            return;
        }

        if (_formBrowse == null)
        {
            return;
        }

        string itemName = wrapper.Item.Name;
        ToolStrip[] allToolbars = [_formBrowse.ToolStripMain, _formBrowse.ToolStripFilters, _formBrowse.ToolStripScripts];

        // Also search all live toolbars by name to cover the case where wrapper.Item
        // is a ToolStripMenuItem but the toolbar holds a converted ToolStripButton.
        ApplyShowTextToToolbars(itemName, wrapper.Item, wrapper.ShowText, labelText, allToolbars);

        // Also check custom toolbars — each clone is updated independently.
        ApplyShowTextToToolbars(itemName, wrapper.Item, wrapper.ShowText, labelText, _dynamicToolbars.Values);
    }

    private static void ApplyShowTextToToolbars(string itemName, ToolStripItem wrapperItem, bool showText, string labelText, IEnumerable<ToolStrip> toolbars)
    {
        foreach (ToolStrip toolbar in toolbars)
        {
            ToolStripItem? liveItem = toolbar.Items.Cast<ToolStripItem>()
                .FirstOrDefault(i => i.Name == itemName
                                  || (i.Tag is ToolStripMenuItem m && m.Name == itemName)
                                  || (i.Tag is ToolStripItem src && src.Name == itemName));
            if (liveItem is not null and not ToolStripSeparator and not ToolStripLabel
                && liveItem != wrapperItem)
            {
                FormBrowse.ApplyItemDisplayStyle(liveItem, showText, labelText);
                toolbar.Invalidate();
            }
        }
    }

    private void ButtonShowAllIconText_Click(object? sender, EventArgs e)
    {
        ToolStrip? currentToolbar = GetToolStripByName(_currentToolbarName);
        if (currentToolbar == null)
        {
            return;
        }

        _allIconTextActive = !_allIconTextActive;

        // Update live toolbar items and keep wrapper.ShowText in sync.
        // Iterate wrappers (not currentToolbar.Items directly) to have access to
        // DisplayName so items with dynamic ToolTipText get a stable label.
        foreach (ToolStripItemWrapper wrapper in listBoxCurrent.Items)
        {
            if (wrapper.Item is not null and not ToolStripSeparator and not ToolStripLabel)
            {
                wrapper.ShowText = _allIconTextActive;
                ApplyShowTextToLiveItem(wrapper);
            }
        }

        currentToolbar.Invalidate();

        UpdateToggleButtonAppearance();
        listBoxCurrent.Invalidate();

        bool selectedIsRealItem = listBoxCurrent.SelectedIndex >= 0 &&
            listBoxCurrent.Items[listBoxCurrent.SelectedIndex] is ToolStripItemWrapper w &&
            w.Item is not null;
        buttonShowIconText.Enabled = selectedIsRealItem && !_allIconTextActive;
    }

    private void UpdateToggleButtonAppearance()
    {
        if (_allIconTextActive)
        {
            buttonShowAllIconText.FlatStyle = FlatStyle.Flat;
            buttonShowAllIconText.FlatAppearance.BorderColor = SystemColors.Highlight;
            buttonShowAllIconText.BackColor = SystemColors.GradientActiveCaption;
        }
        else
        {
            buttonShowAllIconText.FlatStyle = FlatStyle.Standard;
            buttonShowAllIconText.BackColor = SystemColors.Control;
        }
    }

    private void ButtonAdd_Click(object? sender, EventArgs e)
    {
        if (listBoxAvailable.SelectedItem is not ToolStripItemWrapper wrapper)
        {
            return;
        }

        ToolStripItemWrapper? itemToAdd = null;

        if (wrapper.DisplayName is SeparatorDisplayName or ExpandingSpacerDisplayName)
        {
            itemToAdd = new ToolStripItemWrapper(null, wrapper.DisplayName);
        }
        else if (wrapper.DisplayName == "--- custom label ---")
        {
            string? labelText = ShowLabelTextDialog();
            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd = new ToolStripItemWrapper(null, $"--- label:{labelText} ---");
            }
        }
        else if (wrapper.DisplayName.StartsWith(LabelDisplayNamePrefix) && wrapper.DisplayName.EndsWith(LabelDisplayNameSuffix))
        {
            itemToAdd = new ToolStripItemWrapper(null, wrapper.DisplayName);
        }
        else
        {
            bool alreadyInCurrent = listBoxCurrent.Items.Cast<ToolStripItemWrapper>()
                .Any(w => w.Item != null && w.Item.Name == wrapper.Item?.Name);
            if (!alreadyInCurrent)
            {
                itemToAdd = new ToolStripItemWrapper(wrapper.Item);
            }
        }

        if (itemToAdd is null)
        {
            return;
        }

        PushUndoSnapshot();
        listBoxCurrent.Items.Add(itemToAdd);
        listBoxCurrent.SelectedItem = itemToAdd;
    }

    private void ButtonAddAll_Click(object? sender, EventArgs e)
    {
        PushUndoSnapshot();

        HashSet<string> currentItemNames = listBoxCurrent.Items.Cast<ToolStripItemWrapper>()
            .Where(w => w.Item is not null && w.Item.Name is not null)
            .Select(w => w.Item!.Name!)
            .ToHashSet();

        int insertedCount = 0;
        foreach (ToolStripItemWrapper wrapper in listBoxAvailable.Items)
        {
            AddWrapperIfEligible(wrapper, currentItemNames, ref insertedCount);
        }

        StripTrailingSeparators(ref insertedCount);

        FilterAvailableActionsByCategory();

        if (insertedCount > 0 && listBoxCurrent.Items.Count > 0)
        {
            listBoxCurrent.SelectedIndex = Math.Max(0, listBoxCurrent.Items.Count - insertedCount);
        }
        else if (insertedCount == 0)
        {
            // Nothing was added — discard the snapshot to keep the undo stack clean.
            _undoStack.TryPop(out _);
            buttonUndo.Enabled = _undoStack.Count > 0;
        }
    }

    private void AddWrapperIfEligible(ToolStripItemWrapper wrapper, HashSet<string> currentItemNames, ref int insertedCount)
    {
        bool isSeparatorOrSpacer = wrapper.DisplayName == SeparatorDisplayName || wrapper.DisplayName == ExpandingSpacerDisplayName;

        if (isSeparatorOrSpacer)
        {
            // Skip if the current list is empty or already ends with a separator/spacer
            if (listBoxCurrent.Items.Count == 0)
            {
                return;
            }

            ToolStripItemWrapper last = (ToolStripItemWrapper)listBoxCurrent.Items[listBoxCurrent.Items.Count - 1];
            if (last.DisplayName == SeparatorDisplayName || last.DisplayName == ExpandingSpacerDisplayName)
            {
                return;
            }

            listBoxCurrent.Items.Add(wrapper);
            insertedCount++;
        }
        else if (wrapper.Item is null || wrapper.Item.Name is null || !currentItemNames.Contains(wrapper.Item.Name))
        {
            listBoxCurrent.Items.Add(wrapper);
            insertedCount++;
        }
    }

    private void StripTrailingSeparators(ref int insertedCount)
    {
        while (listBoxCurrent.Items.Count > 0)
        {
            ToolStripItemWrapper last = (ToolStripItemWrapper)listBoxCurrent.Items[listBoxCurrent.Items.Count - 1];
            if (last.DisplayName == SeparatorDisplayName || last.DisplayName == ExpandingSpacerDisplayName)
            {
                listBoxCurrent.Items.RemoveAt(listBoxCurrent.Items.Count - 1);
                insertedCount--;
            }
            else
            {
                break;
            }
        }
    }

    private void ButtonRemove_Click(object? sender, EventArgs e)
    {
        if (listBoxCurrent.SelectedItem is ToolStripItemWrapper wrapper)
        {
            PushUndoSnapshot();
            int currentIndex = listBoxCurrent.SelectedIndex;
            listBoxCurrent.Items.Remove(wrapper);

            FilterAvailableActionsByCategory();

            if (listBoxCurrent.Items.Count > 0)
            {
                if (currentIndex < listBoxCurrent.Items.Count)
                {
                    listBoxCurrent.SelectedIndex = currentIndex;
                }
                else
                {
                    listBoxCurrent.SelectedIndex = listBoxCurrent.Items.Count - 1;
                }
            }
            else
            {
                buttonMoveUp.Enabled = false;
                buttonMoveDown.Enabled = false;
                buttonRemove.Enabled = false;
            }
        }
    }

    private void ButtonMoveUp_Click(object? sender, EventArgs e)
    {
        int index = listBoxCurrent.SelectedIndex;
        if (index > 0)
        {
            PushUndoSnapshot();
            object item = listBoxCurrent.Items[index];
            listBoxCurrent.Items.RemoveAt(index);
            listBoxCurrent.Items.Insert(index - 1, item);
            listBoxCurrent.SelectedIndex = index - 1;

            ListBox_SelectedIndexChanged(listBoxCurrent, EventArgs.Empty);
        }
    }

    private void ButtonMoveDown_Click(object? sender, EventArgs e)
    {
        int index = listBoxCurrent.SelectedIndex;
        if (index >= 0 && index < listBoxCurrent.Items.Count - 1)
        {
            PushUndoSnapshot();
            object item = listBoxCurrent.Items[index];
            listBoxCurrent.Items.RemoveAt(index);
            listBoxCurrent.Items.Insert(index + 1, item);
            listBoxCurrent.SelectedIndex = index + 1;

            ListBox_SelectedIndexChanged(listBoxCurrent, EventArgs.Empty);
        }
    }

    private void ListBoxAvailable_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (listBoxAvailable.IndexFromPoint(e.Location) == listBoxAvailable.SelectedIndex
            && listBoxAvailable.SelectedIndex >= 0)
        {
            ButtonAdd_Click(sender, e);
        }
    }

    private void ListBoxCurrent_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (listBoxCurrent.IndexFromPoint(e.Location) == listBoxCurrent.SelectedIndex
            && listBoxCurrent.SelectedIndex >= 0)
        {
            ButtonRemove_Click(sender, e);
        }
    }

    private void ListBoxCurrent_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        int index = listBoxCurrent.IndexFromPoint(e.Location);
        if (index >= 0)
        {
            _dragSourceIndex = index;
            _isDragging = false;
            _dragUndoSnapshotTaken = false;
        }
    }

    private void ListBoxCurrent_MouseMove(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || _dragSourceIndex < 0)
        {
            return;
        }

        if (!_dragUndoSnapshotTaken)
        {
            PushUndoSnapshot();
            _dragUndoSnapshotTaken = true;
        }

        _isDragging = true;

        int mouseY = e.Location.Y;

        if (_dragSourceIndex > 0)
        {
            Rectangle prevRect = listBoxCurrent.GetItemRectangle(_dragSourceIndex - 1);
            if (mouseY < prevRect.Bottom)
            {
                object item = listBoxCurrent.Items[_dragSourceIndex];
                listBoxCurrent.Items.RemoveAt(_dragSourceIndex);
                _dragSourceIndex--;
                listBoxCurrent.Items.Insert(_dragSourceIndex, item);
                listBoxCurrent.SelectedIndex = _dragSourceIndex;
            }
        }

        if (_dragSourceIndex < listBoxCurrent.Items.Count - 1)
        {
            Rectangle nextRect = listBoxCurrent.GetItemRectangle(_dragSourceIndex + 1);
            if (mouseY > nextRect.Top)
            {
                object item = listBoxCurrent.Items[_dragSourceIndex];
                listBoxCurrent.Items.RemoveAt(_dragSourceIndex);
                _dragSourceIndex++;
                listBoxCurrent.Items.Insert(_dragSourceIndex, item);
                listBoxCurrent.SelectedIndex = _dragSourceIndex;
            }
        }

        listBoxCurrent.Cursor = Cursors.Hand;
    }

    private void ListBoxCurrent_MouseUp(object? sender, MouseEventArgs e)
    {
        listBoxCurrent.Cursor = Cursors.Default;

        if (_isDragging)
        {
            ListBox_SelectedIndexChanged(listBoxCurrent, EventArgs.Empty);
        }

        _dragSourceIndex = -1;
        _isDragging = false;
        listBoxCurrent.Invalidate();
    }

    private void SaveCurrentToolbarLayout()
    {
        _toolbarItems[_currentToolbarName] = GetCurrentItemsSnapshot();
    }

    // Returns a snapshot of the current toolbar's full (unfiltered) item list.
    // When a text filter is active, Tag holds the complete list; otherwise Items is complete.
    private List<ToolStripItemWrapper> GetCurrentItemsSnapshot()
    {
        if (listBoxCurrent.Tag is List<ToolStripItemWrapper> taggedItems)
        {
            return [..taggedItems];
        }

        return listBoxCurrent.Items.Cast<ToolStripItemWrapper>().ToList();
    }

    private void PushUndoSnapshot()
    {
        _undoStack.Push((_currentToolbarName, GetCurrentItemsSnapshot()));
        buttonUndo.Enabled = true;
    }

    private void ButtonUndo_Click(object? sender, EventArgs e)
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        var (toolbarName, items) = _undoStack.Pop();

        if (_currentToolbarName != toolbarName)
        {
            int idx = comboBoxToolbar.Items.IndexOf(toolbarName);
            if (idx >= 0)
            {
                comboBoxToolbar.SelectedIndex = idx;
            }
        }

        // Clear active filter so the restored list is shown in full.
        if (!string.IsNullOrWhiteSpace(textBoxFilterCurrent.Text))
        {
            textBoxFilterCurrent.Text = string.Empty;
        }

        listBoxCurrent.Tag = null;
        listBoxCurrent.BeginUpdate();
        listBoxCurrent.Items.Clear();
        foreach (ToolStripItemWrapper wrapper in items)
        {
            listBoxCurrent.Items.Add(wrapper);
        }

        listBoxCurrent.EndUpdate();

        _toolbarItems[_currentToolbarName] = items;

        FilterAvailableActionsByCategory();

        buttonMoveUp.Enabled = false;
        buttonMoveDown.Enabled = false;
        buttonRemove.Enabled = false;
        buttonUndo.Enabled = _undoStack.Count > 0;
    }

    private void TextBoxFilterAvailable_TextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textBoxFilterAvailable.Text))
        {
            listBoxAvailable.Tag = null;
            FilterAvailableActionsByCategory();
        }
        else
        {
            FilterListBox(listBoxAvailable, textBoxFilterAvailable.Text);
        }
    }

    private void TextBoxFilterCurrent_TextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textBoxFilterCurrent.Text))
        {
            listBoxCurrent.Tag = null;

            listBoxCurrent.BeginUpdate();
            listBoxCurrent.Items.Clear();

            if (_toolbarItems.ContainsKey(_currentToolbarName))
            {
                foreach (ToolStripItemWrapper wrapper in _toolbarItems[_currentToolbarName])
                {
                    listBoxCurrent.Items.Add(wrapper);
                }
            }

            listBoxCurrent.EndUpdate();
        }
        else
        {
            FilterListBox(listBoxCurrent, textBoxFilterCurrent.Text);
        }
    }

    private static void FilterListBox(ListBox listBox, string filterText)
    {
        if (listBox.Tag is not List<ToolStripItemWrapper> allItems)
        {
            allItems = listBox.Items.Cast<ToolStripItemWrapper>().ToList();
            listBox.Tag = allItems;
        }

        listBox.BeginUpdate();
        listBox.Items.Clear();

        if (string.IsNullOrWhiteSpace(filterText))
        {
            listBox.Items.AddRange(allItems.ToArray());
        }
        else
        {
            var filtered = allItems.Where(item =>
                item.DisplayName.Contains(filterText, StringComparison.OrdinalIgnoreCase)).ToArray();
            listBox.Items.AddRange(filtered);
        }

        listBox.EndUpdate();
    }

    private void ApplyChanges()
    {
        if (_formBrowse == null)
        {
            return;
        }

        _formBrowse.FreezeToolbarsForAction(ApplyChangesCore);
    }

    // Represents the fully-resolved item to add to a toolbar, its serialization name, and display style.
    // LabelText is the human-readable label to show when ShowText is true (the wrapper's DisplayName).
    private readonly record struct ResolvedToolbarItem(ToolStripItem Item, string ConfigName, bool ShowText, string LabelText);

    private void ApplyChangesCore()
    {
        if (_formBrowse == null)
        {
            return;
        }

        SaveCurrentToolbarLayout();

        List<string> allToolbarNames = comboBoxToolbar.Items.Cast<string>().ToList();
        ToolbarLayoutConfig? existingConfig = AppSettings.ToolbarLayout;

        // Phase 1 — pure computation: resolve every toolbar's final item list and build the
        // serialization config. No ToolStrip is mutated yet, so any exception here is safe.
        Dictionary<ToolStrip, List<ResolvedToolbarItem>> plan = BuildApplyPlan(allToolbarNames, out ToolbarLayoutConfig config);
        ComputeToolbarsVisibilityConfig(allToolbarNames, existingConfig, config);

        // Phase 2 — atomic commit: all computation succeeded, now mutate the UI and persist.
        // From this point forward we do not throw, so no partial state is possible.
        EnsureOrphanCustomToolstripsJoinedToPanel(allToolbarNames);
        ApplyResolvedItemsToToolStrips(plan);

        AppSettings.ToolbarLayout = config;
        AppSettings.SettingsContainer.Save();
        _formBrowse?.ReorganizeToolbarsCore();
    }

    private Dictionary<ToolStrip, List<ResolvedToolbarItem>> BuildApplyPlan(List<string> allToolbarNames, out ToolbarLayoutConfig config)
    {
        Dictionary<ToolStrip, List<ResolvedToolbarItem>> plan = new(allToolbarNames.Count);
        config = new ToolbarLayoutConfig();

        foreach (string toolbarName in allToolbarNames)
        {
            ToolStrip? toolStrip = GetToolStripByName(toolbarName);
            if (toolStrip == null)
            {
                continue;
            }

            List<ToolStripItemWrapper> wrappers = _toolbarItems.TryGetValue(toolbarName, out List<ToolStripItemWrapper>? stored)
                ? stored
                : [];

            List<ResolvedToolbarItem> resolvedItems = BuildResolvedItems(toolbarName, wrappers);
            plan[toolStrip] = resolvedItems;

            int order = 0;
            foreach (ResolvedToolbarItem resolved in resolvedItems)
            {
                config.Items.Add(new ToolbarItemConfig
                {
                    ItemName = resolved.ConfigName,
                    ToolbarName = toolbarName,
                    Order = order++,
                    ShowText = resolved.ShowText
                });
            }
        }

        return plan;
    }

    private void ComputeToolbarsVisibilityConfig(List<string> allToolbarNames, ToolbarLayoutConfig? existingConfig, ToolbarLayoutConfig config)
    {
        foreach (string toolbarName in allToolbarNames)
        {
            ToolStrip? toolStrip = GetToolStripByName(toolbarName);
            bool visible = toolStrip?.Visible ?? true;
            bool allIconsShowText = toolStrip != null && AllRealItemsShowText(toolStrip);

            if (toolbarName.StartsWith(CustomToolbarDisplayPrefix))
            {
                // SetCustomToolbarMetadata writes both CustomToolbars and ToolbarsVisibility in
                // one call, using ToolbarsVisibility as the authoritative source for Row/OrderInRow
                // (same priority as BuildToolbarLayoutInfo). This prevents the two lists from
                // drifting apart when existingConfig has them out of sync.
                int toolbarIndex = GetToolbarIndex(toolbarName);
                ToolbarMetadata? visMeta = existingConfig?.ToolbarsVisibility?.FirstOrDefault(t => t.Name == toolbarName);
                CustomToolbarMetadata? customMeta = existingConfig?.CustomToolbars?.FirstOrDefault(c => c.Name == toolbarName);

                config.SetCustomToolbarMetadata(
                    name: toolbarName,
                    row: visMeta?.Row ?? customMeta?.Row ?? 0,
                    orderInRow: visMeta?.OrderInRow ?? customMeta?.OrderInRow ?? toolbarIndex,
                    visible: visible,
                    iconSize: visMeta?.IconSize ?? customMeta?.IconSize ?? 16,
                    index: toolbarIndex,
                    allIconsShowText: allIconsShowText);
            }
            else
            {
                ToolbarMetadata? existingMeta = existingConfig?.ToolbarsVisibility?.FirstOrDefault(t => t.Name == toolbarName);
                int defaultOrderInRow = toolbarName switch
                {
                    StandardToolbarName => 0,
                    FiltersToolbarName => 1,
                    ScriptsToolbarName => 2,
                    _ => GetToolbarIndex(toolbarName)
                };
                config.ToolbarsVisibility.Add(new ToolbarMetadata
                {
                    Name = toolbarName,
                    Visible = visible,
                    Row = existingMeta?.Row ?? 0,
                    OrderInRow = existingMeta?.OrderInRow ?? defaultOrderInRow,
                    IconSize = existingMeta?.IconSize ?? 16,
                    AllIconsShowText = allIconsShowText
                });
            }
        }
    }

    private void ApplyResolvedItemsToToolStrips(Dictionary<ToolStrip, List<ResolvedToolbarItem>> plan)
    {
        // Track which configNames have already been placed on a toolbar in this cycle.
        // When the same action appears on multiple toolbars, the second occurrence is cloned
        // rather than moved, so the original stays on the first toolbar.
        HashSet<string> placedConfigNames = new(StringComparer.Ordinal);

        foreach ((ToolStrip toolStrip, List<ResolvedToolbarItem> resolvedItems) in plan)
        {
            // Custom toolbars rebuild a fresh converted button on every Apply (ConvertMenuItemToButton
            // forces ImageAndText), so the per-icon text toggle must be re-applied here. Built-in
            // toolbars reuse the original live item, which already carries the toggled state, and
            // re-applying would clobber dynamic labels (working dir path, branch name).
            bool isCustomToolbar = _dynamicToolbars.ContainsValue(toolStrip);

            foreach (ResolvedToolbarItem resolved in resolvedItems)
            {
                if (resolved.ConfigName.StartsWith('_'))
                {
                    continue;
                }

                ToolStripItem item = resolved.Item;
                if (item.Owner != null && item.Owner != toolStrip && !placedConfigNames.Contains(resolved.ConfigName))
                {
                    item.Owner.Items.Remove(item);
                }
            }

            toolStrip.Items.Clear();

            foreach (ResolvedToolbarItem resolved in resolvedItems)
            {
                ToolStripItem itemToAdd = resolved.Item;

                if (!resolved.ConfigName.StartsWith('_') && placedConfigNames.Contains(resolved.ConfigName)
                    && itemToAdd is ToolStripButton or ToolStripSplitButton or ToolStripDropDownButton)
                {
                    itemToAdd = ToolbarItemConverter.CloneItem(itemToAdd, wantsText: resolved.ShowText);
                }
                else if (isCustomToolbar && !resolved.ConfigName.StartsWith('_'))
                {
                    // Honor the per-icon text toggle (wrapper.ShowText) on the freshly converted
                    // button. Without this, ConvertMenuItemToButton's forced ImageAndText would
                    // always show the label on Apply, discarding the user's "For this icon" choice.
                    // CloneItem already applies it on the clone path above.
                    FormBrowse.ApplyItemDisplayStyle(itemToAdd, resolved.ShowText, resolved.LabelText);
                }

                toolStrip.Items.Add(itemToAdd);
                itemToAdd.Visible = true;

                if (!resolved.ConfigName.StartsWith('_'))
                {
                    placedConfigNames.Add(resolved.ConfigName);
                }
            }
        }
    }

    // For a converted button (Tag = source MenuItem) or a clone (Tag = original
    // ToolStripItem with a real Name), serialize under the source name so the
    // entry can be resolved at restart. Falls back to item.Name otherwise.
    private static string GetSerializationName(ToolStripItem item)
    {
        if (item.Tag is ToolStripMenuItem srcMenuItem && !string.IsNullOrWhiteSpace(srcMenuItem.Name))
        {
            return srcMenuItem.Name;
        }

        if (item.Tag is ToolStripItem srcItem && !string.IsNullOrWhiteSpace(srcItem.Name)
            && !srcItem.Name.StartsWith("clone_", StringComparison.Ordinal))
        {
            return srcItem.Name;
        }

        return item.Name ?? string.Empty;
    }

    // Resolve the final ToolStripItem list for one toolbar from its wrapper list.
    // Pure: reads _originalItems and _toolbarItems but does not mutate any ToolStrip.
    private List<ResolvedToolbarItem> BuildResolvedItems(
        string toolbarName,
        List<ToolStripItemWrapper> wrappers)
    {
        List<ResolvedToolbarItem> result = new(wrappers.Count);
        int order = 0;

        foreach (ToolStripItemWrapper wrapper in wrappers)
        {
            if (wrapper.Item == null)
            {
                (ToolStripItem? item, string? configName) = ResolveNullWrapper(wrapper, order);
                if (item == null)
                {
                    order++;
                    continue;
                }

                result.Add(new ResolvedToolbarItem(item, configName!, wrapper.ShowText, wrapper.DisplayName));
            }
            else
            {
                (ToolStripItem item, string configName) = ResolveRealWrapper(wrapper, toolbarName);
                result.Add(new ResolvedToolbarItem(item, configName, wrapper.ShowText, wrapper.DisplayName));
            }

            order++;
        }

        return result;
    }

    private static (ToolStripItem? item, string? configName) ResolveNullWrapper(ToolStripItemWrapper wrapper, int order)
    {
        if (wrapper.DisplayName == SeparatorDisplayName)
        {
            return (new ToolStripSeparator(), $"_SEPARATOR_{order}");
        }

        if (wrapper.DisplayName == ExpandingSpacerDisplayName)
        {
            // Match the startup path (ApplyLayoutToToolStrip) and the configName so a freshly
            // applied spacer carries the same Name as one rebuilt from config; otherwise the
            // View > Toolbars submenu would show the raw Name until the next restart.
            ToolStripLabel spacer = new()
            {
                Name = $"_SPACER_{order}",
                AutoSize = true,
                Text = "     ",
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            return (spacer, $"_SPACER_{order}");
        }

        if (wrapper.DisplayName.StartsWith(LabelDisplayNamePrefix) && wrapper.DisplayName.EndsWith(LabelDisplayNameSuffix))
        {
            string labelText = wrapper.DisplayName.Substring(10, wrapper.DisplayName.Length - 14);
            ToolStripLabel label = new()
            {
                Name = $"editableLabel_{order}",
                Text = labelText,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                AutoSize = true
            };
            return (label, $"_LABEL_{Uri.EscapeDataString(labelText)}_{order}");
        }

        return (null, null);
    }

    private (ToolStripItem item, string configName) ResolveRealWrapper(ToolStripItemWrapper wrapper, string toolbarName)
    {
        // Resolve the canonical original from _originalItems when possible.
        // For a ToolStripPushButtonClone (or any clone whose Tag is the original),
        // look up by the original's name so ApplyChangesCore always clones from the
        // original ToolStripPushButton rather than from the clone itself.
        string lookupName = wrapper.Item!.Name ?? string.Empty;
        if (wrapper.Item is ToolStripPushButtonClone cloneItem
            && cloneItem.Tag is ToolStripPushButton pushOriginal)
        {
            lookupName = pushOriginal.Name ?? lookupName;
        }

        ToolStripItem item = !string.IsNullOrWhiteSpace(lookupName) && _originalItems.TryGetValue(lookupName, out ToolStripItem? original)
            ? original
            : wrapper.Item!;

        if (toolbarName.StartsWith(CustomToolbarDisplayPrefix) && item is ToolStripMenuItem menuItem)
        {
            item = ConvertMenuItemToButton(menuItem);
        }

        if (item is not ToolStripSeparator && item.Image == null)
        {
            item.Image = GitUI.Properties.Images.ApplicationBlue;
        }

        return (item, GetSerializationName(item));
    }

    // Returns true when every non-separator, non-spacer item in the toolbar shows its text label.
    private static bool AllRealItemsShowText(ToolStrip toolStrip)
    {
        List<ToolStripItem> realItems = toolStrip.Items
            .Cast<ToolStripItem>()
            .Where(i => i is not ToolStripSeparator and not ToolStripLabel)
            .ToList();

        return realItems.Count > 0
            && realItems.All(GetItemShowText);
    }

    // For IPushLabelItem (ToolStripPushButton or ToolStripPushButtonClone), ShowLabel is the
    // authoritative source: DisplayStyle is always ImageAndText when ahead/behind data is
    // displayed, regardless of whether the user enabled the text label. Each instance carries
    // its own ShowLabel so no resolution to the original is needed.
    private static bool GetItemShowText(ToolStripItem item)
    {
        if (item is IPushLabelItem pushItem)
        {
            return pushItem.ShowLabel;
        }

        return item.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText;
    }

    private void EnsureOrphanCustomToolstripsJoinedToPanel(List<string> allToolbarNames)
    {
        if (_formBrowse == null)
        {
            return;
        }

        ToolStripContainer? toolPanel = _formBrowse.Controls.OfType<ToolStripContainer>().FirstOrDefault();
        if (toolPanel == null)
        {
            return;
        }

        foreach (string toolbarName in allToolbarNames)
        {
            if (!_dynamicToolbars.TryGetValue(toolbarName, out ToolStrip? toolStrip) || toolStrip.Parent != null)
            {
                continue;
            }

            toolPanel.TopToolStripPanel.Join(toolStrip, toolPanel.TopToolStripPanel.Rows.Length);
        }
    }

    private ToolStrip? GetToolStripByName(string name)
    {
        if (_formBrowse == null)
        {
            return null;
        }

        return name switch
        {
            StandardToolbarName => _formBrowse.ToolStripMain,
            FiltersToolbarName => _formBrowse.ToolStripFilters,
            ScriptsToolbarName => _formBrowse.ToolStripScripts,
            _ => _dynamicToolbars.TryGetValue(name, out ToolStrip? toolStrip) ? toolStrip : null
        };
    }

    private ToolStripItem ConvertMenuItemToButton(ToolStripMenuItem menuItem)
        => ToolbarItemConverter.Convert(
            menuItem,
            ToolStripItemDisplayStyle.ImageAndText,
            _originalItems);

    private int GetToolbarIndex(string name)
    {
        if (name == StandardToolbarName)
        {
            return 0;
        }

        if (name == FiltersToolbarName)
        {
            return 1;
        }

        if (name == ScriptsToolbarName)
        {
            return 2;
        }

        if (name.StartsWith(CustomToolbarDisplayPrefix) && int.TryParse(name.Substring(7), out int number))
        {
            return 2 + number; // Custom 01 → 3, Custom 02 → 4, etc.
        }

        // Non-numeric names: fall back to the position in comboBoxToolbar.
        int pos = comboBoxToolbar.Items.IndexOf(name);
        return pos >= 0 ? pos : comboBoxToolbar.Items.Count;
    }

    private sealed class ToolStripItemWrapper
    {
        private static readonly HashSet<string> DynamicTextItems = new()
        {
            "_NO_TRANSLATE_WorkingDir",
            "WorkingDirectoryToolStripSplitButton",
            "toolStripButtonPush",
            "branchSelect"
        };

        private static readonly Dictionary<string, string> FriendlyNames = new()
        {
            { "RefreshButton", "Refresh" },
            { "toggleLeftPanel", "Toggle left panel" },
            { "toggleSplitViewLayout", "Toggle split view layout" },
            { "menuCommitInfoPosition", "Commit info position" },
            { "toolStripButtonLevelUp", "Submodules" },
            { "_NO_TRANSLATE_WorkingDir", "Change working directory" },
            { "WorkingDirectoryToolStripSplitButton", "Change working directory" },
            { "branchSelect", "Select branch" },
            { "toolStripSplitStash", "Manage stashes" },
            { "toolStripButtonCommit", "Commit" },
            { "toolStripButtonPull", "Pull" },
            { "toolStripButtonPush", "Push" },
            { "toolStripFileExplorer", "File Explorer" },
            { "userShell", "Bash" },
            { "EditSettings", "Settings" },
            { "tsbShowReflog", "Show all reflog references" },
            { "tsbRevisionFilter", "Advanced filter" },
            { "tsbtnAdvancedFilter", "Advanced filter" },
            { "tsmiShowOnlyFirstParent", "Show only first parent" },
            { "ToolStripLabel1", "Branch type" },
            { "ToolStripLabel2", "Filter type" },
            { "tscboBranchFilter", "Show all branches" },
            { "toolStripTextBoxFilter", "Text Filter" }
        };

        public ToolStripItem? Item { get; }
        public string DisplayName { get; }

        // Tracks whether this item should display its text label next to its icon.
        // Stored here so the intent survives item-type mismatches (e.g. ToolStripMenuItem
        // in the wrapper vs. ToolStripButton in the live toolbar).
        public bool ShowText { get; set; }

        public ToolStripItemWrapper(ToolStripItem? item, string? displayName = null)
        {
            Item = item;
            DisplayName = displayName ?? GetDisplayName(item);
            ShowText = item is not null and not ToolStripSeparator and not ToolStripLabel
                && GetItemShowText(item);
        }

        private static string GetDisplayName(ToolStripItem? item)
        {
            if (item is null)
            {
                return "Unknown";
            }

            if (item is ToolStripSeparator)
            {
                return SeparatorDisplayName;
            }

            // Clones store their original in Tag; render the original's display name.
            if (item.Name is not null && item.Name.StartsWith("clone_", StringComparison.Ordinal)
                && item.Tag is ToolStripItem original)
            {
                return GetDisplayName(original);
            }

            if (!string.IsNullOrWhiteSpace(item.Name) && DynamicTextItems.Contains(item.Name))
            {
                return GetFriendlyName(item.Name);
            }

            if (!string.IsNullOrWhiteSpace(item.Text))
            {
                string text = item.Text.Replace("&", "");

                if (text.Length >= 3 && text[1] == ':' && text[2] == '\\')
                {
                    return "Change working directory";
                }

                if (Regex.IsMatch(text, @"^\d+\s*[↑↓]+", RegexOptions.None, TimeSpan.FromSeconds(1)))
                {
                    return "Push";
                }

                return text;
            }

            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                return GetFriendlyName(item.Name);
            }

            return $"[{item.GetType().Name}]";
        }

        private static string GetFriendlyName(string name)
        {
            if (FriendlyNames.TryGetValue(name, out string? friendlyName))
            {
                return friendlyName;
            }

            return ConvertToReadable(name);
        }

        private static string ConvertToReadable(string name)
        {
            name = name.Replace("toolStrip", "")
                      .Replace("ToolStrip", "")
                      .Replace("Button", "")
                      .Replace("Split", "")
                      .Replace("tsb", "")
                      .Replace("tsmi", "");

            var result = Regex.Replace(name, "([a-z])([A-Z])", "$1 $2", RegexOptions.None, TimeSpan.FromSeconds(1));
            result = Regex.Replace(result, "([A-Z]+)([A-Z][a-z])", "$1 $2", RegexOptions.None, TimeSpan.FromSeconds(1));

            if (result.Length > 0)
            {
                result = char.ToUpper(result[0]) + result.Substring(1);
            }

            return result;
        }

        public override string ToString() => DisplayName;
    }

    private void ComboBoxCategory_SelectedIndexChanged(object? sender, EventArgs e)
    {
        textBoxFilterAvailable.Clear();
        FilterAvailableActionsByCategory();
    }

    private void UpdateButtonAddAllEnabled()
    {
        string? selectedCategory = comboBoxCategory.SelectedItem?.ToString();
        if (selectedCategory == AllActionsCategory)
        {
            buttonAddAll.Enabled = false;
            return;
        }

        buttonAddAll.Enabled = listBoxAvailable.Items.Cast<ToolStripItemWrapper>()
            .Any(w => w.DisplayName != SeparatorDisplayName && w.DisplayName != ExpandingSpacerDisplayName);
    }

    private void FilterAvailableActionsByCategory()
    {
        if (_formBrowse == null)
        {
            return;
        }

        string? selectedCategory = comboBoxCategory.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedCategory))
        {
            return;
        }

        MenuStrip? mainMenu = _formBrowse.mainMenuStrip;
        if (mainMenu == null)
        {
            return;
        }

        listBoxAvailable.BeginUpdate();
        listBoxAvailable.Items.Clear();
        listBoxAvailable.Tag = null;

        HashSet<string> addedItemNames = new();
        HashSet<string> noFilter = new();

        if (selectedCategory == AllActionsCategory)
        {
            PopulateAllActionsCategory(mainMenu, noFilter, addedItemNames);
        }
        else if (selectedCategory == "Default Standard toolbar")
        {
            AddItemsFromSnapshot(_defaultStandardItems, noFilter, addedItemNames);
        }
        else if (selectedCategory == "Default Filters toolbar")
        {
            AddItemsFromSnapshot(_defaultFiltersItems, noFilter, addedItemNames);
        }
        else if (selectedCategory == "Right click menu")
        {
            ContextMenuStrip? revisionContextMenu = _formBrowse.RevisionGridControl?.MainContextMenu;
            if (revisionContextMenu != null)
            {
                AddRightClickMenuItems(revisionContextMenu, addedItemNames);
            }
        }
        else
        {
            PopulateMenuCategoryItems(mainMenu, selectedCategory, noFilter, addedItemNames);
        }

        listBoxAvailable.EndUpdate();
        UpdateButtonAddAllEnabled();
    }

    private void PopulateAllActionsCategory(MenuStrip mainMenu, HashSet<string> noFilter, HashSet<string> addedItemNames)
    {
        listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, SeparatorDisplayName));
        listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, ExpandingSpacerDisplayName));
        listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, "--- custom label ---"));

        foreach (ToolStripMenuItem menuItem in mainMenu.Items.OfType<ToolStripMenuItem>())
        {
            AddTopLevelMenuAsAction(menuItem, addedItemNames);
            AddMenuItemsRecursively(menuItem, noFilter, addedItemNames, includeSeparators: false);
        }

        AddToolbarOnlyItems(noFilter, addedItemNames);

        ContextMenuStrip? revisionContextMenu = _formBrowse!.RevisionGridControl?.MainContextMenu;
        if (revisionContextMenu != null)
        {
            AddContextMenuItems(revisionContextMenu, noFilter, addedItemNames);
        }

        ContextMenuStrip? repoObjectsTreeContextMenu = _formBrowse.RepoObjectsTreeContextMenu;
        if (repoObjectsTreeContextMenu != null)
        {
            AddContextMenuItems(repoObjectsTreeContextMenu, noFilter, addedItemNames);
        }
    }

    private void PopulateMenuCategoryItems(MenuStrip mainMenu, string selectedCategory, HashSet<string> noFilter, HashSet<string> addedItemNames)
    {
        // Map category label → control Name (robust to UI translations).
        string? menuControlName = selectedCategory switch
        {
            "Start" => "fileToolStripMenuItem",
            "Repository" => "repositoryToolStripMenuItem",
            "Navigate" => "navigateToolStripMenuItem",
            "View" => "viewToolStripMenuItem",
            "Commands" => "commandsToolStripMenuItem",
            "GitHub" => "_repositoryHostsToolStripMenuItem",
            "Plugins" => "pluginsToolStripMenuItem",
            "Tools" => "toolsToolStripMenuItem",
            "Help" => "helpToolStripMenuItem",
            _ => null
        };

        if (menuControlName is null)
        {
            return;
        }

        ToolStripMenuItem? categoryMenu = mainMenu.Items.OfType<ToolStripMenuItem>()
            .FirstOrDefault(item => item.Name == menuControlName);

        if (categoryMenu != null)
        {
            AddTopLevelMenuAsAction(categoryMenu, addedItemNames);
            AddMenuItemsRecursively(categoryMenu, noFilter, addedItemNames, includeSeparators: true);
        }
    }

    private void AddItemsFromToolbar(ToolStrip? toolbar, HashSet<string> usedItemNames, HashSet<string> addedItemNames, bool includeSeparators = false, bool applyExclusions = false)
    {
        if (toolbar == null)
        {
            return;
        }

        bool pendingSeparator = false;
        foreach (ToolStripItem item in toolbar.Items)
        {
            if (item is ToolStripSeparator)
            {
                if (includeSeparators && listBoxAvailable.Items.Count > 0)
                {
                    pendingSeparator = true;
                }

                continue;
            }

            if (!string.IsNullOrWhiteSpace(item.Name) &&
                !item.Name.StartsWith("clone_", StringComparison.Ordinal) &&
                !usedItemNames.Contains(item.Name) &&
                !addedItemNames.Contains(item.Name) &&
                (!applyExclusions || !_excludedFromAllActions.Contains(item.Name)))
            {
                if (pendingSeparator)
                {
                    AddSeparatorIfNeeded();
                    pendingSeparator = false;
                }

                listBoxAvailable.Items.Add(new ToolStripItemWrapper(item));
                addedItemNames.Add(item.Name);
            }
        }
    }

    private void AddItemsFromSnapshot(List<ToolStripItemWrapper> snapshot, HashSet<string> usedItemNames, HashSet<string> addedItemNames)
    {
        bool pendingSeparator = false;
        foreach (ToolStripItemWrapper wrapper in snapshot)
        {
            AddSnapshotWrapper(wrapper, usedItemNames, addedItemNames, ref pendingSeparator);
        }
    }

    private void AddSnapshotWrapper(ToolStripItemWrapper wrapper, HashSet<string> usedItemNames, HashSet<string> addedItemNames, ref bool pendingSeparator)
    {
        if (wrapper.DisplayName == SeparatorDisplayName || wrapper.DisplayName == ExpandingSpacerDisplayName)
        {
            if (listBoxAvailable.Items.Count > 0)
            {
                pendingSeparator = true;
            }

            return;
        }

        if (wrapper.DisplayName.StartsWith(LabelDisplayNamePrefix) && wrapper.DisplayName.EndsWith(LabelDisplayNameSuffix))
        {
            if (pendingSeparator)
            {
                AddSeparatorIfNeeded();
                pendingSeparator = false;
            }

            listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, wrapper.DisplayName));
            return;
        }

        if (wrapper.Item != null &&
            !string.IsNullOrWhiteSpace(wrapper.Item.Name) &&
            !usedItemNames.Contains(wrapper.Item.Name) &&
            !addedItemNames.Contains(wrapper.Item.Name))
        {
            if (pendingSeparator)
            {
                AddSeparatorIfNeeded();
                pendingSeparator = false;
            }

            listBoxAvailable.Items.Add(new ToolStripItemWrapper(wrapper.Item));
            addedItemNames.Add(wrapper.Item.Name);
        }
    }

    // Populates listBoxAvailable for the "Right click menu" category.
    // Mirrors the real context menu structure: groups separated by separators,
    // sub-menu children prefixed with "ParentMenu > child".
    private void AddRightClickMenuItems(ContextMenuStrip contextMenu, HashSet<string> addedItemNames)
    {
        // Ordered list of (itemName, separatorBefore) pairs, matching the visible context menu.
        // Items excluded from the preset (bisect, stash, branch push/rename/delete, tag delete,
        // select-in-panel, build report, pull request, other actions) are simply omitted.
        (string name, bool sepBefore)[] layout =
        [
            ("copyToClipboardToolStripMenuItem",          false),
            ("mergeBranchToolStripMenuItem",              true),
            ("rebaseOnToolStripMenuItem",                 false),
            ("resetCurrentBranchToHereToolStripMenuItem", false),
            ("createNewBranchToolStripMenuItem",          true),
            ("resetAnotherBranchToHereToolStripMenuItem", false),
            ("createTagToolStripMenuItem",                false),
            ("checkoutRevisionToolStripMenuItem",         true),
            ("revertCommitToolStripMenuItem",             false),
            ("cherryPickCommitToolStripMenuItem",         false),
            ("archiveRevisionToolStripMenuItem",          false),
            ("manipulateCommitToolStripMenuItem",         false),
            ("compareToolStripMenuItem",                  true),
            ("navigateToolStripMenuItem",                 true),
            ("viewToolStripMenuItem",                     false),
            ("runScriptToolStripMenuItem",                false),
        ];

        Dictionary<string, ToolStripMenuItem> byName = contextMenu.Items
            .OfType<ToolStripMenuItem>()
            .ToDictionary(i => i.Name ?? string.Empty, i => i);

        foreach ((string name, bool sepBefore) in layout)
        {
            if (!byName.TryGetValue(name, out ToolStripMenuItem? menuItem))
            {
                continue;
            }

            if (sepBefore && listBoxAvailable.Items.Count > 0)
            {
                listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, SeparatorDisplayName));
            }

            if (!addedItemNames.Contains(menuItem.Name ?? string.Empty))
            {
                listBoxAvailable.Items.Add(new ToolStripItemWrapper(menuItem));
                addedItemNames.Add(menuItem.Name ?? string.Empty);
            }

            // Add sub-items prefixed with "ParentMenu > child"
            string parentLabel = menuItem.Text?.Replace("&", "") ?? string.Empty;
            AddRightClickSubItems(menuItem, parentLabel, addedItemNames);
        }
    }

    private void AddRightClickSubItems(ToolStripMenuItem menuItem, string parentLabel, HashSet<string> addedItemNames)
    {
        foreach (ToolStripItem subItem in menuItem.DropDownItems)
        {
            if (subItem is not ToolStripMenuItem subMenuItem ||
                (!subMenuItem.Enabled && subMenuItem.Font.Italic) ||
                string.IsNullOrWhiteSpace(subMenuItem.Name) ||
                string.IsNullOrWhiteSpace(subMenuItem.Text) ||
                subMenuItem.Text.StartsWith("-") ||
                addedItemNames.Contains(subMenuItem.Name))
            {
                continue;
            }

            string displayName = $"{parentLabel} > {subMenuItem.Text.Replace("&", "")}";
            listBoxAvailable.Items.Add(new ToolStripItemWrapper(subMenuItem, displayName));
            addedItemNames.Add(subMenuItem.Name);

            // One level deeper (e.g. Compare > Open selected commits with difftool > ...)
            string subLabel = subMenuItem.Text.Replace("&", "");
            foreach (ToolStripItem grandChild in subMenuItem.DropDownItems)
            {
                if (grandChild is not ToolStripMenuItem grandMenuItem ||
                    (!grandMenuItem.Enabled && grandMenuItem.Font.Italic) ||
                    string.IsNullOrWhiteSpace(grandMenuItem.Name) ||
                    string.IsNullOrWhiteSpace(grandMenuItem.Text) ||
                    grandMenuItem.Text.StartsWith("-") ||
                    addedItemNames.Contains(grandMenuItem.Name))
                {
                    continue;
                }

                string grandDisplayName = $"{parentLabel} > {subLabel} > {grandMenuItem.Text.Replace("&", "")}";
                listBoxAvailable.Items.Add(new ToolStripItemWrapper(grandMenuItem, grandDisplayName));
                addedItemNames.Add(grandMenuItem.Name);
            }
        }
    }

    private void AddContextMenuItems(ContextMenuStrip contextMenu, HashSet<string> usedItemNames, HashSet<string> addedItemNames)
    {
        foreach (ToolStripItem item in contextMenu.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                AddContextMenuItemRecursively(menuItem, usedItemNames, addedItemNames);
            }
        }
    }

    private void AddContextMenuItemRecursively(ToolStripMenuItem menuItem, HashSet<string> usedItemNames, HashSet<string> addedItemNames)
    {
        if (!string.IsNullOrWhiteSpace(menuItem.Name) &&
            !string.IsNullOrWhiteSpace(menuItem.Text) &&
            !menuItem.Text.StartsWith("-") &&
            !usedItemNames.Contains(menuItem.Name) &&
            !addedItemNames.Contains(menuItem.Name))
        {
            listBoxAvailable.Items.Add(new ToolStripItemWrapper(menuItem));
            addedItemNames.Add(menuItem.Name);
        }

        foreach (ToolStripItem subItem in menuItem.DropDownItems)
        {
            if (subItem is ToolStripMenuItem subMenuItem)
            {
                AddContextMenuItemRecursively(subMenuItem, usedItemNames, addedItemNames);
            }
        }
    }

    private void AddToolbarOnlyItems(HashSet<string> usedItemNames, HashSet<string> addedItemNames)
    {
        if (_formBrowse == null)
        {
            return;
        }

        ToolStrip[] allToolbars = { _formBrowse.ToolStripMain, _formBrowse.ToolStripFilters, _formBrowse.ToolStripScripts };

        foreach (ToolStrip toolbar in allToolbars)
        {
            AddItemsFromToolbar(toolbar, usedItemNames, addedItemNames, applyExclusions: true);

            foreach (ToolStripItem item in toolbar.Items)
            {
                if (item is ToolStripSplitButton or ToolStripDropDownButton)
                {
                    AddDropDownSubItems((ToolStripDropDownItem)item, usedItemNames, addedItemNames);
                }
            }
        }
    }

    private void AddDropDownSubItems(ToolStripDropDownItem dropDown, HashSet<string> usedItemNames, HashSet<string> addedItemNames)
    {
        foreach (ToolStripItem subItem in dropDown.DropDownItems)
        {
            if (subItem is ToolStripMenuItem subMenuItem &&
                !string.IsNullOrWhiteSpace(subMenuItem.Name) &&
                !subMenuItem.Name.StartsWith(FormBrowse.FetchPullToolbarShortcutsPrefix, StringComparison.Ordinal) &&
                subMenuItem.Name != "setDefaultPullButtonActionToolStripMenuItem" &&
                !usedItemNames.Contains(subMenuItem.Name) &&
                !addedItemNames.Contains(subMenuItem.Name))
            {
                listBoxAvailable.Items.Add(new ToolStripItemWrapper(subMenuItem));
                addedItemNames.Add(subMenuItem.Name);
            }
        }
    }

    private void AddTopLevelMenuAsAction(ToolStripMenuItem menuItem, HashSet<string> addedItemNames)
    {
        if (string.IsNullOrWhiteSpace(menuItem.Name) || addedItemNames.Contains(menuItem.Name))
        {
            return;
        }

        listBoxAvailable.Items.Add(new ToolStripItemWrapper(menuItem));
        addedItemNames.Add(menuItem.Name);
        AddSeparatorIfNeeded();
    }

    // Adds a separator unless the list is empty or already ends with one,
    // avoiding consecutive separators when surrounding items get filtered out.
    private void AddSeparatorIfNeeded()
    {
        if (listBoxAvailable.Items.Count == 0)
        {
            return;
        }

        if (listBoxAvailable.Items[^1] is ToolStripItemWrapper { DisplayName: SeparatorDisplayName })
        {
            return;
        }

        listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, SeparatorDisplayName));
    }

    // Groups the two name sets that are always passed together while walking menu items,
    // to keep AddMenuSubItem's parameter count within the SonarCloud S107 limit.
    private sealed record MenuItemNameSets(HashSet<string> UsedItemNames, HashSet<string> AddedItemNames);

    private void AddMenuItemsRecursively(ToolStripMenuItem menuItem, HashSet<string> usedItemNames, HashSet<string> addedItemNames, string? parentName = null, bool includeSeparators = false)
    {
        MenuItemNameSets nameSets = new(usedItemNames, addedItemNames);
        string? selectedCategory = comboBoxCategory.SelectedItem?.ToString();
        string[] hiddenSeparators = { "clearRecentRepositoriesListToolStripMenuItem", "toolbarsCustomizeSeparator" };

        bool pendingSeparator = false;
        foreach (ToolStripItem subItem in menuItem.DropDownItems)
        {
            if (subItem is ToolStripSeparator)
            {
                if (includeSeparators && listBoxAvailable.Items.Count > 0 && !hiddenSeparators.Contains(subItem.Name))
                {
                    pendingSeparator = true;
                }

                continue;
            }

            if (subItem is ToolStripMenuItem subMenuItem && !string.IsNullOrEmpty(subMenuItem.Text))
            {
                AddMenuSubItem(subMenuItem, menuItem, nameSets, parentName, includeSeparators, selectedCategory, ref pendingSeparator);
            }
        }
    }

    private void AddMenuSubItem(
        ToolStripMenuItem subMenuItem,
        ToolStripMenuItem parentMenuItem,
        MenuItemNameSets nameSets,
        string? parentName,
        bool includeSeparators,
        string? selectedCategory,
        ref bool pendingSeparator)
    {
        if (string.IsNullOrEmpty(subMenuItem.Text) || subMenuItem.Text.StartsWith("-"))
        {
            return;
        }

        string currentParent = parentName ?? parentMenuItem.Text?.Replace("&", "") ?? string.Empty;

        if (ShouldSkipItem(subMenuItem, selectedCategory))
        {
            return;
        }

        if (selectedCategory == AllActionsCategory && _excludedFromAllActions.Contains(subMenuItem.Name ?? string.Empty))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(subMenuItem.Name) &&
            !nameSets.UsedItemNames.Contains(subMenuItem.Name) &&
            !nameSets.AddedItemNames.Contains(subMenuItem.Name))
        {
            if (pendingSeparator)
            {
                AddSeparatorIfNeeded();
                pendingSeparator = false;
            }

            string displayName = GetModifiedDisplayName(subMenuItem, currentParent);
            ToolStripItemWrapper wrapper = string.IsNullOrEmpty(displayName)
                ? new ToolStripItemWrapper(subMenuItem)
                : new ToolStripItemWrapper(subMenuItem, displayName);

            listBoxAvailable.Items.Add(wrapper);
            nameSets.AddedItemNames.Add(subMenuItem.Name);
        }

        if (subMenuItem.DropDownItems.Count > 0)
        {
            AddMenuItemsRecursively(subMenuItem, nameSets.UsedItemNames, nameSets.AddedItemNames, subMenuItem.Text?.Replace("&", ""), includeSeparators);
        }
    }

    private static bool ShouldSkipItem(ToolStripMenuItem item, string? category)
    {
        string itemText = item.Text?.Replace("&", "") ?? string.Empty;

        if (itemText == "...")
        {
            return true;
        }

        if (category == "View")
        {
            string[] excludedItems = { "Branches", "Commits", "Grid labels", "Grid info", "Columns", "Sorting", "Settings persistence" };
            if (excludedItems.Contains(itemText))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetModifiedDisplayName(ToolStripMenuItem item, string parentName)
    {
        string itemText = item.Text?.Replace("&", "") ?? string.Empty;

        if (parentName == "Git maintenance")
        {
            return $"Git maintenance > {itemText}";
        }

        if (parentName == "PuTTY")
        {
            return $"PuTTY > {itemText}";
        }

        if (parentName == "Recent repositories")
        {
            return $"Recent repositories > {itemText}";
        }

        if (parentName == "Toolbars")
        {
            return $"Toolbars > {itemText}";
        }

        return string.Empty;
    }

    private static void TextBox_TripleClick(object? sender, MouseEventArgs e)
    {
        if (sender is TextBox textBox && e.Clicks >= 3)
        {
            textBox.SelectAll();
        }
    }

    private void ButtonClearAvailableFilter_Click(object? sender, EventArgs e)
    {
        textBoxFilterAvailable.Clear();
    }

    private void ButtonClearCurrentFilter_Click(object? sender, EventArgs e)
    {
        textBoxFilterCurrent.Clear();
    }

    private void ListBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ListBox listBox || e.Index < 0 || e.Index >= listBox.Items.Count)
        {
            return;
        }

        e.DrawBackground();

        if (listBox.Items[e.Index] is not ToolStripItemWrapper wrapper)
        {
            return;
        }

        bool isEditableLabel = wrapper.DisplayName.StartsWith(LabelDisplayNamePrefix) && wrapper.DisplayName.EndsWith(LabelDisplayNameSuffix);
        bool isEditableLabelAction = wrapper.DisplayName == "--- custom label ---";
        bool isSeparatorItem = wrapper.DisplayName == SeparatorDisplayName || wrapper.DisplayName == ExpandingSpacerDisplayName;

        Image? icon = null;
        if (!isSeparatorItem && !isEditableLabel && !isEditableLabelAction)
        {
            icon = wrapper.Item?.Image ?? GitUI.Properties.Images.ApplicationBlue;
        }

        const int iconSize = 20;
        const int iconPadding = 2;
        Rectangle iconRect = new(
            e.Bounds.Left + iconPadding,
            e.Bounds.Top + ((e.Bounds.Height - iconSize) / 2),
            iconSize,
            iconSize);

        if (icon != null)
        {
            e.Graphics.DrawImage(icon, iconRect);
        }

        int textLeft = icon != null ? iconRect.Right + iconPadding : e.Bounds.Left + iconPadding;
        Rectangle textRect = new(
            textLeft,
            e.Bounds.Top,
            e.Bounds.Width - textLeft,
            e.Bounds.Height);

        using SolidBrush? separatorBrush = isSeparatorItem ? new SolidBrush(Color.FromArgb(0x80, 0x80, 0x80)) : null;
        Brush textBrush = GetListItemBrush(e, isSeparatorItem, separatorBrush);

        using StringFormat stringFormat = new()
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };

        bool showAsterisk = listBox == listBoxCurrent &&
            wrapper.Item is not null and not ToolStripSeparator and not ToolStripLabel &&
            wrapper.ShowText;

        string rawDisplayText = GetDisplayText(wrapper, isEditableLabel, isEditableLabelAction);
        string displayText = showAsterisk ? rawDisplayText + " *" : rawDisplayText;

        e.Graphics.DrawString(
            displayText,
            e.Font ?? listBox.Font,
            textBrush,
            textRect,
            stringFormat);

        e.DrawFocusRectangle();
    }

    private static string GetDisplayText(ToolStripItemWrapper wrapper, bool isEditableLabel, bool isEditableLabelAction)
    {
        if (isEditableLabel)
        {
            return $"[Label] {wrapper.DisplayName.Substring(10, wrapper.DisplayName.Length - 14)}";
        }

        if (isEditableLabelAction)
        {
            return "Custom label";
        }

        return wrapper.DisplayName;
    }

    private static Brush GetListItemBrush(DrawItemEventArgs e, bool isSeparatorItem, SolidBrush? separatorBrush)
    {
        if (isSeparatorItem)
        {
            return separatorBrush!;
        }

        return (e.State & DrawItemState.Selected) != 0
            ? SystemBrushes.HighlightText
            : SystemBrushes.WindowText;
    }

    private void ButtonLocateToolbar_Click(object? sender, EventArgs e)
    {
        ToolStrip? toolbar = GetToolStripByName(_currentToolbarName);

        if (toolbar == null)
        {
            MessageBoxes.Show(
                "Could not find the selected toolbar.",
                "Locate Toolbar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _ = FlashToolbarAsync(toolbar);
    }

    private async Task FlashToolbarAsync(ToolStrip toolbar)
    {
        // Ignore re-entrant flashes: a second request while an animation is still running would
        // capture an already blended (reddish) BackColor as the "original" colour and restore that
        // tint permanently. Let the current animation finish first.
        if (_isFlashing)
        {
            return;
        }

        _isFlashing = true;

        Color originalBackColor = toolbar.BackColor;

        const int flashDurationMs = 3000;
        const int stepDurationMs = 100;
        int steps = flashDurationMs / stepDurationMs;

        try
        {
            for (int i = 0; i < steps; i++)
            {
                if (toolbar.IsDisposed)
                {
                    return;
                }

                double progress = (double)i / steps;
                double sineWave = Math.Sin(progress * Math.PI * 4);
                int alpha = (int)(Math.Abs(sineWave) * 180);

                Color blended = BlendColors(originalBackColor, Color.FromArgb(alpha, Color.Red));
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

                // ConfigureAwait(true) preserves the WinForms SynchronizationContext so the
                // continuation resumes on the UI thread; the InvokeRequired guard above
                // handles the rare case where the context is not available.
                await Task.Delay(stepDurationMs).ConfigureAwait(true);
            }
        }
        finally
        {
            _isFlashing = false;

            if (!toolbar.IsDisposed)
            {
                if (toolbar.InvokeRequired)
                {
                    toolbar.BeginInvoke(() => toolbar.BackColor = originalBackColor);
                }
                else
                {
                    toolbar.BackColor = originalBackColor;
                }
            }
        }
    }

    private static Color BlendColors(Color background, Color overlay)
    {
        int alpha = overlay.A;
        int invAlpha = 255 - alpha;

        int r = ((overlay.R * alpha) + (background.R * invAlpha)) / 255;
        int g = ((overlay.G * alpha) + (background.G * invAlpha)) / 255;
        int b = ((overlay.B * alpha) + (background.B * invAlpha)) / 255;

        return Color.FromArgb(r, g, b);
    }
}
