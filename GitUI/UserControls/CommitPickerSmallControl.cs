using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    public partial class CommitPickerSmallControl : GitModuleControl
    {
        public CommitPickerSmallControl()
        {
            InitializeComponent();
            InitializeComplete();
        }

        [CanBeNull]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectId SelectedObjectId { get; private set; }

        /// <summary>
        /// shows a message box if commitHash is invalid
        /// </summary>
        public void SetSelectedCommitHash(string commitHash)
        {
            var oldCommitHash = SelectedObjectId;

            SelectedObjectId = Module.RevParse(commitHash);

            if (SelectedObjectId == null && !commitHash.IsNullOrWhiteSpace())
            {
                SelectedObjectId = oldCommitHash;
                MessageBox.Show("The given commit hash is not valid for this repository and was therefore discarded.");
            }

            var isArtificialCommitForEmptyRepo = commitHash == "HEAD";

            if (SelectedObjectId == null || isArtificialCommitForEmptyRepo)
            {
                textBoxCommitHash.Text = "";
                lbCommits.Text = "";
            }
            else
            {
                textBoxCommitHash.Text = SelectedObjectId.ToShortString();
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                        var currentCheckout = Module.GetCurrentCheckout();

                        Debug.Assert(currentCheckout != null, "currentCheckout != null");

                        var text = Module.GetCommitCountString(currentCheckout.ToString(), SelectedObjectId.ToString());

                        await this.SwitchToMainThreadAsync();

                        lbCommits.Text = text;
                    });
            }
        }

        private void buttonPickCommit_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, SelectedObjectId?.ToString()))
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
