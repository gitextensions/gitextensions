using System;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public partial class FormCompareToBranch : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCompareToBranch()
        {
            InitializeComponent();
        }

        public FormCompareToBranch([NotNull] GitUICommands commands, [CanBeNull] ObjectId selectedCommit)
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
