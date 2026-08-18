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

public sealed partial class FilterToolBar : TranslatedControl
{
    private const string TranslationCategory = nameof(FormBrowse);
    private const int MaxFilterItems = 30;

    private static readonly string[] RevisionFilterPresets =
    [
        @"--invert-grep --grep=""EXCLUDE_COMMIT_MESSAGE_REGEX_PATTERN""",
        @"--perl-regexp --author=""^(?!.*EXCLUDE_AUTHOR_REGEX_PATTERN)""",
        @"--exclude=refs/remotes/EXCLUDE_REMOTE_REGEX_PATTERN",
    ];

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
    private Func<IGitModule>? _getModule;
    private Func<RefsFilter, IReadOnlyList<IGitRef>>? _getRefs;
    private IRevisionGridFilter? _revisionGridFilter;
    private bool _isApplyingFilter;
    private bool _filterBeingChanged;
    private bool _updatingSuggestions;
    private string _advancedFilterToolTip = string.Empty;
    private string? _tslblRevisionFilterToolTip;
    private Action<string>? _showInvalidReference;

    public FilterToolBar()
    {
        InitializeComponent();

        tsbtnAdvancedFilter.Click += tsbtnAdvancedFilter_ButtonClick;
        tsmiResetPathFilters.Click += (_, _) => RevisionGridFilter.SetAndApplyPathFilter(string.Empty);
        tsmiResetAllFilters.Click += (_, _) => RevisionGridFilter.ResetAllFiltersAndRefresh();
        tsmiAdvancedFilter.Click += (_, _) => RevisionGridFilter.ShowRevisionFilterDialog();
        tsbShowReflog.Click += (_, _) => ApplyPresetBranchesFilter(RevisionGridFilter.ToggleShowReflogReferences);
        tssbtnShowBranches.Click += (_, _) => tssbtnShowBranches.Flyout?.ShowAt(tssbtnShowBranches);
        tsmiShowBranchesAll.Click += (_, _) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowAllBranches);
        tsmiShowBranchesCurrent.Click += (_, _) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowCurrentBranchOnly);
        tsmiShowBranchesFiltered.Click += (_, _) => ApplyPresetBranchesFilter(RevisionGridFilter.ShowFilteredBranches);
        tsmiShowOnlyFirstParent.Click += (_, _) => RevisionGridFilter.ToggleShowOnlyFirstParent();
        tscboBranchFilter.KeyUp += BranchFilterKeyUp;
        tstxtRevisionFilter.KeyUp += RevisionFilterKeyUp;
        tscboBranchFilter.DropDownOpened += (_, _) => UpdateBranchFilterItems();
        tscboBranchFilter.PropertyChanged += BranchFilterPropertyChanged;
        tsmiBranchLocal.Click += (_, _) => UpdateBranchFilterItems();
        tsmiBranchRemote.Click += (_, _) => UpdateBranchFilterItems();
        tsmiBranchTag.Click += (_, _) => UpdateBranchFilterItems();
        tsmiCommitFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();
        tsmiCommitterFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();
        tsmiAuthorFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();
        tsmiDiffContainsFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();

        _revisionFilters = AppSettings.RevisionFilterDropdowns
            .Union(RevisionFilterPresets, StringComparer.Ordinal)
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
        revisionGridFilter.FilterChanged += RevisionGridFilterChanged;
    }

    public void ClearQuickFilters()
    {
        tscboBranchFilter.Text = string.Empty;
        tstxtRevisionFilter.Text = string.Empty;
    }

    /// <summary>
    ///  Sets the branches filter without checking that the supplied refs exist.
    /// </summary>
    public void SetBranchFilter(string? filter)
    {
        tscboBranchFilter.Text = filter;
        ApplyCustomBranchFilter(checkBranch: false);
    }

    public void RefreshRevisionFunction(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        _getRefs = getRefs ?? throw new ArgumentNullException(nameof(getRefs));
        tscboBranchFilter.ItemsSource = Array.Empty<string>();
    }

    /// <summary>
    /// Sets and applies the text revision filter, matching the WinForms toolbar contract.
    /// </summary>
    public void SetRevisionFilter(string? filter)
    {
        if (string.IsNullOrEmpty(tstxtRevisionFilter.Text) && string.IsNullOrEmpty(filter))
        {
            return;
        }

        tstxtRevisionFilter.Text = filter;
        ApplyRevisionFilter();
    }

    /// <summary>Alternates focus between the revision and branch quick filters.</summary>
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

    private static void SetInputGesture(
        MenuItem menuItem,
        IReadOnlyList<HotkeyCommand> hotkeys,
        RevisionGridControl.Command command)
    {
        WinFormsShims.Keys keys = hotkeys.FirstOrDefault(hotkey => hotkey.CommandCode == (int)command)?.KeyData
            ?? WinFormsShims.Keys.None;
        menuItem.InputGesture = KeysMapper.ToKeyGesture(keys);
    }

    private IGitModule GetModule()
    {
        if (_getModule is null)
        {
            throw new InvalidOperationException($"{nameof(Bind)} is not called.");
        }

        return _getModule() ?? throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
    }

    private void ApplyPresetBranchesFilter(Action filterAction)
    {
        _filterBeingChanged = true;
        filterAction();
        _filterBeingChanged = false;
    }

    private void BranchFilterKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyCustomBranchFilter();
        }
    }

    private void RevisionFilterKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyRevisionFilter();
        }
    }

    private void BranchFilterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ComboBox.TextProperty || _isApplyingFilter || _updatingSuggestions)
        {
            return;
        }

        _filterBeingChanged = true;
        if (tscboBranchFilter.IsDropDownOpen)
        {
            UpdateBranchFilterItems();
        }
    }

    private void ApplyCustomBranchFilter(bool checkBranch = true)
    {
        if (_isApplyingFilter)
        {
            return;
        }

        _isApplyingFilter = true;
        try
        {
            _filterBeingChanged = false;
            string filter = tscboBranchFilter.Text == TranslatedStrings.NoResultsFound
                ? string.Empty
                : tscboBranchFilter.Text?.Trim() ?? string.Empty;
            if (checkBranch && !string.IsNullOrWhiteSpace(filter))
            {
                List<string> acceptedFilters = [];
                IReadOnlyList<IGitRef> refs = GetRefs(RefsFilter.NoFilter);
                foreach (string branch in filter.Split(
                             (char[]?)null,
                             StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    if (IsValidBranchFilter(branch, refs))
                    {
                        acceptedFilters.Add(branch);
                        continue;
                    }

                    ShowInvalidReference(branch);
                }

                filter = string.Join(" ", acceptedFilters);
            }

            RevisionGridFilter.SetAndApplyBranchFilter(filter);
        }
        finally
        {
            _isApplyingFilter = false;
        }
    }

    private bool IsValidBranchFilter(string branch, IReadOnlyList<IGitRef> refs)
    {
        bool isExpression = branch.StartsWith("--", StringComparison.Ordinal)
                            || branch.Contains("..", StringComparison.Ordinal)
                            || branch.IndexOfAny(Delimiters.WildcardBranchSearchValues) >= 0;
        if (isExpression || refs.Any(gitRef => gitRef.LocalName == branch))
        {
            return true;
        }

        string gitRef = branch.StartsWith('^') ? branch[1..] : branch;
        return !GetModule().RevParse(gitRef).IsZero;
    }

    private void ApplyRevisionFilterIfPopulated()
    {
        if (!string.IsNullOrWhiteSpace(tstxtRevisionFilter.Text))
        {
            ApplyRevisionFilter();
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

    private IReadOnlyList<IGitRef> GetRefs(RefsFilter filter)
        => _getRefs?.Invoke(filter) ?? GetModule().GetRefs(filter);

    private void UpdateBranchFilterItems()
    {
        if (_getModule is null || !GetModule().IsValidGitWorkingDir())
        {
            IsEnabled = false;
            return;
        }

        IsEnabled = true;
        RefsFilter filter = (tsmiBranchLocal.IsChecked ? RefsFilter.Heads : RefsFilter.NoFilter)
            | (tsmiBranchRemote.IsChecked ? RefsFilter.Remotes : RefsFilter.NoFilter)
            | (tsmiBranchTag.IsChecked ? RefsFilter.Tags : RefsFilter.NoFilter);
        string currentText = tscboBranchFilter.Text ?? string.Empty;
        string[] matches = GetRefs(filter)
            .Select(gitRef => gitRef.Name)
            .Distinct(StringComparer.Ordinal)
            .Where(branch => branch.Contains(currentText, StringComparison.InvariantCultureIgnoreCase))
            .Order(StringComparer.InvariantCulture)
            .ToArray();

        _updatingSuggestions = true;
        try
        {
            tscboBranchFilter.ItemsSource = matches.Length == 0
                ? [TranslatedStrings.NoResultsFound]
                : matches;
            tscboBranchFilter.Text = currentText;
            tscboBranchFilter.IsDropDownOpen = true;
        }
        finally
        {
            _updatingSuggestions = false;
        }
    }

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

    private void RevisionGridFilterChanged(object? sender, FilterChangedEventArgs e)
    {
        _isApplyingFilter = true;
        try
        {
            tsmiShowOnlyFirstParent.IsChecked = e.ShowOnlyFirstParent;
            tsbShowReflog.IsChecked = e.ShowReflogReferences;
            if (e.ShowFilteredBranches)
            {
                // Preserve the typed branch expression while temporarily showing all/current.
                tscboBranchFilter.Text = e.BranchFilter;
            }

            List<(string Filter, MenuItem MenuItem)> revisionFilters =
            [
                (e.MessageFilter, tsmiCommitFilter),
                (e.CommitterFilter, tsmiCommitterFilter),
                (e.AuthorFilter, tsmiAuthorFilter),
                (e.DiffContentFilter, tsmiDiffContainsFilter),
            ];

            tstxtRevisionFilter.Text = string.Empty;
            if (revisionFilters.Any(item => !string.IsNullOrWhiteSpace(item.Filter)))
            {
                foreach ((string filter, MenuItem menuItem) in revisionFilters)
                {
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

            PromoteRevisionFilter(tstxtRevisionFilter.Text?.Trim() ?? string.Empty);
            ToolTip.SetTip(
                tsbtnAdvancedFilter,
                string.IsNullOrEmpty(e.FilterSummary) ? _advancedFilterToolTip : e.FilterSummary);
            tsbtnAdvancedFilter.Icon = e.HasFilter
                ? Properties.Images.FunnelExclamation
                : Properties.Images.FunnelPencil;
            tsmiResetPathFilters.IsEnabled = !string.IsNullOrEmpty(e.PathFilter);
            tsmiResetAllFilters.IsEnabled = e.HasFilter;

            if (e.ShowCurrentBranchOnly)
            {
                SetBranchMode(tsmiShowBranchesCurrent, Properties.Images.BranchFilter);
            }
            else if (e.ShowFilteredBranches)
            {
                SetBranchMode(tsmiShowBranchesFiltered, Properties.Images.BranchFilter);
            }
            else
            {
                SetBranchMode(tsmiShowBranchesAll, Properties.Images.BranchLocal);
            }
        }
        finally
        {
            _isApplyingFilter = false;
        }
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

    internal TestAccessor GetTestAccessor()
        => new(this);

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
