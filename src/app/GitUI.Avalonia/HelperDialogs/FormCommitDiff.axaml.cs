using GitExtensions.Extensibility.Git;

namespace GitUI.HelperDialogs;

public sealed partial class FormCommitDiff : GitModuleForm
{
    public FormCommitDiff()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormCommitDiff(IGitUICommands commands, ObjectId objectId)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        InitializeComplete();

        CommitDiff.UICommandsSource = this;
        CommitDiff.TextChanged += (s, e) => Text = CommitDiff.Text;

        CommitDiff.SetRevision(objectId, fileToSelect: null);
    }
}
