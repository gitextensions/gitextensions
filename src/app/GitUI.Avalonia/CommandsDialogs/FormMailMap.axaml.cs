using System.Diagnostics;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public partial class FormMailMap : GitModuleForm
{
    private readonly TranslationString _mailmapOnlyInWorkingDirSupported =
        new(".mailmap is only supported when there is a working directory.");
    private readonly TranslationString _mailmapOnlyInWorkingDirSupportedCaption =
        new("No working directory");

    private readonly TranslationString _cannotAccessMailmap =
        new("Failed to save .mailmap." + Environment.NewLine + "Check if file is accessible.");
    private readonly TranslationString _cannotAccessMailmapCaption =
        new("Failed to save .mailmap");

    private readonly TranslationString _saveFileQuestion =
        new("Save changes to .mailmap?");
    private readonly TranslationString _saveFileQuestionCaption =
        new("Save changes?");

    public string MailMapFile = string.Empty;
    private readonly IFullPathResolver _fullPathResolver = null!;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormMailMap()
    {
        InitializeComponent();
        Save.Click += SaveClick;
        InitializeComplete();
    }

    public FormMailMap(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        _NO_TRANSLATE_MailMapText.IsReadOnly = false;
        Save.Click += SaveClick;
        InitializeComplete();
        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        LoadFile();
        _NO_TRANSLATE_MailMapText.TextLoaded += MailMapFileLoaded;
        FormMailMapLoad(this, e);
    }

    private void LoadFile()
    {
        try
        {
            string? path = _fullPathResolver.Resolve(".mailmap");
            if (File.Exists(path))
            {
                _ = _NO_TRANSLATE_MailMapText.ViewFileAsync(path);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void SaveClick(object sender, EventArgs e)
    {
        SaveFile();
        Close();
    }

    private bool SaveFile()
    {
        try
        {
            string? fileName = _fullPathResolver.Resolve(".mailmap");

            FileInfoExtensions
                .MakeFileTemporaryWritable(
                    fileName!, // catch NRE below
                    x =>
                    {
                        MailMapFile = _NO_TRANSLATE_MailMapText.GetText();
                        if (!MailMapFile.EndsWith(Environment.NewLine))
                        {
                            MailMapFile += Environment.NewLine;
                        }

                        File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(MailMapFile));
                    });

            UICommands.RepoChangedNotifier.Notify();

            return true;
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, _cannotAccessMailmap.Text + Environment.NewLine + ex.Message,
                _cannotAccessMailmapCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        FormMailMapFormClosing(this, e);
        base.OnClosing(e);
    }

    // Avalonia exposes WindowClosingEventArgs at the original FormClosing event boundary.
    private void FormMailMapFormClosing(object sender, WindowClosingEventArgs e)
    {
        bool needToClose = false;

        if (!IsFileUpToDate())
        {
            switch (MessageBoxes.Show(this, _saveFileQuestion.Text, _saveFileQuestionCaption.Text, WinFormsShims.MessageBoxButtons.YesNoCancel, WinFormsShims.MessageBoxIcon.Question))
            {
                case WinFormsShims.DialogResult.Yes:
                    if (SaveFile())
                    {
                        needToClose = true;
                    }

                    break;
                case WinFormsShims.DialogResult.No:
                    needToClose = true;
                    break;
            }
        }
        else
        {
            needToClose = true;
        }

        if (!needToClose)
        {
            e.Cancel = true;
        }
    }

    private void FormMailMapLoad(object sender, EventArgs e)
    {
        if (!Module.IsBareRepository())
        {
            return;
        }

        MessageBoxes.Show(this, _mailmapOnlyInWorkingDirSupported.Text, _mailmapOnlyInWorkingDirSupportedCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
        Close();
    }

    private bool IsFileUpToDate()
    {
        return MailMapFile == _NO_TRANSLATE_MailMapText.GetText();
    }

    private void MailMapFileLoaded(object? sender, EventArgs e)
    {
        MailMapFile = _NO_TRANSLATE_MailMapText.GetText();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormMailMap form)
    {
        public Editor.FileViewer Editor => form._NO_TRANSLATE_MailMapText;
        public Button Save => form.Save;
        public Label Help => form.label1;
    }
}
