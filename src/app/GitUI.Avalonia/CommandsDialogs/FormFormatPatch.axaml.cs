using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public partial class FormFormatPatch : GitModuleForm
{
    private readonly TranslationString _currentBranchText = new("Current branch:");
    private readonly TranslationString _noOutputPathEnteredText =
        new("You need to enter an output path.");
    private readonly TranslationString _revisionsNeededText =
        new("You need to select at least one revision");
    private readonly TranslationString _revisionsNeededCaption =
        new("Patch error");
    private readonly TranslationString _patchResultCaption =
        new("Patch result");
    private readonly TranslationString _failCreatePatch =
        new("Unable to create patch file(s)");
    private bool _runtimeInitialized;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormFormatPatch()
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    public FormFormatPatch(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        WireEvents();
        RevisionGrid.ShowUncommittedChangesIfPossible = false;
        InitializeComplete();
        _runtimeInitialized = true;
    }

    private void Browse_Click(object? sender, EventArgs e)
    {
        string? userSelectedPath = OsShellUtil.PickFolder(this);

        if (userSelectedPath is not null)
        {
            OutputPath.Text = userSelectedPath;
        }
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        if (!_runtimeInitialized)
        {
            return;
        }

        OutputPath.Text = AppSettings.LastFormatPatchDir;
        string selectedHead = Module.GetSelectedBranch();
        SelectedBranch.Text = _currentBranchText.Text + " " + selectedHead;

        OutputPath.TextChanged += OutputPath_TextChanged;

        // Avalonia's revision grid owns an explicit portable reload boundary instead of WinForms Load().
        RevisionGrid.ReloadRevisions(Module);
    }

    private void OutputPath_TextChanged(object? sender, EventArgs e)
    {
        if (Directory.Exists(OutputPath.Text))
        {
            AppSettings.LastFormatPatchDir = OutputPath.Text;
        }
    }

    private void FormatPatch_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(OutputPath.Text))
        {
            MessageBoxes.Show(this, _noOutputPathEnteredText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string rev1;
        string rev2;
        string result = "";

        IReadOnlyList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions(SortDirection.Descending);
        if (revisions.Count == 0)
        {
            MessageBoxes.Show(this, _revisionsNeededText.Text, _revisionsNeededCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (revisions.Count == 1)
        {
            IReadOnlyList<ObjectId>? parents = revisions[0].ParentIds;
            rev1 = parents?.Count > 0 ? parents[0].ToString() : "";
            rev2 = revisions[0].Guid;
            result = Module.FormatPatch(rev1, rev2, OutputPath.Text);
        }
        else if (revisions.Count == 2)
        {
            IReadOnlyList<ObjectId>? parents = revisions[0].ParentIds;
            rev1 = parents?.Count > 0 ? parents[0].ToString() : "";
            rev2 = revisions[1].Guid;
            result = Module.FormatPatch(rev1, rev2, OutputPath.Text);
        }
        else
        {
            int n = 0;
            foreach (GitRevision revision in revisions)
            {
                n++;
                IReadOnlyList<ObjectId>? parents = revision.ParentIds;
                rev1 = parents?.Count > 0 ? parents[0].ToString() : "";
                rev2 = revision.Guid;
                result += Module.FormatPatch(rev1, rev2, OutputPath.Text, n);
            }
        }

        if (string.IsNullOrEmpty(result))
        {
            MessageBoxes.Show(this, _failCreatePatch.Text, _revisionsNeededCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            MessageBoxes.Show(this, result, _patchResultCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }

    private void WireEvents()
    {
        Browse.Click += Browse_Click;
        FormatPatch.Click += FormatPatch_Click;
    }

    // parity-scaffolding: Exposes the original named fields to focused tests and paired capture seeding.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormFormatPatch form)
    {
        internal Avalonia.Controls.TextBox OutputPath => form.OutputPath;
        internal Button Browse => form.Browse;
        internal Button FormatPatch => form.FormatPatch;
        internal RevisionGridControl RevisionGrid => form.RevisionGrid;
        internal TextBlock SelectedBranch => form.SelectedBranch;
    }
}
