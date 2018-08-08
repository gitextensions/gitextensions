using System;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCommitDiff()
        {
            InitializeComponent();
        }

        public FormCommitDiff([NotNull] GitUICommands commands, [CanBeNull] ObjectId objectId)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            CommitDiff.TextChanged += (s, e) => Text = CommitDiff.Text;

            CommitDiff.SetRevision(objectId, fileToSelect: null);
        }
    }
}