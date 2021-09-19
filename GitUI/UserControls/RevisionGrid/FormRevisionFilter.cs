using System;
using GitCommands;
using GitExtUtils;

namespace GitUI.UserControls.RevisionGrid
{
    public partial class FormRevisionFilter : GitExtensionsForm
    {
        public FormRevisionFilter()
        {
            InitializeComponent();
            InitializeComplete();

            _NO_TRANSLATE_Limit.Value = AppSettings.MaxRevisionGraphCommits;
        }

        private void FormRevisionFilterLoad(object sender, EventArgs e)
        {
            EnableFilters();
        }

        private void SinceCheckCheckedChanged(object sender, EventArgs e)
        {
            EnableFilters();
        }

        private void OnBranchFilterCheckedChanged(object sender, EventArgs e)
        {
            AppSettings.BranchFilterEnabled = BranchFilterCheck.Checked;
            EnableFilters();
        }

        private void OnShowCurrentBranchOnlyCheckedChanged(object sender, EventArgs e)
        {
            AppSettings.ShowCurrentBranchOnly = CurrentBranchOnlyCheck.Checked;
            EnableFilters();
        }

        private void OnSimplifyByDecorationCheckedChanged(object sender, EventArgs e)
        {
            AppSettings.ShowSimplifyByDecoration = SimplifyByDecorationCheck.Checked;
            EnableFilters();
        }

        private void EnableFilters()
        {
            Since.Enabled = SinceCheck.Checked;
            Until.Enabled = CheckUntil.Checked;
            Author.Enabled = AuthorCheck.Checked;
            Committer.Enabled = CommitterCheck.Checked;
            Message.Enabled = MessageCheck.Checked;
            IgnoreCase.Enabled = Author.Enabled || Committer.Enabled || MessageCheck.Checked;
            _NO_TRANSLATE_Limit.Enabled = LimitCheck.Checked;
            FileFilter.Enabled = FileFilterCheck.Checked;

            BranchFilterCheck.Checked = AppSettings.BranchFilterEnabled;
            CurrentBranchOnlyCheck.Checked = AppSettings.ShowCurrentBranchOnly;
            SimplifyByDecorationCheck.Checked = AppSettings.ShowSimplifyByDecoration;

            CurrentBranchOnlyCheck.Enabled = BranchFilterCheck.Checked;
            SimplifyByDecorationCheck.Enabled = BranchFilterCheck.Checked;
            BranchFilter.Enabled = BranchFilterCheck.Checked &&
                                   !CurrentBranchOnlyCheck.Checked;
        }

        public bool FilterEnabled()
        {
            return SinceCheck.Checked ||
                    CheckUntil.Checked ||
                    AuthorCheck.Checked ||
                    CommitterCheck.Checked ||
                    MessageCheck.Checked ||
                    FileFilterCheck.Checked ||
                    BranchFilterCheck.Checked;
        }

        public void DisableFilters()
        {
            SinceCheck.Checked = false;
            CheckUntil.Checked = false;
            AuthorCheck.Checked = false;
            CommitterCheck.Checked = false;
            MessageCheck.Checked = false;
            FileFilterCheck.Checked = false;
            BranchFilterCheck.Checked = false;
        }

        public ArgumentString GetRevisionFilter()
        {
            ArgumentBuilder filter = new();

            if (AuthorCheck.Checked)
            {
                filter.Add($"--author=\"{Author.Text}\"");
            }

            if (CommitterCheck.Checked)
            {
                filter.Add($"--committer=\"{Committer.Text}\"");
            }

            if (MessageCheck.Checked)
            {
                filter.Add($"--grep=\"{Message.Text}\"");
            }

            if (!filter.IsEmpty && IgnoreCase.Checked)
            {
                filter.Add("--regexp-ignore-case");
            }

            if (SinceCheck.Checked)
            {
                filter.Add($"--since=\"{Since.Value:yyyy-MM-dd hh:mm:ss}\"");
            }

            if (CheckUntil.Checked)
            {
                filter.Add($"--until=\"{Until.Value:yyyy-MM-dd hh:mm:ss}\"");
            }

            return filter;
        }

        public int GetMaxCount()
        {
            return LimitCheck.Checked ? (int)_NO_TRANSLATE_Limit.Value : AppSettings.MaxRevisionGraphCommits;
        }

        public string GetPathFilter()
        {
            return FileFilterCheck.Checked ? FileFilter.Text : "";
        }

        public string GetInMemAuthorFilter()
        {
            return AuthorCheck.Checked ? Author.Text : "";
        }

        public string GetInMemCommitterFilter()
        {
            return CommitterCheck.Checked ? Committer.Text : "";
        }

        public string GetInMemMessageFilter()
        {
            return MessageCheck.Checked ? Message.Text : "";
        }

        public bool GetIgnoreCase()
        {
            return IgnoreCase.Checked;
        }

        public string GetBranchFilter()
        {
            return (!AppSettings.BranchFilterEnabled || AppSettings.ShowCurrentBranchOnly) ? "" : BranchFilter.Text;
        }

        public bool SetBranchFilter(string? filter)
        {
            string newFilter = filter?.Trim() ?? string.Empty;
            if (string.Equals(newFilter, BranchFilter.Text, StringComparison.Ordinal))
            {
                // The filter hasn't changed
                return false;
            }

            BranchFilter.Text = newFilter;
            return true;
        }

        public void SetPathFilter(string? filter)
        {
            // If null the value is set from forms, then just uncheck
            if (filter is not null)
            {
                FileFilter.Text = filter;
            }

            FileFilterCheck.Checked = !string.IsNullOrWhiteSpace(filter);
        }

        private void OkClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
