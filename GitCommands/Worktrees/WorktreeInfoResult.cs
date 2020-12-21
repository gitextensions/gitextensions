using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands.Worktrees
{
    public class WorktreeInfoResult
    {
        public IList<WorkTreeInfo> Worktrees { get; } = new List<WorkTreeInfo>();

        [NotNull]
        public WorkTreeInfo CurrentWorktree { get; internal set; }
    }
}
