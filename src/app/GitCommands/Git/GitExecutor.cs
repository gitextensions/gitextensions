using System.Security;
using System.Text;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitCommands;

/// <summary>
/// Provides a minimal git execution context for lightweight repository operations such as reading the current branch.
/// </summary>
public class GitExecutor : IGitExecutor
{
    private static readonly IGitDirectoryResolver GitDirectoryResolverInstance = new GitDirectoryResolver();
    private static Encoding? _systemEncoding;

    protected readonly IGitCommandRunner _gitCommandRunner;
    protected readonly IExecutable _gitExecutable;
    protected readonly IExecutable _gitWindowsExecutable;

    protected bool _isReftableRepo;

    /// <summary>
    /// Name of the WSL distro for the GitExecutable, empty string for the app native Windows Git executable.
    /// This can be seen as the Git "instance" identifier.
    /// </summary>
    protected readonly string WslDistro;

    protected IExecutable GitWindowsExecutable => _gitWindowsExecutable;
    protected IGitCommandRunner GitWindowsCommandRunner { get; }

    public static Encoding SystemEncoding => _systemEncoding ??= new SystemEncodingReader().Read();

    public string WorkingDir { get; init; }
    public IExecutable GitExecutable => _gitExecutable;
    public IGitCommandRunner GitCommandRunner => _gitCommandRunner;

    public GitExecutor(string? workingDir)
    {
        WorkingDir = (workingDir ?? "").NormalizePath().NormalizeWslPath().EnsureTrailingPathSeparator();
        _gitWindowsExecutable = new Executable(() => AppSettings.GitCommand, WorkingDir);
        GitWindowsCommandRunner = new GitCommandRunner(GitWindowsExecutable, () => SystemEncoding);

        WslDistro = AppSettings.WslGitEnabled ? PathUtil.GetWslDistro(WorkingDir) : "";
        if (!string.IsNullOrEmpty(WslDistro))
        {
            // In some WSL environments the current working directory is not passed along to the git command without using the `--cd` argument. Adding it to
            // the command line is required for these environments. For those that do not need it using the argument is just redundant.
            _gitExecutable = new Executable(() => AppSettings.WslCommand, WorkingDir, $"-d {WslDistro} --cd {WorkingDir.RemoveTrailingPathSeparator().Quote()} {AppSettings.WslGitCommand} ");
            _gitCommandRunner = new GitCommandRunner(_gitExecutable, () => SystemEncoding);
        }
        else
        {
            _gitExecutable = GitWindowsExecutable;
            _gitCommandRunner = GitWindowsCommandRunner;
        }
    }

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
                return head;
            }
        }

        GitArgumentBuilder args = new("symbolic-ref")
        {
            "--quiet",
            "HEAD"
        };
        ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: false);

        if (result.ExitedSuccessfully)
        {
            return result.StandardOutput[GitRefName.RefsHeadsPrefix.Length..].TrimEnd();
        }

        return emptyIfDetached ? string.Empty : DetachedHeadParser.DetachedBranch;
    }

    /// <summary>Attempt to read the branch name from the HEAD file instead of calling a git command.</summary>
    /// <remarks>Dirty but fast. This sometimes fails. In reftable repos, it always returns ".invalid".</remarks>
    private static string GetSelectedBranchFast(string? repositoryPath, bool emptyIfDetached = false)
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

    /// <summary>Gets the ".git" directory path.</summary>
    protected string GetGitDirectory()
    {
        return GetGitDirectory(WorkingDir);
    }

    public static string GetGitDirectory(string repositoryPath)
    {
        return GitDirectoryResolverInstance.Resolve(repositoryPath);
    }
}
