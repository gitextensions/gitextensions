using System.ComponentModel;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using Microsoft;
using ResourceManager;

namespace GitUI;

public partial class BranchComboBox : GitExtensionsControl
{
    private readonly TranslationString _branchCheckoutError = new("Branch '{0}' is not selectable, this branch has been removed from the selection.");

    public BranchComboBox()
    {
        InitializeComponent();
        InitializeComplete();

        branches.DisplayMember = nameof(IGitRef.Name);
    }

    /// <summary>
    /// Occurs whenever the branch selection has changed.
    /// </summary>
    [Browsable(true)]
    [Category("Action")]
    [Description("Occurs whenever the branch selection has changed.")]
    public event EventHandler? SelectedValueChanged;

    private IReadOnlyList<IGitRef>? _branchesToSelect;
    public IReadOnlyList<IGitRef>? BranchesToSelect
    {
        get
        {
            return _branchesToSelect;
        }
        set
        {
            _branchesToSelect = value;
            LoadBranches();
        }
    }

    private void LoadBranches()
    {
        if (_branchesToSelect is not null)
        {
            branches.Items.AddRange(_branchesToSelect.ToArray());
        }
    }

    public IEnumerable<IGitRef> GetSelectedBranches()
    {
        if (_branchesToSelect is null)
        {
            yield break;
        }

        foreach (string branch in branches.Text.LazySplit(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            IGitRef? gitHead = _branchesToSelect.FirstOrDefault(g => g.Name == branch);
            if (gitHead is null)
            {
                MessageBoxes.Show(string.Format(_branchCheckoutError.Text, branch), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                yield return gitHead;
            }
        }
    }

    public string GetSelectedText() => branches.Text;

    /// <summary>
    ///  Returns the selected text, except that a purely numeric name which only matches
    ///  non-head refs (e.g. the tag "13435") is returned as the ref's <see cref="IGitRef.CompleteName"/>
    ///  (e.g. "refs/tags/13435"). Otherwise <c>GitRefName.GetFullBranchNameIfNumeric</c>
    ///  would qualify it as a local branch.
    /// </summary>
    public string GetSelectedTextQualifiedIfNumeric()
        => GetQualifiedNameIfNumericNonHead(branches.Text, _branchesToSelect);

    internal static string GetQualifiedNameIfNumericNonHead(string text, IEnumerable<IGitRef>? refs)
    {
        if (refs is null || text.Length == 0 || !text.Trim().All(char.IsDigit))
        {
            return text;
        }

        string name = text.Trim();
        IGitRef[] matches = [.. refs.Where(r => r.Name == name)];

        // A local branch of that name wins: it is what GetFullBranchNameIfNumeric qualifies to.
        return matches.Length == 0 || matches.Any(r => r.IsHead)
            ? text
            : matches[0].CompleteName;
    }

    public void SetSelectedText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        branches.Text = text;
    }

    private void selectMultipleBranchesButton_Click(object sender, EventArgs e)
    {
        Validates.NotNull(_branchesToSelect);

        using FormSelectMultipleBranches formSelectMultipleBranches = new(_branchesToSelect);
        foreach (IGitRef branch in GetSelectedBranches())
        {
            formSelectMultipleBranches.SelectBranch(branch.Name);
        }

        formSelectMultipleBranches.ShowDialog(this);
        string branchesText = string.Empty;
        foreach (GitRef branch in formSelectMultipleBranches.GetSelectedBranches().Cast<GitRef>())
        {
            if (!string.IsNullOrEmpty(branchesText))
            {
                branchesText += " ";
            }

            branchesText += branch.Name;
        }

        branches.Text = branchesText;

        OnSelectedValueChanged();
    }

    private void branches_SelectedValueChanged(object sender, EventArgs e)
        => OnSelectedValueChanged();

    private void OnSelectedValueChanged() => SelectedValueChanged?.Invoke(this, EventArgs.Empty);
}
