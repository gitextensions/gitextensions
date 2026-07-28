using System.Globalization;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;

namespace GitExtensions.Plugins.GitImpact;

public sealed class ImpactLoader : IDisposable
{
    public readonly struct Commit
    {
        public DateOnly Week { get; }
        public string Author { get; }
        public DataPoint Data { get; }

        public Commit(DateOnly week, string author, DataPoint data)
        {
            Week = week;
            Author = author;
            Data = data;
        }
    }

    public readonly struct DataPoint
    {
        public int Commits { get; }
        public int AddedLines { get; }
        public int DeletedLines { get; }

        public int ChangedLines => AddedLines + DeletedLines;

        public DataPoint(int commits, int added, int deleted)
        {
            Commits = commits;
            AddedLines = added;
            DeletedLines = deleted;
        }

        public static DataPoint operator +(DataPoint left, DataPoint right)
        {
            return new DataPoint(
                left.Commits + right.Commits,
                left.AddedLines + right.AddedLines,
                left.DeletedLines + right.DeletedLines);
        }
    }

    /// <summary>
    /// Property to enable mailmap respectfulness.
    /// </summary>
    public bool RespectMailmap { get; set; }

    public event EventHandler? Exited;
    public event Action<IList<Commit>>? CommitLoaded;

    private readonly Lock _cacheLock = new();
    private readonly CancellationTokenSequence _cancellationTokenSequence = new();
    private readonly Lock _lifetimeLock = new();
    private readonly IGitModule _module;
    private readonly TaskManager _operations = ThreadHelper.CreateTaskManager();
    private readonly Dictionary<string, List<Commit>> _modulesCommits = new(1);
    private readonly int _firstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
    public ImpactLoader(IGitModule module)
    {
        _module = module;
    }

    public void Dispose()
    {
        lock (_lifetimeLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cancellationTokenSequence.CancelCurrent();
        }

        _operations.JoinPendingOperations();
        _cancellationTokenSequence.Dispose();
    }

    public void Stop()
    {
        lock (_lifetimeLock)
        {
            if (!_disposed)
            {
                _cancellationTokenSequence.CancelCurrent();
            }
        }
    }

    public void Execute()
    {
        lock (_lifetimeLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            CancellationToken sequenceToken = _cancellationTokenSequence.Next();
            _operations.FileAndForget(
                async () => await ExecuteAsync(sequenceToken));
        }
    }

    private bool _disposed;
    private bool _showSubmodules;

    public bool ShowSubmodules
    {
        get => _showSubmodules;
        set
        {
            Stop();
            _showSubmodules = value;
        }
    }

    private async Task ExecuteAsync(CancellationToken token)
    {
        IReadOnlyList<IGitModule> modules = GetModules(token);
        try
        {
            await Task.WhenAll(modules.Select(LoadAndPublishAsync));
        }
        finally
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            if (!token.IsCancellationRequested)
            {
                Exited?.Invoke(this, EventArgs.Empty);
            }
        }

        return;

