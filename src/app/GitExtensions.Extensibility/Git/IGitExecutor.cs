using System.Text;

namespace GitExtensions.Extensibility.Git;

/// <summary>
///  Provides a minimal git execution context for lightweight repository operations such as reading the current branch.
/// </summary>
public interface IGitExecutor
{
    /// <summary>
    ///  Remembers whether this repo uses reftable.
    /// </summary>
    bool IsReftableRepo { get; set; }

    /// <summary>
    ///  Gets the directory which contains the git repository.
    /// </summary>
    string WorkingDir { get; }

    /// <summary>
    ///  Gets the default Git executable associated with this module.
    ///  This executable can be non-native (i.e. WSL).
    /// </summary>
    IExecutable GitExecutable { get; }

    /// <summary>
    ///  Gets the access to the current git executable associated with this module.
    ///  This command runner can be non-native (i.e. WSL).
    /// </summary>
    IGitCommandRunner GitCommandRunner { get; }

    /// <summary>
    ///  Gets the Windows Git executable associated with this executor.
    /// </summary>
    IExecutable GitWindowsExecutable { get; }

    /// <summary>
    ///  Gets the access to the Windows git executable associated with this executor.
    /// </summary>
    IGitCommandRunner GitWindowsCommandRunner { get; }

    /// <summary>
    ///  Name of the WSL distro for the GitExecutable, empty string for the app native Windows Git executable.
    ///  This can be seen as the Git "instance" identifier.
    /// </summary>
    string WslDistro { get; }

    /// <summary>
    ///  Gets the ".git" directory path.
    /// </summary>
    string GetGitDirectory();
}
