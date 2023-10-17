﻿using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitExtensions.Plugins.Bitbucket
{
    public class Settings
    {
        private const string BitbucketHttpRegex =
            @"https?:\/\/([\w\.\:]+\@)?(?<url>([a-zA-Z0-9\.\-\/]+?)):?(\d+)?\/scm\/(?<project>~?([^\/]+?))\/(?<repo>(.*?)).git";
        private const string BitbucketSshRegex =
            @"ssh:\/\/([\w\.]+\@)(?<url>([a-zA-Z0-9\.\-]+)):?(\d+)?\/(?<project>~?([^\/]+))\/(?<repo>(.*?)).git";

        public static Settings? Parse(IGitModule gitModule, ISettingsSource settings, BitbucketPlugin plugin)
        {
            Settings result = new()
            {
                Username = plugin.BitbucketUsername.ValueOrDefault(settings),
                Password = plugin.BitbucketPassword.ValueOrDefault(settings),
                BitbucketUrl = plugin.BitbucketBaseUrl.ValueOrDefault(settings),
                DisableSSL = plugin.BitbucketDisableSsl.ValueOrDefault(settings)
            };

            GitModule module = (GitModule)gitModule;

            string[] remotes = module.GetRemoteNames()
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .Select(r => module.GetSetting(string.Format(SettingKeyString.RemoteUrl, r)))
                .ToArray();

            foreach (string url in remotes)
            {
                string pattern = url.Contains("http") ? BitbucketHttpRegex : BitbucketSshRegex;
                Match match = Regex.Match(url, pattern);
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
