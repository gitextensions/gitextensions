using System;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    public partial class FormCompareToBranch : GitModuleForm
    {
        private FormCompareToBranch()
            : this(null, null)
        {
        }
        public FormCompareToBranch(GitUICommands aCommands, string selectedCommit): base(aCommands)
        {
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            InitializeComponent();
            Translate();
            branchSelector.Initialize(remote: true, containRevisons: null);
            branchSelector.CommitToCompare = selectedCommit;
            Activated += OnActivated;
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            branchSelector.Focus();
        }

        public string BranchName { get; private set; }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (branchSelector.SelectedBranchName.IsNotNullOrWhitespace())
            {
                BranchName = branchSelector.SelectedBranchName;
                DialogResult = DialogResult.OK;
                Close();
            }
            branchSelector.Focus();
        }
    }
}
