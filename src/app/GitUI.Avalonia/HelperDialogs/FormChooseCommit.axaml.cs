using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.HelperDialogs;

public partial class FormChooseCommit : GitModuleForm
{
    private readonly string? _preselectCommit;
    private readonly bool _showCurrentBranchOnly;
    private readonly string? _lastRevisionToDisplayHash;

    public FormChooseCommit()
    {
        InitializeComponent();
        InitializeStaticContent();
        InitializeComplete();
    }

    public FormChooseCommit(
        IGitUICommands commands,
        string? preselectCommit,
        bool showArtificial = false,
        bool showCurrentBranchOnly = false,
        string? lastRevisionToDisplayHash = null)
        : base(commands, enablePositionRestore: false)
    {
        _preselectCommit = preselectCommit;
        _showCurrentBranchOnly = showCurrentBranchOnly;
        _lastRevisionToDisplayHash = lastRevisionToDisplayHash;

        InitializeComponent();
        InitializeStaticContent();

        revisionGrid.UICommandsSource = this;
        revisionGrid.MultiSelect = false;
        revisionGrid.ShowUncommittedChangesIfPossible = showArtificial;
        revisionGrid.SelectionChanged += revisionGrid_SelectionChanged;
        revisionGrid.DoubleClickRevision += revisionGrid_DoubleClickRevision;
        btnOK.Click += btnOK_Click;
        buttonGotoCommit.Click += buttonGotoCommit_Click;
        linkLabelParent.Click += linkLabelParent_LinkClicked;
        linkLabelParent2.Click += linkLabelParent_LinkClicked;

        AcceptButton = btnOK;
        InitializeComplete();
    }

    public GitRevision? SelectedRevision { get; private set; }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        ObjectId selectedObjectId = string.IsNullOrWhiteSpace(_preselectCommit)
            ? default
            : Module.RevParse(_preselectCommit);
        string revisionFilter = _showCurrentBranchOnly ? "HEAD" : "--all";
        if (!string.IsNullOrWhiteSpace(_lastRevisionToDisplayHash))
        {
            revisionFilter += $" ...{_lastRevisionToDisplayHash}";
        }

        revisionGrid.ReloadRevisions(Module, revisionFilter, selectedObjectId);
    }

    private void InitializeStaticContent()
    {
        flowLayoutPanelParents.IsVisible = false;
        linkLabelParent.IsVisible = false;
        linkLabelParent2.IsVisible = false;
    }

    private void btnOK_Click(object? sender, EventArgs e)
    {
        if (revisionGrid.SelectedRevision is not GitRevision revision)
        {
            return;
        }

        SelectedRevision = revision;
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }

    private void revisionGrid_DoubleClickRevision(object? sender, DoubleClickRevisionEventArgs e)
    {
        if (e.Revision is null)
        {
            return;
        }

        SelectedRevision = e.Revision;
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }

    private void buttonGotoCommit_Click(object? sender, EventArgs e)
    {
        ObjectId objectId = Module.RevParse(txtGotoCommit.Text ?? string.Empty);
        if (objectId.IsZero)
        {
            MessageBoxes.CannotFindGitRevision(this);
        }
        else if (!revisionGrid.SetSelectedRevision(objectId))
        {
            MessageBoxes.RevisionFilteredInGrid(this, objectId);
        }
    }

    private void linkLabelParent_LinkClicked(object? sender, EventArgs e)
    {
        if (sender is not HyperlinkButton { Tag: ObjectId parentId }
            || revisionGrid.SetSelectedRevision(parentId))
        {
            return;
        }

        MessageBoxes.RevisionFilteredInGrid(this, parentId);
    }

    private void revisionGrid_SelectionChanged(object? sender, EventArgs e)
    {
        SelectedRevision = revisionGrid.SelectedRevision;
        IReadOnlyList<ObjectId>? parents = SelectedRevision?.ParentIds;
        bool hasParents = parents?.Count is > 0;
        flowLayoutPanelParents.IsVisible = hasParents;
        linkLabelParent.IsVisible = hasParents;
        linkLabelParent2.IsVisible = parents?.Count is > 1;

        if (!hasParents)
        {
            return;
        }

        linkLabelParent.Tag = parents![0];
        linkLabelParent.Content = parents[0].ToShortString();
        if (parents.Count > 1)
        {
            linkLabelParent2.Tag = parents[1];
            linkLabelParent2.Content = parents[1].ToShortString();
        }
    }
}
