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
    private readonly CancellationTokenSource _closingPluginCancellationToken = new();
    private readonly IGitModule _module;
    private readonly TaskManager _operations = ThreadHelper.CreateTaskManager();
    private readonly Dictionary<string, List<Commit>> _modulesCommits = new(1);
    private readonly int _firstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
    private bool _disposed;

    public ImpactLoader(IGitModule module)
    {
        _module = module;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _closingPluginCancellationToken.Cancel();
        Stop();
        _operations.JoinPendingOperations();
        _cancellationTokenSequence.Dispose();
        _closingPluginCancellationToken.Dispose();
    }

    public void Stop()
    {
        _cancellationTokenSequence.CancelCurrent();
    }

    public void Execute()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        CancellationToken sequenceToken = _cancellationTokenSequence.Next();
        _operations.FileAndForget(
            async () =>
            {
                using CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                    sequenceToken,
                    _closingPluginCancellationToken.Token);
                await ExecuteAsync(linkedCancellation.Token);
            });
    }

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

        using List<string>.Enumerator lineEnumerator = lines.GetEnumerator();

        // Analyze commit listing
        while (!token.IsCancellationRequested && lineEnumerator.MoveNext())
        {
            // Read line
            string line = lineEnumerator.Current;

            // Look for commit delimiters
            if (!line.StartsWith("--- ", StringComparison.Ordinal))
            {
                continue;
            }

            // Strip "--- "
            line = line[4..];

            // Split date and author
            string[] header = line.Split([" --- "], 2, StringSplitOptions.RemoveEmptyEntries);

            if (header.Length != 2)
            {
                continue;
            }

            // Save author in variable
            string author = header[1].TrimEnd('\r');

            // Parse commit date
            DateOnly date = DateOnly.Parse(header[0], CultureInfo.InvariantCulture);

            // Calculate first day of the commit week
            DateOnly week = date.AddDays(_firstDayOfWeek - (int)date.DayOfWeek);

            // Reset commit data
            int commits = 1;
            int added = 0;
            int deleted = 0;

            // Parse commit lines
            while (lineEnumerator.MoveNext()
                   && (line = lineEnumerator.Current) is not null
                   && !line.StartsWith("--- ", StringComparison.Ordinal)
                   && !token.IsCancellationRequested)
            {
                string[] fileLine = line.TrimEnd('\r').Split(Delimiters.Tab);
                if (fileLine.Length >= 2)
                {
                    if (fileLine[0] != "-")
                    {
                        added += int.Parse(fileLine[0], CultureInfo.InvariantCulture);
                    }

                    if (fileLine[1] != "-")
                    {
                        deleted += int.Parse(fileLine[1], CultureInfo.InvariantCulture);
                    }
                }
            }

            if (!token.IsCancellationRequested)
            {
                commitsBatch.Add(new Commit(week, author, new DataPoint(commits, added, deleted)));
            }
        }

        return commitsBatch;
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
