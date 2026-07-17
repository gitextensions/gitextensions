using System.Text;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormFileHistory.cs, reduced to the shell: the revision
// grid filtered to one path plus the Diff and Blame tabs. Deferred: the View/Commit
// tabs, the browse menus and filter toolbar (including the blame display options),
// rename following (--follow and FilePathByObjectId), full-history/simplify-merges
// options, custom diff tools, and the build report. The history always loads on show
// (there is no menu yet to load it later).
public sealed partial class FormFileHistory : GitModuleForm, IRevisionGridFileUpdate
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

        tabControl1.SelectedItem = showBlame ? BlameTab : DiffTab;

        SetTitle();
    }

    private void WireControls()
    {
        RevisionGrid.SelectionChanged += FileChangesSelectionChanged;
        tabControl1.SelectionChanged += TabControl1SelectedIndexChanged;
        Blame.EscapePressed += Close;
    }

    private void TabControl1SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
    {
        // SelectionChanged is a routed event; the revision list and future tab content
        // must not retrigger the viewers.
        if (ReferenceEquals(e.Source, tabControl1))
        {
            FileChangesSelectionChanged(sender, e);
        }
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

        // Like WinForms: no blame tab for artificial commits.
        BlameTab.IsVisible = !revision.IsArtificial;
        if (!BlameTab.IsVisible && ReferenceEquals(tabControl1.SelectedItem, BlameTab))
        {
            tabControl1.SelectedItem = DiffTab;
        }

        if (ReferenceEquals(tabControl1.SelectedItem, BlameTab))
        {
            _ = Blame.LoadBlameAsync(revision, fileName, revisionGridInfo: RevisionGrid, revisionGridFileUpdate: this, Module.FilesEncoding, force: force, cancellationTokenSequence: _viewChangesSequence);
            return;
        }

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

    bool IRevisionGridFileUpdate.SelectFileInRevision(ObjectId commitId, RelativePath ignoredFilename)
        => RevisionGrid.SetSelectedRevision(commitId);
}
