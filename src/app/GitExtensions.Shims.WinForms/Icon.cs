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
    ///  Returns the icon associated with a file. There is no cross-platform file-association
    ///  icon source, so this documented no-op always returns <see langword="null"/>,
    ///  which callers already treat as "no icon available".
    /// </summary>
    public static Icon? ExtractAssociatedIcon(string filePath) => null;

    public void Dispose()
    {
        if (PlatformIcon is IDisposable disposable)
        {
            disposable.Dispose();
            PlatformIcon = null;
        }
    }
}
