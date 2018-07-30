namespace GitCommands.Git.Extensions
{
    public static class GitRevisionExtensions
    {
        public static bool IsArtificial(this string sha1)
        {
            return sha1 == GitRevision.WorkTreeGuid ||
                   sha1 == GitRevision.IndexGuid ||
                   sha1 == GitRevision.CombinedDiffGuid;
        }
    }
}