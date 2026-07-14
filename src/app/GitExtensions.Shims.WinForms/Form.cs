namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.Form</c>: a placeholder window type used in
///  cross-tier signatures.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Git/IGitUICommands.cs</c>
///  (<c>ShowModelessForm</c>), <c>GitExtensions.Extensibility/MessageBoxes.cs</c>
///  (<see cref="ActiveForm"/>).
/// </remarks>
public class Form : Control
{
    /// <summary>
    ///  Gets the currently active form as provided by <see cref="ShimHost.ActiveFormProvider"/>,
    ///  or <see langword="null"/> when none is installed.
    /// </summary>
    public static Form? ActiveForm => ShimHost.ActiveFormProvider?.Invoke();
}
