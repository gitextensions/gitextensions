using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.Tag
{
    public partial class FormDeleteTag : GitModuleForm
    {
        private readonly TranslationString _deleteTagMessageBoxCaption = new TranslationString("Delete Tag");

        public FormDeleteTag(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent(); Translate();
        }

        private void FormDeleteTagLoad(object sender, EventArgs e)
        {
            Tags.DisplayMember = "Name";
            Tags.DataSource = Module.GetHeads(true, false);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                string s = Module.DeleteTag(Tags.Text);

                if (!string.IsNullOrEmpty(s))
                    MessageBox.Show(this, s, _deleteTagMessageBoxCaption.Text);

                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
