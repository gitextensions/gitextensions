namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.LinkLabel</c>: a headless link model whose click
///  event is forwarded by the Avalonia plugin-settings adapter.
/// </summary>
/// <remarks>
///  Consumed by: <c>plugins/GitHub3/GitHub3Plugin.cs</c>.
/// </remarks>
public class LinkLabel : Control
{
    /// <summary>
    ///  Occurs when the link is activated.
    /// </summary>
    public event EventHandler? Click;

    /// <summary>
    ///  Activates the link.
    /// </summary>
    public void PerformClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }
}
