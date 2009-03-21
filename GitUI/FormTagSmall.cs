using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormTagSmall : GitExtensionsForm
    {
        public FormTagSmall()
        {
            InitializeComponent();
        }

        public GitRevision Revision { get; set; }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {

                if (Revision == null)
                {
                    MessageBox.Show("Select 1 revision to create the tag on.", "Tag");
                    return;
                }
                else
                {
                    string s = GitCommands.GitCommands.Tag(TName.Text, Revision.Guid);
                    if (!string.IsNullOrEmpty(s))
                        MessageBox.Show(s, "Tag");
                    Close();
                }
            }
            catch
            {
            }
        }

        private void TName_TextChanged(object sender, EventArgs e)
        {

        }

        private void TName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Ok_Click(null, null);
        }
    }
}
