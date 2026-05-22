namespace GitUI.CommandsDialogs;

/// <summary>
/// Shows a form asking if the user wants to reset their changes.
/// </summary>
public partial class FormResetChanges : GitExtensionsForm
{
    // CANCEL must be placed at first position because it is the default value when
    // closing the dialog via the X button
    public enum ActionEnum
    {
        Cancel,
        Reset,
        ResetAndDelete
    }

    public ActionEnum SelectedAction { get; private set; }

    public FormResetChanges(bool hasExistingFiles, bool hasNewFiles, string? confirmationMessage = null)
    {
        InitializeComponent();
        InitializeComplete();

        if (confirmationMessage is not null)
        {
            txtMessage.AutoSize = false;
            txtMessage.ScrollBars = ScrollBars.None;
            txtMessage.WordWrap = false;

            confirmationMessage = confirmationMessage.ReplaceLineEndings(Environment.NewLine);
            txtMessage.Text = confirmationMessage;

            Size formSize = Size;
            Size preferredSize = formSize + txtMessage.PreferredSize - txtMessage.Size;
            formSize.Height = Math.Clamp(preferredSize.Height, formSize.Height, Screen.GetWorkingArea(this).Height * 3 / 4);
            formSize.Width = Math.Clamp(preferredSize.Width + SystemInformation.VerticalScrollBarWidth, formSize.Width, Screen.GetWorkingArea(this).Width * 3 / 4);
            Size = formSize;
            txtMessage.ScrollBars = confirmationMessage.Contains(Environment.NewLine) ? ScrollBars.Both : ScrollBars.None;
            txtMessage.WordWrap = true;
        }

        if (!hasExistingFiles)
        {
            // No existing files => new files only => force the "delete new files" checkbox on.
            cbDeleteNewFilesAndDirectories.Enabled = false;
            cbDeleteNewFilesAndDirectories.Checked = true;
        }
        else if (!hasNewFiles)
        {
            // No new files => force the "delete new files" checkbox off.
            cbDeleteNewFilesAndDirectories.Enabled = false;
            cbDeleteNewFilesAndDirectories.Checked = false;
        }
        else
        {
            cbDeleteNewFilesAndDirectories.Enabled = true; // A mix of types, so enable the checkbox.
        }
    }

    /// <summary>
    /// Shows the dialog modally under the given owner, and returns the user's selection (RESET, RESET_AND_DELETE, or CANCEL).
    /// </summary>
    /// <param name="owner">Shows this form as a modal dialog with the specified owner.</param>
    /// <param name="hasExistingFiles">Where there are existing (modified) files selected.</param>
    /// <param name="hasNewFiles">Where there are new (untracked) files selected.</param>
    /// <param name="confirmationMessage">Optional confirmation message replacing the default.</param>
    public static ActionEnum ShowResetDialog(IWin32Window? owner, bool hasExistingFiles, bool hasNewFiles, string? confirmationMessage = null)
    {
        using FormResetChanges form = new(hasExistingFiles, hasNewFiles, confirmationMessage);
        form.ShowDialog(owner);
        return form.SelectedAction;
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        SelectedAction = ActionEnum.Cancel;
        Close();
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
        SelectedAction = cbDeleteNewFilesAndDirectories.Checked ? ActionEnum.ResetAndDelete : ActionEnum.Reset;
        Close();
    }
}
