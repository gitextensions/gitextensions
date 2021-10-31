using System;

namespace GitUI.UserControls.RevisionGrid
{
    public partial class FormRevisionFilter : GitExtensionsDialog
    {
        private readonly FilterInfo _filterInfo;

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

            _filterInfo = filterInfo;

            // work-around the designer bug that can't add controls to FlowLayoutPanel
            ControlsPanel.Controls.Add(Ok);
            AcceptButton = Ok;
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
            IgnoreCase.Checked = rawFilterInfo.IgnoreCase;
            IgnoreCase.Enabled = Author.Enabled || Committer.Enabled || MessageCheck.Checked;
            CommitsLimitCheck.Checked = rawFilterInfo.ByCommitsLimit;
            _NO_TRANSLATE_CommitsLimit.Value = rawFilterInfo.CommitsLimit;
            PathFilterCheck.Checked = rawFilterInfo.ByPathFilter;
            PathFilter.Text = rawFilterInfo.PathFilter;
            BranchFilterCheck.Checked = rawFilterInfo.IsShowFilteredBranchesChecked || rawFilterInfo.IsShowCurrentBranchOnlyChecked;
            BranchFilter.Text = rawFilterInfo.BranchFilter;
            CurrentBranchOnlyCheck.Checked = rawFilterInfo.ShowCurrentBranchOnly;
            SimplifyByDecorationCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration;

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
            IgnoreCase.Enabled = Author.Enabled || Committer.Enabled || MessageCheck.Checked;
            _NO_TRANSLATE_CommitsLimit.Enabled = CommitsLimitCheck.Checked;
            PathFilter.Enabled = PathFilterCheck.Checked;

            CurrentBranchOnlyCheck.Enabled = BranchFilterCheck.Checked;
            SimplifyByDecorationCheck.Enabled = BranchFilterCheck.Checked;
            BranchFilter.Enabled = BranchFilterCheck.Checked &&
                                   !CurrentBranchOnlyCheck.Checked;
        }

        private void OkClick(object sender, EventArgs e)
        {
            _filterInfo.ByDateFrom = SinceCheck.Checked;
            _filterInfo.DateFrom = Since.Value;
            _filterInfo.ByDateTo = CheckUntil.Checked;
            _filterInfo.DateTo = Until.Value;
            _filterInfo.ByAuthor = AuthorCheck.Checked;
            _filterInfo.Author = Author.Text;
            _filterInfo.ByCommitter = CommitterCheck.Checked;
            _filterInfo.Committer = Committer.Text;
            _filterInfo.ByMessage = MessageCheck.Checked;
            _filterInfo.Message = Message.Text;
            _filterInfo.IgnoreCase = IgnoreCase.Checked;
            _filterInfo.ByCommitsLimit = CommitsLimitCheck.Checked;
            _filterInfo.CommitsLimit = (int)_NO_TRANSLATE_CommitsLimit.Value;
            _filterInfo.ByPathFilter = PathFilterCheck.Checked;
            _filterInfo.PathFilter = PathFilter.Text;
            _filterInfo.ByBranchFilter = BranchFilterCheck.Checked;
            _filterInfo.BranchFilter = BranchFilter.Text;
            _filterInfo.ShowCurrentBranchOnly = BranchFilterCheck.Checked && CurrentBranchOnlyCheck.Checked;
            _filterInfo.ShowSimplifyByDecoration = BranchFilterCheck.Checked && SimplifyByDecorationCheck.Checked;
        }
    }
}
