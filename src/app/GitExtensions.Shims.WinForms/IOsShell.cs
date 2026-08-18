namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Opens files, folders, and URIs through the desktop shell.
/// </summary>
/// <remarks>
///  Consumed by the portable <c>GitCommands/Git/OsShellUtil.NonWindows.cs</c> substitute.
/// </remarks>
public interface IOsShell
{
    /// <summary>
    ///  Attempts to launch the requested target.
    /// </summary>
    /// <param name="target">A local path or absolute URI.</param>
    /// <param name="kind">The requested desktop-shell operation.</param>
    /// <returns><see langword="true"/> when the desktop accepted the request.</returns>
    bool TryLaunch(string target, OsShellLaunchKind kind);
}

/// <summary>
///  Identifies the desktop-shell operation requested by portable shared code.
/// </summary>
public enum OsShellLaunchKind
{
    /// <summary>Open a local file or directory with its default application.</summary>
    Open,

    /// <summary>Open a local file with its default application.</summary>
    OpenAs,

    /// <summary>Open a directory in the platform file manager.</summary>
    OpenDirectory,

    /// <summary>Open the directory containing a local path.</summary>
    ShowInDirectory,

    /// <summary>Open an absolute URI with its registered handler.</summary>
    OpenUri,
}
