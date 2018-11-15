using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VstsAndTfsIntegration.Settings
{
    public class VstsProjectUrlHelper
    {
        private static readonly Dictionary<Regex, Func<Match, string>> RemoteToProjectUrlLookup = new Dictionary<Regex, Func<Match, string>>()
        {
            { // VS Team Services via HTTPS
                new Regex(@"^(?<prot>.+)://(?<user>[^.@]+)(?:@[^.]*)?\.visualstudio\.com(?<port>:\d*)?(?:/DefaultCollection)?(?<project>(/[^/]+)?/[^/]+)/_(git|ssh)/(.+)$"),
                (match) => $"{match.Groups["prot"].Value}://{match.Groups["user"].Value}.visualstudio.com{match.Groups["port"].Value}{match.Groups["project"].Value}"
            },
            { // VS Team Services via SSH
                new Regex(@"^(?<user>[^.@]+)@vs-ssh\.visualstudio.com:v3(?:/[^/]*)?(?<project>/[^/]+)"),
                (match) => $"https://{match.Groups["user"].Value}.visualstudio.com{match.Groups["project"].Value}"
            },
            { // Azure DevOps via HTTPS
                new Regex(@"^(?<prot>.+)://(?:[^.@]+@)?dev\.azure\.com(?<port>:\d*)?(?<project>(?:/[^/]+)?/[^/]+)/_(?:git|ssh)/(?:.+)$"),
                (match) => $"{match.Groups["prot"].Value}://dev.azure.com{match.Groups["port"].Value}{match.Groups["project"].Value}"
            },
            { // Azure DevOps via SSH
                new Regex(@"^[^.@]+@ssh\.dev\.azure\.com:v3(?<project>(?:/[^/]+)?/[^/]+)"),
                (match) => $"https://dev.azure.com{match.Groups["project"].Value}"
            },
            { // Secondary Project-Repo in TFS on premise with DefaultCollection (need at least something to detect)
                new Regex(@"^(?<instanceurl>.+://[^/]+(?::\d*)?(?:/[^/]+)+/DefaultCollection)(?<project>/[^/]+)/_(?:git|ssh)"),
                (match) => $"{match.Groups["instanceurl"].Value}{match.Groups["project"].Value}"
            },
            { // Main Project-Repo in TFS on premise with DefaultCollection (need at least something to detect)
                new Regex(@"^(?<instanceurl>.+://[^/]+(?::\d*)?(?:/[^/]+)+/DefaultCollection)/_(?:git|ssh)(?<project>/[^/]+)"),
                (match) => $"{match.Groups["instanceurl"].Value}{match.Groups["project"].Value}"
            },
        };

        private static readonly Dictionary<Regex, Func<Match, string>> ProjectToTokenManagementUrlLookup = new Dictionary<Regex, Func<Match, string>>()
        {
            { // VS Team Services
                new Regex(@"^(?<instanceurl>.+://[^.@]+(?:@[^.]*)?\.visualstudio\.com(?::\d*)?)"),
                (match) => $"{match.Groups["instanceurl"].Value}/_details/security/tokens"
            },
            { // Azure DevOps
                new Regex(@"^(?<instanceurl>.+://dev\.azure\.com(?::\d*)?/[^/]+)"),
                (match) => $"{match.Groups["instanceurl"].Value}/_details/security/tokens"
            },
            { // Generic TFS on premise instance
                new Regex(@"^(?<instanceurl>.+://[^/]+(?::\d*)?(?:/[^/]+)+)/[^/]+"),
                (match) => $"{match.Groups["instanceurl"].Value}/_details/security/tokens"
            },
        };

        private static readonly Regex BuildUrlInfoRegex = new Regex(@"^(?<projecturl>.+://[^/]+(?::\d*)?(?:/[^/]+)+)/_build.*(?:&|\?)buildId=(?<buildid>\d+)");

        private static (bool success, string result) TryConvert(string value, Dictionary<Regex, Func<Match, string>> lookupDictionary)
        {
            foreach (var kv in lookupDictionary)
            {
                var regex = kv.Key;
                var converter = kv.Value;

                var match = regex.Match(value);
                if (match.Success)
                {
                    return (true, converter(match));
                }
            }

            return (false, "");
        }

        public static (bool success, string projectUrl) TryConvertRemoteToProjectUrl(string remoteUrl)
        {
            return TryConvert(remoteUrl, RemoteToProjectUrlLookup);
        }

        public static (bool success, string projectUrl) TryDetectProjectUrlFromRemotesList(IEnumerable<string> remoteUrls)
        {
            return remoteUrls.Select(TryConvertRemoteToProjectUrl).FirstOrDefault(r => r.success);
        }

        public static bool TryDetectProjectUrlFromRemotesList(IEnumerable<string> remoteUrls, out string projectUrl)
        {
            var result = TryDetectProjectUrlFromRemotesList(remoteUrls);
            projectUrl = result.projectUrl;
            return result.success;
        }

        public static (bool success, string tokenManagementUrl) TryConvertProjectToTokenManagementUrl(string projectUrl)
        {
            return TryConvert(projectUrl, ProjectToTokenManagementUrlLookup);
        }

        public static (bool success, string projectUrl, int buildId) TryParseBuildUrl(string buildUrl)
        {
            var match = BuildUrlInfoRegex.Match(buildUrl);
            if (match.Success)
            {
                var projectUrl = match.Groups["projecturl"].Value;
                if (int.TryParse(match.Groups["buildid"].Value, out var buildId))
                {
                    return (true, projectUrl, buildId);
                }
            }

            return (false, "", -1);
        }
    }
}
