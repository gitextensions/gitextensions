using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GitCommands.Remotes;

namespace GitExtensions.Plugins.GitlabIntegration.Settings
{
    public partial class GitlabRemoteParser : RemoteParser
    {
        [GeneratedRegex(@"git(?:@|://)(?<host>[^/]+)[:/](?<owner>[^/]+)/(?<repo>[\w_\.\-]+)\.git")]
        private static partial Regex GitlabSshUrlRegex();

        [GeneratedRegex(@"https?://(?<host>[^/@]+)/(?<owner>.+)/(?<repo>[\w_\.\-]+)(?:.git)?")]
        private static partial Regex GitlabHttpsUrlRegex();

        private static readonly Regex[] _gitLabRegexes = [GitlabHttpsUrlRegex(), GitlabSshUrlRegex()];

        public bool IsValidRemoteUrl(string remoteUrl)
            => TryExtractGitlabDataFromRemoteUrl(remoteUrl, out _, out _, out _);

        public bool TryExtractGitlabDataFromRemoteUrl(
            string remoteUrl,
            [NotNullWhen(returnValue: true)] out string? host,
            [NotNullWhen(returnValue: true)] out string? owner,
            [NotNullWhen(returnValue: true)] out string? repository)
        {
            host = null;
            owner = null;
            repository = null;

            Match m = MatchRegExes(remoteUrl, _gitLabRegexes);

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
