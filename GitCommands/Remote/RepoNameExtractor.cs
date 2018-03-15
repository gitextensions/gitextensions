using System;
using System.IO;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitCommands.Remote
{
    public interface IRepoNameExtractor
    {
        /// <summary>
        /// Get a "repo shortname" from the current repo URL
        /// There is no official Git repo shortname, this is one possible definition:
        ///  The filename without extension for the remote URL
        /// This function could have been included in GitModule
        /// </summary>
        void Get(out string repoProject, out string repoName);
    }

    public sealed class RepoNameExtractor : IRepoNameExtractor
    {
        private readonly Func<IGitModule> _getModule;

        public RepoNameExtractor(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <summary>
        /// Get a "repo shortname" from the current repo URL
        /// There is no official Git repo shortname, this is one possible definition:
        ///  The filename without extension for the remote URL
        /// This function could have been included in GitModule
        /// </summary>
        public void Get(out string repoProject, out string repoName)
        {
            var module = _getModule();

            // Extract "name of repo" from remote url
            string remoteName = module.GetCurrentRemote();
            if (remoteName.IsNullOrWhiteSpace())
            {
                // No remote for the branch, for instance a submodule. Use first remote.
                var remotes = module.GetRemotes();
                if (remotes.Length > 0)
                {
                    remoteName = remotes[0];
                }
            }

            var remoteUrl = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName));
            repoName = Path.GetFileNameWithoutExtension(remoteUrl);
            try
            {
                repoProject = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(remoteUrl));
            }
            catch
            {
                repoProject = "";
            }
        }
    }
}