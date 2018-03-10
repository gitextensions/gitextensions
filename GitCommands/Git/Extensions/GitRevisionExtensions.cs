namespace GitCommands.Git.Extensions
{
    public static class GitRevisionExtensions
    {
        public static bool IsArtificial(this string sha1)
        {
            return sha1 == GitRevision.UnstagedGuid ||
                   sha1 == GitRevision.IndexGuid;
        }
    }
}