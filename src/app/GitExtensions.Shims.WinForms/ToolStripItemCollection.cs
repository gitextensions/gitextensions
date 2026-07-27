namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Headless collection used by nested <see cref="ToolStripMenuItem"/> models.
/// </summary>
/// <remarks>
///  Consumed by: <c>plugins/GitHub3/GitHub3Plugin.cs</c>.
/// </remarks>
public sealed class ToolStripItemCollection : List<ToolStripItem>
{
    /// <summary>
    ///  Adds a text-only menu item and returns it, matching the WinForms collection API.
    /// </summary>
    public ToolStripItem Add(string? text)
    {
        ToolStripMenuItem item = new(text, image: null);
        Add(item);
        return item;
    }
}
