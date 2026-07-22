using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Editor;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormEditor : GitModuleForm
{
    private readonly TranslationString _saveChanges = new("Do you want to save changes?");
    private readonly TranslationString _saveChangesCaption = new("Save changes");
    private readonly TranslationString _cannotOpenFile = new("Cannot open file:");
    private readonly TranslationString _cannotSaveFile = new("Cannot save file:");

    private readonly string? _fileName;

    private bool _hasChanges;

    public FormEditor()
    {
        InitializeComponent();
        ConfigureControls();
        InitializeComplete();
    }

    public FormEditor(IGitUICommands commands, string? fileName, bool showWarning, bool readOnly = false, int? lineNumber = null)
        : base(commands, enablePositionRestore: true)
    {
        _fileName = fileName;
        InitializeComponent();
        fileViewer.UICommandsSource = this;
        ConfigureControls();
        InitializeComplete();

        // for translation form
        if (_fileName is not null)
        {
            OpenFile(_fileName, lineNumber);
        }

        panelMessage.IsVisible = showWarning;
        fileViewer.IsReadOnly = readOnly;
    }

    private bool HasChanges
    {
        get => _hasChanges;
        set
        {
            _hasChanges = value;
            toolStripSaveButton.IsEnabled = value;
        }
    }

    private void ConfigureControls()
    {
        // XML attribute whitespace normalization would collapse the original CRLFs and change
        // the established labelWarning.Text translation source.
        labelWarning.Text = "Here be dragons!\r\nChanging this file by hand can be harmful and might break something.\r\nIf you are not sure just close this window.";
        toolStripSaveButton.Click += toolStripSaveButton_Click;
        fileViewer.TextChanged += (_, _) => HasChanges = true;
        fileViewer.TextLoaded += (_, _) => HasChanges = false;
    }

    private void OpenFile(string fileName, int? line = null)
    {
        try
        {
            _ = fileViewer.ViewFileAsync(fileName, line: line);
            fileViewer.IsReadOnly = false;
            Text = fileName;

            // loading a new file from disk, the text hasn't been changed yet.
            HasChanges = false;
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(
                this,
                _cannotOpenFile.Text + Environment.NewLine + ex.Message,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            Close();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!ConfirmClose())
        {
            e.Cancel = true;
        }

        base.OnClosing(e);
    }

    private bool ConfirmClose()
    {
        if (!HasChanges)
        {
            SetDialogResultOnClose(WinFormsShims.DialogResult.OK);
            return true;
        }

        WinFormsShims.DialogResult saveChangesAnswer = MessageBoxes.Show(
            this,
            _saveChanges.Text,
            _saveChangesCaption.Text,
            WinFormsShims.MessageBoxButtons.YesNoCancel,
            WinFormsShims.MessageBoxIcon.Question);
        switch (saveChangesAnswer)
        {
            case WinFormsShims.DialogResult.Yes:
                try
                {
                    SaveChanges();
                }
                catch (Exception ex)
                {
                    if (MessageBoxes.Show(
                            this,
                            $"{_cannotSaveFile.Text}{Environment.NewLine}{ex.Message}",
                            TranslatedStrings.Error,
                            WinFormsShims.MessageBoxButtons.OKCancel,
                            WinFormsShims.MessageBoxIcon.Error) == WinFormsShims.DialogResult.Cancel)
                    {
                        return false;
                    }
                }

                SetDialogResultOnClose(WinFormsShims.DialogResult.OK);
                return true;
            case WinFormsShims.DialogResult.Cancel:
                return false;
            default:
                SetDialogResultOnClose(WinFormsShims.DialogResult.Cancel);
                return true;
        }
    }

    private void toolStripSaveButton_Click(object? sender, EventArgs e)
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
                FileUtility.SafeWriteAllText(_fileName, fileViewer.GetText(), Module.FilesEncoding, filePreamble: []);
            }
            else
            {
                FileUtility.SafeWriteAllText(_fileName, fileViewer.GetText(), Module.FilesEncoding, fileViewer.FilePreamble);
            }

            // we've written the changes out to disk now, nothing to save.
            HasChanges = false;
        }
    }

    public override bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
        if (keyData == (WinFormsShims.Keys.Control | WinFormsShims.Keys.S))
        {
            SaveChangesShowException();
            return true;
        }

        return base.ProcessHotkey(keyData);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormEditor form)
    {
        public FileViewer FileViewer => form.fileViewer;

        public bool HasChanges => form.HasChanges;

        public bool IsSaveEnabled => form.toolStripSaveButton.IsEnabled;

        public bool IsWarningVisible => form.panelMessage.IsVisible;

        public bool ConfirmClose() => form.ConfirmClose();

        public void SaveChanges() => form.SaveChanges();

        public void LoadText(string fileName, string text, bool hasChanges)
        {
            form.Text = fileName;
            form.fileViewer.ViewText(fileName, text);
            form.fileViewer.IsReadOnly = false;
            form.HasChanges = hasChanges;
        }
    }
}
