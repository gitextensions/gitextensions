using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.UserControls.RevisionGrid;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.UserControls;

internal sealed partial class FilterToolBar : TranslatedControl
{
    private const string TranslationCategory = nameof(FormBrowse);
    private const int MaxFilterItems = 30;
    internal const string ReflogButtonName = nameof(tsbShowReflog);

    private static readonly (string Name, string Property, string Text)[] TranslationItems =
    [
        (nameof(tsbtnAdvancedFilter), "ToolTipText", "Advanced filter"),
        (nameof(toolStripLabel1), "Text", "&Branches:"),
        (nameof(toolStripLabel1), "ToolTipText", "Branch filter"),
        (nameof(tsddbtnBranchFilter), "Text", "Branch type"),
        (nameof(tslblRevisionFilter), "Text", "&Filter:"),
        (nameof(tslblRevisionFilter), "ToolTipText", "Text filter"),
        (nameof(tsddbtnRevisionFilter), "Text", "Filter type"),
        (nameof(tsmiResetPathFilters), "Text", "Reset &path filter"),
        (nameof(tsmiResetAllFilters), "Text", "&Reset revision filters"),
        (nameof(tsmiAdvancedFilter), "Text", "&Advanced filter"),
        (nameof(tsmiShowBranchesAll), "Text", "&All branches"),
        (nameof(tsmiShowBranchesAll), "ToolTipText", "Show all branches"),
        (nameof(tsmiShowBranchesCurrent), "Text", "&Current branch only"),
        (nameof(tsmiShowBranchesCurrent), "ToolTipText", "Show current branch only"),
        (nameof(tsmiShowBranchesFiltered), "Text", "&Filtered branches"),
        (nameof(tsmiShowBranchesFiltered), "ToolTipText", "Show filtered branches"),
        (nameof(tsmiBranchLocal), "Text", "&Local"),
        (nameof(tsmiBranchRemote), "Text", "&Remote"),
        (nameof(tsmiBranchTag), "Text", "&Tag"),
        (nameof(tsmiCommitFilter), "Text", "Commit &message"),
        (nameof(tsmiCommitterFilter), "Text", "&Committer"),
        (nameof(tsmiAuthorFilter), "Text", "&Author"),
        (nameof(tsmiDiffContainsFilter), "Text", "&Diff contains (SLOW)"),
        (nameof(tssbtnShowBranches), "Text", "&All branches"),
        (nameof(tssbtnShowBranches), "ToolTipText", "Show all branches"),
    ];

    private readonly List<string> _revisionFilters;

    private static readonly string[] _noResultsFound = [TranslatedStrings.NoResultsFound];
    private Func<IGitModule>? _getModule;
    private IRevisionGridFilter? _revisionGridFilter;
    private bool _isApplyingFilter;
    private bool _filterBeingChanged;
    private bool _updatingSuggestions;
    private string _advancedFilterToolTip = string.Empty;
    private Func<RefsFilter, IReadOnlyList<IGitRef>>? _getRefs;
    private Action<string>? _showInvalidReference;
    private string? _tslblRevisionFilterToolTip;

