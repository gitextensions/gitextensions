using System.Diagnostics;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public partial class FormGitAttributes : GitModuleForm
{
    // The original label1 text is a resx-backed help string; it is seeded here and not yet
    // routed through the Avalonia translation walker (deferred localization gap).
    private const string GitAttributesHelpText = """
        Edit the git attributes
        Define attributes per path

        Examples
        Mark all jpg files as binary:
        *.jpg binary

        Mark sln files as binary:
        *.sln binary

        Mark single file as text:
        weirdchars.txt text

        For more information run
        command "git help gitattributes"

        """;

    private readonly TranslationString _noWorkingDir =
        new(".gitattributes is only supported when there is a working directory.");
    private readonly TranslationString _noWorkingDirCaption =
        new("No working directory");

    private readonly TranslationString _cannotAccessGitattributes =
        new("Failed to save .gitattributes." + Environment.NewLine + "Check if file is accessible.");
    private readonly TranslationString _cannotAccessGitattributesCaption =
        new("Failed to save .gitattributes");

    private readonly TranslationString _saveFileQuestion =
        new("Save changes to .gitattributes?");
    private readonly TranslationString _saveFileQuestionCaption =
        new("Save changes?");

    public string GitAttributesFile = string.Empty;
    private readonly IFullPathResolver _fullPathResolver = null!;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormGitAttributes()
    {
        InitializeComponent();
        label1.Text = GitAttributesHelpText;
        Save.Click += SaveClick;
        InitializeComplete();
    }

    public FormGitAttributes(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        label1.Text = GitAttributesHelpText;
        _NO_TRANSLATE_GitAttributesText.IsReadOnly = false;
        Save.Click += SaveClick;
        InitializeComplete();
        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (Module.IsBareRepository())
        {
            MessageBoxes.Show(this, _noWorkingDir.Text, _noWorkingDirCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            Close();
            return;
        }

        LoadFile();
        _NO_TRANSLATE_GitAttributesText.TextLoaded += GitAttributesFileLoaded;
    }

    private void LoadFile()
    {
        try
        {
            string? path = _fullPathResolver.Resolve(".gitattributes");
            if (File.Exists(path))
            {
                _ = _NO_TRANSLATE_GitAttributesText.ViewFileAsync(path);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void SaveClick(object? sender, EventArgs e)
    {
        SaveFile();
        Close();
    }

    private bool SaveFile()
    {
        try
        {
            FileInfoExtensions
                .MakeFileTemporaryWritable(
                    _fullPathResolver.Resolve(".gitattributes")!, // catch NRE below
                    x =>
                    {
                        GitAttributesFile = _NO_TRANSLATE_GitAttributesText.GetText();
                        if (!GitAttributesFile.EndsWith(Environment.NewLine))
                        {
                            GitAttributesFile += Environment.NewLine;
                        }

                        File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(GitAttributesFile));
                    });

            return true;
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, _cannotAccessGitattributes.Text + Environment.NewLine + ex.Message,
                _cannotAccessGitattributesCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
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

        base.OnClosing(e);
    }

    private bool IsFileUpToDate()
    {
        return GitAttributesFile == _NO_TRANSLATE_GitAttributesText.GetText();
    }

    private void GitAttributesFileLoaded(object? sender, EventArgs e)
    {
        GitAttributesFile = _NO_TRANSLATE_GitAttributesText.GetText();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormGitAttributes form)
    {
        public Editor.FileViewer Editor => form._NO_TRANSLATE_GitAttributesText;
        public Button Save => form.Save;
    }
}
