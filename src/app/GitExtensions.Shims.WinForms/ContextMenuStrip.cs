namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.ContextMenuStrip</c>: a headless menu model that
///  plugins populate; the Avalonia layer materializes a real context menu from it.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Plugins/IRepositoryHostPlugin.cs</c>
///  (<c>ConfigureContextMenu</c>).
/// </remarks>
public class ContextMenuStrip : Control
{
    /// <summary>
    ///  Gets the menu items.
    /// </summary>
    public IList<ToolStripItem> Items { get; } = [];
}