        async Task LoadAndPublishAsync(IGitModule module)
        {
            List<Commit> commitsBatch = await LoadModuleInfoAsync(module, token);
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            token.ThrowIfCancellationRequested();
            CommitLoaded?.Invoke(commitsBatch);
        }
    }

    private IReadOnlyList<IGitModule> GetModules(CancellationToken token)
    {
        List<IGitModule> modules = [_module];
        if (!ShowSubmodules)
        {
            return modules;
        }

        foreach (string submoduleName in _module.GetSubmodulesLocalPaths())
        {
            token.ThrowIfCancellationRequested();
            IGitModule submodule = _module.GetSubmodule(submoduleName);
            if (submodule.IsValidGitWorkingDir())
            {
                modules.Add(submodule);
            }
        }

        return modules;
    }

    private async Task<List<Commit>> LoadModuleInfoAsync(IGitModule module, CancellationToken token)
    {
        lock (_cacheLock)
        {
            if (_modulesCommits.TryGetValue(module.WorkingDir, out List<Commit>? cached))
            {
                return cached;
            }
        }

        List<Commit> loaded = await LoadModuleInfoDataAsync(module, token);
        token.ThrowIfCancellationRequested();

        lock (_cacheLock)
        {
            if (_modulesCommits.TryGetValue(module.WorkingDir, out List<Commit>? cached))
            {
                return cached;
            }

            _modulesCommits.Add(module.WorkingDir, loaded);
            return loaded;
        }
    }

    private async Task<List<Commit>> LoadModuleInfoDataAsync(IGitModule module, CancellationToken token)
    {
        string authorName = RespectMailmap ? "%aN" : "%an";
        string format = $"--- %ad --- {authorName}";
        GitArgumentBuilder arguments = new("log")
        {
            $"--pretty=tformat:{format.Quote()}",
            "--numstat",
            "--date=short",
            "--find-copies",
            "--all",
            "--no-merges",
        };
        ExecutionResult result = await module.GitExecutable.ExecuteAsync(
            arguments,
            cancellationToken: token);
        List<string> lines = [.. result.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries)];

        const int linePerCommitEstimationInGitLogOutput = 6;
        int estimatedCommitCount = lines.Count / linePerCommitEstimationInGitLogOutput;
        List<Commit> commitsBatch = new(estimatedCommitCount);

        for (int lineIndex = 0; lineIndex < lines.Count && !token.IsCancellationRequested; lineIndex++)
        {
            if (!TryParseCommitHeader(lines[lineIndex], out string author, out DateOnly date))
            {
                continue;
            }

            DateOnly week = date.AddDays(_firstDayOfWeek - (int)date.DayOfWeek);
            int added = 0;
            int deleted = 0;
            while (lineIndex + 1 < lines.Count
                   && !lines[lineIndex + 1].StartsWith("--- ", StringComparison.Ordinal)
                   && !token.IsCancellationRequested)
            {
                lineIndex++;
                AccumulateFileStats(lines[lineIndex], ref added, ref deleted);
            }

            if (!token.IsCancellationRequested)
            {
                commitsBatch.Add(new Commit(week, author, new DataPoint(commits: 1, added, deleted)));
            }
        }

        return commitsBatch;
    }

    private static bool TryParseCommitHeader(string line, out string author, out DateOnly date)
    {
        author = string.Empty;
        date = default;
        if (!line.StartsWith("--- ", StringComparison.Ordinal))
        {
            return false;
        }

        string[] header = line[4..].Split([" --- "], 2, StringSplitOptions.RemoveEmptyEntries);
        if (header.Length != 2
            || !DateOnly.TryParse(header[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return false;
        }

        author = header[1].TrimEnd('\r');
        return true;
    }

    private static void AccumulateFileStats(string line, ref int added, ref int deleted)
    {
        string[] fileLine = line.TrimEnd('\r').Split(Delimiters.Tab);
        if (fileLine.Length < 2)
        {
            return;
        }

        if (fileLine[0] != "-")
        {
            added += int.Parse(fileLine[0], CultureInfo.InvariantCulture);
        }

        if (fileLine[1] != "-")
        {
            deleted += int.Parse(fileLine[1], CultureInfo.InvariantCulture);
        }
    }

    public static void AddIntermediateEmptyWeeks(
        ref SortedDictionary<DateOnly, Dictionary<string, DataPoint>> impact,
        IEnumerable<string> authors)
    {
        foreach (string author in authors)
        {
            // Determine first and last commit week of each author
            DateOnly start = DateOnly.MinValue;
            DateOnly end = DateOnly.MinValue;
            bool startFound = false;

            foreach ((DateOnly weekDate, Dictionary<string, DataPoint> weekDataByAuthor) in impact)
            {
                if (weekDataByAuthor.ContainsKey(author))
                {
                    if (!startFound)
                    {
                        start = weekDate;
                        startFound = true;
                    }

                    end = weekDate;
                }
            }

            if (!startFound)
            {
                continue;
            }

            // Add 0 commits weeks in between
            foreach ((DateOnly weekDate, Dictionary<string, DataPoint> weekDataByAuthor) in impact)
            {
                if (!weekDataByAuthor.ContainsKey(author)
                    && weekDate > start
                    && weekDate < end)
                {
                    weekDataByAuthor.Add(author, new DataPoint(0, 0, 0));
                }
            }
        }
    }
}
