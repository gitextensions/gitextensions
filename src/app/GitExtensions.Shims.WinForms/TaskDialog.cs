namespace GitExtensions.Shims.WinForms.TaskDialogs;

/// <summary>
///  Stand-in for a task-dialog button with caller-defined text.
/// </summary>
public class TaskDialogButton
{
    /// <summary>
    ///  Gets the shared cancel button returned when a cancellable dialog is closed.
    /// </summary>
    public static TaskDialogButton Cancel { get; } = new("Cancel");

    public TaskDialogButton(string? text = null)
    {
        Text = text;
    }

    /// <summary>
    ///  Gets or sets the displayed button text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    ///  Occurs after this button is selected.
    /// </summary>
    public event EventHandler? Click;

    internal void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
}

/// <summary>
///  Semantic icon displayed by a <see cref="TaskDialogPage"/>.
/// </summary>
public enum TaskDialogIcon
{
    None = 0,
    Information,
    Warning,
    Error
}

/// <summary>
///  Headless state passed from shared code to the installed task-dialog host.
/// </summary>
public sealed class TaskDialogPage
{
    /// <summary>
    ///  Gets or sets the prominent heading.
    /// </summary>
    public string? Heading { get; set; }

    /// <summary>
    ///  Gets or sets the window caption.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    ///  Gets or sets the semantic dialog icon.
    /// </summary>
    public TaskDialogIcon Icon { get; set; }

    /// <summary>
    ///  Gets or sets whether the dialog may be dismissed without selecting a supplied button.
    /// </summary>
    public bool AllowCancel { get; set; }

    /// <summary>
    ///  Gets the caller-defined buttons in display order.
    /// </summary>
    public List<TaskDialogButton> Buttons { get; } = [];
}

/// <summary>
///  Stand-in for <c>System.Windows.Forms.TaskDialog</c>: routes to the installed UI host.
/// </summary>
/// <remarks>
///  Consumed by: <c>BuildServerIntegration/AzureDevOpsIntegration/AzureDevOpsAdapter.cs</c>.
/// </remarks>
public static class TaskDialog
{
    /// <summary>
    ///  Shows a task dialog and returns the selected button.
    /// </summary>
    public static async Task<TaskDialogButton> ShowDialogAsync(TaskDialogPage page)
    {
        TaskDialogButton result = await GitExtensions.Shims.WinForms.ShimHost.TaskDialogHost.ShowDialogAsync(page).ConfigureAwait(false);
        result.PerformClick();
        return result;
    }
}
