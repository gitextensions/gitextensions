using System;
using System.Windows.Forms;
using ResourceManager.Translation;
using GitCommands;

namespace GitUI
{
    public partial class FormArchive : GitModuleForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox =
            new TranslationString("Select 1 revision to archive");

        private readonly TranslationString _saveFileDialogFilter =
            new TranslationString("Zip file (*.zip)");
        private readonly TranslationString _saveFileDialogCaption =
            new TranslationString("Save archive as");

        private GitRevision _selectedRevision;
        public GitRevision SelectedRevision
        {
            get { return _selectedRevision; }
            set
            {
                _selectedRevision = value;
                commitSummaryUserControl1.Revision = SelectedRevision;
            }
        }

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormArchive()
            : this(null)
        {
        }

        public FormArchive(GitUICommands aCommands)
            : base(true, aCommands)
        {
            InitializeComponent();
            Translate();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string revision = SelectedRevision.Guid;

            using (var saveFileDialog = new SaveFileDialog { Filter = _saveFileDialogFilter.Text + "|*.zip", Title = _saveFileDialogCaption.Text })
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    FormProcess.ShowDialog(this, "archive --format=zip " + revision + " --output \"" + saveFileDialog.FileName + "\"");
                    Close();
                }
            }
        }

        private void btnChooseRevision_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, SelectedRevision.Guid))
            {
                if (chooseForm.ShowDialog() == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    SelectedRevision = chooseForm.SelectedRevision;
                }
            }
        }
    }
}
