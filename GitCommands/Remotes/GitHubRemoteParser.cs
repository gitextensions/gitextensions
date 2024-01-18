using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GitCommands.Remotes
{
    public sealed partial class GitHubRemoteParser : RemoteParser
    {
        [GeneratedRegex(@"git(?:@|://)github.com[:/](?<owner>[^/]+)/(?<repo>[\w_\.\-]+)\.git")]
        private static partial Regex GitHubSshUrlRegex();

        [GeneratedRegex(@"https?://(?:[^@:]+)?(?::[^/@:]+)?@?github.com/(?<owner>[^/]+)/(?<repo>[\w_\.\-]+)(?:.git)?")]
        private static partial Regex GitHubHttpsUrlRegex();
        private static readonly Regex[] GitHubRegexes = { GitHubHttpsUrlRegex(), GitHubSshUrlRegex() };

        public bool IsValidRemoteUrl(string remoteUrl)
        {
            return TryExtractGitHubDataFromRemoteUrl(remoteUrl, out _, out _);
        }

        public bool TryExtractGitHubDataFromRemoteUrl(string remoteUrl, [NotNullWhen(returnValue: true)] out string? owner, [NotNullWhen(returnValue: true)] out string? repository)
        {
            owner = null;
            repository = null;

            Match m = MatchRegExes(remoteUrl, GitHubRegexes);

            if (m is null || !m.Success)
            {
                return false;
            }

            owner = m.Groups["owner"].Value;
            repository = m.Groups["repo"].Value.Replace(".git", "");
            return true;
        }
    }
}
