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
        private bool _textIsChanged = false;


        public FormEditor()
        {
            InitializeComponent();
            Translate();
            fileViewer.TextChanged += fileViewer_TextChanged;
        }

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
                MessageBox.Show(this, "Cannot open file: " + Environment.NewLine + ex.Message, "Error");
                _fileName = string.Empty;
                Close();
            }
        }

        private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.No;

                DialogResult result = DialogResult.No;
                // only offer to save if there's something to save.
                if (_textIsChanged)
                {
                    result = MessageBox.Show(this, _saveChanges.Text, _saveChangesCaption.Text,
                                             MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                }



                if (result == DialogResult.Yes)
                {
                    SaveChanges();
                    this.DialogResult = DialogResult.Yes;
                }

                if (result == DialogResult.Cancel)
                {
                    this.DialogResult = DialogResult.Cancel;
                }

                SavePosition("fileeditor");
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(this, "Cannot save file: " + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
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
                MessageBox.Show(this, "Cannot save file: " + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveChanges()
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                File.WriteAllText(_fileName, fileViewer.GetText(), GitCommands.Settings.Encoding);
                
                // we've written the changes out to disk now, nothing to save.
                _textIsChanged = false;
            }
        }
    }
}
