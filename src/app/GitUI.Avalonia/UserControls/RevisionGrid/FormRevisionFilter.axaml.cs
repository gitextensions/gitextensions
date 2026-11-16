using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using ResourceManager;
using ToolTip = GitUI.Compat.Components.ToolTip;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.UserControls.RevisionGrid;

public partial class FormRevisionFilter : GitExtensionsDialog
{
    // Avalonia does not generate a field for this nonvisual tooltip; retain the component in code.
#pragma warning disable SX1309 // Preserve the original Designer field name.
    private readonly ToolTip toolTip = new();
#pragma warning restore SX1309

    private readonly FilterInfo _filterInfo;

    private readonly TranslationString _since = new("&Since");
    private readonly TranslationString _until = new("&Until");
    private readonly TranslationString _author = new("&Author");
    private readonly TranslationString _committer = new("&Committer");
    private readonly TranslationString _message = new("&Message");
    private readonly TranslationString _diffContent = new("&Diff contains");
    private readonly TranslationString _diffContentToolTip = new("SLOW");
    private readonly TranslationString _limit = new("&Limit");
    private readonly TranslationString _pathFilter = new("&Path filter");
    private readonly TranslationString _branches = new("&Branches");

    // Avalonia's preview loader requires a parameterless constructor without runtime commands.
    public FormRevisionFilter()
        : this(commands: null, filterInfo: new FilterInfo())
    {
    }

