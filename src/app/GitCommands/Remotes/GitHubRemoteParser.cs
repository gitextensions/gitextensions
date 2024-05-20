using System.Diagnostics.CodeAnalysis;

namespace GitCommands.Remotes
{
    public sealed partial class GitHubRemoteParser : GitHostingRemoteParser
    {
        public new bool IsValidRemoteUrl(string remoteUrl)
            => TryExtractGitHubDataFromRemoteUrl(remoteUrl, out _, out _);

        public bool TryExtractGitHubDataFromRemoteUrl(string remoteUrl, [NotNullWhen(returnValue: true)] out string? owner, [NotNullWhen(returnValue: true)] out string? repository)
            => TryExtractGitHostingDataFromRemoteUrl(remoteUrl, out string gitHosting, out owner, out repository) && gitHosting == "github.com";
    }
}
