using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.Worktrees
{
    public interface ISWorktreeStatusProvider : IDisposable
    {
        event EventHandler<WorktreeStatusEventArgs> StatusUpdated;

        void Init();

        Task UpdateWorktreesAsync(string workingDirectory);
    }

    public sealed class WorktreeStatusProvider : ISWorktreeStatusProvider
    {
        private readonly CancellationTokenSequence _worktreesSequence = new CancellationTokenSequence();

        private WorktreeInfoResult _worktreeInfoResult;

        // Singleton accessor
        public static WorktreeStatusProvider Default { get; } = new WorktreeStatusProvider();

        // Invoked when status update is requested (use to clear/lock UI)
        public event EventHandler StatusUpdating;

        // Invoked when status update is complete
        public event EventHandler<WorktreeStatusEventArgs> StatusUpdated;

        public void Dispose()
        {
            _worktreesSequence.Dispose();
        }

        public void Init()
        {
            // Cancel any previous async activities:
            _worktreesSequence.Next();
        }

        public async Task UpdateWorktreesAsync(string workingDirectory)
        {
            _worktreeInfoResult = null;

            // Cancel previous updates
            var cancelToken = _worktreesSequence.Next();

            OnStatusUpdating();

            await TaskScheduler.Default;

            // Start gathering worktrees asynchronously.
            var currentModule = new GitModule(workingDirectory);
            _worktreeInfoResult = GetWorktrees(currentModule);

            OnStatusUpdated(_worktreeInfoResult, cancelToken);
        }

        private WorktreeInfoResult GetWorktrees(GitModule currentModule)
        {
            var lines = currentModule.GitExecutable.GetOutput("worktree list --porcelain")
                .Split('\n').GetEnumerator();

            var result = new WorktreeInfoResult();
            WorkTreeInfo worktree = null;
            while (lines.MoveNext())
            {
                var current = (string)lines.Current;
                if (string.IsNullOrWhiteSpace(current))
                {
                    continue;
                }

                const string worktreeWord = "worktree";
                var strings = current.Split(' ');
                var firstWord = strings[0];

                if (firstWord == worktreeWord)
                {
                    var normalizedPath = current.Substring(worktreeWord.Length + 1).NormalizePath() + "\\";
                    worktree = new WorkTreeInfo { Path = normalizedPath };
                    worktree.IsDeleted = !Directory.Exists(worktree.Path);
                    result.Worktrees.Add(worktree);

                    if (currentModule.WorkingDir == normalizedPath)
                    {
                        result.CurrentWorktree = worktree;
                    }
                }
                else if (worktree != null)
                {
                    switch (firstWord)
                    {
                        case "HEAD":
                            worktree.Sha1 = strings[1];
                            break;

                        case "bare":
                            worktree.Type = HeadType.Bare;
                            break;

                        case "branch":
                            worktree.Type = HeadType.Branch;
                            worktree.Branch = strings[1];
                            break;

                        case "detached":
                            worktree.Type = HeadType.Detached;
                            break;
                    }
                }
            }

            return result;
        }

        private void OnStatusUpdating()
        {
            StatusUpdating?.Invoke(this, EventArgs.Empty);
        }

        private void OnStatusUpdated(WorktreeInfoResult info, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            StatusUpdated?.Invoke(this, new WorktreeStatusEventArgs(info, token));
        }
    }
}
