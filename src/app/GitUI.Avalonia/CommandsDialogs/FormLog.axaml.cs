using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public partial class FormLog : GitModuleForm
{
    private readonly CancellationTokenSequence _viewChangesSequence = new();

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormLog()
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    public FormLog(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    // The original form's Designer sets Name = "FormDiff", so its only translated string
    // ($this.Text) lives under the "FormDiff" catalog category rather than the class name.
    public override void AddTranslationItems(ITranslation translation)
        => GitUI.Compat.AvaloniaTranslationUtils.AddTranslationItemsFromFields("FormDiff", this, translation);

    public override void TranslateItems(ITranslation translation)
        => GitUI.Compat.AvaloniaTranslationUtils.TranslateItemsFromFields("FormDiff", this, translation);

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _viewChangesSequence.Dispose();
        base.OnClosed(e);
    }

    // WinForms wired the grid load, revision selection, and file selection through the
    // Designer/Load events; the twin wires them once in the constructor.
    private void WireEvents()
    {
        diffViewer.ExtraDiffArgumentsChanged += DiffViewerExtraDiffArgumentsChanged;
        diffViewer.TopScrollReached += FileViewer_TopScrollReached;
        diffViewer.BottomScrollReached += FileViewer_BottomScrollReached;
        RevisionGrid.SelectionChanged += RevisionGridSelectionChanged;
        DiffFiles.SelectedIndexChanged += DiffFilesSelectedIndexChanged;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        FormDiffLoad();
    }

    private void FormDiffLoad()
    {
        // WinForms RevisionGrid.Load() refreshes the grid with its current filter.
        RevisionGrid.ReloadRevisions(Module, selectedObjectId: RevisionGrid.SelectedId);
    }

    private void DiffFilesSelectedIndexChanged(object? sender, EventArgs e)
    {
        ViewSelectedFileDiff();
    }

    private void ViewSelectedFileDiff()
    {
        using (WaitCursorScope.Enter())
        {
            // The FileStatusList twin exposes the WinForms SelectedItem (a FileStatusItem)
            // as SelectedFileStatusItem; SelectedItem returns the GitItemStatus instead.
            _ = diffViewer.ViewChangesAsync(DiffFiles.SelectedFileStatusItem,
                cancellationToken: _viewChangesSequence.Next());
        }
    }

    private void RevisionGridSelectionChanged(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            TaskManager.HandleExceptions(() => DiffFiles.SetDiffs(RevisionGrid.GetSelectedRevisions()), WinFormsShims.Application.OnThreadException);
        }
    }

    private void DiffViewerExtraDiffArgumentsChanged(object? sender, EventArgs e)
    {
        ViewSelectedFileDiff();
    }

    private void FileViewer_TopScrollReached(object? sender, EventArgs e)
    {
        DiffFiles.SelectPreviousVisibleItem();
        diffViewer.ScrollToBottom();
    }

    private void FileViewer_BottomScrollReached(object? sender, EventArgs e)
    {
        DiffFiles.SelectNextVisibleItem();
        diffViewer.ScrollToTop();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormLog form)
    {
        public RevisionGridControl RevisionGrid => form.RevisionGrid;
        public FileStatusList DiffFiles => form.DiffFiles;
        public Editor.FileViewer DiffViewer => form.diffViewer;
    }
}
