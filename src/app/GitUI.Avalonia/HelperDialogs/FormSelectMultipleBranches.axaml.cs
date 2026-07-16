using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitExtensions.Extensibility.Git;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.HelperDialogs;

public partial class FormSelectMultipleBranches : GitExtensionsForm
{
    public FormSelectMultipleBranches()
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    public FormSelectMultipleBranches(IReadOnlyList<IGitRef> branchesToSelect)
    {
        InitializeComponent();

        Branches.ItemTemplate = new FuncDataTemplate<IGitRef>(
            (branch, _) => new TextBlock { Text = branch?.Name ?? string.Empty },
            supportsRecycling: false);
        Branches.ItemsSource = branchesToSelect;
        WireEvents();

        InitializeComplete();
    }

    public void SelectBranch(string name)
    {
        IGitRef? branch = Branches.ItemsSource?
            .OfType<IGitRef>()
            .FirstOrDefault(item => item.Name == name);
        if (branch is not null && Branches.SelectedItems is { } selectedItems && !selectedItems.Contains(branch))
        {
            selectedItems.Add(branch);
        }
    }

    public IReadOnlyList<IGitRef> GetSelectedBranches()
        => Branches.SelectedItems?.Cast<IGitRef>().ToList() ?? [];

    private void WireEvents()
    {
        okButton.Click += okButton_Click;
        AcceptButton = okButton;
    }

    private void okButton_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }
}
