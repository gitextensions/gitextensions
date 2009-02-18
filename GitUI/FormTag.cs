using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormTag : GitExtensionsForm
    {
        public FormTag()
        {
            InitializeComponent();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CreateTag_Click(object sender, EventArgs e)
        {
            try
            {

                if (GitRevisions.GetRevisions().Count != 1)
                {
                    MessageBox.Show("Select 1 revision to create the tag on.", "Tag");
                    return;
                }

                MessageBox.Show("Command executed \n" + GitCommands.GitCommands.Tag(Tagname.Text, GitRevisions.GetRevisions()[0].Guid), "Tag");

                Close();
            }
            catch
            {
            }

        }
    }
}
