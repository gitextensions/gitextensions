using System.Diagnostics.CodeAnalysis;
using GitCommands.Remotes;

namespace GitExtensions.Plugins.GitlabIntegration.Settings
{
    public class GitlabRemoteParser : RemoteParser
    {
        private const string _gitlabSshUrlRegex = @"git(?:@|://)(?<host>[^/]+)[:/](?<owner>[^/]+)/(?<repo>[\w_\.\-]+)\.git";
        private const string _gitlabHttpsUrlRegex = @"https?://(?<host>[^@]+)/(?<owner>[^/]+)/(?<repo>[\w_\.\-]+)(?:.git)?";
        private static readonly string[] GitHubRegexes = { _gitlabHttpsUrlRegex, _gitlabSshUrlRegex };

        public bool IsValidRemoteUrl(string remoteUrl)
        {
            return TryExtractGitlabDataFromRemoteUrl(remoteUrl, out _, out _, out _);
        }

        public bool TryExtractGitlabDataFromRemoteUrl(
            string remoteUrl,
            [NotNullWhen(returnValue: true)] out string? host,
            [NotNullWhen(returnValue: true)] out string? owner,
            [NotNullWhen(returnValue: true)] out string? repository)
        {
            host = null;
            owner = null;
            repository = null;

            var m = MatchRegExes(remoteUrl, GitHubRegexes);

            if (m is null || !m.Success)
            {
                return false;
            }

            host = m.Groups["host"].Value;
            owner = m.Groups["owner"].Value;
            repository = m.Groups["repo"].Value.Replace(".git", "");
            return true;
        }
    }
}
