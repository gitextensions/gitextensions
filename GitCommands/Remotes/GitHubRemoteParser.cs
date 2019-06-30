namespace GitCommands.Remotes
{
    public sealed class GitHubRemoteParser : RemoteParser
    {
        private static readonly string GitHubSshUrlRegex = @"git(?:@|://)github.com[:/](?<owner>[^/]+)/(?<repo>[\w_\.\-]+)\.git";
        private static readonly string GitHubHttpsUrlRegex = @"https?://(?:[^@:]+)?(?::[^/@:]+)?@?github.com/(?<owner>[^/]+)/(?<repo>[\w_\.\-]+)(?:.git)?";
        private static readonly string[] GitHubRegexes = { GitHubHttpsUrlRegex, GitHubSshUrlRegex };

        public bool IsValidRemoteUrl(string remoteUrl)
        {
            return TryExtractGitHubDataFromRemoteUrl(remoteUrl, out _, out _);
        }

        public bool TryExtractGitHubDataFromRemoteUrl(string remoteUrl, out string owner, out string repository)
        {
            owner = null;
            repository = null;

            var m = MatchRegExes(remoteUrl, GitHubRegexes);

            if (m == null || !m.Success)
            {
                return false;
            }

            owner = m.Groups["owner"].Value;
            repository = m.Groups["repo"].Value.Replace(".git", "");
            return true;
        }
    }
}