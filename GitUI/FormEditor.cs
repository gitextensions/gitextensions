using System;
using System.Windows.Forms;
using System.IO;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormEditor : GitExtensionsForm
    {
        private readonly TranslationString _saveChanges = new TranslationString("Do you want to save changes?");
        private readonly TranslationString _saveChangesCaption = new TranslationString("Save changes");

        public FormEditor()
        {
            InitializeComponent();
            Translate();
        }

        public FormEditor(string fileName)
        {
            InitializeComponent();
            Translate();

            OpenFile(fileName);
        }

        private string _fileName;

        private void OpenFile(string fileName)
        {
            RestorePosition("fileeditor");

            try
            {
                _fileName = fileName;
                fileViewer.ViewFile(_fileName);
                fileViewer.IsReadOnly = false;
                fileViewer.EnableDiffContextMenu(false);
                Text = _fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open file: " + Environment.NewLine + ex.Message, "Error");
                _fileName = string.Empty;
                Close();
            }
        }

        private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (MessageBox.Show(this, _saveChanges.Text, _saveChangesCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SaveChanges();
                }

                SavePosition("fileeditor");
            }
            catch (Exception ex)
            {
                if (MessageBox.Show("Cannot save file: " + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void toolStripSaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save file: " + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveChanges()
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                File.WriteAllText(_fileName, fileViewer.GetText(), GitCommands.Settings.Encoding);
            }
        }
    }
}
