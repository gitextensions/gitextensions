using Avalonia.Controls;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;
using GitUI.HelperDialogs;

namespace GitUI.UserControls;

/// <summary>Small revision picker accepting a revision name/SHA or the shared commit chooser.</summary>
public partial class CommitPickerSmallControl : GitModuleControl
{
    /// <summary>Occurs whenever the selected commit changes.</summary>
    public event EventHandler? SelectedObjectIdChanged;

    public CommitPickerSmallControl()
    {
        InitializeComponent();
        textBoxCommitHash.LostFocus += textBoxCommitHash_TextLeave;
        buttonPickCommit.Click += buttonPickCommit_Click;
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

        if (SelectedObjectId.IsZero || isArtificialCommitForEmptyRepo)
        {
            textBoxCommitHash.Text = string.Empty;
        }
        else
        {
            ObjectId selectedObjectId = SelectedObjectId;
            textBoxCommitHash.Text = selectedObjectId.ToShortString();
            ThreadHelper.FileAndForget(async () =>
            {
                ObjectId currentCheckout = Module.GetCurrentCheckout();
                if (currentCheckout.IsZero)
                {
                    return;
                }

                string toRef = selectedObjectId.IsArtificial ? "HEAD" : selectedObjectId.ToString();
                string text = Module.GetCommitCountString(currentCheckout, toRef);
                await this.SwitchToMainThreadAsync();
                if (SelectedObjectId == selectedObjectId)
                {
                    lbCommits.Text = text;
                }
            });
        }
    }

    private void textBoxCommitHash_TextLeave(object? sender, EventArgs e)
    {
        SetSelectedCommitHash(textBoxCommitHash.Text?.Trim());
    }

    private void buttonPickCommit_Click(object? sender, EventArgs e)
    {
        using FormChooseCommit chooseForm = new(
            UICommands,
            SelectedObjectId.IsZero ? null : SelectedObjectId.ToString());
        if (chooseForm.ShowDialog(TopLevel.GetTopLevel(this) as IWin32Window) == DialogResult.OK
            && chooseForm.SelectedRevision is not null)
        {
            SetSelectedCommitHash(chooseForm.SelectedRevision.ObjectId.ToString());
        }
    }
}
