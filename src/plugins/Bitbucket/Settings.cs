using System.Text.RegularExpressions;
using GitCommands.Config;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Plugins.Bitbucket
{
    public partial class Settings
    {
        [GeneratedRegex(@"https?:\/\/([\w\.\:]+\@)?(?<url>([a-zA-Z0-9\.\-\/]+?)):?(\d+)?\/scm\/(?<project>~?([^\/]+?))\/(?<repo>(.*?)).git", RegexOptions.ExplicitCapture)]
        private static partial Regex BitbucketHttpRegex();
        [GeneratedRegex(@"ssh:\/\/([\w\.]+\@)(?<url>([a-zA-Z0-9\.\-]+)):?(\d+)?\/(?<project>~?([^\/]+))\/(?<repo>(.*?)).git", RegexOptions.ExplicitCapture)]
        private static partial Regex BitbucketSshRegex();

        public static Settings? Parse(IGitModule module, SettingsSource settings, BitbucketPlugin plugin)
        {
            Settings result = new()
            {
                Username = plugin.BitbucketUsername.ValueOrDefault(settings),
                Password = plugin.BitbucketPassword.ValueOrDefault(settings),
                BitbucketUrl = plugin.BitbucketBaseUrl.ValueOrDefault(settings),
                DisableSSL = plugin.BitbucketDisableSsl.ValueOrDefault(settings)
            };

            string[] remotes = module.GetRemoteNames()
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .Select(r => module.GetSetting(string.Format(SettingKeyString.RemoteUrl, r)))
                .ToArray();

            foreach (string url in remotes)
            {
                Regex pattern = url.Contains("http") ? BitbucketHttpRegex() : BitbucketSshRegex();
                Match match = pattern.Match(url);
                if (match.Success && result.BitbucketUrl.Contains(match.Groups["url"].Value))
                {
                    result.ProjectKey = match.Groups["project"].Value;
                    result.RepoSlug = match.Groups["repo"].Value;
                    return result;
                }
            }

            return null;
        }

        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public bool DisableSSL { get; private set; }
        public string? ProjectKey { get; private set; }
        public string? RepoSlug { get; private set; }
        public string? BitbucketUrl { get; private set; }
    }
}
