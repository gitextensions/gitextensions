using Avalonia.Media;
using GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
/// Resolves the desktop icon associated with a file extension.
/// </summary>
internal sealed class AssociatedFileIconExtractor : IIconExtractor
{
    private readonly IAssociatedFileIconSource _source;

    public AssociatedFileIconExtractor()
        : this(CreateSource())
    {
    }

    internal AssociatedFileIconExtractor(IAssociatedFileIconSource source)
    {
        _source = source;
    }

    public Icon? Extract(string filePath)
        => _source.Get(string.Empty, filePath) is IImage image
            ? new Icon { PlatformIcon = image }
            : null;

    private static IAssociatedFileIconSource CreateSource()
        => OperatingSystem.IsWindows()
            ? new WindowsAssociatedFileIconSource()
            : OperatingSystem.IsMacOS()
                ? new MacAssociatedFileIconSource()
                : OperatingSystem.IsLinux()
                    ? new FreedesktopAssociatedFileIconSource()
                    : NullAssociatedFileIconSource.Instance;
}

internal interface IAssociatedFileIconSource
{
    IImage? Get(string workingDirectory, string relativeFilePath);
}

internal sealed class NullAssociatedFileIconSource : IAssociatedFileIconSource
{
    public static readonly NullAssociatedFileIconSource Instance = new();

    private NullAssociatedFileIconSource()
    {
    }

    public IImage? Get(string workingDirectory, string relativeFilePath) => null;
}
