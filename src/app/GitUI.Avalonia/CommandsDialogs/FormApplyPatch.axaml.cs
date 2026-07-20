using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormApplyPatch.cs. It deliberately keeps the original
// control and handler names so middle-of-patch recovery maps directly between front ends.
public partial class FormApplyPatch : GitModuleForm
{
    private readonly TranslationString _conflictResolvedText = new("Conflicts resolved");
    private readonly TranslationString _conflictMergetoolText = new("&Solve conflicts");
    private readonly TranslationString _selectPatchFileFilter = new("Patch file (*.Patch)");
    private readonly TranslationString _selectPatchFileCaption = new("Select patch file");
    private readonly TranslationString _noFileSelectedText = new("Please select a patch to apply");
    private readonly TranslationString _applyPatchMsgBox = new("Apply patch");

    private static readonly List<PatchFile> Skipped = [];

    public FormApplyPatch()
    {
        InitializeComponent();
        InitializeStaticContent();
        InitializeComplete();
    }

    public FormApplyPatch(IGitUICommands commands)
        : base(commands, true)
    {
        InitializeComponent();
        InitializeStaticContent();

        PatchGrid.UICommandsSource = this;
        PatchGrid.IsManagingRebase = false;
        PatchGrid.SetSkipped(Skipped);

        InitializeComplete();
    }

    public void SetPatchFile(string name)
    {
        PatchFileMode.IsChecked = true;
        PatchFile.Text = name;
    }

    public void SetPatchDir(string name)
    {
        PatchDirMode.IsChecked = true;
        PatchDir.Text = name;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        Text = $"{_applyPatchMsgBox.Text} ({Module.WorkingDir})";
        IgnoreWhitespace.IsChecked = AppSettings.ApplyPatchIgnoreWhitespace;
        SignOff.IsChecked = AppSettings.ApplyPatchSignOff;
        PatchFile.Focus();
        PatchFile.SelectAll();
        EnableButtons();
    }

    private void InitializeStaticContent()
    {
        BrowsePatch.Click += BrowsePatch_Click;
        Apply.Click += Apply_Click;
        Mergetool.Click += Mergetool_Click;
        Skip.Click += Skip_Click;
        Abort.Click += Abort_Click;
        Resolved.Click += Resolved_Click;
        AddFiles.Click += AddFiles_Click;
        BrowseDir.Click += BrowseDir_Click;
        PatchFileMode.IsCheckedChanged += PatchFileMode_CheckedChanged;
        PatchDirMode.IsCheckedChanged += PatchFileMode_CheckedChanged;
        SolveMergeConflicts.Click += SolveMergeConflicts_Click;
        IgnoreWhitespace.IsCheckedChanged += IgnoreWhitespace_CheckedChanged;
        SignOff.IsCheckedChanged += SignOff_CheckedChanged;
        SolveMergeConflicts.Background = new SolidColorBrush(Colors.Goldenrod);
    }

