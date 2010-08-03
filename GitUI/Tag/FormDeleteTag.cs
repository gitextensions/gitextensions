using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace GitUI.Tag
{
    public partial class FormDeleteTag : GitExtensionsForm
    {
        public FormDeleteTag()
        {
            InitializeComponent(); Translate();
        }

        private void FormDeleteTagLoad(object sender, EventArgs e)
        {
            Tags.DisplayMember = "Name";
            Tags.DataSource = GitCommands.GitCommands.GetHeads(true, false);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                string s = GitCommands.GitCommands.DeleteTag(Tags.Text);

                if (!string.IsNullOrEmpty(s))
                    MessageBox.Show(s, "Delete tag");

                Close();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
