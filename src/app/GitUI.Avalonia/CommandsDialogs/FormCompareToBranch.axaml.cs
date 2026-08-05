using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public partial class FormCompareToBranch : GitModuleForm
{
    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormCompareToBranch()
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    public FormCompareToBranch(IGitUICommands commands, ObjectId selectedCommit)
        : base(commands, enablePositionRestore: true)
    {
        // Avalonia exposes the original window-button policy through one resize boundary.
        CanResize = false;
        ShowInTaskbar = false;
        InitializeComponent();
        WireEvents();
        InitializeComplete();

        branchSelector.Initialize(remote: true, containObjectIds: null);
        branchSelector.CommitToCompare = selectedCommit;
        Activated += OnActivated;
    }

    private void OnActivated(object? sender, EventArgs eventArgs)
    {
        branchSelector.Focus();
    }

    public string? BranchName { get; private set; }

    private void btnCompare_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(branchSelector.SelectedBranchName))
        {
            BranchName = branchSelector.SelectedBranchName;
            DialogResult = DialogResult.OK;
            Close();
        }

        branchSelector.Focus();
    }

    private void WireEvents()
    {
        btnCompare.Click += btnCompare_Click;
    }

    // parity-scaffolding: Exposes the original named fields to focused tests and paired capture seeding.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormCompareToBranch form)
    {
        internal UserControls.BranchSelector BranchSelector => form.branchSelector;
        internal Button Compare => form.btnCompare;
    }
}
