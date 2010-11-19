using System;
using GitCommands;

namespace GitUI
{
    public partial class FormRevisionFilter : GitExtensionsForm
    {
        public FormRevisionFilter()
        {
            InitializeComponent();
            Translate();
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
            Settings.BranchFilterEnabled = BranchFilterCheck.Checked;
            EnableFilters();
        }

        private void OnShowCurrentBranchOnlyCheckedChanged(object sender, EventArgs e)
        {
            Settings.ShowCurrentBranchOnly = CurrentBranchOnlyCheck.Checked;
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

            BranchFilterCheck.Checked = Settings.BranchFilterEnabled;
            CurrentBranchOnlyCheck.Checked = Settings.ShowCurrentBranchOnly;

            CurrentBranchOnlyCheck.Enabled = BranchFilterCheck.Checked;
            BranchFilter.Enabled = BranchFilterCheck.Checked &&
                                   !CurrentBranchOnlyCheck.Checked;
        }

        public string GetFilter()
        {
            var filter = "";
            if (SinceCheck.Checked)
                filter += string.Format(" --since=\"{0}\"", Since.Value);
            if (CheckUntil.Checked)
                filter += string.Format(" --until=\"{0}\"", Until.Value);
            if (AuthorCheck.Checked && GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Author.Text))
                filter += string.Format(" --author=\"{0}\"", Author.Text);
            if (CommitterCheck.Checked && GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Committer.Text))
                filter += string.Format(" --committer=\"{0}\"", Committer.Text);
            if (MessageCheck.Checked && GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Message.Text))
                filter += string.Format(" --grep=\"{0}\"", Message.Text);
            if (LimitCheck.Checked)
                filter += string.Format(" --max-count=\"{0}\"", ((int)_NO_TRANSLATE_Limit.Value).ToString());
            if (!string.IsNullOrEmpty(filter) && IgnoreCase.Checked)
                filter += " --regexp-ignore-case";
            if (FileFilterCheck.Checked)
                filter += string.Format(" -- \"{0}\"", FileFilter.Text.Replace('\\', '/'));

            return filter;
        }

        public string GetInMemAuthorFilter()
        {
            if (AuthorCheck.Checked && !GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Author.Text))
                return Author.Text;
            else
                return string.Empty;
        }

        public string GetInMemCommitterFilter()
        {
            if (CommitterCheck.Checked && !GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Committer.Text))
                return Committer.Text;
            else
                return string.Empty;
        }

        public string GetInMemMessageFilter()
        {
            if (MessageCheck.Checked && !GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(Message.Text))
                return Message.Text;
            else
                return string.Empty;
        }

        public bool GetIgnoreCase()
        {
            return IgnoreCase.Checked;
        }

        public string GetBranchFilter()
        {
            if (!Settings.BranchFilterEnabled || Settings.ShowCurrentBranchOnly)
                return String.Empty;

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