    public FormRevisionFilter(IGitUICommands? commands, FilterInfo filterInfo)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        SinceCheck.IsCheckedChanged += option_CheckedChanged;
        CheckUntil.IsCheckedChanged += option_CheckedChanged;
        AuthorCheck.IsCheckedChanged += option_CheckedChanged;
        CommitterCheck.IsCheckedChanged += option_CheckedChanged;
        MessageCheck.IsCheckedChanged += option_CheckedChanged;
        DiffContentCheck.IsCheckedChanged += option_CheckedChanged;
        CommitsLimitCheck.IsCheckedChanged += option_CheckedChanged;
        PathFilterCheck.IsCheckedChanged += option_CheckedChanged;
        BranchFilterCheck.IsCheckedChanged += option_CheckedChanged;
        CurrentBranchOnlyCheck.IsCheckedChanged += option_CheckedChanged;
        ReflogCheck.IsCheckedChanged += option_CheckedChanged;
        OnlyFirstParentCheck.IsCheckedChanged += option_CheckedChanged;
        HideMergeCommitsCheck.IsCheckedChanged += option_CheckedChanged;
        SimplifyByDecorationCheck.IsCheckedChanged += option_CheckedChanged;
        FullHistoryCheck.IsCheckedChanged += option_CheckedChanged;
        SimplifyMergesCheck.IsCheckedChanged += option_CheckedChanged;
        Ok.Click += OkClick;
        AcceptButton = Ok;
        InitializeComplete();
        _NO_TRANSLATE_lblSince.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_since.Text);
        _NO_TRANSLATE_lblUntil.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_until.Text);
        _NO_TRANSLATE_lblAuthor.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_author.Text);
        _NO_TRANSLATE_lblCommitter.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_committer.Text);
        _NO_TRANSLATE_lblMessage.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_message.Text);
        _NO_TRANSLATE_lblDiffContent.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_diffContent.Text);
        toolTip.SetToolTip(DiffContentCheck, _diffContentToolTip.Text);
        toolTip.SetToolTip(DiffContent, _diffContentToolTip.Text);
        _NO_TRANSLATE_lblLimit.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_limit.Text);
        _NO_TRANSLATE_lblPathFilter.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_pathFilter.Text);
        _NO_TRANSLATE_lblBranches.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_branches.Text);

        _filterInfo = filterInfo;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        FilterInfo rawFilterInfo = _filterInfo with { IsRaw = true };

        SinceCheck.IsChecked = rawFilterInfo.ByDateFrom;
        Since.SelectedDate = new DateTimeOffset(rawFilterInfo.DateFrom == DateTime.MinValue ? DateTime.Today : rawFilterInfo.DateFrom);
        CheckUntil.IsChecked = rawFilterInfo.ByDateTo;
        Until.SelectedDate = new DateTimeOffset(rawFilterInfo.DateTo == DateTime.MinValue ? DateTime.Today : rawFilterInfo.DateTo);
        AuthorCheck.IsChecked = rawFilterInfo.ByAuthor;
        Author.Text = rawFilterInfo.Author;
        CommitterCheck.IsChecked = rawFilterInfo.ByCommitter;
        Committer.Text = rawFilterInfo.Committer;
        MessageCheck.IsChecked = rawFilterInfo.ByMessage;
        Message.Text = rawFilterInfo.Message;
        DiffContentCheck.IsChecked = rawFilterInfo.ByDiffContent;
        DiffContent.Text = rawFilterInfo.DiffContent;
        IgnoreCase.IsChecked = rawFilterInfo.IgnoreCase;
        CommitsLimitCheck.IsChecked = rawFilterInfo.ByCommitsLimit;
        _NO_TRANSLATE_CommitsLimit.Value = rawFilterInfo.CommitsLimit;
        PathFilterCheck.IsChecked = rawFilterInfo.ByPathFilter;
        PathFilter.Text = rawFilterInfo.PathFilter;
        BranchFilterCheck.IsChecked = rawFilterInfo.IsShowFilteredBranchesChecked;
        BranchFilter.Text = rawFilterInfo.BranchFilter;
        CurrentBranchOnlyCheck.IsChecked = rawFilterInfo.ShowCurrentBranchOnly;
        ReflogCheck.IsChecked = rawFilterInfo.ShowReflogReferences;
        OnlyFirstParentCheck.IsChecked = rawFilterInfo.ShowOnlyFirstParent;
        HideMergeCommitsCheck.IsChecked = rawFilterInfo.HideMergeCommits;
        SimplifyByDecorationCheck.IsChecked = rawFilterInfo.ShowSimplifyByDecoration;
        FullHistoryCheck.IsChecked = rawFilterInfo.ShowFullHistory;
        SimplifyMergesCheck.IsChecked = rawFilterInfo.ShowSimplifyMerges;

        UpdateFilters();
    }

    private void option_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateFilters();

        // If CommitsLimitCheck was changed, the displayed value may need to be updated too
        if (ReferenceEquals(sender, CommitsLimitCheck) && CommitsLimitCheck.IsChecked != true)
        {
            _NO_TRANSLATE_CommitsLimit.Value = _filterInfo.CommitsLimitDefault;
        }
    }

    private void UpdateFilters()
    {
        Since.IsEnabled = SinceCheck.IsChecked == true;
        Until.IsEnabled = CheckUntil.IsChecked == true;
        Author.IsEnabled = AuthorCheck.IsChecked == true;
        Committer.IsEnabled = CommitterCheck.IsChecked == true;
        Message.IsEnabled = MessageCheck.IsChecked == true;
        DiffContent.IsEnabled = DiffContentCheck.IsChecked == true;
        IgnoreCase.IsEnabled = Author.IsEnabled
            || Committer.IsEnabled
            || MessageCheck.IsChecked == true
            || DiffContentCheck.IsChecked == true;
        _NO_TRANSLATE_CommitsLimit.IsEnabled = CommitsLimitCheck.IsChecked == true;
        PathFilter.IsEnabled = PathFilterCheck.IsChecked == true;

        CurrentBranchOnlyCheck.IsEnabled = ReflogCheck.IsChecked != true;
        BranchFilterCheck.IsEnabled = CurrentBranchOnlyCheck.IsChecked != true
            && ReflogCheck.IsChecked != true;
        BranchFilter.IsEnabled = BranchFilterCheck.IsChecked == true;

        // Simplifying the merges has no effect without the full history, as the file history
        // already indicates by disabling its menu item the same way
        SimplifyMergesCheck.IsEnabled = FullHistoryCheck.IsChecked == true;
    }

    private void OkClick(object? sender, EventArgs e)
    {
        // Note: There is no validation that information (like branch filters) are valid

        _filterInfo.ByDateFrom = SinceCheck.IsChecked == true;
        _filterInfo.DateFrom = Since.SelectedDate?.Date ?? DateTime.Today;
        _filterInfo.ByDateTo = CheckUntil.IsChecked == true;
        _filterInfo.DateTo = Until.SelectedDate?.Date ?? DateTime.Today;
        _filterInfo.ByAuthor = AuthorCheck.IsChecked == true;
        _filterInfo.Author = Author.Text?.Trim() ?? string.Empty;
        _filterInfo.ByCommitter = CommitterCheck.IsChecked == true;
        _filterInfo.Committer = Committer.Text?.Trim() ?? string.Empty;
        _filterInfo.ByMessage = MessageCheck.IsChecked == true;
        _filterInfo.Message = Message.Text?.Trim() ?? string.Empty;
        _filterInfo.ByDiffContent = DiffContentCheck.IsChecked == true;
        _filterInfo.DiffContent = DiffContent.Text?.Trim() ?? string.Empty;
        _filterInfo.IgnoreCase = IgnoreCase.IsChecked == true;
        _filterInfo.ByCommitsLimit = CommitsLimitCheck.IsChecked == true;
        _filterInfo.CommitsLimit = decimal.ToInt32(_NO_TRANSLATE_CommitsLimit.Value ?? 0);
        _filterInfo.ByPathFilter = PathFilterCheck.IsChecked == true;
        _filterInfo.PathFilter = PathFilter.Text ?? string.Empty;
        _filterInfo.ByBranchFilter = BranchFilterCheck.IsChecked == true;
        _filterInfo.BranchFilter = BranchFilter.Text ?? string.Empty;
        _filterInfo.ShowCurrentBranchOnly = CurrentBranchOnlyCheck.IsChecked == true;
        _filterInfo.ShowReflogReferences = ReflogCheck.IsChecked == true;
        _filterInfo.ShowOnlyFirstParent = OnlyFirstParentCheck.IsChecked == true;
        _filterInfo.HideMergeCommits = HideMergeCommitsCheck.IsChecked == true;
        _filterInfo.ShowSimplifyByDecoration = SimplifyByDecorationCheck.IsChecked == true;
        _filterInfo.ShowFullHistory = FullHistoryCheck.IsChecked == true;
        _filterInfo.ShowSimplifyMerges = SimplifyMergesCheck.IsChecked == true;
        DialogResult = WinFormsShims.DialogResult.OK;
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(FormRevisionFilter form)
    {
        public CheckBox FullHistoryCheck => form.FullHistoryCheck;
        public CheckBox SimplifyMergesCheck => form.SimplifyMergesCheck;
        public void UpdateFilters() => form.UpdateFilters();
    }
}
