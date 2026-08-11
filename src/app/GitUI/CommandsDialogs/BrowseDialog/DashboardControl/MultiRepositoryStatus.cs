using System.Runtime.InteropServices;
using System.Text.Json;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

internal sealed record MultiRepositoryStatus
{
    public required string RepositoryPath { get; init; }
    public string Branch { get; init; } = "";
    public string? Upstream { get; init; }
    public bool IsBare { get; init; }
    public bool IsDetached { get; init; }
    public int StagedCount { get; init; }
    public int ModifiedCount { get; init; }
    public int UntrackedCount { get; init; }
    public int? Ahead { get; init; }
    public int? Behind { get; init; }
    public DateTimeOffset LastCheckedUtc { get; init; }
    public DateTimeOffset? LastFetchUtc { get; init; }
    public string? Error { get; init; }

    public bool HasWorkingTreeChanges => StagedCount != 0 || ModifiedCount != 0 || UntrackedCount != 0;
}

internal sealed record MultiRepositoryFetchResult(bool Succeeded, bool Skipped, DateTimeOffset? FetchedUtc, string? Error)
{
    public static MultiRepositoryFetchResult Success(DateTimeOffset fetchedUtc) => new(true, false, fetchedUtc, null);
    public static MultiRepositoryFetchResult Failure(string error) => new(false, false, null, error);
    public static MultiRepositoryFetchResult Busy() => new(false, true, null, "仓库正忙，已跳过本次 Fetch。");
}

internal interface IMultiRepositoryStatusProvider
{
    Task<MultiRepositoryStatus> GetStatusAsync(Repository repository, DateTimeOffset? lastFetchUtc, CancellationToken cancellationToken);
    Task<MultiRepositoryFetchResult> FetchAllRemotesAsync(Repository repository, TimeSpan timeout, CancellationToken cancellationToken);
}

internal sealed class MultiRepositoryStatusProvider(IGitExecutorProvider executorProvider) : IMultiRepositoryStatusProvider
{
    private readonly IGitExecutorProvider _executorProvider = executorProvider;

    public async Task<MultiRepositoryStatus> GetStatusAsync(Repository repository, DateTimeOffset? lastFetchUtc, CancellationToken cancellationToken)
    {
        string path = repository.Path;
        DateTimeOffset checkedUtc = DateTimeOffset.UtcNow;

        if (!Directory.Exists(path))
        {
            return ErrorStatus("仓库目录不存在。");
        }

        bool isBare = GitModule.IsBareRepository(path);
        if (!isBare && !GitModule.IsValidGitWorkingDir(path))
        {
            return ErrorStatus("该目录不是 Git 仓库。");
        }

        try
        {
            IGitExecutor executor = _executorProvider.GetExecutor(path);
            if (isBare)
            {
                return await GetBareStatusAsync(executor, path, checkedUtc, lastFetchUtc, cancellationToken);
            }

            ExecutionResult result = await executor.GitExecutable.ExecuteAsync(
                "status --porcelain=v2 --branch --untracked-files=normal",
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);

            if (!result.ExitedSuccessfully)
            {
                return ErrorStatus(GetError(result));
            }

            return ParsePorcelainV2(path, result.StandardOutput, checkedUtc, lastFetchUtc);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ErrorStatus(ex.Message);
        }

        MultiRepositoryStatus ErrorStatus(string error) => new()
        {
            RepositoryPath = path,
            LastCheckedUtc = checkedUtc,
            LastFetchUtc = lastFetchUtc,
            Error = error
        };
    }

