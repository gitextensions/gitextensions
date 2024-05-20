using System.Globalization;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitExtensions.Plugins.GitImpact
{
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
        /// property to enable mailmap respectfulness
        /// </summary>
        public bool RespectMailmap { get; set; }

        public event EventHandler? Exited;
        public event Action<IList<Commit>>? CommitLoaded;

        private readonly CancellationTokenSequence _cancellationTokenSequence = new();
        private readonly CancellationTokenSource _closingPluginCancellationToken = new();
        private readonly IGitModule _module;
        private readonly JoinableTask _mainModuleLoadingTask;
        private readonly Dictionary<string, List<Commit>> _modulesCommits = new(1);
        private readonly int _firstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

        public ImpactLoader(IGitModule module)
        {
            _module = module;
            _mainModuleLoadingTask = ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                    LoadModuleInfoData(_module, _closingPluginCancellationToken.Token);
                });
        }

        public void Dispose()
        {
            _closingPluginCancellationToken.Cancel();
            Stop();
            _cancellationTokenSequence.Dispose();
            _closingPluginCancellationToken.Dispose();
        }

        public void Stop()
        {
            _cancellationTokenSequence.CancelCurrent();
        }

        public void Execute()
        {
            CancellationToken token = _cancellationTokenSequence.Next();

            IReadOnlyList<JoinableTask> tasks = GetTasks(token);

            ThreadHelper.FileAndForget(async () =>
            {
                try
                {
                    await Task.WhenAll(tasks.Select(joinableTask => joinableTask.Task));
                }
                finally
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                    if (!token.IsCancellationRequested)
                    {
                        Exited?.Invoke(this, EventArgs.Empty);
                    }
                }
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

        private IReadOnlyList<JoinableTask> GetTasks(CancellationToken token)
        {
            List<JoinableTask> tasks = [ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await _mainModuleLoadingTask.JoinAsync(token);
                    LoadModuleInfo(_module, token);
                })];

            if (ShowSubmodules)
            {
                tasks.AddRange(
                    from submoduleName in _module.GetSubmodulesLocalPaths()
                    select _module.GetSubmodule(submoduleName)
                    into submodule
                    where submodule.IsValidGitWorkingDir()
                    select ThreadHelper.JoinableTaskFactory.RunAsync(
                        async () =>
                        {
                            await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                            LoadModuleInfo(submodule, token);
                        }));
            }

            return tasks;
        }

        private void LoadModuleInfo(IGitModule module, CancellationToken token)
        {
            if (_modulesCommits.TryGetValue(module.WorkingDir, out List<Commit> commitsBatch))
            {
                CommitLoaded(commitsBatch);
                return;
            }

            commitsBatch = LoadModuleInfoData(module, token);
            if (!token.IsCancellationRequested)
            {
                CommitLoaded(commitsBatch);
            }
        }

        private List<Commit> LoadModuleInfoData(IGitModule module, CancellationToken token)
        {
            string authorName = RespectMailmap ? "%aN" : "%an";
            string command = $"log --pretty=tformat:\"--- %ad --- {authorName}\" --numstat --date=short --find-copies --all --no-merges";
            ExecutionResult result = module.GitExecutable.Execute(command, cancellationToken: token);
            List<string> lines = result.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            const int linePerCommitEstimationInGitLogOutput = 6; // chosen by fair dice roll, guaranted to be random ;) ( https://xkcd.com/221/ )
            int estimatedCommitCount = lines.Count / linePerCommitEstimationInGitLogOutput;
            List<Commit> commitsBatch = new(estimatedCommitCount);

            using List<string>.Enumerator lineEnumerator = lines.GetEnumerator();

            // Analyze commit listing
            while (!token.IsCancellationRequested && lineEnumerator.MoveNext())
            {
                // Read line
                string line = lineEnumerator.Current;

                // Reached the end ?
                if (line is null)
                {
                    break;
                }

                // Look for commit delimiters
                if (!line.StartsWith("--- "))
                {
                    continue;
                }

                // Strip "--- "
                line = line[4..];

                // Split date and author
                string[] header = line.Split(new[] { " --- " }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (header.Length != 2)
                {
                    continue;
                }

                // Save author in variable
                string author = header[1];

                // Parse commit date
                DateOnly date = DateOnly.Parse(header[0]);

                // Calculate first day of the commit week
                DateOnly week = date.AddDays(_firstDayOfWeek - (int)date.DayOfWeek);

                // Reset commit data
                int commits = 1;
                int added = 0;
                int deleted = 0;

                // Parse commit lines
                while (lineEnumerator.MoveNext() && (line = lineEnumerator.Current) is not null && !line.StartsWith("--- ") && !token.IsCancellationRequested)
                {
                    // Skip empty line
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    string[] fileLine = line.Split(Delimiters.Tab);
                    if (fileLine.Length >= 2)
                    {
                        if (fileLine[0] != "-")
                        {
                            added += int.Parse(fileLine[0]);
                        }

                        if (fileLine[1] != "-")
                        {
                            deleted += int.Parse(fileLine[1]);
                        }
                    }
                }

                if (!token.IsCancellationRequested)
                {
                    commitsBatch.Add(new Commit(week, author, new DataPoint(commits, added, deleted)));
                }
            }

            _modulesCommits.Add(module.WorkingDir, commitsBatch);

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
                    if (!weekDataByAuthor.ContainsKey(author) &&
                        weekDate > start && weekDate < end)
                    {
                        weekDataByAuthor.Add(author, new DataPoint(0, 0, 0));
                    }
                }
            }
        }
    }
}
