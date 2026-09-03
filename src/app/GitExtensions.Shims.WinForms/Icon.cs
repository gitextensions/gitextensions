namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Drawing.Icon</c>: an opaque icon reference.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitCommands/FileAssociatedIconProvider.cs</c>.
/// </remarks>
public sealed class Icon : IDisposable
{
    /// <summary>
    ///  Gets or sets the platform-specific icon object.
    /// </summary>
    public object? PlatformIcon { get; set; }

    /// <summary>
    ///  Returns the icon associated with a file through the installed desktop host.
    ///  A missing host means no icon is available, which callers already support.
    /// </summary>
    public static Icon? ExtractAssociatedIcon(string filePath) => ShimHost.IconExtractor?.Extract(filePath);

    public void Dispose()
    {
        if (PlatformIcon is IDisposable disposable)
        {
            disposable.Dispose();
            PlatformIcon = null;
        }
    }
}

/// <summary>
///  Resolves the desktop icon associated with a file path.
/// </summary>
/// <remarks>
///  Consumed by: <c>Icon.cs</c> and <c>GitUI.Avalonia/Compat/AssociatedFileIconExtractor.cs</c>.
/// </remarks>
public interface IIconExtractor
{
    /// <summary>
    ///  Returns the associated icon, or <see langword="null"/> when the desktop has none.
    /// </summary>
    Icon? Extract(string filePath);
}
