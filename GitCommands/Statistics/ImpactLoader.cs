using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public class CommitEventArgs : EventArgs
        {
            public CommitEventArgs(Commit commit)
            {
                Commit = commit;
            }

            public Commit Commit { get; }
        }

        /// <summary>
        /// property to enable mailmap respectfulness
        /// </summary>
        public bool RespectMailmap { get; set; }

        public event EventHandler Exited;
        public event EventHandler<CommitEventArgs> Updated;

        public struct DataPoint
        {
            public int Commits;
            public int AddedLines;
            public int DeletedLines;

            public int ChangedLines => AddedLines + DeletedLines;

            public DataPoint(int commits, int added, int deleted)
            {
                Commits = commits;
                AddedLines = added;
                DeletedLines = deleted;
            }

            public static DataPoint operator +(DataPoint d1, DataPoint d2)
            {
                return new DataPoint
                {
                    Commits = d1.Commits + d2.Commits,
                    AddedLines = d1.AddedLines + d2.AddedLines,
                    DeletedLines = d1.DeletedLines + d2.DeletedLines
                };
            }
        }

        public struct Commit
        {
            public DateTime week;
            public string author;
            public DataPoint data;
        }

        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();
        private readonly IGitModule _module;

        public ImpactLoader(IGitModule module)
        {
            _module = module;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                _cancellationTokenSequence.Dispose();
            }
        }

        public void Stop()
        {
            _cancellationTokenSequence.CancelCurrent();
        }

        public void Execute()
        {
            var token = _cancellationTokenSequence.Next();

            JoinableTask[] tasks = GetTasks(token);

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

        private JoinableTask[] GetTasks(CancellationToken token)
        {
            List<JoinableTask> tasks = new List<JoinableTask>();
            string authorName = RespectMailmap ? "%aN" : "%an";

            string command = "log --pretty=tformat:\"--- %ad --- " + authorName + "\" --numstat --date=iso -C --all --no-merges";

            tasks.Add(ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                    LoadModuleInfo(command, _module, token);
                }));

            if (ShowSubmodules)
            {
                var submodules = _module.GetSubmodulesLocalPaths();
                foreach (var submoduleName in submodules)
                {
                    IGitModule submodule = _module.GetSubmodule(submoduleName);
                    if (submodule.IsValidGitWorkingDir())
                    {
                        tasks.Add(ThreadHelper.JoinableTaskFactory.RunAsync(
                            async () =>
                            {
                                await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                                LoadModuleInfo(command, submodule, token);
                            }));
                    }
                }
            }

            return tasks.ToArray();
        }

        private void LoadModuleInfo(string command, IGitModule module, CancellationToken token)
        {
            Process p = module.RunGitCmdDetached(command);

            // Read line
            string line = p.StandardOutput.ReadLine();

            // Analyze commit listing
            while (!token.IsCancellationRequested)
            {
                Commit commit = new Commit();

                // Reached the end ?
                if (line == null)
                {
                    break;
                }

                // Look for commit delimiters
                if (!line.StartsWith("--- "))
                {
                    line = p.StandardOutput.ReadLine();
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
                commit.author = header[1];

                // Parse commit date
                DateTime date = DateTime.Parse(header[0]).Date;

                // Calculate first day of the commit week
                commit.week = date.AddDays(-(int)date.DayOfWeek);

                // Reset commit data
                commit.data.Commits = 1;
                commit.data.AddedLines = 0;
                commit.data.DeletedLines = 0;

                // Parse commit lines
                while ((line = p.StandardOutput.ReadLine()) != null && !line.StartsWith("--- ") && !token.IsCancellationRequested)
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
                            commit.data.AddedLines += int.Parse(fileLine[0]);
                        }

                        if (fileLine[1] != "-")
                        {
                            commit.data.DeletedLines += int.Parse(fileLine[1]);
                        }
                    }
                }

                if (Updated != null && !token.IsCancellationRequested)
                {
                    Updated(this, new CommitEventArgs(commit));
                }
            }
        }

        public static void AddIntermediateEmptyWeeks(
            ref SortedDictionary<DateTime, Dictionary<string, DataPoint>> impact, Dictionary<string, DataPoint> authors)
        {
            foreach (var (author, _) in authors)
            {
                // Determine first and last commit week of each author
                DateTime start = new DateTime(), end = new DateTime();
                bool startFound = false;

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
