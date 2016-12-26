using System.Collections.Generic;
using GitCommands.Config;

namespace GitUI.Objects
{
    public class GitRemote
    {
        /// <summary>
        /// Gets or sets the name of the remote branch.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemoteUrl"/> key.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePush"/> key.
        /// </summary>
        public IList<string> Push { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePushUrl"/> key.
        /// </summary>
        public string PushUrl { get; set; }

        /// <summary>
        /// Gets or sets value stored in .git/config via <see cref="SettingKeyString.RemotePuttySshKey"/> key.
        /// </summary>
        public string PuttySshKey { get; set; }
    }
}