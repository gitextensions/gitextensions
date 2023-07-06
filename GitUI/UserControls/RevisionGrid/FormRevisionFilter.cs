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
            HideMergeCommitsCheck.Checked = rawFilterInfo.HideMergeCommits;
            SimplifyByDecorationCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration;
            FullHistoryCheck.Checked = rawFilterInfo.ShowFullHistory;
            SimplifyMergesCheck.Checked = rawFilterInfo.ShowSimplifyMerges;

            UpdateFilters();
        }

        private void option_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFilters();

            // If CommitsLimitCheck was changed, the displayed value may need to be updated too
            if (sender == CommitsLimitCheck && !CommitsLimitCheck.Checked)
            {
                _NO_TRANSLATE_CommitsLimit.Value = _filterInfo.CommitsLimitDefault;
            }
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

        private void OkClick(object sender, EventArgs e)
        {
            // Note: There is no validation that information (like branch filters) are valid

            _filterInfo.ByDateFrom = SinceCheck.Checked;
            _filterInfo.DateFrom = Since.Value;
            _filterInfo.ByDateTo = CheckUntil.Checked;
            _filterInfo.DateTo = Until.Value;
            _filterInfo.ByAuthor = AuthorCheck.Checked;
            _filterInfo.Author = Author.Text.Trim();
            _filterInfo.ByCommitter = CommitterCheck.Checked;
            _filterInfo.Committer = Committer.Text.Trim();
            _filterInfo.ByMessage = MessageCheck.Checked;
            _filterInfo.Message = Message.Text.Trim();
            _filterInfo.ByDiffContent = DiffContentCheck.Checked;
            _filterInfo.DiffContent = DiffContent.Text.Trim();
            _filterInfo.IgnoreCase = IgnoreCase.Checked;
            _filterInfo.ByCommitsLimit = CommitsLimitCheck.Checked;
            _filterInfo.CommitsLimit = (int)_NO_TRANSLATE_CommitsLimit.Value;
            _filterInfo.ByPathFilter = PathFilterCheck.Checked;
            _filterInfo.PathFilter = PathFilter.Text;
            _filterInfo.ByBranchFilter = BranchFilterCheck.Checked;
            _filterInfo.BranchFilter = BranchFilter.Text;
            _filterInfo.ShowCurrentBranchOnly = CurrentBranchOnlyCheck.Checked;
            _filterInfo.ShowReflogReferences = ReflogCheck.Checked;
            _filterInfo.ShowOnlyFirstParent = OnlyFirstParentCheck.Checked;
            _filterInfo.HideMergeCommits = HideMergeCommitsCheck.Checked;
            _filterInfo.ShowSimplifyByDecoration = SimplifyByDecorationCheck.Checked;
            _filterInfo.ShowFullHistory = FullHistoryCheck.Checked;
            _filterInfo.ShowSimplifyMerges = SimplifyMergesCheck.Checked;
        }
    }
}
