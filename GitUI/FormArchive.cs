using System;
using System.Windows.Forms;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormArchive : GitExtensionsForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox =
            new TranslationString("Select 1 revision to archive");

        private readonly TranslationString _saveFileDialogFilter =
            new TranslationString("Zip file (*.zip)");
        private readonly TranslationString _saveFileDialogCaption =
            new TranslationString("Save archive as");

        public FormArchive()
            : base(true)
        {
            InitializeComponent(); 
            Translate();
        }

        private void FormArchive_Load(object sender, EventArgs e)
        {
            revisionGrid1.Load();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (revisionGrid1.GetSelectedRevisions().Count != 1)
            {
                MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string revision = revisionGrid1.GetSelectedRevisions()[0].TreeGuid;

            using (var saveFileDialog = new SaveFileDialog { Filter = _saveFileDialogFilter.Text + "|*.zip", Title = _saveFileDialogCaption.Text })
            {

                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                FormProcess.ShowDialog(this, "archive --format=zip " + revision + " --output \"" + saveFileDialog.FileName + "\"");
                    Close();
                }
            }
        }
    }
}
