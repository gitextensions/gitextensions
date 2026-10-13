namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.ToolStripMenuItem</c>: a headless menu-item model.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitCommands/CustomDiffMergeTool.cs</c> (holds a menu-item reference)
///  and <c>plugins/GitHub3/GitHub3Plugin.cs</c>.
/// </remarks>
public class ToolStripMenuItem : ToolStripItem
{
    public ToolStripMenuItem()
    {
    }

    public ToolStripMenuItem(string? text, Image? image)
    {
        Text = text ?? string.Empty;
        Image = image;
    }

    /// <summary>
    ///  Gets the child menu items.
    /// </summary>
    public ToolStripItemCollection DropDownItems { get; } = [];
}
