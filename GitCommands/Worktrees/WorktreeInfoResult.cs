using System.Collections.Generic;

namespace GitCommands.Worktrees
{
    public class WorktreeInfoResult
    {
        public WorktreeInfoResult(IList<WorkTreeInfo> worktrees, WorkTreeInfo? currentWorktree)
        {
            var trees = new List<WorkTreeInfo>();
            if (worktrees?.Count > 0)
            {
                trees.AddRange(worktrees);
            }

            Worktrees = trees;
            CurrentWorktree = currentWorktree;
        }

        public IReadOnlyList<WorkTreeInfo> Worktrees { get; }

        public WorkTreeInfo? CurrentWorktree { get; }
    }
}