    private void EnableButtons()
    {
        bool inPatch = Module.InTheMiddleOfPatch();
        bool conflictedMerge = Module.InTheMiddleOfConflictedMerge();

        Apply.IsEnabled = !inPatch;
        IgnoreWhitespace.IsEnabled = !inPatch;
        SignOff.IsEnabled = !inPatch;
        PatchFileMode.IsEnabled = !inPatch;
        PatchDirMode.IsEnabled = !inPatch;
        AddFiles.IsEnabled = inPatch;
        Resolved.IsEnabled = inPatch && !conflictedMerge;
        Mergetool.IsEnabled = inPatch && conflictedMerge;
        Skip.IsEnabled = inPatch;
        Abort.IsEnabled = inPatch;

        PatchFile.IsEnabled = !inPatch && PatchFileMode.IsChecked == true;
        PatchFile.IsReadOnly = PatchFileMode.IsChecked != true;
        BrowsePatch.IsEnabled = !inPatch && PatchFileMode.IsChecked == true;
        PatchDir.IsEnabled = !inPatch && PatchDirMode.IsChecked == true;
        PatchDir.IsReadOnly = PatchDirMode.IsChecked != true;
        BrowseDir.IsEnabled = !inPatch && PatchDirMode.IsChecked == true;

        if (PatchGrid.PatchFiles is null || PatchGrid.PatchFiles.Count == 0)
        {
            PatchGrid.Initialize();
        }
        else
        {
            PatchGrid.RefreshGrid();
        }

        SolveMergeConflicts.IsVisible = conflictedMerge;

        Resolved.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_conflictResolvedText.Text);
        Mergetool.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_conflictMergetoolText.Text);
        ContinuePanel.Background = null;
        MergeToolPanel.Background = null;

        if (conflictedMerge)
        {
            Mergetool.Content = $">{AvaloniaTranslationUtils.ToAvaloniaMnemonics(_conflictMergetoolText.Text)}<";
            Mergetool.Focus();
            AcceptButton = Mergetool;
            MergeToolPanel.Background = new SolidColorBrush(Colors.Goldenrod);
        }
        else if (inPatch)
        {
            Resolved.Content = $">{_conflictResolvedText.Text}<";
            Resolved.Focus();
            AcceptButton = Resolved;
            ContinuePanel.Background = new SolidColorBrush(Colors.Goldenrod);
        }
        else
        {
            AcceptButton = Apply;
        }
    }

    private void BrowsePatch_Click(object? sender, EventArgs e)
        => this.InvokeAndForget(BrowsePatchAsync);

    private async Task BrowsePatchAsync()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = _selectPatchFileCaption.Text,
            FileTypeFilter =
            [
                new FilePickerFileType(_selectPatchFileFilter.Text)
                {
                    Patterns = ["*.patch"],
                },
            ],
        };
        string? currentDirectory = Path.GetDirectoryName(PatchFile.Text);
        if (!string.IsNullOrEmpty(currentDirectory))
        {
            options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(currentDirectory);
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            PatchFile.Text = path;
        }
    }

    private void Apply_Click(object? sender, EventArgs e)
    {
        string patchFile = PatchFile.Text ?? string.Empty;
        string directory = PatchDir.Text ?? string.Empty;
        bool ignoreWhitespace = IgnoreWhitespace.IsChecked == true;
        bool signOff = SignOff.IsChecked == true;

        if (string.IsNullOrEmpty(patchFile) && string.IsNullOrEmpty(directory))
        {
            MessageBoxes.Show(
                this,
                _noFileSelectedText.Text,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        using (WaitCursorScope.Enter())
        {
            Skipped.Clear();

            if (PatchFileMode.IsChecked == true)
            {
                ArgumentString arguments = IsDiffFile(patchFile)
                    ? Commands.ApplyDiffPatch(ignoreWhitespace, patchFile, Module.GetPathForGitExecution)
                    : Commands.ApplyMailboxPatch(signOff, ignoreWhitespace, patchFile, Module.GetPathForGitExecution);

                FormProcess.ShowDialog(this, UICommands, arguments, Module.WorkingDir, input: null, useDialogSettings: true);
            }
            else
            {
                ArgumentString arguments = Commands.ApplyMailboxPatch(signOff, ignoreWhitespace);
                Module.ApplyPatch(directory, arguments);
            }

            UICommands.RepoChangedNotifier.Notify();
            EnableButtons();

            if (!Module.InTheMiddleOfAction() && !Module.InTheMiddleOfPatch())
            {
                Close();
            }
        }
    }

    private static bool IsDiffFile(string path)
    {
        try
        {
            using StreamReader reader = new(path);
            string? line = reader.ReadLine();
            return line is not null && (line.StartsWith("diff ", StringComparison.Ordinal) || line.StartsWith("Index: ", StringComparison.Ordinal));
        }
        catch
        {
            return false;
        }
    }

    private void Mergetool_Click(object? sender, EventArgs e)
    {
        UICommands.StartResolveConflictsDialog(this);
        EnableButtons();
    }

    private void Skip_Click(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            PatchFile? applyingPatch = PatchGrid.PatchFiles?.FirstOrDefault(patch => patch.IsNext);
            if (applyingPatch is not null)
            {
                applyingPatch.IsSkipped = true;
                Skipped.Add(applyingPatch);
            }

            FormProcess.ShowDialog(this, UICommands, arguments: Commands.Skip(), Module.WorkingDir, input: null, useDialogSettings: true);
            EnableButtons();
        }
    }

    private void Resolved_Click(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.Resolved(), Module.WorkingDir, input: null, useDialogSettings: true);
            EnableButtons();
        }
    }

    private void Abort_Click(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.Abort(), Module.WorkingDir, input: null, useDialogSettings: true);
            Skipped.Clear();
            EnableButtons();
        }
    }

    private void AddFiles_Click(object? sender, EventArgs e)
        => UICommands.StartAddFilesDialog(this);

    private void BrowseDir_Click(object? sender, EventArgs e)
    {
        string? userSelectedPath = OsShellUtil.PickFolder(this);
        if (userSelectedPath is not null)
        {
            PatchDir.Text = userSelectedPath;
        }
    }

    private void PatchFileMode_CheckedChanged(object? sender, EventArgs e)
    {
        if (TryGetUICommands(out _))
        {
            EnableButtons();
        }
    }

    private void SolveMergeConflicts_Click(object? sender, EventArgs e)
        => Mergetool_Click(sender, e);

    private void IgnoreWhitespace_CheckedChanged(object? sender, EventArgs e)
        => AppSettings.ApplyPatchIgnoreWhitespace = IgnoreWhitespace.IsChecked == true;

    private void SignOff_CheckedChanged(object? sender, EventArgs e)
        => AppSettings.ApplyPatchSignOff = SignOff.IsChecked == true;
}