    public FilterToolBar()
    {
        InitializeComponent();

        // Select an option until we get a filter bound.
        tsbtnAdvancedFilter.Click += tsbtnAdvancedFilter_ButtonClick;
        tsmiResetPathFilters.Click += tsmiDisablePathFilters_Click;
        tsmiResetAllFilters.Click += tsmiDisableAllFilters_Click;
        tsmiAdvancedFilter.Click += tsmiAdvancedFilter_Click;
        tsbShowReflog.Click += tsmiShowReflog_Click;
        tssbtnShowBranches.Click += tssbtnShowBranches_Click;
        tsmiShowBranchesAll.Click += tsmiShowBranchesAll_Click;
        tsmiShowBranchesCurrent.Click += tsmiShowBranchesCurrent_Click;
        tsmiShowBranchesFiltered.Click += tsmiShowBranchesFiltered_Click;
        tsmiShowOnlyFirstParent.Click += tsmiShowOnlyFirstParent_Click;
        tscboBranchFilter.PointerPressed += tscboBranchFilter_Click;
        tscboBranchFilter.DropDownOpened += tscboBranchFilter_DropDown;
        tscboBranchFilter.KeyUp += tscboBranchFilter_KeyUp;
        tscboBranchFilter.PropertyChanged += tscboBranchFilter_TextChanged;
        tstxtRevisionFilter.KeyUp += tstxtRevisionFilter_KeyUp;
        tsmiBranchLocal.Click += (_, _) => UpdateBranchFilterItems();
        tsmiBranchRemote.Click += (_, _) => UpdateBranchFilterItems();
        tsmiBranchTag.Click += (_, _) => UpdateBranchFilterItems();
        tsmiCommitFilter.Click += revisionFilterBox_CheckedChanged;
        tsmiCommitterFilter.Click += revisionFilterBox_CheckedChanged;
        tsmiAuthorFilter.Click += revisionFilterBox_CheckedChanged;
        tsmiDiffContainsFilter.Click += revisionFilterBox_CheckedChanged;

        _revisionFilters = AppSettings.RevisionFilterDropdowns
            .Union([
                @"--invert-grep --grep=""EXCLUDE_COMMIT_MESSAGE_REGEX_PATTERN""",
                @"--perl-regexp --author=""^(?!.*EXCLUDE_AUTHOR_REGEX_PATTERN)""",
                @"--exclude=refs/remotes/EXCLUDE_REMOTE_REGEX_PATTERN",
            ], StringComparer.Ordinal)
            .ToList();
        RefreshRevisionFilterItems();

        ToolTip.SetTip(tsbShowReflog, TranslatedStrings.ShowReflogTooltip);
        ToolTip.SetTip(tsmiShowOnlyFirstParent, TranslatedStrings.ShowOnlyFirstParent);
        SetBranchMode(tsmiShowBranchesAll, Properties.Images.BranchLocal);
        InitializeComplete();
        _advancedFilterToolTip = ToolTip.GetTip(tsbtnAdvancedFilter)?.ToString() ?? string.Empty;
    }

    private IRevisionGridFilter RevisionGridFilter
        => _revisionGridFilter ?? throw new InvalidOperationException($"{nameof(Bind)} is not called.");

    /// <summary>
    ///  Applies the preset branch filters, such as "show all", "show current", and "show filtered".
    /// </summary>
    private void ApplyPresetBranchesFilter(Action filterAction)
    {
        _filterBeingChanged = true;

        // Action the filter
        filterAction();
        _filterBeingChanged = false;
    }

