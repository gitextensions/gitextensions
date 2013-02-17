using System;
using System.Windows.Forms;
using GitUI.HelperDialogs;
using ResourceManager.Translation;
using GitCommands;
using System.IO;

namespace GitUI.CommandsDialogs
{
    public partial class FormArchive : GitModuleForm
    {
        private readonly TranslationString _saveFileDialogFilterZip =
            new TranslationString("Zip file (*.zip)");

        private readonly TranslationString _saveFileDialogFilterTar =
            new TranslationString("Tar file (*.tar)");

        private readonly TranslationString _saveFileDialogCaption =
            new TranslationString("Save archive as");

        private GitRevision _selectedRevision;
        public GitRevision SelectedRevision
        {
            get { return _selectedRevision; }
            set
            {
                _selectedRevision = value;
                commitSummaryUserControl1.Revision = _selectedRevision;
            }
        }

        private enum OutputFormat
        {
            Zip,
            Tar
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

        private void FormArchive_Load(object sender, EventArgs e)
        {
            buttonArchiveRevision.Focus();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string revision = SelectedRevision.Guid;

            string fileFilterCaption = GetSelectedOutputFormat() == OutputFormat.Zip ? _saveFileDialogFilterZip.Text : _saveFileDialogFilterTar.Text;
            string fileFilterEnding = GetSelectedOutputFormat() == OutputFormat.Zip ? "zip" : "tar";

            // TODO (feature): if there is a tag on the revision use the tag name as suggestion
            // TODO (feature): let user decide via GUI
            string filenameSuggestion = string.Format("{0}_{1}", new DirectoryInfo(Module.WorkingDir).Name, revision);

            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = string.Format("{0}|*.{1}", fileFilterCaption, fileFilterEnding),
                Title = _saveFileDialogCaption.Text,
                FileName = filenameSuggestion
            })
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string format = GetSelectedOutputFormat() == OutputFormat.Zip ? "zip" : "tar";

                    FormProcess.ShowDialog(this,
                        string.Format("archive --format={0} {1} --output \"{2}\"",
                        format, revision, saveFileDialog.FileName));
                    Close();
                }
            }
        }

        private OutputFormat GetSelectedOutputFormat()
        {
            return _NO_TRANSLATE_radioButtonFormatZip.Checked ? OutputFormat.Zip : OutputFormat.Tar;
        }

        private void btnChooseRevision_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, SelectedRevision.Guid))
            {
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    SelectedRevision = chooseForm.SelectedRevision;
                }
            }
        }
    }
}
