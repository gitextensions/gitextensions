using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.HelperDialogs;
using GitCommands;

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

        private string _selectedCommitHash;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedCommitHash
        {
            get { return _selectedCommitHash; }
        }

        /// <summary>
        /// shows a message box if commitHash is invalid
        /// </summary>
        /// <param name="commitHash"></param>
        public void SetSelectedCommitHash(string commitHash)
        {
            string oldCommitHash = _selectedCommitHash;

            _selectedCommitHash = Module.RevParse(commitHash);

            if (_selectedCommitHash.IsNullOrEmpty() && !commitHash.IsNullOrWhiteSpace())
            {
                _selectedCommitHash = oldCommitHash;
                MessageBox.Show("The given commit hash is not valid for this repository and was therefore discarded.");
            }

            var isArtificialCommitForEmptyRepo = _selectedCommitHash == "HEAD";
            if (_selectedCommitHash.IsNullOrEmpty() || isArtificialCommitForEmptyRepo)
            {
                textBoxCommitHash.Text = "";
                lbCommits.Text = "";
            }
            else
            {
                textBoxCommitHash.Text = GitRevision.ToShortSha(_selectedCommitHash);
                Task.Factory.StartNew(() => this.Module.GetCommitCountString(this.Module.GetCurrentCheckout(), _selectedCommitHash))
                     .ContinueWith(t => lbCommits.Text = t.Result, TaskScheduler.FromCurrentSynchronizationContext());
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
