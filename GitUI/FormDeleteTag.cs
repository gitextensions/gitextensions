using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormDeleteTag : GitExtensionsForm
    {
        public FormDeleteTag()
        {
            InitializeComponent();
        }

        private void FormDeleteTag_Load(object sender, EventArgs e)
        {
            Tags.DisplayMember = "Name";
            Tags.DataSource = GitCommands.GitCommands.GetHeads(true, false);
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                string s = GitCommands.GitCommands.DeleteTag(Tags.Text);

                if (!string.IsNullOrEmpty(s))
                    MessageBox.Show(s, "Delete tag");

                Close();
            }
            catch
            {
            }
        }
    }
}
