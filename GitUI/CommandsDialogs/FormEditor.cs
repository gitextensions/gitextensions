using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormEditor : GitModuleForm
    {
        private readonly TranslationString _saveChanges = new("Do you want to save changes?");
        private readonly TranslationString _saveChangesCaption = new("Save changes");
        private readonly TranslationString _cannotOpenFile = new("Cannot open file:");
        private readonly TranslationString _cannotSaveFile = new("Cannot save file:");

        private readonly string? _fileName;

        private bool _hasChanges;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormEditor()
        {
            InitializeComponent();
        }

        public FormEditor(GitUICommands commands, string? fileName, bool showWarning, bool readOnly = false)
            : base(commands)
        {
            _fileName = fileName;
            InitializeComponent();
            panelMessage.BackColor = OtherColors.PanelMessageWarningColor;
            panelMessage.SetForeColorForBackColor();
            InitializeComplete();

            // for translation form
            if (_fileName is not null)
            {
                OpenFile(_fileName);
            }

            fileViewer.TextChanged += (s, e) => HasChanges = true;
            fileViewer.TextLoaded += (s, e) => HasChanges = false;
            panelMessage.Visible = showWarning;

            fileViewer.IsReadOnly = readOnly;
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
                fileViewer.ViewFileAsync(fileName);
                fileViewer.IsReadOnly = false;
                Text = fileName;

                // loading a new file from disk, the text hasn't been changed yet.
                HasChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _cannotOpenFile.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                            if (MessageBox.Show(this, $"{_cannotSaveFile.Text}{Environment.NewLine}{ex.Message}", TranslatedStrings.Error, MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
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
                DialogResult = DialogResult.OK;
            }
        }

        private void toolStripSaveButton_Click(object sender, EventArgs e)
        {
            SaveChangesShowException();
        }

        private void SaveChangesShowException()
        {
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBoxes.ShowError(this, $"{_cannotSaveFile.Text}{Environment.NewLine}{ex.Message}");
            }
        }

        private void SaveChanges()
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                if (fileViewer.FilePreamble is null || Module.FilesEncoding.GetPreamble().SequenceEqual(fileViewer.FilePreamble))
                {
                    FileUtility.SafeWriteAllText(_fileName, fileViewer.GetText(), Module.FilesEncoding);
                }
                else
                {
                    using MemoryStream bytes = new();
                    bytes.Write(fileViewer.FilePreamble, 0, fileViewer.FilePreamble.Length);
                    using (StreamWriter writer = new(bytes, Module.FilesEncoding))
                    {
                        writer.Write(fileViewer.GetText());
                    }

                    File.WriteAllBytes(_fileName, bytes.ToArray());
                }

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
                    SaveChangesShowException();
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormEditor _formEditor;

            public TestAccessor(FormEditor formEditor)
            {
                _formEditor = formEditor;
            }

            public FileViewer FileViewer => _formEditor.fileViewer;

            public bool HasChanges => _formEditor.HasChanges;

            public void SaveChanges() => _formEditor.SaveChanges();
        }
    }
}
