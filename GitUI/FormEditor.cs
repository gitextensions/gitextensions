using System;
using System.Windows.Forms;
using System.IO;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormEditor : GitExtensionsForm
    {
        private readonly TranslationString _saveChanges = new TranslationString("Do you want to save changes?");
        private readonly TranslationString _saveChangesCaption = new TranslationString("Save changes");
        private readonly TranslationString _cannotOpenFile = new TranslationString("Cannot open file: ");
        private readonly TranslationString _cannotSaveFile = new TranslationString("Cannot save file: ");
        private readonly TranslationString _error = new TranslationString("Error");
        private bool _textIsChanged;

        public FormEditor(string fileName)
        {
            InitializeComponent();
            Translate();

            OpenFile(fileName);
            fileViewer.TextChanged += fileViewer_TextChanged;
            fileViewer.TextLoaded += fileViewer_TextLoaded;
        }

        void fileViewer_TextChanged(object sender, EventArgs e)
        {
            // I don't care what the old value is, it ought to be set to true whatever the old value is.
            _textIsChanged = true;
            toolStripSaveButton.Enabled = _textIsChanged;
        }

        void fileViewer_TextLoaded(object sender, EventArgs e)
        {
            //reset 'changed' flag
            _textIsChanged = false;
            toolStripSaveButton.Enabled = _textIsChanged;
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

                // loading a new file from disk, the text hasn't been changed yet.
                _textIsChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotOpenFile.Text + Environment.NewLine + ex.Message, _error.Text);
                _fileName = string.Empty;
                Close();
            }
        }

        private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            // only offer to save if there's something to save.
            if (_textIsChanged)
            {
                var saveChangesAnswer = MessageBox.Show(this, _saveChanges.Text, _saveChangesCaption.Text,
                                         MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (saveChangesAnswer)
                {
                    case DialogResult.Yes:
                        try
                        {
                            SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            if (MessageBox.Show(this, _cannotSaveFile.Text + Environment.NewLine + ex.Message, _error.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                        DialogResult = DialogResult.OK;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                    default:
                        DialogResult = DialogResult.Cancel;
                        break;
                }
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }

            SavePosition("fileeditor");
        }

        private void toolStripSaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotSaveFile.Text + Environment.NewLine + ex.Message, _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveChanges()
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                File.WriteAllText(_fileName, fileViewer.GetText(), GitCommands.Settings.FilesEncoding);

                // we've written the changes out to disk now, nothing to save.
                _textIsChanged = false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
