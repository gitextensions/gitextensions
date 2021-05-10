using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    public partial class CommitPickerSmallControl : GitModuleControl
    {
        /// <summary>
        /// Occurs whenever the selected commit hash changes.
        /// </summary>
        public event EventHandler? SelectedObjectIdChanged;

        public CommitPickerSmallControl()
        {
            InitializeComponent();
            InitializeComplete();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectId? SelectedObjectId { get; private set; }

        /// <summary>
        /// shows a message box if commitHash is invalid.
        /// </summary>
        public void SetSelectedCommitHash(string? commitHash)
        {
            var oldCommitHash = SelectedObjectId;

            SelectedObjectId = Module.RevParse(commitHash);

            if (SelectedObjectId is null && !string.IsNullOrWhiteSpace(commitHash))
            {
                SelectedObjectId = oldCommitHash;
                MessageBox.Show("The given commit hash is not valid for this repository and was therefore discarded.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                SelectedObjectIdChanged?.Invoke(this, EventArgs.Empty);
            }

            var isArtificialCommitForEmptyRepo = commitHash == "HEAD";

            if (SelectedObjectId is null || isArtificialCommitForEmptyRepo)
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

                        Validates.NotNull(currentCheckout);

                        var text = Module.GetCommitCountString(currentCheckout.ToString(), SelectedObjectId.ToString());

                        await this.SwitchToMainThreadAsync();

                        lbCommits.Text = text;
                    });
            }
        }

        private void buttonPickCommit_Click(object sender, EventArgs e)
        {
            using FormChooseCommit chooseForm = new(UICommands, SelectedObjectId?.ToString());
            if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision is not null)
            {
                SetSelectedCommitHash(chooseForm.SelectedRevision.Guid);
            }
        }

        private void textBoxCommitHash_TextLeave(object sender, EventArgs e)
        {
            SetSelectedCommitHash(textBoxCommitHash.Text.Trim());
        }
    }
}
