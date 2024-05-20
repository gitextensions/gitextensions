using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GitCommands.Remotes
{
    public partial class GitHostingRemoteParser : RemoteParser
    {
        [GeneratedRegex(@"^(ssh://)?git(?:@|://)(?<hosting>([^.]+\.)+[^.]+)[:/](?<owner>[^/]+)/(?<repo>[\w_\.\-]+)(?:.git)?")]
        private static partial Regex GitHostingSshUrlRegex();

        [GeneratedRegex(@"^https?://(?:[^@:]*?)?(?::[^/@:]*?)?@?(?<hosting>([^./]+\.)+[^./]+)/(?<owner>[^/]+)/(?<repo>[\w_\.\-]+)(?:.git)?$")]
        private static partial Regex GitHostingHttpsUrlRegex();

        private static readonly Regex[] _gitHostingRegexes = [GitHostingHttpsUrlRegex(), GitHostingSshUrlRegex()];

        /// <summary>
        /// Gets if an url is the one of a git hosted repository.
        /// </summary>
        /// <param name="remoteUrl">the url of a git repository.</param>
        /// <returns>
        /// true, if the url has been succefully parsed.
        /// false otherwise.
        /// </returns>
        public bool IsValidRemoteUrl(string remoteUrl)
            => TryExtractGitHostingDataFromRemoteUrl(remoteUrl, out _, out _, out _);

        public bool TryExtractGitHostingDataFromRemoteUrl(string remoteUrl, [NotNullWhen(returnValue: true)] out string? gitHosting, [NotNullWhen(returnValue: true)] out string? owner, [NotNullWhen(returnValue: true)] out string? repository)
        {
            owner = null;
            repository = null;
            gitHosting = null;

            Match m = MatchRegExes(remoteUrl, _gitHostingRegexes);

            if (m is null || !m.Success)
            {
                return false;
            }

            gitHosting = m.Groups["hosting"].Value;
            owner = m.Groups["owner"].Value;
            repository = m.Groups["repo"].Value.Replace(".git", "");
            return true;
        }
    }
}
