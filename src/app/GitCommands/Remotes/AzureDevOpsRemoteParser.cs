using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GitCommands.Remotes
{
    public sealed partial class AzureDevOpsRemoteParser : RemoteParser
    {
        [GeneratedRegex(@"^https:\/\/(?<owner>[^.]*)\.visualstudio\.com\/(?<project>[^\/]*)\/_git\/(?<repo>.*)$")]
        private static partial Regex VstsHttpsRemoteRegex();

        [GeneratedRegex(@"^[^@]*@vs-ssh\.visualstudio\.com:v\d\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/(?<repo>.*)$")]
        private static partial Regex VstsSshRemoteRegex();

        [GeneratedRegex(@"^https:\/\/[^@]*@dev\.azure\.com\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/_git\/(?<repo>.*)$")]
        private static partial Regex AzureDevopsHttpsRemoteRegex();

        [GeneratedRegex(@"^git@ssh\.dev\.azure\.com:v\d\/(?<owner>[^\/]*)\/(?<project>[^\/]*)\/(?<repo>.*)$")]
        private static partial Regex AzureDevopsSshRemoteRegex();

        private static readonly Regex[] _azureDevopsRegexes = [ AzureDevopsHttpsRemoteRegex(), AzureDevopsSshRemoteRegex(), VstsHttpsRemoteRegex(), VstsSshRemoteRegex()];

        public bool IsValidRemoteUrl(string remoteUrl)
            => TryExtractAzureDevopsDataFromRemoteUrl(remoteUrl, out _, out _, out _);

        public bool TryExtractAzureDevopsDataFromRemoteUrl(string remoteUrl, [NotNullWhen(returnValue: true)] out string? owner, [NotNullWhen(returnValue: true)] out string? project, [NotNullWhen(returnValue: true)] out string? repository)
        {
            owner = null;
            project = null;
            repository = null;

            Match m = MatchRegExes(remoteUrl, _azureDevopsRegexes);

            if (m is null || !m.Success)
            {
                return false;
            }

            owner = m.Groups["owner"].Value;
            project = m.Groups["project"].Value;
            repository = m.Groups["repo"].Value;
            return true;
        }
    }
}
