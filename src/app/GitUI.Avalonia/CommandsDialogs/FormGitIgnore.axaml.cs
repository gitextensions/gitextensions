using System.Diagnostics;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.GitIgnoreDialog;
using GitUI.Compat;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormGitIgnore : GitModuleForm
{
    // The original label1 text is a resx-backed help string; it is seeded here and not yet
    // routed through the Avalonia translation walker (deferred localization gap).
    private const string GitIgnoreHelpText = """
        Specify filepatterns you want git to ignore.

        Example:
        #Ignore thumbnails created by Windows
        Thumbs.db
        #Ignore files built by Visual Studio
        *.obj
        *.exe
        *.pdb
        *.user
        *.aps
        *.pch
        *.vspscc
        *_i.c
        *_p.c
        *.ncb
        *.suo
        *.tlb
        *.tlh
        *.bak
        *.cache
        *.ilk
        *.log
        [Bb]in
        [Dd]ebug*/
        *.lib
        *.sbr
        obj/
        [Rr]elease*/
        _ReSharper*/
        [Tt]est[Rr]esult*
        .vs/
        .idea/
        #Nuget packages folder
        packages/
        """;

    private readonly TranslationString _gitignoreOnlyInWorkingDirSupportedCaption =
        new("No working directory");

    private readonly TranslationString _saveFileQuestionCaption =
        new("Save changes?");

    private readonly bool _localExclude;
    private string _originalGitIgnoreFileContent = string.Empty;

    #region default patterns

    private static readonly string DefaultIgnorePatternsFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitExtensions/DefaultIgnorePatterns.txt");

    private static readonly string[] DefaultIgnorePatterns =
    [
        "#Ignore thumbnails created by Windows",
        "Thumbs.db",
        "#Ignore files built by Visual Studio",
        "*.obj",
        "*.exe",
        "*.pdb",
        "*.user",
        "*.aps",
        "*.pch",
        "*.vspscc",
        "*_i.c",
        "*_p.c",
        "*.ncb",
        "*.suo",
        "*.tlb",
        "*.tlh",
        "*.bak",
        "*.cache",
        "*.ilk",
        "*.log",
        "[Bb]in",
        "[Dd]ebug*/",
        "*.lib",
        "*.sbr",
        "obj/",
        "[Rr]elease*/",
        "_ReSharper*/",
        "[Tt]est[Rr]esult*",
        ".vs/",
        ".idea/",
        "#Nuget packages folder",
        "packages/"
    ];

    #endregion

    private readonly IGitIgnoreDialogModel _dialogModel = null!;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormGitIgnore()
    {
        InitializeComponent();
        label1.Text = GitIgnoreHelpText;
        WireEvents();
        InitializeComplete();
    }

    public FormGitIgnore(IGitUICommands commands, bool localExclude)
        : base(commands, enablePositionRestore: false)
    {
        _localExclude = localExclude;
        InitializeComponent();
        label1.Text = GitIgnoreHelpText;
        _NO_TRANSLATE_GitIgnoreEdit.IsReadOnly = false;
        WireEvents();
        InitializeComplete();

        _dialogModel = CreateGitIgnoreDialogModel(localExclude);

        Text = _dialogModel.FormCaption;
    }

    // WinForms wired these handlers in the Designer.
    private void WireEvents()
    {
        Save.Click += SaveClick;
        btnCancel.Click += btnCancel_Click;
        AddDefault.Click += AddDefaultClick;
        AddPattern.Click += AddPattern_Click;
        lnkGitIgnorePatterns.Click += lnkGitIgnorePatterns_LinkClicked;
        lnkGitIgnoreGenerate.Click += lnkGitIgnoreGenerate_LinkClicked;
    }

    private IGitIgnoreDialogModel CreateGitIgnoreDialogModel(bool localExclude)
    {
        if (localExclude)
        {
            return new GitLocalExcludeModel(Module);
        }

        return new GitIgnoreModel(Module);
    }

    private string? ExcludeFile => _dialogModel.ExcludeFile;

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (Module.IsBareRepository())
        {
            MessageBoxes.Show(this, _dialogModel.FileOnlyInWorkingDirSupported, _gitignoreOnlyInWorkingDirSupportedCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            Close();
            return;
        }

        LoadGitIgnore();
        _NO_TRANSLATE_GitIgnoreEdit.TextLoaded += GitIgnoreFileLoaded;
    }

    private void LoadGitIgnore()
    {
        try
        {
            if (File.Exists(ExcludeFile))
            {
                _ = _NO_TRANSLATE_GitIgnoreEdit.ViewFileAsync(ExcludeFile!);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void SaveClick(object? sender, EventArgs e)
    {
        SaveGitIgnore();
        Close();
    }

    private bool SaveGitIgnore()
    {
        if (!HasUnsavedChanges() || ExcludeFile is null)
        {
            return false;
        }

        try
        {
            FileInfoExtensions
                .MakeFileTemporaryWritable(
                    ExcludeFile,
                    x =>
                    {
                        string fileContent = _NO_TRANSLATE_GitIgnoreEdit.GetText();
                        if (!fileContent.EndsWith(Environment.NewLine))
                        {
                            fileContent += Environment.NewLine;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(x)!);
                        File.WriteAllBytes(x, GitModule.SystemEncoding.GetBytes(fileContent));
                        _originalGitIgnoreFileContent = fileContent;
                    });
            return true;
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, _dialogModel.CannotAccessFile + Environment.NewLine + ex.Message,
                _dialogModel.CannotAccessFileCaption, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (HasUnsavedChanges())
        {
            switch (MessageBoxes.Show(this, _dialogModel.SaveFileQuestion, _saveFileQuestionCaption.Text,
                                WinFormsShims.MessageBoxButtons.YesNoCancel, WinFormsShims.MessageBoxIcon.Question))
            {
                case WinFormsShims.DialogResult.Yes:
                    if (!SaveGitIgnore())
                    {
                        e.Cancel = true;
                    }

                    break;
                case WinFormsShims.DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        base.OnClosing(e);
    }

    private void AddDefaultClick(object? sender, EventArgs e)
    {
        string[] defaultIgnorePatterns = File.Exists(DefaultIgnorePatternsFile) ? File.ReadAllLines(DefaultIgnorePatternsFile) : DefaultIgnorePatterns;

        string currentFileContent = _NO_TRANSLATE_GitIgnoreEdit.GetText();
        string[] patternsToAdd = [.. defaultIgnorePatterns.Except(currentFileContent.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries))];
        if (patternsToAdd.Length == 0)
        {
            return;
        }

        // workaround to prevent GitIgnoreFileLoaded event handling (it causes wrong _originalGitIgnoreFileContent update)
        // TODO: implement in FileViewer separate events for loading text from file and for setting text directly via ViewText
        _NO_TRANSLATE_GitIgnoreEdit.InvokeAndForget(async () =>
            {
                _NO_TRANSLATE_GitIgnoreEdit.TextLoaded -= GitIgnoreFileLoaded;
                await _NO_TRANSLATE_GitIgnoreEdit.ViewTextAsync(
                    ExcludeFile,
                    $"{currentFileContent}{Environment.NewLine}{string.Join(Environment.NewLine, patternsToAdd)}{Environment.NewLine}");
                _NO_TRANSLATE_GitIgnoreEdit.TextLoaded += GitIgnoreFileLoaded;
            });
    }

    private void AddPattern_Click(object? sender, EventArgs e)
    {
        SaveGitIgnore();
        UICommands.StartAddToGitIgnoreDialog(this, _localExclude, "*.dll");
        LoadGitIgnore();
    }

    private bool HasUnsavedChanges()
    {
        return _originalGitIgnoreFileContent != _NO_TRANSLATE_GitIgnoreEdit.GetText();
    }

    private void GitIgnoreFileLoaded(object? sender, EventArgs e)
    {
        _originalGitIgnoreFileContent = _NO_TRANSLATE_GitIgnoreEdit.GetText();
    }

    private void lnkGitIgnorePatterns_LinkClicked(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/github/gitignore");
    }

    private void lnkGitIgnoreGenerate_LinkClicked(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://www.gitignore.io/");
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        Close();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormGitIgnore form)
    {
        public Editor.FileViewer Editor => form._NO_TRANSLATE_GitIgnoreEdit;
        public Button Save => form.Save;
        public Button AddDefault => form.AddDefault;
        public Button AddPattern => form.AddPattern;
        public Button Cancel => form.btnCancel;
    }
}
