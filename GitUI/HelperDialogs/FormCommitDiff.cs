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

            this.AdjustForDpiScaling();
        }

        private FormCommitDiff()
            : this(null)
        {
        }

        public FormCommitDiff(GitUICommands commands, string revisionGuid)
            : this(commands)
        {
            CommitDiff.TextChanged += (s, e) => Text = CommitDiff.Text;

            CommitDiff.SetRevision(revisionGuid, fileToSelect: null);
        }
    }
}
