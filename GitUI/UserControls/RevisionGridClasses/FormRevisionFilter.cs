using System;
using GitCommands;

namespace GitUI.RevisionGridClasses
{
    public partial class FormRevisionFilter : GitExtensionsForm
    {
        public FormRevisionFilter()
        {
            InitializeComponent();
            Translate();

            LimitCheck.Checked = AppSettings.MaxRevisionGraphCommits > 0;
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
            return AuthorCheck.Checked ||
                    CommitterCheck.Checked ||
                    MessageCheck.Checked ||
                    SinceCheck.Checked ||
                    CheckUntil.Checked ||
                    FileFilterCheck.Checked;
        }

        public string GetRevisionFilter()
        {
            var filter = "";
            if (AuthorCheck.Checked && GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Author.Text))
            {
                filter += string.Format(" --author=\"{0}\"", Author.Text);
            }

            if (CommitterCheck.Checked && GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Committer.Text))
            {
                filter += string.Format(" --committer=\"{0}\"", Committer.Text);
            }

            if (MessageCheck.Checked && GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Message.Text))
            {
                filter += string.Format(" --grep=\"{0}\"", Message.Text);
            }

            if (!string.IsNullOrEmpty(filter) && IgnoreCase.Checked)
            {
                filter += " --regexp-ignore-case";
            }

            if (SinceCheck.Checked)
            {
                filter += string.Format(" --since=\"{0}\"", Since.Value.ToString("yyyy-MM-dd hh:mm:ss"));
            }

            if (CheckUntil.Checked)
            {
                filter += string.Format(" --until=\"{0}\"", Until.Value.ToString("yyyy-MM-dd hh:mm:ss"));
            }

            if (LimitCheck.Checked && _NO_TRANSLATE_Limit.Value > 0)
            {
                filter += string.Format(" --max-count=\"{0}\"", (int)_NO_TRANSLATE_Limit.Value);
            }

            return filter;
        }

        public string GetPathFilter()
        {
            var filter = "";
            if (FileFilterCheck.Checked)
            {
                filter += string.Format(" \"{0}\"", FileFilter.Text.ToPosixPath());
            }

            return filter;
        }

        public bool ShouldHideGraph()
        {
            return AuthorCheck.Checked || CommitterCheck.Checked ||
                   MessageCheck.Checked || FileFilterCheck.Checked;
        }

        public string GetInMemAuthorFilter()
        {
            if (AuthorCheck.Checked && !GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Author.Text))
            {
                return Author.Text;
            }

            return string.Empty;
        }

        public string GetInMemCommitterFilter()
        {
            if (CommitterCheck.Checked && !GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Committer.Text))
            {
                return Committer.Text;
            }

            return string.Empty;
        }

        public string GetInMemMessageFilter()
        {
            if (MessageCheck.Checked && !GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Message.Text))
            {
                return Message.Text;
            }

            return string.Empty;
        }

        public bool GetIgnoreCase()
        {
            return IgnoreCase.Checked;
        }

        public string GetBranchFilter()
        {
            if (!AppSettings.BranchFilterEnabled || AppSettings.ShowCurrentBranchOnly)
            {
                return string.Empty;
            }

            return BranchFilter.Text;
        }

        public void SetBranchFilter(string filter)
        {
            BranchFilter.Text = filter;
        }

        private void OkClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}