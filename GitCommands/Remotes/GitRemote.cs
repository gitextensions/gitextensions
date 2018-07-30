using System.Collections.Generic;
using GitCommands.Config;

namespace GitCommands.Remotes
{
    public class GitRemote
    {
        /// <summary>
        /// Gets or sets value indicating whether the remote is enabled or not.
        /// If remote section is [remote branch] then it is considered enabled, if it is [-remote branch] then it is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the remote branch.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePush"/> key.
        /// </summary>
        public IReadOnlyList<string> Push { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePushUrl"/> key.
        /// </summary>
        public string PushUrl { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePuttySshKey"/> key.
        /// </summary>
        public string PuttySshKey { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemoteUrl"/> key.
        /// </summary>
        public string Url { get; set; }
    }
}