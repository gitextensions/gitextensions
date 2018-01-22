using System;
using System.IO;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormEditor : GitModuleForm
    {
        private readonly TranslationString _saveChanges = new TranslationString("Do you want to save changes?");
        private readonly TranslationString _saveChangesCaption = new TranslationString("Save changes");
        private readonly TranslationString _cannotOpenFile = new TranslationString("Cannot open file:");
        private readonly TranslationString _cannotSaveFile = new TranslationString("Cannot save file:");
        private readonly TranslationString _error = new TranslationString("Error");
        private bool _hasChanges;
        private string _fileName;
        private bool _formClosing = false;

        public FormEditor(GitUICommands aCommands, string fileName, bool showWarning, string highlightingSyntax)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            // set highlighting syntax
            if (highlightingSyntax != null)
                fileViewer.SetHighlighting(highlightingSyntax);

            // for translation form
            if (fileName != null)
                OpenFile(fileName);

            fileViewer.TextChanged += (s, e) => HasChanges = true;
            fileViewer.TextLoaded += (s, e) => HasChanges = false;

            panelMessage.Visible = showWarning;
        }

        private bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                _hasChanges = value;
                toolStripSaveButton.Enabled = value;
            }
        }

        private void OpenFile(string fileName)
        {
            try
            {
                _fileName = fileName;
                fileViewer.ViewFile(_fileName);
                fileViewer.IsReadOnly = false;
                fileViewer.SetVisibilityDiffContextMenu(false, false);
                Text = _fileName;

                // loading a new file from disk, the text hasn't been changed yet.
                HasChanges = false;
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
            // prevent recursive calls to this method when setting DialogResult
            // due to Mono bug https://bugzilla.xamarin.com/show_bug.cgi?id=5040
            if(_formClosing)
            {
                return;
            }

            // only offer to save if there's something to save.
            if (HasChanges)
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
                        _formClosing = true;
                        DialogResult = DialogResult.OK;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                    default:
                        _formClosing = true;
                        DialogResult = DialogResult.Cancel;
                        break;
                }
            }
            else
            {
                _formClosing = true;
                DialogResult = DialogResult.OK;
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
                MessageBox.Show(this, _cannotSaveFile.Text + Environment.NewLine + ex.Message, _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveChanges()
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                File.WriteAllText(_fileName, fileViewer.GetText(), Module.FilesEncoding);

                // we've written the changes out to disk now, nothing to save.
                HasChanges = false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    Close();
                    return true;
                case Keys.Control | Keys.S:
                    SaveChanges();
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }
    }
}
