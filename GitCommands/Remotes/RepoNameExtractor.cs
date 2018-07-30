using System;
using System.IO;
using GitCommands.Config;
using GitUIPluginInterfaces;

namespace GitCommands.Remotes
{
    public interface IRepoNameExtractor
    {
        /// <summary>
        /// Get a "repo shortname" from the current repo URL
        /// There is no official Git repo shortname, this is one possible definition:
        ///  The filename without extension for the remote URL
        /// This function could have been included in GitModule
        /// </summary>
        (string repoProject, string repoName) Get();
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
        public (string repoProject, string repoName) Get()
        {
            var module = _getModule();

            // Extract "name of repo" from remote url
            var remoteName = module.GetCurrentRemote();

            if (remoteName.IsNullOrWhiteSpace())
            {
                // No remote for the branch, for instance a submodule. Use first remote.
                var remotes = module.GetRemoteNames();
                if (remotes.Count > 0)
                {
                    remoteName = remotes[0];
                }
            }

            var remoteUrl = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName));
            var repoName = Path.GetFileNameWithoutExtension(remoteUrl);

            return (GetRepoProject(), repoName);

            string GetRepoProject()
            {
                try
                {
                    return Path.GetFileNameWithoutExtension(Path.GetDirectoryName(remoteUrl));
                }
                catch
                {
                    return "";
                }
            }
        }
    }
}