    /// <summary>
    ///  Applies custom branch filters supplied via the filter textbox.
    /// </summary>
    private void ApplyCustomBranchFilter(bool checkBranch = true)
    {
        if (_isApplyingFilter)
        {
            return;
        }

        _isApplyingFilter = true;
        try
        {
            // The user has accepted the filter
            _filterBeingChanged = false;

            // Apply the textbox contents, no check if the (multiple) options is in tscboBranchFilter.Items (or that the list is generated)
            string filter = tscboBranchFilter.Text == TranslatedStrings.NoResultsFound
                ? string.Empty
                : tscboBranchFilter.Text?.Trim() ?? string.Empty;
            if (checkBranch && !string.IsNullOrWhiteSpace(filter))
            {
                List<string> newFilter = [];
                IReadOnlyList<IGitRef> refs = _getRefs?.Invoke(RefsFilter.NoFilter) ?? GetModule().GetRefs(RefsFilter.NoFilter);

                // Split at whitespace (char[])null is default) but with split options.
                // Ignore quoting, Git revisions do not allow spaces.
                foreach (string branch in filter.Split(
                             (char[]?)null,
                             StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    bool wildcardBranchFilter = branch.IndexOfAny(Delimiters.WildcardBranchSearchValues) >= 0;
                    if (branch.StartsWith("--", StringComparison.Ordinal)
                        || refs.Any(gitRef => gitRef.LocalName == branch)
                        || branch.Contains("..", StringComparison.Ordinal))
                    {
                        // Added as git-log option or revision filter
                    }
                    else if (wildcardBranchFilter)
                    {
                        // Added as --branches= option
                    }
                    else
                    {
                        string gitRef = branch.StartsWith('^') ? branch[1..] : branch;
                        if (GetModule().RevParse(gitRef).IsZero)
                        {
                            ShowInvalidReference(branch);
                            continue;
                        }
                    }

                    newFilter.Add(branch);
                }

                filter = string.Join(" ", newFilter);
            }

            RevisionGridFilter.SetAndApplyBranchFilter(filter);
        }
        finally
        {
            _isApplyingFilter = false;
        }
    }

    private void ApplyRevisionFilter()
    {
        if (_isApplyingFilter)
        {
            return;
        }

        _isApplyingFilter = true;
        try
        {
            RevisionGridFilter.SetAndApplyRevisionFilter(new RevisionFilter(
                tstxtRevisionFilter.Text?.Trim() ?? string.Empty,
                tsmiCommitFilter.IsChecked,
                tsmiCommitterFilter.IsChecked,
                tsmiAuthorFilter.IsChecked,
                tsmiDiffContainsFilter.IsChecked));
        }
        finally
        {
            _isApplyingFilter = false;
        }
    }

    public void Bind(Func<IGitModule> getModule, IRevisionGridFilter revisionGridFilter)
    {
        ArgumentNullException.ThrowIfNull(getModule);
        ArgumentNullException.ThrowIfNull(revisionGridFilter);
        if (_revisionGridFilter is not null)
        {
            throw new InvalidOperationException($"{nameof(Bind)} must be invoked only once.");
        }

        _getModule = getModule;
        _revisionGridFilter = revisionGridFilter;
        _revisionGridFilter.FilterChanged += revisionGridFilter_FilterChanged;
    }

    public void ClearQuickFilters()
    {
        tscboBranchFilter.Text = string.Empty;
        tstxtRevisionFilter.Text = string.Empty;
    }

    private IGitModule GetModule()
    {
        if (_getModule is null)
        {
            throw new InvalidOperationException($"{nameof(Bind)} is not called.");
        }

        return _getModule() ?? throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
    }

    private void InitBranchSelectionFilter(FilterChangedEventArgs e)
    {
        // Note: it is a weird combination, and it is mimicking the implementations in RevisionGridControl.
        // Refer to it for more details.
        if (e.ShowFilteredBranches)
        {
            // Show filtered branches
            // Keep value if other filter
            tscboBranchFilter.Text = e.BranchFilter;
        }

        if (e.ShowCurrentBranchOnly)
        {
            // Show current branch only
            SelectShowBranchesFilterOption(selectedIndex: 1);
        }
        else if (e.ShowFilteredBranches)
        {
            SelectShowBranchesFilterOption(selectedIndex: 2);
        }
        else
        {
            // Show all branches
            SelectShowBranchesFilterOption(selectedIndex: 0);
        }
    }

    public void InitToolStripStyles(Avalonia.Media.Color toolForeColor, Avalonia.Media.Color toolBackColor)
    {
        Avalonia.Media.SolidColorBrush foreground = new(toolForeColor);
        Avalonia.Media.SolidColorBrush background = new(toolBackColor);
        tsddbtnRevisionFilter.Foreground = foreground;
        tsddbtnRevisionFilter.Background = background;
        tscboBranchFilter.Foreground = foreground;
        tscboBranchFilter.Background = background;
        tstxtRevisionFilter.Foreground = foreground;
        tstxtRevisionFilter.Background = background;
    }

    private void SelectShowBranchesFilterOption(int selectedIndex)
    {
        (MenuItem Item, Avalonia.Media.IImage Icon)[] options =
        [
            (tsmiShowBranchesAll, Properties.Images.BranchLocal),
            (tsmiShowBranchesCurrent, Properties.Images.BranchFilter),
            (tsmiShowBranchesFiltered, Properties.Images.BranchFilter),
        ];
        if ((uint)selectedIndex >= (uint)options.Length)
        {
            selectedIndex = 0;
        }

        (MenuItem item, Avalonia.Media.IImage icon) = options[selectedIndex];
        SetBranchMode(item, icon);
    }

    /// <summary>
    ///  Sets the branches filter.
    ///  No check that the branches exist (must be checked already, expected to be called from left panel).
    /// </summary>
    /// <param name="filter">The branches to filter separated by whitespace.</param>
    public void SetBranchFilter(string? filter)
    {
        tscboBranchFilter.Text = filter;
        ApplyCustomBranchFilter(checkBranch: false);
    }

    private static void SetInputGesture(
        MenuItem menuItem,
        IReadOnlyList<HotkeyCommand> hotkeys,
        RevisionGridControl.Command command)
    {
        WinFormsShims.Keys keys = hotkeys.FirstOrDefault(hotkey => hotkey.CommandCode == (int)command)?.KeyData
            ?? WinFormsShims.Keys.None;
        menuItem.InputGesture = KeysMapper.ToKeyGesture(keys);
    }

    /// <summary>
    /// If focus on branch filter, focus revision filter otherwise branch filter.
    /// </summary>
    public void SetFocus()
    {
        if (tstxtRevisionFilter.IsFocused)
        {
            tscboBranchFilter.Focus();
        }
        else
        {
            tstxtRevisionFilter.Focus();
        }
    }

    /// <summary>
    ///  Sets the revision filter.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    public void SetRevisionFilter(string? filter)
    {
        if (string.IsNullOrEmpty(tstxtRevisionFilter.Text) && string.IsNullOrEmpty(filter))
        {
            // The current filter is empty and the new filter is empty. No-op
            return;
        }

        tstxtRevisionFilter.Text = filter;
        ApplyRevisionFilter();
    }

    /// <summary>
    /// Update the function to get refs for branch dropdown filter
    /// </summary>
    /// <param name="getRefs">Function to get refs, expected to be cached</param>
    public void RefreshRevisionFunction(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        _getRefs = getRefs ?? throw new ArgumentNullException(nameof(getRefs));
        tscboBranchFilter.ItemsSource = Array.Empty<string>();
    }

    /// <summary>
    /// Update the tscboBranchFilter dropdown items matching the current filter.
    /// This is called when dropdown clicked or text is manually changed
    /// (so tscboBranchFilter.Items is not necessarily available when set externally
    /// from the left panel or FormBrowse).
    /// </summary>
    private void UpdateBranchFilterItems()
    {
        if (_getModule is null || !GetModule().IsValidGitWorkingDir())
        {
            IsEnabled = false;
            return;
        }

        IsEnabled = true;

        // Options are interpreted as the refs the search should be limited too
        // If neither option is selected all refs will be queried also including stash and notes
        RefsFilter filter = (tsmiBranchLocal.IsChecked ? RefsFilter.Heads : RefsFilter.NoFilter)
            | (tsmiBranchRemote.IsChecked ? RefsFilter.Remotes : RefsFilter.NoFilter)
            | (tsmiBranchTag.IsChecked ? RefsFilter.Tags : RefsFilter.NoFilter);
        string currentText = tscboBranchFilter.Text ?? string.Empty;
        string[] matches = (_getRefs?.Invoke(filter) ?? GetModule().GetRefs(filter))
            .Select(gitRef => gitRef.Name)
            .Distinct(StringComparer.Ordinal)
            .Where(branch => branch.Contains(currentText, StringComparison.InvariantCultureIgnoreCase))
            .Order(StringComparer.InvariantCulture)
            .ToArray();

        _updatingSuggestions = true;
        try
        {
            tscboBranchFilter.ItemsSource = matches.Length == 0
                ? _noResultsFound
                : matches;
            tscboBranchFilter.Text = currentText;
            tscboBranchFilter.IsDropDownOpen = true;
        }
        finally
        {
            _updatingSuggestions = false;
        }
    }

    public void SetShortcutKeys(Action<MenuItem, RevisionGridControl.Command> setShortcutString)
    {
        setShortcutString(tsmiResetPathFilters, RevisionGridControl.Command.ResetRevisionPathFilter);
        setShortcutString(tsmiResetAllFilters, RevisionGridControl.Command.ResetRevisionFilter);
        setShortcutString(tsmiAdvancedFilter, RevisionGridControl.Command.RevisionFilter);
    }

    private void PromoteRevisionFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)
            || (_revisionFilters.Count > 0 && _revisionFilters[0] == filter))
        {
            return;
        }

        _revisionFilters.Remove(filter);
        _revisionFilters.Insert(0, filter);
        AppSettings.RevisionFilterDropdowns = [.. _revisionFilters.Take(MaxFilterItems)];
        RefreshRevisionFilterItems();
        tstxtRevisionFilter.Text = filter;
    }

    private void RefreshRevisionFilterItems()
        => tstxtRevisionFilter.ItemsSource = _revisionFilters.ToArray();

    private void SetBranchMode(MenuItem source, Avalonia.Media.IImage icon)
    {
        tssbtnShowBranches.Content = source.Header;
        tssbtnShowBranches.Icon = icon;
        ToolTip.SetTip(tssbtnShowBranches, ToolTip.GetTip(source));
    }

    private void revisionGridFilter_FilterChanged(object? sender, FilterChangedEventArgs e)
    {
        _isApplyingFilter = true;
        try
        {
            tsmiShowOnlyFirstParent.IsChecked = e.ShowOnlyFirstParent;
            tsbShowReflog.IsChecked = e.ShowReflogReferences;
            InitBranchSelectionFilter(e);

            List<(string Filter, MenuItem MenuItem)> revisionFilters =
            [
                (e.MessageFilter, tsmiCommitFilter),
                (e.CommitterFilter, tsmiCommitterFilter),
                (e.AuthorFilter, tsmiAuthorFilter),
                (e.DiffContentFilter, tsmiDiffContainsFilter),
            ];

            // If there is no filter in filterInfo, clear text but retain checks
            tstxtRevisionFilter.Text = string.Empty;
            if (revisionFilters.Any(item => !string.IsNullOrWhiteSpace(item.Filter)))
            {
                foreach ((string filter, MenuItem menuItem) in revisionFilters)
                {
                    // Check the first menuitem that matches and following identical filters
                    bool selected = !string.IsNullOrWhiteSpace(filter)
                        && (string.IsNullOrWhiteSpace(tstxtRevisionFilter.Text)
                            || filter == tstxtRevisionFilter.Text);
                    menuItem.IsChecked = selected;
                    if (selected)
                    {
                        tstxtRevisionFilter.Text = filter;
                    }
                }
            }

            // Add to dropdown and settings, unless already included
            PromoteRevisionFilter(tstxtRevisionFilter.Text?.Trim() ?? string.Empty);
            ToolTip.SetTip(
                tsbtnAdvancedFilter,
                string.IsNullOrEmpty(e.FilterSummary) ? _advancedFilterToolTip : e.FilterSummary);
            tsbtnAdvancedFilter.Icon = e.HasFilter
                ? Properties.Images.FunnelExclamation
                : Properties.Images.FunnelPencil;
            tsmiResetPathFilters.IsEnabled = !string.IsNullOrEmpty(e.PathFilter);
            tsmiResetAllFilters.IsEnabled = e.HasFilter;
        }
        finally
        {
            _isApplyingFilter = false;
        }
    }

    private void revisionFilterBox_CheckedChanged(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(tstxtRevisionFilter.Text))
        {
            ApplyRevisionFilter();
        }
    }

    private void tsbtnAdvancedFilter_ButtonClick(object? sender, EventArgs e)
    {
        if (!tsmiResetAllFilters.IsEnabled)
        {
            RevisionGridFilter.ShowRevisionFilterDialog();
        }
        else
        {
            tsbtnAdvancedFilter.Flyout?.ShowAt(tsbtnAdvancedFilter);
        }
    }

    private void tstxtRevisionFilter_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyRevisionFilter();
        }
    }

    private void tscboBranchFilter_Click(object sender, EventArgs e)
    {
        if (!tscboBranchFilter.IsDropDownOpen)
        {
            tscboBranchFilter.IsDropDownOpen = true;
        }
    }

    private void tscboBranchFilter_DropDown(object sender, EventArgs e)
        => UpdateBranchFilterItems();

    private void tscboBranchFilter_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyCustomBranchFilter(checkBranch: true);
        }
    }

    private void tscboBranchFilter_TextChanged(object sender, EventArgs e)
    {
        if (e is AvaloniaPropertyChangedEventArgs args && args.Property != ComboBox.TextProperty)
        {
            return;
        }

        _filterBeingChanged = true;
        tscboBranchFilter_TextUpdate(sender, e);
    }

    private void tscboBranchFilter_TextUpdate(object sender, EventArgs e)
    {
        if (!_isApplyingFilter && !_updatingSuggestions && tscboBranchFilter.IsDropDownOpen)
        {
            UpdateBranchFilterItems();
        }
    }

    private void tsmiDisablePathFilters_Click(object sender, EventArgs e)
        => RevisionGridFilter.SetAndApplyPathFilter(string.Empty);

    private void tsmiDisableAllFilters_Click(object sender, EventArgs e)
        => RevisionGridFilter.ResetAllFiltersAndRefresh();

    private void tsmiAdvancedFilter_Click(object sender, EventArgs e)
        => RevisionGridFilter.ShowRevisionFilterDialog();

    private void tsmiShowReflogBranches_Click(object sender, EventArgs e)
        => ApplyPresetBranchesFilter(RevisionGridFilter.ShowReflog);

    private void tsmiShowBranchesAll_Click(object sender, EventArgs e)
        => ApplyPresetBranchesFilter(RevisionGridFilter.ShowAllBranches);

    private void tsmiShowBranchesCurrent_Click(object sender, EventArgs e)
        => ApplyPresetBranchesFilter(RevisionGridFilter.ShowCurrentBranchOnly);

    private void tsmiShowBranchesFiltered_Click(object sender, EventArgs e)
        => ApplyPresetBranchesFilter(RevisionGridFilter.ShowFilteredBranches);

    private void tsmiShowOnlyFirstParent_Click(object sender, EventArgs e)
        => RevisionGridFilter.ToggleShowOnlyFirstParent();

    private void tsmiShowReflog_Click(object sender, EventArgs e)
        => RevisionGridFilter.ToggleShowReflogReferences();

    private void tssbtnShowBranches_Click(object sender, EventArgs e)
        => tssbtnShowBranches.Flyout?.ShowAt(tssbtnShowBranches);

    private void ShowInvalidReference(string branch)
    {
        if (_showInvalidReference is not null)
        {
            _showInvalidReference(branch);
            return;
        }

        TaskDialogPage page = new()
        {
            Heading = string.Format(TranslatedStrings.IgnoringReference, branch),
            Caption = TranslatedStrings.NonexistingGitRevision,
            Buttons = { TaskDialogButton.OK },
            Icon = TaskDialogIcon.Warning,
            SizeToContent = true,
        };
        TaskDialog.ShowDialog(
            TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
            page);
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    public override void AddTranslationItems(ITranslation translation)
    {
        foreach ((string name, string property, string source) in TranslationItems)
        {
            translation.AddTranslationItem(TranslationCategory, name, property, source);
        }
    }

    public override void TranslateItems(ITranslation translation)
    {
        foreach ((string name, string property, string source) in TranslationItems)
        {
            string translated = translation.TranslateItem(TranslationCategory, name, property, () => source) ?? source;
            ApplyTranslation(name, property, translated);
        }
    }

    private void ApplyTranslation(string name, string property, string translated)
    {
        Control control = this.FindControl<Control>(name)!;
        if (property == "ToolTipText")
        {
            ToolTip.SetTip(control, translated);
        }
        else if (control is MenuItem menuItem)
        {
            menuItem.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(translated);
        }
        else if (control is TextBlock textBlock)
        {
            textBlock.Text = AvaloniaTranslationUtils.ToAvaloniaMnemonics(translated);
        }
        else if (control is ContentControl contentControl)
        {
            contentControl.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(translated);
        }
    }

    internal void RefreshBrowseDialogShortcutKeys(IReadOnlyList<HotkeyCommand> hotkeys)
    {
        _tslblRevisionFilterToolTip ??= ToolTip.GetTip(tslblRevisionFilter)?.ToString() ?? string.Empty;
        ToolTip.SetTip(
            tslblRevisionFilter,
            _tslblRevisionFilterToolTip.UpdateSuffix(hotkeys.GetShortcutToolTip(FormBrowse.Command.FocusFilter)));
    }

    internal void RefreshRevisionGridShortcutKeys(IReadOnlyList<HotkeyCommand> hotkeys)
    {
        ToolTip.SetTip(
            tsbShowReflog,
            TranslatedStrings.ShowReflogTooltip.UpdateSuffix(
                hotkeys.GetShortcutToolTip(RevisionGridControl.Command.ShowReflogReferences)));
        ToolTip.SetTip(
            tsmiShowOnlyFirstParent,
            TranslatedStrings.ShowOnlyFirstParent.UpdateSuffix(
                hotkeys.GetShortcutToolTip(RevisionGridControl.Command.ShowCurrentBranchOnly)));

        SetInputGesture(tsmiShowBranchesAll, hotkeys, RevisionGridControl.Command.ShowAllBranches);
        SetInputGesture(tsmiShowBranchesFiltered, hotkeys, RevisionGridControl.Command.ShowFilteredBranches);
        SetInputGesture(tsmiShowBranchesCurrent, hotkeys, RevisionGridControl.Command.ShowCurrentBranchOnly);
        SetInputGesture(tsmiResetPathFilters, hotkeys, RevisionGridControl.Command.ResetRevisionPathFilter);
        SetInputGesture(tsmiResetAllFilters, hotkeys, RevisionGridControl.Command.ResetRevisionFilter);
        SetInputGesture(tsmiAdvancedFilter, hotkeys, RevisionGridControl.Command.RevisionFilter);
    }

    internal readonly struct TestAccessor
    {
        private readonly FilterToolBar _control;

        public TestAccessor(FilterToolBar control)
        {
            _control = control;
        }

        public MenuItem BranchLocal => _control.tsmiBranchLocal;
        public MenuItem BranchRemote => _control.tsmiBranchRemote;
        public MenuItem BranchTag => _control.tsmiBranchTag;
        public MenuItem CommitFilter => _control.tsmiCommitFilter;
        public MenuItem CommitterFilter => _control.tsmiCommitterFilter;
        public MenuItem AuthorFilter => _control.tsmiAuthorFilter;
        public MenuItem DiffContainsFilter => _control.tsmiDiffContainsFilter;
        public ToggleButton ShowOnlyFirstParent => _control.tsmiShowOnlyFirstParent;
        public ToggleButton ShowReflog => _control.tsbShowReflog;
        public ToolbarComboBox RevisionFilter => _control.tstxtRevisionFilter;
        public Label RevisionFilterLabel => _control.tslblRevisionFilter;
        public IconSplitButton AdvancedFilter => _control.tsbtnAdvancedFilter;
        public MenuItem AdvancedFilterMenuItem => _control.tsmiAdvancedFilter;
        public MenuItem ResetPathFilters => _control.tsmiResetPathFilters;
        public MenuItem ResetAllFilters => _control.tsmiResetAllFilters;
        public ToolbarComboBox BranchFilter => _control.tscboBranchFilter;
        public bool IsApplyingFilter => _control._isApplyingFilter;
        public bool FilterBeingChanged => _control._filterBeingChanged;
        public IReadOnlyList<string> RevisionFilters => _control._revisionFilters;

        public void ApplyCustomBranchFilter(bool checkBranch)
            => _control.ApplyCustomBranchFilter(checkBranch);

        public void ApplyRevisionFilter()
            => _control.ApplyRevisionFilter();

        public void UpdateBranchFilterItems()
            => _control.UpdateBranchFilterItems();

        public void SetInvalidReferenceHandler(Action<string> handler)
            => _control._showInvalidReference = handler;
    }
}