    public async Task<MultiRepositoryFetchResult> FetchAllRemotesAsync(Repository repository, TimeSpan timeout, CancellationToken cancellationToken)
    {
        string path = repository.Path;

        if (!Directory.Exists(path) || (!GitModule.IsBareRepository(path) && !GitModule.IsValidGitWorkingDir(path)))
        {
            return MultiRepositoryFetchResult.Failure("该目录不是 Git 仓库。");
        }

        IGitExecutor executor = _executorProvider.GetExecutor(path);
        if (IsRepositoryBusy(executor))
        {
            return MultiRepositoryFetchResult.Busy();
        }

        using CancellationTokenSource timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            ExecutionResult result = await executor.GitExecutable.ExecuteAsync(
                "-c credential.interactive=false -c core.askPass= fetch --all --no-auto-gc --no-recurse-submodules",
                throwOnErrorExit: false,
                cancellationToken: timeoutCancellation.Token);

            return result.ExitedSuccessfully
                ? MultiRepositoryFetchResult.Success(DateTimeOffset.UtcNow)
                : MultiRepositoryFetchResult.Failure(GetError(result));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return MultiRepositoryFetchResult.Failure($"Fetch 在 {timeout.TotalSeconds:0} 秒后超时。");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return MultiRepositoryFetchResult.Failure(ex.Message);
        }
    }

    internal static MultiRepositoryStatus ParsePorcelainV2(string path, string output, DateTimeOffset checkedUtc, DateTimeOffset? lastFetchUtc)
    {
        string branch = "";
        string? upstream = null;
        bool detached = false;
        int? ahead = null;
        int? behind = null;
        int staged = 0;
        int modified = 0;
        int untracked = 0;

        foreach (string line in output.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("# branch.head ", StringComparison.Ordinal))
            {
                branch = line[14..];
                detached = branch == "(detached)";
                continue;
            }

            if (line.StartsWith("# branch.upstream ", StringComparison.Ordinal))
            {
                upstream = line[18..];
                continue;
            }

            if (line.StartsWith("# branch.ab ", StringComparison.Ordinal))
            {
                string[] counts = line[12..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (counts.Length == 2
                    && int.TryParse(counts[0].TrimStart('+'), out int parsedAhead)
                    && int.TryParse(counts[1].TrimStart('-'), out int parsedBehind))
                {
                    ahead = parsedAhead;
                    behind = parsedBehind;
                }

                continue;
            }

            if (line.StartsWith("? ", StringComparison.Ordinal))
            {
                untracked++;
                continue;
            }

            if ((line.StartsWith("1 ", StringComparison.Ordinal) || line.StartsWith("2 ", StringComparison.Ordinal)) && line.Length > 3)
            {
                if (line[2] != '.')
                {
                    staged++;
                }

                if (line[3] != '.')
                {
                    modified++;
                }

                continue;
            }

            if (line.StartsWith("u ", StringComparison.Ordinal))
            {
                staged++;
                modified++;
            }
        }

        return new MultiRepositoryStatus
        {
            RepositoryPath = path,
            Branch = branch,
            Upstream = upstream,
            IsDetached = detached,
            StagedCount = staged,
            ModifiedCount = modified,
            UntrackedCount = untracked,
            Ahead = ahead,
            Behind = behind,
            LastCheckedUtc = checkedUtc,
            LastFetchUtc = lastFetchUtc
        };
    }

    private static async Task<MultiRepositoryStatus> GetBareStatusAsync(
        IGitExecutor executor,
        string path,
        DateTimeOffset checkedUtc,
        DateTimeOffset? lastFetchUtc,
        CancellationToken cancellationToken)
    {
        ExecutionResult branchResult = await executor.GitExecutable.ExecuteAsync(
            "symbolic-ref --quiet --short HEAD",
            throwOnErrorExit: false,
            cancellationToken: cancellationToken);
        string branch = branchResult.ExitedSuccessfully ? branchResult.StandardOutput.Trim() : "(detached)";

        ExecutionResult upstreamResult = await executor.GitExecutable.ExecuteAsync(
            "rev-parse --abbrev-ref --symbolic-full-name @{upstream}",
            throwOnErrorExit: false,
            cancellationToken: cancellationToken);
        string? upstream = upstreamResult.ExitedSuccessfully ? upstreamResult.StandardOutput.Trim() : null;
        int? ahead = null;
        int? behind = null;

        if (!string.IsNullOrWhiteSpace(upstream))
        {
            ExecutionResult countsResult = await executor.GitExecutable.ExecuteAsync(
                "rev-list --left-right --count HEAD...@{upstream}",
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);
            string[] counts = countsResult.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (countsResult.ExitedSuccessfully
                && counts.Length == 2
                && int.TryParse(counts[0], out int parsedAhead)
                && int.TryParse(counts[1], out int parsedBehind))
            {
                ahead = parsedAhead;
                behind = parsedBehind;
            }
        }

        return new MultiRepositoryStatus
        {
            RepositoryPath = path,
            Branch = branch,
            Upstream = upstream,
            IsBare = true,
            IsDetached = branch == "(detached)",
            Ahead = ahead,
            Behind = behind,
            LastCheckedUtc = checkedUtc,
            LastFetchUtc = lastFetchUtc
        };
    }

    private static bool IsRepositoryBusy(IGitExecutor executor)
    {
        string gitDirectory;
        try
        {
            gitDirectory = executor.GetGitDirectory();
        }
        catch
        {
            return true;
        }

        string[] files = ["index.lock", "MERGE_HEAD", "CHERRY_PICK_HEAD", "REVERT_HEAD", "BISECT_LOG"];
        string[] directories = ["rebase-apply", "rebase-merge"];
        return files.Any(file => File.Exists(Path.Join(gitDirectory, file)))
            || directories.Any(directory => Directory.Exists(Path.Join(gitDirectory, directory)));
    }

    private static string GetError(ExecutionResult result)
    {
        string error = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
        error = error.Trim();
        return error.Length <= 500 ? error : error[..500] + "…";
    }
}

