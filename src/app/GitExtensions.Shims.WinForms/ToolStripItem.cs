namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.ToolStripItem</c>: a headless menu/toolbar item
///  model with disposal tracking.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Plugins/IRepositoryHostPlugin.cs</c>
///  (via <see cref="ContextMenuStrip"/>), <c>GitExtUtils/GitUI/ControlThreadingExtensions.cs</c>
///  (only <see cref="IsDisposed"/> and <see cref="Disposed"/>).
/// </remarks>
public class ToolStripItem : System.ComponentModel.IComponent
{
    /// <summary>
    ///  Gets or sets the item name; used as the translation key by the xlf system.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets the item text.
    /// </summary>
    public virtual string Text { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets arbitrary user data attached to the item.
    /// </summary>
    public object? Tag { get; set; }

    public System.ComponentModel.ISite? Site { get; set; }

    /// <summary>
    ///  Gets a value indicating whether <see cref="Dispose()"/> has been called.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    ///  Occurs when the item is disposed.
    /// </summary>
    public event EventHandler? Disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        if (disposing)
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }
    }
}
