using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitUIPluginInterfaces;

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

            public Commit Commit { get; private set; }
        }

        /// <summary>
        /// property to enable mailmap respectfulness
        /// </summary>
        public bool RespectMailmap { get; set; }

        public event EventHandler Exited;
        public event EventHandler<CommitEventArgs> Updated;

        public struct DataPoint
        {
            public int Commits, AddedLines, DeletedLines;

            public int ChangedLines
            {
                get { return AddedLines + DeletedLines; }
            }

            public DataPoint(int commits, int added, int deleted)
            {
                this.Commits = commits;
                this.AddedLines = added;
                this.DeletedLines = deleted;
            }

            public static DataPoint operator +(DataPoint d1, DataPoint d2)
            {
                DataPoint temp = new DataPoint();
                temp.Commits = d1.Commits + d2.Commits;
                temp.AddedLines = d1.AddedLines + d2.AddedLines;
                temp.DeletedLines = d1.DeletedLines + d2.DeletedLines;
                return temp;
            }
        }

        public struct Commit
        {
            public DateTime week;
            public string author;
            public DataPoint data;
        }

        private CancellationTokenSource _backgroundLoaderTokenSource = new CancellationTokenSource();
        private readonly IGitModule Module;

        public ImpactLoader(IGitModule aModule)
        {
            Module = aModule;
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
                _backgroundLoaderTokenSource.Dispose();
            }
        }

        public void Stop()
        {
            _backgroundLoaderTokenSource.Cancel();
        }

        public void Execute()
        {
            _backgroundLoaderTokenSource.Cancel();
            _backgroundLoaderTokenSource = new CancellationTokenSource();
            var token = _backgroundLoaderTokenSource.Token;
            Task[] tasks = GetTasks(token);
            Task.Factory.ContinueWhenAll(tasks, (task) =>
                {
                    if (!token.IsCancellationRequested && Exited != null)
                        Exited(this, EventArgs.Empty);
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool showSubmodules;
        public bool ShowSubmodules
        {
            get { return showSubmodules; }
            set { Stop(); showSubmodules = value; }
        }

        private Task[] GetTasks(CancellationToken token)
        {
            List<Task> tasks = new List<Task>();
            string authorName = RespectMailmap ? "%aN" : "%an";

            string command = "log --pretty=tformat:\"--- %ad --- " + authorName + "\" --numstat --date=iso -C --all --no-merges";

            tasks.Add(Task.Factory.StartNew(() => LoadModuleInfo(command, Module, token), token));

            if (ShowSubmodules)
            {
                IList<string> submodules = Module.GetSubmodulesLocalPathes();
                foreach (var submoduleName in submodules)
                {
                    IGitModule submodule = Module.GetSubmodule(submoduleName);
                    if (submodule.IsValidGitWorkingDir())
                        tasks.Add(Task.Factory.StartNew(() => LoadModuleInfo(command, submodule, token), token));
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
                    break;

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
                    continue;

                // Save author in variable
                commit.author = header[1];

                // Parse commit date
                DateTime date = DateTime.Parse(header[0]).Date;
                // Calculate first day of the commit week
                date = commit.week = date.AddDays(-(int)date.DayOfWeek);

                // Reset commit data
                commit.data.Commits = 1;
                commit.data.AddedLines = 0;
                commit.data.DeletedLines = 0;

                // Parse commit lines
                while ((line = p.StandardOutput.ReadLine()) != null && !line.StartsWith("--- ") && !token.IsCancellationRequested)
                {
                    // Skip empty line
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] fileLine = line.Split('\t');
                    if (fileLine.Length >= 2)
                    {
                        if (fileLine[0] != "-")
                            commit.data.AddedLines += int.Parse(fileLine[0]);
                        if (fileLine[1] != "-")
                            commit.data.DeletedLines += int.Parse(fileLine[1]);
                    }
                }

                if (Updated != null && !token.IsCancellationRequested)
                    Updated(this, new CommitEventArgs(commit));
            }
        }

        public static void AddIntermediateEmptyWeeks(
            ref SortedDictionary<DateTime, Dictionary<string, DataPoint>> impact, Dictionary<string, DataPoint> authors)
        {
            foreach (var authorData in authors)
            {
                string author = authorData.Key;

                // Determine first and last commit week of each author
                DateTime start = new DateTime(), end = new DateTime();
                bool startFound = false;
                foreach (var week in impact)
                {
                    if (week.Value.ContainsKey(author))
                    {
                        if (!startFound)
                        {
                            start = week.Key;
                            startFound = true;
                        }
                        end = week.Key;
                    }
                }
                if (!startFound)
                    continue;

                // Add 0 commits weeks in between
                foreach (var week in impact)
                    if (!week.Value.ContainsKey(author) &&
                        week.Key > start && week.Key < end)
                        week.Value.Add(author, new DataPoint(0, 0, 0));
            }
        }
    }
}
