using System;
using GitUIPluginInterfaces;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCommitDiff()
        {
            InitializeComponent();
        }

        public FormCommitDiff(GitUICommands commands, ObjectId? objectId)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            CommitDiff.TextChanged += (s, e) => Text = CommitDiff.Text;

            CommitDiff.SetRevision(objectId, fileToSelect: null);
        }
    }
}
