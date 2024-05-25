using GitExtensions.Extensibility.Git;

namespace GitUI.CommandsDialogs
{
    public partial class FormCompareToBranch : GitModuleForm
    {
        public FormCompareToBranch(IGitUICommands commands, ObjectId? selectedCommit)
            : base(commands)
        {
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            InitializeComponent();
            InitializeComplete();

            branchSelector.Initialize(remote: true, containRevisions: null);
            branchSelector.CommitToCompare = selectedCommit;
            Activated += OnActivated;
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            branchSelector.Focus();
        }

        public string? BranchName { get; private set; }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(branchSelector.SelectedBranchName))
            {
                BranchName = branchSelector.SelectedBranchName;
                DialogResult = DialogResult.OK;
                Close();
            }

            branchSelector.Focus();
        }
    }
}
