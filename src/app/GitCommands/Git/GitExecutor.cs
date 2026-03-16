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
    private readonly IGitDirectoryResolver _gitDirectoryResolverInstance;
    private static Encoding? _systemEncoding;

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

    public GitExecutor(string? workingDir)
        : this(new GitDirectoryResolver(), workingDir)
    {
    }

    public IGitCommandRunner GitCommandRunner { get; private set; }

    public IExecutable GitExecutable { get; private set; }

    /// <summary>
    ///  Gets the system encoding.
    /// </summary>
    public static Encoding SystemEncoding => _systemEncoding ??= new SystemEncodingReader().Read();

    public string WorkingDir { get; init; }

    /// <summary>
    ///  Gets the  Windows Git executable associated with this executor.
    /// </summary>
    public IExecutable GitWindowsExecutable { get; private set; }

    /// <summary>
    ///  Gets the access to the Windows git executable associated with this executor.
    /// </summary>
    internal IGitCommandRunner GitWindowsCommandRunner { get; }

    /// <summary>
    ///  Name of the WSL distro for the GitExecutable, empty string for the app native Windows Git executable.
    ///  This can be seen as the Git "instance" identifier.
    /// </summary>
    internal string WslDistro { get; }

    /// <summary>
    ///  Gets a value indicating whether this repository is using the reftable format.
    /// </summary>
    internal bool IsReftableRepo { get; set; }

    public string GetSelectedBranch(bool emptyIfDetached = false)
    {
        if (!IsReftableRepo)
        {
            string head = GetSelectedBranchFast(WorkingDir, emptyIfDetached);

            if (head == ".invalid")
            {
                IsReftableRepo = true;
            }
            else if (head.Length > 0)
            {
                IsReftableRepo = false;
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
            return "???";
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

    internal readonly struct TestAccessor
    {
        private readonly GitExecutor _executor;

        public TestAccessor(GitExecutor executor)
        {
            _executor = executor;
        }

        public IExecutable GitExecutable
        {
            get => _executor.GitExecutable;
            set => _executor.GitExecutable = value;
        }

        public IExecutable GitWindowsExecutable
        {
            get => _executor.GitWindowsExecutable;
            set => _executor.GitWindowsExecutable = value;
        }

        public IGitCommandRunner GitCommandRunner
        {
            get => _executor.GitCommandRunner;
            set => _executor.GitCommandRunner = value;
        }
    }
}
