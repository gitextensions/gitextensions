using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;

namespace GitUI.UserControls;

/// <summary>Small revision picker; the initial port accepts a revision name or SHA as text.</summary>
public partial class CommitPickerSmallControl : GitModuleControl
{
    /// <summary>Occurs whenever the selected commit changes.</summary>
    public event EventHandler? SelectedObjectIdChanged;

    public CommitPickerSmallControl()
    {
        InitializeComponent();
        textBoxCommitHash.LostFocus += textBoxCommitHash_TextLeave;
        InitializeComplete();
    }

    /// <summary>Gets the selected object id.</summary>
    public ObjectId SelectedObjectId { get; private set; }

    /// <summary>Resolves and selects a revision name or commit hash.</summary>
    public void SetSelectedCommitHash(string? commitHash)
    {
        ObjectId oldCommitHash = SelectedObjectId;
        SelectedObjectId = Module.RevParse(commitHash!);

        if (SelectedObjectId.IsZero && !string.IsNullOrWhiteSpace(commitHash))
        {
            SelectedObjectId = oldCommitHash;
            MessageBoxes.Show("The given commit hash is not valid for this repository and was therefore discarded.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            SelectedObjectIdChanged?.Invoke(this, EventArgs.Empty);
        }

        bool isArtificialCommitForEmptyRepo = commitHash == "HEAD";
        lbCommits.Text = string.Empty;
        textBoxCommitHash.Text = SelectedObjectId.IsZero || isArtificialCommitForEmptyRepo
            ? string.Empty
            : SelectedObjectId.ToShortString();
    }

    private void textBoxCommitHash_TextLeave(object? sender, EventArgs e)
    {
        SetSelectedCommitHash(textBoxCommitHash.Text?.Trim());
    }
}
