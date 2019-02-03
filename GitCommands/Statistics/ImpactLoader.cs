using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.Statistics
{
    public sealed class ImpactLoader : IDisposable
    {
        public readonly struct Commit
        {
            public DateTime Week { get; }
            public string Author { get; }
            public DataPoint Data { get; }

            public Commit(DateTime week, string author, DataPoint data)
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

        public event EventHandler Exited;
        public event Action<Commit> CommitLoaded;

        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();
        private readonly IGitModule _module;

        public ImpactLoader(IGitModule module)
        {
            _module = module;
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSequence.Dispose();
        }

        public void Stop()
        {
            _cancellationTokenSequence.CancelCurrent();
        }

        public void Execute()
        {
            var token = _cancellationTokenSequence.Next();

            var tasks = GetTasks(token);

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
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
            var authorName = RespectMailmap ? "%aN" : "%an";
            var command = $"log --pretty=tformat:\"--- %ad --- {authorName}\" --numstat --date=iso -C --all --no-merges";

            var tasks = new List<JoinableTask>
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                        LoadModuleInfo(command, _module, token);
                    })
            };

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
                            LoadModuleInfo(command, submodule, token);
                        }));
            }

            return tasks;
        }

        private void LoadModuleInfo(string command, IGitModule module, CancellationToken token)
        {
            using (var lineEnumerator = module.GitExecutable.GetOutputLines(command).GetEnumerator())
            {
                // Analyze commit listing
                while (!token.IsCancellationRequested && lineEnumerator.MoveNext())
                {
                    // Read line
                    var line = lineEnumerator.Current;

                    // Reached the end ?
                    if (line == null)
                    {
                        break;
                    }

                    // Look for commit delimiters
                    if (!line.StartsWith("--- "))
                    {
                        continue;
                    }

                    // Strip "--- "
                    line = line.Substring(4);

                    // Split date and author
                    string[] header = line.Split(new[] { " --- " }, 2, StringSplitOptions.RemoveEmptyEntries);

                    if (header.Length != 2)
                    {
                        continue;
                    }

                    // Save author in variable
                    var author = header[1];

                    // Parse commit date
                    var date = DateTime.Parse(header[0]).Date;

                    // Calculate first day of the commit week
                    var week = date.AddDays(-(int)date.DayOfWeek);

                    // Reset commit data
                    var commits = 1;
                    var added = 0;
                    var deleted = 0;

                    // Parse commit lines
                    while (lineEnumerator.MoveNext() && (line = lineEnumerator.Current) != null && !line.StartsWith("--- ") && !token.IsCancellationRequested)
                    {
                        // Skip empty line
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        string[] fileLine = line.Split('\t');
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
                        CommitLoaded?.Invoke(new Commit(week, author, new DataPoint(commits, added, deleted)));
                    }
                }
            }
        }

        public static void AddIntermediateEmptyWeeks(
            ref SortedDictionary<DateTime, Dictionary<string, DataPoint>> impact,
            IEnumerable<string> authors)
        {
            foreach (var author in authors)
            {
                // Determine first and last commit week of each author
                var start = new DateTime();
                var end = new DateTime();
                var startFound = false;

                foreach (var (weekDate, weekDataByAuthor) in impact)
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
                foreach (var (weekDate, weekDataByAuthor) in impact)
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
