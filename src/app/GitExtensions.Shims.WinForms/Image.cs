namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Drawing.Image</c>: an opaque image reference. The Avalonia layer
///  stores its own bitmap in <see cref="PlatformImage"/>; shared code only passes the
///  reference around.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Plugins/IGitPlugin.cs</c> (plugin icon),
///  <c>GitExtensions.Extensibility/Git/IGitUICommands.cs</c> (<c>AddCommitTemplate</c>),
///  <c>GitCommands/CommitTemplateItem.cs</c>.
/// </remarks>
public class Image : IDisposable
{
    /// <summary>
    ///  Gets or sets the platform-specific image object (an Avalonia bitmap in the Avalonia app).
    /// </summary>
    public object? PlatformImage { get; set; }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && PlatformImage is IDisposable disposable)
        {
            disposable.Dispose();
            PlatformImage = null;
        }
    }
}
