using Avalonia.Controls;
using Avalonia.Platform;
using GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormResetChanges.cs.

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

    /// <summary>For the visual designer and the XAML loader only, like WinForms.</summary>
    public FormResetChanges()
        : this(hasExistingFiles: true, hasNewFiles: true)
    {
    }

    public FormResetChanges(bool hasExistingFiles, bool hasNewFiles, string? confirmationMessage = null)
    {
        InitializeComponent();
        InitializeComplete();

        btnCancel.Click += btnCancel_Click;
        btnReset.Click += btnReset_Click;

        if (confirmationMessage is not null)
        {
            confirmationMessage = confirmationMessage.ReplaceLineEndings(Environment.NewLine);
            txtMessage.Text = confirmationMessage;

            // Grow the dialog to fit the message, never below its designed size and never past
            // three quarters of the screen. The original computes that size itself because
            // Windows Forms cannot; here the layout measures the message and the bounds only
            // have to be stated.
            MinWidth = Width;
            MinHeight = Height;

            Screen? screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
            if (screen is not null)
            {
                MaxWidth = screen.WorkingArea.Width / screen.Scaling * 3 / 4;
                MaxHeight = screen.WorkingArea.Height / screen.Scaling * 3 / 4;
            }

            // An explicit size wins over SizeToContent, so the designed one is handed over to
            // the minimums above and released here.
            Width = double.NaN;
            Height = double.NaN;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        if (!hasExistingFiles)
        {
            // No existing files => new files only => force the "delete new files" checkbox on.
            cbDeleteNewFilesAndDirectories.IsEnabled = false;
            cbDeleteNewFilesAndDirectories.IsChecked = true;
        }
        else if (!hasNewFiles)
        {
            // No new files => force the "delete new files" checkbox off.
            cbDeleteNewFilesAndDirectories.IsEnabled = false;
            cbDeleteNewFilesAndDirectories.IsChecked = false;
        }
        else
        {
            cbDeleteNewFilesAndDirectories.IsEnabled = true; // A mix of types, so enable the checkbox.
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
        SelectedAction = cbDeleteNewFilesAndDirectories.IsChecked == true ? ActionEnum.ResetAndDelete : ActionEnum.Reset;
        Close();
    }
}
