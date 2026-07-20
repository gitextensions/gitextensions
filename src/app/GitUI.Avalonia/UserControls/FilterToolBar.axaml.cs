using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using GitUI.UserControls.RevisionGrid;
using ResourceManager;

namespace GitUI.UserControls;

public sealed partial class FilterToolBar : TranslatedControl
{
    private const string TranslationCategory = nameof(CommandsDialogs.FormBrowse);

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

    private Func<IGitModule>? _getModule;
    private IRevisionGridFilter? _revisionGridFilter;
    private bool _updating;

    public FilterToolBar()
    {
        InitializeComponent();

        tsbtnAdvancedFilter.Click += (_, _) => tstxtRevisionFilter.Focus();
        tsmiResetPathFilters.Click += (_, _) => RevisionGridFilter.SetAndApplyPathFilter(string.Empty);
        tsmiResetAllFilters.Click += (_, _) => RevisionGridFilter.ResetAllFiltersAndRefresh();
        tsbShowReflog.Click += (_, _) => RevisionGridFilter.ToggleShowReflogReferences();
        tssbtnShowBranches.Click += (_, _) => tssbtnShowBranches.Flyout?.ShowAt(tssbtnShowBranches);
        tsmiShowBranchesAll.Click += (_, _) => RevisionGridFilter.ShowAllBranches();
        tsmiShowBranchesCurrent.Click += (_, _) => RevisionGridFilter.ShowCurrentBranchOnly();
        tsmiShowBranchesFiltered.Click += (_, _) => ApplyBranchFilter();
        tsmiShowOnlyFirstParent.Click += (_, _) => RevisionGridFilter.ToggleShowOnlyFirstParent();
        tscboBranchFilter.KeyUp += BranchFilterKeyUp;
        tstxtRevisionFilter.KeyUp += RevisionFilterKeyUp;
        tscboBranchFilter.DropDownOpened += (_, _) => RefreshBranches();
        tsmiBranchLocal.Click += (_, _) => RefreshBranches();
        tsmiBranchRemote.Click += (_, _) => RefreshBranches();
        tsmiBranchTag.Click += (_, _) => RefreshBranches();
        tsmiCommitFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();
        tsmiCommitterFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();
        tsmiAuthorFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();
        tsmiDiffContainsFilter.Click += (_, _) => ApplyRevisionFilterIfPopulated();

        tstxtRevisionFilter.ItemsSource = AppSettings.RevisionFilterDropdowns;
        ToolTip.SetTip(tsbShowReflog, TranslatedStrings.ShowReflogTooltip);
        ToolTip.SetTip(tsmiShowOnlyFirstParent, TranslatedStrings.ShowOnlyFirstParent);
        InitializeComplete();
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

    public void RefreshRevisionFunction(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        ArgumentNullException.ThrowIfNull(getRefs);
        RefreshBranches(getRefs);
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

    public void SetFocus()
        => tstxtRevisionFilter.Focus();

    private void BranchFilterKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyBranchFilter();
        }
    }

    private void RevisionFilterKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyRevisionFilter();
        }
    }

    private void ApplyBranchFilter()
    {
        if (_updating)
        {
            return;
        }

        RevisionGridFilter.SetAndApplyBranchFilter(tscboBranchFilter.Text?.Trim() ?? string.Empty);
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
        if (_updating)
        {
            return;
        }

        string text = tstxtRevisionFilter.Text?.Trim() ?? string.Empty;
        RevisionGridFilter.SetAndApplyRevisionFilter(new RevisionFilter(
            text,
            tsmiCommitFilter.IsChecked,
            tsmiCommitterFilter.IsChecked,
            tsmiAuthorFilter.IsChecked,
            tsmiDiffContainsFilter.IsChecked));
    }

    private void RefreshBranches()
    {
        if (_getModule is null)
        {
            return;
        }

        RefsFilter filter = (tsmiBranchLocal.IsChecked ? RefsFilter.Heads : RefsFilter.NoFilter)
            | (tsmiBranchRemote.IsChecked ? RefsFilter.Remotes : RefsFilter.NoFilter)
            | (tsmiBranchTag.IsChecked ? RefsFilter.Tags : RefsFilter.NoFilter);
        IGitModule module = _getModule();
        ThreadHelper.FileAndForget(async () =>
        {
            IReadOnlyList<IGitRef> refs = module.GetRefs(filter);
            await this.SwitchToMainThreadAsync();
            if (ReferenceEquals(module, _getModule()))
            {
                RefreshBranches(refs);
            }
        });
    }

    private void RefreshBranches(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        RefsFilter filter = (tsmiBranchLocal.IsChecked ? RefsFilter.Heads : RefsFilter.NoFilter)
            | (tsmiBranchRemote.IsChecked ? RefsFilter.Remotes : RefsFilter.NoFilter)
            | (tsmiBranchTag.IsChecked ? RefsFilter.Tags : RefsFilter.NoFilter);
        RefreshBranches(getRefs(filter));
    }

    private void RefreshBranches(IReadOnlyList<IGitRef> refs)
    {
        string currentText = tscboBranchFilter.Text ?? string.Empty;
        tscboBranchFilter.ItemsSource = refs.Select(gitRef => gitRef.Name).Distinct().Order().ToArray();
        tscboBranchFilter.Text = currentText;
    }

    private void RevisionGridFilterChanged(object? sender, FilterChangedEventArgs e)
    {
        _updating = true;
        tscboBranchFilter.Text = e.BranchFilter;
        tstxtRevisionFilter.Text = FirstPopulated(e.MessageFilter, e.CommitterFilter, e.AuthorFilter, e.DiffContentFilter);
        if (!string.IsNullOrEmpty(tstxtRevisionFilter.Text))
        {
            tsmiCommitFilter.IsChecked = !string.IsNullOrEmpty(e.MessageFilter);
            tsmiCommitterFilter.IsChecked = !string.IsNullOrEmpty(e.CommitterFilter);
            tsmiAuthorFilter.IsChecked = !string.IsNullOrEmpty(e.AuthorFilter);
            tsmiDiffContainsFilter.IsChecked = !string.IsNullOrEmpty(e.DiffContentFilter);
        }

        tsbShowReflog.IsChecked = e.ShowReflogReferences;
        tsmiShowOnlyFirstParent.IsChecked = e.ShowOnlyFirstParent;
        tsmiResetPathFilters.IsEnabled = !string.IsNullOrEmpty(e.PathFilter);
        tsmiResetAllFilters.IsEnabled = e.HasFilter;
        tsbtnAdvancedFilter.Icon = e.HasFilter ? Properties.Images.FunnelExclamation : Properties.Images.FunnelPencil;

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

        _updating = false;
    }

    private void SetBranchMode(MenuItem source, Avalonia.Media.IImage icon)
    {
        tssbtnShowBranches.Content = source.Header;
        tssbtnShowBranches.Icon = icon;
        ToolTip.SetTip(tssbtnShowBranches, ToolTip.GetTip(source));
    }

    private static string FirstPopulated(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

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
}
