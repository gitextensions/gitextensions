using System.Text;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormFileHistory.cs, reduced to the shell: the revision
// grid filtered to one path plus the Diff tab. Deferred: the View/Blame/Commit tabs
// (blame is subphase 3.9), the browse menus and filter toolbar, rename following
// (--follow and FilePathByObjectId), full-history/simplify-merges options, custom diff
// tools, and the build report. The history always loads on show (there is no menu yet
// to load it later).
public sealed partial class FormFileHistory : GitModuleForm
{
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private readonly ObjectId _initialSelectedId = default;

    private string FileName { get; init; } = string.Empty;

    public FormFileHistory()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormFileHistory(IGitUICommands commands, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        WireControls();
        RevisionGrid.DisableContextMenu();

        InitializeComplete();

        _initialSelectedId = revision?.ObjectId ?? default;

        // Replace Windows path separator to Linux path separator.
        // This is needed to keep the file history working when started from file tree in
        // browse dialog.
        FileName = fileName.RemoveQuotes().ToPosixPath();

        SetTitle();
    }

    private void WireControls()
    {
        RevisionGrid.SelectionChanged += FileChangesSelectionChanged;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        LoadFileHistory();
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewChangesSequence.Dispose();
        base.OnClosed(e);
    }

    private void LoadFileHistory()
    {
        if (string.IsNullOrEmpty(FileName))
        {
            return;
        }

        RevisionGrid.ReloadRevisions(Module, revisionFilter: "", _initialSelectedId, pathFilter: FileName.Quote());
    }

    private void FileChangesSelectionChanged(object? sender, EventArgs e)
    {
        UpdateSelectedFileViewers();
    }

    private void SetTitle(string? alternativeFileName = null)
    {
        StringBuilder str = new StringBuilder()
            .Append("File History - ")
            .Append(FileName);

        if (!string.IsNullOrEmpty(alternativeFileName) && alternativeFileName != FileName)
        {
            str.Append(" (").Append(alternativeFileName).Append(')');
        }

        str.Append(" - ").Append(PathUtil.GetDisplayPath(Module.WorkingDir));

        Text = str.ToString();
    }

    private void UpdateSelectedFileViewers(bool force = false)
    {
        if (RevisionGrid.SelectedRevision is not GitRevision revision)
        {
            return;
        }

        // Rename following waits for FilePathByObjectId; the original name is used.
        string fileName = FileName;

        SetTitle(alternativeFileName: fileName);

        GitItemStatus file = new(name: fileName)
        {
            IsTracked = true,
        };
        FileStatusItem item = new(
            firstRev: null,
            secondRev: revision,
            file);
        _ = Diff.ViewChangesAsync(item, _viewChangesSequence.Next());
    }
}
