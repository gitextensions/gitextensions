using GitExtensions.Extensibility.Git;

namespace GitExtensions.Extensibility;

/// <summary>
/// Provides the ability to resolve full path.
/// </summary>
public interface IFullPathResolver
{
    /// <summary>
    /// Resolves the provided path (folder or file) against the current working directory.
    /// </summary>
    /// <param name="path">Folder or file path to resolve.</param>
    /// <returns>
    /// <paramref name="path"/> if <paramref name="path"/> is rooted; otherwise resolved path from <see cref="IGitModule.WorkingDir"/>.
    /// </returns>
    string? Resolve(string? path);
}
