using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        /// <summary>
        /// For VS designer and translation test.
        /// </summary>
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
