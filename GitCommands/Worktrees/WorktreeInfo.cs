namespace GitCommands.Worktrees
{
    // Here are the 3 types of lines returned by the `worktree list --porcelain` that we need to handle:
    //
    // 1:
    // worktree /path/to/bare-source
    // bare
    //
    // 2:
    // /worktree /path/to/linked-worktree
    // /HEAD abcd1234abcd1234abcd1234abcd1234abcd1234
    // /branch refs/heads/master
    //
    // 3:
    // worktree /path/to/other-linked-worktree
    // HEAD 1234abc1234abc1234abc1234abc1234abc1234a
    // detached
    public class WorkTreeInfo
    {
        public string? Path { get; set; }
        public WorktreeHeadType Type { get; set; }
        public string? Sha1 { get; set; }
        public string? Branch { get; set; }
        public bool IsDeleted { get; set; }
    }
}
