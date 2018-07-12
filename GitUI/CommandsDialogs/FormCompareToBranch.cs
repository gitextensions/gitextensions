using System;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public partial class FormCompareToBranch : GitModuleForm
    {
        private FormCompareToBranch()
            : this(null, null)
        {
        }

        public FormCompareToBranch([CanBeNull] GitUICommands commands, [CanBeNull] ObjectId selectedCommit) : base(commands)
        {
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            InitializeComponent();
            InitializeComplete();
            if (!IsUICommandsInitialized)
            {
                // UICommands is not initialized in translation unit test.
                return;
            }

            branchSelector.Initialize(remote: true, containRevisions: null);
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
