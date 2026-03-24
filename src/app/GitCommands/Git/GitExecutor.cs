using System.Diagnostics;
using System.Security;
using System.Text;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitCommands;

/// <summary>
///  Provides a minimal git execution context for lightweight repository operations such as reading the current branch.
/// </summary>
internal sealed class GitExecutor : IGitExecutor
{
    private static Encoding? _systemEncoding;

    private readonly IGitDirectoryResolver _gitDirectoryResolverInstance;
    private bool _isReftableRepo;

    public GitExecutor(IGitDirectoryResolver gitDirectoryResolver, string? workingDir)
    {
        _gitDirectoryResolverInstance = gitDirectoryResolver;

        WorkingDir = (workingDir ?? "").NormalizePath().NormalizeWslPath().EnsureTrailingPathSeparator();
        GitWindowsExecutable = new Executable(() => AppSettings.GitCommand, WorkingDir);
        GitWindowsCommandRunner = new GitCommandRunner(GitWindowsExecutable, () => SystemEncoding);

        WslDistro = AppSettings.WslGitEnabled ? PathUtil.GetWslDistro(WorkingDir) : "";
        if (!string.IsNullOrEmpty(WslDistro))
        {
            // In some WSL environments the current working directory is not passed along to the git command without using the `--cd` argument. Adding it to
            // the command line is required for these environments. For those that do not need it using the argument is just redundant.
            GitExecutable = new Executable(() => AppSettings.WslCommand, WorkingDir, $"-d {WslDistro} --cd {WorkingDir.RemoveTrailingPathSeparator().Quote()} {AppSettings.WslGitCommand} ");
            GitCommandRunner = new GitCommandRunner(GitExecutable, () => SystemEncoding);
        }
        else
        {
            GitExecutable = GitWindowsExecutable;
            GitCommandRunner = GitWindowsCommandRunner;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///  Setter is needed for tests that need to replace the GitCommandRunner with a mock.
    /// </remarks>
    public IGitCommandRunner GitCommandRunner { get; private set; }

    /// <inheritdoc />
    /// <remarks>
    ///  Setter is needed for tests that need to replace the GitExecutable with a mock.
    /// </remarks>
    public IExecutable GitExecutable { get; private set; }

    /// <summary>
    ///  Gets the system encoding.
    /// </summary>
    public static Encoding SystemEncoding => _systemEncoding ??= new SystemEncodingReader().Read();

    public string WorkingDir { get; init; }

    /// <summary>
    ///  Gets the  Windows Git executable associated with this executor.
    /// </summary>
    /// <remarks>
    ///  Setter is needed for tests that need to replace the GitWindowsExecutable with a mock.
    /// </remarks>
    public IExecutable GitWindowsExecutable { get; private set; }

    public IGitCommandRunner GitWindowsCommandRunner { get; }

    public string WslDistro { get; }

    public string GetSelectedBranch(bool emptyIfDetached = false)
    {
        if (!_isReftableRepo)
        {
            string head = GetSelectedBranchFast(WorkingDir, emptyIfDetached);

            if (head == ".invalid")
            {
                _isReftableRepo = true;
            }
            else if (head.Length > 0)
            {
                _isReftableRepo = false;
                return head;
            }
        }

        GitArgumentBuilder args = new("symbolic-ref")
        {
            "--quiet",
            "HEAD"
        };

        try
        {
            ExecutionResult result = GitExecutable.Execute(args, throwOnErrorExit: false);

            if (result.ExitedSuccessfully)
            {
                return result.StandardOutput[GitRefName.RefsHeadsPrefix.Length..].TrimEnd();
            }

            return emptyIfDetached ? string.Empty : DetachedHeadParser.DetachedBranch;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            return DetachedHeadParser.UnknownBranchName;
        }
    }

    /// <summary>
    ///  Attempt to read the branch name from the HEAD file instead of calling a git command.
    /// </summary>
    /// <remarks>
    ///  Dirty but fast. This sometimes fails. In reftable repos, it always returns ".invalid".
    /// </remarks>
    private string GetSelectedBranchFast(string? repositoryPath, bool emptyIfDetached = false)
    {
        if (string.IsNullOrEmpty(repositoryPath))
        {
            return string.Empty;
        }

        string headFileContents;
        try
        {
            // eg. "/path/to/repo/.git/HEAD"
            string headFileName = Path.Combine(GetGitDirectory(repositoryPath), "HEAD");

            if (!File.Exists(headFileName))
            {
                return string.Empty;
            }

            headFileContents = File.ReadAllText(headFileName, SystemEncoding);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
        {
            // ignore inaccessible file
            return string.Empty;
        }

        // eg. "ref: refs/heads/master"
        //     "9601551c564b48208bccd50b705264e9bd68140d"

        if (!headFileContents.StartsWith("ref: "))
        {
            return emptyIfDetached ? string.Empty : DetachedHeadParser.DetachedBranch;
        }

        const string prefix = "ref: refs/heads/";

        if (!headFileContents.StartsWith(prefix))
        {
            return string.Empty;
        }

        return headFileContents[prefix.Length..].TrimEnd();
    }

    /// <summary>
    ///  Gets the ".git" directory path.
    /// </summary>
    public string GetGitDirectory()
    {
        return GetGitDirectory(WorkingDir);
    }

    /// <summary>
    ///  Gets the path to the Git directory associated with the specified repository path.
    /// </summary>
    /// <param name="repositoryPath">The file system path to the root of the repository. This path must refer to an existing Git repository.</param>
    /// <returns>
    ///  The path to the Git directory for the specified repository, or null if the path does not correspond to a
    ///  valid Git repository.
    /// </returns>
    internal string GetGitDirectory(string repositoryPath)
    {
        return _gitDirectoryResolverInstance.Resolve(repositoryPath);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(GitExecutor executor)
    {
        public IExecutable GitExecutable
        {
            get => executor.GitExecutable;
            set => executor.GitExecutable = value;
        }

        public IExecutable GitWindowsExecutable
        {
            get => executor.GitWindowsExecutable;
            set => executor.GitWindowsExecutable = value;
        }

        public IGitCommandRunner GitCommandRunner
        {
            get => executor.GitCommandRunner;
            set => executor.GitCommandRunner = value;
        }
    }
}
