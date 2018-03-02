using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    public partial class CommitPickerSmallControl : GitModuleControl
    {
        public CommitPickerSmallControl()
        {
            InitializeComponent();
            Translate();
        }

        ////public event EventHandler<EventArgs> OnCommitSelected;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedCommitHash { get; private set; }

        /// <summary>
        /// shows a message box if commitHash is invalid
        /// </summary>
        public void SetSelectedCommitHash(string commitHash)
        {
            string oldCommitHash = SelectedCommitHash;

            SelectedCommitHash = Module.RevParse(commitHash)?.ToString() ?? "";

            if (SelectedCommitHash.IsNullOrEmpty() && !commitHash.IsNullOrWhiteSpace())
            {
                SelectedCommitHash = oldCommitHash;
                MessageBox.Show("The given commit hash is not valid for this repository and was therefore discarded.");
            }

            var isArtificialCommitForEmptyRepo = SelectedCommitHash == "HEAD";
            if (SelectedCommitHash.IsNullOrEmpty() || isArtificialCommitForEmptyRepo)
            {
                textBoxCommitHash.Text = "";
                lbCommits.Text = "";
            }
            else
            {
                textBoxCommitHash.Text = GitRevision.ToShortSha(SelectedCommitHash);
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                        var text = Module.GetCommitCountString(Module.GetCurrentCheckout(), SelectedCommitHash);

                        await this.SwitchToMainThreadAsync();

                        lbCommits.Text = text;
                    });
            }
        }

        private void buttonPickCommit_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, SelectedCommitHash))
            {
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    SetSelectedCommitHash(chooseForm.SelectedRevision.Guid);
                }
            }
        }

        private void textBoxCommitHash_TextLeave(object sender, EventArgs e)
        {
            SetSelectedCommitHash(textBoxCommitHash.Text.Trim());
        }
    }
}
