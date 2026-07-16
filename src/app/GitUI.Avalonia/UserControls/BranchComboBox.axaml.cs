using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using Microsoft;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

// Twin of GitUI/UserControls/BranchComboBox.cs. Avalonia's editable ComboBox replaces
// WinForms autocomplete; the adjacent dialog preserves multi-head merge selection.
public partial class BranchComboBox : GitExtensionsControl
{
    private readonly TranslationString _branchCheckoutError = new("Branch '{0}' is not selectable, this branch has been removed from the selection.");
    private IReadOnlyList<IGitRef>? _branchesToSelect;

    public BranchComboBox()
    {
        InitializeComponent();

        branches.ItemTemplate = new FuncDataTemplate<IGitRef>(
            (branch, _) => new TextBlock { Text = branch?.Name ?? string.Empty },
            supportsRecycling: false);
        branches.SelectionChanged += branches_SelectedValueChanged;
        selectMultipleBranchesButton.Click += selectMultipleBranchesButton_Click;

        InitializeComplete();
    }

    /// <summary>
    /// Occurs whenever the branch selection has changed.
    /// </summary>
    [Browsable(true)]
    [Category("Action")]
    [Description("Occurs whenever the branch selection has changed.")]
    public event EventHandler? SelectedValueChanged;

    public IReadOnlyList<IGitRef>? BranchesToSelect
    {
        get => _branchesToSelect;
        set
        {
            _branchesToSelect = value;
            branches.ItemsSource = value;
        }
    }

    public IEnumerable<IGitRef> GetSelectedBranches()
    {
        if (_branchesToSelect is null)
        {
            yield break;
        }

        foreach (string branch in GetSelectedText().LazySplit(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            IGitRef? gitHead = _branchesToSelect.FirstOrDefault(gitRef => gitRef.Name == branch);
            if (gitHead is null)
            {
                MessageBoxes.Show(
                    string.Format(_branchCheckoutError.Text, branch),
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
            else
            {
                yield return gitHead;
            }
        }
    }

    public string GetSelectedText()
    {
        string text = branches.Text ?? string.Empty;
        return !string.IsNullOrWhiteSpace(text)
            ? text
            : (branches.SelectedItem as IGitRef)?.Name ?? string.Empty;
    }

    public void SetSelectedText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        branches.SelectedItem = _branchesToSelect?.FirstOrDefault(branch => branch.Name == text);
        branches.Text = text;
        OnSelectedValueChanged();
    }

    public void Select() => branches.Focus();

    private void selectMultipleBranchesButton_Click(object? sender, EventArgs e)
    {
        if (_branchesToSelect is null)
        {
            return;
        }

        using FormSelectMultipleBranches formSelectMultipleBranches = new(_branchesToSelect);
        foreach (IGitRef branch in GetSelectedBranches())
        {
            formSelectMultipleBranches.SelectBranch(branch.Name);
        }

        WinFormsShims.IWin32Window? owner = TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;
        formSelectMultipleBranches.ShowDialog(owner);
        branches.SelectedItem = null;
        branches.Text = string.Join(' ', formSelectMultipleBranches.GetSelectedBranches().Select(branch => branch.Name));
        OnSelectedValueChanged();
    }

    private void branches_SelectedValueChanged(object? sender, EventArgs e)
        => OnSelectedValueChanged();

    private void OnSelectedValueChanged() => SelectedValueChanged?.Invoke(this, EventArgs.Empty);
}
