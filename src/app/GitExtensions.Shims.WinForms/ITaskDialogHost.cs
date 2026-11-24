namespace GitExtensions.Shims.WinForms.TaskDialogs;

/// <summary>
///  Displays custom-button task dialogs on behalf of the <see cref="TaskDialog"/> stand-in.
/// </summary>
public interface ITaskDialogHost
{
    /// <summary>
    ///  Shows a task dialog and returns the button selected by the user.
    /// </summary>
    Task<TaskDialogButton> ShowDialogAsync(TaskDialogPage page);
}