internal sealed class MultiRepositoryStatusCache
{
    private const string CacheFileName = "MultiRepositoryStatusCache.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static string CacheFilePath => Path.Join(AppSettings.LocalApplicationDataPath.Value!, CacheFileName);

    public IReadOnlyDictionary<string, MultiRepositoryStatus> Load()
    {
        try
        {
            if (!File.Exists(CacheFilePath))
            {
                return new Dictionary<string, MultiRepositoryStatus>(StringComparer.OrdinalIgnoreCase);
            }

            List<MultiRepositoryStatus>? statuses = JsonSerializer.Deserialize<List<MultiRepositoryStatus>>(File.ReadAllText(CacheFilePath), JsonOptions);
            return (statuses ?? []).ToDictionary(status => status.RepositoryPath, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new Dictionary<string, MultiRepositoryStatus>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Save(IEnumerable<MultiRepositoryStatus> statuses)
    {
        try
        {
            string cacheFilePath = CacheFilePath;
            string temporaryFilePath = cacheFilePath + ".tmp";
            File.WriteAllText(temporaryFilePath, JsonSerializer.Serialize(statuses, JsonOptions));
            File.Move(temporaryFilePath, cacheFilePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The status view remains usable when its optional cache cannot be persisted.
        }
    }
}

internal sealed class MultiRepositoryFetchSchedule
{
    private DateTimeOffset? _lastAttemptUtc;

    public bool ShouldFetch(DateTimeOffset now, bool enabled, TimeSpan idleTime, TimeSpan idleThreshold, TimeSpan interval)
    {
        if (!enabled || idleTime < idleThreshold)
        {
            _lastAttemptUtc = null;
            return false;
        }

        return _lastAttemptUtc is null || now - _lastAttemptUtc >= interval;
    }

    public void MarkAttempt(DateTimeOffset now)
    {
        _lastAttemptUtc = now;
    }
}

internal interface ISystemIdleTimeProvider
{
    TimeSpan GetIdleTime();
}

internal sealed class SystemIdleTimeProvider : ISystemIdleTimeProvider
{
    public TimeSpan GetIdleTime()
    {
        if (!OperatingSystem.IsWindows())
        {
            return TimeSpan.Zero;
        }

        LastInputInfo info = new() { Size = (uint)Marshal.SizeOf<LastInputInfo>() };
        return GetLastInputInfo(ref info)
            ? TimeSpan.FromMilliseconds(unchecked((uint)Environment.TickCount - info.Time))
            : TimeSpan.Zero;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetLastInputInfo(ref LastInputInfo lastInputInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }
}
