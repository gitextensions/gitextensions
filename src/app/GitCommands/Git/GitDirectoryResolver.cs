using System.IO.Abstractions;

namespace GitCommands.Git;

/// <summary>
/// Provides the ability to resolve the location of .git folder.
/// </summary>
public interface IGitDirectoryResolver
{
    /// <summary>
    /// Resolves the .git folder for the given repository.
    /// </summary>
    /// <param name="repositoryPath">The repository working folder.</param>
    /// <returns>The resolved location of .git folder.</returns>
    string Resolve(string repositoryPath);
}

/// <summary>
/// Resolves the location of .git folder.
/// </summary>
public sealed class GitDirectoryResolver : IGitDirectoryResolver
{
    private readonly IFileSystem _fileSystem;

    public GitDirectoryResolver(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public GitDirectoryResolver()
        : this(new FileSystem())
    {
    }

    /// <summary>
    /// Resolves the .git folder for the given repository.
    /// </summary>
    /// <param name="repositoryPath">The repository working folder.</param>
    /// <returns>
    /// The resolved location of .git folder.
    /// <list type="table">
    ///   <item>
    ///     <term>If <paramref name="repositoryPath"/> is an empty string</term>
    ///     <description>it resolves to <see cref="string.Empty"/></description>
    ///   </item>
    ///   <item>
    ///     <term>If <paramref name="repositoryPath"/> contains a .git file (i.e. the repository is a submodule)</term>
    ///     <description>it resolves to the location of the submodule's .git folder under the superproject's .git folder with the trailing slash</description>
    ///   </item>
    ///   <item>
    ///     <term>If <paramref name="repositoryPath"/> contains .git folder</term>
    ///     <description>it resolves to the .git folder with the trailing slash</description>
    ///   </item>
    ///   <item>
    ///     <term>else</term>
    ///     <description>it returns <paramref name="repositoryPath"/> unchanged.</description>
    ///   </item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath"/> is <see langword="null"/>.</exception>
    public string Resolve(string repositoryPath)
    {
        ArgumentNullException.ThrowIfNull(repositoryPath);

        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            return string.Empty;
        }

        // Workaround for links to .git directories on WSL
        bool isWslLink = false;
        string gitPath = Path.Join(repositoryPath, ".git");
        if (_fileSystem.File.Exists(gitPath))
        {
            const string gitdir = "gitdir:";
            string? line;
            try
            {
                line = _fileSystem.File.ReadLines(gitPath).FirstOrDefault(l => l.StartsWith(gitdir));
            }
            catch (IOException) when (PathUtil.IsWslLink(gitPath))
            {
                // Assume this is a directory link as created by e.g. Google's repo tool.
                // A link to a "gitdir:" file is not expected and not supported.
                isWslLink = true;
                line = null;
            }

            if (line is not null)
            {
                // The gitdir path is written by the git executable, so it is in the format that
                // executable uses: a POSIX path for WSL git, a native Windows path otherwise.
                // Do NOT convert to native separators before classifying, because
                // GetWindowsPath() relies on the original POSIX form (e.g. "/mnt/c/..." or "/home/...").
                string rawPath = line[gitdir.Length..].Trim();

                // A fully qualified path (Windows drive path like "c:\..." or a UNC path like
                // "\\wsl$\...") is already usable as-is once separators are normalized.
                string nativePath = rawPath.ToNativePath();
                if (Path.IsPathFullyQualified(nativePath))
                {
                    return nativePath.EnsureTrailingPathSeparator();
                }

                // A rooted-but-not-fully-qualified path (e.g. "/home/user/...") is a POSIX
                // absolute path produced by WSL git. Reconstruct the Windows \\wsl$ UNC path.
                // For a non-WSL repository GetWslDistro() returns "", so GetWindowsPath() falls
                // back to ToNativePath() and this remains correct for Windows too.
                if (Path.IsPathRooted(nativePath))
                {
                    return PathUtil.GetWindowsPath(rawPath, PathUtil.GetWslDistro(repositoryPath))
                        .EnsureTrailingPathSeparator();
                }

                // Otherwise it is a relative path (e.g. "../../.git/modules/...") — resolve it
                // against the repository working folder.
                return Path.GetFullPath(Path.Join(repositoryPath, nativePath)).EnsureTrailingPathSeparator();
            }
        }

        gitPath = gitPath.EnsureTrailingPathSeparator();
        return _fileSystem.Directory.Exists(gitPath) || isWslLink ? gitPath : repositoryPath;
    }
}
