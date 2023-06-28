using ResourceManager;

namespace GitUI.UserControls.RevisionGrid
{
    public partial class FormRevisionFilter : GitExtensionsDialog
    {
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

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormRevisionFilter()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormRevisionFilter(GitUICommands commands, FilterInfo filterInfo)
            : base(commands, enablePositionRestore: false)
        {
            InitializeComponent();
            InitializeComplete();
            _NO_TRANSLATE_lblSince.Text = _since.Text;
            _NO_TRANSLATE_lblUntil.Text = _until.Text;
            _NO_TRANSLATE_lblAuthor.Text = _author.Text;
            _NO_TRANSLATE_lblCommitter.Text = _committer.Text;
            _NO_TRANSLATE_lblMessage.Text = _message.Text;
            _NO_TRANSLATE_lblDiffContent.Text = _diffContent.Text;
            toolTip.SetToolTip(DiffContentCheck, _diffContentToolTip.Text);
            toolTip.SetToolTip(DiffContent, _diffContentToolTip.Text);
            _NO_TRANSLATE_lblLimit.Text = _limit.Text;
            _NO_TRANSLATE_lblPathFilter.Text = _pathFilter.Text;
            _NO_TRANSLATE_lblBranches.Text = _branches.Text;

            _filterInfo = filterInfo;
        }

        private void CheckIfChanged()
        {
            FilterInfo tempFilter = new();
            UpdateFilterInfoFromUI(tempFilter);
            btnResetDefault.Visible = tempFilter.HasFilter;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FilterInfo rawFilterInfo = _filterInfo with { IsRaw = true };

            SinceCheck.Checked = rawFilterInfo.ByDateFrom;
            Since.Value = rawFilterInfo.DateFrom == DateTime.MinValue ? DateTime.Today : rawFilterInfo.DateFrom;
            CheckUntil.Checked = rawFilterInfo.ByDateTo;
            Until.Value = rawFilterInfo.DateTo == DateTime.MinValue ? DateTime.Today : rawFilterInfo.DateTo;
            AuthorCheck.Checked = rawFilterInfo.ByAuthor;
            Author.Text = rawFilterInfo.Author;
            CommitterCheck.Checked = rawFilterInfo.ByCommitter;
            Committer.Text = rawFilterInfo.Committer;
            MessageCheck.Checked = rawFilterInfo.ByMessage;
            Message.Text = rawFilterInfo.Message;
            DiffContentCheck.Checked = rawFilterInfo.ByDiffContent;
            DiffContent.Text = rawFilterInfo.DiffContent;
            IgnoreCase.Checked = rawFilterInfo.IgnoreCase;
            IgnoreCase.Enabled = Author.Enabled || Committer.Enabled || MessageCheck.Checked || DiffContentCheck.Checked;
            CommitsLimitCheck.Checked = rawFilterInfo.ByCommitsLimit;
            _NO_TRANSLATE_CommitsLimit.Value = rawFilterInfo.CommitsLimit;
            PathFilterCheck.Checked = rawFilterInfo.ByPathFilter;
            PathFilter.Text = rawFilterInfo.PathFilter;
            BranchFilterCheck.Checked = rawFilterInfo.IsShowFilteredBranchesChecked;
            BranchFilter.Text = rawFilterInfo.BranchFilter;
            CurrentBranchOnlyCheck.Checked = rawFilterInfo.ShowCurrentBranchOnly;
            ReflogCheck.Checked = rawFilterInfo.ShowReflogReferences;
            OnlyFirstParentCheck.Checked = rawFilterInfo.ShowOnlyFirstParent;
            NoMergeCommitsCheck.Checked = rawFilterInfo.NoMergeCommits;
            SimplifyByDecorationCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration;
            FullHistoryCheck.Checked = rawFilterInfo.ShowFullHistory;
            SimplifyMergesCheck.Checked = rawFilterInfo.ShowSimplifyMerges;

            UpdateFilters();
        }

        private void UpdateFilters()
        {
            Since.Enabled = SinceCheck.Checked;
            Until.Enabled = CheckUntil.Checked;
            Author.Enabled = AuthorCheck.Checked;
            Committer.Enabled = CommitterCheck.Checked;
            Message.Enabled = MessageCheck.Checked;
            DiffContent.Enabled = DiffContentCheck.Checked;
            IgnoreCase.Enabled = Author.Enabled || Committer.Enabled || MessageCheck.Checked || DiffContentCheck.Checked;
            _NO_TRANSLATE_CommitsLimit.Enabled = CommitsLimitCheck.Checked;
            PathFilter.Enabled = PathFilterCheck.Checked;

            CurrentBranchOnlyCheck.Enabled = !ReflogCheck.Checked;
            BranchFilterCheck.Enabled = !CurrentBranchOnlyCheck.Checked && !ReflogCheck.Checked;
            BranchFilter.Enabled = BranchFilterCheck.Checked;
        }

        private void UpdateFilterInfoFromUI(FilterInfo filterInfo)
        {
            // Note: There is no validation that information (like branch filters) are valid

            filterInfo.ByDateFrom = SinceCheck.Checked;
            filterInfo.DateFrom = Since.Value;
            filterInfo.ByDateTo = CheckUntil.Checked;
            filterInfo.DateTo = Until.Value;
            filterInfo.ByAuthor = AuthorCheck.Checked;
            filterInfo.Author = Author.Text.Trim();
            filterInfo.ByCommitter = CommitterCheck.Checked;
            filterInfo.Committer = Committer.Text.Trim();
            filterInfo.ByMessage = MessageCheck.Checked;
            filterInfo.Message = Message.Text.Trim();
            filterInfo.ByDiffContent = DiffContentCheck.Checked;
            filterInfo.DiffContent = DiffContent.Text.Trim();
            filterInfo.IgnoreCase = IgnoreCase.Checked;
            filterInfo.ByCommitsLimit = CommitsLimitCheck.Checked;
            filterInfo.CommitsLimit = (int)_NO_TRANSLATE_CommitsLimit.Value;
            filterInfo.ByPathFilter = PathFilterCheck.Checked;
            filterInfo.PathFilter = PathFilter.Text;
            filterInfo.ByBranchFilter = BranchFilterCheck.Checked;
            filterInfo.BranchFilter = BranchFilter.Text;
            filterInfo.ShowCurrentBranchOnly = CurrentBranchOnlyCheck.Checked;
            filterInfo.ShowReflogReferences = ReflogCheck.Checked;
            filterInfo.ShowOnlyFirstParent = OnlyFirstParentCheck.Checked;
            filterInfo.NoMergeCommits = NoMergeCommitsCheck.Checked;
            filterInfo.ShowSimplifyByDecoration = SimplifyByDecorationCheck.Checked;
            filterInfo.ShowFullHistory = FullHistoryCheck.Checked;
            filterInfo.ShowSimplifyMerges = SimplifyMergesCheck.Checked;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Note: There is no validation that information (like branch filters) are valid

            UpdateFilterInfoFromUI(_filterInfo);
        }

        private void option_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFilters();

            // If CommitsLimitCheck was changed, the displayed value may need to be updated too
            if (sender == CommitsLimitCheck && !CommitsLimitCheck.Checked)
            {
                _NO_TRANSLATE_CommitsLimit.Value = _filterInfo.CommitsLimitDefault;
            }

            CheckIfChanged();
        }
    }
}
