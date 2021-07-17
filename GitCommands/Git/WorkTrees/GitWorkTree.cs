namespace GitCommands.Git.WorkTrees
{
    public class GitWorkTree
    {
        public string? Path { get; set; }
        public HeadType Type { get; set; }
        public string? Sha1 { get; set; }
        public string? CompleteBranchName { get; set; }
        public string? BranchName { get; set; }
        public bool IsDeleted { get; set; }
    }
}
