using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUI.HelperDialogs;

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
            set
            {
                _selectedCommitHash = Module.RevParse(value);
                if (_selectedCommitHash.IsNullOrEmpty())
                {
                    textBoxCommitHash.Text = "";
                }
                else
                {
                    textBoxCommitHash.Text = _selectedCommitHash.Substring(0, 10);
                }
            }
        }

        private void buttonPickCommit_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, SelectedCommitHash))
            {
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    SelectedCommitHash = chooseForm.SelectedRevision.Guid;
                }
            }
        }

        private void textBoxCommitHash_TextChanged(object sender, EventArgs e)
        {
            SelectedCommitHash = textBoxCommitHash.Text;
        }
    }
}
