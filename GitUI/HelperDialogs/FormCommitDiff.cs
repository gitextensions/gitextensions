using System;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private FormCommitDiff(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            Translate();
        }

        private FormCommitDiff()
            : this(null)
        {
        }

        public FormCommitDiff(GitUICommands commands, string revisionGuid)
            : this(commands)
        {
            CommitDiff.TextChanged += CommitDiff_TextChanged;
            CommitDiff.SetRevision(revisionGuid, null);
        }

        private void CommitDiff_TextChanged(object sender, EventArgs e)
        {
            Text = CommitDiff.Text;
        }
    }
}
