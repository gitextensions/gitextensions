using System.ComponentModel;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;

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
            ObjectId oldCommitHash = SelectedObjectId;

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

            bool isArtificialCommitForEmptyRepo = commitHash == "HEAD";

            lbCommits.Text = "";

            if (SelectedObjectId is null || isArtificialCommitForEmptyRepo)
            {
                textBoxCommitHash.Text = "";
            }
            else
            {
                textBoxCommitHash.Text = SelectedObjectId.ToShortString();
                ThreadHelper.FileAndForget(async () =>
                    {
                        ObjectId currentCheckout = Module.GetCurrentCheckout();

                        if (currentCheckout is null)
                        {
                            return;
                        }

                        string toRef = SelectedObjectId.IsArtificial ? "HEAD" : SelectedObjectId.ToString();
                        string text = Module.GetCommitCountString(currentCheckout, toRef);

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
