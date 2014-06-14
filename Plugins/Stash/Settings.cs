﻿using GitCommands;
using GitCommands.Config;
using GitUIPluginInterfaces;
using System.Linq;
using System.Text.RegularExpressions;

namespace Stash
{
    class Settings
    {
        private const string StashHttpRegex = 
            @"https?:\/\/([\w\.\:]+\@)?(?<url>([a-zA-Z0-9\.\-]+)):?(\d+)?\/scm\/(?<project>~?\w+)\/(?<repo>\w+).git";
        private const string StashSshRegex =
            @"ssh:\/\/([\w\.]+\@)(?<url>([a-zA-Z0-9\.\-]+)):?(\d+)?\/(?<project>~?\w+)\/(?<repo>\w+).git";

        public static Settings Parse(IGitModule gitModule, ISettingsSource setting)
        {
            var result = new Settings
                             {
                                 Username = StashPlugin.StashUsername[setting],
                                 Password = StashPlugin.StashPassword[setting],
                                 StashUrl = StashPlugin.StashBaseURL[setting],
                                 DisableSSL = StashPlugin.StashDisableSSL[setting].Value
                             };

            var module = ((GitModule)gitModule);

            var remotes = module.GetRemotes()
                .Select(r => module.GetPathSetting(string.Format(SettingKeyString.RemoteUrl, r)))
                .ToArray();

            foreach (var url in remotes)
            {
                var pattern = url.Contains("http") ? StashHttpRegex : StashSshRegex;
                var match = Regex.Match(url, pattern);
                if (match.Success && result.StashUrl.Contains(match.Groups["url"].Value))
                {
                    result.ProjectKey = match.Groups["project"].Value;
                    result.RepoSlug = match.Groups["repo"].Value;
                    return result;
                }
            }

            return null;
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool DisableSSL { get; private set; }
        public string ProjectKey { get; private set; }
        public string RepoSlug { get; private set; }
        public string StashUrl { get; private set; }
    }
}
