using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration.Settings
{
    /// <summary>
    /// Provides several operations that parse or convert urls in Azure DevOps (or TFS>=2015) projects
    /// </summary>
    public class ProjectUrlHelper
    {
        private static readonly Dictionary<Regex, Func<Match, string>> RemoteToProjectUrlLookup = new Dictionary<Regex, Func<Match, string>>()
        {
            { // VS Team Services via HTTPS
                new Regex(@"^(?<prot>(?:http|https))://(?<user>[^.@]+)(?:@[^.]*)?\.visualstudio\.com(?<port>:\d*)?(?:/DefaultCollection)?(?<project>(/[^/]+)?/[^/]+)/_(git|ssh)/(.+)$"),
                (match) => $"{match.Groups["prot"].Value}://{match.Groups["user"].Value}.visualstudio.com{match.Groups["port"].Value}{match.Groups["project"].Value}"
            },
            { // VS Team Services via SSH
                new Regex(@"^(?<user>[^.@]+)@vs-ssh\.visualstudio.com:v3(?:/[^/]*)?(?<project>/[^/]+)"),
                (match) => $"https://{match.Groups["user"].Value}.visualstudio.com{match.Groups["project"].Value}"
            },
            { // Azure DevOps via HTTPS
                new Regex(@"^(?<prot>(?:http|https))://(?:[^.@]+@)?dev\.azure\.com(?<port>:\d*)?(?<project>(?:/[^/]+)?/[^/]+)/_(?:git|ssh)/(?:.+)$"),
                (match) => $"{match.Groups["prot"].Value}://dev.azure.com{match.Groups["port"].Value}{match.Groups["project"].Value}"
            },
            { // Azure DevOps via SSH
                new Regex(@"^[^.@]+@ssh\.dev\.azure\.com:v3(?<project>(?:/[^/]+)?/[^/]+)"),
                (match) => $"https://dev.azure.com{match.Groups["project"].Value}"
            },
            { // Secondary Project-Repo in TFS on premise with DefaultCollection (need at least something to detect)
                new Regex(@"^(?<instanceurl>(?:http|https)://[^/]+(?::\d*)?(?:/[^/]+)+/DefaultCollection)(?<project>/[^/]+)/_(?:git|ssh)"),
                (match) => $"{match.Groups["instanceurl"].Value}{match.Groups["project"].Value}"
            },
            { // Main Project-Repo in TFS on premise with DefaultCollection (need at least something to detect)
                new Regex(@"^(?<instanceurl>(?:http|https)://[^/]+(?::\d*)?(?:/[^/]+)+/DefaultCollection)/_(?:git|ssh)(?<project>/[^/]+)"),
                (match) => $"{match.Groups["instanceurl"].Value}{match.Groups["project"].Value}"
            },
        };

        private static readonly Dictionary<Regex, Func<Match, string>> ProjectToTokenManagementUrlLookup = new Dictionary<Regex, Func<Match, string>>()
        {
            { // VS Team Services
                new Regex(@"^(?<instanceurl>(?:http|https)://[^.@]+(?:@[^.]*)?\.visualstudio\.com(?::\d*)?)"),
                (match) => $"{match.Groups["instanceurl"].Value}/_details/security/tokens"
            },
            { // Azure DevOps
                new Regex(@"^(?<instanceurl>(?:http|https)://dev\.azure\.com(?::\d*)?/[^/]+)"),
                (match) => $"{match.Groups["instanceurl"].Value}/_details/security/tokens"
            },
            { // Generic TFS on premise instance
                new Regex(@"^(?<instanceurl>(?:http|https)://[^/]+(?::\d*)?(?:/[^/]+)+)/[^/]+"),
                (match) => $"{match.Groups["instanceurl"].Value}/_details/security/tokens"
            },
        };

        private static readonly Regex BuildUrlInfoRegex = new Regex(@"^(?<projecturl>(?:http|https)://[^/]+(?::\d*)?(?:/[^/]+)+)/_build.*(?:&|\?)buildId=(?<buildid>\d+)");

        /// <summary>
        /// Tries to transform a supplied string into a different one using a number of regular expressions to check against.
        /// </summary>
        /// <param name="value">
        /// The string that should be transformed.
        /// </param>
        /// <param name="lookupDictionary">
        /// A dictionary of regular expressions the input string should be checked against and corresponding functions,
        /// that should be used to transform the found pattern into the desired result.
        /// </param>
        /// <returns>
        /// A tuple that contains whether a matching pattern could be found and the string that resulted from the transformation.
        /// </returns>
        private static (bool success, string result) TryTransformString(string value, Dictionary<Regex, Func<Match, string>> lookupDictionary)
        {
            if (value == null)
            {
                return (false, null);
            }

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

        /// <summary>
        /// Tries to detect the Azure DevOps / TFS project home page url from a repository url of the same project.
        /// </summary>
        /// <param name="remoteUrl">
        /// The url of the repository to find the Azure DevOps / TFS project url for.
        /// </param>
        /// <returns>
        /// A tuple that contains whether a Azure DevOps / TFS project could be recognized from the given url and the resulting home page url of the project.
        /// </returns>
        public static (bool success, string projectUrl) TryDetectProjectFromRemoteUrl(string remoteUrl)
        {
            if (remoteUrl == null && !BuildServerSettingsHelper.IsUrlValid(remoteUrl))
            {
                return (false, null);
            }

            return TryTransformString(remoteUrl, RemoteToProjectUrlLookup);
        }

        /// <summary>
        /// Tries to detect a Azure DevOps / TFS project home page url from a list of repository urls.
        /// </summary>
        /// <param name="remoteUrls">
        /// A list of repository urls to find a Azure DevOps / TFS project url for.
        /// </param>
        /// <returns>
        /// A tuple that contains whether a Azure DevOps / TFS project could be recognized from the given list and the resulting home page url of the project.
        /// </returns>
        public static (bool success, string projectUrl) TryDetectProjectFromRemoteUrls(IEnumerable<string> remoteUrls)
        {
            return remoteUrls.Select(TryDetectProjectFromRemoteUrl).FirstOrDefault(r => r.success);
        }

        /// <summary>
        /// Tries to get the token management url of the Azure DevOps / TFS instance for a given project url, without testing
        /// whether <paramref name="projectUrl"/> actually points to a Azure DevOps / TFS instance.
        /// </summary>
        /// <remarks>
        /// TryGetTokenManagementUrlFromProject will happlily convert anything that somewhat looks like a project url
        /// in favor of better availability for on premise installations of TFS
        /// </remarks>
        /// <param name="projectUrl">
        /// The url to the home page of a Azure DevOps / TFS project.
        /// </param>
        /// <returns>
        /// A tuple that contains whether the token management url could be recognized from the given project url and the resulting url.
        /// </returns>
        public static (bool success, string tokenManagementUrl) TryGetTokenManagementUrlFromProject(string projectUrl)
        {
            if (projectUrl == null && !BuildServerSettingsHelper.IsUrlValid(projectUrl))
            {
                return (false, null);
            }

            return TryTransformString(projectUrl, ProjectToTokenManagementUrlLookup);
        }

        /// <summary>
        /// Tries to parse a url to a Azure DevOps / TFS build result and get the corresponding project and build id from.
        /// </summary>
        /// <param name="buildUrl">
        /// A url that points to the build status / build results page of a build in a Azure DevOps / TFS project
        /// </param>
        /// <returns>
        /// A tuple that contains whether the project and build id could be detected from the given url, as well as both informations.
        /// </returns>
        public static (bool success, string projectUrl, int buildId) TryParseBuildUrl(string buildUrl)
        {
            if (buildUrl == null && !BuildServerSettingsHelper.IsUrlValid(buildUrl))
            {
                return (false, null, -1);
            }

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
